# Warewolf Version: 3.0.2.79  |  Stamped: 2026-09-16
<#
.SYNOPSIS
  WOLF-8512 follow-up: deploys the CommandTimeout=120 fix for usp_ScoreMandateCollectionDates,
  usp_GetMandateScoredResults (the resource backing the stale "usp_GetMandateScoresCollectionDates"
  caller label), RabbitProcess, and RequestForProfiling to wwuat3, via the engine's own
  /mcp-api/deploy_workflow route.

.DESCRIPTION
  Requires the secure.config WindowsGroup fix ("Warewolf Administrators" -> "Warewolf_Adminstrators")
  to already be deployed — without it every deploy_workflow call in -Mode Deploy fails with
  "You do not have permission to deploy workflow '...'" (Contribute denied). If you see that error,
  the secure.config fix + redeploy hasn't landed yet.

  Two-phase by design — the `name` deploy_workflow resolves against is a relative path with
  no extension, and getting it wrong doesn't fail loudly: WorkflowNameResolver just won't find
  an existing file, `overwrite` becomes irrelevant, and deploy_workflow silently CREATES a new
  file at the wrong path instead of replacing the one wwuat3 actually executes.

    -Mode List    (default) Authenticates, then calls /mcp-api/list_workflows and prints every
                  workflow whose path contains "ScoreMandateCollectionDates", "GetMandateScoredResults",
                  "RabbitProcess", or "RequestForProfiling" — read these before choosing -Mode Deploy.
    -Mode Deploy  Calls /mcp-api/deploy_workflow for each of the four workflows (REQUIRED name params
                  below — copy the exact 'path' values List printed) with overwrite:true, using the
                  already-edited local .bite files as the body.

  Auth: interactive device-code flow against wwuat3-auth (TenantId
  ca0cc53b-9af4-4067-bcdf-be9c648450d1, ClientId 5d8c2e7a-afdb-4caa-9d53-c32e440f6c41) — you sign
  in as yourself in a browser; the access token lives only in this process's memory and is never
  written to disk or echoed to output.

.PARAMETER Mode
  List (default) or Deploy. See DESCRIPTION.

.PARAMETER ScoreMandateCollectionDatesName
  REQUIRED for -Mode Deploy. Exact `name` (from -Mode List output) for the workflow backing
  usp_ScoreMandateCollectionDates.

