# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
  End-to-end example: provisions two client apps (Orders SPA, Sales Daemon),
  acquires tokens, and calls the wwexecution Function App secured endpoints.

.DESCRIPTION
  Demonstrates a realistic multi-client scenario:

    1. "Contoso-Orders" — SPA client (browser app) that calls /secure/ProcessOrder
       using delegated user_impersonation via device-code flow (simulates browser).
    2. "Contoso-Sales"  — Background daemon that calls /secure/SyncSales using
       client-credentials (app-only) tokens.

  The script performs:
    • Azure provisioning (Entra app registrations + role assignments)
    • Token acquisition for both clients
    • HTTP calls to the function app
    • Token inspection and error handling
    • Optional cleanup of created registrations

.PARAMETER TenantId
  Entra ID tenant GUID.

.PARAMETER ResourceAppId
  wwexecution resource app client ID (from Configure-WwExecutionAuth.ps1).

.PARAMETER FunctionAppUrl
  Base URL of the function app.  E.g. https://wwexecution.azurewebsites.net

.PARAMETER Cleanup
  When set, removes the example app registrations instead of creating them.

.PARAMETER SkipProvisioning
  Skip Entra provisioning (use when apps are already created from a prior run).
  Reads client IDs from the output file.

.PARAMETER DryRun
  Print the plan without making changes.

.EXAMPLE
  # Full end-to-end
  ./Example-ClientApps-OrdersSales.ps1 `
      -TenantId "22222222-..." `
      -ResourceAppId "11111111-..." `
      -FunctionAppUrl "https://wwexecution.azurewebsites.net"

.EXAMPLE
  # Cleanup
  ./Example-ClientApps-OrdersSales.ps1 `
      -TenantId "22222222-..." `
      -ResourceAppId "11111111-..." `
      -Cleanup
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $TenantId,
    [Parameter(Mandatory)][string] $ResourceAppId,
    [string] $FunctionAppUrl = 'https://wwexecution.azurewebsites.net',
    [switch] $Cleanup,
    [switch] $SkipProvisioning,
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$OutputFile = Join-Path $PSScriptRoot 'Example-OrdersSales.output.json'
$TokenEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
$DeviceCodeEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/devicecode"
$Scope = "api://$ResourceAppId/.default"

# ──────────────────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────────────────

function Invoke-Az {
    param([Parameter(Mandatory)][string[]] $A)
    $out = & az @A 2>&1
    if ($LASTEXITCODE -ne 0) { throw "az failed: $($out | Out-String)" }
    return $out
}

function Parse-Az {
    param([Parameter(ValueFromPipeline)][object] $I)
    begin { $sb = [System.Text.StringBuilder]::new() }
    process { foreach ($l in @($I)) { [void]$sb.AppendLine([string]$l) } }
    end {
        $t = $sb.ToString().Trim()
        if ([string]::IsNullOrWhiteSpace($t) -or $t -eq 'null') { return $null }
        $s = $t.IndexOf('{'); $b = $t.IndexOf('[')
        $start = if ($s -ge 0 -and ($b -lt 0 -or $s -lt $b)) { $s } elseif ($b -ge 0) { $b } else { 0 }
        if ($start -gt 0) { $t = $t.Substring($start) }
        return $t | ConvertFrom-Json -Depth 50
    }
}

function Decode-Jwt {
    param([string] $Token)
    $p = $Token.Split('.')[1].Replace('-','+').Replace('_','/')
    $pad = 4 - ($p.Length % 4); if ($pad -ne 4) { $p += '=' * $pad }
    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p)) | ConvertFrom-Json
}

# ══════════════════════════════════════════════════════════════════════════════
# CLEANUP MODE
# ══════════════════════════════════════════════════════════════════════════════

