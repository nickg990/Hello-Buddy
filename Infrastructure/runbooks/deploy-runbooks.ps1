<#
.SYNOPSIS
Deploy or remove container scaling runbooks and schedules in Azure Automation.

.DESCRIPTION
This script uses Azure CLI exclusively (no Az PowerShell modules required).
It creates runbooks and schedules for scaling Container Apps during working hours.
#>

[CmdletBinding()]
param(
    [string]$ResourceGroup = "rg-hellobuddy-prod",
    [string]$AutomationAccountName = "aa-hellobuddy-prod",
    [string]$ContainerAppsResourceGroup = "rg-hellobuddy-prod",
    [string]$SubscriptionId = "",
    [string]$TimeZone = "GMT Standard Time",
    [ValidatePattern('^\d{2}:\d{2}$')]
    [string]$StartTime = "09:00",
    [ValidatePattern('^\d{2}:\d{2}$')]
    [string]$EndTime = "17:00",
    [switch]$Cleanup,
    [switch]$SkipPublish
)

# Suppress Azure CLI warnings at the source (experimental/preview command groups)
$env:AZURE_CORE_ONLY_SHOW_ERRORS = "true"
$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Green
}

function Write-WarnText {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

function Write-ErrText {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Red
}

# Helper to run Azure CLI commands with proper error handling.
# Key behaviour: native stderr (warnings) must NOT terminate the script.
# PowerShell 5.1 turns native stderr into terminating errors when
# $ErrorActionPreference = 'Stop', so we relax it for the duration of the call
# and rely on the exit code to decide success/failure.
function Invoke-AzureCLI {
    param(
        [string[]]$Arguments,
        [switch]$SuppressOutput,
        [switch]$IgnoreError
    )
    
    $previousEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    
    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        # Always force errors-only output and redirect stderr to a temp file so
        # warnings never reach the PowerShell error stream.
        $allArgs = @($Arguments) + @("--only-show-errors")
        $output = & az @allArgs 2>$tempFile
        $exitCode = $LASTEXITCODE
        
        $stderr = Get-Content $tempFile -ErrorAction SilentlyContinue
        
        if ($exitCode -ne 0 -and -not $IgnoreError) {
            $errorMsg = "Azure CLI command failed: az $($Arguments -join ' ')"
            if ($stderr) {
                $errorMsg += "`nError: $($stderr -join ' ')"
            }
            if ($output) {
                $errorMsg += "`nOutput: $($output -join ' ')"
            }
            throw $errorMsg
        }
        
        if (-not $SuppressOutput) {
            return $output
        }
    }
    finally {
        $ErrorActionPreference = $previousEap
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }
}

# Helper for ARM REST calls. Writes the JSON body to a temp file (avoids shell
# quoting issues) and sets an explicit Content-Type header (az rest otherwise
# sends application/octet-stream which ARM rejects with 415 Unsupported Media Type).
function Invoke-ArmRest {
    param(
        [Parameter(Mandatory = $true)][ValidateSet("GET", "PUT", "POST", "DELETE", "PATCH")][string]$Method,
        [Parameter(Mandatory = $true)][string]$Url,
        [string]$JsonBody,
        [switch]$IgnoreError,
        [switch]$ReturnOutput
    )

    $restArgs = @("rest", "--method", $Method, "--url", $Url)

    $bodyFile = $null
    if ($PSBoundParameters.ContainsKey('JsonBody') -and -not [string]::IsNullOrWhiteSpace($JsonBody)) {
        $bodyFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $bodyFile -Value $JsonBody -Encoding UTF8 -NoNewline
        $restArgs += @("--headers", "Content-Type=application/json", "--body", "@$bodyFile")
    }

    try {
        if ($ReturnOutput) {
            return Invoke-AzureCLI -Arguments ($restArgs + @("-o", "json")) -IgnoreError:$IgnoreError
        }
        Invoke-AzureCLI -Arguments ($restArgs + @("-o", "none")) -SuppressOutput -IgnoreError:$IgnoreError
    }
    finally {
        if ($bodyFile) {
            Remove-Item $bodyFile -Force -ErrorAction SilentlyContinue
        }
    }
}

