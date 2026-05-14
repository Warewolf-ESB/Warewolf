<#
.SYNOPSIS
  Acquires tokens for the Warewolf wwexecution Function App and demonstrates
  calling secured endpoints.  Includes PowerShell and curl examples for every
  supported grant type.

.DESCRIPTION
  This script provides ready-to-run examples for:
    1. Client Credentials (daemon / service)
    2. Device Code Flow (interactive, headless terminals)
    3. ROPC — Resource Owner Password Credentials (legacy/test only)
    4. Curl equivalents for all of the above
    5. Calling /secure/* and /services/* endpoints with the acquired token
    6. Token decode helper

.PARAMETER TenantId
  Entra ID tenant GUID.

.PARAMETER ResourceAppId
  Application (client) ID of the wwexecution resource app.

.PARAMETER ClientId
  Client app ID (daemon, web, or SPA registration).

.PARAMETER ClientSecret
  Client secret for confidential client flows.  Not needed for device code.

.PARAMETER FunctionAppUrl
  Base URL of the deployed function app.
  Default: https://<inferred-from-resource>.azurewebsites.net

.PARAMETER WorkflowName
  Workflow to call in the demo requests.  Default: "Hello World"

.EXAMPLE
  # Client credentials (daemon)
  ./Get-WwExecutionToken.ps1 `
      -TenantId "22222222-..." -ResourceAppId "11111111-..." `
      -ClientId "33333333-..." -ClientSecret "s3cr3t" `
      -GrantType ClientCredentials

.EXAMPLE
  # Device code (interactive)
  ./Get-WwExecutionToken.ps1 `
      -TenantId "22222222-..." -ResourceAppId "11111111-..." `
      -ClientId "33333333-..." -GrantType DeviceCode
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $TenantId,
    [Parameter(Mandatory)][string] $ResourceAppId,
    [Parameter(Mandatory)][string] $ClientId,
    [string] $ClientSecret,
    [string] $FunctionAppUrl,

    [ValidateSet('ClientCredentials','DeviceCode','ROPC')]
    [string] $GrantType = 'ClientCredentials',

    [string] $WorkflowName = 'Hello World',

    # ROPC only (test/dev — NOT for production)
    [string] $Username,
    [string] $Password
)

$ErrorActionPreference = 'Stop'

$TokenEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
$DeviceCodeEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/devicecode"
$Scope = "api://$ResourceAppId/.default"

if (-not $FunctionAppUrl) {
    $FunctionAppUrl = "https://wwexecution.azurewebsites.net"
    Write-Host "  [info] Using default FunctionAppUrl: $FunctionAppUrl" -ForegroundColor DarkGray
    Write-Host "         Override with -FunctionAppUrl parameter" -ForegroundColor DarkGray
}

# ══════════════════════════════════════════════════════════════════════════════
# Helper: Decode JWT payload (no signature verification — for inspection only)
# ══════════════════════════════════════════════════════════════════════════════

function Decode-JwtPayload {
    param([string] $Token)
    $parts = $Token.Split('.')
    if ($parts.Length -lt 2) { throw "Not a valid JWT" }
    $payload = $parts[1].Replace('-','+').Replace('_','/')
    $pad = 4 - ($payload.Length % 4)
    if ($pad -ne 4) { $payload += ('=' * $pad) }
    $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
    return $json | ConvertFrom-Json
}

# ══════════════════════════════════════════════════════════════════════════════
# 1. CLIENT CREDENTIALS FLOW (Daemon / Service)
# ══════════════════════════════════════════════════════════════════════════════

if ($GrantType -eq 'ClientCredentials') {
    if (-not $ClientSecret) { throw "-ClientSecret is required for ClientCredentials grant." }

    Write-Host ""
    Write-Host "═══ Client Credentials Flow ═══════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # ── PowerShell ─────────────────────────────────────────────────────────────
    Write-Host "  [PowerShell] Acquiring token..." -ForegroundColor White

    $body = @{
        grant_type    = 'client_credentials'
        client_id     = $ClientId
        client_secret = $ClientSecret
        scope         = $Scope
    }

    $tokenResponse = Invoke-RestMethod -Method POST -Uri $TokenEndpoint `
        -ContentType 'application/x-www-form-urlencoded' -Body $body

    $accessToken = $tokenResponse.access_token
    Write-Host "  ✓ Token acquired (expires_in: $($tokenResponse.expires_in)s)" -ForegroundColor Green

    # Decode and display claims
    $claims = Decode-JwtPayload $accessToken
    Write-Host ""
    Write-Host "  Token claims:" -ForegroundColor DarkGray
    Write-Host "    aud:   $($claims.aud)"
    Write-Host "    iss:   $($claims.iss)"
    Write-Host "    roles: $($claims.roles -join ', ')"
    Write-Host "    exp:   $([DateTimeOffset]::FromUnixTimeSeconds($claims.exp).LocalDateTime)"

    # ── Curl equivalent ────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "  [Curl equivalent]:" -ForegroundColor DarkGray
    Write-Host @"
    curl -X POST "$TokenEndpoint" \
      -H "Content-Type: application/x-www-form-urlencoded" \
      -d "grant_type=client_credentials&client_id=$ClientId&client_secret=<SECRET>&scope=$Scope"
"@ -ForegroundColor DarkGray
}

# ══════════════════════════════════════════════════════════════════════════════
# 2. DEVICE CODE FLOW (Interactive — headless terminals, CI with user)
# ══════════════════════════════════════════════════════════════════════════════

if ($GrantType -eq 'DeviceCode') {
    Write-Host ""
    Write-Host "═══ Device Code Flow ══════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # Step 1: Request device code
    $dcBody = @{
        client_id = $ClientId
        scope     = $Scope
    }
    $dcResponse = Invoke-RestMethod -Method POST -Uri $DeviceCodeEndpoint `
        -ContentType 'application/x-www-form-urlencoded' -Body $dcBody

    Write-Host "  ┌─────────────────────────────────────────────────────────────────┐" -ForegroundColor Yellow
    Write-Host "  │ ACTION REQUIRED:                                                │" -ForegroundColor Yellow
    Write-Host "  │  1. Open: $($dcResponse.verification_uri)" -ForegroundColor Yellow
    Write-Host "  │  2. Enter code: $($dcResponse.user_code)                        │" -ForegroundColor Yellow
    Write-Host "  └─────────────────────────────────────────────────────────────────┘" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Polling for authorization..." -ForegroundColor DarkGray

    # Step 2: Poll token endpoint
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
    Write-Host "  ✓ Token acquired!" -ForegroundColor Green

    $claims = Decode-JwtPayload $accessToken
    Write-Host "    sub:   $($claims.preferred_username ?? $claims.oid)"
    Write-Host "    roles: $($claims.roles -join ', ')"
    Write-Host "    scp:   $($claims.scp)"

    # ── Curl equivalent ────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "  [Curl equivalent — step 1: get device code]:" -ForegroundColor DarkGray
    Write-Host @"
    curl -X POST "$DeviceCodeEndpoint" \
      -d "client_id=$ClientId&scope=$Scope"
"@ -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  [Curl equivalent — step 2: poll for token]:" -ForegroundColor DarkGray
    Write-Host @"
    curl -X POST "$TokenEndpoint" \
      -d "grant_type=urn:ietf:params:oauth:grant-type:device_code&client_id=$ClientId&device_code=<DEVICE_CODE>"
"@ -ForegroundColor DarkGray
}

# ══════════════════════════════════════════════════════════════════════════════
# 3. ROPC FLOW (Resource Owner Password Credentials — TEST/DEV ONLY)
# ══════════════════════════════════════════════════════════════════════════════

if ($GrantType -eq 'ROPC') {
    if (-not $Username -or -not $Password) { throw "-Username and -Password required for ROPC." }
    if (-not $ClientSecret) { throw "-ClientSecret required for ROPC (confidential client)." }

    Write-Host ""
    Write-Host "═══ ROPC Flow (TEST/DEV ONLY — NOT for production) ════════════════" -ForegroundColor Red
    Write-Host ""

    $body = @{
        grant_type    = 'password'
        client_id     = $ClientId
        client_secret = $ClientSecret
        scope         = $Scope
        username      = $Username
        password      = $Password
    }

    $tokenResponse = Invoke-RestMethod -Method POST -Uri $TokenEndpoint `
        -ContentType 'application/x-www-form-urlencoded' -Body $body

    $accessToken = $tokenResponse.access_token
    Write-Host "  ✓ Token acquired" -ForegroundColor Green

    $claims = Decode-JwtPayload $accessToken
    Write-Host "    user:  $($claims.preferred_username)"
    Write-Host "    roles: $($claims.roles -join ', ')"

    # ── Curl equivalent ────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "  [Curl equivalent]:" -ForegroundColor DarkGray
    Write-Host @"
    curl -X POST "$TokenEndpoint" \
      -d "grant_type=password&client_id=$ClientId&client_secret=<SECRET>&scope=$Scope&username=<USER>&password=<PASS>"
"@ -ForegroundColor DarkGray
}

# ══════════════════════════════════════════════════════════════════════════════
# 4. CALL SECURED ENDPOINTS
# ══════════════════════════════════════════════════════════════════════════════

if ($accessToken) {
    Write-Host ""
    Write-Host "═══ Calling Secured Endpoints ═════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    $encodedWorkflow = [Uri]::EscapeDataString($WorkflowName)
    $headers = @{ Authorization = "Bearer $accessToken" }

    # ── Call /secure/{workflow}.json ───────────────────────────────────────────
    $secureUrl = "$FunctionAppUrl/secure/$encodedWorkflow.json"
    Write-Host "  [PowerShell] GET $secureUrl" -ForegroundColor White
    try {
        $resp = Invoke-RestMethod -Uri $secureUrl -Headers $headers -Method GET
        Write-Host "  ✓ 200 OK" -ForegroundColor Green
        Write-Host "    Response: $($resp | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor DarkGray
    } catch {
        $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { '?' }
        Write-Host "  ✗ HTTP $code" -ForegroundColor Red
        if ($code -eq 403) {
            Write-Host "    → Check secure.config WindowsGroupPermissions for '$WorkflowName'" -ForegroundColor Yellow
        }
    }

    # ── Call /services/{workflow}.json ─────────────────────────────────────────
    $servicesUrl = "$FunctionAppUrl/services/$encodedWorkflow.json"
    Write-Host ""
    Write-Host "  [PowerShell] POST $servicesUrl" -ForegroundColor White
    try {
        $resp = Invoke-RestMethod -Uri $servicesUrl -Headers $headers -Method POST `
            -ContentType 'application/json' -Body '{}'
        Write-Host "  ✓ 200 OK" -ForegroundColor Green
        Write-Host "    Response: $($resp | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor DarkGray
    } catch {
        $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { '?' }
        Write-Host "  ✗ HTTP $code" -ForegroundColor Red
    }

    # ── Curl equivalents ──────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "  [Curl equivalents]:" -ForegroundColor DarkGray
    Write-Host @"

    # GET /secure/{workflow}
    curl -H "Authorization: Bearer <TOKEN>" \
      "$secureUrl"

    # POST /services/{workflow} with JSON body
    curl -X POST \
      -H "Authorization: Bearer <TOKEN>" \
      -H "Content-Type: application/json" \
      -d '{"Name": "World"}' \
      "$servicesUrl"

    # POST /secure/{workflow} with form data
    curl -X POST \
      -H "Authorization: Bearer <TOKEN>" \
      -d "Name=World&Id=42" \
      "$FunctionAppUrl/secure/$encodedWorkflow.json"

"@ -ForegroundColor DarkGray

    # ── One-liner: acquire + call ─────────────────────────────────────────────
    Write-Host "  [Curl one-liner — acquire token & call in single pipeline]:" -ForegroundColor DarkGray
    Write-Host @"

    TOKEN=$$(curl -s -X POST "$TokenEndpoint" \
      -d "grant_type=client_credentials&client_id=$ClientId&client_secret=<SECRET>&scope=$Scope" \
      | jq -r '.access_token')

    curl -H "Authorization: Bearer $$TOKEN" "$secureUrl"

"@ -ForegroundColor DarkGray
}

# ══════════════════════════════════════════════════════════════════════════════
# 5. TOKEN INSPECTION REFERENCE
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══ Token Decode Reference ════════════════════════════════════════" -ForegroundColor Cyan
Write-Host @"

  # PowerShell — decode any JWT (no verification)
  `$token = "<paste-token-here>"
  `$payload = `$token.Split('.')[1].Replace('-','+').Replace('_','/')
  `$pad = 4 - (`$payload.Length % 4); if (`$pad -ne 4) { `$payload += '=' * `$pad }
  [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(`$payload)) | ConvertFrom-Json

  # Bash — decode with jq
  echo "<TOKEN>" | cut -d. -f2 | base64 -d 2>/dev/null | jq .

  # Online (non-production tokens only)
  https://jwt.ms

"@ -ForegroundColor DarkGray

Write-Host "═══ Done ══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
