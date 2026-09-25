#!/usr/bin/env pwsh
# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
    Configure Application Insights for Warewolf Execution Lightweight Azure Function

.DESCRIPTION
    This script automates the setup of Application Insights integration:
    1. Creates an Application Insights resource (if it doesn't exist)
    2. Retrieves the connection string
    3. Configures the Function App with the connection string
    4. Enables Application Insights logging
    5. Verifies the configuration

.PARAMETER ResourceGroup
    The Azure resource group name

.PARAMETER FunctionAppName
    The name of your Azure Function App

.PARAMETER Location
    Azure region (default: eastus)

.PARAMETER AppInsightsName
    Name for the Application Insights resource (default: {FunctionAppName}-ai)

.PARAMETER ConnectionStringSettingName
    App setting the connection string is written to. Default WAREWOLF_APPINSIGHTS_CONNECTION_STRING -
    the Execution Engine's deliberately non-standard name (see Step 5). The Service Bus worker and
    the job processor pass APPLICATIONINSIGHTS_CONNECTION_STRING: they read the standard name, and
    they WANT the Functions host pipeline, which is what writes the requests/exceptions tables.

.PARAMETER SkipLoggingConfigFlag
    Do not set WAREWOLF_LOGGING_CONFIG.appInsights. That flag is read only by the Execution Engine;
    on any other app it is a setting nothing consumes.

.EXAMPLE
    .\Setup-ApplicationInsights.ps1 -ResourceGroup "my-rg" -FunctionAppName "my-function-app"

.EXAMPLE
    .\Setup-ApplicationInsights.ps1 -ResourceGroup "my-rg" -FunctionAppName "my-function-app" -Location "westus2"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true)]
    [string]$FunctionAppName,

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $false)]
    [string]$AppInsightsName = "$FunctionAppName-ai",

    [Parameter(Mandatory = $false)]
    [ValidateSet('WAREWOLF_APPINSIGHTS_CONNECTION_STRING', 'APPLICATIONINSIGHTS_CONNECTION_STRING')]
    [string]$ConnectionStringSettingName = 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING',

    [Parameter(Mandatory = $false)]
    [switch]$SkipLoggingConfigFlag,

    # Waits (seconds) between retries of a TRANSIENT az failure - see WwAzCli.psm1. Not part of
    # the documented surface: the tests pass @(0) so a retry does not sleep.
    [Parameter(DontShow)]
    [int[]]$RetryDelaySeconds = @(3, 7, 15, 30)
)

$ErrorActionPreference = "Stop"

Import-Module (Join-Path $PSScriptRoot 'WwAzCli.psm1') -Force

function Invoke-AiAz {
    <#
        Runs one az command, retrying it while the failure is transient (an ARM outage page,
        throttling, a connection reset). Build 30801 failed this script on a one-off ARM 503.
        Returns az's stdout and leaves $LASTEXITCODE at the final exit code, so every caller
        keeps its existing `$LASTEXITCODE -ne 0` check and message. On success, az's stderr
        (warnings) is echoed rather than returned, so it cannot corrupt a `-o tsv` value.
        Arguments are never echoed: one of them is the App Insights connection string.
    #>
    param(
        [Parameter(Mandatory)][string[]] $Arguments,
        # Do not echo the failure output (a probe whose "not found" is an expected answer).
        [switch] $Quiet
    )
    $result = Invoke-WwAzWithRetry -Invoke { & az @Arguments 2>&1 } -DelaySeconds $RetryDelaySeconds -OnRetry {
        param($attempt, $max, $wait)
        Write-Host ("   ⚠️  transient Azure error (attempt {0}/{1}) - retrying in {2}s" -f $attempt, $max, $wait) -ForegroundColor DarkYellow
    }
    if ($result.ExitCode -ne 0) {
        # Surface the real cause (az's stderr used to reach the console directly) without failing
        # here - the caller prints its own message and exits.
        if (-not $Quiet) { $result.Output | ForEach-Object { Write-Host "   $_" -ForegroundColor DarkGray } }
        $global:LASTEXITCODE = $result.ExitCode
        return $null
    }
    $result.Output | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] } |
        ForEach-Object { Write-Host "   $_" -ForegroundColor DarkGray }
    $global:LASTEXITCODE = 0
    return @($result.Output | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })
}

Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Application Insights Setup for Warewolf Execution Lightweight" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 1: Verify Azure CLI is installed and logged in
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 1/6] Verifying Azure CLI..." -ForegroundColor Yellow

