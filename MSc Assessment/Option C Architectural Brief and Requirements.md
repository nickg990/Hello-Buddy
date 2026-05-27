# COM712 Assessment 3 Option C

## Architectural Brief and Requirements

## Purpose

This document defines the recommended architecture, delivery scope, technical requirements, and evidence strategy for a distinction-level submission for COM712 Assessment 3 Option C using the Hello Buddy canine physiotherapy administration domain.

It is written as a development reference for implementation and deployment work to be carried out with Opus 4.7 and Sonnet. It is intentionally assessment-led rather than product-led: the objective is not to build the whole business system, but to deliver a credible, containerised, cloud-deployed vertical slice that scores strongly against the published rubric.

## Assessment-Driven Positioning

### Recommended interpretation of Option C

Use the canine physiotherapy administration system as the real-world problem domain, with the assessed workload implemented only as a containerised admin web solution that supports a practitioner workflow and generates owner-facing rehabilitation programme PDFs from structured data.

### Recommended assessed solution name

Hello Buddy Cloud Admin

### Problem statement

Small canine physiotherapy practices often create rehabilitation programmes manually and distribute them as ad hoc files, creating duplicated effort, poor version control, limited auditability, and inconsistent owner instructions.

### Proposed solution

Build and deploy a containerised practitioner-facing cloud application that:

- stores synthetic owner, pet, treatment case, and programme data;
- supports draft programme creation;
- generates a branded PDF exercise programme through a dedicated PDF service;
- stores published outputs in cloud object storage;
- exposes monitoring, logs, scaling behaviour, and cost evidence suitable for the assessment report.

## Why This Option Can Score at Distinction Level

The rubric rewards more than simply running containers. A strong distinction-level submission must show depth, justified trade-offs, troubleshooting discipline, reproducibility, metrics, and critical awareness of industry trends.

This option is strong because it naturally supports:

- a realistic multi-component architecture rather than a toy example;
- at least two containers with clear responsibility boundaries;
- meaningful networking and security design;
- measurable performance behaviour under load;
- storage design decisions beyond a single local volume;
- critical discussion of managed services versus containerised dependencies;
- sustainability and cost comparison against a VM baseline;
- future-facing discussion of GitOps, serverless containers, and cloud portability.

## Rubric Strategy

### Knowledge and understanding of cloud, container, and industry concepts

To reach distinction standard, the report and implementation must show:

- accurate comparison of VM, ACI, and Fargate approaches;
- understanding of image-based deployment, immutable infrastructure, service discovery, ingress, autoscaling, observability, and storage separation;
- explicit discussion of current industry directions such as serverless containers, GitOps, green IT, and managed platform trade-offs;
- clear links between theory and technical decisions rather than feature listing.

### Critical thinking and problem-solving

To reach distinction standard, the work must include:

- a justified platform choice with counter-arguments addressed;
- evidence-backed trade-offs around cost, scalability, operational complexity, latency, and security;
- at least one genuine fault, failure, or bottleneck investigated and fixed;
- redesign decisions defended with measurements rather than opinion.

### Application of evidence

To reach distinction standard, the work must include:

- successful local container build and cloud deployment evidence;
- screenshots or exported metrics from logs, monitoring, and scaling events;
- seven-day cost estimate and carbon estimate using named tools;
- reproducible artefacts such as Dockerfiles, Compose, IaC, and README;
- basic SLA or availability discussion rather than only screenshots.

### Structure and communication

To reach distinction standard, the written output must:

- follow the exact report structure from the assessment brief;
- use one clear architecture diagram and one deployment diagram;
- include plain-English explanations for non-specialist readers;
- use concise tables for trade-offs, costs, tests, and incidents.

### Referencing and academic conventions

To reach distinction standard, the work must:

- use accurate Harvard referencing throughout;
- include a mix of academic, vendor, and professional sources;
- explicitly reference ethical handling of synthetic data and health-adjacent information;
- avoid unsupported claims about cost, sustainability, or security.

## Recommended Technical Direction

### Recommendation

Use Azure as the assessed cloud and build a server-rendered practitioner web application plus dedicated PDF service as the first deployment slice.

### Recommended Azure runtime choice

Preferred choice: Azure Container Apps.

Why this is the cleanest delivery route:

- It supports two containerised application services cleanly with HTTP ingress, revisions, scaling rules, and managed observability.
- It is easier to evidence autoscaling, logs, and networking than plain Azure Container Instances alone.
- It fits the assessment's containerisation intent while avoiding unnecessary AKS complexity for a first increment.
- It gives a stronger distinction-level story around operations, resilience, and managed cloud trade-offs.

