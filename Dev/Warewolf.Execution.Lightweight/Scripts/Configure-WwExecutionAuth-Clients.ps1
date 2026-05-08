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

    Type A — SPA              Authorization Code + PKCE (public client)
    Type B — Confidential     Authorization Code (confidential client) + OBO
    Type C — Daemon / MI      Client Credentials (application permissions)

  Operations are idempotent — re-running updates existing registrations in
  place without losing existing secrets or role assignments.

.PARAMETER ResourceAppId
  The Application (client) ID of the wwexecution resource app registration
  (the one created by Configure-WwExecutionAuth.ps1).  Required.

.PARAMETER TenantId
  Microsoft Entra ID tenant GUID.  Required.

.PARAMETER ClientType
  Which client type to provision: 'SPA', 'Confidential', 'Daemon', or 'All'.
  Default: 'All'.

.PARAMETER ClientDisplayNamePrefix
  Prefix for the client app display names.  E.g. "wwexec" produces
  "wwexec-spa", "wwexec-web", "wwexec-daemon".  Default: "wwexecution".

.PARAMETER SpaRedirectUris
  Redirect URIs for the SPA client (comma-separated or array).
  Default: "http://localhost:4200", "http://localhost:3000".

.PARAMETER WebRedirectUris
  Redirect URIs for the confidential web client.
  Default: "https://localhost:5001/signin-oidc".

.PARAMETER DaemonUseManagedIdentity
  When set, skips client-secret creation for the daemon and documents
  that MI should be used instead.  The caller must assign the MI's SP
  to the resource app roles separately.

.PARAMETER SecretLifetimeYears
  Secret validity in years (1–2, Entra cap).  Default: 1.

.PARAMETER AppRolesToAssign
  App role values to assign to the daemon SP.  Default:
  @('Permission.Execute','Permission.View').

.PARAMETER DryRun
  Print the plan and exit without making changes.

.PARAMETER NonInteractive
  Skip all prompts; fail on missing required values.

.EXAMPLE
  # Provision all three client types
  ./Configure-WwExecutionAuth-Clients.ps1 `
      -ResourceAppId "11111111-1111-1111-1111-111111111111" `
      -TenantId "22222222-2222-2222-2222-222222222222"

.EXAMPLE
  # Provision only a daemon client with MI
  ./Configure-WwExecutionAuth-Clients.ps1 `
      -ResourceAppId "11111111-..." -TenantId "22222222-..." `
      -ClientType Daemon -DaemonUseManagedIdentity

.EXAMPLE
  # Dry-run for CI validation
  ./Configure-WwExecutionAuth-Clients.ps1 `
      -ResourceAppId "11111111-..." -TenantId "22222222-..." `
      -DryRun -NonInteractive
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResourceAppId,

    [Parameter(Mandatory)]
    [string] $TenantId,

    [ValidateSet('SPA','Confidential','Daemon','All')]
    [string] $ClientType = 'All',

    [string] $ClientDisplayNamePrefix = 'wwexecution',

    [string[]] $SpaRedirectUris = @(
        'http://localhost:4200',
        'http://localhost:3000'
    ),

    [string[]] $WebRedirectUris = @(
        'https://localhost:5001/signin-oidc'
    ),

    [switch] $DaemonUseManagedIdentity,

    [ValidateRange(1, 2)]
    [int] $SecretLifetimeYears = 1,

    [string[]] $AppRolesToAssign = @('Permission.Execute', 'Permission.View'),

    [switch] $DryRun,
    [switch] $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ──────────────────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────────────────

function Invoke-Az {
    param([Parameter(Mandatory)][string[]] $Args_)
    $output = & az @Args_ 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "az CLI failed ($LASTEXITCODE): $($output | Out-String)"
    }
    return $output
}

function ConvertFrom-Az {
    param([Parameter(ValueFromPipeline)][object] $Input_)
    begin { $sb = [System.Text.StringBuilder]::new() }
    process { foreach ($l in @($Input_)) { [void]$sb.AppendLine([string]$l) } }
    end {
        $text = $sb.ToString().Trim()
        if ([string]::IsNullOrWhiteSpace($text) -or $text -eq 'null') { return $null }
        $start = [Math]::Max($text.IndexOf('{'), 0)
        $startB = $text.IndexOf('[')
        if ($startB -ge 0 -and ($start -lt 0 -or $startB -lt $start)) { $start = $startB }
        if ($start -gt 0) { $text = $text.Substring($start) }
        return $text | ConvertFrom-Json -Depth 50
    }
}

