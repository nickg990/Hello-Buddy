# Container Scaling Runbook (Planned Enhancement)

## Overview

This runbook describes how to configure Azure Automation to scale Hello Buddy Container Apps from 0 to 1 replica during working hours and back to 0 outside hours, reducing operational costs when the system is not in active use.

## Rationale

- **Cost Optimization:** Container Apps incur charges per vCPU-hour and per replica. Scaling to 0 outside working hours eliminates compute charges for idle periods.
- **Sustainability:** Reduces energy consumption and carbon footprint during off-hours.
- **Simplicity:** Azure Automation handles scheduled scaling without application-level complexity.

## Architecture

The implementation uses:

1. **Azure Automation Account** – Already provisioned by Terraform (DEC-002, used for MySQL start/stop)
2. **Managed Identity** – Automation Account uses system-assigned identity with Contributor role on the Container Apps resource group
3. **PowerShell Runbooks** – Two runbooks via REST API (Invoke-AzRestMethod):
   - `Scale-ContainersUp` – sets minReplicas=1 for UI and API (before work)
   - `Scale-ContainersDown` – sets minReplicas=0 for UI and API (after work)
4. **Schedules** – Six automation schedules (matching MySQL pattern):
   - Monday 8 AM → Scale up
   - Tuesday–Friday 8 AM → Scale up
   - Monday–Thursday 6 PM → Scale down
   - Friday 6 PM → Scale down
   - (Saturday/Sunday – no operations, stays at 0)

**Note:** The PDF service (`ca-hello-buddy-pdf`) remains fixed at minReplicas=1 / maxReplicas=1 (no scaling), as it is a dependency of the API and must be available if the API is running.

## Container Selection

The runbooks target only the two user-facing Container Apps:

- **ca-hello-buddy-ui** – Current: minReplicas=1, maxReplicas=3 (scales with HTTP traffic). Will scale to 0 during off-hours.
- **ca-hello-buddy-api** – Current: minReplicas=1, maxReplicas=3 (scales with HTTP traffic). Will scale to 0 during off-hours.
- **ca-hello-buddy-pdf** – **No change** – remains fixed at 1 replica (not included in scaling runbooks).

## Terraform Implementation

Add the following to `Infrastructure/terraform/data-tier/main.tf` after the existing MySQL runbooks (after line 190):

### Step 1: Runbook Definitions

