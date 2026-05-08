<#
.SYNOPSIS
  Provisions client app registrations in Microsoft Entra ID for calling the
  Warewolf wwexecution Function App.  Supports SPA, confidential web app,
  and daemon (Managed Identity or client-secret) client types.

.DESCRIPTION
  This script creates or updates Entra ID client app registrations that are
  authorised to call the wwexecution resource app (provisioned by
  Configure-WwExecutionAuth.ps1).  Each client type follows a distinct
  OAuth 2.0 grant:

    Type A - SPA              Authorization Code + PKCE (public client)
    Type B - Confidential     Authorization Code (confidential client) + OBO
    Type C - Daemon / MI      Client Credentials (application permissions)

  Operations are idempotent - re-running updates existing registrations in
  place without losing existing secrets or role assignments.

  Stage 0   Pre-flight        - resolve resource app, SP, scope, role IDs
  Stage 1   Interactive cfg   - prompt / validate all inputs, confirm plan
  Stage 2   Type A SPA        - create/update SPA registration + PKCE URIs
  Stage 3   Type B Confidential - create/update web registration + secret
  Stage 4   Type C Daemon     - create/update daemon registration + secret + roles
  Stage 5   Output            - persist summary JSON

.PARAMETER ResourceAppId
  The Application (client) ID of the wwexecution resource app registration.

.PARAMETER TenantId
  Microsoft Entra ID tenant GUID.

.PARAMETER ClientType
  SPA, Confidential, Daemon, or All.  Default: All.

.PARAMETER ClientDisplayNamePrefix
  Prefix for client app display names.  Default: wwexecution.

.PARAMETER SpaRedirectUris
  Redirect URIs for the SPA client.

.PARAMETER WebRedirectUris
  Redirect URIs for the confidential web client.

.PARAMETER DaemonUseManagedIdentity
  Skip client-secret creation for the daemon.

.PARAMETER SecretLifetimeYears
  Secret validity in years (1-2).  Default: 1.

.PARAMETER AppRolesToAssign
  App role values to assign to the daemon SP (sanitized, underscores not spaces).

.PARAMETER DryRun
  Print resolved configuration and exit without changes.

.PARAMETER NonInteractive
  Skip all prompts; fail on missing required values.

.EXAMPLE
  ./Configure-WwExecutionAuth-Clients.ps1
  ./Configure-WwExecutionAuth-Clients.ps1 -ResourceAppId "..." -TenantId "..." -NonInteractive
  ./Configure-WwExecutionAuth-Clients.ps1 -ResourceAppId "..." -TenantId "..." -DryRun -NonInteractive
#>