function Get-OrCreateApp {
    param(
        [string] $DisplayName,
        [string] $SignInAudience = 'AzureADMyOrg'
    )
    $existing = Invoke-Az @('ad','app','list','--display-name',$DisplayName,'--query','[0]','-o','json') | ConvertFrom-Az
    if ($existing) {
        Write-Host "    found existing: $($existing.appId)" -ForegroundColor Green
        return $existing
    }
    Write-Host "    creating: $DisplayName" -ForegroundColor Yellow
    $app = Invoke-Az @('ad','app','create','--display-name',$DisplayName,'--sign-in-audience',$SignInAudience,'-o','json') | ConvertFrom-Az
    Start-Sleep -Seconds 5  # Graph replication
    return $app
}

function Ensure-ServicePrincipal {
    param([string] $AppId)
    $sp = $null
    try { $sp = Invoke-Az @('ad','sp','show','--id',$AppId,'-o','json') | ConvertFrom-Az } catch {}
    if (-not $sp) {
        $sp = Invoke-Az @('ad','sp','create','--id',$AppId,'-o','json') | ConvertFrom-Az
        Start-Sleep -Seconds 5
    }
    return $sp
}

function Grant-DelegatedPermission {
    param([string] $ClientAppId, [string] $ResourceAppId_, [string] $ScopeId)
    try {
        Invoke-Az @(
            'ad','app','permission','add',
            '--id', $ClientAppId,
            '--api', $ResourceAppId_,
            '--api-permissions', "$ScopeId=Scope"
        ) | Out-Null
        Write-Host "    delegated permission added (scope $ScopeId)" -ForegroundColor Green
    } catch {
        if ($_.Exception.Message -match 'already') {
            Write-Host "    delegated permission already exists" -ForegroundColor DarkGray
        } else { throw }
    }
}

function Grant-AppRoleToSP {
    param([string] $SpObjectId, [string] $ResourceSpId, [string] $RoleId)
    $body = @{
        principalId = $SpObjectId
        resourceId  = $ResourceSpId
        appRoleId   = $RoleId
    } | ConvertTo-Json -Compress
    $tmp = [System.IO.Path]::GetTempFileName()
    $body | Set-Content $tmp -Encoding UTF8
    try {
        Invoke-Az @(
            'rest','--method','POST',
            '--url',"https://graph.microsoft.com/v1.0/servicePrincipals/$SpObjectId/appRoleAssignments",
            '--headers','Content-Type=application/json',
            '--body',"@$tmp"
        ) | Out-Null
    } catch {
        if ($_.Exception.Message -match 'already exists|already been assigned') {
            return  # idempotent
        }
        throw
    } finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
}

# ──────────────────────────────────────────────────────────────────────────────
# Resolve resource app metadata
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Resolving resource app ════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    ResourceAppId: $ResourceAppId"
Write-Host "    TenantId:      $TenantId"

$resourceApp = Invoke-Az @('ad','app','show','--id',$ResourceAppId,'-o','json') | ConvertFrom-Az
if (-not $resourceApp) { throw "Resource app '$ResourceAppId' not found in tenant." }

$resourceSpRaw = Invoke-Az @('ad','sp','show','--id',$ResourceAppId,'-o','json') | ConvertFrom-Az
if (-not $resourceSpRaw) { throw "Resource app SP not found. Run Configure-WwExecutionAuth.ps1 first." }
$ResourceSpId = $resourceSpRaw.id

# Find user_impersonation scope ID
$scopeId = $null
$scopes = $resourceApp.api.oauth2PermissionScopes
if ($scopes) {
    foreach ($s in @($scopes)) {
        if ($s.value -eq 'user_impersonation' -and $s.isEnabled -eq $true) {
            $scopeId = $s.id
            break
        }
    }
}
if (-not $scopeId) {
    throw "Resource app does not have an enabled 'user_impersonation' scope. Run Configure-WwExecutionAuth.ps1 Stage 3b."
}
Write-Host "    user_impersonation scope: $scopeId" -ForegroundColor Green

# Resolve app role IDs for daemon assignment
$appRoles = @($resourceSpRaw.appRoles)
$roleIdMap = @{}
foreach ($r in $appRoles) {
    if ($r.isEnabled) { $roleIdMap[$r.value] = $r.id }
}

