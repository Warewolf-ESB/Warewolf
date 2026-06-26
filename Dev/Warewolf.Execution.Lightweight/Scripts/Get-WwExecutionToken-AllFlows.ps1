# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
  Comprehensive token acquisition and Azure Function calling examples for the
  Warewolf wwexecution Function App.  Covers every supported OAuth 2.0 grant
  type and client scenario.

.DESCRIPTION
  This script is a self-contained reference for ALL supported authentication
  flows against the wwexecution Azure Function App secured with Entra ID
  Easy Auth.

  ┌─────────────────────────────────────────────────────────────────────────┐
  │  Flow                        │ Client Type           │ Section          │
  ├─────────────────────────────────────────────────────────────────────────┤
  │  1. Browser Redirect         │ Any browser           │ Section A        │
  │  2. Client Credentials       │ Daemon / Service      │ Section B        │
  │  3. Device Code              │ CLI / Interactive     │ Section C        │
  │  4. Auth Code + PKCE         │ SPA (simulated)       │ Section D        │
  │  5. Auth Code (confidential) │ Web App               │ Section E        │
  │  6. On-Behalf-Of (OBO)       │ Web App → API         │ Section F        │
  │  7. Managed Identity         │ Azure-hosted service  │ Section G        │
  │  8. Federated Identity Cred  │ GitHub Actions / AKS  │ Section H        │
  │  9. ROPC (legacy/test only)  │ Username + Password   │ Section I        │
  └─────────────────────────────────────────────────────────────────────────┘

  Each section shows:
    • PowerShell (Invoke-RestMethod)
    • curl.exe (bash/WSL/Git-Bash compatible)
    • az CLI shortcut where applicable

  IMPORTANT: In PowerShell, `curl` is an alias for Invoke-WebRequest.
  Always use `curl.exe` explicitly or use `Invoke-RestMethod`.

.PARAMETER TenantId
  Entra ID tenant GUID.  Read from output JSON when omitted.

.PARAMETER ResourceAppId
  Application (client) ID of the wwexecution resource app.

.PARAMETER FunctionAppName
  Name of the Azure Function App (used to build the base URL).

.PARAMETER Section
  Run only a specific section (A-I).  Default: prints all examples as
  documentation without executing them.

.PARAMETER Execute
  When combined with -Section, actually runs the HTTP calls.
  Without -Execute the script only prints the example commands.

.PARAMETER WorkflowName
  Workflow to call in demo requests.  Default: "Hello World"

.EXAMPLE
  # Print all examples (documentation mode)
  ./Get-WwExecutionToken-AllFlows.ps1

.EXAMPLE
  # Execute Client Credentials flow (Section B) against live function app
  ./Get-WwExecutionToken-AllFlows.ps1 -Section B -Execute `
      -TenantId "..." -ResourceAppId "..." -FunctionAppName "wwexecution"

.EXAMPLE
  # Execute Device Code flow (Section C)
  ./Get-WwExecutionToken-AllFlows.ps1 -Section C -Execute `
      -TenantId "..." -ResourceAppId "..." -FunctionAppName "wwexecution"
#>

[CmdletBinding()]
param(
    [string] $TenantId,
    [string] $ResourceAppId,
    [string] $FunctionAppName,
    [string] $DaemonClientId,
    [string] $DaemonClientSecret,
    [string] $SpaClientId,
    [string] $WebClientId,
    [string] $WebClientSecret,
    [string] $WorkflowName    = 'Hello World',

    [ValidateSet('A','B','C','D','E','F','G','H','I')]
    [string] $Section,

    [switch] $Execute
)

$ErrorActionPreference = 'Stop'

# ─── Load from output JSON files if values not supplied ──────────────────────

$serverOutput = $null
$clientOutput = $null

$serverOutputPath = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth.output.json'
$clientOutputPath = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth-Clients.output.json'

if (Test-Path $serverOutputPath) {
    $serverOutput = Get-Content $serverOutputPath | ConvertFrom-Json
}
if (Test-Path $clientOutputPath) {
    $clientOutput = Get-Content $clientOutputPath | ConvertFrom-Json
}

if (-not $TenantId          -and $clientOutput) { $TenantId          = $clientOutput.TenantId }
if (-not $ResourceAppId     -and $clientOutput) { $ResourceAppId     = $clientOutput.ResourceAppId }
if (-not $FunctionAppName   -and $serverOutput) { $FunctionAppName   = $serverOutput.FunctionAppName }
if (-not $DaemonClientId    -and $clientOutput) { $DaemonClientId    = $clientOutput.Clients.Daemon.ClientId }
if (-not $DaemonClientSecret -and $clientOutput){ $DaemonClientSecret= $clientOutput.Clients.Daemon.ClientSecret }
if (-not $SpaClientId       -and $clientOutput) { $SpaClientId       = $clientOutput.Clients.SPA.ClientId }
if (-not $WebClientId       -and $clientOutput) { $WebClientId       = $clientOutput.Clients.Confidential.ClientId }
if (-not $WebClientSecret   -and $clientOutput) { $WebClientSecret   = $clientOutput.Clients.Confidential.ClientSecret }

