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
    .\Deploy-ToAzure.ps1
    Runs interactively — prompts for app name, resource group, and region.

.EXAMPLE
    .\Deploy-ToAzure.ps1 -AppName "my-warewolf-server"
    Prompts only for resource group and region (defaults shown in brackets).

.EXAMPLE
    .\Deploy-ToAzure.ps1 -AppName "my-warewolf-server" -ResourceGroup "my-rg" -Location "westeurope"
    Fully non-interactive.
#>
param(
    [Parameter()]
    [string]$AppName,

    [Parameter()]
    [string]$ResourceGroup,

    [Parameter()]
    [string]$Location,

    [Parameter()]
    [string]$StorageAccountName
)

$ErrorActionPreference = 'Stop'

function Prompt-WithDefault {
    param([string]$Message, [string]$Default)
    if ($Default) {
        $input = Read-Host "$Message [$Default]"
        if ([string]::IsNullOrWhiteSpace($input)) { return $Default }
        return $input.Trim()
    }
    else {
        do {
            $input = Read-Host $Message
        } while ([string]::IsNullOrWhiteSpace($input))
        return $input.Trim()
    }
}

if (-not $AppName)      { $AppName       = Prompt-WithDefault "Azure Functions app name (must be globally unique)" }
if (-not $ResourceGroup){ $ResourceGroup = Prompt-WithDefault "Resource group" -Default "$AppName-rg" }
if (-not $Location) {
    $LocationSuggestions = [ordered]@{
        'westindia'        = 'Ahmedabad / West India'
        'uksouth'          = 'London / UK South'
        'northeurope'      = 'Dublin / North Europe'
        'southafricanorth' = 'Durban / South Africa North'
        'eastus'           = 'East US'
        'westeurope'       = 'Amsterdam / West Europe'
        'centralindia'     = 'Pune / Central India'
    }
    Write-Host ""
    Write-Host "Common Azure regions:"
    $i = 1
    foreach ($key in $LocationSuggestions.Keys) {
        Write-Host "  [$i] $key  ($($LocationSuggestions[$key]))"
        $i++
    }
    $LocationKeys = @($LocationSuggestions.Keys)
    Write-Host ""
    $LocationInput = Read-Host "Azure region — enter a number from the list, or type any region [eastus]"
    if ([string]::IsNullOrWhiteSpace($LocationInput)) {
        $Location = 'eastus'
    }
    elseif ($LocationInput -match '^\d+$' -and [int]$LocationInput -ge 1 -and [int]$LocationInput -le $LocationKeys.Count) {
        $Location = $LocationKeys[[int]$LocationInput - 1]
    }
    else {
        $Location = $LocationInput.Trim()
    }
    Write-Host "Using region: $Location"
}
if (-not $StorageAccountName) {
    $StorageAccountName = (($AppName -replace '[^a-z0-9]', '') + 'sa').Substring(0, [Math]::Min(24, ($AppName -replace '[^a-z0-9]', '').Length + 2))
}

$ScriptDir = $PSScriptRoot

# Inform about optional secure.config
$SecureConfigPath = Join-Path $ScriptDir 'secure.config'
if (Test-Path $SecureConfigPath) {
    Write-Host "Found secure.config — it will be included in the deployment package."
    Write-Host "  NOTE: SecureConfigLoader will read it from the function app bin directory at runtime."
    Write-Host "  Alternatively, set the WAREWOLF_SECURE_CONFIG app setting to load it from a mounted path."
} else {
    Write-Warning "No secure.config found — the function app will DENY every request with HTTP 503 until a policy is supplied."
    Write-Host "  To enforce authorization: copy your secure.config next to this script and redeploy,"
    Write-Host "  OR set the WAREWOLF_SECURE_CONFIG app setting to a mounted file path."
    Write-Host "  For open-access (dev/test ONLY): set the BYPASS_SECURE_CONFIG=true app setting."
}

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

# Exclude documentation and developer-only artifacts from the deployed package.
# These are never needed at runtime by the Azure Function. Excluding them keeps the
# upload lean and avoids shipping internal docs / local dev settings to the cloud,
# regardless of whether the script is run from the publish output or the source tree.
$ExcludeNames = @('local.settings.json', 'Skill.md')
$ExcludeDirs  = @('docs')
$PackageItems = Get-ChildItem -Path $ScriptDir -Force | Where-Object {
    $_.Name -notin $ExcludeNames -and
    $_.Name -notlike '*.md' -and
    -not ($_.PSIsContainer -and $_.Name -in $ExcludeDirs)
}
Compress-Archive -Path $PackageItems.FullName -DestinationPath $TempZipPath -Force
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
