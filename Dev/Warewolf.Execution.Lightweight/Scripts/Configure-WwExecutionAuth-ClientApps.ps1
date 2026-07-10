# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-30
#Requires -Version 7.0
<#
.SYNOPSIS
  Orchestrates Entra ID client app registrations for EVERY client-example app
  shipped under Warewolf.Execution.Lightweight.ClientExamples, mapping each
  example to its correct registration type and driving the existing
  Configure-WwExecutionAuth-Clients.ps1 once per app.

.DESCRIPTION
  This is a THIN orchestrator. It does not re-implement any Entra / Graph logic:
  it dot-sources Configure-WwExecutionAuth-Clients.ps1 -LoadFunctionsOnly to reuse
  its output / prompt / az helpers, then calls that script (NonInteractive) once
  per example app with the app-appropriate ClientType, display-name prefix and
  redirect URIs.

  Example app  -> registration                       -> OAuth flow
  ───────────────────────────────────────────────────────────────────────────────
  Angular17    -> SPA          (wwexecution-angular)  -> Auth Code + PKCE (delegated)
  React        -> SPA          (wwexecution-react)    -> Auth Code + PKCE (delegated)
  DotNetWebMvc -> Confidential (wwexecution-webmvc)   -> Auth Code (confidential)+OBO
  DotNetConsole-> Console      (wwexecution-console)  -> device-code/interactive + CC
  AzureFunction-> Daemon       (wwexecution-azurefunction) -> Managed Identity / CC
  AzureServiceBus-> Daemon     (wwexecution-servicebus)    -> Managed Identity / CC

  Stage 1  Interactive cfg  - prompt / validate inputs, pick apps, confirm plan
  Stage 2  Provision        - call Configure-WwExecutionAuth-Clients.ps1 per app
  Stage 3  Validate         - acquire a token + call /secure/{workflow}.json
                              (app-only flows run automatically; delegated flows
                               are an interactive device-code opt-in)
  Stage 4  Summary          - per-app transcript log + masked summary JSON

  App-only callers (Daemon / Managed Identity, and the client-credentials flow of
  the Console registration) are authorised purely by their app-role assignments.
  -AppRolesToAssign therefore has NO default and the Daemon path fails loudly when
  no role resolves (enforced by Configure-WwExecutionAuth-Clients.ps1).

.PARAMETER ResourceAppId
  Application (client) ID of the wwexecution resource app (from Configure-WwExecutionAuth.ps1).

.PARAMETER TenantId
  Microsoft Entra ID tenant GUID.

.PARAMETER Apps
  Which example apps to provision: angular, react, webmvc, console, azurefunction,
  servicebus, or All. Default: All.

.PARAMETER AppRolesToAssign
  App-role values to assign to app-only clients (console client-credentials, the two
  daemon workers). MUST match app-role values that exist on the resource app (created
  from the GroupPermissions keys in Deploy-WwExecutionEngine.authconfig.json — the
  example ships a dedicated 'Warewolf_ClientApps' group for these callers). No default.
  The engine only authorizes workflows whose secure.config has a matching
  WindowsGroupPermissions row (WindowsGroup = the app-role value, Execute=true).

.PARAMETER SecretLifetimeYears
  Secret validity in years (1-2). Default: 1.

.PARAMETER FunctionAppName
  Name of the Azure Function App (engine). Used for SPA CORS and for validation HTTP calls.

.PARAMETER FunctionAppResourceGroup
  Resource group of the Function App (required for SPA CORS setup).

.PARAMETER AzureFunctionMiObjectId
  Object (principal) ID of an EXISTING managed identity for the AzureFunction worker.
  When supplied, that app is provisioned as a Managed-Identity role assignment (no secret).

.PARAMETER AzureFunctionClientAppName
  Name of the CLIENT Azure Function App for the AzureFunction worker. When supplied
  (and -AzureFunctionMiObjectId is not), the child script enables that app's
  system-assigned managed identity, reads its principalId and assigns the role -
  automating the manual "enable MI + look up principalId" step. Requires
  -AzureFunctionClientResourceGroup.

.PARAMETER AzureFunctionClientResourceGroup
  Resource group of the client Function App named by -AzureFunctionClientAppName.

.PARAMETER ServiceBusMiObjectId
  As above, for the AzureServiceBus worker.

.PARAMETER WorkflowName
  Workflow exercised by validation. Default: "Hello World".

.PARAMETER Validate
  Run Stage 3 validation without prompting (NonInteractive). Interactive runs are
  always prompted regardless of this switch.

.PARAMETER SkipCorsConfiguration
  Skip Function App CORS / Allowed Origins configuration for SPA clients.

.PARAMETER LogDir
  Directory for the transcript log. Default: <Scripts>/logs.

.PARAMETER DryRun
  Print the resolved plan and exit without creating / changing anything.

.PARAMETER NonInteractive
  Skip all prompts; fail on missing required values.

.PARAMETER LoadFunctionsOnly
  Dot-source helper functions only (no prompts, no cloud/Graph calls). Used by the
  Pester test suite to unit-test helpers in isolation.

.EXAMPLE
  ./Configure-WwExecutionAuth-ClientApps.ps1

.EXAMPLE
  # Provision every example app, non-interactively, then validate app-only flows
  ./Configure-WwExecutionAuth-ClientApps.ps1 -ResourceAppId "..." -TenantId "..." `
      -FunctionAppName "wwexecution" -FunctionAppResourceGroup "DEV2" `
      -AppRolesToAssign "Warewolf_ClientApps" -Validate -NonInteractive