[CmdletBinding()]
param(
    [string]   $ResourceAppId,
    [string]   $TenantId,

    [ValidateSet('SPA', 'Confidential', 'Daemon', 'All')]
    [string]   $ClientType = 'All',

    [string]   $ClientDisplayNamePrefix = 'wwexecution',

    [string[]] $SpaRedirectUris = @(
        'http://localhost:4200',
        'http://localhost:3000'
    ),

    [string[]] $WebRedirectUris = @(
        'https://localhost:5001/signin-oidc'
    ),

    [switch]   $DaemonUseManagedIdentity,

    [ValidateRange(1, 2)]
    [int]      $SecretLifetimeYears = 1,

    [string[]] $AppRolesToAssign = @('Permission.Execute', 'Permission.View'),

    [switch]   $DryRun,
    [switch]   $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

#region Helpers

# ── Output helpers ─────────────────────────────────────────────────────────────
# Write-Ok    Green   - operation succeeded
# Write-Step  Cyan    - about to execute a named operation (shown BEFORE the call)
# Write-Warn  Yellow  - non-fatal warning
# Write-Err   Red     - fatal / verification failure
# Write-Info  DarkGray - informational / trace

function Write-Ok   { param([string]$Message) Write-Host "    [OK]   $Message" -ForegroundColor Green    }
function Write-Step { param([string]$Message) Write-Host "    [...] $Message"  -ForegroundColor Cyan     }
function Write-Warn { param([string]$Message) Write-Host "    [WRN] $Message"  -ForegroundColor Yellow   }
function Write-Err  { param([string]$Message) Write-Host "    [ERR] $Message"  -ForegroundColor Red      }
function Write-Info { param([string]$Message) Write-Host "    [INF] $Message"  -ForegroundColor DarkGray }

function Format-AzArgsForLog {
    param([Parameter(Mandatory)][string[]] $Arguments)
    $secretFlagRegex = '(?i)^(--password|--client-secret|--secret|--certificate)$'
    $kvSecretRegex   = '(?i)^([A-Z0-9_]*?(SECRET|PASSWORD|KEY|TOKEN|CONNECTIONSTRING)[A-Z0-9_]*?)=(.+)$'
    $rendered = New-Object System.Collections.Generic.List[string]
    $maskNext = $false
    foreach ($arg in $Arguments) {
        if ($maskNext)                    { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($arg -match $secretFlagRegex) { $rendered.Add($arg);             $maskNext = $true;  continue }
        if ($arg -match $kvSecretRegex)   { $rendered.Add("$($Matches[1])=***REDACTED***");      continue }
        if ($arg -match '\s')             { $rendered.Add(('"{0}"' -f ($arg -replace '"', '\"')));continue }
        $rendered.Add($arg)
    }
    return ($rendered -join ' ')
}

function Invoke-AzCli {
    param(
        [Parameter(Mandatory)][string[]] $Arguments,
        [int] $MaxAttempts = 5
    )
    $printable = Format-AzArgsForLog -Arguments $Arguments
    Write-Host "          > az $printable" -ForegroundColor DarkGray

    $transientPattern = (@(
        'Connection aborted', 'ConnectionResetError',
        '10054', '10053', '10060',
        'Read timed out', 'ReadTimeoutError', 'ConnectTimeoutError',
        'SSLEOFError', 'SSL.*EOF occurred in violation',
        'Max retries exceeded',
        'temporary failure in name resolution', 'Could not resolve host', 'getaddrinfo failed',
        'TooManyRequests', '"code":\s*"429"', ' 429 ',
        ' 500 Internal', ' 502 Bad Gateway', ' 503 ', 'ServiceUnavailable',
        ' 504 Gateway', 'GatewayTimeout', 'BadGateway'
    ) -join '|')

    $delays     = @(3, 7, 15, 30)
    $lastOutput = $null

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $lastOutput = & az @Arguments 2>&1
        if ($LASTEXITCODE -eq 0) { return $lastOutput }

        $asString    = ($lastOutput | Out-String)
        $isTransient = $asString -match $transientPattern

        if ($isTransient -and $attempt -lt $MaxAttempts) {
            $wait = $delays[[Math]::Min($attempt - 1, $delays.Length - 1)]
            Write-Warn ("transient error (attempt {0}/{1}) - retrying in {2}s" -f $attempt, $MaxAttempts, $wait)
            Start-Sleep -Seconds $wait
            continue
        }
        throw "az CLI failed ($LASTEXITCODE) running ``az $printable``:`n$asString"
    }
}

function ConvertFrom-AzJson {
    [CmdletBinding()]
    param([Parameter(ValueFromPipeline)][object] $InputObject)
    begin   { $sb = [System.Text.StringBuilder]::new() }
    process {
        if ($null -ne $InputObject) {
            foreach ($line in @($InputObject)) { [void]$sb.AppendLine([string]$line) }
        }
    }
    end {
        $text = $sb.ToString().Trim()
        if ([string]::IsNullOrWhiteSpace($text) -or $text -eq 'null') { return $null }

        $startBrace   = $text.IndexOf('{')
        $startBracket = $text.IndexOf('[')
        $start = -1
        if     ($startBrace   -ge 0 -and ($startBracket -lt 0 -or $startBrace   -lt $startBracket)) { $start = $startBrace   }
        elseif ($startBracket -ge 0)                                                                { $start = $startBracket }

        if ($start -gt 0)   { $text = $text.Substring($start) }
        elseif ($start -lt 0) {
            Write-Verbose "ConvertFrom-AzJson: no JSON in input. Buffer: $text"
            return $null
        }
        return $text | ConvertFrom-Json -Depth 50
    }
}

function Write-TempJson {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Json
    )
    # BOM-free UTF-8: PowerShell 5.x Set-Content -Encoding UTF8 prepends a 3-byte
    # BOM that causes `az rest --body @file` to fail with 400 Bad Request.
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Json, $utf8NoBom)
}

function Test-IsPlaceholder {
    param([string] $Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return $true }
    return $Value -match '^<[^>]+>$'
}

function Read-ScalarVariable {
    param(
        [Parameter(Mandatory)][string] $Name,
        [string] $Current,
        [string] $Description,
        [switch] $Required
    )
    $isPlaceholder = Test-IsPlaceholder $Current
    while ($true) {
        $hint = if ($Description) { "  ($Description)" } else { '' }
        if ($isPlaceholder) {
            Write-Host "  $Name$hint" -ForegroundColor White
            $userInput = Read-Host '    enter value'
        } else {
            Write-Host "  $Name$hint" -ForegroundColor White
            $userInput = Read-Host "    [Enter = '$Current']"
        }
        if ($null -ne $userInput) { $userInput = $userInput.Trim() }
        if ([string]::IsNullOrWhiteSpace($userInput)) {
            if ($isPlaceholder -and $Required) {
                Write-Host "    (required - please provide a value)" -ForegroundColor Yellow
                continue
            }
            return $Current
        }
        return $userInput
    }
}

