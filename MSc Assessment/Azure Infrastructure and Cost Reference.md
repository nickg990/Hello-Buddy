# Hello Buddy Cloud Admin

## Azure Infrastructure and Cost Reference

## Purpose

This document captures the recommended Azure deployment model, container layout, scaling approach, and cost position for COM712 Assessment 3 Option C.

It is designed to support both implementation decisions and assessment write-up. It also records the rationale for evaluating AKS and ruling it out for the current workload.

## Scope Assumptions

This reference assumes the following production profile:

- one production environment only;
- separate development environment spun up only when needed;
- two physiotherapists using the system;
- typical operating window of 09:00 to 17:00 on working days;
- low traffic volume;
- moderate PDF generation volume;
- initial increment focused on case detail, programme builder, PDF preview, and publish;
- durable business persistence in MySQL;
- published PDF storage in Azure Blob Storage.

## Recommended Azure Architecture

### Chosen platform

Use Azure Container Apps rather than AKS for the production MVP.

### Why Azure Container Apps is recommended

- lower baseline cost than AKS for small and intermittent workloads;
- scale-to-zero support for non-public workloads;
- independent scaling per service;
- less operational overhead than Kubernetes cluster management;
- still provides strong container, networking, and monitoring evidence for the assessment.

## Container Topology

Deploy three application containers.

### 1. UI container

Name:

- `hello-buddy-ui`

Responsibility:

- public-facing practitioner web frontend;
- renders case detail, programme builder, PDF preview, and publish workflow;
- calls the internal API service.

Ingress:

- public HTTPS ingress enabled.

### 2. API container

Name:

- `hello-buddy-api`

Responsibility:

- loads case and programme data from MySQL;
- saves draft programme changes;
- runs validation rules;
- orchestrates publish operations;
- calls the PDF service;
- stores metadata and Blob references.

Ingress:

- internal only.

### 3. PDF generator container

Name:

- `hello-buddy-pdf`

Responsibility:

- renders HTML to PDF using a shared template;
- returns generated PDF output to the API or writes it via the API workflow;
- handles the heavier rendering workload independently from the UI.

Ingress:

- internal only.

## Managed Azure Services

In addition to the three application containers, use these managed Azure services:

- Azure Container Registry
- Azure Container Apps Environment
- Azure Database for MySQL Flexible Server
- Azure Blob Storage
- Azure Key Vault
- Azure Monitor
- Log Analytics
- Application Insights

Optional for custom domain and edge routing:

- Azure Front Door
- Azure DNS

## Public And Internal Exposure Model

Only the UI container should be internet-facing.

Recommended exposure model:

- `hello-buddy-ui`: external ingress enabled;
- `hello-buddy-api`: internal ingress only;
- `hello-buddy-pdf`: internal ingress only;
- MySQL: private access only;
- Blob Storage: private container access only.

This gives a clean security story for the assessment:

- the public edge is limited;
- internal services are not exposed directly;
- sensitive data paths stay behind the application boundary.

## Data And Storage Model

### MySQL

Use MySQL to store:

- treatment case data for the demo slice;
- draft programme data;
- programme sessions and exercises;
- publish metadata;
- PDF reference data.

### Blob Storage

Use Blob Storage to store:

- published PDF files;
- optional supporting artefacts for demo or evidence capture if needed.

### Why both are used

MySQL provides durable structured business data. Blob Storage provides durable object storage for generated PDF outputs. This is stronger than either a blob-only or in-memory-only architecture for the canine physiotherapy admin scenario.

## Why AKS Was Evaluated And Rejected

AKS is technically valid for this solution, but it is not the recommended platform for the current workload.

### AKS strengths

- stronger Kubernetes-native control;
- richer scheduling and networking options;
- strong fit for larger multi-service systems;
- useful if the assessment specifically wanted Kubernetes depth.

### AKS weaknesses for this project

