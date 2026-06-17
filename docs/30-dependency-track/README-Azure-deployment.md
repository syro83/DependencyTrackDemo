# Dependency-Track Azure Deployment Guide

See [README.md](./README.md) for the main overview. The `dt/` folder contains all infrastructure as code and pipeline definitions for the Azure deployment. It is organized into two main subfolders: `infra/` for Bicep files and `pipeline/` for Azure DevOps pipeline YAML files.

- [Dependency-Track Azure Deployment Guide](#dependency-track-azure-deployment-guide)
  - [Azure Subscription](#azure-subscription)
    - [What gets created On Azure](#what-gets-created-on-azure)
      - [Foundation layer](#foundation-layer)
      - [Data layer](#data-layer)
      - [App layer](#app-layer)
    - [Additional notes](#additional-notes)
  - [Steps](#steps)
    - [Step 1. Create the Azure service principal and service connection](#step-1-create-the-azure-service-principal-and-service-connection)
    - [Step 2. Store secrets as pipeline variables](#step-2-store-secrets-as-pipeline-variables)
    - [Step 3. Create the Azure DevOps pipelines](#step-3-create-the-azure-devops-pipelines)
    - [Step 4. Run the Pipelines](#step-4-run-the-pipelines)
    - [Step 5. Continue with configuration](#step-5-continue-with-configuration)
  - [Tear down when done](#tear-down-when-done)
    - [Production Considerations](#production-considerations)

---

## Azure Subscription

If you are using a new Azure subscription for this setup, prepare it before creating the service connection and running the pipelines.

1. Sign in to <https://portal.azure.com/> and switch to the correct Microsoft Entra tenant.
2. Navigate to `Subscriptions` and select the correct one.
3. Register the required resource providers under `Subscriptions` > `Resource providers`:
   - `Microsoft.App`
   - `Microsoft.DBforPostgreSQL`
   - `Microsoft.Network`
   - `Microsoft.Storage`
4. Provider registration can take a few minutes. If a pipeline fails with a provider or resource type error immediately after starting, check the registration status in the portal.
5. Navigate to `Resource groups`, create a resource group named `rg-dependecytrack`, and select the subscription for this demo.

### What gets created On Azure

All resources land in a single resource group; they are created by three pipelines with three related Bicep files.

![Azure resources](assets/image-29.png)

#### Foundation layer

- Storage account (`Standard_LRS`, TLS 1.2 only, no public blob access)
  - Blob container `config` for pipeline parameter handoff between stages
- Azure Container Registry (`Basic` SKU, admin user enabled) — used for DTH API image builds and pulls
- Virtual network (`10.0.0.0/22`) with two delegated subnets
  - `snet-containers` (`10.0.0.0/23`) — delegated to `Microsoft.App/environments`
  - `snet-db` (`10.0.2.0/24`) — delegated to `Microsoft.DBforPostgreSQL/flexibleServers`

#### Data layer

- Azure Files share `vulnerability-data` (50 GB) on the foundation storage account — mounted at `/data` inside the DT API server container so vulnerability mirrors survive container restarts
- Private DNS zone `privatelink.postgres.database.azure.com` with a VNet link
- PostgreSQL 16 Flexible Server (`Standard_B1ms`, burstable) — VNet-injected into `snet-db`, no public internet access
- PostgreSQL database `dependencytrack`

#### App layer

- Container Apps managed environment — VNet-integrated into `snet-containers`
- `dependencytrack/apiserver` Container App (1 CPU, 2 Gi) with the Azure Files volume mounted at `/data`
- `dependencytrack/frontend` Container App (0.25 CPU, 0.5 Gi) with the API URL injected at runtime

### Additional notes

- All shared configuration (service connection, resource group name, region, image names) is defined once in `dt/pipeline/variables/common.yml`
- The `dbAdminPassword` is read automatically from the `DependecyTrackSecrets` variable group — no manual parameter passing is needed.
  - **Secret handling:** `dbAdminPassword` is mapped to the `DB_ADMIN_PASSWORD` environment variable inside the `AzureCLI@2` task. It is never expanded inline in the script.
- For network security a VNet is created in the foundation layer and used by both the data and app layers.

---

## Steps

### Step 1. Create the Azure service principal and service connection

Create an `Azure Resource Manager` service connection named `sc-dependecytrack` for the resource group `rg-dependecytrack`. This name must match the value in `dt/pipeline/variables/common.yml`.

- In DevOps, go to `Project Settings`, followed by `Service Connections`. Click on `Create service connection`, select `App registration`, `Workload Identity federation`, `Subscription` and `rg-dependecytrack`, name it `sc-dependecytrack`.

For this demo, `Contributor` on the resource group is sufficient. If you scope permissions more narrowly, make sure the service principal can deploy networking, Container Apps, PostgreSQL, and read storage account keys.

![Project service connection](assets/image.png)

### Step 2. Store secrets as pipeline variables

The `dbAdminPassword` is required by both the data pipeline and the app pipeline. Store it as a secret pipeline variable so it is masked in logs and never committed to source control.

In Azure DevOps we do it via a Variable group (shared across pipelines):

1. Go to `Pipelines` > `Library`.
2. Create a variable group named `DependecyTrackSecrets`.
3. Add a variable named `dbAdminPassword`, set a strong value, and mark it as secret.
4. Authorize the variable group for the pipelines when Azure DevOps prompts on first use.
   - Password requirements: Be at least 8 characters; contain characters from three of the following: uppercase, lowercase, digits, special characters; not contain the username

![DevOps pipeline variable group](assets/image-1.png)

### Step 3. Create the Azure DevOps pipelines

Create three pipelines in Azure DevOps:

| Pipeline name | YAML file |
| --- | --- |
| `DependencyTrack - Foundation` | `dt/pipeline/dependecytrack-foundation-pipeline.yml` |
| `DependencyTrack - Data` | `dt/pipeline/dependecytrack-data-pipeline.yml` |
| `DependencyTrack - App` | `dt/pipeline/dependecytrack-app-pipeline.yml` |

For each pipeline:

1. Go to `Pipelines` > `New pipeline`.
2. Connect to your repository.
3. Select `Existing Azure Pipelines YAML file`.
4. Pick the YAML file path from the table above.
5. Save (do not run yet).

### Step 4. Run the Pipelines

1. Run the `DependencyTrack - Foundation` pipeline.
   - Foundation outputs written to blob: config/foundation-outputs.json
2. Run the `DependencyTrack - Data` pipeline.
   - Data outputs written to blob: config/data-outputs.json
3. Run the `DependencyTrack - App` pipeline.

- When complete, the job log shows the backend URL and the frontend URL. Save these URLs for the next guide.

![DevOps pipeline runs](assets/image-2.png)

Open the `Frontend URL` in a browser. The Dependency-Track login page should appear within a few seconds. The API server can take 1–2 minutes to fully initialize on first start while it populates the vulnerability database from its mirrors.

![DevOps pipeline urls](assets/image-3.png)

### Step 5. Continue with configuration

Continue with [README-configuration.md](./README-configuration.md) for the first-time configuration steps.

---

## Tear down when done

When you are finished with the demo, remove everything including the Storage Account and the resource group itself:

```bash
az group delete --name rg-dependecytrack --yes
```

---

### Production Considerations

For production use, apply standard security and reliability hardening, for example:

- Store secrets in Azure Key Vault.
- Improve pipeline secret handoff and access controls.
- Harden PostgreSQL configuration and choose an appropriate production SKU.
- Place an API gateway and WAF in front of Container Apps.
- Use private endpoints where possible, and restrict access to private networks.
- Scale Container Apps and database resources based on expected load.

I could go on but you get the point: do not use this setup as-is in production. It is intentionally optimized for demo and evaluation purposes.