.EXAMPLE
  # Only the two SPA examples
  ./Configure-WwExecutionAuth-ClientApps.ps1 -ResourceAppId "..." -TenantId "..." `
      -Apps angular,react -FunctionAppName "wwexecution" -FunctionAppResourceGroup "DEV2" -NonInteractive

.EXAMPLE
  # Daemon workers bound to existing managed identities (no secrets).
  # 'Warewolf_ClientApps' must exist as a GroupPermissions key on the resource app
  # (see Deploy-WwExecutionEngine.authconfig.example.json) and have a matching
  # secure.config WindowsGroupPermissions row for the workflows the clients call.
  ./Configure-WwExecutionAuth-ClientApps.ps1 -ResourceAppId "..." -TenantId "..." `
      -Apps azurefunction,servicebus -AppRolesToAssign "Warewolf_ClientApps" `
      -AzureFunctionMiObjectId "<mi-sp-object-id>" -ServiceBusMiObjectId "<mi-sp-object-id>" -NonInteractive

.EXAMPLE
  # AzureFunction worker — enable the client Function App's system-assigned MI and
  # assign the default 'Warewolf_ClientApps' role automatically (no object id needed).
  ./Configure-WwExecutionAuth-ClientApps.ps1 -ResourceAppId "..." -TenantId "..." `
      -Apps azurefunction -AzureFunctionClientAppName "my-caller-func" `
      -AzureFunctionClientResourceGroup "myRG" -NonInteractive
#>

[CmdletBinding()]
param(
    [string]   $ResourceAppId,
    [string]   $TenantId,

    [ValidateSet('angular', 'react', 'webmvc', 'console', 'azurefunction', 'servicebus', 'All')]
    [string[]] $Apps = @('All'),

    [string[]] $AppRolesToAssign = @(),

    [ValidateRange(1, 2)]
    [int]      $SecretLifetimeYears = 1,

    [string]   $FunctionAppName,
    [string]   $FunctionAppResourceGroup,

    [string]   $AzureFunctionMiObjectId,
    [string]   $AzureFunctionClientAppName,
    [string]   $AzureFunctionClientResourceGroup,
    [string]   $ServiceBusMiObjectId,

    [string]   $WorkflowName = 'Hello World',

    [switch]   $Validate,
    [switch]   $SkipCorsConfiguration,

    [string]   $LogDir,

    [switch]   $DryRun,
    [switch]   $NonInteractive,

    # Dot-source the helper functions only (no prompts, no cloud/Graph calls).
    [switch]   $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ClientScript = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-Clients.ps1'

#region Pure (testable) helpers

function Get-ClientAppDefinitions {
    <#
    .SYNOPSIS
      Returns the static mapping of every client-example app to its Entra
      registration type, display-name prefix and redirect URIs.

    .DESCRIPTION
      Pure function of no inputs - the single source of truth for the per-app
      provisioning plan. The Pester suite asserts this table's shape (6 apps,
      correct ClientType / Prefix / RedirectUris, role requirements).

      RequiresRole = the app's PRIMARY (app-only) flow is rejected by the engine
      without a role. AppOnly = the app's primary validated flow is client-credentials
      (or managed identity). Cors = the app is a browser SPA needing Function App CORS.
    #>
    return @(
        [pscustomobject]@{
            Key          = 'angular'
            Folder       = 'Angular17'
            DisplayName  = 'Angular 17 SPA'
            ClientType   = 'SPA'
            Prefix       = 'wwexecution-angular'
            RedirectUris = @('http://localhost:4201')
            RequiresRole = $false
            AppOnly      = $false
            Cors         = $true
            ConfigFile   = 'src/environments/environment.ts'
        }
        [pscustomobject]@{
            Key          = 'react'
            Folder       = 'React'
            DisplayName  = 'React SPA'
            ClientType   = 'SPA'
            Prefix       = 'wwexecution-react'
            RedirectUris = @('http://localhost:5173')
            RequiresRole = $false
            AppOnly      = $false
            Cors         = $true
            ConfigFile   = '.env.local'
        }
        [pscustomobject]@{
            Key          = 'webmvc'
            Folder       = 'DotNetWebMvc'
            DisplayName  = '.NET 8 Web App (MVC)'
            ClientType   = 'Confidential'
            Prefix       = 'wwexecution-webmvc'
            RedirectUris = @('https://localhost:5001/signin-oidc')
            RequiresRole = $false
            AppOnly      = $false
            Cors         = $false
            ConfigFile   = 'appsettings.json'
        }
        [pscustomobject]@{
            Key          = 'console'
            Folder       = 'DotNetConsole'
            DisplayName  = '.NET 8 Console'
            ClientType   = 'Console'
            Prefix       = 'wwexecution-console'
            RedirectUris = @('http://localhost')
            RequiresRole = $true
            AppOnly      = $true
            Cors         = $false
            ConfigFile   = 'appsettings.json'
        }
        [pscustomobject]@{
            Key          = 'azurefunction'
            Folder       = 'AzureFunction'
            DisplayName  = 'Azure Function caller'
            ClientType   = 'Daemon'
            Prefix       = 'wwexecution-azurefunction'
            RedirectUris = @()
            RequiresRole = $true
            AppOnly      = $true
            Cors         = $false
            ConfigFile   = 'local.settings.json'
        }
        [pscustomobject]@{
            Key          = 'servicebus'
            Folder       = 'AzureServiceBus'
            DisplayName  = 'Azure Service Bus worker'
            ClientType   = 'Daemon'
            Prefix       = 'wwexecution-servicebus'
            RedirectUris = @()
            RequiresRole = $true
            AppOnly      = $true
            Cors         = $false
            ConfigFile   = 'local.settings.json'
        }
    )
}

function Resolve-SelectedApps {
    <#
    .SYNOPSIS
      Expands the -Apps selection (with 'All') into the matching app definitions,
      preserving the canonical order from Get-ClientAppDefinitions.
    #>
    param([string[]] $Selection)

    $defs = Get-ClientAppDefinitions
    if ($null -eq $Selection -or @($Selection).Count -eq 0 -or ($Selection -contains 'All')) {
        return $defs
    }
    return @($defs | Where-Object { $Selection -contains $_.Key })
}

function Get-ValidationPlan {
    <#
    .SYNOPSIS
      Pure decision: given a registration type and whether a usable secret /
      managed identity is available, returns how that app should be validated.

    .DESCRIPTION
      Flow        - the token grant used to validate ('client-credentials',
                    'device-code', 'managed-identity', 'manual', 'none').
      Auto        - $true when validation runs without prompting (app-only flows).
      Interactive - $true when an interactive (device-code) opt-in is offered.
      Note        - operator guidance for the cases that cannot be auto-validated.
    #>
    param(
        [Parameter(Mandatory)][string] $ClientType,
        [bool] $HasSecret,
        [bool] $UseManagedIdentity
    )

    switch ($ClientType) {
        'Daemon' {
            if ($UseManagedIdentity) {
                return [pscustomobject]@{ Flow = 'managed-identity'; Auto = $false; Interactive = $false
                    Note = 'No local secret (managed identity). Validate from inside Azure where the MI is available.' }
            }
            if ($HasSecret) {
                return [pscustomobject]@{ Flow = 'client-credentials'; Auto = $true; Interactive = $false; Note = '' }
            }
            return [pscustomobject]@{ Flow = 'none'; Auto = $false; Interactive = $false
                Note = 'No client secret available - cannot acquire an app-only token locally.' }
        }
        'Console' {
            if ($HasSecret) {
                return [pscustomobject]@{ Flow = 'client-credentials'; Auto = $true; Interactive = $true
                    Note = 'Client-credentials validated automatically; device-code (delegated) offered as an interactive opt-in.' }
            }
            return [pscustomobject]@{ Flow = 'device-code'; Auto = $false; Interactive = $true
                Note = 'No secret - only the delegated device-code flow can be validated (interactive opt-in).' }
        }
        'SPA' {
            return [pscustomobject]@{ Flow = 'device-code'; Auto = $false; Interactive = $true
                Note = 'Delegated public client - validated interactively via device-code (opt-in).' }
        }
        'Confidential' {
            return [pscustomobject]@{ Flow = 'manual'; Auto = $false; Interactive = $false
                Note = 'Confidential client - device-code is not supported. Validate by signing in through the web app in a browser.' }
        }
        default {
            return [pscustomobject]@{ Flow = 'none'; Auto = $false; Interactive = $false; Note = "Unknown client type '$ClientType'." }
        }
    }
}

function Format-ClientAppSummary {
    <#
    .SYNOPSIS
      Builds the aggregate summary object from per-app result records, masking any
      client secret so the summary JSON is safe to keep / commit.

    .DESCRIPTION
      Pure function of its inputs (the Pester suite drives it with synthetic
      records). Secrets are replaced with '***REDACTED***'; the full secret remains
      only in the per-app output JSON written by Configure-WwExecutionAuth-Clients.ps1.
    #>
    param(
        [object[]] $Results,
        [string]   $TenantId,
        [string]   $ResourceAppId,
        [string]   $FunctionAppName,
        [string]   $Timestamp
    )

    $clients = foreach ($r in @($Results)) {
        $hasSecret = -not [string]::IsNullOrWhiteSpace([string]$r.ClientSecret)
        [pscustomobject]@{
            App            = $r.App
            DisplayName    = $r.DisplayName
            ClientType     = $r.ClientType
            ClientId       = $r.ClientId
            GrantType      = $r.GrantType
            RedirectUris   = $r.RedirectUris
            RolesAssigned  = $r.RolesAssigned
            SecretExpiry   = $r.SecretExpiry
            HasSecret      = $hasSecret
            ClientSecret   = if ($hasSecret) { '***REDACTED***' } else { $null }
            OutputFile     = $r.OutputFile
            Status         = $r.Status
            Validation     = $r.Validation
        }
    }

    return [pscustomobject]@{
        Timestamp       = $Timestamp
        TenantId        = $TenantId
        ResourceAppId   = $ResourceAppId
        Scope           = "api://$ResourceAppId/.default"
        Authority       = "https://login.microsoftonline.com/$TenantId"
        FunctionAppName = $FunctionAppName
        FunctionAppUrl  = "https://$FunctionAppName.azurewebsites.net"
        Clients         = @($clients)
    }
}

#endregion Pure helpers

# Reuse the child script's output / prompt / az helpers instead of duplicating
# them. Dot-sourcing runs the child's param block in this scope, which resets the
# parameters we share by name to the child's defaults - so capture our bound values
# first and restore them immediately afterwards. The switches matter most:
# without restoring LoadFunctionsOnly the child's -LoadFunctionsOnly binding
# leaks into this scope and the orchestrator silently exits at its own
# LoadFunctionsOnly check below; NonInteractive / DryRun / SkipCorsConfiguration
# would likewise be reset to $false.
$__bound = @{}
foreach ($k in 'ResourceAppId', 'TenantId', 'SecretLifetimeYears', 'AppRolesToAssign', 'FunctionAppName', 'FunctionAppResourceGroup',
               'SkipCorsConfiguration', 'DryRun', 'NonInteractive', 'LoadFunctionsOnly') {
    $__bound[$k] = Get-Variable -Name $k -ValueOnly -ErrorAction SilentlyContinue
}
if (-not (Test-Path $ClientScript)) {
    throw "Cannot find Configure-WwExecutionAuth-Clients.ps1 next to this script ('$ClientScript')."
}
. $ClientScript -LoadFunctionsOnly
foreach ($k in $__bound.Keys) { Set-Variable -Name $k -Value $__bound[$k] }

# When dot-sourced by the test suite we only need the functions defined above.
if ($LoadFunctionsOnly) { return }

#region Validation helpers (real run only)

function Invoke-SecureCall {
    <#
    .SYNOPSIS
      Calls GET /secure/{workflow}.json with a Bearer token and returns the
      HTTP status without throwing on error responses.
    #>
    param(
        [Parameter(Mandatory)][string] $FunctionAppUrl,
        [Parameter(Mandatory)][string] $Workflow,
        [Parameter(Mandatory)][string] $Token
    )
    $encoded = [Uri]::EscapeDataString($Workflow)
    $url = "$FunctionAppUrl/secure/$encoded.json?Name=Validation"
    Write-Step "GET $url"
    $resp = Invoke-WebRequest -Uri $url -Headers @{ Authorization = "Bearer $Token" } `
        -SkipHttpErrorCheck -MaximumRedirection 0 -ErrorAction Stop
    return [int]$resp.StatusCode
}

