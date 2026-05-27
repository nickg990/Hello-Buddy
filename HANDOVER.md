# Hello Buddy — Handover (end of Day 1, Tuesday 27 May 2026)

You are picking this up on your **laptop** in a fresh VS Code install. This document is the bridge between the DevBox session that completed Day 1 and the laptop session that will run Day 2 (Wednesday) onwards.

---

## 1. Where things stand

**Day 1 (Tuesday) is COMPLETE.** The data tier is deployed to Azure and seeded.

Verified by query at end of Day 1: Buddy treatment case exists with active programme containing 10 session exercises.

### Deployed Azure resources

Subscription: `ca4b6814-924d-4263-9637-9a8599b21a60`
Tenant: `fa896e75-4446-4f9c-ae57-8c735aa692a6`

| Resource              | Name                                             | Region     |
| --------------------- | ------------------------------------------------ | ---------- |
| Resource Group        | `rg-hellobuddy-prod`                             | uksouth    |
| Key Vault             | `kv-hellobuddy-prod`                             | uksouth    |
| MySQL Flexible Server | `mysql-hellobuddy-prod.mysql.database.azure.com` | **ukwest** |
| MySQL Database        | `canine_physiotherapy`                           | ukwest     |
| Automation Account    | `aa-hellobuddy-prod`                             | uksouth    |

### Key Vault secrets

- `mysql-admin-password` — admin password
- `mysql-connection-string` — full ADO.NET-style connection string (use this from the app)

### MySQL firewall rules currently active

| Name                 | IP               | Purpose                                                          |
| -------------------- | ---------------- | ---------------------------------------------------------------- |
| `developer-ip-temp`  | `109.158.71.135` | **Your laptop's known IP** — may need updating if IP has changed |
| `devbox-ip-temp`     | `20.90.136.3`    | DevBox egress — can be removed once you've left DevBox           |
| `AllowAzureServices` | `0.0.0.0`        | Lets Cloud Shell and Container Apps reach the server             |

If the laptop IP rule doesn't match your current public IP, update it:

```powershell
$myip = (Invoke-RestMethod -Uri "https://api.ipify.org")
az mysql flexible-server firewall-rule update --resource-group rg-hellobuddy-prod --name mysql-hellobuddy-prod --rule-name developer-ip-temp --start-ip-address $myip --end-ip-address $myip
```

### Automation schedules

MySQL auto-starts and auto-stops on a working-hours pattern (Europe/London time):

| Day              | Start | Stop  |
| ---------------- | ----- | ----- |
| Monday           | 06:00 | 19:00 |
| Tuesday–Thursday | 07:00 | 19:00 |
| Friday           | 07:00 | 18:00 |
| Saturday–Sunday  | OFF   | OFF   |

If the server appears unreachable, check whether it has been stopped by the schedule before debugging anything else:

```powershell
az mysql flexible-server show --resource-group rg-hellobuddy-prod --name mysql-hellobuddy-prod --query state -o tsv
az mysql flexible-server start --resource-group rg-hellobuddy-prod --name mysql-hellobuddy-prod
```

---

## 2. Laptop setup checklist

Install in this order. PowerShell is the assumed shell.

1. **VS Code** — https://code.visualstudio.com/
2. **Git for Windows** — https://git-scm.com/download/win
3. **.NET 8 SDK** — `winget install Microsoft.DotNet.SDK.8` (needed for the existing MAUI app and Day 2 web app)
4. **Azure CLI** — `winget install Microsoft.AzureCLI`
5. **Terraform** — `winget install HashiCorp.Terraform`
6. **MySQL Shell** (optional, for CLI access) — `winget install Oracle.MySQLShell`
7. **VS Code extensions:** see the table below. All extensions installed on the DevBox are listed, plus a few extras you'll want for Day 2 (Terraform, MySQL client inside VS Code, Azure Resources).

### Extensions installed on the DevBox (replicate on laptop)

