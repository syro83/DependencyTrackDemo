# Dependency-Track Deployment, Configuration, and Implementation Guide

After you complete the demo application tutorial in [demo-application](../20-demo-application/README.md), continue with the Dependency-Track deployment, configuration, and implementation guides in this folder.

The `dt` folder contains the Azure infrastructure and Azure DevOps pipeline configuration for hosting [OWASP Dependency-Track](https://dependencytrack.org/) on Azure using Container Apps and PostgreSQL.

---

## Steps

This guide is organized into three steps, which are described in separate files.

Start with [README-Azure-deployment.md](./README-Azure-deployment.md) for the Azure subscription and Azure DevOps setup steps.

After deployment, use [README-configuration.md](./README-configuration.md) to configure Dependency-Track.

Then continue with [README-implementation.md](./README-implementation.md) for the walkthrough on integrating Dependency-Track into the demo application CI/CD pipelines.

![Takeaways](assets/image-27.png)

---

## Final guide

Finally, check [../40-dependency-track-helper/README.md](../40-dependency-track-helper/README.md) for an opinionated API that addresses some of Dependency-Track's shortcomings and provides a more streamlined interface for managing projects, components, and vulnerabilities.