```hcl
# Container scaling runbooks
resource "azurerm_automation_runbook" "scale_containers_up" {
  name                    = "Scale-ContainersUp"
  location                = data.azurerm_resource_group.main.location
  resource_group_name     = data.azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  log_verbose             = false
  log_progress            = false
  description             = "Scales Hello Buddy UI and API container apps from 0 to 1 replica at start of working hours."
  runbook_type            = "PowerShell"

  content = <<-SCRIPT
    Connect-AzAccount -Identity
    $sub = "${var.subscription_id}"
    $rg  = "${var.container_apps_resource_group_name}"  # Container apps live in a separate RG
    
    $apps = @("ca-hello-buddy-ui", "ca-hello-buddy-api")
    
    foreach ($app in $apps) {
      Write-Output "Scaling up: $app (minReplicas → 1)"
      $uri = "/subscriptions/$sub/resourceGroups/$rg/providers/Microsoft.App/containerApps/$app?api-version=2023-05-02"
      
      # GET current app definition
      $getResult = Invoke-AzRestMethod -Method GET -Path $uri
      if ($getResult.StatusCode -ne 200) {
        Write-Error "Failed to get $app — HTTP $($getResult.StatusCode)"
        continue
      }
      
      $body = $getResult.Content | ConvertFrom-Json
      
      # Update minReplicas to 1
      $body.properties.template.scale.minReplicas = 1
      
      # PATCH the app
      $patchBody = $body | ConvertTo-Json -Depth 10
      $patchResult = Invoke-AzRestMethod -Method PATCH -Path $uri -Payload $patchBody
      
      if ($patchResult.StatusCode -notin 200, 201) {
        Write-Error "Failed to scale $app — HTTP $($patchResult.StatusCode): $($patchResult.Content)"
      } else {
        Write-Output "✓ Scaled $app to minReplicas=1 (HTTP $($patchResult.StatusCode))"
      }
    }
  SCRIPT
}

resource "azurerm_automation_runbook" "scale_containers_down" {
  name                    = "Scale-ContainersDown"
  location                = data.azurerm_resource_group.main.location
  resource_group_name     = data.azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  log_verbose             = false
  log_progress            = false
  description             = "Scales Hello Buddy UI and API container apps from 1 to 0 replicas at end of working hours."
  runbook_type            = "PowerShell"

  content = <<-SCRIPT
    Connect-AzAccount -Identity
    $sub = "${var.subscription_id}"
    $rg  = "${var.container_apps_resource_group_name}"
    
    $apps = @("ca-hello-buddy-ui", "ca-hello-buddy-api")
    
    foreach ($app in $apps) {
      Write-Output "Scaling down: $app (minReplicas → 0)"
      $uri = "/subscriptions/$sub/resourceGroups/$rg/providers/Microsoft.App/containerApps/$app?api-version=2023-05-02"
      
      # GET current app definition
      $getResult = Invoke-AzRestMethod -Method GET -Path $uri
      if ($getResult.StatusCode -ne 200) {
        Write-Error "Failed to get $app — HTTP $($getResult.StatusCode)"
        continue
      }
      
      $body = $getResult.Content | ConvertFrom-Json
      
      # Update minReplicas to 0
      $body.properties.template.scale.minReplicas = 0
      
      # PATCH the app
      $patchBody = $body | ConvertTo-Json -Depth 10
      $patchResult = Invoke-AzRestMethod -Method PATCH -Path $uri -Payload $patchBody
      
      if ($patchResult.StatusCode -notin 200, 201) {
        Write-Error "Failed to scale $app — HTTP $($patchResult.StatusCode): $($patchResult.Content)"
      } else {
        Write-Output "✓ Scaled $app to minReplicas=0 (HTTP $($patchResult.StatusCode))"
      }
    }
  SCRIPT
}
```

### Step 2: Schedules (add after the MySQL schedules, around line 232+)

```hcl
# Container scaling schedules (working hours: Mon–Fri 8 AM to 6 PM, UK timezone)

resource "azurerm_automation_schedule" "scale_up_monday" {
  name                    = "Scale-Up-Monday-8AM"
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  frequency               = "Week"
  interval                = 1
  start_time              = "2025-01-01T08:00:00Z"  # 8 AM UTC (adjust for your timezone)
  week_days               = ["Monday"]
  timezone                = "UTC"  # Change to "GMT Standard Time" for UK time
  description             = "Scale containers up on Monday morning"
}

resource "azurerm_automation_schedule" "scale_up_tue_fri" {
  name                    = "Scale-Up-Tue-Fri-8AM"
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  frequency               = "Week"
  interval                = 1
  start_time              = "2025-01-01T08:00:00Z"
  week_days               = ["Tuesday", "Wednesday", "Thursday", "Friday"]
  timezone                = "UTC"
  description             = "Scale containers up Tuesday–Friday mornings"
}

resource "azurerm_automation_schedule" "scale_down_mon_thu" {
  name                    = "Scale-Down-Mon-Thu-6PM"
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  frequency               = "Week"
  interval                = 1
  start_time              = "2025-01-01T18:00:00Z"  # 6 PM UTC
  week_days               = ["Monday", "Tuesday", "Wednesday", "Thursday"]
  timezone                = "UTC"
  description             = "Scale containers down Monday–Thursday evenings"
}

resource "azurerm_automation_schedule" "scale_down_friday" {
  name                    = "Scale-Down-Friday-6PM"
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  frequency               = "Week"
  interval                = 1
  start_time              = "2025-01-01T18:00:00Z"
  week_days               = ["Friday"]
  timezone                = "UTC"
  description             = "Scale containers down Friday evening"
}
```

### Step 3: Job Schedules (link runbooks to schedules, add after MySQL job schedules around line 250+)