.PARAMETER GetMandateScoredResultsName
  REQUIRED for -Mode Deploy. Exact `name` (from -Mode List output) for the workflow backing
  usp_GetMandateScoredResults (the caller-side label "usp_GetMandateScoresCollectionDates" is
  stale — see docs/ShovelBridge-Architecture.md and this session's investigation).

.PARAMETER RabbitProcessName
  REQUIRED for -Mode Deploy. Exact `name` (from -Mode List output) for RabbitProcess.

.PARAMETER RequestForProfilingName
  REQUIRED for -Mode Deploy. Exact `name` (from -Mode List output) for RequestForProfiling
  (the Service Bus trigger's own entry workflow — matches -ServiceBusTriggerWorkflowName in
  Deploy-WwExecutionEngine.ps1, "ProfilerServices/ProfilerWrapper/RequestForProfiling").

.PARAMETER FunctionAppUrl
  Base URL of wwuat3. Default: https://wwuat3.azurewebsites.net

.EXAMPLE
  # Step 1 — find the exact names
  ./Deploy-WwProfilerCommandTimeoutFix.ps1 -Mode List

.EXAMPLE
  # Step 2 — deploy, using the names List printed
  ./Deploy-WwProfilerCommandTimeoutFix.ps1 -Mode Deploy `
      -ScoreMandateCollectionDatesName "ProfilerServices/Profiler/V2/Private/DAL/usp_ScoreMandateCollectionDates" `
      -GetMandateScoredResultsName "ProfilerServices/Profiler/V2/Private/DAL/usp_GetMandateScoredResults" `
      -RabbitProcessName "rabbit/RabbitProcess" `
      -RequestForProfilingName "ProfilerServices/ProfilerWrapper/RequestForProfiling"
#>

[CmdletBinding()]
param(
    [ValidateSet('List', 'Deploy')]
    [string] $Mode = 'List',

    [string] $ScoreMandateCollectionDatesName,
    [string] $GetMandateScoredResultsName,
    [string] $RabbitProcessName,
    [string] $RequestForProfilingName,

    [string] $FunctionAppUrl = 'https://wwuat3.azurewebsites.net',

    [string] $TenantId = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1',
    [string] $ResourceAppId = '5d8c2e7a-afdb-4caa-9d53-c32e440f6c41',
    [string] $ClientId = '5d8c2e7a-afdb-4caa-9d53-c32e440f6c41'
)

$ErrorActionPreference = 'Stop'

if ($Mode -eq 'Deploy') {
    foreach ($pair in @(
        @{ Name = 'ScoreMandateCollectionDatesName'; Value = $ScoreMandateCollectionDatesName },
        @{ Name = 'GetMandateScoredResultsName';     Value = $GetMandateScoredResultsName },
        @{ Name = 'RabbitProcessName';               Value = $RabbitProcessName },
        @{ Name = 'RequestForProfilingName';         Value = $RequestForProfilingName }
    )) {
        if (-not $pair.Value) {
            throw "-$($pair.Name) is required for -Mode Deploy. Run -Mode List first to find it."
        }
    }
}

$ScoreMandateCollectionDatesPath = Join-Path $PSScriptRoot '..\Resources\ProfilerServices\Profiler\V2\Private\DAL\usp_ScoreMandateCollectionDates.bite'
$GetMandateScoredResultsPath     = Join-Path $PSScriptRoot '..\Resources\ProfilerServices\Profiler\V2\Private\DAL\usp_GetMandateScoredResults.bite'
$RabbitProcessPath               = Join-Path $PSScriptRoot '..\Resources\rabbit\RabbitProcess.bite'
$RequestForProfilingPath         = Join-Path $PSScriptRoot '..\Resources\ProfilerServices\ProfilerWrapper\RequestForProfiling.bite'

foreach ($p in @($ScoreMandateCollectionDatesPath, $GetMandateScoredResultsPath, $RabbitProcessPath, $RequestForProfilingPath)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Expected edited .bite file not found: $p" }
}

# ══════════════════════════════════════════════════════════════════════════════
# 1. Device-code auth — interactive, token stays in-memory only
# ══════════════════════════════════════════════════════════════════════════════

$TokenEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
$DeviceCodeEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/devicecode"
# AADSTS90009: when ClientId and ResourceAppId are the SAME app (true here — wwuat3-auth is
# registered as both client and resource), the device-code endpoint rejects the "api://<guid>/.default"
# URI-form scope ("requesting a token for itself... supported only if resource is specified using
# the GUID based App Identifier") and requires the bare GUID form instead.
$Scope = "$ResourceAppId/.default"

Write-Host ""
Write-Host "=== Signing in to wwuat3-auth ===" -ForegroundColor Cyan
Write-Host "  FunctionAppUrl: $FunctionAppUrl" -ForegroundColor DarkGray

$dcBody = @{ client_id = $ClientId; scope = $Scope }
$dcResponse = Invoke-RestMethod -Method POST -Uri $DeviceCodeEndpoint `
    -ContentType 'application/x-www-form-urlencoded' -Body $dcBody

Write-Host ""
Write-Host "  ACTION REQUIRED:" -ForegroundColor Yellow
Write-Host "    1. Open:        $($dcResponse.verification_uri)" -ForegroundColor Yellow
Write-Host "    2. Enter code:  $($dcResponse.user_code)" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Polling for authorization..." -ForegroundColor DarkGray

$pollBody = @{
    grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
    client_id   = $ClientId
    device_code = $dcResponse.device_code
}
$interval = if ($dcResponse.interval) { $dcResponse.interval } else { 5 }
$accessToken = $null

for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds $interval
    try {
        $tokenResponse = Invoke-RestMethod -Method POST -Uri $TokenEndpoint `
            -ContentType 'application/x-www-form-urlencoded' -Body $pollBody
        $accessToken = $tokenResponse.access_token
        break
    } catch {
        $err = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($err.error -eq 'authorization_pending') { continue }
        if ($err.error -eq 'slow_down') { $interval += 5; continue }
        throw
    }
}
if (-not $accessToken) { throw "Device code flow timed out." }

# Decode just enough to show WHO signed in — never print the token itself.
$claimsPart = $accessToken.Split('.')[1].Replace('-', '+').Replace('_', '/')
$pad = 4 - ($claimsPart.Length % 4); if ($pad -ne 4) { $claimsPart += ('=' * $pad) }
$claims = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($claimsPart)) | ConvertFrom-Json
Write-Host "  Signed in as: $($claims.preferred_username ?? $claims.upn ?? $claims.oid)" -ForegroundColor Green
Write-Host "  Roles:        $($claims.roles -join ', ')" -ForegroundColor Green