- higher baseline monthly cost;
- cluster and node management overhead;
- more moving parts to configure and monitor;
- more delivery risk for a small student project;
- more difficult to justify economically for two practitioners using the system only during working hours.

### Rationale for rejection

For a low-volume, cost-sensitive admin application with intermittent PDF generation, Azure Container Apps gives the required service separation, autoscaling, ingress, and monitoring without the fixed infrastructure cost and operational complexity of AKS.

## Cost Comparison Summary

### High-level conclusion

Three Azure Container Apps plus a small MySQL Flexible Server and Blob Storage is realistically capable of staying near the target budget of around `£30/month`.

AKS is not realistically compatible with that target budget once node compute and monitoring overhead are included.

## Pricing Notes

These figures are planning estimates only.

- Azure pricing varies by region, agreement, and currency conversion.
- Microsoft pricing pages explicitly state that prices are estimates and not actual quotes.
- Final costing should be validated in the Azure Pricing Calculator before submission.

## Published Pricing Signals Used

### Azure Container Apps

Published pricing indicates:

- consumption-based billing by the second;
- active and idle compute billing rates;
- included monthly free grant for some usage;
- no usage charges when an app scales to zero;
- requests billed separately after free allowance.

### Azure Database for MySQL Flexible Server

Published pricing indicates burstable tier entry points including:

- `B1s` at about `$6.205/month`
- `B1ms` at about `$12.41/month`

Storage is listed separately, with small additional monthly cost at low volume.

### Azure Blob Storage

Published pricing indicates low storage cost for small volumes, with hot-tier storage roughly around `$0.15/GB/month` and low operation cost at modest transaction volume.

### AKS

Published pricing indicates:

- Free tier removes control-plane charge but not node cost;
- Standard tier currently shows about `$73/month` control-plane pricing before node charges;
- node VMs, networking, load balancing, and monitoring remain additional costs.

## Monthly Cost Shape For Recommended Architecture

These are indicative cost bands, not exact invoice predictions.

### Recommended production target shape

- UI container app always warm;
- API container app scale-to-zero allowed;
- PDF container app scale-to-zero allowed;
- MySQL Flexible Server on burstable tier;
- Blob Storage hot tier with low total volume.

### Estimated monthly cost bands

| Component             | Assumption                             | Indicative monthly cost |
| --------------------- | -------------------------------------- | ----------------------- |
| MySQL Flexible Server | `B1ms` burstable                       | `£9 to £10`             |
| MySQL storage         | small dataset                          | `under £1`              |
| Blob Storage          | low PDF volume, hot tier               | `under £1`              |
| UI Container App      | small always-warm frontend             | `£4 to £8`              |
| API Container App     | internal, low volume, scales down hard | `£3 to £6`              |
| PDF Container App     | bursty, scale-to-zero                  | `£1 to £4`              |
| Monitoring overhead   | light use of logs and metrics          | `small but variable`    |

### Indicative total

- lean operating pattern: `£18 to £24/month`
- safer allowance with some headroom: `£24 to £30/month`

This means the target budget is plausible, but only if service sizes remain small and internal services are allowed to scale down aggressively.

## Why The Budget Works With Container Apps

The budget works because:

- the public UI is the only service kept warm;
- the API can be mostly idle outside user interactions;
- the PDF service can remain at zero until publish activity occurs;
- the workload is light and limited to office hours;
- the MySQL server can begin on a burstable tier.

## Why The Budget Does Not Work Well With AKS

The budget becomes difficult with AKS because:

- cluster nodes are paid for even when workload is low;
- small workloads still need enough node capacity for system pods and app pods;
- monitoring and ingress overhead are less trivial;
- the fixed cost floor is much higher than a consumption-based service model.

Even if AKS Free tier is used, node cost still makes the architecture poor for a `£30/month` target.

## Recommended Scaling Rules

### UI container app

Recommended starting allocation:

- `0.25 vCPU`
- `0.5 GiB RAM`