function Test-SecureStatus {
    # 2xx = pass. 401 = bad audience/token. 403/500 = engine authz denial (WOLF-8418
    # wraps role/permission denials as 500). Anything else = fail with the raw code.
    param([int] $StatusCode)
    if ($StatusCode -ge 200 -and $StatusCode -lt 300) {
        return [pscustomobject]@{ Result = 'pass'; Detail = "HTTP $StatusCode" }
    }
    $hint = switch ($StatusCode) {
        401 { 'token audience/scope mismatch (aud must be api://<ResourceAppId>)' }
        403 { 'caller role/permission not authorised for this workflow' }
        500 { 'engine authorization denial (roleless caller, or no View/Execute) - denials are wrapped as 500 (WOLF-8418)' }
        default { "unexpected status $StatusCode" }
    }
    return [pscustomobject]@{ Result = 'fail'; Detail = "HTTP $StatusCode - $hint" }
}

function Invoke-AppOnlyValidation {
    param(
        [Parameter(Mandatory)][string] $TenantId,
        [Parameter(Mandatory)][string] $ResourceAppId,
        [Parameter(Mandatory)][string] $FunctionAppUrl,
        [Parameter(Mandatory)][string] $Workflow,
        [Parameter(Mandatory)][string] $ClientId,
        [Parameter(Mandatory)][string] $ClientSecret
    )
    try {
        Write-Step "Acquiring client-credentials token for $ClientId"
        $tokenResp = Invoke-RestMethod -Method Post `
            -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{
                grant_type    = 'client_credentials'
                client_id     = $ClientId
                client_secret = $ClientSecret
                scope         = "api://$ResourceAppId/.default"
            }
        $status = Invoke-SecureCall -FunctionAppUrl $FunctionAppUrl -Workflow $Workflow -Token $tokenResp.access_token
        $verdict = Test-SecureStatus -StatusCode $status
        return [pscustomobject]@{ Flow = 'client-credentials'; HttpStatus = $status; Result = $verdict.Result; Detail = $verdict.Detail }
    } catch {
        return [pscustomobject]@{ Flow = 'client-credentials'; HttpStatus = $null; Result = 'fail'; Detail = $_.Exception.Message }
    }
}

function Invoke-DeviceCodeValidation {
    param(
        [Parameter(Mandatory)][string] $TenantId,
        [Parameter(Mandatory)][string] $ResourceAppId,
        [Parameter(Mandatory)][string] $FunctionAppUrl,
        [Parameter(Mandatory)][string] $Workflow,
        [Parameter(Mandatory)][string] $ClientId
    )
    try {
        $scope = "api://$ResourceAppId/user_impersonation"
        Write-Step "Starting device-code flow for $ClientId"
        $device = Invoke-RestMethod -Method Post `
            -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/devicecode" `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{ client_id = $ClientId; scope = $scope }
        Write-Host ""
        Write-Host "    $($device.message)" -ForegroundColor Yellow
        Write-Host ""

        $token = $null
        $deadline = (Get-Date).AddSeconds([int]$device.expires_in)
        do {
            Start-Sleep -Seconds ([int]$device.interval)
            try {
                $tr = Invoke-RestMethod -Method Post `
                    -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
                    -ContentType 'application/x-www-form-urlencoded' `
                    -Body @{
                        grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                        client_id   = $ClientId
                        device_code = $device.device_code
                    }
                $token = $tr.access_token
            } catch {
                $errCode = ''
                try { $errCode = ($_.ErrorDetails.Message | ConvertFrom-Json).error } catch {}
                if ($errCode -eq 'authorization_pending') { continue }
                if ($errCode -eq 'slow_down') { Start-Sleep -Seconds 5; continue }
                throw "device-code token failed: $errCode"
            }
        } until ($token -or (Get-Date) -gt $deadline)

        if (-not $token) { throw 'device-code timed out before sign-in completed' }

        $status = Invoke-SecureCall -FunctionAppUrl $FunctionAppUrl -Workflow $Workflow -Token $token
        $verdict = Test-SecureStatus -StatusCode $status
        return [pscustomobject]@{ Flow = 'device-code'; HttpStatus = $status; Result = $verdict.Result; Detail = $verdict.Detail }
    } catch {
        return [pscustomobject]@{ Flow = 'device-code'; HttpStatus = $null; Result = 'fail'; Detail = $_.Exception.Message }
    }
}

