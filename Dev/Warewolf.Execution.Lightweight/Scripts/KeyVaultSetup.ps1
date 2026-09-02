# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
#Requires -Version 7.0
<#
.SYNOPSIS
    One-time Azure RBAC and Function App configuration for Warewolf Execution Lightweight.
    PowerShell 7 port of KeyVaultSetup.azcli.

.DESCRIPTION
    Assumes the Key Vault and Function App already exist and the Function's
    System-Assigned Managed Identity is already enabled.

    What this script does:
      4. Retrieves the Key Vault resource ID
      5. Grants "Key Vault Secrets User"    → Function's Managed Identity (read-only)
      6. Grants "Key Vault Secrets Officer" → Developer's Entra ID account (read+write)
      7. Prints the Settings/executionengine.settings.json content to stage on the Function
         App's already-deployed package (WOLF-8516 — Key Vault name/secret are no longer
         app settings; see Step 5 below for how to actually apply it)
      8. (Optional) Links Application Insights

.PARAMETER WhatIf
    Dry-run: prints every az command that would be executed without running it.

.EXAMPLE
    .\KeyVaultSetup.ps1
    .\KeyVaultSetup.ps1 -WhatIf

.NOTES
    Prerequisites: az login, Owner or Contributor + User Access Administrator
                   on the target subscription.
#>

[CmdletBinding(SupportsShouldProcess)]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Variables — edit before running ──────────────────────────────────────────
$ResourceGroup       = 'DEV2'
$Location            = 'southafricanorth'        # az account list-locations -o table
$VaultName           = 'WWExecutionEngine' # Must be globally unique (3-24 chars)
$FunctionAppName     = 'WarewolfServer'
$SecretName          = 'WWExecutionEngineTestSecret'
$FunctionPrincipalId = '76f13cbb-e27e-4ecf-ae53-e6f61a138831'

# Get with: az ad signed-in-user show --query id -o tsv
$DeveloperObjectId   = 'aa870f32-59da-47c4-9e7b-635ccd2f9c05'  # ashley.lewis

# Optional — set to an existing App Insights name, or leave empty to skip
$AppInsightsName     = ''

# ── Guard: az CLI present ─────────────────────────────────────────────────────
if (-not (Get-Command 'az' -ErrorAction SilentlyContinue)) {
    Write-Error 'az CLI not found in PATH. Install from https://aka.ms/installazurecliwindows'
    exit 1
}

# ── Guard: Azure login ────────────────────────────────────────────────────────
$azCtx = az account show 2>$null | ConvertFrom-Json
if (-not $azCtx) {
    Write-Error 'Not logged in to Azure. Run: az login'
    exit 1
}
Write-Host "  Subscription : $($azCtx.name)  [$($azCtx.id)]" -ForegroundColor DarkGray
Write-Host "  User         : $($azCtx.user.name)"             -ForegroundColor DarkGray

# ── Retrieve Key Vault resource ID ────────────────────────────────────────────
Write-Host "`n[4a] Retrieving Key Vault resource ID..." -ForegroundColor Cyan
$VaultResourceId = az keyvault show `
    --name           $VaultName `
    --resource-group $ResourceGroup `
    --query          id -o tsv

if (-not $VaultResourceId) {
    Write-Error "Key Vault '$VaultName' not found in resource group '$ResourceGroup'."
    exit 1
}
Write-Host "     Scope = $VaultResourceId" -ForegroundColor DarkGray

# ── Step 4: Assign roles ──────────────────────────────────────────────────────
Write-Host "`n[4/6] Assigning RBAC roles..." -ForegroundColor Cyan

# Function App → "Key Vault Secrets User" (read secrets only)
if ($PSCmdlet.ShouldProcess(
        "Principal '$FunctionPrincipalId'",
        "Assign role 'Key Vault Secrets User' on '$VaultName'")) {
    az role assignment create `
        --role     'Key Vault Secrets User' `
        --assignee $FunctionPrincipalId `
        --scope    $VaultResourceId `
        --output none
}
Write-Host "      'Key Vault Secrets User' → Function Managed Identity" -ForegroundColor Green

