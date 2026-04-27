#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Tests Warewolf.Execution.Lightweight Azure Function endpoints running locally at
    http://localhost:7071. Simulates Easy Auth by building fake X-MS-CLIENT-PRINCIPAL headers.
.DESCRIPTION
    Run this script in a second terminal while 'func start' (or 'dotnet run') is running.
    Each test sends an HTTP request and compares the actual status code to the expected one.
    Green = pass, Red = fail.
.EXAMPLE
    # Terminal 1:
    func start

    # Terminal 2:
    ./LocalTestHelper.ps1
#>

$BaseUrl = "http://localhost:7071"
$Passed  = 0
$Failed  = 0

# ── Helpers ───────────────────────────────────────────────────────────────────

function Build-FakePrincipal {
    param(
        [string]   $UserId      = "test-user-id-001",
        [string]   $Upn         = "alice@yourtenant.onmicrosoft.com",
        [string[]] $Roles       = @("WarewolfAdministrators"),
        [string[]] $Permissions = @("Permission.View","Permission.Execute",
                                    "Permission.Contribute","Permission.DeployTo",
                                    "Permission.DeployFrom","Permission.Administrator")
    )
    $claims = @(
        @{ typ = "http://schemas.microsoft.com/identity/claims/objectidentifier"; val = $UserId }
        @{ typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";   val = $Upn }
        @{ typ = "scp"; val = "user_impersonation" }
    )
    foreach ($r in $Roles)       { $claims += @{ typ = "roles"; val = $r } }
    foreach ($p in $Permissions) { $claims += @{ typ = "roles"; val = $p } }

    $payload = @{ auth_typ = "aad"; claims = $claims } | ConvertTo-Json -Depth 5 -Compress
    return [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($payload))
}

function Test-Endpoint {
    param(
        [string]    $Description,
        [string]    $Url,
        [string]    $Method         = "GET",
        [hashtable] $Headers        = @{},
        [int]       $ExpectedStatus = 200
    )

    try {
        $response = Invoke-RestMethod -Uri $Url -Method $Method -Headers $Headers `
                        -ResponseHeadersVariable rh -StatusCodeVariable sc `
                        -ErrorAction Stop -SkipHttpErrorCheck
        $actual = [int]$sc
    } catch {
        $actual = [int]$_.Exception.Response.StatusCode
    }

    $pass  = $actual -eq $ExpectedStatus
    $icon  = $pass ? "✓" : "✗"
    $color = $pass ? "Green" : "Red"

    if ($pass) { $script:Passed++ } else { $script:Failed++ }

    Write-Host ("  {0} {1,-60} expected={2} actual={3}" -f `
        $icon, $Description, $ExpectedStatus, $actual) -ForegroundColor $color
}

# ── Build fake principals ─────────────────────────────────────────────────────

$adminHeader  = @{ "X-MS-CLIENT-PRINCIPAL" = Build-FakePrincipal }
$publicHeader = @{ "X-MS-CLIENT-PRINCIPAL" = Build-FakePrincipal -Roles @("PUBLIC") -Permissions @() }
$devBypass    = @{
    "X-MS-CLIENT-PRINCIPAL" = Build-FakePrincipal
    "X-WW-Bypass-Auth"      = "local-dev-bypass"
}

# ── Run tests ─────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Warewolf.Execution.Lightweight — Local Endpoint Tests      " -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ── Public routes (no auth required) ─────────────────────────────────────────
Write-Host "  Public routes (no auth required)" -ForegroundColor White
Test-Endpoint "GET /public/order.json — no token"        "$BaseUrl/public/order.json?id=111"  ExpectedStatus 200
Test-Endpoint "GET /public/report.json — no token"       "$BaseUrl/public/report.json?id=222" ExpectedStatus 200
Test-Endpoint "GET /public/order.json — with admin token" "$BaseUrl/public/order.json?id=333" Headers $adminHeader ExpectedStatus 200

Write-Host ""

# ── Legacy Public/* routes ────────────────────────────────────────────────────
Write-Host "  Legacy Public/* routes (no auth required)" -ForegroundColor White
Test-Endpoint "GET /Public/Hello World — no token"       "$BaseUrl/Public/Hello%20World"      ExpectedStatus 200

Write-Host ""

# ── Secure /secure/* routes — unauthenticated (expect 401) ───────────────────
Write-Host "  /secure/* routes — unauthenticated (expect 401)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — no token"        "$BaseUrl/secure/order.json?id=111"  ExpectedStatus 401
Test-Endpoint "GET /secure/report.json — no token"       "$BaseUrl/secure/report.json"        ExpectedStatus 401
Test-Endpoint "GET /secure/deploy.json — no token"       "$BaseUrl/secure/deploy.json"        ExpectedStatus 401

Write-Host ""

# ── Secure /secure/* routes — authenticated admin (expect 200) ───────────────
Write-Host "  /secure/* routes — authenticated admin (expect 200)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — admin token"     "$BaseUrl/secure/order.json?id=111"  Headers $adminHeader ExpectedStatus 200
Test-Endpoint "GET /secure/report.json — admin token"    "$BaseUrl/secure/report.json"        Headers $adminHeader ExpectedStatus 200
Test-Endpoint "GET /secure/deploy.json — admin token"    "$BaseUrl/secure/deploy.json"        Headers $adminHeader ExpectedStatus 200

Write-Host ""

# ── Secure routes — wrong workflow name (expect 403) ─────────────────────────
Write-Host "  /secure/* routes — unknown workflow (expect 403)" -ForegroundColor White
Test-Endpoint "GET /secure/unknown.json — admin token"   "$BaseUrl/secure/unknown.json"       Headers $adminHeader ExpectedStatus 403

Write-Host ""

# ── Secure routes — PUBLIC role only (expect 403) ────────────────────────────
Write-Host "  /secure/* routes — PUBLIC role only (expect 403)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — PUBLIC role"     "$BaseUrl/secure/order.json"         Headers $publicHeader ExpectedStatus 403

Write-Host ""

# ── Dev bypass (Development environment only) ─────────────────────────────────
Write-Host "  Dev bypass header (Development env only)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — bypass header"   "$BaseUrl/secure/order.json"         Headers $devBypass ExpectedStatus 200

Write-Host ""

# ── Licensing endpoint ────────────────────────────────────────────────────────
Write-Host "  Licensing endpoints" -ForegroundColor White
Test-Endpoint "GET /IsLicensed — no token"               "$BaseUrl/IsLicensed"                ExpectedStatus 200
Test-Endpoint "POST /secure/Subscriptions — no token"    "$BaseUrl/secure/Subscriptions"  Method "POST" ExpectedStatus 401
Test-Endpoint "POST /secure/Subscriptions — admin token" "$BaseUrl/secure/Subscriptions"  Method "POST" Headers $adminHeader ExpectedStatus 400

Write-Host ""

# ── apis.json discovery ───────────────────────────────────────────────────────
Write-Host "  apis.json discovery" -ForegroundColor White
Test-Endpoint "GET /apis.json — no token"                "$BaseUrl/apis.json"                 ExpectedStatus 200
Test-Endpoint "GET /Public/apis.json — no token"         "$BaseUrl/Public/apis.json"          ExpectedStatus 200
Test-Endpoint "GET /Secure/apis.json — admin token"      "$BaseUrl/Secure/apis.json"          Headers $adminHeader ExpectedStatus 200

Write-Host ""

# ── Login ─────────────────────────────────────────────────────────────────────
Write-Host "  Login endpoint" -ForegroundColor White
Test-Endpoint "POST /login — no body (expect 501 or 400)"  "$BaseUrl/login"  Method "POST" ExpectedStatus 501

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
$resultColor = $Failed -eq 0 ? "Green" : "Red"
Write-Host ("  Results: {0} passed  {1} failed" -f $Passed, $Failed) -ForegroundColor $resultColor
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