# ──────────────────────────────────────────────────────────────────────────────
# Plan summary
# ──────────────────────────────────────────────────────────────────────────────

$provisionSpa    = $ClientType -in @('SPA','All')
$provisionWeb    = $ClientType -in @('Confidential','All')
$provisionDaemon = $ClientType -in @('Daemon','All')

Write-Host ""
Write-Host "═══ Plan ══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    Provision SPA:          $provisionSpa"
Write-Host "    Provision Confidential: $provisionWeb"
Write-Host "    Provision Daemon:       $provisionDaemon"
Write-Host "    Daemon MI mode:         $DaemonUseManagedIdentity"
Write-Host "    Secret lifetime:        $SecretLifetimeYears year(s)"
Write-Host "    Roles for daemon:       $($AppRolesToAssign -join ', ')"
Write-Host ""

if ($DryRun) {
    Write-Host "    -DryRun: exiting without changes." -ForegroundColor Yellow
    return
}

# ──────────────────────────────────────────────────────────────────────────────
# Type A — SPA (Public Client, Authorization Code + PKCE)
# ──────────────────────────────────────────────────────────────────────────────

$results = @{}

if ($provisionSpa) {
    Write-Host ""
    Write-Host "═══ Type A — SPA Client ═══════════════════════════════════════════" -ForegroundColor Cyan

    $spaName = "$ClientDisplayNamePrefix-spa"
    $spaApp = Get-OrCreateApp -DisplayName $spaName

    # Set SPA redirect URIs
    $uriArgs = @('ad','app','update','--id',$spaApp.appId)
    $uriArgs += '--public-client-redirect-uris'
    $uriArgs += $SpaRedirectUris
    Invoke-Az $uriArgs | Out-Null

    # Also set as SPA platform (for PKCE)
    $spaBody = @{ spa = @{ redirectUris = $SpaRedirectUris } } | ConvertTo-Json -Depth 4
    $tmp = [System.IO.Path]::GetTempFileName()
    $spaBody | Set-Content $tmp -Encoding UTF8
    try {
        Invoke-Az @(
            'rest','--method','PATCH',
            '--url',"https://graph.microsoft.com/v1.0/applications/$($spaApp.id)",
            '--headers','Content-Type=application/json',
            '--body',"@$tmp"
        ) | Out-Null
    } finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
    Write-Host "    SPA redirect URIs: $($SpaRedirectUris -join ', ')" -ForegroundColor Green

    # Grant delegated user_impersonation
    Grant-DelegatedPermission -ClientAppId $spaApp.appId -ResourceAppId_ $ResourceAppId -ScopeId $scopeId

    # Admin consent
    try {
        Invoke-Az @('ad','app','permission','admin-consent','--id',$spaApp.appId) | Out-Null
        Write-Host "    admin consent granted" -ForegroundColor Green
    } catch {
        Write-Warning "    admin consent failed (may require Global Admin): $($_.Exception.Message)"
    }

    $results['SPA'] = @{
        DisplayName = $spaName
        ClientId    = $spaApp.appId
        RedirectUris = $SpaRedirectUris
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Type B — Confidential Web App / API (Authorization Code + OBO)
# ──────────────────────────────────────────────────────────────────────────────

if ($provisionWeb) {
    Write-Host ""
    Write-Host "═══ Type B — Confidential Web App ═════════════════════════════════" -ForegroundColor Cyan

    $webName = "$ClientDisplayNamePrefix-web"
    $webApp = Get-OrCreateApp -DisplayName $webName

    # Set web redirect URIs
    Invoke-Az @('ad','app','update','--id',$webApp.appId,'--web-redirect-uris',($WebRedirectUris -join ' ')) | Out-Null
    Write-Host "    Web redirect URIs: $($WebRedirectUris -join ', ')" -ForegroundColor Green

    # Create client secret
    $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
    $cred = Invoke-Az @(
        'ad','app','credential','reset',
        '--id',$webApp.appId,
        '--display-name',"client-secret-$(Get-Date -Format yyyyMMdd)",
        '--end-date',$endDate,
        '--append','-o','json'
    ) | ConvertFrom-Az
    Write-Host "    client secret created (expires $endDate)" -ForegroundColor Green

    # Grant delegated user_impersonation
    Grant-DelegatedPermission -ClientAppId $webApp.appId -ResourceAppId_ $ResourceAppId -ScopeId $scopeId

    # Also add application permission for server-to-server
    foreach ($roleName in $AppRolesToAssign) {
        if ($roleIdMap.ContainsKey($roleName)) {
            try {
                Invoke-Az @(
                    'ad','app','permission','add',
                    '--id', $webApp.appId,
                    '--api', $ResourceAppId,
                    '--api-permissions', "$($roleIdMap[$roleName])=Role"
                ) | Out-Null
            } catch {
                if (-not ($_.Exception.Message -match 'already')) { throw }
            }
        }
    }

    # Admin consent
    try {
        Invoke-Az @('ad','app','permission','admin-consent','--id',$webApp.appId) | Out-Null
        Write-Host "    admin consent granted" -ForegroundColor Green
    } catch {
        Write-Warning "    admin consent failed: $($_.Exception.Message)"
    }

    $results['Confidential'] = @{
        DisplayName  = $webName
        ClientId     = $webApp.appId
        ClientSecret = $cred.password
        SecretExpiry = $endDate
        RedirectUris = $WebRedirectUris
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Type C — Daemon / Service (Client Credentials or MI)
# ──────────────────────────────────────────────────────────────────────────────

if ($provisionDaemon) {
    Write-Host ""
    Write-Host "═══ Type C — Daemon / Service ═════════════════════════════════════" -ForegroundColor Cyan

    $daemonName = "$ClientDisplayNamePrefix-daemon"
    $daemonApp = Get-OrCreateApp -DisplayName $daemonName

    $daemonSecret = $null
    if (-not $DaemonUseManagedIdentity) {
        $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
        $cred = Invoke-Az @(
            'ad','app','credential','reset',
            '--id',$daemonApp.appId,
            '--display-name',"daemon-secret-$(Get-Date -Format yyyyMMdd)",
            '--end-date',$endDate,
            '--append','-o','json'
        ) | ConvertFrom-Az
        $daemonSecret = $cred.password
        Write-Host "    client secret created (expires $endDate)" -ForegroundColor Green
    } else {
        Write-Host "    MI mode — no client secret created" -ForegroundColor Green
        Write-Host "    Assign the MI's SP to app roles using:" -ForegroundColor DarkGray
        Write-Host "      az rest --method POST --url 'https://graph.microsoft.com/v1.0/servicePrincipals/<MI_OID>/appRoleAssignments' ..." -ForegroundColor DarkGray
    }

    # Ensure SP exists
    $daemonSp = Ensure-ServicePrincipal -AppId $daemonApp.appId

    # Assign app roles to the daemon SP
    foreach ($roleName in $AppRolesToAssign) {
        if (-not $roleIdMap.ContainsKey($roleName)) {
            Write-Warning "    role '$roleName' not found on resource app — skipping"
            continue
        }
        Grant-AppRoleToSP -SpObjectId $daemonSp.id -ResourceSpId $ResourceSpId -RoleId $roleIdMap[$roleName]
        Write-Host "    + assigned $roleName to $daemonName SP" -ForegroundColor Green
    }

    $results['Daemon'] = @{
        DisplayName  = $daemonName
        ClientId     = $daemonApp.appId
        ClientSecret = $daemonSecret
        UseMI        = [bool]$DaemonUseManagedIdentity
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Output summary
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Summary ═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$outputFile = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-Clients.output.json'
$output = [pscustomobject]@{
    Timestamp     = (Get-Date).ToString('o')
    TenantId      = $TenantId
    ResourceAppId = $ResourceAppId
    Scope         = "api://$ResourceAppId/.default"
    Authority     = "https://login.microsoftonline.com/$TenantId"
    Clients       = $results
}
$output | ConvertTo-Json -Depth 6 | Set-Content $outputFile -Encoding UTF8
Write-Host "    Output: $outputFile" -ForegroundColor Green

Write-Host ""
Write-Host "  ┌─────────────────────────────────────────────────────────────────┐"
Write-Host "  │ Next steps:                                                     │"
Write-Host "  │  1. Use Get-WwExecutionToken.ps1 to acquire tokens              │"
Write-Host "  │  2. See Example-ClientApps-OrdersSales.ps1 for E2E demo         │"
Write-Host "  │  3. Store secrets in Key Vault for production                   │"
Write-Host "  └─────────────────────────────────────────────────────────────────┘"
Write-Host ""
