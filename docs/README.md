# Open-Source Software (OSS) Register (Dependency-Track Demo) Tutorial

Start with the project plan in [/10-project/README.md](./10-project/README.md). If you prefer a hands-on flow, you can skip ahead and start with the demo application.

Next, review the demo application in [/20-demo-application/README.md](./20-demo-application/README.md). This is the application you integrate with Dependency-Track. It contains a simple React frontend and .NET backend, plus a pipeline that builds both and includes dummy deploy stages for `dev`, `test`, `acc`, and `prd`. The app is intentionally simple so the tutorial can focus on Dependency-Track integration.

Then continue with the Dependency-Track deployment and configuration guides in [/30-dependency-track/README.md](./30-dependency-track/README.md). These guides cover Azure infrastructure provisioning with Bicep, Azure DevOps pipelines, and initial platform setup. After Dependency-Track is running, continue with the implementation guide to add SBOM generation and upload steps to the CI/CD pipelines.

Finally, check [/40-dependency-track-helper/README.md](./40-dependency-track-helper/README.md) for an opinionated helper API that addresses common Dependency-Track lifecycle gaps and provides a streamlined interface for managing projects, components, and vulnerabilities.

---

## Prerequisites

- An Azure subscription with permission to create resources and RBAC assignments
- An Azure DevOps organization and project
- Permission to create pipelines and service connections in Azure DevOps

---

## Repository Structure

The repository is organized into four main folders:

| Folder | Purpose |
| --- | --- |
| demo | A small full-stack weather application with a simplified staged pipeline and dummy deploy stages |
| docs | Documentation for the project, including tutorials and implementation details |
| dt | Azure infrastructure and Azure DevOps pipelines to deploy and configure Dependency-Track |
| dth | An API with opinionated logic to manage Dependency-Track |

---