function Read-IntVariable {
    param(
        [Parameter(Mandatory)][string] $Name,
        [int] $Current,
        [int] $Min = 1,
        [int] $Max = [int]::MaxValue
    )
    while ($true) {
        Write-Host "  $Name" -ForegroundColor White
        $userInput = Read-Host "    [Enter = $Current]"
        if ($null -ne $userInput) { $userInput = $userInput.Trim() }
        if ([string]::IsNullOrWhiteSpace($userInput)) { return $Current }
        $parsed = 0
        if (-not [int]::TryParse($userInput, [ref]$parsed)) {
            Write-Host "    (not a valid integer)" -ForegroundColor Yellow; continue
        }
        if ($parsed -lt $Min -or $parsed -gt $Max) {
            Write-Host "    (must be between $Min and $Max)" -ForegroundColor Yellow; continue
        }
        return $parsed
    }
}

function Read-StringArrayVariable {
    param(
        [Parameter(Mandatory)][string] $Name,
        [string[]] $Current,
        [string]   $Description
    )
    if ($null -eq $Current) { $Current = @() }

    Write-Host ""
    Write-Host "  $Name  ($Description)" -ForegroundColor White
    if ($Current.Count -eq 0) {
        Write-Host "    [current] (empty)" -ForegroundColor DarkGray
    } else {
        foreach ($v in $Current) { Write-Host "    [current] $v" -ForegroundColor DarkGray }
    }

    $action = (Read-Host "    [k]eep / [a]dd / [r]eplace / [c]lear  (default: k)").ToLower()
    if ([string]::IsNullOrWhiteSpace($action)) { $action = 'k' }

    switch ($action[0]) {
        'k' { return ,$Current }
        'c' { return ,@() }
        default {
            $list = New-Object 'System.Collections.Generic.List[string]'
            if ($action[0] -eq 'a') { foreach ($v in $Current) { [void]$list.Add($v) } }
            Write-Host "    Enter values one per line (empty line to finish):" -ForegroundColor DarkGray
            while ($true) {
                $v = (Read-Host "      value").Trim()
                if ([string]::IsNullOrWhiteSpace($v)) { break }
                [void]$list.Add($v)
            }
            return ,$list.ToArray()
        }
    }
}

function Get-OrCreateApp {
    param(
        [Parameter(Mandatory)][string] $DisplayName,
        [string] $SignInAudience = 'AzureADMyOrg'
    )
    Write-Step "Looking up app registration '$DisplayName'"
    $existing = Invoke-AzCli @('ad', 'app', 'list', '--display-name', $DisplayName, '--query', '[0]', '-o', 'json') |
        ConvertFrom-AzJson
    if ($existing) {
        Write-Ok "Found existing app: $($existing.appId)"
        return $existing
    }
    Write-Step "Creating app registration '$DisplayName'"
    $app = Invoke-AzCli @('ad', 'app', 'create', '--display-name', $DisplayName,
        '--sign-in-audience', $SignInAudience, '-o', 'json') | ConvertFrom-AzJson
    Write-Ok "App created: $($app.appId)"
    Write-Info "Waiting 10s for Graph replication"
    Start-Sleep -Seconds 10
    return $app
}

function Ensure-ServicePrincipal {
    param([Parameter(Mandatory)][string] $AppId)
    Write-Step "Checking service principal for $AppId"
    $sp = $null
    try {
        $sp = Invoke-AzCli @('ad', 'sp', 'show', '--id', $AppId, '-o', 'json') | ConvertFrom-AzJson
    } catch {}
    if ($sp) {
        Write-Ok "SP found: $($sp.id)"
        return $sp
    }
    Write-Step "Creating service principal for $AppId"
    $sp = Invoke-AzCli @('ad', 'sp', 'create', '--id', $AppId, '-o', 'json') | ConvertFrom-AzJson
    Write-Ok "SP created: $($sp.id)"
    Write-Info "Waiting 10s for Graph replication"
    Start-Sleep -Seconds 10
    return $sp
}

function Grant-DelegatedPermission {
    param(
        [Parameter(Mandatory)][string] $ClientAppId,
        [Parameter(Mandatory)][string] $ResourceAppId_,
        [Parameter(Mandatory)][string] $ScopeId
    )
    Write-Step "Granting delegated user_impersonation permission (scope $ScopeId) to $ClientAppId"
    try {
        Invoke-AzCli @(
            'ad', 'app', 'permission', 'add',
            '--id', $ClientAppId,
            '--api', $ResourceAppId_,
            '--api-permissions', "$ScopeId=Scope"
        ) | Out-Null
        Write-Ok "Delegated permission granted"
    } catch {
        if ($_.Exception.Message -match 'already') {
            Write-Info "Delegated permission already present - skipped"
        } else {
            Write-Err "Failed to grant delegated permission: $($_.Exception.Message)"
            throw
        }
    }
}