# Derived values
$Scope           = "api://$ResourceAppId/.default"
$Authority       = "https://login.microsoftonline.com/$TenantId"
$TokenEndpoint   = "$Authority/oauth2/v2.0/token"
$DeviceEndpoint  = "$Authority/oauth2/v2.0/devicecode"
$FunctionAppUrl  = "https://$FunctionAppName.azurewebsites.net"
$WorkflowEncoded = [Uri]::EscapeDataString($WorkflowName)

# ─── Helpers ─────────────────────────────────────────────────────────────────

function Write-Header { param([string]$Title) Write-Host "`n$('═'*70)`n  $Title`n$('─'*70)" -ForegroundColor Cyan }
function Write-Sub    { param([string]$Title) Write-Host "`n  ── $Title" -ForegroundColor Yellow }
function Write-Code   { param([string]$Code)
    Write-Host ""
    foreach ($line in $Code -split "`n") { Write-Host "    $line" -ForegroundColor DarkGray }
    Write-Host ""
}

function Invoke-WorkflowGet {
    param([string]$Token, [string]$Route = 'secure')
    $uri = "$FunctionAppUrl/$Route/$WorkflowEncoded.json?Name=PowerShell"
    Write-Host "    GET $uri" -ForegroundColor DarkGray
    try {
        $resp = Invoke-RestMethod -Uri $uri -Headers @{ Authorization = "Bearer $Token" } -Method GET
        Write-Host "    Response: $($resp | ConvertTo-Json -Compress)" -ForegroundColor Green
        return $resp
    } catch {
        Write-Host "    Error ($($_.Exception.Message))" -ForegroundColor Red
    }
}

