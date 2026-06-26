# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
    Performs a Dropbox PKCE Authorization Code flow and outputs the resulting
    access token and refresh token — no Studio or Warewolf server required.

.DESCRIPTION
    *** PREFERRED ALTERNATIVE ***
    If the lightweight server is running, use its built-in OAuth flow instead:

        https://{your-function-app}.azurewebsites.net/oauth/dropbox/start?sourceId={guid}&appKey={key}
        (or locally: http://localhost:7071/oauth/dropbox/start?sourceId={guid}&appKey={key})

    The server handles the entire PKCE flow — opens Dropbox, receives the callback,
    exchanges the code for tokens, writes them back to the .bite file, and invalidates
    the source cache — all automatically, no copy-pasting required.
    See DropboxOAuthFunction.cs for details and required Dropbox App Console setup.

    *** USE THIS SCRIPT WHEN ***
    - The lightweight server is not running yet (cold bootstrap).
    - You need tokens in plain text to paste into a .bite file manually.
    - You are troubleshooting the server-side OAuth flow.

    *** SCRIPT FLOW ***
    Generates a PKCE code verifier/challenge, opens the Dropbox authorization
    page in the default browser, waits for you to paste the authorization code
    back, then exchanges it for tokens via the Dropbox token endpoint.

    The output is the plain-text Warewolf connection string ready to paste into
    a decrypted .bite file before re-running Encrypt-Config.ps1.

    Redirect URI used: https://www.dropbox.com/1/oauth2/redirect_receiver/
    (This is Dropbox's own hosted receiver — the code appears in the page URL
    and query string after you approve the app.  Copy it from the browser
    address bar.)

    IMPORTANT: Copy the authorization code and paste it below within ~60 seconds.
    The code is bound to THIS script run's PKCE session — codes from previous runs
    cannot be reused.

.PARAMETER AppKey
    Your Dropbox app key (from the Dropbox App Console).

.EXAMPLE
    .\Get-DropboxTokens.ps1 -AppKey "abc123xyz"

.NOTES
    Requires: PowerShell 7+, internet access to api.dropboxapi.com.
#>

#Requires -Version 7.0

param(
    [Parameter(Mandatory)]
    [string] $AppKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$redirectUri = 'https://www.dropbox.com/1/oauth2/redirect_receiver/'
$scopes      = 'account_info.read files.metadata.read files.content.read files.content.write'

# ── Generate PKCE code verifier (32 random bytes → URL-safe base64, no padding) ──
$raw           = [byte[]]::new(32)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($raw)
$codeVerifier  = [Convert]::ToBase64String($raw).TrimEnd('=').Replace('+', '-').Replace('/', '_')

# ── code_challenge = BASE64URL(SHA256(ASCII(code_verifier))) ──────────────────
$sha256        = [System.Security.Cryptography.SHA256]::Create()
$verifierBytes = [System.Text.Encoding]::ASCII.GetBytes($codeVerifier)
$hashBytes     = $sha256.ComputeHash($verifierBytes)
$codeChallenge = [Convert]::ToBase64String($hashBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

# ── Build the authorize URL ───────────────────────────────────────────────────
$state    = [System.Guid]::NewGuid().ToString('N')
$authUrl  = 'https://www.dropbox.com/oauth2/authorize' +
            "?client_id=$([Uri]::EscapeDataString($AppKey))" +
            "&response_type=code" +
            "&redirect_uri=$([Uri]::EscapeDataString($redirectUri))" +
            "&state=$state" +
            "&code_challenge=$codeChallenge" +
            "&code_challenge_method=S256" +
            "&token_access_type=offline" +
            "&scope=$([Uri]::EscapeDataString($scopes))"

Write-Host ''
Write-Host '[*] Opening Dropbox authorization page in your browser...' -ForegroundColor Cyan
Write-Host "    State : $state" -ForegroundColor DarkGray
Start-Process $authUrl

Write-Host ''
Write-Host '[*] After you approve the app, Dropbox redirects to its own receiver page.' -ForegroundColor Cyan
Write-Host '    The browser address bar will contain:' -ForegroundColor Cyan
Write-Host '    https://www.dropbox.com/1/oauth2/redirect_receiver/?code=<CODE>&state=...' -ForegroundColor White
Write-Host ''
Write-Host '    Copy just the CODE value (everything between "code=" and "&state") and' -ForegroundColor Cyan
Write-Host '    paste it below.' -ForegroundColor Cyan
Write-Host ''

$code = (Read-Host 'Authorization code').Trim()
if ([string]::IsNullOrWhiteSpace($code)) {
    Write-Host '[!] No code entered. Exiting.' -ForegroundColor Red
    exit 1
}

# ── Exchange code for tokens ──────────────────────────────────────────────────
Write-Host ''
Write-Host '[*] Exchanging authorization code for tokens...' -ForegroundColor Cyan

$body = @{
    code          = $code
    grant_type    = 'authorization_code'
    client_id     = $AppKey
    redirect_uri  = $redirectUri
    code_verifier = $codeVerifier
}

try {
    $response = Invoke-RestMethod `
        -Uri         'https://api.dropboxapi.com/oauth2/token' `
        -Method      Post `
        -Body        $body `
        -ContentType 'application/x-www-form-urlencoded'
} catch {
    Write-Host "[!] Token exchange failed: $_" -ForegroundColor Red
    # PowerShell 7 / .NET 7+ surfaces the response body in ErrorDetails.Message
    $errBody = $_.ErrorDetails.Message
    if (-not $errBody -and $_.Exception.Response) {
        # Fallback: read from HttpResponseMessage.Content (not WebResponse.GetResponseStream)
        $errBody = $_.Exception.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    }
    if ($errBody) {
        Write-Host "    Response body: $errBody" -ForegroundColor Red
    }
    exit 1
}

$accessToken  = $response.access_token
$refreshToken = $response.refresh_token

if ([string]::IsNullOrWhiteSpace($accessToken)) {
    Write-Host '[!] Token exchange returned no access_token.' -ForegroundColor Red
    Write-Host "    Full response: $($response | ConvertTo-Json)" -ForegroundColor Red
    exit 1
}

$tokenPreview = if ($accessToken.Length -gt 8) { $accessToken.Substring(0,8) + '…' } else { $accessToken }

Write-Host ''
Write-Host '[+] Token exchange succeeded.' -ForegroundColor Green
Write-Host "    AccessToken  : $tokenPreview (len=$($accessToken.Length))" -ForegroundColor Green
Write-Host "    RefreshToken : $(if ($refreshToken) { 'present (len=' + $refreshToken.Length + ')' } else { 'MISSING — token_access_type=offline may not be enabled for this app' })" -ForegroundColor $(if ($refreshToken) { 'Green' } else { 'Red' })
Write-Host "    ExpiresIn    : $($response.expires_in)s" -ForegroundColor DarkGray

if (-not $refreshToken) {
    Write-Host ''
    Write-Host '[!] No refresh token returned.' -ForegroundColor Red
    Write-Host '    Check your Dropbox App Console: the app must have "offline" access' -ForegroundColor Yellow
    Write-Host '    (Permissions tab → access type must allow refresh tokens).' -ForegroundColor Yellow
    exit 1
}

# ── Output the Warewolf connection string ─────────────────────────────────────
$connectionString = "AccessToken=$accessToken;AppKey=$AppKey;RefreshToken=$refreshToken"

Write-Host ''
Write-Host ('─' * 70) -ForegroundColor DarkGray
Write-Host '  Plain-text connection string (paste this into the decrypted .bite file):' -ForegroundColor Cyan
Write-Host ''
Write-Host "  $connectionString" -ForegroundColor White
Write-Host ''
Write-Host ('─' * 70) -ForegroundColor DarkGray
Write-Host ''
Write-Host '[*] Next steps:' -ForegroundColor Cyan
Write-Host '    1. Run Encrypt-Config.ps1 -Decrypt to get plain-text .bite files.' -ForegroundColor White
Write-Host '    2. Open the decrypted "Azure Dropbox Source.bite".' -ForegroundColor White
Write-Host '    3. Replace the ConnectionString attribute value with the string above.' -ForegroundColor White
Write-Host '    4. Run Encrypt-Config.ps1 on the edited file to re-encrypt.' -ForegroundColor White
Write-Host '    5. Deploy the re-encrypted .bite to /home/site/wwwroot/Resources/.' -ForegroundColor White
Write-Host ''

# Also copy to clipboard if possible
try {
    $connectionString | Set-Clipboard
    Write-Host '[+] Connection string copied to clipboard.' -ForegroundColor Green
} catch {
    # Set-Clipboard may not be available in all PS7 environments — not fatal
}
