<#
.SYNOPSIS
  Provisions Microsoft Entra ID + Easy Auth for the Warewolf wwexecution Azure Function.

.DESCRIPTION
  Idempotent end-to-end automation that performs steps 1.1 - 1.6 of the
  EasyAuth-Entra-Tutorial.md document:

    1. Creates / refreshes the Entra app registration backing wwexecution.
    2. Exposes api://<clientId> with a default scope.
    3. Creates the WarewolfAdministrators / PUBLIC group app roles.
    4. Creates the Permission.* app roles (View / Execute / Contribute /
       DeployTo / DeployFrom / Administrator).
    5. Assigns roles to nominated users (admins get all permissions; public
       users get Permission.View only).
    6. Pushes authsettingsV2.json to App Service Authentication via az rest.
    7. Sets the required Function App settings (tenant id, audience,
       provider secret, secure config path).

  Re-running the script safely upgrades existing objects rather than
  duplicating them. Generated artefacts (clientId, secret expiry, role
  assignments) are written to ./Configure-WwExecutionAuth.output.json.

.PREREQUISITES
  * Azure CLI >= 2.55
  * Caller must be Application.ReadWrite.All AND have rights on the
    target subscription / function app.

.USAGE
  az login
  az account set --subscription $SubscriptionId
  ./Scripts/Configure-WwExecutionAuth.ps1
  ./Scripts/Configure-WwExecutionAuth.ps1 -RotateSecret   # rotate client secret
  ./Scripts/Configure-WwExecutionAuth.ps1 -SkipUserAssignment

#>

[CmdletBinding()]
param(
    [switch] $RotateSecret,
    [switch] $SkipUserAssignment
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURATION  -- edit these before running
# ──────────────────────────────────────────────────────────────────────────────

$SubscriptionId       = 'dd0bc517-5cc7-4b56-bd6a-68e6140db7b3'
$TenantId             = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1'
$ResourceGroupName    = 'DEV2'
$FunctionAppName      = 'wwexecution2'                         # configurable name
$EntraAppDisplayName  = "$FunctionAppName-auth"
$SecretLifetimeYears  = 1
$SecureConfigMountPath = 'D:\home\site\wwwroot\secure.config' # WAREWOLF_SECURE_CONFIG

# Group → permissions mapping
$GroupPermissions = @{
    'WarewolfAdministrators' = @(
        'Permission.View','Permission.Execute','Permission.Contribute',
        'Permission.DeployTo','Permission.DeployFrom','Permission.Administrator'
    )
    'PUBLIC' = @('Permission.View')
    'EXECUTE' = @('Permission.Execute')
    'DEPLOY' = @('Permission.DeployTo','Permission.DeployFrom')
}

# Users → group
$UserAssignments = @(
    @{ Upn='ashley.lewis@theunlimited.co.za'; Group='WarewolfAdministrators' },
    @{ Upn='aakash.gaikwad@theunlimited.co.za'; Group='PUBLIC' }
    @{ Upn='sehul.shah@theunlimited.co.za'; Group='EXECUTE' }
)

# Path to the existing Easy Auth template alongside this script
$AuthSettingsTemplatePath = Join-Path $PSScriptRoot 'authsettingsV2.json'
$OutputPath               = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth.output.json'

# ──────────────────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────────────────

function Format-AzArgsForLog {
    <#
        Renders an az argument array as a single line, masking values that
        look like secrets so client_secret / app keys never end up in logs.
        Recognised secret patterns:
          - <NAME>=<value>  where NAME contains SECRET / PASSWORD / KEY /
            TOKEN / CONNECTIONSTRING (used in `--settings KEY=value` lists)
          - --password / --client-secret / --secret  followed by a value
    #>
    param([Parameter(Mandatory)][string[]] $Arguments)

    $secretFlagRegex   = '(?i)^(--password|--client-secret|--secret|--certificate)$'
    $kvSecretRegex     = '(?i)^([A-Z0-9_]*?(SECRET|PASSWORD|KEY|TOKEN|CONNECTIONSTRING)[A-Z0-9_]*?)=(.+)$'

    $rendered = New-Object System.Collections.Generic.List[string]
    $maskNext = $false
    foreach ($arg in $Arguments) {
        if ($maskNext) {
            $rendered.Add('***REDACTED***')
            $maskNext = $false
            continue
        }
        if ($arg -match $secretFlagRegex) {
            $rendered.Add($arg)
            $maskNext = $true
            continue
        }
        if ($arg -match $kvSecretRegex) {
            $rendered.Add("$($Matches[1])=***REDACTED***")
            continue
        }
        # Quote arguments that contain whitespace so the printed line is
        # actually copy-paste re-runnable.
        if ($arg -match '\s') {
            $rendered.Add(('"{0}"' -f ($arg -replace '"','\"')))
        } else {
            $rendered.Add($arg)
        }
    }
    return ($rendered -join ' ')
}

function Invoke-AzCli {
    <#
        Wraps az CLI invocations so callers can use PowerShell-friendly arrays
        instead of `--query` strings.  Throws on non-zero exit.
        Always echoes the (secret-redacted) command before invoking it so
        any failure can be traced to the exact command line that produced it.
    #>
    param([Parameter(Mandatory)][string[]] $Arguments)

    $printable = Format-AzArgsForLog -Arguments $Arguments
    Write-Host "    > az $printable" -ForegroundColor DarkGray

    $output = & az @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Echo the failing command on the error line as well so the throw
        # message is self-contained in CI logs that strip surrounding output.
        throw "az CLI failed ($LASTEXITCODE) running ``az $printable``:`n$output"
    }
    return $output
}