```hcl
# Container scaling job schedules
resource "azurerm_automation_job_schedule" "scale_containers_up_monday" {
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  schedule_name           = azurerm_automation_schedule.scale_up_monday.name
  runbook_name            = azurerm_automation_runbook.scale_containers_up.name
}

resource "azurerm_automation_job_schedule" "scale_containers_up_tue_fri" {
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  schedule_name           = azurerm_automation_schedule.scale_up_tue_fri.name
  runbook_name            = azurerm_automation_runbook.scale_containers_up.name
}

resource "azurerm_automation_job_schedule" "scale_containers_down_mon_thu" {
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  schedule_name           = azurerm_automation_schedule.scale_down_mon_thu.name
  runbook_name            = azurerm_automation_runbook.scale_containers_down.name
}

resource "azurerm_automation_job_schedule" "scale_containers_down_friday" {
  automation_account_name = azurerm_automation_account.main.name
  resource_group_name     = data.azurerm_resource_group.main.name
  schedule_name           = azurerm_automation_schedule.scale_down_friday.name
  runbook_name            = azurerm_automation_runbook.scale_containers_down.name
}
```

### Step 4: Variables

Add a variable to `Infrastructure/terraform/data-tier/variables.tf` (if not already present):

```hcl
variable "container_apps_resource_group_name" {
  type        = string
  description = "Name of the resource group containing Container Apps (ca-hello-buddy-ui, ca-hello-buddy-api, ca-hello-buddy-pdf)"
  default     = "rg-hellobuddy-prod"  # Update to match your container RG
}
```

Or, if the container apps are in the same resource group as MySQL, simplify by using `data.azurerm_resource_group.main.name` directly in the runbook content.

## Deployment Instructions

1. **Update Terraform:** Add the runbook, schedule, and job schedule code above to `Infrastructure/terraform/data-tier/main.tf`.

2. **Plan:** 
   ```powershell
   Set-Location "Infrastructure/terraform/data-tier"
   terraform plan -out=scaling.plan
   ```

3. **Review:** Verify that the plan creates 2 new runbooks, 4 new schedules, and 4 new job schedules.

4. **Apply:**
   ```powershell
   terraform apply scaling.plan
   ```

5. **Verify in Azure Portal:**
   - Navigate to the Automation Account.
   - Check **Runbooks** → confirm `Scale-ContainersUp` and `Scale-ContainersDown` exist.
   - Check **Schedules** → confirm the 4 scaling schedules are listed.
   - Check **Jobs** → after the next scheduled run, you should see execution logs.

## Manual Testing

You can trigger the runbooks manually to verify they work before relying on the schedule:

### Via Azure Portal

1. Open the Automation Account → **Runbooks** → **Scale-ContainersUp**.
2. Click **Start** (top bar).
3. Monitor the **Jobs** tab for output (may take 10–15 seconds).

### Via Azure CLI

```powershell
# Test scale up
az automation runbook start --automation-account-name "aa-hellobuddy-prod" `
  --name "Scale-ContainersUp" --resource-group "rg-hellobuddy-prod"

# Test scale down
az automation runbook start --automation-account-name "aa-hellobuddy-prod" `
  --name "Scale-ContainersDown" --resource-group "rg-hellobuddy-prod"
```

Monitor job output in the portal under **Automation Account** → **Jobs**.

## Monitoring & Alerts

### View Scaling Events

**Azure Portal:**
1. Container Apps → **ca-hello-buddy-ui** → **Replicas** pane.
2. Observe minReplicas change from 1 ↔ 0 at scheduled times.

**Azure CLI:**
```powershell
# Watch replicas in real-time
while($true){
  cls
  Write-Host "UI Replicas:"
  az containerapp replica list -g rg-hellobuddy-prod -n ca-hello-buddy-ui -o table
  Write-Host "`nAPI Replicas:"
  az containerapp replica list -g rg-hellobuddy-prod -n ca-hello-buddy-api -o table
  Start-Sleep -Seconds 5
}
```

### View Automation Job Logs

**Azure Portal:**
1. Automation Account → **Jobs** → select the latest `Scale-ContainersUp` or `Scale-ContainersDown` job.
2. View **Output** (streaming logs during execution) and **All Logs** (full transcript).

**Azure CLI:**
```powershell
# List recent scaling jobs
az automation job list --automation-account-name "aa-hellobuddy-prod" `
  --resource-group "rg-hellobuddy-prod" -o table
```

