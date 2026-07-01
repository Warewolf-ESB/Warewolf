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
    [string]$AppInsightsName = "$FunctionAppName-ai"
)

$ErrorActionPreference = "Stop"

Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Application Insights Setup for Warewolf Execution Lightweight" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 1: Verify Azure CLI is installed and logged in
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 1/6] Verifying Azure CLI..." -ForegroundColor Yellow

try {
    $null = az account show 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Not logged in to Azure CLI. Please run: az login" -ForegroundColor Red
        exit 1
    }
    $subscription = az account show --query name -o tsv
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

$rgExists = az group exists --name $ResourceGroup
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

$aiExists = az monitor app-insights component show `
    --app $AppInsightsName `
    --resource-group $ResourceGroup `
    2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "📦 Application Insights '$AppInsightsName' not found. Creating..." -ForegroundColor Yellow

    az monitor app-insights component create `
        --app $AppInsightsName `
        --location $Location `
        --resource-group $ResourceGroup `
        --application-type web `
        --kind web | Out-Null

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

$connectionString = az monitor app-insights component show `
    --app $AppInsightsName `
    --resource-group $ResourceGroup `
    --query connectionString `
    --output tsv

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
# IMPORTANT: the lightweight worker reads WAREWOLF_APPINSIGHTS_CONNECTION_STRING — a
# DELIBERATELY non-standard name (see Program.cs). The standard
# APPLICATIONINSIGHTS_CONNECTION_STRING auto-enables the Functions HOST's own AI
# pipeline (a separate process the worker cannot switch off), so it is intentionally
# NOT set here — ENABLEAPPLICATIONINSIGHTS remains the single authoritative switch.
Write-Host "   → Setting WAREWOLF_APPINSIGHTS_CONNECTION_STRING..." -ForegroundColor Gray
az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroup `
    --settings "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$connectionString" `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to set connection string" -ForegroundColor Red
    exit 1
}

# Enable Application Insights logging in AzureExecutionLogger
Write-Host "   → Setting ENABLEAPPLICATIONINSIGHTS=true..." -ForegroundColor Gray
az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroup `
    --settings "ENABLEAPPLICATIONINSIGHTS=true" `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to enable Application Insights logging" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Function App configured successfully" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# Step 6: Verify configuration
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "[Step 6/6] Verifying configuration..." -ForegroundColor Yellow

$settings = az functionapp config appsettings list `
    --name $FunctionAppName `
    --resource-group $ResourceGroup `
    --query "[?name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING' || name=='ENABLEAPPLICATIONINSIGHTS']" `
    --output json | ConvertFrom-Json

$aiConnectionSet = $settings | Where-Object { $_.name -eq "WAREWOLF_APPINSIGHTS_CONNECTION_STRING" }
$aiEnabledSet = $settings | Where-Object { $_.name -eq "ENABLEAPPLICATIONINSIGHTS" }

if ($aiConnectionSet -and $aiEnabledSet.value -eq "true") {
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