function Invoke-WorkflowPost {
    param([string]$Token, [string]$Route = 'services', [hashtable]$Body = @{})
    $uri = "$FunctionAppUrl/$Route/$WorkflowEncoded.json"
    Write-Host "    POST $uri" -ForegroundColor DarkGray
    try {
        $resp = Invoke-RestMethod -Uri $uri -Headers @{ Authorization = "Bearer $Token"; 'Content-Type' = 'application/json' } `
                                  -Method POST -Body ($Body | ConvertTo-Json)
        Write-Host "    Response: $($resp | ConvertTo-Json -Compress)" -ForegroundColor Green
        return $resp
    } catch {
        Write-Host "    Error ($($_.Exception.Message))" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION A — Browser Redirect (Easy Auth)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'A') {
    Write-Header "SECTION A — Browser Redirect (Easy Auth)"
    Write-Host @"

  HOW IT WORKS
  ────────────
  Azure Easy Auth intercepts any unauthenticated request to /secure/* or
  /services/* from a browser and issues a 302 redirect to:
      https://login.microsoftonline.com/<tenant>/oauth2/authorize?...

  After the user authenticates, Entra redirects back to:
      https://<function-app>.azurewebsites.net/.auth/login/aad/callback

  Easy Auth then sets the X-MS-CLIENT-PRINCIPAL header (base64-encoded JSON)
  on all subsequent requests within the session.  The function app's
  ClaimsPrincipalBuilderMiddleware reads this header and builds the
  WorkflowClaimsPrincipal automatically.

  /public/* routes bypass authentication entirely.

  ROUTES
  ──────
  Public  (no auth): $FunctionAppUrl/public/$WorkflowEncoded.json
  Secure  (auth req): $FunctionAppUrl/secure/$WorkflowEncoded.json
  Services(auth req): $FunctionAppUrl/services/$WorkflowEncoded.json

"@ -ForegroundColor White

    Write-Sub "A1 — Navigate in Browser (works as-is)"
    Write-Code @"
# Open any of these URLs directly in your browser.
# Easy Auth handles the login redirect automatically.

Public  (no auth):  $FunctionAppUrl/public/$WorkflowEncoded.json?Name=Browser
Secure  (redirects to login): $FunctionAppUrl/secure/$WorkflowEncoded.json?Name=Browser
Services(redirects to login): $FunctionAppUrl/services/$WorkflowEncoded.json?Name=Browser

# After login, Easy Auth redirects back with the token set in the session.
# The /.auth/me endpoint shows the decoded claims:
$FunctionAppUrl/.auth/me
"@

    Write-Sub "A2 — Manual /.auth/login/aad redirect"
    Write-Code @"
# Trigger Easy Auth login flow explicitly.
# post_login_redirect_url is where Easy Auth sends the user after login.
$FunctionAppUrl/.auth/login/aad?post_login_redirect_url=%2Fsecure%2F$WorkflowEncoded.json%3FName%3DBrowser

# To sign out:
$FunctionAppUrl/.auth/logout
"@

    Write-Sub "A3 — Check session claims (PowerShell)"
    Write-Code @"
# After browser login, Easy Auth exposes decoded claims at /.auth/me
# (only accessible from the same browser session via cookies).

# With Invoke-WebRequest (preserves session cookies):
`$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
`$resp    = Invoke-WebRequest -Uri '$FunctionAppUrl/.auth/me' -WebSession `$session
`$resp.Content | ConvertFrom-Json
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION B — Client Credentials (Daemon / Service)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'B') {
    Write-Header "SECTION B — Client Credentials Flow (Daemon / Service)"
    Write-Host @"

  WHEN TO USE: Background services, CI/CD pipelines, automated integrations.
  No user interaction required.  Uses application permissions (app roles).
  The token aud claim = api://<ResourceAppId>
  The token roles claim = app roles assigned to the daemon SP.

  CLIENT REGISTERED AS: $($DaemonClientId ?? '<wwexecution-daemon ClientId>')
  SCOPE:                $Scope

"@ -ForegroundColor White

    Write-Sub "B1 — Acquire Token (PowerShell / Invoke-RestMethod)"
    Write-Code @"
`$TenantId        = '$TenantId'
`$DaemonClientId  = '$($DaemonClientId ?? '<daemon-client-id>')'
`$DaemonSecret    = '<daemon-client-secret>'   # from output JSON / Key Vault
`$Scope           = '$Scope'
`$TokenEndpoint   = 'https://login.microsoftonline.com/`$TenantId/oauth2/v2.0/token'

`$tokenResponse = Invoke-RestMethod ``
    -Method Post ``
    -Uri `$TokenEndpoint ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{
        grant_type    = 'client_credentials'
        client_id     = `$DaemonClientId
        client_secret = `$DaemonSecret
        scope         = `$Scope
    }

`$TOKEN = `$tokenResponse.access_token
Write-Host "Token acquired (first 50 chars): `$(`$TOKEN.Substring(0,50))..."
"@

    Write-Sub "B2 — Call Secure Workflow (PowerShell)"
    Write-Code @"
# GET
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=DaemonService' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }

# POST with JSON body
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/services/$WorkflowEncoded.json' ``
    -Method POST ``
    -Headers @{ Authorization = "Bearer `$TOKEN"; 'Content-Type' = 'application/json' } ``
    -Body '{"OrderId":"ORD-001","Customer":"Contoso"}'

# POST with form-encoded body
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/services/$WorkflowEncoded.json' ``
    -Method POST ``
    -Headers @{ Authorization = "Bearer `$TOKEN" } ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body 'OrderId=ORD-001&Customer=Contoso'
"@

    Write-Sub "B3 — Acquire Token (curl.exe / bash)"
    Write-Code @"
TENANT_ID='$TenantId'
DAEMON_CLIENT_ID='$($DaemonClientId ?? '<daemon-client-id>')'
DAEMON_SECRET='<daemon-client-secret>'
SCOPE='$Scope'

TOKEN=`$(curl.exe -s -X POST \
  "https://login.microsoftonline.com/`${TENANT_ID}/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=`${DAEMON_CLIENT_ID}&client_secret=`${DAEMON_SECRET}&scope=`${SCOPE}" \
  | jq -r '.access_token')
"@

    Write-Sub "B4 — Call Workflow (curl.exe / bash)"
    Write-Code @"
# GET
curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=Daemon"

# POST JSON
curl.exe -X POST \
  -H "Authorization: Bearer `${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"OrderId":"ORD-001"}' \
  "$FunctionAppUrl/services/$WorkflowEncoded.json"

# One-liner: acquire + call
curl.exe -H "Authorization: Bearer `$(curl.exe -s -X POST \
  "https://login.microsoftonline.com/`${TENANT_ID}/oauth2/v2.0/token" \
  -d "grant_type=client_credentials&client_id=`${DAEMON_CLIENT_ID}&client_secret=`${DAEMON_SECRET}&scope=`${SCOPE}" \
  | jq -r '.access_token')" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=OneLiner"
"@

    Write-Sub "B5 — Using az CLI shortcut"
    Write-Code @"
# Acquire token directly via az CLI (useful in CI/CD)
`$TOKEN = az account get-access-token --resource "api://$ResourceAppId" --query accessToken -o tsv

# Or using a service principal login:
az login --service-principal ``
    --tenant '$TenantId' ``
    --username '<daemon-client-id>' ``
    --password '<daemon-client-secret>'

`$TOKEN = az account get-access-token --resource "api://$ResourceAppId" --query accessToken -o tsv
"@

    if ($Execute -and $Section -eq 'B') {
        Write-Host "`n  [EXECUTING Section B]" -ForegroundColor Cyan
        if (-not $DaemonClientId -or -not $DaemonClientSecret) {
            Write-Host "  DaemonClientId / DaemonClientSecret not set - skipping execution." -ForegroundColor Yellow
        } else {
            $tokenResp = Invoke-RestMethod -Method Post -Uri $TokenEndpoint `
                -ContentType 'application/x-www-form-urlencoded' `
                -Body @{ grant_type='client_credentials'; client_id=$DaemonClientId; client_secret=$DaemonClientSecret; scope=$Scope }
            $tok = $tokenResp.access_token
            Write-Host "  Token acquired." -ForegroundColor Green
            Invoke-WorkflowGet -Token $tok
        }
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION C — Device Code Flow (Interactive CLI)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'C') {
    Write-Header "SECTION C — Device Code Flow (Interactive CLI / Headless)"
    Write-Host @"

  WHEN TO USE: CLI tools, admin scripts, testing delegated user tokens in
  headless environments where a browser cannot be launched inline.
  The user opens a URL on any device; the script polls for the token.
  Requires: SPA app registration with isFallbackPublicClient=true
            + user_impersonation scope with admin consent.

  SPA CLIENT: $($SpaClientId ?? '<wwexecution-spa ClientId>')

"@ -ForegroundColor White

    Write-Sub "C1 — Device Code (PowerShell)"
    Write-Code @"
`$TenantId    = '$TenantId'
`$SpaClientId = '$($SpaClientId ?? '<spa-client-id>')'
`$Scope       = '$Scope'
`$Authority   = 'https://login.microsoftonline.com/`$TenantId'

# Step 1: request device code
`$deviceResp = Invoke-RestMethod ``
    -Method Post ``
    -Uri "`$Authority/oauth2/v2.0/devicecode" ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{ client_id = `$SpaClientId; scope = `$Scope }

Write-Host `$deviceResp.message   # "Go to https://microsoft.com/devicelogin and enter code XXXXXXXX"

# Step 2: poll for token
`$TOKEN = `$null
do {
    Start-Sleep -Seconds `$deviceResp.interval
    try {
        `$tokenResp = Invoke-RestMethod ``
            -Method Post ``
            -Uri "`$Authority/oauth2/v2.0/token" ``
            -ContentType 'application/x-www-form-urlencoded' ``
            -Body @{
                grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                client_id   = `$SpaClientId
                device_code = `$deviceResp.device_code
            }
        `$TOKEN = `$tokenResp.access_token
    } catch {
        `$errBody = `$null
        try { `$errBody = `$_.ErrorDetails.Message | ConvertFrom-Json } catch {}
        `$errCode = if (`$errBody) { `$errBody.error } else { '' }

        if (`$errCode -in @('authorization_pending','slow_down')) {
            if (`$errCode -eq 'slow_down') { Start-Sleep -Seconds 5 }
            continue
        }
        `$msg = if (`$errBody) { "`$errCode : `$(`$errBody.error_description)" } else { `$_.Exception.Message }
        Write-Error "Token request failed: `$msg"
        break
    }
} until (`$TOKEN)

# Step 3: call the function
if (`$TOKEN) {
    Invoke-RestMethod ``
        -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=DeviceCodeUser' ``
        -Headers @{ Authorization = "Bearer `$TOKEN" }
}
"@

    Write-Sub "C2 — Device Code (curl.exe / bash)"
    Write-Code @"
TENANT_ID='$TenantId'
SPA_CLIENT_ID='$($SpaClientId ?? '<spa-client-id>')'
SCOPE='$Scope'

# Step 1: request device code
DEVICE=`$(curl.exe -s -X POST \
  "https://login.microsoftonline.com/`${TENANT_ID}/oauth2/v2.0/devicecode" \
  --data-urlencode "client_id=`${SPA_CLIENT_ID}" \
  --data-urlencode "scope=`${SCOPE}")

echo "`$DEVICE" | jq -r '.message'
DEVICE_CODE=`$(echo "`$DEVICE" | jq -r '.device_code')
INTERVAL=`$(echo "`$DEVICE" | jq -r '.interval')

# Step 2: poll
TOKEN=""
while true; do
  sleep "`$INTERVAL"
  RESPONSE=`$(curl.exe -s -X POST \
    "https://login.microsoftonline.com/`${TENANT_ID}/oauth2/v2.0/token" \
    --data-urlencode "grant_type=urn:ietf:params:oauth:grant-type:device_code" \
    --data-urlencode "client_id=`${SPA_CLIENT_ID}" \
    --data-urlencode "device_code=`${DEVICE_CODE}")
  ERR=`$(echo "`$RESPONSE" | jq -r '.error // empty')
  if [ -z "`$ERR" ]; then
    TOKEN=`$(echo "`$RESPONSE" | jq -r '.access_token'); break
  elif [ "`$ERR" = "authorization_pending" ]; then continue
  elif [ "`$ERR" = "slow_down" ]; then INTERVAL=`$((INTERVAL + 5)); continue
  else echo "Fatal: `$ERR — `$(echo "`$RESPONSE" | jq -r '.error_description')" >&2; break
  fi
done

# Step 3: call the function
[ -n "`$TOKEN" ] && curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=DeviceCodeUser"
"@

    if ($Execute -and $Section -eq 'C') {
        Write-Host "`n  [EXECUTING Section C - Device Code]" -ForegroundColor Cyan
        if (-not $SpaClientId) {
            Write-Host "  SpaClientId not set - skipping execution." -ForegroundColor Yellow
        } else {
            $deviceResp = Invoke-RestMethod -Method Post -Uri $DeviceEndpoint `
                -ContentType 'application/x-www-form-urlencoded' `
                -Body @{ client_id=$SpaClientId; scope=$Scope }
            Write-Host $deviceResp.message -ForegroundColor Yellow
            $TOKEN = $null
            do {
                Start-Sleep -Seconds $deviceResp.interval
                try {
                    $tr = Invoke-RestMethod -Method Post -Uri $TokenEndpoint `
                        -ContentType 'application/x-www-form-urlencoded' `
                        -Body @{ grant_type='urn:ietf:params:oauth:grant-type:device_code'; client_id=$SpaClientId; device_code=$deviceResp.device_code }
                    $TOKEN = $tr.access_token
                } catch {
                    $eb = $null; try { $eb = $_.ErrorDetails.Message | ConvertFrom-Json } catch {}
                    $ec = if ($eb) { $eb.error } else { '' }
                    if ($ec -in @('authorization_pending','slow_down')) {
                        if ($ec -eq 'slow_down') { Start-Sleep 5 }; continue
                    }
                    Write-Host "  Fatal: $ec" -ForegroundColor Red; break
                }
            } until ($TOKEN)
            if ($TOKEN) { Invoke-WorkflowGet -Token $TOKEN }
        }
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION D — Authorization Code + PKCE (SPA — simulated non-interactively)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'D') {
    Write-Header "SECTION D — Authorization Code + PKCE (SPA, public client)"
    Write-Host @"

  WHEN TO USE: Single-Page Applications (Angular, React, React Native).
  The browser handles the redirect; the SPA exchanges the code for a token.
  This cannot be fully scripted (the browser redirect requires user interaction),
  but the token exchange step is shown below for reference / testing.

  NOTE: In a real SPA, use MSAL.js (Angular/React) or MSAL React Native.
  The examples in docs/ClientExamples/ show full MSAL integration.

  SPA CLIENT: $($SpaClientId ?? '<wwexecution-spa ClientId>')
  PKCE:       Required (code_challenge / code_verifier)

"@ -ForegroundColor White

    Write-Sub "D1 — Generate PKCE challenge (PowerShell)"
    Write-Code @"
# Generate a cryptographically random code_verifier and derive the challenge.
`$bytes        = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes(`$bytes)
`$CodeVerifier = [Convert]::ToBase64String(`$bytes) -replace '[+/=]',{ @{'+'=>'-'; '/'=>'_'; '='=>''}[`$args[0].Value] }

`$sha256       = [Security.Cryptography.SHA256]::Create()
`$hashBytes    = `$sha256.ComputeHash([Text.Encoding]::ASCII.GetBytes(`$CodeVerifier))
`$CodeChallenge = [Convert]::ToBase64String(`$hashBytes) -replace '[+/=]',{ @{'+'=>'-'; '/'=>'_'; '='=>''}[`$args[0].Value] }

Write-Host "code_verifier  : `$CodeVerifier"
Write-Host "code_challenge : `$CodeChallenge"
"@

    Write-Sub "D2 — Authorization URL (open in browser)"
    Write-Code @"
# Construct the authorization URL.  Open this in a browser.
# Replace <code_challenge> with the value from D1.
`$AuthUrl = 'https://login.microsoftonline.com/$TenantId/oauth2/v2.0/authorize' +
    '?client_id=$($SpaClientId ?? '<spa-client-id>')' +
    '&response_type=code' +
    '&redirect_uri=http%3A%2F%2Flocalhost%3A4200' +
    '&scope=$([Uri]::EscapeDataString($Scope))' +
    '&code_challenge=<code_challenge>' +
    '&code_challenge_method=S256' +
    '&state=random_state_value'

Start-Process `$AuthUrl
# OR open manually: Write-Host `$AuthUrl
"@

    Write-Sub "D3 — Exchange authorization code for token (PowerShell)"
    Write-Code @"
# After the browser redirects to http://localhost:4200?code=<AUTH_CODE>&state=...
# extract the AUTH_CODE and exchange it here.

`$AuthCode    = '<paste-auth-code-from-redirect>'
`$RedirectUri = 'http://localhost:4200'

`$tokenResp = Invoke-RestMethod ``
    -Method Post ``
    -Uri '$TokenEndpoint' ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{
        grant_type    = 'authorization_code'
        client_id     = '$($SpaClientId ?? '<spa-client-id>')'
        code          = `$AuthCode
        redirect_uri  = `$RedirectUri
        code_verifier = `$CodeVerifier   # from D1
        scope         = '$Scope'
    }

`$TOKEN = `$tokenResp.access_token

# Call the function
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=SPAUser' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION E — Authorization Code (Confidential Web App)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'E') {
    Write-Header "SECTION E — Authorization Code (Confidential Web App)"
    Write-Host @"

  WHEN TO USE: Server-rendered web applications (.NET MVC, Node.js, Python).
  The server exchanges the code using a client_secret (no PKCE needed).
  Uses delegated user permissions via user_impersonation scope.

  WEB CLIENT: $($WebClientId ?? '<wwexecution-web ClientId>')

"@ -ForegroundColor White

    Write-Sub "E1 — Token exchange (confidential client, PowerShell)"
    Write-Code @"
`$AuthCode    = '<paste-auth-code-from-redirect>'
`$RedirectUri = 'https://localhost:5001/signin-oidc'

`$tokenResp = Invoke-RestMethod ``
    -Method Post ``
    -Uri '$TokenEndpoint' ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{
        grant_type    = 'authorization_code'
        client_id     = '$($WebClientId ?? '<web-client-id>')'
        client_secret = '<web-client-secret>'
        code          = `$AuthCode
        redirect_uri  = `$RedirectUri
        scope         = '$Scope'
    }

`$TOKEN = `$tokenResp.access_token

# Call the function
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=WebUser' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }
"@

    Write-Sub "E2 — Token exchange (curl.exe)"
    Write-Code @"
AUTH_CODE='<paste-auth-code-from-redirect>'

TOKEN=`$(curl.exe -s -X POST '$TokenEndpoint' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d "grant_type=authorization_code&client_id=$($WebClientId ?? '<web-client-id>')&client_secret=<web-secret>&code=`${AUTH_CODE}&redirect_uri=https%3A%2F%2Flocalhost%3A5001%2Fsignin-oidc&scope=$([Uri]::EscapeDataString($Scope))" \
  | jq -r '.access_token')

curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=WebUser"
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION F — On-Behalf-Of (OBO) — Web App calling Function App as the user
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'F') {
    Write-Header "SECTION F — On-Behalf-Of (OBO) Flow"
    Write-Host @"

  WHEN TO USE: A middle-tier web app (or another Azure Function) has a user
  token for itself, and needs to call wwexecution on behalf of that user.
  The user's identity flows through to the function app.

  Flow: User → Web App (gets token A) → Web App exchanges A for token B
        targeting wwexecution → calls /secure/* with token B.

  Requires: Web App has user_impersonation permission on wwexecution resource
            AND the user has consented.

"@ -ForegroundColor White

    Write-Sub "F1 — OBO exchange (PowerShell)"
    Write-Code @"
# `$UserAccessToken = token the web app received from the user (audience = web app)
`$UserAccessToken = '<user-token-for-web-app>'

`$oboResp = Invoke-RestMethod ``
    -Method Post ``
    -Uri '$TokenEndpoint' ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{
        grant_type            = 'urn:ietf:params:oauth:grant-type:jwt-bearer'
        client_id             = '$($WebClientId ?? '<web-client-id>')'
        client_secret         = '<web-client-secret>'
        assertion             = `$UserAccessToken
        scope                 = '$Scope'
        requested_token_use   = 'on_behalf_of'
    }

`$TOKEN = `$oboResp.access_token

# Call the function with the OBO token (user identity preserved)
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=OBOUser' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }
"@

    Write-Sub "F2 — OBO exchange (curl.exe)"
    Write-Code @"
USER_TOKEN='<user-token-for-web-app>'

TOKEN=`$(curl.exe -s -X POST '$TokenEndpoint' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d "grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&client_id=$($WebClientId ?? '<web-client-id>')&client_secret=<web-secret>&assertion=`${USER_TOKEN}&scope=$([Uri]::EscapeDataString($Scope))&requested_token_use=on_behalf_of" \
  | jq -r '.access_token')

curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=OBOUser"
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION G — Managed Identity (Azure-hosted services)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'G') {
    Write-Header "SECTION G — Managed Identity (Azure VM, ACI, AKS, App Service)"
    Write-Host @"

  WHEN TO USE: Any Azure service (VM, App Service, AKS Pod, ACI) with a
  system-assigned or user-assigned managed identity.
  No secrets to manage - Azure handles credential rotation automatically.

  PREREQUISITES:
  1. Enable system-assigned MI on the Azure resource.
  2. Assign the MI's service principal the desired app roles on the wwexecution
     resource app (Permission.Execute etc.) via Graph API or portal.

"@ -ForegroundColor White

    Write-Sub "G1 — Acquire token from IMDS (PowerShell — run INSIDE Azure VM/App Service)"
    Write-Code @"
# This runs from inside an Azure resource that has a managed identity enabled.
# The IMDS endpoint (169.254.169.254) is only reachable from within Azure.

`$ResourceAppId = '$ResourceAppId'

`$tokenResp = Invoke-RestMethod ``
    -Uri "http://169.254.169.254/metadata/identity/oauth2/token" ``
    -Headers @{ Metadata = 'true' } ``
    -Method GET ``
    -Body @{
        'api-version' = '2018-02-01'
        resource      = "api://`$ResourceAppId"
    }

`$TOKEN = `$tokenResp.access_token

# Call the function
Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=ManagedIdentity' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }
"@

    Write-Sub "G2 — Acquire token from IMDS (curl.exe — inside Azure)"
    Write-Code @"
RESOURCE_APP_ID='$ResourceAppId'

TOKEN=`$(curl.exe -s \
  "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=api%3A%2F%2F`${RESOURCE_APP_ID}" \
  -H "Metadata: true" | jq -r '.access_token')

curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=ManagedIdentity"
"@

    Write-Sub "G3 — Acquire token from IMDS (user-assigned MI)"
    Write-Code @"
# Specify the client_id of the user-assigned MI
`$MiClientId    = '<user-assigned-mi-client-id>'
`$ResourceAppId = '$ResourceAppId'

`$tokenResp = Invoke-RestMethod ``
    -Uri "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=api://`$ResourceAppId&client_id=`$MiClientId" ``
    -Headers @{ Metadata = 'true' }

`$TOKEN = `$tokenResp.access_token
"@

    Write-Sub "G4 — Via Azure SDK DefaultAzureCredential (.NET / Python / JS)"
    Write-Code @"
# In .NET (DefaultAzureCredential automatically uses MI in Azure):
# var credential = new DefaultAzureCredential();
# var token = await credential.GetTokenAsync(
#     new TokenRequestContext(new[] { "api://<ResourceAppId>/.default" }));
# httpClient.DefaultRequestHeaders.Authorization =
#     new AuthenticationHeaderValue("Bearer", token.Token);

# See Section 2.5 (DotNetConsole) and Section 2.6 (AzureFunction) examples
# for full .NET 8 implementations using DefaultAzureCredential.
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION H — Federated Identity Credentials (GitHub Actions, AKS Workload ID)
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'H') {
    Write-Header "SECTION H — Federated Identity Credentials (OIDC — GitHub Actions / AKS)"
    Write-Host @"

  WHEN TO USE: GitHub Actions CI/CD, AKS workload identity, or any OIDC-
  capable external system.  No secrets exchanged - uses OIDC token from the
  external issuer to get an Entra access token (workload identity federation).

  PREREQUISITES:
  1. Configure federated credential on the Entra app registration.
  2. Assign the app's SP the desired app roles on wwexecution resource.

"@ -ForegroundColor White

    Write-Sub "H1 — Configure federated credential (az CLI)"
    Write-Code @"
# Add a federated credential for GitHub Actions
az ad app federated-credential create ``
    --id '<daemon-app-object-id>' ``
    --parameters '{
        "name": "github-actions-main",
        "issuer": "https://token.actions.githubusercontent.com",
        "subject": "repo:<org>/<repo>:ref:refs/heads/main",
        "description": "GitHub Actions main branch",
        "audiences": ["api://AzureADTokenExchange"]
    }'

# For AKS workload identity:
az ad app federated-credential create ``
    --id '<daemon-app-object-id>' ``
    --parameters '{
        "name": "aks-workload-identity",
        "issuer": "https://oidc.prod-aks.azure.com/<cluster-oidc-issuer>",
        "subject": "system:serviceaccount:<namespace>:<service-account>",
        "audiences": ["api://AzureADTokenExchange"]
    }'
"@

    Write-Sub "H2 — GitHub Actions workflow using OIDC"
    Write-Code @"
# .github/workflows/call-warewolf.yml
name: Call Warewolf Workflow
on: [push]
permissions:
  id-token: write   # Required for OIDC
  contents: read

jobs:
  call-workflow:
    runs-on: ubuntu-latest
    steps:
      - name: Azure Login (OIDC — no secret needed)
        uses: azure/login@v2
        with:
          client-id:       `${{ secrets.AZURE_CLIENT_ID }}
          tenant-id:       `${{ secrets.AZURE_TENANT_ID }}
          subscription-id: `${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Acquire token for wwexecution
        id: get_token
        run: |
          TOKEN=`$(az account get-access-token \
            --resource api://$ResourceAppId \
            --query accessToken -o tsv)
          echo "TOKEN=`$TOKEN" >> `$GITHUB_OUTPUT

      - name: Call Warewolf workflow
        run: |
          curl -H "Authorization: Bearer `${{ steps.get_token.outputs.TOKEN }}" \
            "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=GitHubActions"
"@
}

# ═══════════════════════════════════════════════════════════════════════════════
# SECTION I — ROPC (Resource Owner Password Credentials) — TEST ONLY
# ═══════════════════════════════════════════════════════════════════════════════
if (-not $Section -or $Section -eq 'I') {
    Write-Header "SECTION I — ROPC (Username + Password) — TEST / DEV ONLY"
    Write-Host @"

  ⚠️  WARNING: ROPC is a legacy flow that passes passwords to the token endpoint.
  It does NOT support MFA, conditional access, or federated identities.
  Use ONLY in isolated dev/test environments where no other flow is feasible.
  NEVER use in production.

  Requires: SPA app registration with isFallbackPublicClient=true
            AND ROPC explicitly enabled on the tenant (Home > Azure AD >
            Enterprise applications > Consent and permissions > User consent settings)

"@ -ForegroundColor White

    Write-Sub "I1 — ROPC (PowerShell)"
    Write-Code @"
`$Username = 'testuser@yourdomain.onmicrosoft.com'
`$Password = '<test-user-password>'    # NEVER hardcode in production

`$tokenResp = Invoke-RestMethod ``
    -Method Post ``
    -Uri '$TokenEndpoint' ``
    -ContentType 'application/x-www-form-urlencoded' ``
    -Body @{
        grant_type = 'password'
        client_id  = '$($SpaClientId ?? '<spa-client-id>')'
        username   = `$Username
        password   = `$Password
        scope      = '$Scope'
    }

`$TOKEN = `$tokenResp.access_token

Invoke-RestMethod ``
    -Uri '$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=ROPCUser' ``
    -Headers @{ Authorization = "Bearer `$TOKEN" }
"@

    Write-Sub "I2 — ROPC (curl.exe)"
    Write-Code @"
TOKEN=`$(curl.exe -s -X POST '$TokenEndpoint' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d "grant_type=password&client_id=$($SpaClientId ?? '<spa-client-id>')&username=testuser%40yourdomain.onmicrosoft.com&password=<test-password>&scope=$([Uri]::EscapeDataString($Scope))" \
  | jq -r '.access_token')

curl.exe -H "Authorization: Bearer `${TOKEN}" \
  "$FunctionAppUrl/secure/$WorkflowEncoded.json?Name=ROPCUser"
"@
}

# ─── Token Decode Helper ─────────────────────────────────────────────────────

Write-Header "TOKEN DECODE HELPER"
Write-Code @"
# Decode a JWT token (no signature verification — for inspection only)
function Decode-JwtToken {
    param([string] `$Token)
    `$parts   = `$Token -split '\.'
    `$payload = `$parts[1]
    # Add padding
    `$mod4 = `$payload.Length % 4
    if (`$mod4) { `$payload += '=' * (4 - `$mod4) }
    `$bytes   = [Convert]::FromBase64String((`$payload -replace '-','+'  -replace '_','/'))
    return [Text.Encoding]::UTF8.GetString(`$bytes) | ConvertFrom-Json
}

`$claims = Decode-JwtToken -Token `$TOKEN
Write-Host "Subject (sub) : `$(`$claims.sub)"
Write-Host "Audience (aud): `$(`$claims.aud)"
Write-Host "Issuer (iss)  : `$(`$claims.iss)"
Write-Host "Roles         : `$(`$claims.roles -join ', ')"
Write-Host "Expiry (exp)  : `$([DateTimeOffset]::FromUnixTimeSeconds(`$claims.exp).LocalDateTime)"
Write-Host "Full claims:`n`$(`$claims | ConvertTo-Json)"
"@

Write-Host "`n  ═══ Examples complete.  Use -Section <A-I> -Execute to run a specific flow. ═══`n" -ForegroundColor Cyan