## Customization

### Change Working Hours

Edit the `start_time` in the Terraform schedules:

- **8 AM UTC** = `"2025-01-01T08:00:00Z"`
- **6 PM UTC** = `"2025-01-01T18:00:00Z"`

For **UK timezone** (GMT/BST), use `timezone = "GMT Standard Time"` in the schedule resource (Terraform will apply daylight-saving adjustments automatically).

**Example:** To shift from 8 AM–6 PM to 9 AM–5 PM:

```hcl
start_time = "2025-01-01T09:00:00Z"  # for scale-up
start_time = "2025-01-01T17:00:00Z"  # for scale-down
```

### Include/Exclude Container Apps

In the runbook `$apps` array, add or remove app names:

```powershell
$apps = @("ca-hello-buddy-ui", "ca-hello-buddy-api")  # Exclude PDF
```

### Adjust Replica Counts

Edit the runbook `content` property:

- **Scale to 2 replicas (instead of 1):** `$body.properties.template.scale.minReplicas = 2`
- **Scale to 0 replicas (instead of 1):** `$body.properties.template.scale.minReplicas = 0`

## Troubleshooting

### Issue: "Failed to get ca-hello-buddy-ui — HTTP 403"

**Cause:** Automation Account identity lacks Contributor role on the Container Apps resource group.

**Solution:**
```powershell
$automationAccountPrincipalId = (Get-AzAutomationAccount -ResourceGroupName "rg-hellobuddy-prod" -Name "aa-hellobuddy-prod").Identity.PrincipalId
New-AzRoleAssignment -ObjectId $automationAccountPrincipalId -RoleDefinitionName "Contributor" -ResourceGroupName "rg-hellobuddy-prod"
```

Then re-run the runbook.

### Issue: "Scaling did not trigger at the scheduled time"

**Cause:** Schedule timezone mismatch or runbook disabled.

**Solution:**
1. Verify schedule timezone matches your location (use `"GMT Standard Time"` for UK).
2. In Azure Portal, check **Automation Account** → **Schedules** → select schedule → verify **Next run** time.
3. Check the runbook is **Published** (not Draft). In Portal: **Runbooks** → select runbook → if Draft, click **Publish**.

### Issue: "RuntimeError: MinReplicas must be >= 0 and <= maxReplicas"

**Cause:** Container App definition specifies `maxReplicas < minReplicas` (invalid state).

**Solution:** Ensure the container app's current scale rule has `maxReplicas >= minReplicas`. Check via:
```powershell
az containerapp show -g rg-hellobuddy-prod -n ca-hello-buddy-ui --query "properties.template.scale"
```

## Cost Savings Estimate

Assuming:
- 2 Container Apps scaled (UI, API)
- Each: 0.5 vCPU, 1 GB memory
- Off-hours: 10 PM Friday – 8 AM Monday (60 hours/week) + 6 PM–8 AM weekdays (26 hours/week) = ~86 hours/week at scale 0
- Azure Container Apps pricing: ~£0.00264/hour per vCPU

**Rough weekly savings:**
- 86 hours × 0.5 vCPU × 2 apps × £0.00264/hour ≈ **£0.23/week** (~£12/year per app)
- Scales higher with more vCPU or off-hours.

(Note: CPU is only charged when scale > 0; memory is always charged, but memory cost is minimal.)

## Related Decision Records

- **DEC-002:** MySQL scheduled start/stop (same Automation Account pattern).
- **Previous autoscaling:** UI/API use HTTP-based scale rules (concurrent_requests=50) for load-driven scaling; this runbook adds **time-based scaling**.

## See Also

- [Azure Container Apps Scaling](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)
- [Azure Automation Runbooks](https://learn.microsoft.com/en-us/azure/automation/automation-runbook-types)
- [Azure REST API – Container Apps](https://learn.microsoft.com/en-us/rest/api/containerapps/)