#endregion Validation helpers

# ──────────────────────────────────────────────────────────────────────────────
# Defaults / placeholder sentinels
# ──────────────────────────────────────────────────────────────────────────────

if (-not $PSBoundParameters.ContainsKey('ResourceAppId'))            { $ResourceAppId            = '<resourceAppId>' }
if (-not $PSBoundParameters.ContainsKey('TenantId'))                 { $TenantId                 = '<tenantId>' }
if (-not $PSBoundParameters.ContainsKey('FunctionAppName'))          { $FunctionAppName          = '<functionAppName>' }
if (-not $PSBoundParameters.ContainsKey('FunctionAppResourceGroup')) { $FunctionAppResourceGroup = '<functionAppResourceGroup>' }
if (-not $PSBoundParameters.ContainsKey('AzureFunctionMiObjectId'))  { $AzureFunctionMiObjectId  = '' }
if (-not $PSBoundParameters.ContainsKey('AzureFunctionClientAppName'))       { $AzureFunctionClientAppName       = '' }
if (-not $PSBoundParameters.ContainsKey('AzureFunctionClientResourceGroup')) { $AzureFunctionClientResourceGroup = '' }
if (-not $PSBoundParameters.ContainsKey('ServiceBusMiObjectId'))     { $ServiceBusMiObjectId     = '' }