| Extension ID                           | Name                | Purpose                                                                  |
| -------------------------------------- | ------------------- | ------------------------------------------------------------------------ |
| `github.copilot`                       | GitHub Copilot      | AI code completion (install alongside Copilot Chat)                      |
| `github.copilot-chat`                  | GitHub Copilot Chat | Chat-based AI assistance — this conversation lives here                  |
| `ms-dotnettools.csdevkit`              | C# Dev Kit          | Full .NET project/solution support                                       |
| `ms-dotnettools.csharp`                | C#                  | C# language server (dependency of Dev Kit)                               |
| `ms-dotnettools.dotnet-maui`           | .NET MAUI           | MAUI tooling (the existing Canine Physio App)                            |
| `ms-dotnettools.vscode-dotnet-runtime` | .NET Install Tool   | Runtime acquisition helper                                               |
| `nromanov.dotnet-meteor`               | .NET Meteor         | Alternative MAUI runner / debugger                                       |
| `ms-vscode.powershell`                 | PowerShell          | PowerShell language support and integrated console                       |
| `ms-azuretools.vscode-containers`      | Container Tools     | Dockerfile + container management (needed Thu for Container Apps)        |
| `ms-vscode-remote.remote-containers`   | Dev Containers      | Open repo inside a dev container if desired                              |
| `mermaidchart.vscode-mermaid-chart`    | Mermaid Chart       | Edit/preview the `.md` mermaid diagrams in `Canine Physio Requirements/` |
| `tomoki1207.pdf`                       | vscode-pdf          | **PDF preview inside VS Code** — needed for the publish-flow PDFs        |
| `eamodio.gitlens`                      | GitLens             | Git history, blame, branches                                             |
| `figma.figma-vscode-extension`         | Figma for VS Code   | View Figma wireframes from `Designs/wireframes/`                         |
| `ms-vsliveshare.vsliveshare`           | Live Share          | Optional pair-programming                                                |
| `sonarsource.sonarlint-vscode`         | SonarLint           | Static analysis                                                          |
| `dbaeumer.vscode-eslint`               | ESLint              | JS/TS lint                                                               |
| `esbenp.prettier-vscode`               | Prettier            | Code formatter                                                           |
| `christian-kohler.path-intellisense`   | Path Intellisense   | Filepath autocomplete                                                    |
| `ronnidc.nunjucks`                     | Nunjucks            | Template language support                                                |
| `vscode-icons-team.vscode-icons`       | vscode-icons        | File-tree icons                                                          |

### Extras to add on the laptop (not on the DevBox yet)

| Extension ID                               | Name                 | Why                                                                     |
| ------------------------------------------ | -------------------- | ----------------------------------------------------------------------- |
| `hashicorp.terraform`                      | HashiCorp Terraform  | Syntax + validation for `Infrastructure/terraform/`                     |
| `cweijan.vscode-mysql-client2`             | MySQL (Weijan Chen)  | Query Azure MySQL from inside VS Code (replaces Cloud Shell for ad-hoc) |
| `ms-azuretools.vscode-azureresourcegroups` | Azure Resources      | Browse the deployed Azure resources                                     |
| `ms-azuretools.vscode-azurecontainerapps`  | Azure Container Apps | Useful from Thursday onwards for US-15 to US-19                         |

### One-shot install command (run on the laptop after installing VS Code)

```powershell
$extensions = @(
  'github.copilot',
  'github.copilot-chat',
  'ms-dotnettools.csdevkit',
  'ms-dotnettools.csharp',
  'ms-dotnettools.dotnet-maui',
  'ms-dotnettools.vscode-dotnet-runtime',
  'nromanov.dotnet-meteor',
  'ms-vscode.powershell',
  'ms-azuretools.vscode-containers',
  'ms-vscode-remote.remote-containers',
  'mermaidchart.vscode-mermaid-chart',
  'tomoki1207.pdf',
  'eamodio.gitlens',
  'figma.figma-vscode-extension',
  'ms-vsliveshare.vsliveshare',
  'sonarsource.sonarlint-vscode',
  'dbaeumer.vscode-eslint',
  'esbenp.prettier-vscode',
  'christian-kohler.path-intellisense',
  'ronnidc.nunjucks',
  'vscode-icons-team.vscode-icons',
  'hashicorp.terraform',
  'cweijan.vscode-mysql-client2',
  'ms-azuretools.vscode-azureresourcegroups',
  'ms-azuretools.vscode-azurecontainerapps'
)
foreach ($ext in $extensions) { code --install-extension $ext }
```

After each winget install, refresh PATH in any open terminal:

```powershell
$env:Path = [System.Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path','User')
```

### Authenticate Azure CLI

```powershell
az login
az account set --subscription ca4b6814-924d-4263-9637-9a8599b21a60
```

### Clone the repo