try {
    $null = Invoke-AiAz -Arguments @('account', 'show') -Quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Not logged in to Azure CLI. Please run: az login" -ForegroundColor Red
        exit 1
    }
    $subscription = Invoke-AiAz -Arguments @('account', 'show', '--query', 'name', '-o', 'tsv')
    Write-Host "✅ Logged in to Azure subscription: $subscription" -ForegroundColor Green
}
catch {
    Write-Host "❌ Azure CLI not found. Please install: https://aka.ms/azure-cli" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 2: Verify resource group exists
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 2/6] Verifying resource group..." -ForegroundColor Yellow

$rgExists = Invoke-AiAz -Arguments @('group', 'exists', '--name', $ResourceGroup)
if ($rgExists -eq "false") {
    Write-Host "❌ Resource group '$ResourceGroup' does not exist." -ForegroundColor Red
    Write-Host "   Create it with: az group create --name $ResourceGroup --location $Location" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Resource group '$ResourceGroup' exists" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 3: Check if Application Insights resource exists, create if needed
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 3/6] Checking Application Insights resource..." -ForegroundColor Yellow

# -Quiet: "not found" is the expected answer for a first run, not an error worth printing.
$aiExists = Invoke-AiAz -Quiet -Arguments @(
    'monitor', 'app-insights', 'component', 'show',
    '--app', $AppInsightsName, '--resource-group', $ResourceGroup)

if ($LASTEXITCODE -ne 0) {
    Write-Host "📦 Application Insights '$AppInsightsName' not found. Creating..." -ForegroundColor Yellow

    Invoke-AiAz -Arguments @(
        'monitor', 'app-insights', 'component', 'create',
        '--app', $AppInsightsName, '--location', $Location, '--resource-group', $ResourceGroup,
        '--application-type', 'web', '--kind', 'web') | Out-Null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Application Insights resource created successfully" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Failed to create Application Insights resource" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "✅ Application Insights resource '$AppInsightsName' already exists" -ForegroundColor Green
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 4: Get Application Insights connection string
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 4/6] Retrieving Application Insights connection string..." -ForegroundColor Yellow

$connectionString = (Invoke-AiAz -Arguments @(
    'monitor', 'app-insights', 'component', 'show',
    '--app', $AppInsightsName, '--resource-group', $ResourceGroup,
    '--query', 'connectionString', '--output', 'tsv') | Out-String).Trim()

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host "❌ Failed to retrieve connection string" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Connection string retrieved" -ForegroundColor Green
Write-Host "   Connection String: $($connectionString.Substring(0, 50))..." -ForegroundColor Gray
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 5: Configure Function App with Application Insights
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 5/6] Configuring Function App..." -ForegroundColor Yellow

# Set Application Insights connection string.
# IMPORTANT: by default this is WAREWOLF_APPINSIGHTS_CONNECTION_STRING — the Execution Engine's
# DELIBERATELY non-standard name (see Warewolf.Execution.Lightweight/Program.cs). The standard
# APPLICATIONINSIGHTS_CONNECTION_STRING auto-enables the Functions HOST's own AI pipeline (a
# separate process the engine cannot switch off), so for the engine it is intentionally NOT
# set — WAREWOLF_LOGGING_CONFIG.appInsights remains the single authoritative switch.
# The Service Bus worker and job processor pass -ConnectionStringSettingName
# APPLICATIONINSIGHTS_CONNECTION_STRING: their SDK reads the standard name, and the host
# pipeline is what gives them request telemetry (see their WorkerTelemetry.cs).
Write-Host "   → Setting $ConnectionStringSettingName..." -ForegroundColor Gray
Invoke-AiAz -Arguments @(
    'functionapp', 'config', 'appsettings', 'set',
    '--name', $FunctionAppName, '--resource-group', $ResourceGroup,
    '--settings', "$ConnectionStringSettingName=$connectionString",
    '--output', 'none') | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to set connection string" -ForegroundColor Red
    exit 1
}