# ──────────────────────────────────────────────────────────────────────────────
# Stage 1 — Interactive configuration / NonInteractive validation
# ──────────────────────────────────────────────────────────────────────────────

if ($NonInteractive) {
    Write-Host ""
    Write-Host "═══ Configuration (non-interactive) ════════════════════════════" -ForegroundColor Cyan
    foreach ($r in @(
        @{ Name = 'ResourceAppId'; Value = $ResourceAppId },
        @{ Name = 'TenantId';      Value = $TenantId      }
    )) {
        if (Test-IsPlaceholder $r.Value) {
            Write-Err "$($r.Name) is unset or placeholder ('$($r.Value)')"
            throw ("[$($r.Name)] is unset or still a placeholder. Pass -$($r.Name) <value> or omit -NonInteractive.")
        }
    }
    Write-Ok "Required values validated"
} else {
    Write-Host ""
    Write-Host "═══ Interactive configuration ═════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Hit Enter to keep the [bracketed] default; type a value to override." -ForegroundColor DarkGray
    Write-Host ""

    $ResourceAppId            = Read-ScalarVariable -Name 'ResourceAppId'            -Current $ResourceAppId            -Description 'wwexecution resource app Application ID (GUID)' -Required
    $TenantId                 = Read-ScalarVariable -Name 'TenantId'                 -Current $TenantId                 -Description 'Microsoft Entra tenant GUID'                    -Required
    $FunctionAppName          = Read-ScalarVariable -Name 'FunctionAppName'          -Current $FunctionAppName          -Description 'Azure Function App (engine) name - used for SPA CORS and validation'
    $FunctionAppResourceGroup = Read-ScalarVariable -Name 'FunctionAppResourceGroup' -Current $FunctionAppResourceGroup -Description 'Resource group of the Function App (for SPA CORS)'

    Write-Host ""
    Write-Host "  Apps to provision  (angular, react, webmvc, console, azurefunction, servicebus, All)" -ForegroundColor White
    $appsInput = (Read-Host "    [Enter = '$($Apps -join ',')']").Trim()
    if (-not [string]::IsNullOrWhiteSpace($appsInput)) {
        $candidate = @($appsInput -split '[,\s]+' | Where-Object { $_ })
        $valid = @('angular', 'react', 'webmvc', 'console', 'azurefunction', 'servicebus', 'All')
        $bad = @($candidate | Where-Object { $_ -notin $valid })
        if ($bad.Count -gt 0) { Write-Warn "Ignoring invalid app key(s): $($bad -join ', ')" }
        $candidate = @($candidate | Where-Object { $_ -in $valid })
        if ($candidate.Count -gt 0) { $Apps = $candidate }
    }

    $SecretLifetimeYears = Read-IntVariable -Name 'SecretLifetimeYears' -Current $SecretLifetimeYears -Min 1 -Max 2
    $AppRolesToAssign    = Read-StringArrayVariable -Name 'AppRolesToAssign' -Current $AppRolesToAssign -Description 'App-role values for app-only clients (console/daemon), MUST exist on resource app e.g. Warewolf_ClientApps'

    $selPreview = Resolve-SelectedApps -Selection $Apps
    if (@($selPreview | Where-Object { $_.Key -eq 'azurefunction' }).Count -gt 0) {
        $AzureFunctionMiObjectId = Read-ScalarVariable -Name 'AzureFunctionMiObjectId' -Current $AzureFunctionMiObjectId -Description 'Existing MI SP object id for AzureFunction (blank = enable MI on the client Function App below, or create a secret-bearing daemon app)'
        if (Test-IsPlaceholder $AzureFunctionMiObjectId) {
            $AzureFunctionClientAppName       = Read-ScalarVariable -Name 'AzureFunctionClientAppName'       -Current $AzureFunctionClientAppName       -Description 'Client Function App name to enable a system-assigned MI on (blank = create a secret-bearing daemon app)'
            if (-not (Test-IsPlaceholder $AzureFunctionClientAppName)) {
                $AzureFunctionClientResourceGroup = Read-ScalarVariable -Name 'AzureFunctionClientResourceGroup' -Current $AzureFunctionClientResourceGroup -Description 'Resource group of the client Function App'
            }
        }
    }
    if (@($selPreview | Where-Object { $_.Key -eq 'servicebus' }).Count -gt 0) {
        $ServiceBusMiObjectId = Read-ScalarVariable -Name 'ServiceBusMiObjectId' -Current $ServiceBusMiObjectId -Description 'Existing MI SP object id for AzureServiceBus (blank = create a secret-bearing daemon app)'
    }

    Write-Host ""
    Write-Host "  SkipCorsConfiguration  (skip CORS/Allowed Origins for SPA apps)" -ForegroundColor White
    $skipCorsInput = (Read-Host "    [Enter = $SkipCorsConfiguration]  (true/false)").Trim()
    if ($skipCorsInput -eq 'true')  { $SkipCorsConfiguration = $true }
    if ($skipCorsInput -eq 'false') { $SkipCorsConfiguration = $false }

    Write-Host ""
    Write-Host "  Validate after provisioning  (acquire token + call /secure/{workflow})" -ForegroundColor White
    $valInput = (Read-Host "    [Enter = $([bool]$Validate)]  (true/false)").Trim()
    if ($valInput -eq 'true')  { $Validate = $true }
    if ($valInput -eq 'false') { $Validate = $false }
}

