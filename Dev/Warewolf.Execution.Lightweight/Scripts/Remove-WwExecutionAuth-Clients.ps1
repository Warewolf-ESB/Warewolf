# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
  Removes client app registrations, service principals, and role assignments
  created by Configure-WwExecutionAuth-Clients.ps1.

.DESCRIPTION
  Reverses all provisioning steps performed by Configure-WwExecutionAuth-Clients.ps1:

    Stage 1   Interactive cfg   - prompt / validate inputs, confirm plan
    Stage 2   Resolve apps      - locate registrations by display name
    Stage 3   Remove role asgns - revoke daemon app-role assignments from resource SP
    Stage 4   Remove perms      - remove delegated / app-role permission grants
    Stage 5   Delete SPs        - delete client service principals
    Stage 6   Delete apps       - delete client app registrations

  Operations are safe to re-run - skips resources that no longer exist.

.PARAMETER ResourceAppId
  Application (client) ID of the wwexecution resource app (needed to revoke
  daemon role assignments and delegated permissions).

.PARAMETER TenantId
  Microsoft Entra ID tenant GUID.

.PARAMETER ClientType
  Which client type(s) to remove: SPA, Confidential, Daemon, or All.
  Default: All.

.PARAMETER ClientDisplayNamePrefix
  Prefix used when registrations were created.  Default: wwexecution.

.PARAMETER DryRun
  Print what would be removed and exit without making changes.

.PARAMETER NonInteractive
  Skip all prompts; fail on missing required values.

.EXAMPLE
  ./Remove-WwExecutionAuth-Clients.ps1

.EXAMPLE
  ./Remove-WwExecutionAuth-Clients.ps1 -ResourceAppId "..." -TenantId "..." -NonInteractive

.EXAMPLE
  ./Remove-WwExecutionAuth-Clients.ps1 -ResourceAppId "..." -TenantId "..." -ClientType Daemon -DryRun
#>

