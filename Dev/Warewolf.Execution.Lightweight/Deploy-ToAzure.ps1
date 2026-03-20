<#
.SYNOPSIS
    Deploys the Warewolf Lightweight Execution Server to an Azure Functions app.

.DESCRIPTION
    Place your .bite workflow resource files in the Resources folder next to this
    script, then run it. The script creates the Azure Functions app if it does not
    already exist, then deploys the package via zip deploy.

    Requires the Azure CLI (az) to be installed and logged in:
        az login

.PARAMETER AppName
    The name of your Azure Functions app (e.g. "my-warewolf-server").
    Must be globally unique across Azure.

.PARAMETER ResourceGroup
    The Azure resource group to create or use. Defaults to "$AppName-rg".

.PARAMETER Location
    Azure region for new resources. Defaults to "eastus".
    Only used when creating a new app.

.PARAMETER StorageAccountName
    Storage account name for the Functions app backend.
    Defaults to the first 24 characters of "$($AppName -replace '[^a-z0-9]','')sa".
    Only used when creating a new app.

.EXAMPLE
    .\Deploy-ToAzure.ps1 -AppName "my-warewolf-server"

.EXAMPLE
    .\Deploy-ToAzure.ps1 -AppName "my-warewolf-server" -ResourceGroup "my-rg" -Location "westeurope"
#>
param(
    [Parameter(Mandatory)]
    [string]$AppName,

    [Parameter()]
    [string]$ResourceGroup = "$AppName-rg",

    [Parameter()]
    [string]$Location = 'eastus',

    [Parameter()]
    [string]$StorageAccountName = (($AppName -replace '[^a-z0-9]', '') + 'sa').Substring(0, [Math]::Min(24, ($AppName -replace '[^a-z0-9]', '').Length + 2))
)

$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot

# Validate resources folder
$ResourcesPath = Join-Path $ScriptDir 'Resources'
if (-not (Test-Path $ResourcesPath)) {
    Write-Error "Resources folder not found at '$ResourcesPath'. Create it and add your .bite files before deploying."
    exit 1
}
$BiteFiles = Get-ChildItem -Path $ResourcesPath -Filter '*.bite' -Recurse
if ($BiteFiles.Count -eq 0) {
    Write-Warning "No .bite files found in '$ResourcesPath'. Deploying without workflow resources."
}
else {
    Write-Host "Found $($BiteFiles.Count) .bite file(s) in Resources."
}

# Verify az CLI is available
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed or not on PATH. Install it from https://aka.ms/installazurecliwindows"
    exit 1
}

# Verify logged in
$Account = az account show 2>$null | ConvertFrom-Json
if (-not $Account) {
    Write-Error "Not logged in to Azure CLI. Run 'az login' first."
    exit 1
}
Write-Host "Using Azure subscription: $($Account.name) ($($Account.id))"

# Create resource group if needed
$RgExists = az group exists --name $ResourceGroup | ConvertFrom-Json
if (-not $RgExists) {
    Write-Host "Creating resource group '$ResourceGroup' in '$Location'..."
    az group create --name $ResourceGroup --location $Location | Out-Null
    Write-Host "Resource group created."
}

# Create storage account if needed
$StorageExists = az storage account show --name $StorageAccountName --resource-group $ResourceGroup 2>$null
if (-not $StorageExists) {
    Write-Host "Creating storage account '$StorageAccountName'..."
    az storage account create `
        --name $StorageAccountName `
        --resource-group $ResourceGroup `
        --location $Location `
        --sku Standard_LRS | Out-Null
    Write-Host "Storage account created."
}

# Create Function App if needed
$AppExists = az functionapp show --name $AppName --resource-group $ResourceGroup 2>$null
if (-not $AppExists) {
    Write-Host "Creating Azure Functions app '$AppName'..."
    az functionapp create `
        --name $AppName `
        --resource-group $ResourceGroup `
        --storage-account $StorageAccountName `
        --consumption-plan-location $Location `
        --runtime dotnet-isolated `
        --runtime-version 8 `
        --functions-version 4 | Out-Null
    Write-Host "Azure Functions app created."
}
else {
    Write-Host "Azure Functions app '$AppName' already exists."
}

# Create temp zip
$TempZipPath = Join-Path $env:TEMP "AzureFunctionsPackage-$AppName.zip"
if (Test-Path $TempZipPath) { Remove-Item $TempZipPath -Force }
Write-Host "Creating deployment package..."
Compress-Archive -Path "$ScriptDir\*" -DestinationPath $TempZipPath -Force
Write-Host "Package created."

# Deploy
try {
    Write-Host "Deploying to '$AppName'..."
    az functionapp deployment source config-zip `
        --name $AppName `
        --resource-group $ResourceGroup `
        --src $TempZipPath
    Write-Host "Deployment complete. Your workflows are available at https://$AppName.azurewebsites.net/"
}
catch {
    Write-Error "Deployment failed: $_"
    exit 1
}
finally {
    if (Test-Path $TempZipPath) {
        Remove-Item $TempZipPath -Force
    }
}