function Grant-AppRoleToSP {
    param(
        [Parameter(Mandatory)][string] $ClientSpObjectId,
        [Parameter(Mandatory)][string] $ResourceSpId,
        [Parameter(Mandatory)][string] $RoleId,
        [Parameter(Mandatory)][string] $RoleName
    )
    Write-Step "Assigning app role '$RoleName' to SP $ClientSpObjectId"
    $bodyObj = @{
        principalId = $ClientSpObjectId
        resourceId  = $ResourceSpId
        appRoleId   = $RoleId
    }
    $tmp = [System.IO.Path]::GetTempFileName()
    Write-TempJson -Path $tmp -Json ($bodyObj | ConvertTo-Json -Compress)
    try {
        Invoke-AzCli @(
            'rest', '--method', 'POST',
            '--url', "https://graph.microsoft.com/v1.0/servicePrincipals/$ResourceSpId/appRoleAssignedTo",
            '--headers', 'Content-Type=application/json',
            '--body', "@$tmp"
        ) | Out-Null
        Write-Ok "Role '$RoleName' assigned to $ClientSpObjectId"
    } catch {
        $msg = $_.Exception.Message
        if ($msg -match 'already exists|already been assigned|Permission being assigned') {
            Write-Info "Role '$RoleName' already assigned - skipped"
        } else {
            Write-Err "Failed to assign role '$RoleName': $msg"
            throw
        }
    } finally {
        Remove-Item $tmp -Force -ErrorAction SilentlyContinue
    }
}

#endregion Helpers

# ──────────────────────────────────────────────────────────────────────────────
# Defaults / placeholder sentinels
# ──────────────────────────────────────────────────────────────────────────────

if (-not $PSBoundParameters.ContainsKey('ResourceAppId'))           { $ResourceAppId          = '<resourceAppId>' }
if (-not $PSBoundParameters.ContainsKey('TenantId'))                { $TenantId               = '<tenantId>' }
if (-not $PSBoundParameters.ContainsKey('ClientDisplayNamePrefix')) { $ClientDisplayNamePrefix = 'wwexecution' }
if (-not $PSBoundParameters.ContainsKey('SecretLifetimeYears'))     { $SecretLifetimeYears     = 1 }

# ──────────────────────────────────────────────────────────────────────────────
# Stage 1 — Interactive configuration / NonInteractive validation
# ──────────────────────────────────────────────────────────────────────────────