[CmdletBinding()]
param(
    [string] $ResourceAppId,
    [string] $TenantId,

    [ValidateSet('SPA', 'Confidential', 'Daemon', 'All')]
    [string] $ClientType = 'All',

    [string] $ClientDisplayNamePrefix = 'wwexecution',

    [switch] $DryRun,
    [switch] $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

#region Helpers

# ── Output helpers ─────────────────────────────────────────────────────────────
function Write-Ok   { param([string]$Message) Write-Host "    [OK]   $Message" -ForegroundColor Green    }
function Write-Step { param([string]$Message) Write-Host "    [...] $Message"  -ForegroundColor Cyan     }
function Write-Warn { param([string]$Message) Write-Host "    [WRN] $Message"  -ForegroundColor Yellow   }
function Write-Err  { param([string]$Message) Write-Host "    [ERR] $Message"  -ForegroundColor Red      }
function Write-Info { param([string]$Message) Write-Host "    [INF] $Message"  -ForegroundColor DarkGray }

function Format-AzArgsForLog {
    param([Parameter(Mandatory)][string[]] $Arguments)
    $secretFlagRegex = '(?i)^(--password|--client-secret|--secret|--certificate)$'
    $rendered = New-Object System.Collections.Generic.List[string]
    $maskNext = $false
    foreach ($arg in $Arguments) {
        if ($maskNext)                    { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($arg -match $secretFlagRegex) { $rendered.Add($arg);             $maskNext = $true;  continue }
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
            Write-Warn "Transient error (attempt $attempt/$MaxAttempts) - retrying in ${wait}s"
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
        if     ($startBrace   -ge 0 -and ($startBracket -lt 0 -or $startBrace -lt $startBracket)) { $start = $startBrace   }
        elseif ($startBracket -ge 0)                                                              { $start = $startBracket }

        if ($start -gt 0)    { $text = $text.Substring($start) }
        elseif ($start -lt 0) {
            Write-Verbose "ConvertFrom-AzJson: no JSON in input. Buffer: $text"
            return $null
        }
        return $text | ConvertFrom-Json -Depth 50
    }
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

# Resolve an app by display name; returns $null if not found (no throw)
function Get-AppByName {
    param([Parameter(Mandatory)][string] $DisplayName)
    Write-Step "Looking up app registration '$DisplayName'"
    $app = $null
    try {
        $app = Invoke-AzCli @('ad', 'app', 'list', '--display-name', $DisplayName, '--query', '[0]', '-o', 'json') |
            ConvertFrom-AzJson
    } catch {
        Write-Warn "Could not query app '$DisplayName': $($_.Exception.Message)"
    }
    if ($app) {
        Write-Ok "Found: $($app.appId)  (object id: $($app.id))"
    } else {
        Write-Info "Not found: '$DisplayName' - already removed or never created"
    }
    return $app
}

# Resolve SP by appId; returns $null if not found
function Get-SpByAppId {
    param([Parameter(Mandatory)][string] $AppId)
    $sp = $null
    try {
        $sp = Invoke-AzCli @('ad', 'sp', 'show', '--id', $AppId, '-o', 'json') | ConvertFrom-AzJson
    } catch {}
    return $sp
}

# Remove all app-role assignments from the resource SP that belong to a given principal
function Remove-AppRoleAssignmentsForSP {
    param(
        [Parameter(Mandatory)][string] $ClientSpObjectId,
        [Parameter(Mandatory)][string] $ResourceSpId
    )
    Write-Step "Listing app-role assignments for SP $ClientSpObjectId on resource $ResourceSpId"
    $assignments = $null
    try {
        $assignments = Invoke-AzCli @(
            'rest', '--method', 'GET',
            '--url', "https://graph.microsoft.com/v1.0/servicePrincipals/$ResourceSpId/appRoleAssignedTo?`$filter=principalId eq '$ClientSpObjectId'"
        ) | ConvertFrom-AzJson
    } catch {
        Write-Warn "Could not list assignments: $($_.Exception.Message)"
        return
    }

    $list = if ($assignments -and $assignments.PSObject.Properties['value']) { @($assignments.value) } else { @() }
    if ($list.Count -eq 0) {
        Write-Info "No role assignments found for $ClientSpObjectId"
        return
    }

    foreach ($asgn in $list) {
        Write-Step "Removing role assignment $($asgn.id) (role: $($asgn.appRoleId))"
        try {
            Invoke-AzCli @(
                'rest', '--method', 'DELETE',
                '--url', "https://graph.microsoft.com/v1.0/servicePrincipals/$ResourceSpId/appRoleAssignedTo/$($asgn.id)"
            ) | Out-Null
            Write-Ok "Role assignment $($asgn.id) removed"
        } catch {
            if ($_.Exception.Message -match '404|does not exist|Not Found') {
                Write-Info "Assignment $($asgn.id) already removed"
            } else {
                Write-Err "Failed to remove assignment $($asgn.id): $($_.Exception.Message)"
                throw
            }
        }
    }
}

# Remove all permission grants (OAuth2 + app role) for a client app against the resource
function Remove-AppPermissions {
    param(
        [Parameter(Mandatory)][string] $ClientAppId,
        [Parameter(Mandatory)][string] $ResourceAppId_
    )
    Write-Step "Removing all API permissions for $ClientAppId against $ResourceAppId_"
    try {
        Invoke-AzCli @(
            'ad', 'app', 'permission', 'delete',
            '--id', $ClientAppId,
            '--api', $ResourceAppId_
        ) | Out-Null
        Write-Ok "Permissions removed"
    } catch {
        if ($_.Exception.Message -match '404|does not exist|Not Found|No.*permission') {
            Write-Info "Permissions already absent - skipped"
        } else {
            Write-Warn "Failed to remove permissions (non-fatal): $($_.Exception.Message)"
        }
    }
}

# Delete a service principal; noop if already gone
function Remove-ServicePrincipal {
    param([Parameter(Mandatory)][string] $SpObjectId)
    Write-Step "Deleting service principal $SpObjectId"
    try {
        Invoke-AzCli @('ad', 'sp', 'delete', '--id', $SpObjectId) | Out-Null
        Write-Ok "Service principal $SpObjectId deleted"
    } catch {
        if ($_.Exception.Message -match '404|does not exist|Not Found') {
            Write-Info "Service principal $SpObjectId already removed"
        } else {
            Write-Err "Failed to delete SP ${SpObjectId}: $($_.Exception.Message)"
            throw
        }
    }
}

# Delete an app registration; noop if already gone
function Remove-AppRegistration {
    param([Parameter(Mandatory)][string] $AppId)
    Write-Step "Deleting app registration $AppId"
    try {
        Invoke-AzCli @('ad', 'app', 'delete', '--id', $AppId) | Out-Null
        Write-Ok "App registration $AppId deleted"
    } catch {
        if ($_.Exception.Message -match '404|does not exist|Not Found') {
            Write-Info "App registration $AppId already removed"
        } else {
            Write-Err "Failed to delete app ${AppId}: $($_.Exception.Message)"
            throw
        }
    }
}

#endregion Helpers

# ──────────────────────────────────────────────────────────────────────────────
# Defaults / placeholder sentinels
# ──────────────────────────────────────────────────────────────────────────────

if (-not $PSBoundParameters.ContainsKey('ResourceAppId'))           { $ResourceAppId          = '<resourceAppId>' }
if (-not $PSBoundParameters.ContainsKey('TenantId'))                { $TenantId               = '<tenantId>' }
if (-not $PSBoundParameters.ContainsKey('ClientDisplayNamePrefix')) { $ClientDisplayNamePrefix = 'wwexecution' }

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
    Write-Host ""

    $ResourceAppId           = Read-ScalarVariable -Name 'ResourceAppId'          -Current $ResourceAppId          -Description 'wwexecution resource app Application ID (GUID)' -Required
    $TenantId                = Read-ScalarVariable -Name 'TenantId'               -Current $TenantId               -Description 'Microsoft Entra tenant GUID'                    -Required
    $ClientDisplayNamePrefix = Read-ScalarVariable -Name 'ClientDisplayNamePrefix' -Current $ClientDisplayNamePrefix -Description "Prefix used when registrations were created e.g. 'wwexecution'"

    Write-Host ""
    Write-Host "  ClientType  (SPA / Confidential / Daemon / All)" -ForegroundColor White
    $ctInput = (Read-Host "    [Enter = '$ClientType']").Trim()
    if (-not [string]::IsNullOrWhiteSpace($ctInput)) {
        if ($ctInput -notin @('SPA', 'Confidential', 'Daemon', 'All')) {
            Write-Warn "Invalid value '$ctInput' - keeping '$ClientType'"
        } else { $ClientType = $ctInput }
    }

    $removeSpa    = $ClientType -in @('SPA', 'All')
    $removeWeb    = $ClientType -in @('Confidential', 'All')
    $removeDaemon = $ClientType -in @('Daemon', 'All')

    Write-Host ""
    Write-Host "═══ Removal plan summary ══════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ("  {0,-25} : {1}" -f 'ResourceAppId',           $ResourceAppId)
    Write-Host ("  {0,-25} : {1}" -f 'TenantId',                $TenantId)
    Write-Host ("  {0,-25} : {1}" -f 'ClientDisplayNamePrefix', $ClientDisplayNamePrefix)
    Write-Host ("  {0,-25} : {1}" -f 'ClientType',              $ClientType)
    Write-Host ("  {0,-25} : {1}" -f 'Remove SPA',              $removeSpa)
    Write-Host ("  {0,-25} : {1}" -f 'Remove Confidential',     $removeWeb)
    Write-Host ("  {0,-25} : {1}" -f 'Remove Daemon',           $removeDaemon)
    Write-Host ("  {0,-25} : {1}" -f 'DryRun',                  $DryRun)
    Write-Host ""
    Write-Host "  Registrations that will be DELETED:" -ForegroundColor Yellow
    if ($removeSpa)    { Write-Host "    $ClientDisplayNamePrefix-spa"    -ForegroundColor Yellow }
    if ($removeWeb)    { Write-Host "    $ClientDisplayNamePrefix-web"    -ForegroundColor Yellow }
    if ($removeDaemon) { Write-Host "    $ClientDisplayNamePrefix-daemon" -ForegroundColor Yellow }
    Write-Host ""

    $proceed = Read-Host "  Proceed with DELETION of these registrations? [y/N]"
    if ([string]::IsNullOrWhiteSpace($proceed)) { $proceed = 'n' }
    if ($proceed -notmatch '^[yY]') {
        Write-Warn "Aborted by user."
        return
    }
}

# Derive removal flags (needed by both interactive and non-interactive paths)
$removeSpa    = $ClientType -in @('SPA', 'All')
$removeWeb    = $ClientType -in @('Confidential', 'All')
$removeDaemon = $ClientType -in @('Daemon', 'All')

if ($DryRun) {
    Write-Host ""
    Write-Host "═══ Dry-run (no changes) ══════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ("  {0,-25} : {1}" -f 'ResourceAppId',           $ResourceAppId)
    Write-Host ("  {0,-25} : {1}" -f 'TenantId',                $TenantId)
    Write-Host ("  {0,-25} : {1}" -f 'ClientDisplayNamePrefix', $ClientDisplayNamePrefix)
    Write-Host ("  {0,-25} : {1}" -f 'ClientType',              $ClientType)
    Write-Host ""
    Write-Host "  Would delete:" -ForegroundColor Yellow
    if ($removeSpa)    { Write-Host "    $ClientDisplayNamePrefix-spa    (app + SP + permissions)" -ForegroundColor Yellow }
    if ($removeWeb)    { Write-Host "    $ClientDisplayNamePrefix-web    (app + SP + permissions)" -ForegroundColor Yellow }
    if ($removeDaemon) { Write-Host "    $ClientDisplayNamePrefix-daemon (app + SP + role assignments)" -ForegroundColor Yellow }
    Write-Warn "-DryRun set: exiting without changes."
    return
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2 — Resolve resource app SP (needed for role-assignment removal)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 2  Resolve resource app ═════════════════════════════════" -ForegroundColor Cyan

$ResourceSpId = $null
Write-Step "Fetching resource SP for $ResourceAppId"
$resourceSpRaw = $null
try {
    $resourceSpRaw = Invoke-AzCli @('ad', 'sp', 'show', '--id', $ResourceAppId, '-o', 'json') | ConvertFrom-AzJson
} catch {}

if ($resourceSpRaw) {
    $ResourceSpId = $resourceSpRaw.id
    Write-Ok "Resource SP: $ResourceSpId"
} else {
    Write-Warn "Resource SP not found for $ResourceAppId - role-assignment removal will be skipped"
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 3 — Daemon: revoke app-role assignments from resource SP
# ──────────────────────────────────────────────────────────────────────────────

if ($removeDaemon -and $ResourceSpId) {
    Write-Host ""
    Write-Host "═══ Stage 3  Daemon - remove role assignments ══════════════════════" -ForegroundColor Cyan

    $daemonApp = Get-AppByName -DisplayName "$ClientDisplayNamePrefix-daemon"
    if ($daemonApp) {
        $daemonSp = Get-SpByAppId -AppId $daemonApp.appId
        if ($daemonSp) {
            Remove-AppRoleAssignmentsForSP -ClientSpObjectId $daemonSp.id -ResourceSpId $ResourceSpId
        } else {
            Write-Info "Daemon SP not found - role assignments already gone"
        }
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 4 — Remove delegated + app-role permission grants from all client apps
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 4  Remove API permissions ═══════════════════════════════" -ForegroundColor Cyan

if ($removeSpa) {
    $spaApp = Get-AppByName -DisplayName "$ClientDisplayNamePrefix-spa"
    if ($spaApp) { Remove-AppPermissions -ClientAppId $spaApp.appId -ResourceAppId_ $ResourceAppId }
}

if ($removeWeb) {
    $webApp = Get-AppByName -DisplayName "$ClientDisplayNamePrefix-web"
    if ($webApp) { Remove-AppPermissions -ClientAppId $webApp.appId -ResourceAppId_ $ResourceAppId }
}

if ($removeDaemon) {
    $daemonApp2 = Get-AppByName -DisplayName "$ClientDisplayNamePrefix-daemon"
    if ($daemonApp2) { Remove-AppPermissions -ClientAppId $daemonApp2.appId -ResourceAppId_ $ResourceAppId }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 5 — Delete service principals
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 5  Delete service principals ════════════════════════════" -ForegroundColor Cyan

$spNames = [System.Collections.Generic.List[string]]::new()
if ($removeSpa)    { [void]$spNames.Add("$ClientDisplayNamePrefix-spa")    }
if ($removeWeb)    { [void]$spNames.Add("$ClientDisplayNamePrefix-web")    }
if ($removeDaemon) { [void]$spNames.Add("$ClientDisplayNamePrefix-daemon") }
foreach ($appName in $spNames) {
    $app = Get-AppByName -DisplayName $appName
    if ($app) {
        $sp = Get-SpByAppId -AppId $app.appId
        if ($sp) {
            Remove-ServicePrincipal -SpObjectId $sp.id
        } else {
            Write-Info "SP for '$appName' not found - already removed"
        }
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 6 — Delete app registrations
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 6  Delete app registrations ═════════════════════════════" -ForegroundColor Cyan

$regNames = [System.Collections.Generic.List[string]]::new()
if ($removeSpa)    { [void]$regNames.Add("$ClientDisplayNamePrefix-spa")    }
if ($removeWeb)    { [void]$regNames.Add("$ClientDisplayNamePrefix-web")    }
if ($removeDaemon) { [void]$regNames.Add("$ClientDisplayNamePrefix-daemon") }
foreach ($appName in $regNames) {
    $app = Get-AppByName -DisplayName $appName
    if ($app) {
        Remove-AppRegistration -AppId $app.appId
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Done
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Done ═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Ok "Client app cleanup complete"
Write-Info "Re-run Configure-WwExecutionAuth-Clients.ps1 to re-provision these registrations"
Write-Host ""