$selectedApps = Resolve-SelectedApps -Selection $Apps

# ──────────────────────────────────────────────────────────────────────────────
# Plan summary
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Plan ═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ("  {0,-26} : {1}" -f 'ResourceAppId',            $ResourceAppId)
Write-Host ("  {0,-26} : {1}" -f 'TenantId',                 $TenantId)
Write-Host ("  {0,-26} : {1}" -f 'FunctionAppName',          $FunctionAppName)
Write-Host ("  {0,-26} : {1}" -f 'FunctionAppResourceGroup', $FunctionAppResourceGroup)
Write-Host ("  {0,-26} : {1}" -f 'SecretLifetimeYears',      $SecretLifetimeYears)
Write-Host ("  {0,-26} : {1}" -f 'AppRolesToAssign',         ($AppRolesToAssign -join ', '))
Write-Host ("  {0,-26} : {1}" -f 'SkipCorsConfiguration',    $SkipCorsConfiguration)
Write-Host ("  {0,-26} : {1}" -f 'Validate',                 [bool]$Validate)
Write-Host ("  {0,-26} : {1}" -f 'DryRun',                   [bool]$DryRun)
Write-Host ""
Write-Host "  Registrations to create / update:" -ForegroundColor DarkGray
foreach ($d in $selectedApps) {
    $extra = if ($d.Key -eq 'azurefunction' -and -not (Test-IsPlaceholder $AzureFunctionMiObjectId)) { ' (managed identity: existing SP)' }
             elseif ($d.Key -eq 'azurefunction' -and -not (Test-IsPlaceholder $AzureFunctionClientAppName)) { " (managed identity: enable on $AzureFunctionClientAppName)" }
             elseif ($d.Key -eq 'servicebus' -and -not (Test-IsPlaceholder $ServiceBusMiObjectId))   { ' (managed identity: existing SP)' }
             else { '' }
    $role = if ($d.RequiresRole) { '  [role REQUIRED]' } else { '' }
    Write-Host ("    {0,-28} {1,-13} {2}{3}{4}" -f $d.Prefix, $d.ClientType, ($d.RedirectUris -join ', '), $extra, $role) -ForegroundColor DarkGray
}
Write-Host ""

if ($DryRun) {
    Write-Warn "-DryRun set: exiting without changes."
    return
}

if (-not $NonInteractive) {
    $proceed = Read-Host "  Proceed with these values? [Y/n]"
    if ([string]::IsNullOrWhiteSpace($proceed)) { $proceed = 'y' }
    if ($proceed -notmatch '^[yY]') { Write-Warn "Aborted by user."; return }
}

# Console role pre-flight (fail-loud) — mirror the child's guard so we don't start a
# multi-app run that we know will leave a console client-credentials flow roleless.
# Daemon apps are NOT included: the child defaults them to 'Warewolf_ClientApps' and
# fails loud there if that role does not exist on the resource app.
$consoleSelected = @($selectedApps | Where-Object { $_.ClientType -eq 'Console' }).Count -gt 0
if ($consoleSelected -and @($AppRolesToAssign | Where-Object { $_ }).Count -eq 0) {
    throw ("Selected apps include a Console client whose client-credentials flow REQUIRES -AppRolesToAssign. " +
           "A roleless app-only caller carries no 'roles' claim and is rejected by the engine. " +
           "Pass -AppRolesToAssign with one or more values that exist on the resource app (e.g. Warewolf_ClientApps).")
}

# ──────────────────────────────────────────────────────────────────────────────
# Logging / transcript
# ──────────────────────────────────────────────────────────────────────────────

if ([string]::IsNullOrWhiteSpace($LogDir)) { $LogDir = Join-Path $PSScriptRoot 'logs' }
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$stamp          = Get-Date -Format 'yyyyMMdd-HHmmss'
$transcriptPath = Join-Path $LogDir "Configure-WwExecutionAuth-ClientApps-$stamp.log"
try { Start-Transcript -Path $transcriptPath -Force | Out-Null } catch { Write-Warn "Could not start transcript: $($_.Exception.Message)" }