if ($SkipLoggingConfigFlag) {
    Write-Host "   → Skipping WAREWOLF_LOGGING_CONFIG.appInsights (-SkipLoggingConfigFlag: only the engine reads it)" -ForegroundColor Gray
}
else {
    # Enable Application Insights logging in AzureExecutionLogger.
    # WOLF-8516: ENABLEAPPLICATIONINSIGHTS was merged into the WAREWOLF_LOGGING_CONFIG JSON
    # app setting alongside console/elasticsearch/performanceCounters/structuredLogs. An app
    # setting is a whole-value overwrite, so read-merge-write: fetch whatever
    # Deploy-WwExecutionEngine.ps1 already wrote, flip appInsights to true, preserve the rest.
    Write-Host "   → Setting WAREWOLF_LOGGING_CONFIG.appInsights=true..." -ForegroundColor Gray
    $existingLoggingConfigRaw = (Invoke-AiAz -Arguments @(
        'functionapp', 'config', 'appsettings', 'list',
        '--name', $FunctionAppName, '--resource-group', $ResourceGroup,
        '--query', "[?name=='WAREWOLF_LOGGING_CONFIG'].value | [0]",
        '--output', 'tsv') | Out-String).Trim()

    $loggingConfigValue = [ordered]@{}
    if ($existingLoggingConfigRaw -and $existingLoggingConfigRaw -ne 'None') {
        try {
            ($existingLoggingConfigRaw | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $loggingConfigValue[$_.Name] = $_.Value }
        }
        catch {
            Write-Host "   ⚠️  Existing WAREWOLF_LOGGING_CONFIG is not valid JSON — it will be replaced." -ForegroundColor Yellow
        }
    }
    $loggingConfigValue['appInsights'] = $true

    Invoke-AiAz -Arguments @(
        'functionapp', 'config', 'appsettings', 'set',
        '--name', $FunctionAppName, '--resource-group', $ResourceGroup,
        '--settings', "WAREWOLF_LOGGING_CONFIG=$($loggingConfigValue | ConvertTo-Json -Compress)",
        '--output', 'none') | Out-Null

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to enable Application Insights logging" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ Function App configured successfully" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 6: Verify configuration
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 6/6] Verifying configuration..." -ForegroundColor Yellow

$settingsJson = Invoke-AiAz -Arguments @(
    'functionapp', 'config', 'appsettings', 'list',
    '--name', $FunctionAppName, '--resource-group', $ResourceGroup,
    '--query', "[?name=='$ConnectionStringSettingName' || name=='WAREWOLF_LOGGING_CONFIG']",
    '--output', 'json') | Out-String
$settings = if ([string]::IsNullOrWhiteSpace($settingsJson)) { @() } else { $settingsJson | ConvertFrom-Json }

$aiConnectionSet = $settings | Where-Object { $_.name -eq $ConnectionStringSettingName }
$loggingConfigSet = $settings | Where-Object { $_.name -eq "WAREWOLF_LOGGING_CONFIG" }
# With -SkipLoggingConfigFlag the flag is not part of this app's contract, so it is not checked.
$aiEnabled = [bool]$SkipLoggingConfigFlag
if (-not $aiEnabled -and $loggingConfigSet) {
    try { $aiEnabled = ($loggingConfigSet.value | ConvertFrom-Json).appInsights -eq $true } catch { $aiEnabled = $false }
}

if ($aiConnectionSet -and $aiEnabled) {
    Write-Host "✅ Configuration verified successfully" -ForegroundColor Green
}
else {
    Write-Host "⚠️  Configuration incomplete. Please verify manually." -ForegroundColor Yellow
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Summary
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ Application Insights Setup Complete!" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Application Insights Details:" -ForegroundColor Cyan
Write-Host "   Resource Name    : $AppInsightsName" -ForegroundColor White
Write-Host "   Resource Group   : $ResourceGroup" -ForegroundColor White
Write-Host "   Function App     : $FunctionAppName" -ForegroundColor White
Write-Host ""
Write-Host "🔍 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Wait 2-3 minutes for telemetry to start flowing" -ForegroundColor White
Write-Host "   2. Trigger a workflow execution" -ForegroundColor White
Write-Host "   3. View logs in Azure Portal → Application Insights → Logs" -ForegroundColor White
Write-Host ""
Write-Host "📖 Testing Queries (run in Application Insights → Logs):" -ForegroundColor Cyan
Write-Host ""
Write-Host "   // View all Warewolf logs from last 10 minutes" -ForegroundColor Gray
Write-Host "   traces" -ForegroundColor Yellow
Write-Host "   | where timestamp > ago(10m)" -ForegroundColor Yellow
Write-Host "   | where customDimensions.Category contains 'Warewolf'" -ForegroundColor Yellow
Write-Host "   | order by timestamp desc" -ForegroundColor Yellow
Write-Host ""
Write-Host "   // View function invocations" -ForegroundColor Gray
Write-Host "   requests" -ForegroundColor Yellow
Write-Host "   | where timestamp > ago(10m)" -ForegroundColor Yellow
Write-Host "   | project timestamp, name, success, duration" -ForegroundColor Yellow
Write-Host ""
Write-Host "   // View audit logs (security events)" -ForegroundColor Gray
Write-Host "   traces" -ForegroundColor Yellow
Write-Host "   | where customDimensions.EventId == 9000" -ForegroundColor Yellow
Write-Host "   | where message contains '[AUDIT]'" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔗 Quick Links:" -ForegroundColor Cyan
Write-Host "   Portal: https://portal.azure.com/#@/resource/subscriptions/.../resourceGroups/$ResourceGroup/providers/microsoft.insights/components/$AppInsightsName" -ForegroundColor Blue
Write-Host "   Docs  : docs/Warewolf-Lightweight-Logging-Guide.md" -ForegroundColor Blue
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