function Get-NextWeekdayStart {
    param(
        [string]$Weekday,
        [string]$ClockTime
    )
    
    $targetDay = [System.DayOfWeek]::$Weekday
    $parts = $ClockTime.Split(':')
    $hour = [int]$parts[0]
    $minute = [int]$parts[1]
    
    $now = Get-Date
    $candidate = Get-Date -Year $now.Year -Month $now.Month -Day $now.Day -Hour $hour -Minute $minute -Second 0
    
    # Find next occurrence of the target weekday
    while ($candidate.DayOfWeek -ne $targetDay -or $candidate -le $now) {
        $candidate = $candidate.AddDays(1)
    }
    
    return $candidate
}

Write-Info "========================================="
Write-Info " Container Scaling Runbook Deployment"
Write-Info "========================================="

# Check Azure CLI availability
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-ErrText "Azure CLI not found. Please install from: https://aka.ms/installazurecli"
    exit 1
}

# Check Azure login
Write-Info "Checking Azure CLI authentication..."
$accountInfo = Invoke-AzureCLI @("account", "show", "--output", "json") -IgnoreError
if ($LASTEXITCODE -ne 0) {
    Write-ErrText "Not logged in to Azure. Please run: az login"
    exit 1
}

$account = $accountInfo | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($SubscriptionId)) {
    $SubscriptionId = $account.id
    Write-Info "Using current subscription: $($account.name)"
}

# Set subscription
Invoke-AzureCLI @("account", "set", "--subscription", $SubscriptionId) -SuppressOutput

# Install automation extension if needed (ignore preview warnings)
Write-Info "Ensuring Azure CLI automation extension is installed..."
$extJson = Invoke-AzureCLI @("extension", "list", "--output", "json") -IgnoreError
$hasAutomation = $false
if ($extJson) {
    $extList = $extJson | ConvertFrom-Json
    $hasAutomation = ($extList | Where-Object { $_.name -eq "automation" }) -ne $null
}
if (-not $hasAutomation) {
    Invoke-AzureCLI @("extension", "add", "--name", "automation", "--yes", "--allow-preview") -SuppressOutput -IgnoreError
}

# Verify automation account exists and capture its managed identity principalId
Write-Info "Verifying Automation Account..."
$aaCheck = Invoke-AzureCLI @("automation", "account", "show", "-g", $ResourceGroup, "-n", $AutomationAccountName, "-o", "json") -IgnoreError
if ($LASTEXITCODE -ne 0) {
    Write-ErrText "Automation account '$AutomationAccountName' not found in resource group '$ResourceGroup'"
    exit 1
}

$aa = $aaCheck | ConvertFrom-Json
$automationPrincipalId = $null
if ($aa.identity -and $aa.identity.principalId) {
    $automationPrincipalId = $aa.identity.principalId
}

Write-Ok "Connected to Azure"
Write-Ok "Subscription: $SubscriptionId"  
Write-Ok "Automation Account: $AutomationAccountName"

# Container apps that get scaled (must match the runbook content below)
$scaledContainerApps = @("ca-hello-buddy-ui", "ca-hello-buddy-api", "ca-hello-buddy-pdf")

# Define resources
$runbookNames = @("Scale-ContainersUp", "Scale-ContainersDown")
$scheduleMap = @(
    @{ Name = "Scale-Up-Mon"; Day = "Monday"; Time = $StartTime; Runbook = "Scale-ContainersUp" },
    @{ Name = "Scale-Up-Tue"; Day = "Tuesday"; Time = $StartTime; Runbook = "Scale-ContainersUp" },
    @{ Name = "Scale-Up-Wed"; Day = "Wednesday"; Time = $StartTime; Runbook = "Scale-ContainersUp" },
    @{ Name = "Scale-Up-Thu"; Day = "Thursday"; Time = $StartTime; Runbook = "Scale-ContainersUp" },
    @{ Name = "Scale-Up-Fri"; Day = "Friday"; Time = $StartTime; Runbook = "Scale-ContainersUp" },
    @{ Name = "Scale-Down-Mon"; Day = "Monday"; Time = $EndTime; Runbook = "Scale-ContainersDown" },
    @{ Name = "Scale-Down-Tue"; Day = "Tuesday"; Time = $EndTime; Runbook = "Scale-ContainersDown" },
    @{ Name = "Scale-Down-Wed"; Day = "Wednesday"; Time = $EndTime; Runbook = "Scale-ContainersDown" },
    @{ Name = "Scale-Down-Thu"; Day = "Thursday"; Time = $EndTime; Runbook = "Scale-ContainersDown" },
    @{ Name = "Scale-Down-Fri"; Day = "Friday"; Time = $EndTime; Runbook = "Scale-ContainersDown" }
)

