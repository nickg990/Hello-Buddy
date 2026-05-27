# Hello Buddy Cloud Admin — MSc Assessment Decision Log

Decisions recorded here are those with cost, architecture, or operational significance to the COM712 Assessment 3 submission. Each entry captures the choice made, the rationale, and the evidence supporting it.

---

## DEC-001 — MySQL Flexible Server tier and compute size

**Date:** 27 May 2026
**Area:** Data tier cost management
**Decision:** Deploy Azure Database for MySQL Flexible Server using the Burstable tier, B1ms SKU (1 vCore, 2 GiB RAM), 20 GB storage, 1-day backup retention, geo-redundant backup disabled.

**Rationale:**
The assessment workload is a single-practitioner demo with low and intermittent query volume. The Burstable tier is explicitly designed for workloads with flexible compute requirements that do not need full CPU continuously. B1ms is the smallest available Flexible Server SKU and is sufficient for the first-increment vertical slice. General Purpose and Memory Optimised tiers would add significant cost with no benefit at this scale.

**Cost basis (UK South, pay-as-you-go, May 2026):**

| Component                             | Rate                                   | Monthly cost      |
| ------------------------------------- | -------------------------------------- | ----------------- |
| Compute — B1ms                        | $12.41/month (full 720 hrs)            | $12.41            |
| Storage — 20 GB                       | $0.115/GB/month                        | $2.30             |
| Backup storage (1-day retention, LRS) | Free up to 100% of provisioned storage | $0.00             |
| IOPS                                  | $0.20/million (pay-per-use)            | ~$0.00            |
| **Unscheduled total**                 |                                        | **~$14.71/month** |

**Alternatives considered:**

- B2ms (2 vCore): $99.28/month compute — not justified for a two-practitioner demo workload.
- General Purpose D2ds: $124–163/month — appropriate for production multi-tenant scale, not for assessment MVP.

---

## DEC-002 — Azure Automation scheduled start/stop for MySQL

**Date:** 27 May 2026
**Area:** Data tier cost management
**Decision:** Deploy an Azure Automation Account (Basic SKU) with scheduled runbooks to start and stop the MySQL Flexible Server on a business-hours-only pattern. The Automation Account is provisioned as part of the data-tier Terraform module.

**Schedule:**

| Day              | Start                  | Stop  |
| ---------------- | ---------------------- | ----- |
| Monday           | 06:00                  | 19:00 |
| Tuesday–Thursday | 07:00                  | 19:00 |
| Friday           | 07:00                  | 18:00 |
| Saturday–Sunday  | — (server remains off) | —     |

Timezone: `Europe/London` (handles BST/GMT transition automatically).

**Rationale:**
MySQL Flexible Server charges for compute by the hour regardless of traffic — there is no scale-to-zero for the database engine. Stopping the server outside working hours and over the weekend reduces active compute hours from 720/month to approximately 65/week (≈ 282/month), a saving of roughly 61%.

**Implementation:**
Runbooks use `Connect-AzAccount -Identity` (managed identity) and `Invoke-AzRestMethod` to call the MySQL start/stop REST API. No additional Az module installation is required as `Az.Accounts` is pre-installed in all Automation Accounts. The Automation Account's managed identity is granted `Contributor` scoped to the MySQL server resource only (least-privilege).

**Cost basis:**

| Component                                    | Unscheduled       | Scheduled (~65 hrs/week)         |
| -------------------------------------------- | ----------------- | -------------------------------- |
| Compute                                      | $12.41/month      | ~$5.50/month                     |
| Storage                                      | $2.30/month       | $2.30/month (unchanged)          |
| Automation jobs (~43 runs/month, each <60 s) | —                 | $0.00 (within 500 min free tier) |
| **Total**                                    | **~$14.71/month** | **~$7.80/month**                 |

**Saving:** approximately $6.90/month (~47%) on the data tier during the assessment window.

**Note for report:** MySQL stop/start is appropriate during development and for the assessment period. In a production deployment serving active practitioners, the server would remain running during business hours by default and this schedule would be replaced by monitoring-driven scaling decisions.

---

## DEC-003 — MySQL Flexible Server deployed to UK West (data tier only)

**Date:** 27 May 2026
**Area:** Region selection
**Decision:** The MySQL Flexible Server is provisioned in **UK West**. All other resources (Resource Group, Key Vault, Automation Account) remain in **UK South**.

**Rationale:**
The first `terraform apply` failed with `ProvisionNotSupportedForRegion` for MySQL Flexible Server B1ms in UK South. Capability probing via `az rest` against `/subscriptions/.../providers/Microsoft.DBforMySQL/locations/{region}/capabilities` confirmed that, for this subscription on this date, the capabilities endpoint itself returned `InternalServerError` for `northeurope`, `uksouth`, and `eastus`, while returning `OK` for `westeurope` and `ukwest`. UK West is the closest available region and was selected to keep data-residency in the UK.

**Implementation:**
The Terraform module exposes a `mysql_location` variable that defaults to the same region as the rest of the data tier but is overridden in `terraform.tfvars` to `ukwest`. The MySQL server has no `zone` pin so Azure is free to place it in any available zone within UK West.

**Cost / latency impact:**
Inter-region latency UK West ↔ UK South is typically <10 ms — negligible for this workload. There is no inter-region data egress charge for Azure-internal traffic between paired UK regions in the same subscription. No cost change versus the original UK South plan.

**Note for report:** This is a real-world capacity issue that demonstrates the value of declarative IaC — region migration was a single variable change plus a fresh apply, not a manual portal recreation. Future production deployments should adopt either UK West permanently or implement a region-fallback pattern at the Terraform variable level.

---

## DEC-004 — `AllowAzureServices` firewall rule on MySQL (assessment scope only)

**Date:** 27 May 2026
**Area:** Network security
**Decision:** Add a firewall rule named `AllowAzureServices` (start/end IP `0.0.0.0`) to the MySQL Flexible Server to permit connections from Azure Cloud Shell and other Azure-internal services during the assessment phase.

**Rationale:**
The DevBox used for development sits behind a corporate egress firewall that blocks outbound TCP/3306. Azure Cloud Shell is the practical workaround for running the database initialisation and seed scripts. The `AllowAzureServices` rule (start IP `0.0.0.0`) is Azure's documented mechanism for enabling Cloud Shell and other PaaS-internal traffic to reach a MySQL Flexible Server with public networking.

**Security implications:**
This rule permits TCP/3306 from any Azure tenant, not just our own. Authentication is still required (strong admin password, SSL/TLS enforced). For the assessment window this is an acceptable trade-off, but it is **not appropriate for production**.

**Lockdown plan (Friday US-29):**
On Friday the firewall posture is tightened:

1. The `developer-ip-temp` and `devbox-ip-temp` rules are removed.
2. The `AllowAzureServices` rule is removed once the Container Apps subnet has direct line-of-sight to MySQL via VNet integration (or, if VNet integration is descoped, a single rule scoped to the Container Apps egress IP range).
3. Final state: MySQL is reachable only from the Hello Buddy Container Apps environment.

**Note for report:** This decision is documented as a deliberate, time-bounded compromise for the assessment seed workflow. The lockdown step on Friday is the evidence that the production posture differs from the development posture.

---