$childOutputPath = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-Clients.output.json'
$results = New-Object System.Collections.Generic.List[object]

try {
    # ──────────────────────────────────────────────────────────────────────────
    # Stage 2 — Provision each selected app
    # ──────────────────────────────────────────────────────────────────────────
    foreach ($def in $selectedApps) {
        Write-Host ""
        Write-Host ("═══ Provision: {0} ({1}) ════════════════════════════" -f $def.DisplayName, $def.Prefix) -ForegroundColor Cyan

        $record = [pscustomobject]@{
            App           = $def.Key
            DisplayName   = $def.DisplayName
            ClientType    = $def.ClientType
            ClientId      = $null
            ClientSecret  = $null
            GrantType     = $null
            RedirectUris  = $def.RedirectUris
            RolesAssigned = @()
            SecretExpiry  = $null
            OutputFile    = $null
            UseMI         = $false
            Status        = 'pending'
            Validation    = $null
        }

        try {
            $childArgs = @{
                ResourceAppId           = $ResourceAppId
                TenantId                = $TenantId
                ClientType              = $def.ClientType
                ClientDisplayNamePrefix = $def.Prefix
                SecretLifetimeYears     = $SecretLifetimeYears
                NonInteractive          = $true
            }

            switch ($def.ClientType) {
                'SPA' {
                    $childArgs['SpaRedirectUris'] = $def.RedirectUris
                    if (-not (Test-IsPlaceholder $FunctionAppName))          { $childArgs['FunctionAppName'] = $FunctionAppName }
                    if (-not (Test-IsPlaceholder $FunctionAppResourceGroup)) { $childArgs['FunctionAppResourceGroup'] = $FunctionAppResourceGroup }
                    if ($SkipCorsConfiguration) { $childArgs['SkipCorsConfiguration'] = $true }
                }
                'Confidential' {
                    $childArgs['WebRedirectUris'] = $def.RedirectUris
                    if (@($AppRolesToAssign | Where-Object { $_ }).Count -gt 0) { $childArgs['AppRolesToAssign'] = $AppRolesToAssign }
                }
                'Console' {
                    $childArgs['ConsoleRedirectUris'] = $def.RedirectUris
                    $childArgs['AppRolesToAssign']    = $AppRolesToAssign
                }
                'Daemon' {
                    $childArgs['AppRolesToAssign'] = $AppRolesToAssign
                    $miId = if ($def.Key -eq 'azurefunction') { $AzureFunctionMiObjectId } else { $ServiceBusMiObjectId }
                    if (-not (Test-IsPlaceholder $miId)) {
                        # Existing MI SP object id supplied - assign roles to it directly.
                        $childArgs['DaemonUseManagedIdentity'] = $true
                        $childArgs['ManagedIdentityObjectId']  = $miId
                        $record.UseMI = $true
                    }
                    elseif ($def.Key -eq 'azurefunction' -and
                            -not (Test-IsPlaceholder $AzureFunctionClientAppName) -and
                            -not (Test-IsPlaceholder $AzureFunctionClientResourceGroup)) {
                        # No object id - derive the MI from the client Function App: the
                        # child enables its system-assigned MI and reads the principalId.
                        $childArgs['DaemonUseManagedIdentity']       = $true
                        $childArgs['DaemonFunctionAppName']          = $AzureFunctionClientAppName
                        $childArgs['DaemonFunctionAppResourceGroup'] = $AzureFunctionClientResourceGroup
                        $record.UseMI = $true
                    }
                }
            }

            # Remove any stale child output so we only ever read THIS app's result.
            Remove-Item $childOutputPath -Force -ErrorAction SilentlyContinue

            & $ClientScript @childArgs

            if (-not (Test-Path $childOutputPath)) {
                throw "Child script produced no output JSON - provisioning did not complete."
            }
            $childOut = Get-Content $childOutputPath -Raw | ConvertFrom-Json
            $block = $childOut.Clients.($def.ClientType)
            if (-not $block) { throw "Child output JSON has no '$($def.ClientType)' client block." }

            $record.ClientId      = $block.ClientId
            $record.ClientSecret  = if ($block.PSObject.Properties['ClientSecret']) { $block.ClientSecret } else { $null }
            $record.GrantType     = $block.GrantType
            $record.RolesAssigned = if ($block.PSObject.Properties['RolesAssigned']) { @($block.RolesAssigned) } else { @() }
            $record.SecretExpiry  = if ($block.PSObject.Properties['SecretExpiry']) { $block.SecretExpiry } else { $null }
            if ($block.PSObject.Properties['UseMI']) { $record.UseMI = [bool]$block.UseMI }

            # Persist this app's full result to its own per-app output file.
            $perAppPath = Join-Path $PSScriptRoot "Configure-WwExecutionAuth-ClientApps.$($def.Key).output.json"
            Move-Item $childOutputPath $perAppPath -Force
            $record.OutputFile = Split-Path $perAppPath -Leaf
            $record.Status     = 'provisioned'
            Write-Ok "$($def.DisplayName) provisioned: $($record.ClientId)"
        } catch {
            $record.Status = 'failed'
            $record.Validation = [pscustomobject]@{ Flow = 'n/a'; Result = 'skipped'; Detail = "provisioning failed: $($_.Exception.Message)" }
            Write-Err "$($def.DisplayName) failed: $($_.Exception.Message)"
        }

        $results.Add($record)
    }

    # ──────────────────────────────────────────────────────────────────────────
    # Stage 3 — Validation
    # ──────────────────────────────────────────────────────────────────────────
    if ($Validate) {
        Write-Host ""
        Write-Host "═══ Stage 3  Validate access to the Execution Engine ══════════════" -ForegroundColor Cyan

        if (Test-IsPlaceholder $FunctionAppName) {
            Write-Warn "FunctionAppName not provided - cannot run validation. Re-run with -FunctionAppName."
        } else {
            $functionAppUrl = "https://$FunctionAppName.azurewebsites.net"
            foreach ($record in $results) {
                if ($record.Status -ne 'provisioned') { continue }
                $def  = $selectedApps | Where-Object { $_.Key -eq $record.App } | Select-Object -First 1
                $hasSecret = -not [string]::IsNullOrWhiteSpace([string]$record.ClientSecret)
                $plan = Get-ValidationPlan -ClientType $record.ClientType -HasSecret $hasSecret -UseManagedIdentity $record.UseMI

                Write-Host ""
                Write-Host ("  ── {0} ({1}) ─ validation flow: {2}" -f $def.DisplayName, $record.ClientType, $plan.Flow) -ForegroundColor White

                $validation = $null
                if ($plan.Auto -and $plan.Flow -eq 'client-credentials') {
                    $validation = Invoke-AppOnlyValidation -TenantId $TenantId -ResourceAppId $ResourceAppId `
                        -FunctionAppUrl $functionAppUrl -Workflow $WorkflowName `
                        -ClientId $record.ClientId -ClientSecret $record.ClientSecret
                }

                if ($plan.Interactive) {
                    $doInteractive = $false
                    if ($NonInteractive) {
                        Write-Info "Interactive device-code validation skipped (NonInteractive). $($plan.Note)"
                    } else {
                        $ans = (Read-Host "    Run interactive device-code validation for this app? [y/N]").Trim()
                        $doInteractive = $ans -match '^[yY]'
                    }
                    if ($doInteractive) {
                        $dc = Invoke-DeviceCodeValidation -TenantId $TenantId -ResourceAppId $ResourceAppId `
                            -FunctionAppUrl $functionAppUrl -Workflow $WorkflowName -ClientId $record.ClientId
                        # Prefer the interactive result when no auto result exists.
                        if (-not $validation) { $validation = $dc }
                        else { $validation = [pscustomobject]@{ Flow = "$($validation.Flow)+device-code"; HttpStatus = $dc.HttpStatus; Result = "$($validation.Result)/$($dc.Result)"; Detail = "client-credentials: $($validation.Detail); device-code: $($dc.Detail)" } }
                    }
                }

                if (-not $validation) {
                    $validation = [pscustomobject]@{ Flow = $plan.Flow; HttpStatus = $null; Result = 'skipped'; Detail = $plan.Note }
                }

                switch ($validation.Result) {
                    'pass'    { Write-Ok  "Validation passed: $($validation.Detail)" }
                    'skipped' { Write-Info "Validation skipped: $($validation.Detail)" }
                    default   { Write-Err "Validation: $($validation.Result) - $($validation.Detail)" }
                }
                $record.Validation = $validation
            }
        }
    } else {
        Write-Info "Validation not requested (-Validate). Skipping Stage 3."
    }

    # ──────────────────────────────────────────────────────────────────────────
    # Stage 4 — Summary
    # ──────────────────────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "═══ Stage 4  Summary ══════════════════════════════════════════════" -ForegroundColor Cyan

    $summary = Format-ClientAppSummary -Results $results.ToArray() -TenantId $TenantId `
        -ResourceAppId $ResourceAppId -FunctionAppName $FunctionAppName -Timestamp ((Get-Date).ToString('o'))

    $summaryPath = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-ClientApps.summary.json'
    Write-TempJson -Path $summaryPath -Json ($summary | ConvertTo-Json -Depth 8)
    Write-Ok "Summary written to: $summaryPath"
    Write-Ok "Transcript log: $transcriptPath"

    Write-Host ""
    Write-Host ("  {0,-28} {1,-13} {2,-12} {3}" -f 'App', 'Type', 'Status', 'ClientId / Validation') -ForegroundColor DarkGray
    foreach ($r in $results) {
        $val = if ($r.Validation) { "$($r.Validation.Result)" } else { 'n/a' }
        $colour = switch ($r.Status) { 'provisioned' { 'Green' } 'failed' { 'Red' } default { 'Yellow' } }
        Write-Host ("  {0,-28} {1,-13} {2,-12} {3}  [val:{4}]" -f $r.App, $r.ClientType, $r.Status, $r.ClientId, $val) -ForegroundColor $colour
    }
    Write-Host ""

    $secretApps = @($results | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.ClientSecret) })
    if ($secretApps.Count -gt 0) {
        Write-Warn "Client secrets are stored in the per-app *.output.json files - move them to Azure Key Vault for production."
    }

    Write-Host "  +-------------------------------------------------------------------+"
    Write-Host "  | Next steps:                                                       |"
    Write-Host "  |  1. See docs/KB-ClientApps-Configuration.md to configure each app |"
    Write-Host "  |  2. Per-app output JSON holds ClientId/Secret for each example    |"
    Write-Host "  |  3. Move secrets to Azure Key Vault for production                |"
    Write-Host "  |  4. Run Remove-WwExecutionAuth-Clients.ps1 to undo a registration |"
    Write-Host "  +-------------------------------------------------------------------+"
    Write-Host ""
} finally {
    try { Stop-Transcript | Out-Null } catch {}
}