Recommended scaling:

- `min replicas: 1`
- `max replicas: 3`
- HTTP concurrency trigger: `20 to 30`
- optional CPU trigger around `70%`

Reasoning:

- the UI is public-facing and should avoid cold starts;
- two practitioners do not require large scale, but a small ceiling allows a visible load-test story.

### API container app

Recommended starting allocation:

- `0.25 vCPU`
- `0.5 GiB RAM`

Recommended scaling:

- `min replicas: 0`
- `max replicas: 3`
- HTTP concurrency trigger: `20 to 25`
- CPU trigger around `65 to 70%`

Reasoning:

- the API can scale to zero when unused;
- it should scale independently from the UI;
- low baseline size keeps cost down.

### PDF container app

Recommended starting allocation:

- `0.5 vCPU`
- `1 GiB RAM`

Recommended scaling:

- `min replicas: 0`
- `max replicas: 2`
- synchronous HTTP concurrency trigger: `1 to 2`
- optional CPU trigger around `60 to 70%`

Reasoning:

- PDF generation is the heaviest workload per request;
- it should not remain warm permanently unless user experience requires it;
- independent scale protects the UI from PDF-related workload spikes.

## Expected Behaviour Under Load

### Under low normal usage

- UI remains available with one warm replica;
- API may scale down to zero or near-zero activity;
- PDF service remains at zero until a publish action occurs.

### Under moderate interactive usage

- UI scales first on HTTP concurrency;
- API scales independently as save and preview requests increase;
- PDF service only scales when publish or preview rendering activity increases.

### Under bursty publish activity

- PDF service can scale separately without forcing the UI to scale the same way;
- this is one of the strongest reasons to keep it as a dedicated service.

## Recommended Production Configuration

This is the recommended initial production shape.

### Container Apps

- UI: `min 1`, `max 3`
- API: `min 0`, `max 3`
- PDF: `min 0`, `max 2`

### MySQL

- start at `B1ms`
- small provisioned storage only
- upgrade later only if measured load justifies it

### Blob Storage

- standard general-purpose v2
- hot tier
- private container

### Security and secrets

- use managed identities where possible
- keep secrets in Key Vault
- keep API and PDF services internal only

## Recommended Development Approach

To stay within budget and reduce friction:

- use the production infrastructure for the initial development period before go-live, if acceptable for the module and client;
- spin up separate development infrastructure only when needed later;
- keep the initial container sizes small and measure before increasing capacity.

## Assessment-Ready Justification Paragraph

The deployment architecture was designed to balance cloud-native separation of concerns with a strict budget target. Three Azure Container Apps were selected for the UI, application service, and PDF generator because they allow each workload to scale independently and, for internal services, scale to zero when idle. Azure Kubernetes Service was evaluated but rejected on cost and operational-overhead grounds. Although AKS offers stronger Kubernetes-native control, it introduces a materially higher baseline cost through cluster infrastructure and node capacity that is not justified for a two-practitioner workload operating mainly during business hours. Azure Database for MySQL Flexible Server on a burstable tier and Azure Blob Storage for published PDFs provide durable persistence at relatively low cost. This architecture therefore offers the strongest balance of separation, security, scalability, and affordability for the assessment scenario.

## Report Notes

For the final assessment report, include:

- AKS as an evaluated but rejected option;
- the budget target as a formal design constraint;
- the fact that the PDF service scales independently from UI traffic;
- the reason the UI stays warm while API and PDF can scale to zero;
- a note that the exact prices should be validated in the Azure Pricing Calculator.

## Final Recommendation

Use three Azure Container Apps, MySQL Flexible Server `B1ms`, and Blob Storage.

That is the most defensible design for:

- budget;
- operational simplicity;
- assessment clarity;
- a strong comparison against AKS;
- a realistic path to demonstrating secure networking, autoscaling, monitoring, and cloud trade-offs.