### Important assessment note

The published brief names Azure Container Instances explicitly. If your tutor or module guidance requires strict use of ACI rather than a broader Azure container platform, keep the same application design but deploy the containers to ACI and document the operational limitations. If there is flexibility, Azure Container Apps is the stronger choice for marks, evidence quality, and implementation cleanliness.

### Why the assessed solution should be admin web only

- Release 1 is defined as an administration system plus PDF generation.
- The assessment is focused on cloud infrastructure, networking, containerisation, and operational evidence.
- A small admin web application plus PDF service slice is much easier to justify, monitor, scale, and evaluate than introducing any future mobile scope.

## Target Architecture

### Core runtime components

The assessed system should contain at minimum these two application containers:

1. `hello-buddy-admin-web`
   A practitioner-facing ASP.NET Core web application that renders the wireframe-led admin workflow and handles the core owner, pet, case, and programme flow.

2. `hello-buddy-pdf-service`
   A dedicated PDF generation container that renders branded programme PDFs from structured payloads.

### Supporting cloud services

- Azure Container Registry for container image storage
- Azure Container Apps Environment for running the admin web service and PDF service
- Azure Front Door or Container Apps ingress with custom domain for public HTTPS access
- Azure DNS for DNS management if needed
- Azure Key Vault for secrets and certificates
- Azure Database for MySQL Flexible Server for durable relational data
- Azure Blob Storage for published PDF storage and optional evidence artefacts
- Azure Monitor, Log Analytics, and Application Insights for logs, metrics, tracing, and dashboards

### Local development topology

Use Docker Compose locally with three containers:

- admin web app
- PDF service
- MySQL

This supports local integration testing and provides clear evidence of containerisation before cloud deployment.

### Cloud deployment topology

- Internet user reaches Azure DNS name or custom domain
- Azure Front Door or Container Apps ingress routes HTTPS traffic to the admin web container
- Admin web container calls PDF service over private service-to-service networking inside the Container Apps environment
- Admin web persists structured data to Azure Database for MySQL Flexible Server
- Admin web and PDF service store generated PDF outputs in Azure Blob Storage
- Logs and metrics are emitted to Azure Monitor and Log Analytics

### Network design

Use a VNet with:

- dedicated ingress path for the public endpoint;
- delegated subnets for the Container Apps environment where required;
- separate private database connectivity;
- NSG rules restricting traffic by tier;
- no direct public access to the database.

### Security design

- TLS termination at Front Door or Container Apps ingress using managed certificates where possible
- managed identities for container access to Blob Storage and Key Vault
- secrets stored outside images
- database credentials injected through Key Vault references or Container Apps secrets
- Blob container private by default
- log retention configured in Log Analytics
- synthetic data only

### Scaling design

The admin web service must scale horizontally based on at least two conditions:

- average CPU utilisation
- HTTP concurrency or average memory utilisation

The PDF service may remain fixed-size for the MVP, but the report should justify this decision and explain when queue-based scaling would be preferable.

## Frontend Strategy For The First Increment

### Clean implementation choice

The cleanest first increment is not a separate SPA plus separate API. It is a server-rendered ASP.NET Core admin web app that contains the initial practitioner UI, calls the database through an application layer, and delegates PDF rendering to a separate PDF service.

This keeps the architecture simple enough for the assessment while still meeting the two-container requirement.

### Why this aligns well with the wireframes

The wireframes show a structured, workflow-led admin experience rather than a highly interactive consumer app. That means the first increment can be implemented cleanly as:

- a shared admin shell;
- tabbed case detail pages;
- form-heavy programme builder pages;
- an HTML-based PDF preview page;
- a publish action that creates a final PDF through the PDF service.

This is a better fit for server-side rendering than a client-heavy SPA in the first iteration.

### Alignment with the existing canine physio app

To stay visually aligned with the established Hello Buddy product design language:

- port the existing design tokens into CSS custom properties;
- reuse the Hello Buddy palette, spacing, rounded cards, and typography hierarchy;
- use the same logo treatment and calm clinical tone;
- keep the admin shell and page components visually consistent with the existing app.

### Shared template strategy for PDF

The cleanest PDF approach is to use one shared HTML template for both on-screen preview and final PDF generation.

Flow:

1. Admin web builds a structured programme view model.
2. Admin web renders the owner-facing HTML preview.
3. Admin web sends the same rendered payload or canonical HTML to the PDF service.
4. PDF service converts the HTML to PDF and stores the file in Blob Storage.
5. Admin web records the published version metadata in MySQL.

