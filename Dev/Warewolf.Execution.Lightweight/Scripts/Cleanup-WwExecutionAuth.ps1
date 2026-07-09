# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
<#
.SYNOPSIS
  Reverses everything Configure-WwExecutionAuth.ps1 set up.  Safe to run
  after testing is complete.

.DESCRIPTION
  Targeted, non-destructive cleanup of the wwexecution Easy Auth + Entra
  bootstrap.  Operates only on the resources the configure script created;
  every other resource in the subscription / tenant is left untouched.

  What this WILL do (in order):

    Stage 1   Discovery       - locate the function app and Entra app by name
    Stage 2   Plan + confirm  - print the action list, ask for confirmation
    Stage 3   Disable Easy Auth on the function app  (`enabled = false`)
    Stage 4   Remove the four function-app settings the configure script
              wrote (WAREWOLF_ENTRA_TENANT_ID, WAREWOLF_ENTRA_AUDIENCE,
              WAREWOLF_SECURE_CONFIG, MICROSOFT_PROVIDER_AUTHENTICATION_SECRET)
    Stage 5   Delete the Entra app registration.  This single delete
              CASCADES the following automatically (no separate steps
              required):
                - the service principal
                - all user appRoleAssignments tied to that SP
                - the app roles + oauth2PermissionScopes on the app
                - every passwordCredential / clientSecret on the app

  What this WILL NOT do (deliberate safety):
    - delete the function app resource itself
    - delete the resource group
    - delete the storage account or any other Azure resource
    - delete any user account in Entra
    - touch any other Entra app registration in the tenant
    - delete the secure.config file from the deployed package
    - revoke user consent records from other apps

.PARAMETER WhatIfOnly
  Print the discovery + plan and exit without making any change.

.PARAMETER NonInteractive
  Skip the confirmation prompt and the value-resolution prompts.  Required
  values (SubscriptionId, ResourceGroupName, FunctionAppName) must be
  supplied as parameters when this is set.

.PARAMETER Force
  Skip the confirmation prompt only.  Still prompts for any unset variables.

.PARAMETER KeepEntraApp
  Do everything except delete the Entra app registration.  Use when you
  intend to re-run Configure-WwExecutionAuth.ps1 on the same app shortly.

.PARAMETER KeepFunctionAppSettings
  Do everything except touch the function app (no Easy Auth disable, no
  app-setting removal).  Use when you only want to undo the Entra side.

.USAGE
  ./Scripts/Cleanup-WwExecutionAuth.ps1
  ./Scripts/Cleanup-WwExecutionAuth.ps1 -WhatIfOnly
  ./Scripts/Cleanup-WwExecutionAuth.ps1 -Force
  ./Scripts/Cleanup-WwExecutionAuth.ps1 -KeepEntraApp -Force

  ./Scripts/Cleanup-WwExecutionAuth.ps1 -NonInteractive `
      -SubscriptionId    dd0bc517-... `
      -TenantId          ca0cc53b-... `
      -ResourceGroupName DEV2 `
      -FunctionAppName   wwexecution2

.PREREQUISITES
  Same as Configure-WwExecutionAuth.ps1:
    - Azure CLI >= 2.55
    - Caller must have Application.ReadWrite.All in the tenant and
      Contributor on the function app.
#>