# Handle cleanup mode
if ($Cleanup) {
    Write-WarnText "Cleanup mode: Removing container scaling resources..."
    
    # Remove job schedules
    Write-Info "Removing job schedule links..."
    $jobSchedules = Invoke-ArmRest -Method GET -Url "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccountName/jobSchedules?api-version=2023-11-01" -ReturnOutput -IgnoreError
    if ($jobSchedules) {
        $js = $jobSchedules | ConvertFrom-Json
        foreach ($item in $js.value) {
            $scheduleName = $item.properties.schedule.name
            if ($scheduleName -like "Scale-*") {
                Write-Info "  Removing link for schedule: $scheduleName"
                Invoke-ArmRest -Method DELETE -Url "https://management.azure.com$($item.id)?api-version=2023-11-01" -IgnoreError
            }
        }
    }
    
    # Remove schedules
    Write-Info "Removing schedules..."
    foreach ($s in $scheduleMap) {
        Write-Info "  Removing schedule: $($s.Name)"
        Invoke-ArmRest -Method DELETE -Url "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccountName/schedules/$($s.Name)?api-version=2023-11-01" -IgnoreError
    }
    
    # Remove runbooks
    Write-Info "Removing runbooks..."
    foreach ($runbookName in $runbookNames) {
        Write-Info "  Removing runbook: $runbookName"
        Invoke-AzureCLI @("automation", "runbook", "delete", "-g", $ResourceGroup, "--automation-account-name", $AutomationAccountName, "-n", $runbookName, "--yes") -SuppressOutput -IgnoreError
    }
    
    Write-Ok "Cleanup complete!"
    exit 0
}

# ---------------------------------------------------------------------------
# Grant the Automation Account managed identity permission to scale the
# container apps. Without this the runbook authenticates but gets HTTP 403
# when calling the Container Apps management API.
# Scoped to each container app individually (least privilege).
# ---------------------------------------------------------------------------
Write-Info ""
Write-Info "Ensuring managed identity has scale permissions on container apps..."

if ([string]::IsNullOrWhiteSpace($automationPrincipalId)) {
    Write-WarnText "  Could not determine Automation Account managed identity principalId."
    Write-WarnText "  The runbooks may fail with HTTP 403. Ensure the account has a system-assigned identity."
}
else {
    foreach ($appName in $scaledContainerApps) {
        $appScope = "/subscriptions/$SubscriptionId/resourceGroups/$ContainerAppsResourceGroup/providers/Microsoft.App/containerApps/$appName"

        # Check whether the role assignment already exists (idempotent)
        $existingJson = Invoke-AzureCLI @("role", "assignment", "list", "--assignee", $automationPrincipalId, "--scope", $appScope, "--role", "Container Apps Contributor", "-o", "json") -IgnoreError
        $alreadyAssigned = $false
        if ($existingJson) {
            $existing = $existingJson | ConvertFrom-Json
            $alreadyAssigned = @($existing).Count -gt 0
        }

        if ($alreadyAssigned) {
            Write-Ok "  Permission already present: $appName"
        }
        else {
            Write-Info "  Granting Container Apps Contributor on $appName ..."
            Invoke-AzureCLI @("role", "assignment", "create", "--assignee-object-id", $automationPrincipalId, "--assignee-principal-type", "ServicePrincipal", "--role", "Container Apps Contributor", "--scope", $appScope) -SuppressOutput
            Write-Ok "  Permission granted: $appName"
        }
    }
    Write-WarnText "  Note: RBAC changes can take 1-5 minutes to propagate before runbooks succeed."
}

# Create runbook content
$scaleUpContent = @'
# Scale Container Apps Up (minReplicas = 1)

$ErrorActionPreference = "Stop"

$subscriptionId = "ca4b6814-924d-4263-9637-9a8599b21a60"
$resourceGroup = "rg-hellobuddy-prod"