function ConvertFrom-AzJson {
    <#
        Aggregates every line emitted by az CLI into a single buffer, then parses
        it in the `end` block.  The earlier implementation parsed in `process`,
        which fired once per line because PowerShell iterates string arrays
        through the pipeline element-by-element — the first iteration saw just
        "{" and ConvertFrom-Json failed with "Unexpected end... position 1".

        Returns $null when az emitted nothing or the literal "null" (which is
        what `--query '[0]'` produces when no match is found).
    #>
    [CmdletBinding()]
    param([Parameter(ValueFromPipeline)][object] $InputObject)

    begin   { $sb = [System.Text.StringBuilder]::new() }
    process {
        if ($null -ne $InputObject) {
            foreach ($line in @($InputObject)) {
                [void]$sb.AppendLine([string]$line)
            }
        }
    }
    end {
        $text = $sb.ToString().Trim()
        if ([string]::IsNullOrWhiteSpace($text) -or $text -eq 'null') {
            return $null
        }
        return $text | ConvertFrom-Json -Depth 50
    }
}

function New-AppRoleObject {
    param([string] $Value, [string] $DisplayName, [string] $Description)
    [pscustomobject]@{
        allowedMemberTypes = @('User','Application')
        description        = $Description
        displayName        = $DisplayName
        id                 = [guid]::NewGuid().ToString()
        isEnabled          = $true
        value              = $Value
    }
}

function Invoke-WithRetry {
    <#
        Retries a script block on Microsoft Graph propagation failures.
        Graph is eventually consistent: an app registration created seconds ago
        is not always visible to the replica that az ad sp create reads from,
        which surfaces as
            "Resource '<appId>' does not exist or one of its queried
             reference-property objects are not present."
        We back off exponentially up to MaxAttempts times.
    #>
    param(
        [Parameter(Mandatory)][scriptblock] $ScriptBlock,
        [int]                  $MaxAttempts = 6,
        [int]                  $InitialDelaySeconds = 2,
        [string]               $OperationName = 'operation'
    )
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        try { return & $ScriptBlock }
        catch {
            if ($i -eq $MaxAttempts) {
                Write-Host "    $OperationName failed after $MaxAttempts attempts" -ForegroundColor Red
                throw
            }
            $delay = [Math]::Min(60, $InitialDelaySeconds * [Math]::Pow(2, $i - 1))
            Write-Host ("    {0} attempt {1}/{2} not yet consistent; retrying in {3}s" -f
                $OperationName, $i, $MaxAttempts, $delay) -ForegroundColor DarkYellow
            Start-Sleep -Seconds $delay
        }
    }
}