[CmdletBinding()]
param(
    [string] $SubscriptionId,
    [string] $TenantId,
    [string] $ResourceGroupName,
    [string] $FunctionAppName,
    [string] $EntraAppDisplayName,
    [switch] $WhatIfOnly,
    [switch] $NonInteractive,
    [switch] $Force,
    [switch] $KeepEntraApp,
    [switch] $KeepFunctionAppSettings
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURATION DEFAULTS  (match Configure-WwExecutionAuth.ps1)
# ──────────────────────────────────────────────────────────────────────────────

if (-not $PSBoundParameters.ContainsKey('SubscriptionId'))        { $SubscriptionId        = '<subscriptionId>' }
if (-not $PSBoundParameters.ContainsKey('TenantId'))              { $TenantId              = '<tenantId>' }
if (-not $PSBoundParameters.ContainsKey('ResourceGroupName'))     { $ResourceGroupName     = '<resourceGroupName>' }
if (-not $PSBoundParameters.ContainsKey('FunctionAppName'))       { $FunctionAppName       = '<functionAppName>' }
if (-not $PSBoundParameters.ContainsKey('EntraAppDisplayName'))   { $EntraAppDisplayName   = '<auto>' }

# These app-setting names must match what Configure-WwExecutionAuth.ps1 writes
# in its Stage 8.  Keep them in sync.
$ClientSecretSettingName = 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
$AppSettingsToRemove = @(
    'WAREWOLF_ENTRA_TENANT_ID',
    'WAREWOLF_ENTRA_AUDIENCE',
    'WAREWOLF_SECURE_CONFIG',
    $ClientSecretSettingName
)

# ──────────────────────────────────────────────────────────────────────────────
# Helpers  (deliberately mirror the configure script for consistency)
# ──────────────────────────────────────────────────────────────────────────────

function Format-AzArgsForLog {
    param([Parameter(Mandatory)][string[]] $Arguments)

    $secretFlagRegex = '(?i)^(--password|--client-secret|--secret|--certificate)$'
    $kvSecretRegex   = '(?i)^([A-Z0-9_]*?(SECRET|PASSWORD|KEY|TOKEN|CONNECTIONSTRING)[A-Z0-9_]*?)=(.+)$'

    $rendered = New-Object System.Collections.Generic.List[string]
    $maskNext = $false
    foreach ($arg in $Arguments) {
        if ($maskNext)                            { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($arg -match $secretFlagRegex)         { $rendered.Add($arg);              $maskNext = $true;  continue }
        if ($arg -match $kvSecretRegex)           { $rendered.Add("$($Matches[1])=***REDACTED***");      continue }
        if ($arg -match '\s')                     { $rendered.Add(('"{0}"' -f ($arg -replace '"','\"')));continue }
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
    Write-Host "    > az $printable" -ForegroundColor DarkGray

    # Same transient-error retry policy as Configure-WwExecutionAuth.ps1.
    $transientPattern = (@(
        'Connection aborted','ConnectionResetError','10054','10053','10060',
        'Read timed out','ReadTimeoutError','ConnectTimeoutError',
        'SSLEOFError','SSL.*EOF occurred in violation','Max retries exceeded',
        'temporary failure in name resolution','Could not resolve host','getaddrinfo failed',
        'TooManyRequests','"code":\s*"429"',' 429 ',
        ' 500 Internal',' 502 Bad Gateway',' 503 ','ServiceUnavailable',
        ' 504 Gateway','GatewayTimeout','BadGateway'
    ) -join '|')

    $delays = @(3, 7, 15, 30)

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $output = & az @Arguments 2>&1
        if ($LASTEXITCODE -eq 0) { return $output }

        $asString = ($output | Out-String)
        if (($asString -match $transientPattern) -and ($attempt -lt $MaxAttempts)) {
            $wait = $delays[[Math]::Min($attempt - 1, $delays.Length - 1)]
            Write-Host ("    transient error (attempt {0}/{1}) - retrying in {2}s" -f
                $attempt, $MaxAttempts, $wait) -ForegroundColor DarkYellow
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

        if     ($start -gt 0) { $text = $text.Substring($start) }
        elseif ($start -lt 0) { return $null }

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

function Test-FunctionAppExists {
    param([string] $Name, [string] $ResourceGroup)
    try {
        Invoke-AzCli @('functionapp','show','--name',$Name,'--resource-group',$ResourceGroup,'--query','name','-o','tsv') |
            Out-Null
        return $true
    } catch {
        return $false
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Interactive value resolution
# ──────────────────────────────────────────────────────────────────────────────

if ($NonInteractive) {
    Write-Host "═══ Configuration (non-interactive) ════════════════════════════" -ForegroundColor Cyan
    foreach ($r in @(
        @{ Name = 'SubscriptionId';    Value = $SubscriptionId    },
        @{ Name = 'ResourceGroupName'; Value = $ResourceGroupName },
        @{ Name = 'FunctionAppName';   Value = $FunctionAppName   }
    )) {
        if (Test-IsPlaceholder $r.Value) {
            throw "[$($r.Name)] is unset or still a placeholder ('$($r.Value)'). Pass -$($r.Name) <value> on the command line."
        }
    }
    if (Test-IsPlaceholder $EntraAppDisplayName) { $EntraAppDisplayName = "$FunctionAppName-auth" }
} else {
    Write-Host ""
    Write-Host "═══ Cleanup configuration ═════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Hit Enter to keep the [bracketed] default; type a value to override." -ForegroundColor DarkGray
    Write-Host ""

    $SubscriptionId    = Read-ScalarVariable -Name 'SubscriptionId'    -Current $SubscriptionId    -Description 'Azure subscription GUID'    -Required
    $ResourceGroupName = Read-ScalarVariable -Name 'ResourceGroupName' -Current $ResourceGroupName -Description 'Existing resource group'     -Required
    $FunctionAppName   = Read-ScalarVariable -Name 'FunctionAppName'   -Current $FunctionAppName   -Description 'Function app to clean up'    -Required

    if (Test-IsPlaceholder $EntraAppDisplayName -or $EntraAppDisplayName -eq '<auto>') {
        $EntraAppDisplayName = "$FunctionAppName-auth"
    }
    $EntraAppDisplayName = Read-ScalarVariable -Name 'EntraAppDisplayName' -Current $EntraAppDisplayName -Description 'Entra app to delete'

    # TenantId is informational only here (we look the app up by name within
    # the active subscription's tenant), but resolve it for the summary.
    if (Test-IsPlaceholder $TenantId) {
        $TenantId = Read-ScalarVariable -Name 'TenantId' -Current $TenantId -Description 'Microsoft Entra tenant GUID (informational)'
    }
}

Invoke-AzCli @('account','set','--subscription',$SubscriptionId) | Out-Null

# ──────────────────────────────────────────────────────────────────────────────
# Stage 1 — Discovery
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 1  Discovery ════════════════════════════════════════════" -ForegroundColor Cyan

$functionAppExists = Test-FunctionAppExists -Name $FunctionAppName -ResourceGroup $ResourceGroupName
if ($functionAppExists) {
    Write-Host "    Function app  : $FunctionAppName  (found)" -ForegroundColor Green
} else {
    Write-Host "    Function app  : $FunctionAppName  (NOT found - skipping function-app cleanup)" -ForegroundColor DarkGray
}

$app = $null
if (-not [string]::IsNullOrWhiteSpace($EntraAppDisplayName)) {
    $app = Invoke-AzCli @(
        'ad','app','list','--display-name',$EntraAppDisplayName,'--query','[0]','-o','json'
    ) | ConvertFrom-AzJson
}

$ClientId    = $null
$AppObjectId = $null
if ($app) {
    $ClientId    = $app.appId
    $AppObjectId = $app.id
    Write-Host "    Entra app     : $EntraAppDisplayName  (found)" -ForegroundColor Green
    Write-Host "      clientId    : $ClientId"
    Write-Host "      appObjectId : $AppObjectId"
} else {
    Write-Host "    Entra app     : $EntraAppDisplayName  (NOT found - skipping Entra cleanup)" -ForegroundColor DarkGray
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2 — Plan + confirmation
# ──────────────────────────────────────────────────────────────────────────────

$plan = New-Object 'System.Collections.Generic.List[string]'

if ($functionAppExists -and -not $KeepFunctionAppSettings) {
    [void]$plan.Add("Disable Easy Auth on '$FunctionAppName' (platform.enabled = false)")
    [void]$plan.Add("Remove $($AppSettingsToRemove.Count) function app settings: $($AppSettingsToRemove -join ', ')")
}
if ($app -and -not $KeepEntraApp) {
    [void]$plan.Add("Delete Entra app '$EntraAppDisplayName' (clientId=$ClientId)")
    [void]$plan.Add("    + cascade: service principal, all appRoleAssignments tied to this SP")
    [void]$plan.Add("    + cascade: app roles, oauth2PermissionScopes, passwordCredentials")
}

Write-Host ""
Write-Host "═══ Stage 2  Cleanup plan ═════════════════════════════════════════" -ForegroundColor Cyan

if ($plan.Count -eq 0) {
    Write-Host "    Nothing to do - everything is already clean." -ForegroundColor Green
    return
}

foreach ($p in $plan) { Write-Host "    - $p" }

Write-Host ""
Write-Host "  NOT TOUCHED:" -ForegroundColor DarkGray
Write-Host "    - the function app resource itself   ('$FunctionAppName' stays)" -ForegroundColor DarkGray
Write-Host "    - the resource group                 ('$ResourceGroupName' stays)" -ForegroundColor DarkGray
Write-Host "    - storage / Application Insights / any other resource" -ForegroundColor DarkGray
Write-Host "    - any user account or other Entra app registration"   -ForegroundColor DarkGray

if ($WhatIfOnly) {
    Write-Host ""
    Write-Host "  -WhatIfOnly set; exiting without changes." -ForegroundColor Yellow
    return
}

if (-not ($Force -or $NonInteractive)) {
    Write-Host ""
    Write-Host "  WARNING: these actions are IRREVERSIBLE." -ForegroundColor Yellow
    $proceed = Read-Host "  Proceed? [y/N]"
    if ($proceed -notmatch '^[yY]') {
        Write-Host "  Aborted." -ForegroundColor Yellow
        return
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 3 — Disable Easy Auth on the function app
# ──────────────────────────────────────────────────────────────────────────────

if ($functionAppExists -and -not $KeepFunctionAppSettings) {
    Write-Host ""
    Write-Host "═══ Stage 3  Disable Easy Auth ═══════════════════════════════════" -ForegroundColor Cyan
    try {
        Invoke-AzCli @(
            'webapp','auth','update',
            '--name',           $FunctionAppName,
            '--resource-group', $ResourceGroupName,
            '--enabled',        'false'
        ) | Out-Null
        Write-Host "    Easy Auth disabled (platform.enabled = false)" -ForegroundColor Green
    } catch {
        Write-Warning "    Failed to disable Easy Auth: $($_.Exception.Message)"
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 4 — Remove function app settings
# ──────────────────────────────────────────────────────────────────────────────

if ($functionAppExists -and -not $KeepFunctionAppSettings) {
    Write-Host ""
    Write-Host "═══ Stage 4  Remove function app settings ════════════════════════" -ForegroundColor Cyan
    try {
        # `appsettings delete --setting-names a b c` is idempotent: missing
        # names are silently no-ops, so this is safe even on a partially-
        # cleaned app.
        Invoke-AzCli (@(
            'functionapp','config','appsettings','delete',
            '--name',           $FunctionAppName,
            '--resource-group', $ResourceGroupName,
            '--setting-names'
        ) + $AppSettingsToRemove) | Out-Null

        Write-Host "    Removed $($AppSettingsToRemove.Count) app settings:" -ForegroundColor Green
        foreach ($s in $AppSettingsToRemove) { Write-Host "      - $s" -ForegroundColor DarkGray }
    } catch {
        Write-Warning "    Failed to remove app settings: $($_.Exception.Message)"
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 5 — Delete Entra app  (cascades SP, role assignments, secrets)
# ──────────────────────────────────────────────────────────────────────────────

if ($app -and -not $KeepEntraApp) {
    Write-Host ""
    Write-Host "═══ Stage 5  Delete Entra app ════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "    Deleting '$EntraAppDisplayName' ($ClientId)..." -ForegroundColor Yellow

    try {
        # `az ad app delete` removes the application object.  Microsoft Graph
        # then automatically removes:
        #   - the service principal that backed this app
        #   - every appRoleAssignment whose resourceId pointed at that SP
        #   - all passwordCredentials and keyCredentials
        #   - the appRoles and oauth2PermissionScopes (they live on the app)
        # The deleted object is held in the Graph "deleted items" tomb for
        # 30 days during which it can be restored via
        #   az rest --method POST --url https://graph.microsoft.com/v1.0/directory/deletedItems/<id>/restore
        Invoke-AzCli @('ad','app','delete','--id',$ClientId) | Out-Null

        Write-Host "    Entra app deleted." -ForegroundColor Green
        Write-Host "      Cascaded: service principal, role assignments, credentials, app roles, scopes" -ForegroundColor DarkGray
        Write-Host "      Restore window: 30 days from now via Graph deletedItems endpoint" -ForegroundColor DarkGray
    } catch {
        Write-Warning "    Failed to delete Entra app: $($_.Exception.Message)"
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Done
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Done ═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Verification snapshot — show what's left so the operator can spot anything
# the cleanup missed (e.g. the operator added extra app settings outside this
# script that we deliberately did not remove).

if ($functionAppExists -and -not $KeepFunctionAppSettings) {
    try {
        $remaining = Invoke-AzCli @(
            'functionapp','config','appsettings','list',
            '--name',$FunctionAppName,'--resource-group',$ResourceGroupName,
            '--query',"[?contains(name,'WAREWOLF') || name=='$ClientSecretSettingName'].name",
            '-o','tsv'
        )
        $remaining = ($remaining -split "`n") | ForEach-Object { $_.Trim() } | Where-Object { $_ }
        if ($remaining.Count -gt 0) {
            Write-Warning "  These settings still match WAREWOLF_* / $ClientSecretSettingName patterns:"
            foreach ($r in $remaining) { Write-Host "      $r" -ForegroundColor DarkYellow }
        } else {
            Write-Host "  No WAREWOLF_* / $ClientSecretSettingName app settings remain." -ForegroundColor Green
        }
    } catch { }
}

if (-not $KeepEntraApp) {
    try {
        $still = Invoke-AzCli @(
            'ad','app','list','--display-name',$EntraAppDisplayName,'--query','[0]','-o','json'
        ) | ConvertFrom-AzJson
        if ($still) {
            Write-Warning "  Entra app '$EntraAppDisplayName' still resolves - delete may have been queued; re-check in a minute."
        } else {
            Write-Host "  Entra app '$EntraAppDisplayName' is no longer present." -ForegroundColor Green
        }
    } catch { }
}

Write-Host ""
