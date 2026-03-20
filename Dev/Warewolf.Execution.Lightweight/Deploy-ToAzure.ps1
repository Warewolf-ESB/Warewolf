<#
.SYNOPSIS
    Deploys the Warewolf Lightweight Execution Server to an Azure Functions app.

.DESCRIPTION
    Place your .bite workflow resource files in the Resources folder next to this
    script, then run it. The script zips the folder contents and deploys them to
    your Azure Functions app using the Kudu zip-deploy API.

    Deployment credentials (username and password) can be found in the Azure portal
    under your Function App > Deployment Center > FTPS credentials.

.PARAMETER AppName
    The name of your Azure Functions app (e.g. "my-warewolf-server").

.PARAMETER DeploymentPassword
    The deployment password from the Azure portal. If omitted you will be prompted.

.EXAMPLE
    .\Deploy-ToAzure.ps1 -AppName "my-warewolf-server"
#>
param(
    [Parameter(Mandatory)]
    [string]$AppName,

    [Parameter()]
    [string]$DeploymentPassword
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

# Prompt for password if not supplied
if ([string]::IsNullOrEmpty($DeploymentPassword)) {
    $SecurePassword   = Read-Host "Enter deployment password for '$AppName'" -AsSecureString
    $DeploymentPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    )
}

# Create temp zip
$TempZipPath = Join-Path $env:TEMP "AzureFunctionsPackage-$AppName.zip"
if (Test-Path $TempZipPath) { Remove-Item $TempZipPath -Force }
Write-Host "Creating deployment package..."
Compress-Archive -Path "$ScriptDir\*" -DestinationPath $TempZipPath -Force
Write-Host "Package created at '$TempZipPath'."

# Deploy
$DeployUri    = "https://$AppName.scm.azurewebsites.net/api/zipdeploy"
$Username     = "`$$AppName"
$Pair         = "${Username}:${DeploymentPassword}"
$EncodedCreds = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Pair))
$Headers      = @{ Authorization = "Basic $EncodedCreds" }

try {
    Write-Host "Deploying to $DeployUri ..."
    Invoke-RestMethod -Uri $DeployUri -Method POST -Headers $Headers -InFile $TempZipPath -ContentType 'application/zip'
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