$containerApps = @(
    "ca-hello-buddy-ui",
    "ca-hello-buddy-api",
    "ca-hello-buddy-pdf"
)

$minReplicas = 1
$maxReplicas = 3

Connect-AzAccount -Identity | Out-Null
Set-AzContext -SubscriptionId $subscriptionId | Out-Null

foreach ($appName in $containerApps) {
    Write-Output "Scaling up: $appName"

    $uri = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.App/containerApps/${appName}?api-version=2023-05-01"

    $patchBody = @{
        properties = @{
            template = @{
                scale = @{
                    minReplicas = $minReplicas
                    maxReplicas = $maxReplicas
                }
            }
        }
    } | ConvertTo-Json -Depth 10

    try {
        $patchResponse = Invoke-AzRestMethod `
            -Method PATCH `
            -Path $uri `
            -Payload $patchBody

        if ($patchResponse.StatusCode -in @(200, 201, 202)) {
            Write-Output "Successfully requested scale up for $appName to minReplicas=$minReplicas"
        }
        else {
            Write-Error "Failed to scale $appName. Status: $($patchResponse.StatusCode). Response: $($patchResponse.Content)"
        }
    }
    catch {
        Write-Error "Exception while scaling $appName : $_"
    }
}

Write-Output "Scale up operation completed"
'@

$scaleDownContent = @'
# Scale Container Apps Down (minReplicas = 0)

$ErrorActionPreference = "Stop"

$subscriptionId = "ca4b6814-924d-4263-9637-9a8599b21a60"
$resourceGroup = "rg-hellobuddy-prod"

$containerApps = @(
    "ca-hello-buddy-ui",
    "ca-hello-buddy-api",
    "ca-hello-buddy-pdf"
)

$minReplicas = 0
$maxReplicas = 3

Connect-AzAccount -Identity | Out-Null
Set-AzContext -SubscriptionId $subscriptionId | Out-Null

foreach ($appName in $containerApps) {
    Write-Output "Scaling down: $appName"

    $uri = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.App/containerApps/${appName}?api-version=2023-05-01"

    $patchBody = @{
        properties = @{
            template = @{
                scale = @{
                    minReplicas = $minReplicas
                    maxReplicas = $maxReplicas
                }
            }
        }
    } | ConvertTo-Json -Depth 10

    try {
        $patchResponse = Invoke-AzRestMethod `
            -Method PATCH `
            -Path $uri `
            -Payload $patchBody

        if ($patchResponse.StatusCode -in @(200, 201, 202)) {
            Write-Output "Successfully requested scale down for $appName to minReplicas=$minReplicas"
        }
        else {
            Write-Error "Failed to scale $appName. Status: $($patchResponse.StatusCode). Response: $($patchResponse.Content)"
        }
    }
    catch {
        Write-Error "Exception while scaling $appName : $_"
    }
}

Write-Output "Scale down operation completed"
'@

# Create or update runbooks
Write-Info ""
Write-Info "Creating/updating runbooks..."

foreach ($runbook in @(
    @{ Name = "Scale-ContainersUp"; Content = $scaleUpContent; Description = "Scale container apps to minReplicas=1" },
    @{ Name = "Scale-ContainersDown"; Content = $scaleDownContent; Description = "Scale container apps to minReplicas=0" }
)) {
    Write-Info "  Processing runbook: $($runbook.Name)"
    
    # Check if runbook exists
    $existingRunbooks = Invoke-AzureCLI @("automation", "runbook", "list", "-g", $ResourceGroup, "--automation-account-name", $AutomationAccountName, "-o", "json")
    $runbookList = $existingRunbooks | ConvertFrom-Json
    $exists = ($runbookList | Where-Object { $_.name -eq $runbook.Name }) -ne $null
    
    if (-not $exists) {
        # Create new runbook
        Write-Info "    Creating new runbook..."
        Invoke-AzureCLI @("automation", "runbook", "create", "-g", $ResourceGroup, "--automation-account-name", $AutomationAccountName, "-n", $runbook.Name, "--type", "PowerShell", "--description", $runbook.Description) -SuppressOutput
    }
    
    # Update runbook content
    Write-Info "    Updating runbook content..."
    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        Set-Content -Path $tempFile -Value $runbook.Content -Encoding UTF8
        Invoke-AzureCLI @("automation", "runbook", "replace-content", "-g", $ResourceGroup, "--automation-account-name", $AutomationAccountName, "-n", $runbook.Name, "--content", "@$tempFile") -SuppressOutput
        
        if (-not $SkipPublish) {
            Write-Info "    Publishing runbook..."
            Invoke-AzureCLI @("automation", "runbook", "publish", "-g", $ResourceGroup, "--automation-account-name", $AutomationAccountName, "-n", $runbook.Name) -SuppressOutput
        }
    }
    finally {
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }
    
    Write-Ok "  Runbook ready: $($runbook.Name)"
}