function Get-EntraServicePrincipal {
    param([Parameter(Mandatory)][string] $AppId)
    try {
        return Invoke-AzCli @('ad','sp','show','--id',$AppId,'-o','json') |
               ConvertFrom-AzJson
    } catch {
        return $null
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# 0. Sanity
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Setting subscription $SubscriptionId" -ForegroundColor Cyan
Invoke-AzCli @('account','set','--subscription',$SubscriptionId) | Out-Null

if (-not (Test-Path $AuthSettingsTemplatePath)) {
    throw "authsettingsV2.json template not found at $AuthSettingsTemplatePath"
}

# ──────────────────────────────────────────────────────────────────────────────
# 1. Entra app registration  (create or upgrade)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Resolving Entra app '$EntraAppDisplayName'" -ForegroundColor Cyan

$app = Invoke-AzCli @(
    'ad','app','list',
    '--display-name',$EntraAppDisplayName,
    '--query','[0]','-o','json'
) | ConvertFrom-AzJson

if ($null -eq $app) {
    Write-Host "    creating new app registration" -ForegroundColor Yellow
    $app = Invoke-AzCli @(
        'ad','app','create',
        '--display-name',$EntraAppDisplayName,
        '--sign-in-audience','AzureADMyOrg',
        '--web-redirect-uris',"https://$FunctionAppName.azurewebsites.net/.auth/login/aad/callback",
        '-o','json'
    ) | ConvertFrom-AzJson

    # Microsoft Graph is eventually consistent — give the new app a moment
    # to replicate before later operations (sp create, role assignments) hit
    # a "Resource does not exist" error.
    Write-Host "    waiting 15s for Graph replication" -ForegroundColor DarkGray
    Start-Sleep -Seconds 15
} else {
    Write-Host "    found existing appId $($app.appId) - will upgrade in place" -ForegroundColor Green
}

$ClientId = $app.appId
$AppObjectId = $app.id

# Ensure the redirect URI is present (idempotent re-add)
Invoke-AzCli @(
    'ad','app','update','--id',$ClientId,
    '--web-redirect-uris',"https://$FunctionAppName.azurewebsites.net/.auth/login/aad/callback"
) | Out-Null

# ──────────────────────────────────────────────────────────────────────────────
# 2. Expose the API + default scope
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Exposing api://$ClientId" -ForegroundColor Cyan
Invoke-AzCli @(
    'ad','app','update','--id',$ClientId,
    '--identifier-uris',"api://$ClientId"
) | Out-Null

# ──────────────────────────────────────────────────────────────────────────────
# 3. App roles (groups + permissions) — declarative replace
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Reconciling app roles" -ForegroundColor Cyan

$desiredRoles = @()
foreach ($groupName in $GroupPermissions.Keys) {
    $desiredRoles += New-AppRoleObject -Value $groupName -DisplayName $groupName `
        -Description "Warewolf group: $groupName"
}
$permissionRoles = @{
    'Permission.View'          = 'Read workflow definitions and execution status'
    'Permission.Execute'       = 'Trigger workflow execution'
    'Permission.Contribute'    = 'Create and modify workflow definitions'
    'Permission.DeployTo'      = 'Deploy workflows to a target environment'
    'Permission.DeployFrom'    = 'Pull workflow deployments from a source environment'
    'Permission.Administrator' = 'Full permission over all workflow operations'
}
foreach ($p in $permissionRoles.GetEnumerator()) {
    $desiredRoles += New-AppRoleObject -Value $p.Key -DisplayName $p.Key -Description $p.Value
}

# Preserve existing role IDs to avoid invalidating live assignments
$current = Invoke-AzCli @('ad','app','show','--id',$ClientId,'--query','appRoles','-o','json') |
           ConvertFrom-AzJson
foreach ($desired in $desiredRoles) {
    $existing = $current | Where-Object { $_.value -eq $desired.value } | Select-Object -First 1
    if ($existing) { $desired.id = $existing.id }
}

$rolesPayload = $desiredRoles | ConvertTo-Json -Depth 5 -Compress
$tempPatch = [System.IO.Path]::GetTempFileName()
@{ appRoles = $desiredRoles } | ConvertTo-Json -Depth 6 | Set-Content -Path $tempPatch -Encoding UTF8

Invoke-AzCli @(
    'rest','--method','PATCH',
    '--url',"https://graph.microsoft.com/v1.0/applications/$AppObjectId",
    '--headers','Content-Type=application/json',
    '--body',"@$tempPatch"
) | Out-Null
Remove-Item $tempPatch -Force

# ──────────────────────────────────────────────────────────────────────────────
# 4. Service principal (required for app role assignment)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Ensuring service principal" -ForegroundColor Cyan

# Lookup is best-effort - returns $null if the SP isn't there yet.
$sp = Get-EntraServicePrincipal -AppId $ClientId

# Create if missing, retrying through Graph replication delays.  An app
# created moments ago may not be visible to the replica that handles
# `az ad sp create`, surfacing as "Resource <appId> does not exist".
if (-not $sp) {
    $sp = Invoke-WithRetry -OperationName 'sp create' -ScriptBlock {
        Invoke-AzCli @('ad','sp','create','--id',$ClientId,'-o','json') |
            ConvertFrom-AzJson
    }
}
$SpObjectId = $sp.id

# Refresh roles cache after PATCH so we have the right IDs.  The PATCH
# above is also subject to the same replication lag.
$spRoles = Invoke-WithRetry -OperationName 'sp show appRoles' -ScriptBlock {
    Invoke-AzCli @('ad','sp','show','--id',$ClientId,'--query','appRoles','-o','json') |
        ConvertFrom-AzJson
}

# ──────────────────────────────────────────────────────────────────────────────
# 5. User → role assignments
# ──────────────────────────────────────────────────────────────────────────────

if (-not $SkipUserAssignment) {
    Write-Host "==> Assigning users to roles" -ForegroundColor Cyan
    foreach ($u in $UserAssignments) {
        $userObj = Invoke-AzCli @('ad','user','show','--id',$u.Upn,'-o','json') |
                   ConvertFrom-AzJson
        $userOid = $userObj.id

        # ── Pre-fetch existing assignments so we can skip duplicates cleanly ──
        # Building a HashSet<appRoleId> scoped to THIS service principal lets
        # us decide locally (no POST) whether each role is already in place.
        $existing = $null
        try {
            $existing = Invoke-AzCli @(
                'rest','--method','GET',
                '--url',"https://graph.microsoft.com/v1.0/users/$userOid/appRoleAssignments"
            ) | ConvertFrom-AzJson
        } catch {
            Write-Warning "    could not list existing assignments for $($u.Upn): $($_.Exception.Message)"
        }

        $alreadyAssigned = New-Object 'System.Collections.Generic.HashSet[string]'
        if ($existing -and $existing.value) {
            foreach ($e in $existing.value) {
                if ($e.resourceId -eq $SpObjectId) {
                    [void]$alreadyAssigned.Add([string]$e.appRoleId)
                }
            }
        }

        $rolesToAssign = @($u.Group) + $GroupPermissions[$u.Group]
        foreach ($roleValue in $rolesToAssign) {
            $appRole = $spRoles | Where-Object { $_.value -eq $roleValue } | Select-Object -First 1
            if (-not $appRole) {
                Write-Warning "    role '$roleValue' not found on SP, skipping for $($u.Upn)"
                continue
            }

            # ── Already assigned? Skip without touching Graph ────────────────
            if ($alreadyAssigned.Contains([string]$appRole.id)) {
                Write-Host "    = $($u.Upn) already has $roleValue (skipped)" -ForegroundColor DarkGray
                continue
            }

            $bodyObj = @{
                principalId = $userOid
                resourceId  = $SpObjectId
                appRoleId   = $appRole.id
            }
            $tempBody = [System.IO.Path]::GetTempFileName()
            $bodyObj | ConvertTo-Json -Depth 4 | Set-Content -Path $tempBody -Encoding UTF8

            try {
                Invoke-AzCli @(
                    'rest','--method','POST',
                    '--url',"https://graph.microsoft.com/v1.0/users/$userOid/appRoleAssignments",
                    '--headers','Content-Type=application/json',
                    '--body',"@$tempBody"
                ) | Out-Null
                Write-Host "    + $($u.Upn) => $roleValue" -ForegroundColor Green
                [void]$alreadyAssigned.Add([string]$appRole.id)
            } catch {
                # Race-condition fallback: another runner / earlier session may
                # have created this assignment between our GET and POST.  Treat
                # the duplicate envelopes as a soft success; everything else
                # logs a warning and lets the loop carry on with the next role.
                $msg = $_.Exception.Message
                $duplicate =
                    ($msg -match 'Permission being assigned\s+(was already assigned|already exists)') -or
                    ($msg -match '"code"\s*:\s*"InvalidUpdate"')

                if ($duplicate) {
                    Write-Host "    = $($u.Upn) already has $roleValue (race)" -ForegroundColor DarkGray
                } else {
                    Write-Warning "    ! $($u.Upn) => $roleValue failed: $msg"
                    # Continue to the next role rather than aborting the script
                }
            } finally {
                Remove-Item $tempBody -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# 6. Client secret (create or rotate)
# ──────────────────────────────────────────────────────────────────────────────

$ClientSecret = $null
$SecretExpiry = $null

$existingCreds = (Invoke-AzCli @('ad','app','show','--id',$ClientId,'--query','passwordCredentials','-o','json') |
                  ConvertFrom-AzJson)
$activeCreds = $existingCreds | Where-Object {
    $_.endDateTime -and ([DateTime]$_.endDateTime) -gt (Get-Date).AddDays(30)
}

if ($RotateSecret -or -not $activeCreds) {
    Write-Host "==> Creating client secret (years=$SecretLifetimeYears)" -ForegroundColor Cyan
    $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
    $cred = Invoke-AzCli @(
        'ad','app','credential','reset',
        '--id',$ClientId,
        '--display-name',"easyauth-$(Get-Date -Format yyyyMMdd)",
        '--end-date',$endDate,
        '--append','-o','json'
    ) | ConvertFrom-AzJson
    $ClientSecret = $cred.password
    $SecretExpiry = $endDate
}

# ──────────────────────────────────────────────────────────────────────────────
# 7. Push Easy Auth (authsettingsV2)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Configuring Easy Auth" -ForegroundColor Cyan

# Pre-flight: every value must be non-empty.  The previous template-substitute
# approach silently stored "" / "api://" when a variable was unset; using the
# dedicated `az webapp auth ...` commands with explicit args removes that
# foot-gun entirely.
foreach ($pair in @(
    @{ Name = 'FunctionAppName';    Value = $FunctionAppName    },
    @{ Name = 'ResourceGroupName';  Value = $ResourceGroupName  },
    @{ Name = 'TenantId';           Value = $TenantId           },
    @{ Name = 'ClientId';           Value = $ClientId           })) {
    if ([string]::IsNullOrWhiteSpace($pair.Value)) {
        throw "Easy Auth precondition failed: `$$($pair.Name) is empty."
    }
}

# 7a. Configure the Microsoft (Entra) identity provider.  This writes the
#     identityProviders.azureActiveDirectory subtree atomically.
Invoke-AzCli @(
    'webapp','auth','microsoft','update',
    '--name',                       $FunctionAppName,
    '--resource-group',             $ResourceGroupName,
    '--client-id',                  $ClientId,
    '--client-secret-setting-name', 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET',
    '--issuer',                     "https://login.microsoftonline.com/$TenantId/v2.0",
    '--allowed-token-audiences',    "api://$ClientId",
    '--yes'
) | Out-Null

# 7b. Enable the Easy Auth platform itself and set the global behaviour
#     (AllowAnonymous keeps /Public/* accessible; our middleware enforces 401
#     on /Secure/* so we don't want the platform to redirect on us).
Invoke-AzCli @(
    'webapp','auth','update',
    '--name',           $FunctionAppName,
    '--resource-group', $ResourceGroupName,
    '--enabled',        'true',
    '--action',         'AllowAnonymous',
    '--token-store',    'true'
) | Out-Null

# 7c. Read back and verify - fail loudly if Microsoft Graph stored anything
#     other than the exact clientId we just sent.
$verify = Invoke-AzCli @(
    'webapp','auth','show',
    '--name',           $FunctionAppName,
    '--resource-group', $ResourceGroupName
) | ConvertFrom-AzJson

$storedClientId = $verify.identityProviders.azureActiveDirectory.registration.clientId
$storedEnabled  = $verify.platform.enabled

if ($storedEnabled -ne $true -or $storedClientId -ne $ClientId) {
    throw ("Easy Auth verification failed. enabled='{0}' storedClientId='{1}' expected='{2}'. " +
           "Inspect: az webapp auth show -n {3} -g {4}") `
        -f $storedEnabled, $storedClientId, $ClientId, $FunctionAppName, $ResourceGroupName
}
Write-Host "    Easy Auth verified: enabled=$storedEnabled clientId=$storedClientId" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# 8. Function App settings
# ──────────────────────────────────────────────────────────────────────────────

Write-Host "==> Updating Function App settings" -ForegroundColor Cyan

$settings = @(
    "WAREWOLF_ENTRA_TENANT_ID=$TenantId",
    "WAREWOLF_ENTRA_AUDIENCE=api://$ClientId",
    "WAREWOLF_SECURE_CONFIG=$SecureConfigMountPath"
)
if ($ClientSecret) {
    $settings += "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET=$ClientSecret"
}

Invoke-AzCli (@(
    'functionapp','config','appsettings','set',
    '--name',$FunctionAppName,
    '--resource-group',$ResourceGroupName,
    '--settings'
) + $settings) | Out-Null

# ──────────────────────────────────────────────────────────────────────────────
# 9. Persist outputs
# ──────────────────────────────────────────────────────────────────────────────

$summary = [pscustomobject]@{
    Timestamp           = (Get-Date).ToString('o')
    SubscriptionId      = $SubscriptionId
    TenantId            = $TenantId
    FunctionAppName     = $FunctionAppName
    EntraAppDisplayName = $EntraAppDisplayName
    ClientId            = $ClientId
    AppObjectId         = $AppObjectId
    SpObjectId          = $SpObjectId
    Audience            = "api://$ClientId"
    SecretRotated       = [bool]$ClientSecret
    SecretExpiry        = $SecretExpiry
    AppRoles            = $desiredRoles | Select-Object value, displayName, id
    UserAssignments     = $UserAssignments
}
$summary | ConvertTo-Json -Depth 6 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host ""
Write-Host "Done. Summary written to $OutputPath" -ForegroundColor Green
Write-Host "  ClientId:  $ClientId"
Write-Host "  Audience:  api://$ClientId"
if ($ClientSecret) {
    Write-Host "  Secret:    [stored in Function App settings - not echoed]"
}
