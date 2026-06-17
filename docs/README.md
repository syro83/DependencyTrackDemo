# The Open-source software (OSS) Registry with Dependency-Track Tutorial

Guides and walkthroughs for integrating Dependency-Track into a CI/CD pipeline, including a demo application, Azure infrastructure provisioning, and SBOM generation and upload steps. Guides are organized into four main sections:

- [10-project](./10-project/README.md) - A dummy project plan that outlines the goals, scope, and implementation steps for the tutorial.
- [20-demo-application](./20-demo-application/README.md) - A small full-stack weather application that serves as the starting point for the Dependency-Track tutorial. It includes a React frontend, a .NET backend, and an Azure DevOps pipeline with dummy deploy stages.
- [30-dependency-track](./30-dependency-track/README.md) - Guides for deploying, configuring, and implementing Dependency-Track on Azure using Container Apps and PostgreSQL. It includes Azure infrastructure provisioning with Bicep, Azure DevOps pipelines, and initial platform setup.
- [40-dependency-track-helper](./40-dependency-track-helper/README.md) - An opinionated helper API that addresses Dependency-Track project lifecycle gaps.

![Tutorial from code to confidence](./10-project/assets/image-2.png)

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