This matches the PDF preview wireframe cleanly and avoids divergence between preview and final output.

### Recommended PDF technology

Use Playwright or PuppeteerSharp inside the PDF container to render HTML to PDF. This is cleaner for brand-consistent output than constructing PDFs procedurally for the first increment.

## Wireframe-Aligned Increment Definition

### Recommended first increment

The first increment should focus on the workflow visible across these wireframes:

- [c:/Projects/Hello-Buddy/Designs/wireframes/04_case_detail.svg](c:/Projects/Hello-Buddy/Designs/wireframes/04_case_detail.svg)
- [c:/Projects/Hello-Buddy/Designs/wireframes/05_programme_builder.svg](c:/Projects/Hello-Buddy/Designs/wireframes/05_programme_builder.svg)
- [c:/Projects/Hello-Buddy/Designs/wireframes/06_pdf_preview.svg](c:/Projects/Hello-Buddy/Designs/wireframes/06_pdf_preview.svg)

This creates a clean vertical slice:

- open treatment case;
- review case summary and current programme status;
- create or resume draft programme;
- add or edit session exercises;
- preview the owner-facing PDF;
- validate blockers and warnings;
- publish the PDF and store the output.

### What to defer from the wireframes

To keep the first increment clean, defer or reduce:

- full owner CRUD;
- broad dashboard analytics;
- complete exercise-library authoring flows;
- advanced drag-and-drop reordering;
- full mobile navigation support beyond responsive layout.

### Minimum frontend pages for the increment

1. Case detail page with Summary and Programme tabs.
2. Programme builder page with editable sessions and exercise cards.
3. PDF preview page with validation panel and publish action.

These pages are sufficient to demonstrate business value, container interactions, storage, security, and monitoring.

## Architecture Principles

- Build the smallest clinically credible vertical slice.
- Keep containers single-purpose.
- Prefer managed stateful services in the cloud over self-hosted database containers.
- Separate application compute from durable storage.
- Design for reproducibility first, then polish.
- Measure everything needed for the report while implementing.

## Assessed MVP Scope

### In scope

- practitioner login placeholder or simple seeded authentication
- a lightweight dashboard or landing page if it materially helps navigation, otherwise direct case-entry flow
- treatment case browse and detail flow using synthetic data
- create or edit a draft programme for a treatment case
- preview and publish a programme
- PDF generation through the separate PDF container
- file upload or persistence of generated PDF to Azure Blob Storage
- application logging and monitoring
- local Docker Compose deployment
- cloud deployment to Azure
- scripted load test and failure testing

### Explicitly out of scope

- real client mobile deployment
- owner portal
- offline sync
- multi-tenant administration
- advanced role-based access control
- automated WhatsApp or email distribution
- full production hardening beyond what is needed for the assessment

## Functional Requirements

### FR1 Practitioner access

The system shall provide a basic practitioner-facing entry point suitable for demonstration, using seeded credentials or a simple local authentication mechanism.

### FR2 Case browsing

The system shall allow a practitioner to browse synthetic owner, pet, and treatment case data.

### FR3 Programme draft management

The system shall allow a practitioner to create and edit a draft rehabilitation programme linked to a treatment case.

### FR4 Session structure

The system shall support either a single daily session or AM/PM session structure.

### FR5 Exercise prescription

The system shall store structured exercise prescription fields including title, repetitions, sets, hold seconds, frequency, and notes.

### FR6 PDF preview

The system shall provide a preview pathway before publish so missing fields can be identified.

### FR7 PDF publish

The system shall publish a final branded PDF through the dedicated PDF service rather than inline page printing.

### FR8 Object storage

The system shall store published PDF files in cloud object storage and persist metadata in the relational database.

### FR9 Version integrity

The system shall avoid overwriting already published outputs and shall preserve published programme metadata.

### FR10 Auditability

The system shall log operational events such as programme created, publish requested, PDF generated, PDF generation failed, and file persisted.

## Non-Functional Requirements

### NFR1 Containerisation

The application shall be packaged into at least two Docker images with separate Dockerfiles.

### NFR2 Portability

The solution shall run locally via Docker Compose and in the cloud via the selected runtime with environment-specific configuration.

### NFR3 Availability

The public endpoint shall remain reachable through a load-balanced entry point and support restart or rescheduling of failed tasks.

### NFR4 Security

Secrets shall not be hard-coded into source control, Dockerfiles, or image layers.

### NFR5 Data safety

Only synthetic data shall be used for development, testing, deployment, screenshots, and the final report.