if ($NonInteractive) {
    Write-Host ""
    Write-Host "═══ Configuration (non-interactive) ════════════════════════════" -ForegroundColor Cyan
    Write-Info "Skipping prompts.  Validating required values..."

    foreach ($r in @(
        @{ Name = 'ResourceAppId'; Value = $ResourceAppId },
        @{ Name = 'TenantId';      Value = $TenantId      }
    )) {
        if (Test-IsPlaceholder $r.Value) {
            Write-Err "$($r.Name) is unset or placeholder ('$($r.Value)')"
            throw ("[$($r.Name)] is unset or still a placeholder. " +
                   "Pass -$($r.Name) <value> on the command line or omit -NonInteractive to be prompted.")
        }
    }
    Write-Ok "Required values validated"
} else {
    Write-Host ""
    Write-Host "═══ Interactive configuration ═════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Hit Enter to keep the [bracketed] default; type a value to override." -ForegroundColor DarkGray
    Write-Host "  Required values (shown without a default) must be entered." -ForegroundColor DarkGray
    Write-Host ""

    $ResourceAppId           = Read-ScalarVariable -Name 'ResourceAppId'          -Current $ResourceAppId          -Description 'wwexecution resource app Application ID (GUID)' -Required
    $TenantId                = Read-ScalarVariable -Name 'TenantId'               -Current $TenantId               -Description 'Microsoft Entra tenant GUID'                    -Required
    $ClientDisplayNamePrefix = Read-ScalarVariable -Name 'ClientDisplayNamePrefix' -Current $ClientDisplayNamePrefix -Description "Prefix for client app names e.g. 'wwexecution' -> wwexecution-spa"

    Write-Host ""
    Write-Host "  ClientType  (SPA / Confidential / Daemon / All)" -ForegroundColor White
    $ctInput = (Read-Host "    [Enter = '$ClientType']").Trim()
    if (-not [string]::IsNullOrWhiteSpace($ctInput)) {
        if ($ctInput -notin @('SPA', 'Confidential', 'Daemon', 'All')) {
            Write-Warn "Invalid value '$ctInput' - keeping '$ClientType'"
        } else { $ClientType = $ctInput }
    }

    $SecretLifetimeYears = Read-IntVariable -Name 'SecretLifetimeYears' -Current $SecretLifetimeYears -Min 1 -Max 2

    $SpaRedirectUris  = Read-StringArrayVariable -Name 'SpaRedirectUris'  -Current $SpaRedirectUris  -Description 'Redirect URIs for the SPA client (PKCE)'
    $WebRedirectUris  = Read-StringArrayVariable -Name 'WebRedirectUris'  -Current $WebRedirectUris  -Description 'Redirect URIs for the confidential web client'
    $AppRolesToAssign = Read-StringArrayVariable -Name 'AppRolesToAssign' -Current $AppRolesToAssign -Description 'App role values for daemon SP (sanitized e.g. Warewolf_Developers)'

    Write-Host ""
    Write-Host "  DaemonUseManagedIdentity  (skip client-secret for daemon)" -ForegroundColor White
    $miInput = (Read-Host "    [Enter = $DaemonUseManagedIdentity]  (true/false)").Trim()
    if ($miInput -eq 'true')  { $DaemonUseManagedIdentity = $true  }
    if ($miInput -eq 'false') { $DaemonUseManagedIdentity = $false }

    $provisionSpa    = $ClientType -in @('SPA', 'All')
    $provisionWeb    = $ClientType -in @('Confidential', 'All')
    $provisionDaemon = $ClientType -in @('Daemon', 'All')

    Write-Host ""
    Write-Host "═══ Configuration summary ═════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ("  {0,-25} : {1}" -f 'ResourceAppId',           $ResourceAppId)
    Write-Host ("  {0,-25} : {1}" -f 'TenantId',                $TenantId)
    Write-Host ("  {0,-25} : {1}" -f 'ClientDisplayNamePrefix', $ClientDisplayNamePrefix)
    Write-Host ("  {0,-25} : {1}" -f 'ClientType',              $ClientType)
    Write-Host ("  {0,-25} : {1}" -f 'SecretLifetimeYears',     $SecretLifetimeYears)
    Write-Host ("  {0,-25} : {1}" -f 'DaemonUseManagedIdentity',$DaemonUseManagedIdentity)
    Write-Host ("  {0,-25} : {1}" -f 'Provision SPA',           $provisionSpa)
    Write-Host ("  {0,-25} : {1}" -f 'Provision Confidential',  $provisionWeb)
    Write-Host ("  {0,-25} : {1}" -f 'Provision Daemon',        $provisionDaemon)
    if ($provisionSpa)    { Write-Host ("  {0,-25} : {1}" -f 'SpaRedirectUris',  ($SpaRedirectUris  -join ', ')) }
    if ($provisionWeb)    { Write-Host ("  {0,-25} : {1}" -f 'WebRedirectUris',  ($WebRedirectUris  -join ', ')) }
    if ($provisionDaemon) { Write-Host ("  {0,-25} : {1}" -f 'AppRolesToAssign', ($AppRolesToAssign -join ', ')) }
    Write-Host ("  {0,-25} : {1}" -f 'DryRun',                 $DryRun)
    Write-Host ""
    Write-Host "  Client app names that will be created / updated:" -ForegroundColor DarkGray
    if ($provisionSpa)    { Write-Host "    $ClientDisplayNamePrefix-spa"    -ForegroundColor DarkGray }
    if ($provisionWeb)    { Write-Host "    $ClientDisplayNamePrefix-web"    -ForegroundColor DarkGray }
    if ($provisionDaemon) { Write-Host "    $ClientDisplayNamePrefix-daemon" -ForegroundColor DarkGray }
    Write-Host ""

    $proceed = Read-Host "  Proceed with these values? [Y/n]"
    if ([string]::IsNullOrWhiteSpace($proceed)) { $proceed = 'y' }
    if ($proceed -notmatch '^[yY]') {
        Write-Warn "Aborted by user."
        return
    }
}

# Derive provisioning flags (needed by both interactive and non-interactive paths)
$provisionSpa    = $ClientType -in @('SPA', 'All')
$provisionWeb    = $ClientType -in @('Confidential', 'All')
$provisionDaemon = $ClientType -in @('Daemon', 'All')