# Create schedules and job schedules
Write-Info ""
Write-Info "Creating schedules and linking to runbooks..."

foreach ($schedule in $scheduleMap) {
    Write-Info "  Processing schedule: $($schedule.Name)"
    
    # Calculate next start time
    $nextStart = Get-NextWeekdayStart -Weekday $schedule.Day -ClockTime $schedule.Time
    $startTimeIso = $nextStart.ToString("yyyy-MM-ddTHH:mm:ssK")
    
    # Create or update schedule via REST API
    $scheduleBody = @{
        properties = @{
            description = "Automated scale $($schedule.Day) at $($schedule.Time)"
            startTime = $startTimeIso
            frequency = "Week"
            interval = 1
            timeZone = $TimeZone
            advancedSchedule = @{
                weekDays = @($schedule.Day)
            }
        }
    } | ConvertTo-Json -Depth 10 -Compress
    
    $scheduleUrl = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccountName/schedules/$($schedule.Name)?api-version=2023-11-01"
    Invoke-ArmRest -Method PUT -Url $scheduleUrl -JsonBody $scheduleBody
    
    # Remove existing job schedule links for this schedule
    $jobSchedulesUrl = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccountName/jobSchedules?api-version=2023-11-01"
    $existingLinks = Invoke-ArmRest -Method GET -Url $jobSchedulesUrl -ReturnOutput -IgnoreError
    
    if ($existingLinks) {
        $links = $existingLinks | ConvertFrom-Json
        foreach ($link in $links.value) {
            if ($link.properties.schedule.name -eq $schedule.Name) {
                $deleteUrl = "https://management.azure.com$($link.id)?api-version=2023-11-01"
                Invoke-ArmRest -Method DELETE -Url $deleteUrl -IgnoreError
            }
        }
    }
    
    # Create new job schedule link
    $jobScheduleId = [guid]::NewGuid().ToString()
    $linkBody = @{
        properties = @{
            schedule = @{ name = $schedule.Name }
            runbook = @{ name = $schedule.Runbook }
            parameters = @{}
        }
    } | ConvertTo-Json -Depth 10 -Compress
    
    $linkUrl = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Automation/automationAccounts/$AutomationAccountName/jobSchedules/${jobScheduleId}?api-version=2023-11-01"
    Invoke-ArmRest -Method PUT -Url $linkUrl -JsonBody $linkBody
    
    Write-Ok "  Schedule linked: $($schedule.Name) -> $($schedule.Runbook)"
}

# Display summary
Write-Info ""
Write-Ok "========================================="
Write-Ok " Deployment Complete!"
Write-Ok "========================================="
Write-Host ""
Write-Host "Resources deployed:" -ForegroundColor White
Write-Host "  * 2 runbooks (Scale-ContainersUp, Scale-ContainersDown)" -ForegroundColor Gray
Write-Host "  * 10 schedules (Mon-Fri at $StartTime and $EndTime)" -ForegroundColor Gray
Write-Host "  * 10 job schedule links" -ForegroundColor Gray
Write-Host ""
Write-Host "Test the runbooks manually:" -ForegroundColor White
Write-Host "  az automation runbook start -g $ResourceGroup --automation-account-name $AutomationAccountName -n Scale-ContainersUp" -ForegroundColor Yellow
Write-Host ""
Write-Host "View job output:" -ForegroundColor White
Write-Host "  az automation job list -g $ResourceGroup --automation-account-name $AutomationAccountName -o table" -ForegroundColor Yellow
Write-Host ""
Write-Host "Remove everything:" -ForegroundColor White
Write-Host "  .\deploy-runbooks.ps1 -Cleanup" -ForegroundColor Yellow