# Developer → "Key Vault Secrets Officer" (read + write secrets)
if ($PSCmdlet.ShouldProcess(
        "Principal '$DeveloperObjectId'",
        "Assign role 'Key Vault Secrets Officer' on '$VaultName'")) {
    az role assignment create `
        --role     'Key Vault Secrets Officer' `
        --assignee $DeveloperObjectId `
        --scope    $VaultResourceId `
        --output none
}
Write-Host "      'Key Vault Secrets Officer' → Developer ($DeveloperObjectId)" -ForegroundColor Green

# ── Step 5: Key Vault topology (WOLF-8516: no longer app settings) ────────────
# AZURE_KEYVAULT_NAME / KEYVAULT_SECRET_NAME are no longer read by the engine at all —
# HostEnvironmentConfig sources Key Vault name/secret SOLELY from the deploy-bundled
# Settings/executionengine.settings.json file (no env-var fallback). This script has no
# local package directory to stage into, so it cannot write that file itself — either:
#   (a) re-run Deploy-WwExecutionEngine.ps1 with -KeyVaultName/-KeyVaultSecretName (it
#       stages the file into the deploy package automatically), or
#   (b) hand-edit Settings/executionengine.settings.json on the already-deployed app via
#       Kudu (https://$FunctionAppName.scm.azurewebsites.net) or an FTP/WebDeploy tool.
Write-Host "`n[5/6] Key Vault topology (stage this into Settings/executionengine.settings.json)" -ForegroundColor Cyan
$engineSettingsPreview = (@{ keyVaultName = $VaultName; keyVaultSecretName = $SecretName } | ConvertTo-Json)
Write-Host $engineSettingsPreview -ForegroundColor DarkGray
Write-Host '      Not applied by this script — see the comment above Step 5 for how to apply it.' -ForegroundColor Yellow

# ── Step 6: (Optional) Link Application Insights ──────────────────────────────
if ($AppInsightsName) {
    Write-Host "`n[6/6] Linking Application Insights '$AppInsightsName'..." -ForegroundColor Cyan

    $AppInsightsConnStr = az monitor app-insights component show `
        --app            $AppInsightsName `
        --resource-group $ResourceGroup `
        --query          connectionString -o tsv

    if (-not $AppInsightsConnStr) {
        Write-Warning "Application Insights '$AppInsightsName' not found — skipping link."
    } else {
        if ($PSCmdlet.ShouldProcess(
                "Function App '$FunctionAppName'",
                'Set APPLICATIONINSIGHTS_CONNECTION_STRING app setting')) {
            az functionapp config appsettings set `
                --name           $FunctionAppName `
                --resource-group $ResourceGroup `
                --settings       "APPLICATIONINSIGHTS_CONNECTION_STRING=$AppInsightsConnStr" `
                --output none
        }
        Write-Host '      Application Insights linked.' -ForegroundColor Green
    }
} else {
    Write-Host "`n[6/6] Skipping Application Insights (AppInsightsName not set)." -ForegroundColor DarkGray
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Host ''
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor White
Write-Host ' Infrastructure setup complete.'                          -ForegroundColor Green
Write-Host " Key Vault URI   : https://$VaultName.vault.azure.net/"
Write-Host " Secret name     : $SecretName"
Write-Host " Function MI PID : $FunctionPrincipalId"
Write-Host ''
Write-Host ' NEXT STEPS:'
Write-Host '   1. On developer machine, run Encrypt-Config.ps1 to'
Write-Host '      generate the key and encrypt .bite source files.'
Write-Host '   2. Deploy the Function App with the encrypted files.'
Write-Host '   3. Verify cold-start log shows: Event=ColdStart'
Write-Host '═══════════════════════════════════════════════════════' -ForegroundColor White