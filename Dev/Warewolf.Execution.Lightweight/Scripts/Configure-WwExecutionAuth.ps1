<#
.SYNOPSIS
  Provisions Microsoft Entra ID + Easy Auth for the Warewolf wwexecution
  Azure Function App.  Idempotent end-to-end - safe to re-run against an
  existing or brand-new function app.

.DESCRIPTION
  This script is the single source of truth for everything the wwexecution
  function needs at the Azure / Entra control plane:

    Stage 0   Pre-flight   - subscription, az CLI, function app exists
    Stage 1   Entra app    - create or upgrade the registration
    Stage 2   ID token     - enable implicit grant ID tokens (AADSTS700054 fix)
    Stage 3   Expose API   - identifierUris = api://<clientId>
    Stage 3b  Expose scope - oauth2PermissionScopes.user_impersonation
                             (AADSTS650057 fix - required for token acquisition
                             via OAuth clients like az CLI / MSAL)
    Stage 4   App roles    - groups + Permission.* (declarative replace)
    Stage 5   Service ppl  - create SP if missing (with Graph retry)
    Stage 6   Assignments  - users -> roles (skip-if-exists, never abort)
    Stage 7   Secret       - create or rotate; force-rotate when app setting
                             is missing because we cannot recover the old value
    Stage 8   App settings - tenant id, audience, secure config path, secret
                             (MUST be in place before Easy Auth references it)
    Stage 9   Easy Auth    - migrate V1 auth -> V2 if needed, then
                             microsoft provider + platform enable
    Stage 10  Verify       - cross-check Entra app, app settings, and Easy Auth
                             all line up; fail loudly otherwise
    Stage 11  Smoke test   - optional public + secure-401 HTTP probe

  Re-running upgrades existing objects in place; nothing is destructive.

.PARAMETER RotateSecret
  Force a fresh client secret even if a non-expired one exists.

.PARAMETER SkipUserAssignment
  Skip Stage 6 entirely (useful when running the script with limited
  Microsoft Graph permissions).

.PARAMETER SkipSmokeTest
  Skip the optional Stage 11 HTTP probe.  The probe needs the function app
  to be deployed and reachable; on a freshly-created shell app it will fail
  benignly, so set this flag for first-run setup.

.PARAMETER WhatIfOnly
  Print the resolved configuration and exit without making any changes.

.PREREQUISITES
  * Azure CLI >= 2.55  (must support `az ad app update --enable-id-token-issuance`
                        and `az webapp auth microsoft update`)
  * Caller must have Application.Administrator (or Application.ReadWrite.All)
    in the tenant, plus Contributor on the function app.

.USAGE
  az login
  ./Scripts/Configure-WwExecutionAuth.ps1
  ./Scripts/Configure-WwExecutionAuth.ps1 -RotateSecret
  ./Scripts/Configure-WwExecutionAuth.ps1 -SkipUserAssignment
  ./Scripts/Configure-WwExecutionAuth.ps1 -WhatIfOnly
#>

[CmdletBinding()]
param(
    # ── Resource targeting ────────────────────────────────────────────────────
    # Pass any of these on the command line to skip the matching prompt.
    [string]    $SubscriptionId,
    [string]    $TenantId,
    [string]    $ResourceGroupName,
    [string]    $FunctionAppName,
    [string]    $EntraAppDisplayName,
    [int]       $SecretLifetimeYears,
    [string]    $SecureConfigMountPath,

    # ── Collections (default empty; pass via splat or fill in interactively) ──
    [hashtable] $GroupPermissions,
    [array]     $UserAssignments,

    # ── Stage flags ───────────────────────────────────────────────────────────
    [switch]    $RotateSecret,
    [switch]    $SkipUserAssignment,
    [switch]    $SkipSmokeTest,
    [switch]    $WhatIfOnly,

    # Skip every interactive prompt.  Suitable for CI/CD; placeholders or
    # missing required values cause an early throw.
    [switch]    $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURATION DEFAULTS  -- placeholders are auto-prompted when interactive,
#                            or rejected with a clear error in -NonInteractive.
#                            Edit any line below to harden a value into the
#                            script (then it'll be shown as the default for
#                            confirmation rather than prompted).
# ──────────────────────────────────────────────────────────────────────────────

if (-not $PSBoundParameters.ContainsKey('SubscriptionId'))        { $SubscriptionId        = '<subscriptionId>' }
if (-not $PSBoundParameters.ContainsKey('TenantId'))              { $TenantId              = '<tenantId>' }
if (-not $PSBoundParameters.ContainsKey('ResourceGroupName'))     { $ResourceGroupName     = '<resourceGroupName>' }
if (-not $PSBoundParameters.ContainsKey('FunctionAppName'))       { $FunctionAppName       = '<functionAppName>' }
if (-not $PSBoundParameters.ContainsKey('EntraAppDisplayName'))   { $EntraAppDisplayName   = '<auto>' }   # auto-derives "<FunctionAppName>-auth"
if (-not $PSBoundParameters.ContainsKey('SecretLifetimeYears'))   { $SecretLifetimeYears   = 1 }
if (-not $PSBoundParameters.ContainsKey('SecureConfigMountPath')) { $SecureConfigMountPath = 'D:\home\site\wwwroot\secure.config' }

# Group → permissions mapping.  Every Permission.* in the right column is
# auto-assigned alongside the group role to every user mapped to that group.
# Default is empty - populate interactively or via -GroupPermissions splat.
if (-not $PSBoundParameters.ContainsKey('GroupPermissions')) { $GroupPermissions = @{} }

# Users → group assignment.  Each row: @{ Upn = '<upn>'; Group = '<groupName>' }
# Default is empty - populate interactively or via -UserAssignments splat.
if (-not $PSBoundParameters.ContainsKey('UserAssignments'))  { $UserAssignments  = @() }

$AuthSettingsTemplatePath = Join-Path $PSScriptRoot 'authsettingsV2.json'  # legacy template, retained as docs
$OutputPath               = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth.output.json'

# ── Fixed constants (rarely changed; shown for confirmation only) ──────────────
# Easy Auth references this name to look up the client_secret in app settings.
# Changing it here means the same name change is needed in Stage 9's
# --client-secret-setting-name argument; the Confirm-Configuration step will
# show this and accept an override but most setups should keep the default.
$ClientSecretSettingName = 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'

# ──────────────────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────────────────

function Format-AzArgsForLog {
    <#
        Renders an az argument array as a single line, masking values that
        look like secrets so client_secret / app keys never end up in logs.
    #>
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
    <#
        Invokes the az CLI, echoing the (secret-redacted) command before
        execution so any failure can be traced to the exact line that produced
        it.  Throws on non-zero exit; embeds the failing command in the throw.
    #>
    param([Parameter(Mandatory)][string[]] $Arguments)

    $printable = Format-AzArgsForLog -Arguments $Arguments
    Write-Host "    > az $printable" -ForegroundColor DarkGray

    $output = & az @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "az CLI failed ($LASTEXITCODE) running ``az $printable``:`n$output"
    }
    return $output
}