if ($DryRun) {
    Write-Host ""
    Write-Host "═══ Dry-run summary (no changes made) ════════════════════════════" -ForegroundColor Yellow
    Write-Host ("  {0,-25} : {1}" -f 'ResourceAppId',           $ResourceAppId)
    Write-Host ("  {0,-25} : {1}" -f 'TenantId',                $TenantId)
    Write-Host ("  {0,-25} : {1}" -f 'ClientType',              $ClientType)
    Write-Host ("  {0,-25} : {1}" -f 'ClientDisplayNamePrefix', $ClientDisplayNamePrefix)
    Write-Host ("  {0,-25} : {1}" -f 'SecretLifetimeYears',     $SecretLifetimeYears)
    Write-Host ("  {0,-25} : {1}" -f 'DaemonUseManagedIdentity',$DaemonUseManagedIdentity)
    Write-Host ("  {0,-25} : {1}" -f 'Provision SPA',           $provisionSpa)
    Write-Host ("  {0,-25} : {1}" -f 'Provision Confidential',  $provisionWeb)
    Write-Host ("  {0,-25} : {1}" -f 'Provision Daemon',        $provisionDaemon)
    if ($provisionSpa)    { Write-Host ("  {0,-25} : {1}" -f 'SpaRedirectUris',  ($SpaRedirectUris  -join ', ')) }
    if ($provisionWeb)    { Write-Host ("  {0,-25} : {1}" -f 'WebRedirectUris',  ($WebRedirectUris  -join ', ')) }
    if ($provisionDaemon) { Write-Host ("  {0,-25} : {1}" -f 'AppRolesToAssign', ($AppRolesToAssign -join ', ')) }
    Write-Warn "-DryRun set: exiting without changes."
    return
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 0b — Resolve resource app metadata
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 0  Pre-flight: resolve resource app ═════════════════════" -ForegroundColor Cyan
Write-Host "    ResourceAppId: $ResourceAppId"
Write-Host "    TenantId:      $TenantId"

Write-Step "Fetching resource app registration $ResourceAppId"
$resourceApp = Invoke-AzCli @('ad', 'app', 'show', '--id', $ResourceAppId, '-o', 'json') | ConvertFrom-AzJson
if (-not $resourceApp) {
    Write-Err "Resource app '$ResourceAppId' not found in tenant."
    throw "Resource app '$ResourceAppId' not found in tenant."
}
Write-Ok "Resource app found: $($resourceApp.displayName)"

Write-Step "Fetching resource app service principal"
$resourceSpRaw = Invoke-AzCli @('ad', 'sp', 'show', '--id', $ResourceAppId, '-o', 'json') | ConvertFrom-AzJson
if (-not $resourceSpRaw) {
    Write-Err "Resource SP not found. Run Configure-WwExecutionAuth.ps1 first."
    throw "Resource app service principal not found."
}
$ResourceSpId = $resourceSpRaw.id
Write-Ok "Resource SP: $ResourceSpId"

# Resolve user_impersonation scope ID
$scopeId = $null
if ($resourceApp.api -and $resourceApp.api.oauth2PermissionScopes) {
    foreach ($s in @($resourceApp.api.oauth2PermissionScopes)) {
        $sVal = if ($s.PSObject.Properties['value'])     { $s.value }     else { $null }
        $sEn  = if ($s.PSObject.Properties['isEnabled']) { $s.isEnabled } else { $false }
        if ($sVal -eq 'user_impersonation' -and $sEn -eq $true) { $scopeId = $s.id; break }
    }
}
if (-not $scopeId) {
    Write-Err "No enabled 'user_impersonation' scope on resource app. Run Configure-WwExecutionAuth.ps1 Stage 3b."
    throw "Resource app missing user_impersonation scope."
}
Write-Ok "user_impersonation scope: $scopeId"

# Resolve app role IDs from resource SP
$roleIdMap = @{}
if ($resourceSpRaw.appRoles) {
    foreach ($r in @($resourceSpRaw.appRoles)) {
        $rVal = if ($r.PSObject.Properties['value'])     { $r.value }     else { $null }
        $rEn  = if ($r.PSObject.Properties['isEnabled']) { $r.isEnabled } else { $false }
        if ($rVal -and $rEn) { $roleIdMap[$rVal] = $r.id }
    }
}
if ($roleIdMap.Count -gt 0) {
    Write-Ok "Available app roles: $($roleIdMap.Keys -join ', ')"
} else {
    Write-Warn "No enabled app roles found on resource SP - daemon role assignment will skip"
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2 — Type A: SPA (Public Client, Authorization Code + PKCE)
# ──────────────────────────────────────────────────────────────────────────────

$results = @{}

if ($provisionSpa) {
    Write-Host ""
    Write-Host "═══ Stage 2  Type A - SPA Client ══════════════════════════════════" -ForegroundColor Cyan

    $spaName = "$ClientDisplayNamePrefix-spa"
    $spaApp  = Get-OrCreateApp -DisplayName $spaName

    Write-Step "Setting SPA redirect URIs and enabling public client (isFallbackPublicClient) on $($spaApp.id)"
    $spaTmp = [System.IO.Path]::GetTempFileName()
    Write-TempJson -Path $spaTmp -Json (@{
        spa                    = @{ redirectUris = $SpaRedirectUris }
        isFallbackPublicClient = $true
    } | ConvertTo-Json -Depth 4)
    try {
        Invoke-AzCli @(
            'rest', '--method', 'PATCH',
            '--url', "https://graph.microsoft.com/v1.0/applications/$($spaApp.id)",
            '--headers', 'Content-Type=application/json',
            '--body', "@$spaTmp"
        ) | Out-Null
        Write-Ok "SPA redirect URIs set and isFallbackPublicClient enabled: $($SpaRedirectUris -join ', ')"
    } finally { Remove-Item $spaTmp -Force -ErrorAction SilentlyContinue }

    Grant-DelegatedPermission -ClientAppId $spaApp.appId -ResourceAppId_ $ResourceAppId -ScopeId $scopeId

    Write-Step "Granting admin consent for $($spaApp.appId)"
    try {
        Invoke-AzCli @('ad', 'app', 'permission', 'admin-consent', '--id', $spaApp.appId) | Out-Null
        Write-Ok "Admin consent granted"
    } catch {
        Write-Warn "Admin consent failed (may require Global Admin): $($_.Exception.Message)"
    }

    $results['SPA'] = @{
        DisplayName  = $spaName
        ClientId     = $spaApp.appId
        RedirectUris = $SpaRedirectUris
        GrantType    = 'Authorization Code + PKCE (public client)'
    }
    Write-Ok "SPA provisioned: $($spaApp.appId)"
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 3 — Type B: Confidential Web App (Authorization Code + OBO)
# ──────────────────────────────────────────────────────────────────────────────

if ($provisionWeb) {
    Write-Host ""
    Write-Host "═══ Stage 3  Type B - Confidential Web App ════════════════════════" -ForegroundColor Cyan

    $webName = "$ClientDisplayNamePrefix-web"
    $webApp  = Get-OrCreateApp -DisplayName $webName

    Write-Step "Setting web redirect URIs on $($webApp.appId)"
    Invoke-AzCli (@('ad', 'app', 'update', '--id', $webApp.appId, '--web-redirect-uris') + $WebRedirectUris) | Out-Null
    Write-Ok "Web redirect URIs set: $($WebRedirectUris -join ', ')"

    $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
    Write-Step "Creating client secret for $($webApp.appId) (expires $endDate)"
    $webCred = Invoke-AzCli @(
        'ad', 'app', 'credential', 'reset',
        '--id', $webApp.appId,
        '--display-name', "client-secret-$(Get-Date -Format yyyyMMddHHmm)",
        '--end-date', $endDate,
        '--append', '-o', 'json'
    ) | ConvertFrom-AzJson
    Write-Ok "Client secret created (expires $endDate)"

    Grant-DelegatedPermission -ClientAppId $webApp.appId -ResourceAppId_ $ResourceAppId -ScopeId $scopeId

    foreach ($roleName in $AppRolesToAssign) {
        if ($roleIdMap.ContainsKey($roleName)) {
            Write-Step "Adding app role permission '$roleName' to $($webApp.appId)"
            try {
                Invoke-AzCli @(
                    'ad', 'app', 'permission', 'add',
                    '--id', $webApp.appId,
                    '--api', $ResourceAppId,
                    '--api-permissions', "$($roleIdMap[$roleName])=Role"
                ) | Out-Null
                Write-Ok "App role permission added: $roleName"
            } catch {
                if ($_.Exception.Message -match 'already') {
                    Write-Info "App role permission already present: $roleName"
                } else {
                    Write-Err "Failed to add app role '$roleName': $($_.Exception.Message)"
                    throw
                }
            }
        } else {
            Write-Warn "Role '$roleName' not found on resource app - skipping permission add"
        }
    }

    Write-Step "Granting admin consent for $($webApp.appId)"
    try {
        Invoke-AzCli @('ad', 'app', 'permission', 'admin-consent', '--id', $webApp.appId) | Out-Null
        Write-Ok "Admin consent granted"
    } catch {
        Write-Warn "Admin consent failed: $($_.Exception.Message)"
    }

    $results['Confidential'] = @{
        DisplayName  = $webName
        ClientId     = $webApp.appId
        ClientSecret = $webCred.password
        SecretExpiry = $endDate
        RedirectUris = $WebRedirectUris
        GrantType    = 'Authorization Code (confidential) + OBO'
    }
    Write-Ok "Confidential client provisioned: $($webApp.appId)"
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 4 — Type C: Daemon / Service (Client Credentials or MI)
# ──────────────────────────────────────────────────────────────────────────────

if ($provisionDaemon) {
    Write-Host ""
    Write-Host "═══ Stage 4  Type C - Daemon / Service ════════════════════════════" -ForegroundColor Cyan

    $daemonName = "$ClientDisplayNamePrefix-daemon"
    $daemonApp  = Get-OrCreateApp -DisplayName $daemonName

    $daemonSecret = $null
    if (-not $DaemonUseManagedIdentity) {
        $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
        Write-Step "Creating daemon client secret (expires $endDate)"
        $daemonCred = Invoke-AzCli @(
            'ad', 'app', 'credential', 'reset',
            '--id', $daemonApp.appId,
            '--display-name', "daemon-secret-$(Get-Date -Format yyyyMMddHHmm)",
            '--end-date', $endDate,
            '--append', '-o', 'json'
        ) | ConvertFrom-AzJson
        $daemonSecret = $daemonCred.password
        Write-Ok "Daemon client secret created (expires $endDate)"
    } else {
        Write-Info "MI mode - no client secret created"
        Write-Info "Assign the MI's SP to the resource app roles manually:"
        Write-Info "  az rest --method POST --url 'https://graph.microsoft.com/v1.0/servicePrincipals/$ResourceSpId/appRoleAssignedTo' ..."
    }

    $daemonSp = Ensure-ServicePrincipal -AppId $daemonApp.appId

    foreach ($roleName in $AppRolesToAssign) {
        if (-not $roleIdMap.ContainsKey($roleName)) {
            Write-Warn "Role '$roleName' not found on resource SP - skipping"
            continue
        }
        Grant-AppRoleToSP `
            -ClientSpObjectId $daemonSp.id `
            -ResourceSpId     $ResourceSpId `
            -RoleId           $roleIdMap[$roleName] `
            -RoleName         $roleName
    }

    $results['Daemon'] = @{
        DisplayName   = $daemonName
        ClientId      = $daemonApp.appId
        SpObjectId    = $daemonSp.id
        ClientSecret  = $daemonSecret
        UseMI         = [bool]$DaemonUseManagedIdentity
        RolesAssigned = $AppRolesToAssign
        GrantType     = 'Client Credentials'
    }
    Write-Ok "Daemon client provisioned: $($daemonApp.appId)"
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 5 — Output summary (BOM-free so jq / CI pipelines can parse it)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 5  Output summary ════════════════════════════════════════" -ForegroundColor Cyan

$outputFile = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-Clients.output.json'
$outputObj  = [pscustomobject]@{
    Timestamp               = (Get-Date).ToString('o')
    TenantId                = $TenantId
    ResourceAppId           = $ResourceAppId
    ResourceSpId            = $ResourceSpId
    Scope                   = "api://$ResourceAppId/.default"
    Authority               = "https://login.microsoftonline.com/$TenantId"
    ClientDisplayNamePrefix = $ClientDisplayNamePrefix
    Clients                 = $results
}
Write-Step "Writing output JSON to $outputFile"
Write-TempJson -Path $outputFile -Json ($outputObj | ConvertTo-Json -Depth 6)
Write-Ok "Output written to: $outputFile"

# ──────────────────────────────────────────────────────────────────────────────
# Done
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Done ═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
if ($results.ContainsKey('SPA')) {
    Write-Ok "SPA ClientId:          $($results['SPA'].ClientId)"
}
if ($results.ContainsKey('Confidential')) {
    Write-Ok "Confidential ClientId: $($results['Confidential'].ClientId)"
    Write-Warn "Confidential Secret stored in output JSON - move to Key Vault for production"
}
if ($results.ContainsKey('Daemon')) {
    Write-Ok "Daemon ClientId:       $($results['Daemon'].ClientId)"
    if (-not $DaemonUseManagedIdentity) {
        Write-Warn "Daemon Secret stored in output JSON - move to Key Vault for production"
    }
}
Write-Host ""
Write-Host "  +-------------------------------------------------------------------+"
Write-Host "  | Next steps:                                                       |"
Write-Host "  |  1. Use Get-WwExecutionToken.ps1 to acquire tokens                |"
Write-Host "  |  2. See Example-ClientApps-OrdersSales.ps1 for E2E demo           |"
Write-Host "  |  3. Store secrets in Azure Key Vault for production deployments   |"
Write-Host "  |  4. Run Remove-WwExecutionAuth-Clients.ps1 to undo this setup     |"
Write-Host "  +-------------------------------------------------------------------+"
Write-Host ""
