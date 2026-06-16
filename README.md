# Open-source software (OSS) Register (Dependency-Track Demo) Tutorial

This repository contains a tutorial on integrating [OWASP Dependency-Track](https://dependencytrack.org/) with a small full-stack demo application managed through CI/CD. It walks through using the demo application as a baseline, deploying Dependency-Track on Azure, and then wiring Dependency-Track into the demo application's build pipeline. The goal is to demonstrate how to implement an open-source software (OSS) register with SBOM monitoring and supply-chain security, providing continuous, structured visibility into all software components in use.

---

## Tutorial: from code to confidence

The `docs/` folder contains the main [README.md](docs/README.md) entry point and guides for each aspect of the project. It provides an overview of the project, its structure, and a summary of the goals and rationale. The other README files in the subfolders provide detailed guides and walkthroughs for each aspect of the project, such as setting up the demo application, deploying Dependency-Track on Azure, and integrating it into CI/CD pipelines.

![From code to confidence](docs/10-project/assets/image-2.png)

So, for the tutorial _goto_ the [`docs/`](docs/README.md) and follow the links in the documentation section to get started with the demo application, Dependency-Track deployment, and CI/CD integration. Beside this tutorial, read the original documentation of Dependency-Track and more best practices for your own implementation. This tutorial is a starting point and good base, but you should adapt the implementation to your specific context and requirements.

---

## Context

Just imagine you update all your packages to the latest versions, and you ship the update to production.

![Ships it](docs/10-project/assets/image.png)

### Original case

This tutorial is based on a specific use case for a customer, and the implementation choices may reflect that context.

The goal is to provide a practical example of how to implement an OSS register with SBOM monitoring, but the specific architecture and design decisions may not be universally applicable. The tutorial focuses on demonstrating the core concepts and integration patterns, rather than prescribing a one-size-fits-all solution. Depending on your organization's specific requirements, constraints, and existing tooling, you may need to adapt the implementation accordingly. The key is to understand the underlying principles and how to leverage Dependency-Track effectively within your own context.

> The code in this repository was 'recreated' based on the ideas and implementation for that customer. The the code is _simplified_ and adapted for demo purposes.

### AI

I used AI for faster prototyping, coding and documentation, I guided the AI to steer it towards the desired outcomes, tried to review everything and to fix mistakes or unclear parts, but some issues may remain. Always review code and documentation found on the internet, including this repository.

### Credits

Because this was a recreation I have NOT captured all resources that inspired the implementation. Thanks to everyone who contributed to the open-source projects, and to the people who shared their knowledge and experience in the field of software supply-chain security

---

## Project summary

See [readme project plan](docs/10-project/README.md) for the project details, including the project goal, acceptance criteria, tool comparison, and rationale for choosing Dependency-Track.

This sections will describe those in high level, but check the docs for the more details.

### Project Goal

_Implementing an Open-source software (OSS) and dependency Register with SBOM monitoring and supply-chain security is essential to gain continuous, structured visibility into all software components in use._

See a couple articles from the last few months on Tweakers.net [1](https://tweakers.net/nieuws/241384/owasp-foundation-zet-gaten-in-softwaresupplychains-voor-het-eerst-in-top-drie.html) [2](https://tweakers.net/nieuws/247146/bitwarden-lost-kwetsbaarheid-in-cli-tool-op-na-geinfecteerd-npm-package.html) [3](https://tweakers.net/reviews/13812/wat-zijn-npm-packages-en-waarom-richten-hackers-zich-erop.html) [4](https://tweakers.net/nieuws/239220/hackers-infecteren-187-npm-packages-met-malware-die-credentials-steelt.html) [5](https://tweakers.net/nieuws/241914/onderzoekers-ontdekken-tweede-golf-van-malware-in-npm-packages.html) (Dutch) for context.

![Software problems](docs/10-project/assets/image-3.png)

Without this visibility:

- **License risk goes undetected.** Developers may introduce packages with commercially undesirable or prohibited licenses. Open-source projects can change their license at any time, as recently happened with Fluent Assertions, AutoMapper, and MediatR, causing updates to introduce immediate legal risk.
- **Vulnerability impact is hard to assess.** When a vulnerability is disclosed, it becomes difficult to determine quickly where affected packages are used across a software portfolio. Incidents like Log4Shell and the Axios supply-chain attack illustrate the severity of this risk.

With SBOM monitoring, teams gain continuous insight into component usage, licenses, and vulnerabilities, enabling faster response, better compliance, and a more secure software supply chain.

#### What is an SBOM?

A **Software Bill of Materials (SBOM)** is a formal, machine-readable inventory of all components, libraries, and modules that make up a software product. Think of it as an ingredient list for software: it details what is included, along with the relationships, versions, suppliers, and licensing information for each component.

> See also: [SBOM explained (YouTube)](https://www.youtube.com/watch?v=QV2JcwHpjeQ)

SBOMs are a cornerstone of modern software supply-chain security and compliance. High-profile incidents such as the SolarWinds breach and the Log4j vulnerability have underscored the need for **transparency and traceability** in software composition. Without an SBOM, organizations lack visibility into their dependencies, making it difficult to assess exposure to vulnerabilities or comply with regulatory mandates.

### Project Main Acceptance Criteria

- Automatically import SBOMs from CI/CD pipelines for all relevant applications and services.
- Provide a central view of all software components in use, including version, license type, and origin.
- Detect license changes in OSS packages and alert when a component adopts a more restrictive license model.
- Automatically detect vulnerabilities via linked sources (NVD, OSS Index, etc.) and display them per project.
- Show exactly where vulnerable components are used, enabling impact analysis within minutes.
- Support policy rules for license compliance, including flagging prohibited or undesirable license types.
- Provide a notification mechanism (email, webhook, or ticketing integration) for new vulnerabilities or license risks.
- Offer a dashboard with real-time insight into risks, vulnerabilities, licenses, and component trends.
- Support future SBOM validation gates in build and release pipelines.

### Why Dependency-Track?

[Dependency-Track](https://dependencytrack.org/) is an **open-source platform for SBOM monitoring and supply-chain security**. It helps organizations maintain continuous visibility into vulnerabilities across the software components they use — critical in a world of ever-growing open-source dependencies.

#### Key Features

- **SBOM analysis** — Ingests SBOMs to identify exactly which components are in your applications.
- **Vulnerability detection** — Correlates components with known vulnerabilities via NVD, OSS Index, VulnDB, and other sources.
- **License monitoring** — Tracks license types and flags compliance risks.
- **Risk scoring** — Provides risk scores at both the project and component level.
- **Dashboards & reporting** — Offers visual dashboards and reports for security and compliance teams.
- **CI/CD integration** — Surfaces vulnerabilities early in the development pipeline via its REST API.

![What is Dependency-Track](docs/10-project/assets/image-4.png)

#### Decision Rationale

Dependency-Track was selected because it:

- Is **open-source** with no license costs for evaluation.
- Good at **governance, license monitoring, and lifecycle analysis**.
- Has an **API-first design**, ideal for integration with Azure DevOps.
- Is **lightweight** and straightforward to deploy on Azure or AWS.

---

Thanks and good luck with your own implementation!