$headers = @{ Authorization = "Bearer $accessToken"; 'Content-Type' = 'application/json' }

function Invoke-McpApi {
    param([string] $Tool, [hashtable] $Body)
    $uri = "$FunctionAppUrl/mcp-api/$Tool"
    $json = $Body | ConvertTo-Json -Depth 10 -Compress
    return Invoke-RestMethod -Method POST -Uri $uri -Headers $headers -Body $json
}

# ══════════════════════════════════════════════════════════════════════════════
# 2. -Mode List — find the exact resolved names before touching anything
# ══════════════════════════════════════════════════════════════════════════════

if ($Mode -eq 'List') {
    Write-Host ""
    Write-Host "=== Listing workflows (paginating, filtering client-side) ===" -ForegroundColor Cyan

    $needles = @('ScoreMandateCollectionDates', 'GetMandateScoredResults', 'RabbitProcess', 'RequestForProfiling')
    $matches = @()
    $cursor = $null

    do {
        $body = @{}
        if ($cursor) { $body.cursor = $cursor }
        $page = Invoke-McpApi -Tool 'list_workflows' -Body $body
        foreach ($wf in $page.workflows) {
            foreach ($needle in $needles) {
                if ($wf.path -like "*$needle*") { $matches += $wf }
            }
        }
        $cursor = $page.nextCursor
    } while ($cursor)

    Write-Host ""
    if ($matches.Count -eq 0) {
        Write-Host "  No matches found for: $($needles -join ', ')" -ForegroundColor Red
    } else {
        Write-Host "  Matches:" -ForegroundColor Green
        $matches | ForEach-Object { Write-Host "    path: $($_.path)   (display name: $($_.name))" -ForegroundColor White }
    }
    Write-Host ""
    Write-Host "  Re-run with -Mode Deploy -ScoreMandateCollectionDatesName <path> -GetMandateScoredResultsName <path>" -ForegroundColor DarkGray
    Write-Host "                           -RabbitProcessName <path> -RequestForProfilingName <path>" -ForegroundColor DarkGray
    Write-Host "  using the exact 'path' values above (NOT the display name — deploy_workflow resolves by path)." -ForegroundColor DarkGray
    return
}

# ══════════════════════════════════════════════════════════════════════════════
# 3. -Mode Deploy — get_workflow_definition sanity check, then deploy_workflow
# ══════════════════════════════════════════════════════════════════════════════

function Deploy-One {
    param([string] $Name, [string] $LocalPath)

    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan

    # Sanity check: confirm this name currently resolves to a workflow before overwriting it
    # (deploy_workflow requires overwrite:true to replace an existing one anyway, but this
    # surfaces a wrong -Name early with a clear message instead of a generic 400).
    try {
        $existing = Invoke-McpApi -Tool 'get_workflow_definition' -Body @{ name = $Name }
        Write-Host "  Found existing workflow at '$Name' (confirms overwrite target)." -ForegroundColor DarkGray
    } catch {
        Write-Host "  WARNING: could not resolve an existing workflow at '$Name'." -ForegroundColor Yellow
        Write-Host "  deploy_workflow would CREATE a new file here rather than replace the live one." -ForegroundColor Yellow
        $confirm = Read-Host "  Type YES to proceed anyway, anything else to abort"
        if ($confirm -ne 'YES') { throw "Aborted: '$Name' did not resolve to an existing workflow." }
    }

    $biteContent = Get-Content -LiteralPath $LocalPath -Raw

    $result = Invoke-McpApi -Tool 'deploy_workflow' -Body @{
        name        = $Name
        biteContent = $biteContent
        overwrite   = $true
    }

    Write-Host "  deployed:    $($result.deployed)" -ForegroundColor Green
    Write-Host "  overwritten: $($result.overwritten)" -ForegroundColor Green
}

Deploy-One -Name $ScoreMandateCollectionDatesName -LocalPath $ScoreMandateCollectionDatesPath
Deploy-One -Name $GetMandateScoredResultsName -LocalPath $GetMandateScoredResultsPath
Deploy-One -Name $RabbitProcessName -LocalPath $RabbitProcessPath
Deploy-One -Name $RequestForProfilingName -LocalPath $RequestForProfilingPath

Write-Host ""
Write-Host "=== Done. CommandTimeout=120 is live on wwuat3 for all four workflows. ===" -ForegroundColor Cyan
Write-Host "No host restart needed — deploy_workflow evicts the cached compiled workflow itself." -ForegroundColor DarkGray
Write-Host ""
