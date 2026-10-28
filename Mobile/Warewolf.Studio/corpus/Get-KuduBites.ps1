<#
.SYNOPSIS
    Downloads every .bite file from a Warewolf engine's Kudu VFS, byte for byte, for the System 1 corpus.

.DESCRIPTION
    Walks RemoteRoot recursively over the Function App's Kudu VFS API (read-only) and saves each .bite
    under Destination with the same relative path. Authenticates with an Azure Resource Manager token from
    the current `az` login, so no publishing credentials are fetched or stored. Workflows and sources are
    both downloaded; the corpus index separates them by ResourceType.

.EXAMPLE
    ./corpus/Get-KuduBites.ps1 -SubscriptionId ba4929ac-0d6c-49cf-823e-cdaacb7e0d1b -AppName ashleyswarewolfserver -Destination C:\Users\ultra\wwc\asws
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $SubscriptionId,
    [Parameter(Mandatory)] [string] $AppName,
    [Parameter(Mandatory)] [string] $Destination,
    # The Lightweight engine keeps saved resources under %HOME%\data\Warewolf\Resources so they survive deploys.
    [string] $RemoteRoot = '/data/Warewolf/Resources/'
)

$ErrorActionPreference = 'Stop'

$token = az account get-access-token --subscription $SubscriptionId --resource https://management.azure.com/ --query accessToken -o tsv
if ($LASTEXITCODE -ne 0 -or -not $token) { throw "Could not get an ARM token for subscription $SubscriptionId; run 'az login' first." }
$headers = @{ Authorization = "Bearer $token" }
$vfs = "https://$AppName.scm.azurewebsites.net/api/vfs"

function Get-Bites([string] $remoteDir) {
    # Assign first: iterating Invoke-RestMethod directly in a foreach statement yields the whole JSON array as one item.
    $entries = Invoke-RestMethod -Headers $headers -Uri "$vfs$remoteDir"
    foreach ($entry in $entries) {
        $remotePath = "$remoteDir$($entry.name)"
        if ($entry.mime -eq 'inode/directory') {
            Get-Bites "$remotePath/"
        }
        elseif ($entry.name -like '*.bite') {
            $relative = $remotePath.Substring($RemoteRoot.Length)
            $local = Join-Path $Destination ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)
            New-Item -ItemType Directory -Force (Split-Path $local) | Out-Null
            # -OutFile writes the response bytes untouched: CRLF, emoji and entity escaping survive as stored.
            Invoke-WebRequest -Headers $headers -Uri ($vfs + [Uri]::EscapeUriString($remotePath)) -OutFile $local
            [pscustomobject]@{ Path = $relative; Bytes = (Get-Item $local).Length; RemoteBytes = $entry.size }
        }
    }
}

$files = @(Get-Bites $RemoteRoot)
$mismatched = @($files | Where-Object { $_.Bytes -ne $_.RemoteBytes })
"Downloaded $($files.Count) .bite files from $AppName to $Destination"
if ($mismatched) {
    $mismatched | Format-Table -AutoSize | Out-String | Write-Warning
    throw "$($mismatched.Count) file(s) differ in size from the VFS listing."
}