function ConvertFrom-AzJson {
    <#
        Aggregates every line emitted by az CLI in a StringBuilder, strips any
        non-JSON preamble (extension banners, WARNING: lines, deprecation
        notices, etc), then parses the whole buffer in `end`.

        Why the preamble strip exists: the authV2 extension (and others) print
        notices to stderr - e.g.
            WARNING: The behavior of this command has been altered by the
                     following extension: authV2
        Our `Invoke-AzCli` merges stderr into stdout via `2>&1` so the throw
        message can quote them in CI logs, but ConvertFrom-Json then chokes
        on the leading 'W'.  We find the first '{' or '[' and start parsing
        from there.

        Earlier line-by-line bug: parsing inside `process` fired once per
        line because PowerShell binds string arrays element-by-element and
        the first iteration saw just "{".  Buffer in `process`, parse in `end`.

        Returns $null when the source emitted nothing or the literal "null".
    #>
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

        # Find the first JSON-document character ({ or [) and discard anything
        # before it.  Defends against extension banners, WARNING: lines, and
        # any other prose az may have prepended to the response.
        $startBrace   = $text.IndexOf('{')
        $startBracket = $text.IndexOf('[')

        $start = -1
        if     ($startBrace   -ge 0 -and ($startBracket -lt 0 -or $startBrace   -lt $startBracket)) { $start = $startBrace   }
        elseif ($startBracket -ge 0)                                                                { $start = $startBracket }

        if ($start -gt 0) {
            $text = $text.Substring($start)
        } elseif ($start -lt 0) {
            # No JSON document found in the buffer at all - log what came
            # back so the caller can investigate, then return $null.
            Write-Verbose "ConvertFrom-AzJson: no JSON document in input. Buffer was: $text"
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
        Retries a script block on Microsoft Graph propagation failures.  Graph
        is eventually consistent: an app registration created seconds ago is
        not always visible to the replica that az ad sp create reads from.
        Backs off exponentially (2,4,8,16,32,60s) up to MaxAttempts times.
    #>
    param(
        [Parameter(Mandatory)][scriptblock] $ScriptBlock,
        [int]    $MaxAttempts        = 6,
        [int]    $InitialDelaySeconds = 2,
        [string] $OperationName       = 'operation'
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
        return Invoke-AzCli @('ad','sp','show','--id',$AppId,'-o','json') | ConvertFrom-AzJson
    } catch {
        return $null
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

function Get-FunctionAppSetting {
    <#
        Returns the value (as string) of a single app setting, or $null when
        the setting is missing or empty.  Avoids az's `--query` for safer
        empty-string handling.
    #>
    param([string] $Name, [string] $ResourceGroup, [string] $SettingName)

    $list = Invoke-AzCli @(
        'functionapp','config','appsettings','list',
        '--name',$Name,'--resource-group',$ResourceGroup,'-o','json'
    ) | ConvertFrom-AzJson

    if (-not $list) { return $null }
    $hit = $list | Where-Object { $_.name -eq $SettingName } | Select-Object -First 1
    if (-not $hit) { return $null }
    if ([string]::IsNullOrWhiteSpace($hit.value)) { return $null }
    return [string]$hit.value
}

function Get-DeepValue {
    <#
        Strict-mode-safe deep property access.  Walks a sequence of property
        names from $Obj and returns $null at the first missing or null
        intermediate, instead of throwing
            "The property 'X' cannot be found on this object."

        Usage:
            Get-DeepValue $auth @('identityProviders','azureActiveDirectory','registration','clientId')
    #>
    param(
        [Parameter(Mandatory, Position=0)] $Obj,
        [Parameter(Mandatory, Position=1)][string[]] $Path
    )
    foreach ($segment in $Path) {
        if ($null -eq $Obj) { return $null }
        if (-not $Obj.PSObject) { return $null }
        $prop = $Obj.PSObject.Properties[$segment]
        if (-not $prop) { return $null }
        $Obj = $prop.Value
    }
    return $Obj
}

function Test-IsPlaceholder {
    <#
        Returns $true when a value is unset (null/whitespace) or matches the
        <name> placeholder pattern.  Used to decide whether a variable needs
        user input.
    #>
    param([string] $Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return $true }
    return $Value -match '^<[^>]+>$'
}

function Read-ScalarVariable {
    <#
        Prompts for a single value with an obvious default.  When the current
        value is a placeholder and the variable is required, loops until the
        user supplies something.
    #>
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
        [int] $Min = 0,
        [int] $Max = [int]::MaxValue
    )
    while ($true) {
        Write-Host "  $Name" -ForegroundColor White
        $userInput = Read-Host "    [Enter = $Current]"
        if ($null -ne $userInput) { $userInput = $userInput.Trim() }
        if ([string]::IsNullOrWhiteSpace($userInput)) { return $Current }
        $parsed = 0
        if (-not [int]::TryParse($userInput, [ref]$parsed)) {
            Write-Host "    (not a valid integer)" -ForegroundColor Yellow
            continue
        }
        if ($parsed -lt $Min -or $parsed -gt $Max) {
            Write-Host "    (must be between $Min and $Max)" -ForegroundColor Yellow
            continue
        }
        return $parsed
    }
}

function Read-GroupPermissions {
    <#
        Interactive editor for the Group => Permissions hashtable.
        Actions: keep / replace / add / clear.

        Container is always a fresh hashtable; we copy keys explicitly to avoid
        any reference-sharing surprises if the caller mutates the result.
    #>
    param([hashtable] $Current)
    if ($null -eq $Current) { $Current = @{} }

    Write-Host ""
    Write-Host "  GroupPermissions  (group => Permission.* roles)" -ForegroundColor White
    if ($Current.Count -eq 0) {
        Write-Host "    [current] (empty)" -ForegroundColor DarkGray
    } else {
        foreach ($k in $Current.Keys) {
            Write-Host "    [current] $k => $($Current[$k] -join ', ')" -ForegroundColor DarkGray
        }
    }

    $action = (Read-Host "    [k]eep / [a]dd / [r]eplace / [c]lear  (default: k)").ToLower()
    if ([string]::IsNullOrWhiteSpace($action)) { $action = 'k' }

    switch ($action[0]) {
        'k' { return $Current }
        'c' { return @{} }
        default {
            $result = @{}
            if ($action[0] -eq 'a') {
                foreach ($k in $Current.Keys) { $result[$k] = $Current[$k] }
            }

            Write-Host "    Adding entries (empty group name to finish):" -ForegroundColor DarkGray
            while ($true) {
                $groupName = Read-Host "      Group name"
                if ([string]::IsNullOrWhiteSpace($groupName)) { break }
                $permsRaw = Read-Host "      Permissions for '$groupName' (comma-separated, empty for none)"
                $perms = if ([string]::IsNullOrWhiteSpace($permsRaw)) {
                    ,@()                       # comma forces empty array, never `$null`
                } else {
                    ,@(($permsRaw -split ',') | ForEach-Object { $_.Trim() } | Where-Object { $_ })
                }
                $result[$groupName.Trim()] = $perms
            }
            return $result
        }
    }
}

function Read-UserAssignments {
    <#
        Interactive editor for the User => Group list.
        Actions: keep / replace / add / clear.

        Uses a strongly-typed List[object] for accumulation rather than `+=`
        on a `[array]`.  PowerShell's `+=` on `@()` with a hashtable on the
        right is fragile: under some builds and modes the hashtable is folded
        into the array as DictionaryEntry pairs, or the array is coerced to
        a hashtable, both of which cause "Item has already been added.
        Key in dictionary: 'Upn'" on the second iteration.  List[object].Add
        avoids the entire ambiguity.
    #>
    param([array] $Current)
    if ($null -eq $Current) { $Current = @() }

    Write-Host ""
    Write-Host "  UserAssignments  (UPN => group)" -ForegroundColor White
    if ($Current.Count -eq 0) {
        Write-Host "    [current] (empty)" -ForegroundColor DarkGray
    } else {
        foreach ($u in $Current) {
            Write-Host "    [current] $($u.Upn) => $($u.Group)" -ForegroundColor DarkGray
        }
    }

    $action = (Read-Host "    [k]eep / [a]dd / [r]eplace / [c]lear  (default: k)").ToLower()
    if ([string]::IsNullOrWhiteSpace($action)) { $action = 'k' }

    switch ($action[0]) {
        'k' { return ,$Current }
        'c' { return ,@()      }
        default {
            $list = New-Object 'System.Collections.Generic.List[object]'
            if ($action[0] -eq 'a') {
                foreach ($u in $Current) { [void]$list.Add($u) }
            }

            Write-Host "    Adding entries (empty UPN to finish):" -ForegroundColor DarkGray
            while ($true) {
                $upn = Read-Host "      User UPN"
                if ([string]::IsNullOrWhiteSpace($upn)) { break }
                $group = Read-Host "      Group for '$upn'"
                if ([string]::IsNullOrWhiteSpace($group)) {
                    Write-Host "      (group is required - skipping)" -ForegroundColor Yellow
                    continue
                }
                [void]$list.Add(@{ Upn = $upn.Trim(); Group = $group.Trim() })
            }
            # Comma operator on return prevents PowerShell from unrolling a
            # single-element array into a bare hashtable on the caller side.
            return ,$list.ToArray()
        }
    }
}

function Format-CollectionForSummary {
    <#
        Renders an array or hashtable for the confirmation summary in a way
        the user can actually read.
    #>
    param($Value)
    if ($null -eq $Value) { return '(none)' }
    if ($Value -is [hashtable]) {
        if ($Value.Count -eq 0) { return '(empty)' }
        return ($Value.Keys | Sort-Object | ForEach-Object {
            "$_ => $($Value[$_] -join ', ')"
        }) -join '; '
    }
    if ($Value -is [array]) {
        if ($Value.Count -eq 0) { return '(empty)' }
        return ($Value | ForEach-Object {
            if ($_ -is [hashtable]) { "$($_.Upn) => $($_.Group)" } else { "$_" }
        }) -join '; '
    }
    return [string]$Value
}

function Get-EasyAuthConfig {
    <#
        Returns the V2 Easy Auth configuration, normalised to the inner
        properties shape regardless of which authV2 CLI extension version is
        installed.

        Different builds of the extension return different shapes from
        `az webapp auth show`:
          * older / some installs: the inner properties subtree directly
            ({ identityProviders, platform, login, ... })
          * current / others:      the full ARM resource
            ({ id, name, properties: { identityProviders, platform, ... }, type })

        We normalise by returning .properties when present, otherwise the
        root.  Callers can then always do
            (Get-EasyAuthConfig …).identityProviders.azureActiveDirectory.…
        without caring about extension version.
    #>
    param([string] $Name, [string] $ResourceGroup)

    $raw = Invoke-AzCli @(
        'webapp','auth','show','--name',$Name,'--resource-group',$ResourceGroup
    ) | ConvertFrom-AzJson

    if ($null -eq $raw) { return $null }
    if ($raw.PSObject -and $raw.PSObject.Properties['properties']) {
        return $raw.properties
    }
    return $raw
}

# ──────────────────────────────────────────────────────────────────────────────
# Interactive configuration  -- runs unless -NonInteractive is passed.
# Resolves placeholders, displays the full plan, and asks for confirmation
# before any cloud change happens.
# ──────────────────────────────────────────────────────────────────────────────

if ($NonInteractive) {
    Write-Host "═══ Configuration (non-interactive) ════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Skipping prompts.  Validating placeholders..." -ForegroundColor DarkGray

    $required = @(
        @{ Name = 'SubscriptionId';    Value = $SubscriptionId    },
        @{ Name = 'TenantId';          Value = $TenantId          },
        @{ Name = 'ResourceGroupName'; Value = $ResourceGroupName },
        @{ Name = 'FunctionAppName';   Value = $FunctionAppName   }
    )
    foreach ($r in $required) {
        if (Test-IsPlaceholder $r.Value) {
            throw ("[$($r.Name)] is unset or still a placeholder ('$($r.Value)'). " +
                   "Pass -$($r.Name) <value> on the command line, edit the script's " +
                   "default, or omit -NonInteractive to be prompted.")
        }
    }
    if (Test-IsPlaceholder $EntraAppDisplayName) { $EntraAppDisplayName = "$FunctionAppName-auth" }
} else {
    Write-Host ""
    Write-Host "═══ Interactive configuration ═════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Hit Enter to keep the [bracketed] default; type a value to override." -ForegroundColor DarkGray
    Write-Host "  Required values (shown without a default) must be entered." -ForegroundColor DarkGray
    Write-Host ""

    $SubscriptionId        = Read-ScalarVariable -Name 'SubscriptionId'        -Current $SubscriptionId        -Description 'Azure subscription GUID'    -Required
    $TenantId              = Read-ScalarVariable -Name 'TenantId'              -Current $TenantId              -Description 'Microsoft Entra tenant GUID' -Required
    $ResourceGroupName     = Read-ScalarVariable -Name 'ResourceGroupName'     -Current $ResourceGroupName     -Description 'Existing resource group'     -Required
    $FunctionAppName       = Read-ScalarVariable -Name 'FunctionAppName'       -Current $FunctionAppName       -Description 'Existing function app'       -Required

    # EntraAppDisplayName: default to "<FunctionAppName>-auth" if still <auto>
    if (Test-IsPlaceholder $EntraAppDisplayName -or $EntraAppDisplayName -eq '<auto>') {
        $EntraAppDisplayName = "$FunctionAppName-auth"
    }
    $EntraAppDisplayName   = Read-ScalarVariable -Name 'EntraAppDisplayName'   -Current $EntraAppDisplayName   -Description 'Entra app registration name'

    $SecretLifetimeYears   = Read-IntVariable    -Name 'SecretLifetimeYears'   -Current $SecretLifetimeYears   -Min 1 -Max 2
    $SecureConfigMountPath = Read-ScalarVariable -Name 'SecureConfigMountPath' -Current $SecureConfigMountPath -Description 'Path used by WAREWOLF_SECURE_CONFIG'

    Write-Host ""
    Write-Host "  Fixed constants - press Enter to accept, type to override:" -ForegroundColor White
    $ClientSecretSettingName = Read-ScalarVariable -Name 'ClientSecretSettingName' -Current $ClientSecretSettingName -Description 'Easy Auth references this name'

    $GroupPermissions = Read-GroupPermissions -Current $GroupPermissions
    $UserAssignments  = Read-UserAssignments  -Current $UserAssignments

    # ── Configuration summary + confirmation ──────────────────────────────────
    Write-Host ""
    Write-Host "═══ Configuration summary ═════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ("  {0,-25} : {1}" -f 'SubscriptionId',          $SubscriptionId)
    Write-Host ("  {0,-25} : {1}" -f 'TenantId',                $TenantId)
    Write-Host ("  {0,-25} : {1}" -f 'ResourceGroupName',       $ResourceGroupName)
    Write-Host ("  {0,-25} : {1}" -f 'FunctionAppName',         $FunctionAppName)
    Write-Host ("  {0,-25} : {1}" -f 'EntraAppDisplayName',     $EntraAppDisplayName)
    Write-Host ("  {0,-25} : {1}" -f 'SecretLifetimeYears',     $SecretLifetimeYears)
    Write-Host ("  {0,-25} : {1}" -f 'SecureConfigMountPath',   $SecureConfigMountPath)
    Write-Host ("  {0,-25} : {1}" -f 'ClientSecretSettingName', $ClientSecretSettingName)
    Write-Host ("  {0,-25} : {1}" -f 'GroupPermissions',        (Format-CollectionForSummary $GroupPermissions))
    Write-Host ("  {0,-25} : {1}" -f 'UserAssignments',         (Format-CollectionForSummary $UserAssignments))
    Write-Host ("  {0,-25} : {1}" -f 'RotateSecret',            $RotateSecret)
    Write-Host ("  {0,-25} : {1}" -f 'SkipUserAssignment',      $SkipUserAssignment)
    Write-Host ("  {0,-25} : {1}" -f 'SkipSmokeTest',           $SkipSmokeTest)
    Write-Host ("  {0,-25} : {1}" -f 'WhatIfOnly',              $WhatIfOnly)
    Write-Host ""

    $proceed = Read-Host "  Proceed with these values? [Y/n]"
    if ([string]::IsNullOrWhiteSpace($proceed)) { $proceed = 'y' }
    if ($proceed -notmatch '^[yY]') {
        Write-Host "  Aborted by user." -ForegroundColor Yellow
        return
    }
}

# Final cross-check: warn (don't fail) when a user assignment references a
# group with no entry in GroupPermissions - that user will get only the
# group role, no Permission.* roles.
foreach ($u in $UserAssignments) {
    if (-not ($GroupPermissions -is [hashtable]) -or -not $GroupPermissions.ContainsKey($u.Group)) {
        Write-Warning ("UserAssignment '$($u.Upn)' references group '$($u.Group)' with no " +
                       "GroupPermissions entry; that user will receive only the group role.")
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 0 — Pre-flight
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 0  Pre-flight ═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    Subscription:    $SubscriptionId"
Write-Host "    Tenant:          $TenantId"
Write-Host "    Resource group:  $ResourceGroupName"
Write-Host "    Function app:    $FunctionAppName"
Write-Host "    Entra app:       $EntraAppDisplayName"
Write-Host "    Secret rotate:   $RotateSecret"
Write-Host "    Skip users:      $SkipUserAssignment"
Write-Host "    Skip smoke test: $SkipSmokeTest"
Write-Host "    What-if only:    $WhatIfOnly"
Write-Host ""

if ($WhatIfOnly) {
    Write-Host "    -WhatIfOnly set; exiting before any change." -ForegroundColor Yellow
    return
}

Invoke-AzCli @('account','set','--subscription',$SubscriptionId) | Out-Null

# Hard fail when the function app doesn't exist yet — wrong name was the
# silent root cause of 'enabled:null clientId:null' that wasted hours.
if (-not (Test-FunctionAppExists -Name $FunctionAppName -ResourceGroup $ResourceGroupName)) {
    throw ("Function app '$FunctionAppName' was not found in resource group '$ResourceGroupName'. " +
           "Create it first or correct `$FunctionAppName / `$ResourceGroupName at the top of this script.")
}
Write-Host "    Function app exists." -ForegroundColor Green

if (-not (Test-Path $AuthSettingsTemplatePath)) {
    Write-Warning "authsettingsV2.json not found at $AuthSettingsTemplatePath - it is documentation only, not strictly required."
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 1 — Entra app registration  (create or upgrade)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 1  Entra app registration ════════════════════════════════" -ForegroundColor Cyan

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

    Write-Host "    waiting 15s for Microsoft Graph replication" -ForegroundColor DarkGray
    Start-Sleep -Seconds 15
} else {
    Write-Host "    found existing appId $($app.appId) - upgrading in place" -ForegroundColor Green
}

$ClientId    = $app.appId
$AppObjectId = $app.id

# Re-assert the redirect URI on every run (the `--web-redirect-uris` flag is
# replace-not-append, so we pass exactly one value).
Invoke-AzCli @(
    'ad','app','update','--id',$ClientId,
    '--web-redirect-uris',"https://$FunctionAppName.azurewebsites.net/.auth/login/aad/callback"
) | Out-Null

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2 — Implicit-grant ID-token issuance  (AADSTS700054 fix)
# ──────────────────────────────────────────────────────────────────────────────
# Easy Auth on Functions uses response_type=code+id_token (hybrid flow).
# Without enableIdTokenIssuance=true on the registration, the AAD authorize
# endpoint rejects the request with AADSTS700054 and the browser sign-in
# stalls at /.auth/login/aad/callback with an error payload.

Write-Host ""
Write-Host "═══ Stage 2  Implicit grant - ID token issuance ══════════════════" -ForegroundColor Cyan

try {
    Invoke-AzCli @(
        'ad','app','update','--id',$ClientId,
        '--enable-id-token-issuance','true'
    ) | Out-Null
} catch {
    # Older az CLI versions may not have the flag.  Fall back to a Graph PATCH.
    Write-Host "    --enable-id-token-issuance not supported by az CLI; using Graph PATCH" -ForegroundColor DarkYellow
    $tempPatch = [System.IO.Path]::GetTempFileName()
    @{ web = @{ implicitGrantSettings = @{ enableIdTokenIssuance = $true; enableAccessTokenIssuance = $false } } } |
        ConvertTo-Json -Depth 6 | Set-Content -Path $tempPatch -Encoding UTF8
    try {
        Invoke-AzCli @(
            'rest','--method','PATCH',
            '--url',"https://graph.microsoft.com/v1.0/applications/$AppObjectId",
            '--headers','Content-Type=application/json',
            '--body',"@$tempPatch"
        ) | Out-Null
    } finally {
        Remove-Item $tempPatch -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "    enableIdTokenIssuance=true on $ClientId" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 3 — Expose api://<clientId>
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 3  Expose API ═══════════════════════════════════════════" -ForegroundColor Cyan
Invoke-AzCli @(
    'ad','app','update','--id',$ClientId,
    '--identifier-uris',"api://$ClientId"
) | Out-Null
Write-Host "    identifierUris=[api://$ClientId]" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 3b — Expose user_impersonation OAuth2 scope
# ──────────────────────────────────────────────────────────────────────────────
# Why this is necessary: Stage 3 only sets identifierUris, which makes
# `api://<clientId>` a valid resource value but does NOT declare any delegated
# permissions an OAuth client can request.  Without at least one entry in
# api.oauth2PermissionScopes, any attempt to acquire a delegated token via
#     az login --scope "api://<clientId>/.default"
#     az account get-access-token --resource api://<clientId>
# fails with
#     AADSTS650057: Invalid resource. List of valid resources from app
#     registration: <empty>
# Adding `user_impersonation` is the standard one-and-done fix.  Idempotent:
# we look up existing scopes by .value and only PATCH when the scope is
# missing, which means re-runs preserve the GUID assigned to the scope so
# any prior consents stay valid.

Write-Host ""
Write-Host "═══ Stage 3b  Expose user_impersonation scope ═════════════════════" -ForegroundColor Cyan

# Read existing scopes.  Force-array via @(...) so $existingScopes is always
# an enumerable, even when the resource has zero scopes (returns $null/[]
# from ConvertFrom-AzJson) or exactly one scope (which strict-mode would
# otherwise treat as a bare object).
$rawScopes = Invoke-AzCli @(
    'ad','app','show','--id',$ClientId,
    '--query','api.oauth2PermissionScopes','-o','json'
) | ConvertFrom-AzJson

$existingScopes = @()
if ($null -ne $rawScopes) { $existingScopes = @($rawScopes) }

# Helper: read a property from a PSCustomObject without triggering the
# strict-mode "property X cannot be found" error if the field is missing.
function Get-ScopeProperty {
    param($Scope, [string] $Name, $Default = $null)
    if ($null -eq $Scope) { return $Default }
    if (-not $Scope.PSObject) { return $Default }
    $prop = $Scope.PSObject.Properties[$Name]
    if (-not $prop) { return $Default }
    return $prop.Value
}

$hasUserImpersonation = $false
foreach ($s in $existingScopes) {
    $val = Get-ScopeProperty -Scope $s -Name 'value'
    $en  = Get-ScopeProperty -Scope $s -Name 'isEnabled' -Default $false
    if ($val -eq 'user_impersonation' -and $en -eq $true) {
        $hasUserImpersonation = $true
        break
    }
}

if ($hasUserImpersonation) {
    Write-Host "    user_impersonation scope already present - keeping existing GUID" -ForegroundColor Green
} else {
    Write-Host "    adding user_impersonation scope" -ForegroundColor Yellow

    # New scope as a HASHTABLE - strict-mode safe and serialises cleanly with
    # ConvertTo-Json regardless of PowerShell version.  Earlier PSCustomObject
    # construction tripped "Argument types do not match" on certain builds
    # when the object was wrapped in @() and run through ConvertTo-Json.
    $newScope = @{
        id                      = [guid]::NewGuid().ToString()
        adminConsentDescription = "Allow the application to access $FunctionAppName on behalf of the signed-in user."
        adminConsentDisplayName = "Access $FunctionAppName"
        userConsentDescription  = "Allow the application to access $FunctionAppName on your behalf."
        userConsentDisplayName  = "Access $FunctionAppName"
        value                   = 'user_impersonation'
        type                    = 'User'
        isEnabled               = $true
    }

    # Carry forward any other scopes the operator may have added manually.
    # Convert each existing PSCustomObject -> hashtable so the whole array
    # has uniform type before serialisation; strongly-typed List[hashtable]
    # avoids the array-+= gotchas seen elsewhere in this script.
    $scopeList = New-Object 'System.Collections.Generic.List[hashtable]'
    foreach ($s in $existingScopes) {
        $val = Get-ScopeProperty -Scope $s -Name 'value'
        if ($val -eq 'user_impersonation') { continue }

        $copy = @{}
        if ($s.PSObject) {
            foreach ($p in $s.PSObject.Properties) { $copy[$p.Name] = $p.Value }
        }
        [void]$scopeList.Add($copy)
    }
    [void]$scopeList.Add($newScope)

    $body = @{
        api = @{
            requestedAccessTokenVersion = 2
            oauth2PermissionScopes      = $scopeList.ToArray()
        }
    }

    $scopePatch = [System.IO.Path]::GetTempFileName()
    try {
        $body | ConvertTo-Json -Depth 6 | Set-Content -Path $scopePatch -Encoding UTF8
        Invoke-AzCli @(
            'rest','--method','PATCH',
            '--url',"https://graph.microsoft.com/v1.0/applications/$AppObjectId",
            '--headers','Content-Type=application/json',
            '--body',"@$scopePatch"
        ) | Out-Null
    } finally {
        Remove-Item $scopePatch -Force -ErrorAction SilentlyContinue
    }

    Write-Host "    user_impersonation scope added" -ForegroundColor Green
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 4 — App roles  (declarative replace, preserving existing IDs)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 4  App roles ════════════════════════════════════════════" -ForegroundColor Cyan

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

# Keep existing role IDs to avoid invalidating live appRoleAssignments
$current = Invoke-AzCli @('ad','app','show','--id',$ClientId,'--query','appRoles','-o','json') |
           ConvertFrom-AzJson
foreach ($desired in $desiredRoles) {
    $existing = $current | Where-Object { $_.value -eq $desired.value } | Select-Object -First 1
    if ($existing) { $desired.id = $existing.id }
}

$tempPatch = [System.IO.Path]::GetTempFileName()
@{ appRoles = $desiredRoles } | ConvertTo-Json -Depth 6 | Set-Content -Path $tempPatch -Encoding UTF8
try {
    Invoke-AzCli @(
        'rest','--method','PATCH',
        '--url',"https://graph.microsoft.com/v1.0/applications/$AppObjectId",
        '--headers','Content-Type=application/json',
        '--body',"@$tempPatch"
    ) | Out-Null
} finally {
    Remove-Item $tempPatch -Force -ErrorAction SilentlyContinue
}
Write-Host "    reconciled $($desiredRoles.Count) app roles" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 5 — Service principal
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 5  Service principal ════════════════════════════════════" -ForegroundColor Cyan

$sp = Get-EntraServicePrincipal -AppId $ClientId
if (-not $sp) {
    $sp = Invoke-WithRetry -OperationName 'sp create' -ScriptBlock {
        Invoke-AzCli @('ad','sp','create','--id',$ClientId,'-o','json') | ConvertFrom-AzJson
    }
}
$SpObjectId = $sp.id
Write-Host "    SP objectId=$SpObjectId" -ForegroundColor Green

# Re-fetch with the SP-side appRoles list (Graph mirrors the app's appRoles
# onto the SP, but with eventual-consistency lag).
$spRoles = Invoke-WithRetry -OperationName 'sp show appRoles' -ScriptBlock {
    Invoke-AzCli @('ad','sp','show','--id',$ClientId,'--query','appRoles','-o','json') | ConvertFrom-AzJson
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 6 — User → role assignments  (skip-if-exists, never abort on one row)
# ──────────────────────────────────────────────────────────────────────────────

if (-not $SkipUserAssignment) {
    Write-Host ""
    Write-Host "═══ Stage 6  User role assignments ═══════════════════════════════" -ForegroundColor Cyan

    foreach ($u in $UserAssignments) {
        $userObj = Invoke-AzCli @('ad','user','show','--id',$u.Upn,'-o','json') | ConvertFrom-AzJson
        $userOid = $userObj.id

        # Pre-fetch existing assignments on this user, keyed to OUR SP only,
        # so we can decide locally whether to POST or skip.
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
                $msg = $_.Exception.Message
                $duplicate =
                    ($msg -match 'Permission being assigned\s+(was already assigned|already exists)') -or
                    ($msg -match '"code"\s*:\s*"InvalidUpdate"')

                if ($duplicate) {
                    Write-Host "    = $($u.Upn) already has $roleValue (race)" -ForegroundColor DarkGray
                } else {
                    Write-Warning "    ! $($u.Upn) => $roleValue failed: $msg"
                }
            } finally {
                Remove-Item $tempBody -Force -ErrorAction SilentlyContinue
            }
        }
    }
} else {
    Write-Host ""
    Write-Host "═══ Stage 6  User role assignments (skipped via -SkipUserAssignment)" -ForegroundColor DarkYellow
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 7 — Client secret (rotate-or-keep, with app-setting awareness)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 7  Client secret ════════════════════════════════════════" -ForegroundColor Cyan

# We rotate when ANY of:
#   - caller passed -RotateSecret
#   - no current credential has more than 30 days remaining
#   - the MICROSOFT_PROVIDER_AUTHENTICATION_SECRET app setting is missing
#     (we cannot recover the old secret value, so it has to be recreated)
$existingCreds = Invoke-AzCli @(
    'ad','app','show','--id',$ClientId,'--query','passwordCredentials','-o','json'
) | ConvertFrom-AzJson
$activeCreds = $existingCreds | Where-Object {
    $_.endDateTime -and ([DateTime]$_.endDateTime) -gt (Get-Date).AddDays(30)
}

$existingSecretSetting = Get-FunctionAppSetting `
    -Name $FunctionAppName -ResourceGroup $ResourceGroupName -SettingName $ClientSecretSettingName

$rotationReason = $null
if ($RotateSecret)               { $rotationReason = '-RotateSecret flag' }
elseif (-not $activeCreds)       { $rotationReason = 'no credential has >30 days remaining' }
elseif (-not $existingSecretSetting) {
    $rotationReason = "app setting '$ClientSecretSettingName' missing"
}

$ClientSecret = $null
$SecretExpiry = $null
if ($rotationReason) {
    Write-Host "    rotating (reason: $rotationReason)" -ForegroundColor Yellow
    $endDate = (Get-Date).AddYears($SecretLifetimeYears).ToString('yyyy-MM-dd')
    $cred = Invoke-AzCli @(
        'ad','app','credential','reset',
        '--id',$ClientId,
        '--display-name',"easyauth-$(Get-Date -Format yyyyMMddHHmm)",
        '--end-date',$endDate,
        '--append','-o','json'
    ) | ConvertFrom-AzJson
    $ClientSecret = $cred.password
    $SecretExpiry = $endDate
    Write-Host "    new secret created, expires $endDate" -ForegroundColor Green
} else {
    Write-Host "    keeping existing credential (>30 days remain, app setting present)" -ForegroundColor Green
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 8 — Function App settings  (must precede Easy Auth which references them)
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 8  Function App settings ════════════════════════════════" -ForegroundColor Cyan

$settings = @(
    "WAREWOLF_ENTRA_TENANT_ID=$TenantId",
    "WAREWOLF_ENTRA_AUDIENCE=api://$ClientId",
    "WAREWOLF_SECURE_CONFIG=$SecureConfigMountPath"
)
if ($ClientSecret) {
    $settings += "$ClientSecretSettingName=$ClientSecret"
}

Invoke-AzCli (@(
    'functionapp','config','appsettings','set',
    '--name',$FunctionAppName,
    '--resource-group',$ResourceGroupName,
    '--settings'
) + $settings) | Out-Null
Write-Host "    wrote $($settings.Count) app setting(s)" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 9 — Easy Auth
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 9  Easy Auth ════════════════════════════════════════════" -ForegroundColor Cyan

# Pre-flight - none of these can be empty or Easy Auth will silently store ""
foreach ($pair in @(
    @{ Name='FunctionAppName';   Value=$FunctionAppName   },
    @{ Name='ResourceGroupName'; Value=$ResourceGroupName },
    @{ Name='TenantId';          Value=$TenantId          },
    @{ Name='ClientId';          Value=$ClientId          })) {
    if ([string]::IsNullOrWhiteSpace($pair.Value)) {
        throw "Easy Auth precondition failed: `$$($pair.Name) is empty."
    }
}

# 9-pre. Migrate Easy Auth V1 -> V2 if needed.
#
#   Some freshly-created function apps default to authsettings V1 (the legacy
#   resource), and any V2 command refuses outright with:
#     ERROR: Usage Error: Cannot use auth v2 commands when the app is using
#            auth v1. Update the auth settings using the az webapp auth-classic
#            command group.
#
#   The probe MUST be `webapp auth config-version show` rather than the V1
#   resource's `enabled` flag - Azure considers an app to be "using V1" by
#   the existence of any V1 state, not just enabled=true.  An earlier
#   implementation that probed `authsettings/list` and checked
#   `properties.enabled` reported "V1 not active" while the runtime
#   simultaneously rejected V2 commands.  Use the dedicated command.

function Invoke-WebAppAuthV1Migration {
    <#
        Calls `az webapp auth config-version upgrade`, tolerating the --yes
        flag drift across authV2 extension builds (older versions don't have
        it; newer ones require it for non-interactive runs).
    #>
    param(
        [Parameter(Mandatory)][string] $FunctionAppName,
        [Parameter(Mandatory)][string] $ResourceGroupName
    )

    $upgradeArgs = @(
        'webapp','auth','config-version','upgrade',
        '--name',           $FunctionAppName,
        '--resource-group', $ResourceGroupName,
        '--yes'
    )
    try {
        Invoke-AzCli $upgradeArgs | Out-Null
    } catch {
        $msg = $_.Exception.Message
        if ($msg -match 'unrecognized arguments.*--yes') {
            Write-Host "    --yes not supported by this az CLI; retrying without" -ForegroundColor DarkYellow
            Invoke-AzCli ($upgradeArgs | Where-Object { $_ -ne '--yes' }) | Out-Null
        } elseif ($msg -match 'already.*V2|already on V2|V2.*already') {
            # Some builds error here even when migration succeeds quietly.
            Write-Host "    upgrade reported 'already V2' - treating as success" -ForegroundColor DarkGray
        } else {
            throw
        }
    }
}

$configVersion = $null
try {
    $cv = Invoke-AzCli @(
        'webapp','auth','config-version','show',
        '--name',           $FunctionAppName,
        '--resource-group', $ResourceGroupName
    ) | ConvertFrom-AzJson

    if ($cv) {
        if ($cv -is [string]) {
            # Some builds return the bare string "v1" / "v2" instead of an object.
            $configVersion = $cv
        } elseif ($cv.PSObject -and $cv.PSObject.Properties['configVersion']) {
            $configVersion = [string]$cv.configVersion
        }
    }
} catch {
    Write-Verbose "config-version show failed (will assume V1): $($_.Exception.Message)"
}

if ($configVersion) {
    Write-Host "    detected auth config version: $configVersion" -ForegroundColor DarkGray
} else {
    Write-Host "    auth config version unknown (assuming V1)" -ForegroundColor DarkGray
}

if ($configVersion -ne 'v2') {
    Write-Host "    migrating auth V1 -> V2 (one-time)" -ForegroundColor Yellow
    Invoke-WebAppAuthV1Migration -FunctionAppName $FunctionAppName -ResourceGroupName $ResourceGroupName
    Write-Host "    auth migrated to V2" -ForegroundColor Green
} else {
    Write-Host "    auth already at V2 - safe to apply V2 commands" -ForegroundColor DarkGray
}

# Wrapper that runs an authV2 command, and if it fails with the V1-blocked
# error, runs the migration and retries once.  This is the final safety net
# in case the explicit version check above missed something (CLI version
# drift, race with another tool, etc).
function Invoke-AuthV2OrMigrate {
    param(
        [Parameter(Mandatory)][string[]] $Arguments,
        [Parameter(Mandatory)][string]   $FunctionAppName,
        [Parameter(Mandatory)][string]   $ResourceGroupName
    )
    try {
        return Invoke-AzCli $Arguments
    } catch {
        if ($_.Exception.Message -match 'auth v2.*auth v1|auth-classic') {
            Write-Host "    V2 command rejected by V1 - migrating and retrying" -ForegroundColor Yellow
            Invoke-WebAppAuthV1Migration -FunctionAppName $FunctionAppName -ResourceGroupName $ResourceGroupName
            return Invoke-AzCli $Arguments
        }
        throw
    }
}

# 9a. Microsoft (Entra) provider — atomic write of the AAD subtree.
#     Note: --issuer and --tenant-id are mutually exclusive in az CLI.  We use
#     --issuer because we want the v2.0 endpoint explicitly.
Invoke-AuthV2OrMigrate -FunctionAppName $FunctionAppName -ResourceGroupName $ResourceGroupName -Arguments @(
    'webapp','auth','microsoft','update',
    '--name',                       $FunctionAppName,
    '--resource-group',             $ResourceGroupName,
    '--client-id',                  $ClientId,
    '--client-secret-setting-name', $ClientSecretSettingName,
    '--issuer',                     "https://login.microsoftonline.com/$TenantId/v2.0",
    '--allowed-token-audiences',    "api://$ClientId",
    '--yes'
) | Out-Null

# 9b. Platform - enabled + AllowAnonymous.  AllowAnonymous keeps /Public/*
#     open; the worker's EasyAuthRedirectMiddleware enforces 401/302 on
#     /Secure/* itself.
#
#     Note on the missing --token-store flag: it was renamed twice across
#     authV2 extension releases (--enable-token-store → --token-store) and
#     some builds drop it from this subcommand entirely.  Easy Auth on the
#     Microsoft (AAD) provider auto-enables tokenStore as a side-effect of
#     `az webapp auth microsoft update`, so we don't pass it here.  Stage
#     9c below explicitly reasserts it via GET-modify-PUT if it's somehow
#     off, and Stage 10 fails the run if it's still false.
Invoke-AuthV2OrMigrate -FunctionAppName $FunctionAppName -ResourceGroupName $ResourceGroupName -Arguments @(
    'webapp','auth','update',
    '--name',           $FunctionAppName,
    '--resource-group', $ResourceGroupName,
    '--enabled',        'true',
    '--action',         'AllowAnonymous'
) | Out-Null

# 9c. Belt-and-braces: re-assert tokenStore.enabled=true via GET-modify-PUT.
#     Microsoft.Web/sites/{name}/config/authsettingsV2 only accepts GET and
#     PUT (PATCH returns 405 Method Not Allowed - it's a control-plane
#     declaration, not a permissions issue).  We GET the live config, flip
#     just login.tokenStore.enabled, and PUT the whole properties subtree
#     back.  This is idempotent and preserves every other field intact.
$authUrl = ("https://management.azure.com/subscriptions/$SubscriptionId" +
            "/resourceGroups/$ResourceGroupName/providers/Microsoft.Web" +
            "/sites/$FunctionAppName/config/authsettingsV2?api-version=2022-03-01")

$liveAuth = Invoke-AzCli @('rest','--method','GET','--url',$authUrl) | ConvertFrom-AzJson

# Defensive null-check chain - any of these can be missing on a freshly-
# upgraded app where 9a/9b just wrote the bare minimum config.
$hasProperties = $false
if ($liveAuth -and $liveAuth.PSObject -and $liveAuth.PSObject.Properties['properties'] -and $liveAuth.properties) {
    $hasProperties = $true
}

if (-not $hasProperties) {
    Write-Warning "    GET authsettingsV2 returned no properties; skipping token-store re-assert"
} else {
    # Walk-and-create the path: properties.login.tokenStore.enabled
    if (-not $liveAuth.properties.PSObject.Properties['login'] -or $null -eq $liveAuth.properties.login) {
        $liveAuth.properties | Add-Member -NotePropertyName login -NotePropertyValue ([pscustomobject]@{}) -Force
    }
    if (-not $liveAuth.properties.login.PSObject.Properties['tokenStore'] -or $null -eq $liveAuth.properties.login.tokenStore) {
        $liveAuth.properties.login | Add-Member -NotePropertyName tokenStore -NotePropertyValue ([pscustomobject]@{}) -Force
    }
    if (-not $liveAuth.properties.login.tokenStore.PSObject.Properties['enabled']) {
        $liveAuth.properties.login.tokenStore | Add-Member -NotePropertyName enabled -NotePropertyValue $true -Force
    } else {
        $liveAuth.properties.login.tokenStore.enabled = $true
    }

    # PUT requires only the {properties: ...} wrapper, not the top-level
    # id/name/type metadata that GET returned.
    $putBody = [pscustomobject]@{ properties = $liveAuth.properties }
    $tokenStorePut = [System.IO.Path]::GetTempFileName()
    $putBody | ConvertTo-Json -Depth 50 | Set-Content -Path $tokenStorePut -Encoding UTF8
    try {
        Invoke-AzCli @(
            'rest','--method','PUT',
            '--url',$authUrl,
            '--headers','Content-Type=application/json',
            '--body',"@$tokenStorePut"
        ) | Out-Null
    } finally {
        Remove-Item $tokenStorePut -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "    Easy Auth Microsoft provider + platform configured (token store: enabled)" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 10 — End-to-end verification
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══ Stage 10  End-to-end verification ════════════════════════════" -ForegroundColor Cyan

$verifyErrors = New-Object 'System.Collections.Generic.List[string]'

# 10a. Easy Auth state — normalise the response shape once, then walk every
#      property safely so a missing intermediate produces a verifyError row
#      rather than crashing the whole stage.
$auth = Get-EasyAuthConfig -Name $FunctionAppName -ResourceGroup $ResourceGroupName
if ($null -eq $auth) {
    $verifyErrors.Add("az webapp auth show returned no configuration for $FunctionAppName")
} else {
    $liveEnabled    = Get-DeepValue $auth @('platform','enabled')
    $liveClientId   = Get-DeepValue $auth @('identityProviders','azureActiveDirectory','registration','clientId')
    $liveIssuer     = Get-DeepValue $auth @('identityProviders','azureActiveDirectory','registration','openIdIssuer')
    $liveAudiences  = Get-DeepValue $auth @('identityProviders','azureActiveDirectory','validation','allowedAudiences')
    $liveAudience   = if ($liveAudiences) { @($liveAudiences)[0] } else { $null }
    $liveAction     = Get-DeepValue $auth @('globalValidation','unauthenticatedClientAction')
    $liveTokenStore = Get-DeepValue $auth @('login','tokenStore','enabled')

    if ($liveEnabled -ne $true)                                              { $verifyErrors.Add("platform.enabled is '$liveEnabled', expected True") }
    if ($liveClientId -ne $ClientId)                                         { $verifyErrors.Add("clientId is '$liveClientId', expected '$ClientId'") }
    if ($liveAudience -ne "api://$ClientId")                                 { $verifyErrors.Add("audience is '$liveAudience', expected 'api://$ClientId'") }
    if ($liveIssuer  -ne "https://login.microsoftonline.com/$TenantId/v2.0") { $verifyErrors.Add("issuer is '$liveIssuer', expected 'https://login.microsoftonline.com/$TenantId/v2.0'") }
    if ($liveAction  -ne 'AllowAnonymous')                                   { $verifyErrors.Add("unauthenticatedClientAction is '$liveAction', expected 'AllowAnonymous'") }
    if ($liveTokenStore -ne $true)                                           { $verifyErrors.Add("login.tokenStore.enabled is '$liveTokenStore', expected True") }
}

# 10b. Entra app implicit grant - the AADSTS700054 trip wire
$liveApp = Invoke-AzCli @(
    'ad','app','show','--id',$ClientId,'--query','web.implicitGrantSettings','-o','json'
) | ConvertFrom-AzJson
if (-not $liveApp -or $liveApp.enableIdTokenIssuance -ne $true) {
    $verifyErrors.Add("enableIdTokenIssuance on the Entra app is not true (browser sign-in will fail with AADSTS700054)")
}

# 10b'. Entra app delegated scope - the AADSTS650057 trip wire
$rawLiveScopes = Invoke-AzCli @(
    'ad','app','show','--id',$ClientId,'--query','api.oauth2PermissionScopes','-o','json'
) | ConvertFrom-AzJson

$liveScopes = @()
if ($null -ne $rawLiveScopes) { $liveScopes = @($rawLiveScopes) }

$hasUserImpersonationLive = $false
foreach ($s in $liveScopes) {
    $val = Get-ScopeProperty -Scope $s -Name 'value'
    $en  = Get-ScopeProperty -Scope $s -Name 'isEnabled' -Default $false
    if ($val -eq 'user_impersonation' -and $en -eq $true) {
        $hasUserImpersonationLive = $true
        break
    }
}
if (-not $hasUserImpersonationLive) {
    $verifyErrors.Add("api.oauth2PermissionScopes on the Entra app does not contain an enabled 'user_impersonation' scope (token acquisition via az/MSAL will fail with AADSTS650057)")
}

# 10c. Required app settings present
$requiredSettings = @(
    'WAREWOLF_ENTRA_TENANT_ID',
    'WAREWOLF_ENTRA_AUDIENCE',
    'WAREWOLF_SECURE_CONFIG',
    $ClientSecretSettingName
)
foreach ($name in $requiredSettings) {
    $val = Get-FunctionAppSetting -Name $FunctionAppName -ResourceGroup $ResourceGroupName -SettingName $name
    if (-not $val) { $verifyErrors.Add("app setting '$name' is missing or empty") }
}

# 10d. Redirect URI registered
$liveRedirects = Invoke-AzCli @(
    'ad','app','show','--id',$ClientId,'--query','web.redirectUris','-o','json'
) | ConvertFrom-AzJson
$expectedRedirect = "https://$FunctionAppName.azurewebsites.net/.auth/login/aad/callback"
if (-not ($liveRedirects -contains $expectedRedirect)) {
    $verifyErrors.Add("redirect URI '$expectedRedirect' is not registered on the Entra app")
}

if ($verifyErrors.Count -gt 0) {
    Write-Host "    VERIFICATION FAILED:" -ForegroundColor Red
    foreach ($e in $verifyErrors) { Write-Host "      - $e" -ForegroundColor Red }
    throw "Stage 10 verification failed with $($verifyErrors.Count) issue(s) - see above."
}

Write-Host "    Easy Auth enabled=$liveEnabled clientId=$liveClientId" -ForegroundColor Green
Write-Host "    Audience=$liveAudience" -ForegroundColor Green
Write-Host "    Issuer=$liveIssuer" -ForegroundColor Green
Write-Host "    Token store enabled=$liveTokenStore" -ForegroundColor Green
Write-Host "    Implicit ID-token issuance: enabled" -ForegroundColor Green
Write-Host "    user_impersonation scope: exposed" -ForegroundColor Green
Write-Host "    All required app settings present" -ForegroundColor Green
Write-Host "    Redirect URI registered" -ForegroundColor Green

# ──────────────────────────────────────────────────────────────────────────────
# Stage 11 — Optional smoke test
# ──────────────────────────────────────────────────────────────────────────────

if (-not $SkipSmokeTest) {
    Write-Host ""
    Write-Host "═══ Stage 11  Smoke test (HTTP probe) ═════════════════════════════" -ForegroundColor Cyan

    $base = "https://$FunctionAppName.azurewebsites.net"

    function Probe {
        param([string] $Url, [int[]] $ExpectStatus, [string] $Label)
        try {
            $resp = Invoke-WebRequest -Uri $Url -Method GET -MaximumRedirection 0 -ErrorAction Stop
            $code = [int]$resp.StatusCode
        } catch [System.Net.WebException] {
            $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        } catch {
            # In PowerShell 7 redirects throw a different exception type
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $code = [int]$_.Exception.Response.StatusCode
            } else {
                $code = 0
            }
        }
        if ($ExpectStatus -contains $code) {
            Write-Host "    $Label : $Url -> $code (OK)" -ForegroundColor Green
            return $true
        } else {
            Write-Host "    $Label : $Url -> $code (expected $($ExpectStatus -join '/'))" -ForegroundColor DarkYellow
            return $false
        }
    }

    # Note: these assume the function is deployed and a workflow named 'Hello World'
    # exists in /Public.  Failures here are warnings, not throws - the script may
    # have run before the function code was deployed.
    $null = Probe -Url "$base/Public/Hello%20World.json?Name=ping" -ExpectStatus @(200) -Label 'public  '
    $null = Probe -Url "$base/Secure/Hello%20World.json?id=1"        -ExpectStatus @(401,302) -Label 'secure-no-auth'
} else {
    Write-Host ""
    Write-Host "═══ Stage 11  Smoke test (skipped via -SkipSmokeTest) ═════════════" -ForegroundColor DarkYellow
}

# ──────────────────────────────────────────────────────────────────────────────
# Stage 12 — Persist outputs
# ──────────────────────────────────────────────────────────────────────────────

$summary = [pscustomobject]@{
    Timestamp           = (Get-Date).ToString('o')
    SubscriptionId      = $SubscriptionId
    TenantId            = $TenantId
    ResourceGroup       = $ResourceGroupName
    FunctionAppName     = $FunctionAppName
    EntraAppDisplayName = $EntraAppDisplayName
    ClientId            = $ClientId
    AppObjectId         = $AppObjectId
    SpObjectId          = $SpObjectId
    Audience            = "api://$ClientId"
    Issuer              = "https://login.microsoftonline.com/$TenantId/v2.0"
    SecretRotated       = [bool]$ClientSecret
    SecretRotationReason = $rotationReason
    SecretExpiry        = $SecretExpiry
    AppRoles            = $desiredRoles | Select-Object value, displayName, id
    UserAssignments     = $UserAssignments
}
$summary | ConvertTo-Json -Depth 6 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host ""
Write-Host "═══ Done ═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    Summary written to $OutputPath" -ForegroundColor Green
Write-Host "    ClientId:  $ClientId"
Write-Host "    Audience:  api://$ClientId"
Write-Host "    Issuer:    https://login.microsoftonline.com/$TenantId/v2.0"
if ($ClientSecret) {
    Write-Host "    Secret:    [stored in Function App settings - not echoed]"
}
Write-Host ""