### NFR6 Observability

The system shall expose logs, infrastructure metrics, container metrics, and alarm conditions sufficient to demonstrate health and fault diagnosis.

### NFR7 Performance

The admin web service shall handle concurrent load sufficient to demonstrate autoscaling and response-time behaviour under scripted test conditions.

### NFR8 Cost awareness

The solution shall be designed to support clear seven-day cost estimation and comparison with an always-on VM baseline.

### NFR9 Sustainability awareness

The solution shall support a defensible carbon-impact comparison using published estimation methods or calculators.

### NFR10 Reproducibility

Another technically competent reader shall be able to rebuild and redeploy the system from the submitted artefacts with only minor environment-specific changes.

## Cloud Infrastructure Requirements

### CIR1 Registry

All deployable images shall be stored in Azure Container Registry with tagged versions.

### CIR2 Runtime

The deployed application containers shall run on Azure Container Apps or, if required for strict brief compliance, Azure Container Instances.

### CIR3 Ingress

Public HTTPS access shall be provided through Azure ingress with a DNS name and TLS.

### CIR4 Networking

The deployment shall use a VNet with controlled ingress and private data connectivity.

### CIR5 Data tier isolation

The relational database shall not be publicly accessible.

### CIR6 Storage

Generated PDF files shall be stored in Azure Blob Storage with restricted container access.

### CIR7 Secrets

Sensitive configuration shall be injected at runtime from Azure Key Vault or managed secrets.

### CIR8 Monitoring

Azure Monitor, Log Analytics, and Application Insights shall collect container logs, service metrics, alerts, and dashboard views.

### CIR9 Scaling

At least one Azure-hosted container service shall implement autoscaling thresholds and scale-in or scale-out evidence.

### CIR10 Infrastructure definition

Infrastructure should be defined in Terraform or Azure-native infrastructure-as-code templates where feasible.

## Data Requirements

- Use synthetic owner, pet, treatment case, and programme data only.
- Store structured programme metadata in MySQL.
- Store PDF file path, version, generation time, and file identifier in the database.
- Keep the schema small and assessment-focused.
- Do not attempt full production clinical retention policy implementation.

## Security and Ethics Requirements

- No live personal or clinical data.
- No secrets in repository history.
- HTTPS required for deployed demonstration.
- Limit public exposure to the ingress endpoint only.
- Use least-privilege managed identities and role assignments.
- Mention ethical handling of health-adjacent records in the report.
- Mention residual risks, including credential leakage, insecure object storage, and excessive logging.

## Observability and Testing Requirements

### Required evidence-producing tests

1. Local integration test proving all Compose services run together.
2. Cloud smoke test proving the public endpoint responds over HTTPS.
3. PDF generation test proving the second container is used successfully.
4. Load test proving measurable latency and scaling behaviour.
5. Failure test proving diagnosis and recovery, for example bad secret, service crash, or database connection misconfiguration.

### Required metrics and artefacts

- request count
- response time or latency percentile
- CPU and memory usage
- replica count before and after scaling
- application logs for a publish request
- error logs for one failed scenario
- alarm trigger or threshold screenshot where feasible

### Recommended tooling

- `k6` for load testing
- Azure Monitor dashboards, Log Analytics queries, and Application Insights for monitoring
- Terraform plan or deployment manifest for reproducibility

## Cost and Carbon Requirements

### Required comparison

The report must compare the assessed container solution against a VM baseline over seven days.

### Recommended comparison model

- Azure container deployment sized for the assessed MVP
- Azure VM baseline sized to host equivalent application components continuously

### Recommended evidence sources

- Azure Pricing Calculator for infrastructure cost estimation
- Microsoft sustainability guidance, Cloud Carbon Footprint methodology, or another clearly cited carbon estimation approach
- Azure regional service documentation and availability information

### Distinction-level expectation

Do not provide only one total number. Include:

- workload assumptions
- uptime assumptions
- traffic assumptions
- storage assumptions
- a brief discussion of why bursty container workloads can reduce over-provisioning compared with always-on VMs

## Architecture Decisions to Defend in the Report

### Decision 1: Azure container platform over VM-only deployment

Defence:

- cleaner autoscaling, ingress, and observability than a VM-only solution
- less server maintenance than a VM
- stronger managed-container narrative for the rubric

Counter-argument to address:

- higher abstraction can reduce low-level control and may cost more at steady high utilisation than a right-sized reserved VM

### Decision 2: Managed Azure Database for MySQL instead of MySQL container in cloud

Defence:

- separates application container concerns from stateful database operations
- improves backup, patching, resilience, and assessment credibility

Counter-argument to address:

- increases service cost and reduces symmetry with local Compose environment

### Decision 3: Separate PDF service

Defence:

- satisfies two-container requirement with a meaningful boundary
- isolates a CPU- or memory-sensitive rendering workload
- creates an explicit service interaction to discuss and monitor

Counter-argument to address:

- adds complexity compared with inline PDF generation in the web app

### Decision 4: Synthetic data and thin vertical slice

Defence:

- ethically appropriate
- faster to finish end to end
- better aligned to assessment evidence than building broad unused functionality

## Required Report Support Material

The implementation must produce evidence for each required report section.

### Executive summary

Prepare one paragraph summarising the business problem, container platform chosen, key architecture choices, and measured outcomes.

### Scenario and requirements

Capture the manual-programme problem, practitioner needs, security constraints, synthetic-data ethics, and performance assumptions.

### Design justification

Prepare a comparison table of ACI, Fargate, and VM covering:

- scaling
- networking
- operational complexity
- observability
- cost behaviour
- portability

### Architecture diagram

Create one labelled diagram showing:

Internet -> Front Door or Azure ingress -> Azure containerised admin service -> PDF service -> Azure Database for MySQL and Blob Storage -> Azure Monitor

### Build and deploy

Record:

- Dockerfile design
- Compose usage
- image push to Azure Container Registry
- container app or ACI deployment steps
- secret injection approach
- storage configuration

### Network, security, and scaling

Record:

- VNet layout
- subnet purpose
- NSGs and ingress restrictions
- DNS and TLS
- autoscaling policies
- why the database is private

### Test, monitor, and fix

Record:

- load test setup
- one realistic failure
- Azure Monitor metrics and logs
- diagnosis and remediation steps

### Cost and carbon

Record:

- seven-day container cost
- seven-day VM baseline
- storage assumptions
- carbon method and limitations

### Trends and next steps

Discuss:

- serverless containers
- GitOps
- green computing
- future queue-based PDF scaling
- optional migration or multi-cloud portability considerations

## Deliverables Required From Development Work

### Mandatory implementation artefacts

- Dockerfile for admin web service
- Dockerfile for PDF service
- `docker-compose.yml`
- cloud deployment configuration
- application configuration template
- synthetic seed data
- load test script
- screenshots or exported evidence folder
- token-to-CSS design mapping for the frontend shell

### Strongly recommended optional artefacts

- Terraform IaC
- short demo video
- concise deployment README
- architecture diagram source

## Suggested Delivery Plan for Opus 4.7 and Sonnet

### Phase 1 Architecture and repository setup

- create server-side solution structure
- define container boundaries
- add local Compose stack
- seed synthetic data

### Phase 2 Core workflow slice

- implement owner, pet, case, and programme browse flow
- implement draft programme creation
- implement PDF generation API contract

### Phase 3 Deployment readiness

- write Dockerfiles
- validate Compose
- configure ACR and Azure deployment
- configure secrets and environment variables

### Phase 4 Observability and resilience

- add structured logging
- configure Azure Monitor metrics and alerts
- define autoscaling thresholds
- run failure scenarios and fixes

### Phase 5 Assessment evidence

- run load tests
- capture screenshots and metrics
- estimate seven-day cost and carbon
- build report tables and diagrams

## Minimum Acceptance Criteria

The solution is assessment-ready only when all of the following are true:

- two application containers build successfully;
- local Compose stack runs successfully;
- images are pushed to Azure Container Registry;
- cloud deployment is reachable over HTTPS;
- practitioner can complete one end-to-end publish flow with synthetic data;
- PDF is generated by the separate PDF service;
- generated PDF is stored in Azure Blob Storage;
- Azure Monitor shows logs and metrics for the deployed services;
- at least one scaling event or clearly measured scaling policy is demonstrated;
- seven-day cost and carbon comparison is documented;
- one real issue has been investigated and fixed with evidence.

## Stretch Goals

These are useful only if the assessed MVP is already complete:

- queue-based PDF generation
- blue-green deployment variant
- WAF integration
- private service discovery between containers
- GitHub Actions deployment pipeline
- ACI fallback appendix if strict brief compliance is required

## Final Guidance

The safest path to a strong distinction is to deliver a smaller solution with excellent evidence rather than a broad solution with weak operational proof. The marker is rewarding depth, critique, and reproducibility. Build the minimum clinically believable workflow, deploy it cleanly, measure it properly, and document the trade-offs honestly.