if ($Cleanup) {
    Write-Host ""
    Write-Host "═══ Cleanup — Removing example client apps ════════════════════════" -ForegroundColor Red
    foreach ($name in @('contoso-orders-spa', 'contoso-sales-daemon')) {
        $app = Invoke-Az @('ad','app','list','--display-name',$name,'--query','[0]','-o','json') | Parse-Az
        if ($app) {
            if (-not $DryRun) {
                Invoke-Az @('ad','app','delete','--id',$app.appId) | Out-Null
                Write-Host "    deleted: $name ($($app.appId))" -ForegroundColor Yellow
            } else {
                Write-Host "    [dry-run] would delete: $name ($($app.appId))" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "    not found: $name (already deleted?)" -ForegroundColor DarkGray
        }
    }
    if (Test-Path $OutputFile) { Remove-Item $OutputFile -Force }
    Write-Host "    Done." -ForegroundColor Green
    return
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 1: AZURE PROVISIONING
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Example: Orders (SPA) + Sales (Daemon) Client Apps            ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Resource App:  $ResourceAppId"
Write-Host "  Tenant:        $TenantId"
Write-Host "  Function URL:  $FunctionAppUrl"
Write-Host "  Scope:         $Scope"
Write-Host ""

$ordersClientId = $null
$salesClientId  = $null
$salesSecret    = $null

if ($SkipProvisioning -and (Test-Path $OutputFile)) {
    $prior = Get-Content $OutputFile -Raw | ConvertFrom-Json
    $ordersClientId = $prior.Orders.ClientId
    $salesClientId  = $prior.Sales.ClientId
    $salesSecret    = $prior.Sales.ClientSecret
    Write-Host "  [skip] Using previously provisioned apps from output file" -ForegroundColor DarkGray
} elseif (-not $SkipProvisioning) {

    if ($DryRun) {
        Write-Host "  [DryRun] Would create:" -ForegroundColor Yellow
        Write-Host "    • contoso-orders-spa   (SPA, device-code, delegated user_impersonation)"
        Write-Host "    • contoso-sales-daemon (confidential, client-credentials, Permission.Execute + Permission.View)"
        Write-Host ""
        Write-Host "  Exiting." -ForegroundColor Yellow
        return
    }

    # ── Resolve resource app scope + roles ────────────────────────────────────
    $resApp = Invoke-Az @('ad','app','show','--id',$ResourceAppId,'-o','json') | Parse-Az
    $resSp  = Invoke-Az @('ad','sp','show','--id',$ResourceAppId,'-o','json') | Parse-Az

    $uiScopeId = $null
    foreach ($s in @($resApp.api.oauth2PermissionScopes)) {
        if ($s.value -eq 'user_impersonation' -and $s.isEnabled) { $uiScopeId = $s.id; break }
    }
    if (-not $uiScopeId) { throw "Resource app missing user_impersonation scope." }

    $execRoleId = $null; $viewRoleId = $null
    foreach ($r in @($resSp.appRoles)) {
        if ($r.value -eq 'Permission.Execute' -and $r.isEnabled) { $execRoleId = $r.id }
        if ($r.value -eq 'Permission.View'    -and $r.isEnabled) { $viewRoleId = $r.id }
    }

    # ── 1A. Contoso Orders — SPA ─────────────────────────────────────────────
    Write-Host "═══ Provisioning: contoso-orders-spa ══════════════════════════════" -ForegroundColor Cyan

    $ordersApp = Invoke-Az @('ad','app','list','--display-name','contoso-orders-spa','--query','[0]','-o','json') | Parse-Az
    if (-not $ordersApp) {
        Write-Host "    creating..." -ForegroundColor Yellow
        $ordersApp = Invoke-Az @(
            'ad','app','create',
            '--display-name','contoso-orders-spa',
            '--sign-in-audience','AzureADMyOrg',
            '--is-fallback-public-client','true',
            '-o','json'
        ) | Parse-Az
        Start-Sleep -Seconds 5
    } else {
        Write-Host "    found existing: $($ordersApp.appId)" -ForegroundColor Green
    }
    $ordersClientId = $ordersApp.appId

    # Set SPA platform redirect URIs
    $spaBody = @{ spa = @{ redirectUris = @('http://localhost:3000/auth/callback') } } | ConvertTo-Json -Depth 4
    $tmp = [System.IO.Path]::GetTempFileName()
    $spaBody | Set-Content $tmp -Encoding UTF8
    Invoke-Az @('rest','--method','PATCH','--url',"https://graph.microsoft.com/v1.0/applications/$($ordersApp.id)",'--headers','Content-Type=application/json','--body',"@$tmp") | Out-Null
    Remove-Item $tmp -Force

    # Grant delegated permission
    try {
        Invoke-Az @('ad','app','permission','add','--id',$ordersClientId,'--api',$ResourceAppId,'--api-permissions',"$uiScopeId=Scope") | Out-Null
    } catch { if (-not ($_.Exception.Message -match 'already')) { throw } }
    try { Invoke-Az @('ad','app','permission','admin-consent','--id',$ordersClientId) | Out-Null } catch {}

    Write-Host "    ✓ contoso-orders-spa: $ordersClientId" -ForegroundColor Green

    # ── 1B. Contoso Sales — Daemon ───────────────────────────────────────────
    Write-Host ""
    Write-Host "═══ Provisioning: contoso-sales-daemon ════════════════════════════" -ForegroundColor Cyan

    $salesApp = Invoke-Az @('ad','app','list','--display-name','contoso-sales-daemon','--query','[0]','-o','json') | Parse-Az
    if (-not $salesApp) {
        Write-Host "    creating..." -ForegroundColor Yellow
        $salesApp = Invoke-Az @(
            'ad','app','create',
            '--display-name','contoso-sales-daemon',
            '--sign-in-audience','AzureADMyOrg',
            '-o','json'
        ) | Parse-Az
        Start-Sleep -Seconds 5
    } else {
        Write-Host "    found existing: $($salesApp.appId)" -ForegroundColor Green
    }
    $salesClientId = $salesApp.appId

    # Create/rotate secret
    $endDate = (Get-Date).AddYears(1).ToString('yyyy-MM-dd')
    $cred = Invoke-Az @(
        'ad','app','credential','reset','--id',$salesClientId,
        '--display-name',"sales-daemon-$(Get-Date -Format yyyyMMdd)",
        '--end-date',$endDate,'--append','-o','json'
    ) | Parse-Az
    $salesSecret = $cred.password
    Write-Host "    secret created (expires $endDate)" -ForegroundColor Green

    # Ensure SP + assign app roles
    $salesSp = $null
    try { $salesSp = Invoke-Az @('ad','sp','show','--id',$salesClientId,'-o','json') | Parse-Az } catch {}
    if (-not $salesSp) {
        $salesSp = Invoke-Az @('ad','sp','create','--id',$salesClientId,'-o','json') | Parse-Az
        Start-Sleep -Seconds 5
    }

    foreach ($roleId in @($execRoleId, $viewRoleId)) {
        if (-not $roleId) { continue }
        $body = @{ principalId=$salesSp.id; resourceId=$resSp.id; appRoleId=$roleId } | ConvertTo-Json -Compress
        $tmp = [System.IO.Path]::GetTempFileName()
        $body | Set-Content $tmp -Encoding UTF8
        try {
            Invoke-Az @('rest','--method','POST','--url',"https://graph.microsoft.com/v1.0/servicePrincipals/$($salesSp.id)/appRoleAssignments",'--headers','Content-Type=application/json','--body',"@$tmp") | Out-Null
        } catch { if (-not ($_.Exception.Message -match 'already')) { throw } }
        finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
    }
    Write-Host "    ✓ contoso-sales-daemon: $salesClientId" -ForegroundColor Green

    # Save output
    @{
        Timestamp   = (Get-Date).ToString('o')
        TenantId    = $TenantId
        ResourceApp = $ResourceAppId
        Orders      = @{ ClientId = $ordersClientId; Type = 'SPA' }
        Sales       = @{ ClientId = $salesClientId; ClientSecret = $salesSecret; Type = 'Daemon' }
    } | ConvertTo-Json -Depth 4 | Set-Content $OutputFile -Encoding UTF8
}

if (-not $ordersClientId -or -not $salesClientId) {
    throw "Client IDs not resolved. Run without -SkipProvisioning or check output file."
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 2: TOKEN ACQUISITION — Orders SPA (Device Code)
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ Token: Orders SPA (Device Code Flow) ══════════════════════════" -ForegroundColor Cyan

$dcResp = Invoke-RestMethod -Method POST -Uri $DeviceCodeEndpoint `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{ client_id = $ordersClientId; scope = $Scope }

Write-Host ""
Write-Host "  ┌─────────────────────────────────────────────────────────────────┐" -ForegroundColor Yellow
Write-Host "  │  Open:  $($dcResp.verification_uri)" -ForegroundColor Yellow
Write-Host "  │  Code:  $($dcResp.user_code)                                   │" -ForegroundColor Yellow
Write-Host "  └─────────────────────────────────────────────────────────────────┘" -ForegroundColor Yellow
Write-Host "  Waiting for user to authenticate..." -ForegroundColor DarkGray

$ordersToken = $null
$interval = if ($dcResp.interval) { $dcResp.interval } else { 5 }
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds $interval
    try {
        $tr = Invoke-RestMethod -Method POST -Uri $TokenEndpoint `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{
                grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                client_id   = $ordersClientId
                device_code = $dcResp.device_code
            }
        $ordersToken = $tr.access_token
        break
    } catch {
        $err = $null
        try { $err = $_.ErrorDetails.Message | ConvertFrom-Json } catch {}
        if ($err.error -eq 'authorization_pending') { continue }
        if ($err.error -eq 'slow_down') { $interval += 5; continue }
        throw
    }
}
if (-not $ordersToken) { throw "Device code timed out for Orders." }

$ordersClaims = Decode-Jwt $ordersToken
Write-Host "  ✓ Orders token acquired" -ForegroundColor Green
Write-Host "    user:  $($ordersClaims.preferred_username)"
Write-Host "    roles: $($ordersClaims.roles -join ', ')"
Write-Host "    scp:   $($ordersClaims.scp)"

# ══════════════════════════════════════════════════════════════════════════════
# STEP 3: TOKEN ACQUISITION — Sales Daemon (Client Credentials)
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ Token: Sales Daemon (Client Credentials) ═════════════════════" -ForegroundColor Cyan

if (-not $salesSecret) { throw "Sales daemon secret not available. Re-run provisioning." }

$salesTokenResp = Invoke-RestMethod -Method POST -Uri $TokenEndpoint `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        grant_type    = 'client_credentials'
        client_id     = $salesClientId
        client_secret = $salesSecret
        scope         = $Scope
    }
$salesToken = $salesTokenResp.access_token

$salesClaims = Decode-Jwt $salesToken
Write-Host "  ✓ Sales token acquired" -ForegroundColor Green
Write-Host "    oid:   $($salesClaims.oid)"
Write-Host "    roles: $($salesClaims.roles -join ', ')"
Write-Host "    exp:   $([DateTimeOffset]::FromUnixTimeSeconds($salesClaims.exp).LocalDateTime)"

# ══════════════════════════════════════════════════════════════════════════════
# STEP 4: CALL FUNCTION APP — Orders (user-delegated)
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ API Call: Orders → /secure/ProcessOrder ═══════════════════════" -ForegroundColor Cyan

$ordersUrl = "$FunctionAppUrl/secure/ProcessOrder.json"
$ordersBody = @{ OrderId = 'ORD-001'; Customer = 'Contoso'; Amount = 250.00 }

Write-Host "  POST $ordersUrl"
Write-Host "  Body: $($ordersBody | ConvertTo-Json -Compress)"
try {
    $resp = Invoke-RestMethod -Uri $ordersUrl -Method POST `
        -Headers @{ Authorization = "Bearer $ordersToken" } `
        -ContentType 'application/json' `
        -Body ($ordersBody | ConvertTo-Json)
    Write-Host "  ✓ 200 OK" -ForegroundColor Green
    Write-Host "    $($resp | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor DarkGray
} catch {
    $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { '?' }
    Write-Host "  ✗ HTTP $code" -ForegroundColor Red
    Write-Host "    $($_.ErrorDetails.Message)" -ForegroundColor DarkGray
    if ($code -eq 401) { Write-Host "    → Token expired or audience mismatch" -ForegroundColor Yellow }
    if ($code -eq 403) { Write-Host "    → Check secure.config: user needs Execute on 'ProcessOrder'" -ForegroundColor Yellow }
    if ($code -eq 404) { Write-Host "    → Workflow 'ProcessOrder' may not exist in the function app" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "  [Curl equivalent]:" -ForegroundColor DarkGray
Write-Host @"
    curl -X POST "$ordersUrl" \
      -H "Authorization: Bearer <ORDERS_TOKEN>" \
      -H "Content-Type: application/json" \
      -d '{"OrderId":"ORD-001","Customer":"Contoso","Amount":250}'
"@ -ForegroundColor DarkGray

# ══════════════════════════════════════════════════════════════════════════════
# STEP 5: CALL FUNCTION APP — Sales Daemon (app-only)
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ API Call: Sales Daemon → /secure/SyncSales ════════════════════" -ForegroundColor Cyan

$salesUrl = "$FunctionAppUrl/secure/SyncSales.json"
$salesBody = @{ Region = 'EMEA'; SinceDate = (Get-Date).AddDays(-7).ToString('yyyy-MM-dd') }

Write-Host "  POST $salesUrl"
Write-Host "  Body: $($salesBody | ConvertTo-Json -Compress)"
try {
    $resp = Invoke-RestMethod -Uri $salesUrl -Method POST `
        -Headers @{ Authorization = "Bearer $salesToken" } `
        -ContentType 'application/json' `
        -Body ($salesBody | ConvertTo-Json)
    Write-Host "  ✓ 200 OK" -ForegroundColor Green
    Write-Host "    $($resp | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor DarkGray
} catch {
    $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { '?' }
    Write-Host "  ✗ HTTP $code" -ForegroundColor Red
    Write-Host "    $($_.ErrorDetails.Message)" -ForegroundColor DarkGray
    if ($code -eq 401) { Write-Host "    → Token expired or aud mismatch" -ForegroundColor Yellow }
    if ($code -eq 403) { Write-Host "    → Daemon SP needs Permission.Execute role + secure.config entry" -ForegroundColor Yellow }
    if ($code -eq 404) { Write-Host "    → Workflow 'SyncSales' may not exist" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "  [Curl equivalent]:" -ForegroundColor DarkGray
Write-Host @"
    # Step 1: Acquire daemon token
    SALES_TOKEN=$$(curl -s -X POST "$TokenEndpoint" \
      -d "grant_type=client_credentials&client_id=$salesClientId&client_secret=<SECRET>&scope=$Scope" \
      | jq -r '.access_token')

    # Step 2: Call the workflow
    curl -X POST "$salesUrl" \
      -H "Authorization: Bearer $$SALES_TOKEN" \
      -H "Content-Type: application/json" \
      -d '{"Region":"EMEA","SinceDate":"$(((Get-Date).AddDays(-7).ToString('yyyy-MM-dd')))"}'
"@ -ForegroundColor DarkGray

# ══════════════════════════════════════════════════════════════════════════════
# STEP 6: TOKEN REFRESH PATTERNS
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ Token Management Patterns ═════════════════════════════════════" -ForegroundColor Cyan
Write-Host @"

  ┌─ Orders SPA (delegated) ──────────────────────────────────────────────────┐
  │ • MSAL.js handles refresh automatically via acquireTokenSilent            │
  │ • Refresh token valid 90 days (sliding window, single-tenant)             │
  │ • On InteractionRequiredAuthError → trigger loginRedirect/loginPopup      │
  │ • Device-code tokens: re-run device code flow on expiry                   │
  └───────────────────────────────────────────────────────────────────────────┘

  ┌─ Sales Daemon (app-only) ─────────────────────────────────────────────────┐
  │ • No refresh token for client_credentials grant                           │
  │ • Simply re-request: POST /token with same grant_type=client_credentials  │
  │ • Token lifetime: 60-90 min (Entra default, not configurable per-app)     │
  │ • MSAL.NET / Azure.Identity cache tokens and refresh transparently        │
  │ • For long-running workers: cache token, check exp before each call       │
  └───────────────────────────────────────────────────────────────────────────┘

  ┌─ Error handling ──────────────────────────────────────────────────────────┐
  │ HTTP 401 → token expired/invalid → re-acquire, retry once                │
  │ HTTP 403 → token valid but policy denied → fix secure.config / roles     │
  │ HTTP 429 → Entra throttling → exponential backoff, increase cache TTL    │
  └───────────────────────────────────────────────────────────────────────────┘

"@ -ForegroundColor DarkGray

# ══════════════════════════════════════════════════════════════════════════════
# DONE
# ══════════════════════════════════════════════════════════════════════════════

Write-Host "═══ Summary ═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Orders SPA:     $ordersClientId  (delegated, device-code)"
Write-Host "  Sales Daemon:   $salesClientId  (app-only, client-credentials)"
Write-Host "  Function URL:   $FunctionAppUrl"
Write-Host "  Output file:    $OutputFile"
Write-Host ""
Write-Host "  To clean up:  ./Example-ClientApps-OrdersSales.ps1 -TenantId ... -ResourceAppId ... -Cleanup"
Write-Host ""
Write-Host "═══ Done ══════════════════════════════════════════════════════════" -ForegroundColor Cyan