The current workspace lives at `C:\Projects\Hello-Buddy\` on the DevBox. On the laptop, clone wherever you like; paths in this document will need adjusting.

### Recreate `terraform.tfvars` (NOT committed)

The file `Infrastructure/terraform/data-tier/terraform.tfvars` is gitignored because it contains secrets. Recreate it on the laptop before running any Terraform:

```hcl
subscription_id      = "ca4b6814-924d-4263-9637-9a8599b21a60"
resource_group_name  = "rg-hellobuddy-prod"
location             = "uksouth"
key_vault_name       = "kv-hellobuddy-prod"
mysql_server_name    = "mysql-hellobuddy-prod"
mysql_admin_username = "netintentions"
mysql_admin_password = "<paste from password manager>"
developer_ip         = "<your laptop public IP>"
mysql_location       = "ukwest"
```

Credentials are in your password manager — they were shared in the original Copilot chat session but are not stored here.

### Optional: hydrate Terraform state

The Terraform state file from Day 1 is on the DevBox at `Infrastructure/terraform/data-tier/terraform.tfstate`. Three options:

1. **Copy it over** — fastest. Bring `terraform.tfstate` + `.terraform/` directory across.
2. **Re-init and import** — clean but tedious; 11 resources to import.
3. **Run `terraform import` only when you need to make a change** — pragmatic; just leave the state empty until Friday's Terraform changes.

If you go with option 1, also bring the lock file `.terraform.lock.hcl`.

---

## 3. Day 2 plan (Wednesday)

**Headline goal:** Build the admin web app in VS Code against Azure MySQL.

### Story group 5 deliverables

- ASP.NET Core web app (Razor Pages or MVC — decision needed)
- Case detail page
- Programme builder page
- In-page preview of the programme PDF
- Publish flow that writes the PDF to local filesystem (Blob wiring deferred to Friday)
- Application Insights telemetry wired via configuration (resource creation deferred)
- Connection string sourced from Key Vault secret `mysql-connection-string`

### Wednesday COB checkpoint

1. App boots against Azure MySQL using the Key Vault-backed connection string
2. Case detail + programme builder + in-page preview pages render
3. Publish flow writes a PDF to local filesystem
4. App Insights telemetry config in place

### Open decisions to make before coding starts

1. **Web framework**: ASP.NET Core MVC vs Razor Pages vs Blazor Server. The repo already has a MAUI app (client-side) — the web app is a **new project** alongside it, not a replacement.
2. **PDF library**: PuppeteerSharp (Chromium-based, container-friendly), QuestPDF (pure C#, simpler API, but commercial licence for >$1M revenue orgs — fine for assessment), or DinkToPdf (wkhtmltopdf wrapper, dated).
3. **Solution structure**: separate `.sln` for the web admin? Or add to existing `Canine Physio App.slnx`?
4. **Authentication**: Five-Day Plan says "seeded single-practitioner auth placeholder" — confirm no Entra ID work this week.

---

## 4. Key gotchas learned on Day 1

- **UK South MySQL B1ms unavailable** at the subscription level — fell back to UK West via `mysql_location` tfvar. The capabilities API itself returns 500 for uksouth currently.
- **DevBox blocks outbound TCP 3306** — corporate egress firewall. This is why we used Cloud Shell for SQL execution. Your laptop should not have this restriction.
- **Cloud Shell ships MariaDB client**, not MySQL — use `--ssl` flag, not `--ssl-mode=REQUIRED`.
- **`AllowAzureServices` firewall rule** is required for Cloud Shell to reach MySQL — start IP `0.0.0.0` is Azure's convention for this. Will be removed Friday during US-29 lockdown.
- **Failed Azure deployments leave stale resources** that block re-creation in different regions. Cleanup: `az group delete --yes --no-wait` then `az keyvault purge --name <kv> --location <region>`.
- **VS Code terminals lose PATH** after a `winget install`. Refresh with the one-liner above.

---

## 5. Reference files

| File                                                                                      | Purpose                                                                            |
| ----------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `MSc Assessment/Five-Day Delivery Plan.md`                                                | Day-by-day plan and checkpoints                                                    |
| `MSc Assessment/Epic and User Stories.md`                                                 | Backlog with story groups 3–12                                                     |
| `MSc Assessment/DecisionLog.md`                                                           | DEC-001 to DEC-004 — MySQL tier, scheduled start/stop, UK West, AllowAzureServices |
| `MSc Assessment/Option C Architectural Brief and Requirements.md`                         | Overall architecture target                                                        |
| `Infrastructure/terraform/data-tier/main.tf`                                              | The deployed data-tier module (19 resources)                                       |
| `Infrastructure/terraform/data-tier/terraform.tfvars`                                     | Gitignored — recreate on laptop                                                    |
| `Canine Physio Database/Build and Initialise/Canine Physio DB Scripts v2.3 (fresh).sql`   | Schema (already applied)                                                           |
| `Canine Physio Database/Build and Initialise/Canine Physio DB Day 1 Initialise v2.4.sql`  | Reference data (already applied)                                                   |
| `Canine Physio Database/Build and Initialise/Canine Physio DB MSc Assessment Seed v1.sql` | MSc demo data (already applied)                                                    |

---

## 6. First task on laptop

1. Connect VS Code's MySQL extension to `mysql-hellobuddy-prod.mysql.database.azure.com` (user `netintentions`, SSL required).
2. Run:
   ```sql
   USE canine_physiotherapy;
   SELECT * FROM Owner WHERE Email = 'emma.thompson@example.com';
   SELECT * FROM Pet WHERE OwnerId = (SELECT OwnerId FROM Owner WHERE Email = 'emma.thompson@example.com');
   SELECT COUNT(*) AS ExerciseCount FROM SessionExercise se
   JOIN Session s ON se.SessionId = s.SessionId
   JOIN Programme p ON s.ProgrammeId = p.ProgrammeId
   WHERE p.TreatmentCaseId = (SELECT TreatmentCaseId FROM TreatmentCase WHERE Title = 'Buddy hind limb rehabilitation');
   ```
   The last query should return `10`. If it does, Day 1 has been correctly handed over.
3. Open a fresh Copilot chat and paste a link to this file as the starting context. Begin Day 2 by answering the four open decisions in section 3.

---

_Generated by Copilot at end of Day 1 (Tuesday 27 May 2026). All Day 1 commitments verified before this handover was written._
