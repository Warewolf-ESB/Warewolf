#Requires -Version 7.0
<#
.SYNOPSIS
    Roll back / tear down everything Deploy-WwExecutionEngine.ps1 created for a
    single live-test run — safely, without touching pre-existing resources.

.DESCRIPTION
    Companion to Deploy-WwExecutionEngine.ps1.  It deletes ONLY the Azure and
    Entra artifacts that a specific deploy run created, identified by either:

      * the deploy's summary.json  (-SummaryPath) — AUTHORITATIVE.  Its `created`
        map records, per resource, whether the run created it ($true) or found it
        already present ($false).  Pre-existing ($false) resources are NEVER
        deleted.
      * failing that, the run tag `wwx-test-run=<runId>` (-RunId) stamped on every
        resource the deploy created.  A resource whose tag does not match is
        treated as pre-existing and preserved.

    Teardown order (reverse of creation, dependency-aware):
        1. Auth        - delegate to Cleanup-WwExecutionAuth.ps1 (disable Easy Auth,
                         remove the 4 auth app-settings, delete the Entra app which
                         cascades its SP, role assignments and secrets)
        2. Key Vault   - delete THEN purge (soft-delete) so the name is freed
        3. App Insights- delete the component (a shared/default Log Analytics
                         workspace is left untouched). Uses 'az monitor app-insights'
                         and FALLS BACK to extension-independent ARM 'az resource'
                         commands if that extension is broken/locked.
        4. Function App- delete (also removes its system-assigned identity)
        5. Storage     - delete the account
        6. Resource grp- ONLY when -DeleteResourceGroup AND the run created the RG
        7. Local       - (optional, -CleanLocal) remove staged secure.config /
                         license / Elasticsearch source from the publish dir
    A leak check then re-queries every artifact and reports anything still present.

.PARAMETER SummaryPath
    Path to the deploy's *.summary.json.  Hydrates every target + the created map.

.PARAMETER SubscriptionId / ResourceGroup / AppName / StorageAccount / AppInsightsName / KeyVaultName / EntraAppDisplayName
    Explicit targets, used when no -SummaryPath is given (or to override it).

.PARAMETER RunId
    Tag value (`wwx-test-run=<RunId>`) used to confirm ownership when the summary's
    created map is unavailable.

.PARAMETER IncludeEntraApp
    No-summary fallback only: opt in to delete the Entra app by display name.
    Entra apps carry no Azure tag, so ownership cannot be auto-verified; without
    a summary recording created.entraApp=true, the app is preserved unless this
    switch is set.

.PARAMETER DeleteResourceGroup
    Also delete the resource group — honoured ONLY when the run created it
    (created.resourceGroup = true).  Ignored for pre-existing groups.

.PARAMETER CleanLocal
    Remove the staged secure.config, 'Warewolf License.secureconfig' and
    Settings\ElasticsearchLoggingSource.bite from -PublishDir.

.PARAMETER PublishDir
    Publish directory to clean when -CleanLocal is set.

.PARAMETER Force
    Skip the confirmation prompt (still resolves targets).

.PARAMETER NonInteractive
    Never prompt; missing required targets throw.

.PARAMETER DryRun
    Print every delete without executing it.  Read-only probes still run.

.NOTES
    Prerequisites: PowerShell 7+, Azure CLI (az, logged in).  Caller needs
    Contributor on the resource group, permission to delete + purge the Key Vault,
    and Application.ReadWrite.All in the tenant (Entra app deletion).
#>

[CmdletBinding()]
param(
    [string] $SummaryPath,
    [string] $SubscriptionId,
    [string] $ResourceGroup,
    [string] $AppName,
    [string] $StorageAccount,
    [string] $AppInsightsName,
    [string] $KeyVaultName,
    [string] $EntraAppDisplayName,
    [string] $RunId,
    [switch] $IncludeEntraApp,
    [switch] $DeleteResourceGroup,
    [switch] $CleanLocal,
    [string] $PublishDir,
    [switch] $Force,
    [switch] $NonInteractive,
    [switch] $DryRun,

    # Test hook: define helpers and return before any cloud/filesystem action.
    [switch] $LoadFunctionsOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers (mirror Deploy-WwExecutionEngine.ps1 for consistency)
# ════════════════════════════════════════════════════════════════════════════

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('═' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('═' * 76) -ForegroundColor Cyan
}

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function Test-CommandExists {
    param([string] $Name)
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Confirm-Yes {
    param([string] $Message, [bool] $DefaultYes = $false)
    if ($NonInteractive -or $Force) { return $true }
    $suffix = if ($DefaultYes) { '[Y/n]' } else { '[y/N]' }
    $answer = Read-Host "  $Message $suffix"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $DefaultYes }
    return $answer -imatch '^y'
}

function Confirm-Action {
    <#
        Per-action confirmation gate for an individual destructive step.  The
        operator is prompted before EACH real deletion ("confirm every action
        before it really cleans up anything").  Auto-proceeds when nothing is
        actually deleted (-DryRun) or when explicitly running unattended
        (-Force / -NonInteractive).  Default answer is NO.
    #>
    param([Parameter(Mandatory)][string] $Message)
    if ($DryRun)         { return $true }   # nothing is really deleted under DryRun
    if ($Force)          { return $true }   # explicit unattended override
    if ($NonInteractive) { return $true }   # unattended; top-level gate already passed
    $answer = Read-Host "    CONFIRM DELETE: $Message ? [y/N]"
    return $answer -imatch '^y'
}

function Format-AzArgsForLog {
    param([string[]] $Arguments)
    $flagRegex = '(?i)^(--password|--client-secret|--secret)$'
    $secretNameRegex = '(?i)((^|_)(PASSWORD|SECRET|TOKEN|KEY)$|CONNECTION_?STRING)'
    $rendered  = New-Object System.Collections.Generic.List[string]
    $maskNext  = $false
    foreach ($a in $Arguments) {
        if ($maskNext)            { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($a -match $flagRegex) { $rendered.Add($a); $maskNext = $true; continue }
        if ($a -match '^([A-Za-z0-9_.\-]+)=(.+)$') {
            $name = $Matches[1]
            if ($name -match $secretNameRegex) { $rendered.Add("$name=***REDACTED***") } else { $rendered.Add($a) }
            continue
        }
        $rendered.Add($a)
    }
    return ($rendered -join ' ')
}

function Invoke-Az {
    <#
        -Mutating  : changes cloud state; skipped (echoed) under -DryRun.
        -AllowFail : non-zero exit returns $null instead of throwing (probes).
    #>
    param(
        [Parameter(Mandatory)][string[]] $Args,
        [switch] $Mutating,
        [switch] $AllowFail
    )
    if ($Mutating -and $DryRun) {
        Write-Host "      [DRYRUN] az $(Format-AzArgsForLog $Args)" -ForegroundColor DarkGray
        return $null
    }
    $out = & az @Args 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFail) { return $null }
        throw "az CLI failed ($LASTEXITCODE): az $(Format-AzArgsForLog $Args)`n$($out | Out-String)"
    }
    return $out
}

function Invoke-ChildScript {
    param(
        [Parameter(Mandatory)][string] $Path,
        [hashtable] $Parameters = @{},
        [string] $Label
    )
    $name = if ($Label) { $Label } else { Split-Path $Path -Leaf }
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required sibling script not found: $Path" }
    if ($DryRun) {
        $rendered = ($Parameters.GetEnumerator() | ForEach-Object { "-$($_.Key)" }) -join ' '
        Write-Host "      [DRYRUN] & '$name' $rendered" -ForegroundColor DarkGray
        return
    }
    Write-Step "Invoking $name"
    & $Path @Parameters
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "$name exited with code $LASTEXITCODE." }
}

function Get-OwnershipDecision {
    <#
        Decide whether a resource belongs to this run.  PURE (no az) so it is
        unit-testable.  Precedence:
          * created map present ($true/$false)  -> Owned / Preserved
          * else tag match (TagRunId==ExpectedRunId) -> Owned, else Preserved
        Default-safe: anything not provably ours is Preserved.
    #>
    param(
        [nullable[bool]] $Created,
        [string] $TagRunId,
        [string] $ExpectedRunId
    )
    if ($null -ne $Created) { return ($Created ? 'Owned' : 'Preserved') }
    if ($TagRunId -and $ExpectedRunId -and ($TagRunId -eq $ExpectedRunId)) { return 'Owned' }
    return 'Preserved'
}

# ════════════════════════════════════════════════════════════════════════════
# Path resolution
# ════════════════════════════════════════════════════════════════════════════

$ScriptDir     = $PSScriptRoot
$CleanupScript = Join-Path $ScriptDir 'Cleanup-WwExecutionAuth.ps1'
$ElasticsearchBiteName = 'ElasticsearchLoggingSource.bite'

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight + load plan
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight + load plan'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) is not installed or not on PATH. Install: https://aka.ms/installazurecliwindows'
}

# created map: resource -> [bool] (true = this run created it).  Empty => fall back to tags.
$created = @{}

if ($SummaryPath) {
    if (-not (Test-Path -LiteralPath $SummaryPath)) { throw "SummaryPath not found: $SummaryPath" }
    Write-Step "Loading deploy summary '$SummaryPath'"
    $s = Get-Content -LiteralPath $SummaryPath -Raw | ConvertFrom-Json
    if (-not $SubscriptionId      -and $s.PSObject.Properties['subscriptionId'])      { $SubscriptionId      = $s.subscriptionId }
    if (-not $ResourceGroup       -and $s.PSObject.Properties['resourceGroup'])       { $ResourceGroup       = $s.resourceGroup }
    if (-not $AppName             -and $s.PSObject.Properties['appName'])             { $AppName             = $s.appName }
    if (-not $StorageAccount      -and $s.PSObject.Properties['storageAccount'])      { $StorageAccount      = $s.storageAccount }
    if (-not $AppInsightsName     -and $s.PSObject.Properties['appInsightsName'])     { $AppInsightsName     = $s.appInsightsName }
    if (-not $EntraAppDisplayName -and $s.PSObject.Properties['entraAppDisplayName']) { $EntraAppDisplayName = $s.entraAppDisplayName }
    if (-not $RunId               -and $s.PSObject.Properties['runId'])               { $RunId               = $s.runId }
    if (-not $KeyVaultName -and $s.PSObject.Properties['keyVault'] -and $s.keyVault) { $KeyVaultName = $s.keyVault.name }
    if ($s.PSObject.Properties['created'] -and $s.created) {
        foreach ($p in $s.created.PSObject.Properties) {
            if ($p.Value -is [bool]) { $created[$p.Name] = [bool]$p.Value }
        }
    }
    # A deploy that failed/aborted mid-run still writes a summary (crash-safe,
    # incremental). Its created-map records intent: resources THIS run set out to
    # create. Flag the partial run — each created=$true entry is still re-verified
    # for existence AND the run tag below, so a never-created resource is skipped
    # and a pre-existing one (created=$false) is preserved.
    if ($s.PSObject.Properties['status'] -and $s.status -in @('failed', 'in-progress')) {
        $lastPhase = if ($s.PSObject.Properties['lastPhase']) { $s.lastPhase } else { '(unknown phase)' }
        Write-Note ("Summary is from a PARTIAL run (status='{0}', lastPhase='{1}'). Cleaning up what it created; absent/never-created resources are skipped." -f $s.status, $lastPhase)
    }
}

if ([string]::IsNullOrWhiteSpace($ResourceGroup)) {
    throw 'ResourceGroup could not be resolved. Pass -SummaryPath <deploy summary.json> or -ResourceGroup <name>.'
}
if ($created.Count -eq 0 -and [string]::IsNullOrWhiteSpace($RunId)) {
    Write-Note 'No created-map (summary) and no -RunId: ownership falls back to tags only; without a matching tag, resources are PRESERVED (nothing deleted).'
}

# NOTE: capture first, then parse — see Deploy-WwExecutionEngine.ps1.
$accountJson = Invoke-Az @('account', 'show', '-o', 'json') -AllowFail
$account = if ($accountJson) { $accountJson | ConvertFrom-Json } else { $null }
if (-not $account) { throw "Not logged in to Azure CLI. Run 'az login' first." }
if (-not $SubscriptionId) { $SubscriptionId = $account.id }
Write-Ok "Azure account: $($account.user.name) | Subscription: $($account.name)"

Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

# ════════════════════════════════════════════════════════════════════════════
# Phase 1 — Discovery (existence + ownership per resource)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 1  Discovery (existence + ownership)'

function Get-ResourceState {
    <#
        Probe one resource: does it exist, what is its run tag, and is it Owned /
        Preserved / Absent for this run?
    #>
    param([string] $Key, [string[]] $ShowArgs, [string[]] $FallbackShowArgs)
    $json = Invoke-Az $ShowArgs -AllowFail
    if (-not $json -and $FallbackShowArgs) {
        # Primary path failed (e.g. a broken/locked 'az monitor app-insights'
        # extension). Fall back to the extension-independent ARM 'az resource show'
        # so a present resource is never mis-read as Absent (which would leak it).
        $json = Invoke-Az $FallbackShowArgs -AllowFail
    }
    $obj  = if ($json) { $json | ConvertFrom-Json } else { $null }
    $exists = [bool]$obj
    $tagRun = $null
    if ($exists -and $obj.PSObject.Properties['tags'] -and $obj.tags) {
        $tp = $obj.tags.PSObject.Properties['wwx-test-run']
        if ($tp) { $tagRun = [string]$tp.Value }
    }
    $createdVal = $null
    if ($created.ContainsKey($Key)) { $createdVal = [bool]$created[$Key] }
    $decision = if (-not $exists) { 'Absent' }
                else { Get-OwnershipDecision -Created $createdVal -TagRunId $tagRun -ExpectedRunId $RunId }
    return [pscustomobject]@{ Key = $Key; Exists = $exists; Decision = $decision; TagRun = $tagRun }
}

$states = [ordered]@{}
if ($KeyVaultName)   { $states['keyVault']      = Get-ResourceState 'keyVault'      @('keyvault','show','--name',$KeyVaultName,'-o','json') }
if ($AppInsightsName){ $states['appInsights']   = Get-ResourceState 'appInsights' `
        @('monitor','app-insights','component','show','--app',$AppInsightsName,'--resource-group',$ResourceGroup,'-o','json') `
        @('resource','show','--name',$AppInsightsName,'--resource-group',$ResourceGroup,'--resource-type','microsoft.insights/components','-o','json') }
if ($AppName)        { $states['functionApp']   = Get-ResourceState 'functionApp'   @('functionapp','show','--name',$AppName,'--resource-group',$ResourceGroup,'-o','json') }
if ($StorageAccount) { $states['storageAccount']= Get-ResourceState 'storageAccount'@('storage','account','show','--name',$StorageAccount,'--resource-group',$ResourceGroup,'-o','json') }
$states['resourceGroup'] = Get-ResourceState 'resourceGroup' @('group','show','--name',$ResourceGroup,'-o','json')

foreach ($k in $states.Keys) {
    $st = $states[$k]
    $colour = switch ($st.Decision) { 'Owned' { 'Green' } 'Preserved' { 'DarkYellow' } default { 'DarkGray' } }
    Write-Host ("    {0,-16}: exists={1,-5} -> {2}" -f $k, $st.Exists, $st.Decision) -ForegroundColor $colour
}

# Entra app is tenant-scoped (no Azure tag to verify ownership).  Treat it as
# OWNED only when the summary's created map says we created it; in the no-summary
# fallback we cannot prove ownership, so require an explicit -IncludeEntraApp
# (default-safe = preserve), consistent with the Azure-resource rule.
$entraAppCreated = if ($created.ContainsKey('entraApp')) { [bool]$created['entraApp'] } else { $null }
$entraOwned = [bool]$EntraAppDisplayName -and (($entraAppCreated -eq $true) -or ($null -eq $entraAppCreated -and $IncludeEntraApp))

# ════════════════════════════════════════════════════════════════════════════
# Phase 2 — Plan + single confirmation
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 2  Rollback plan'

$toDelete  = @($states.GetEnumerator() | Where-Object { $_.Value.Decision -eq 'Owned' -and $_.Value.Exists } | ForEach-Object { $_.Key })
$preserved = @($states.GetEnumerator() | Where-Object { $_.Value.Decision -eq 'Preserved' } | ForEach-Object { $_.Key })

$rgWillDelete = $DeleteResourceGroup -and ($created.ContainsKey('resourceGroup')) -and ($created['resourceGroup'] -eq $true)

Write-Host '  Will DELETE (owned by this run):' -ForegroundColor White
if ($entraOwned)                       { Write-Host "    - Entra app + Easy Auth  ($EntraAppDisplayName)  [via Cleanup-WwExecutionAuth.ps1]" }
if ($toDelete -contains 'keyVault')    { Write-Host "    - Key Vault              ($KeyVaultName)  [delete + purge]" }
if ($toDelete -contains 'appInsights') { Write-Host "    - App Insights component ($AppInsightsName)" }
if ($toDelete -contains 'functionApp') { Write-Host "    - Function App           ($AppName)" }
if ($toDelete -contains 'storageAccount') { Write-Host "    - Storage account        ($StorageAccount)" }
if ($rgWillDelete)                     { Write-Host "    - Resource group         ($ResourceGroup)" }
if (-not $entraOwned -and $toDelete.Count -eq 0 -and -not $rgWillDelete) { Write-Host '    (nothing — no owned resources found)' -ForegroundColor DarkGray }

Write-Host ''
Write-Host '  Will PRESERVE (pre-existing / not owned by this run):' -ForegroundColor White
if ($preserved.Count) { foreach ($p in $preserved) { Write-Host "    - $p" -ForegroundColor DarkYellow } }
if ($EntraAppDisplayName -and -not $entraOwned) {
    Write-Host "    - Entra app              ($EntraAppDisplayName)  [not summary-confirmed; pass -IncludeEntraApp to include — cannot tag-verify Entra apps]" -ForegroundColor DarkYellow
}
if (-not $rgWillDelete) { Write-Host "    - Resource group         ($ResourceGroup)  [never deleted unless run-created + -DeleteResourceGroup]" -ForegroundColor DarkYellow }
if ($CleanLocal -and $PublishDir) { Write-Host ''; Write-Host "  Will CLEAN local staged secrets under: $PublishDir" -ForegroundColor White }
Write-Host ''

if ($DryRun) { Write-Note 'DryRun: plan only, no changes made.'; }

if (-not (Confirm-Yes 'Proceed with rollback?' $false)) {
    Write-Note 'Aborted by user.'
    return
}

$runStamp    = Get-Date -Format 'yyyyMMdd-HHmmss'
$deleted     = New-Object System.Collections.Generic.List[string]
$skipped     = New-Object System.Collections.Generic.List[string]
foreach ($p in $preserved) { $skipped.Add($p) }

# ════════════════════════════════════════════════════════════════════════════
# Phase 3 — Teardown (dependency-ordered)
# ════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 3  Teardown'

# 3.1 Auth (must run while the Function App still exists so Easy Auth/settings clear).
if ($entraOwned) {
    if (Confirm-Action "delete Entra app + disable Easy Auth ($EntraAppDisplayName)") {
        Invoke-ChildScript -Path $CleanupScript -Label 'Cleanup-WwExecutionAuth.ps1' -Parameters @{
            SubscriptionId      = $SubscriptionId
            ResourceGroupName   = $ResourceGroup
            FunctionAppName     = $AppName
            EntraAppDisplayName = $EntraAppDisplayName
            NonInteractive      = $true
        }
        $deleted.Add('entraApp+easyAuth')
        Write-Ok 'Auth teardown delegated to Cleanup-WwExecutionAuth.ps1.'
    } else {
        $skipped.Add('entraApp+easyAuth (declined)')
        Write-Note 'Entra/Easy Auth teardown skipped by operator.'
    }
} else {
    Write-Note 'Entra/Easy Auth teardown skipped (not owned / no display name).'
}

# 3.2 Key Vault — delete THEN purge (soft-delete frees the name only after purge).
if ($toDelete -contains 'keyVault') {
    if (Confirm-Action "delete + purge Key Vault '$KeyVaultName'") {
        Write-Step "Deleting Key Vault '$KeyVaultName'"
        Invoke-Az @('keyvault','delete','--name',$KeyVaultName,'--resource-group',$ResourceGroup) -Mutating | Out-Null
        Write-Step "Purging Key Vault '$KeyVaultName' (soft-delete)"
        Invoke-Az @('keyvault','purge','--name',$KeyVaultName) -Mutating -AllowFail | Out-Null
        $deleted.Add('keyVault')
        Write-Ok 'Key Vault deleted + purged.'
    } else { $skipped.Add('keyVault (declined)'); Write-Note 'Key Vault deletion skipped by operator.' }
}

# 3.3 App Insights component (leave any shared/default Log Analytics workspace).
if ($toDelete -contains 'appInsights') {
    if (Confirm-Action "delete App Insights component '$AppInsightsName'") {
        Write-Step "Deleting App Insights component '$AppInsightsName'"
        try {
            Invoke-Az @('monitor','app-insights','component','delete','--app',$AppInsightsName,'--resource-group',$ResourceGroup) -Mutating | Out-Null
        } catch {
            # Extension path failed (e.g. broken 'az monitor app-insights'); fall back
            # to the extension-independent ARM resource delete.
            Write-Note "app-insights extension delete failed; falling back to ARM resource delete."
            Invoke-Az @('resource','delete','--name',$AppInsightsName,'--resource-group',$ResourceGroup,'--resource-type','microsoft.insights/components') -Mutating | Out-Null
        }
        $deleted.Add('appInsights')
        Write-Ok 'App Insights component deleted.'
    } else { $skipped.Add('appInsights (declined)'); Write-Note 'App Insights deletion skipped by operator.' }
}

# 3.4 Function App (also removes its system-assigned managed identity).
if ($toDelete -contains 'functionApp') {
    if (Confirm-Action "delete Function App '$AppName'") {
        Write-Step "Deleting Function App '$AppName'"
        Invoke-Az @('functionapp','delete','--name',$AppName,'--resource-group',$ResourceGroup) -Mutating | Out-Null
        $deleted.Add('functionApp')
        Write-Ok 'Function App deleted.'
    } else { $skipped.Add('functionApp (declined)'); Write-Note 'Function App deletion skipped by operator.' }
}

# 3.5 Storage account.
if ($toDelete -contains 'storageAccount') {
    if (Confirm-Action "delete storage account '$StorageAccount'") {
        Write-Step "Deleting storage account '$StorageAccount'"
        Invoke-Az @('storage','account','delete','--name',$StorageAccount,'--resource-group',$ResourceGroup,'--yes') -Mutating | Out-Null
        $deleted.Add('storageAccount')
        Write-Ok 'Storage account deleted.'
    } else { $skipped.Add('storageAccount (declined)'); Write-Note 'Storage account deletion skipped by operator.' }
}

# 3.6 Resource group — ONLY when the run created it and -DeleteResourceGroup is set.
if ($rgWillDelete) {
    if (Confirm-Action "DELETE RESOURCE GROUP '$ResourceGroup' (and everything in it)") {
        Write-Step "Deleting resource group '$ResourceGroup'"
        Invoke-Az @('group','delete','--name',$ResourceGroup,'--yes') -Mutating | Out-Null
        $deleted.Add('resourceGroup')
        Write-Ok 'Resource group deleted.'
    } else { $skipped.Add('resourceGroup (declined)'); Write-Note 'Resource group deletion skipped by operator.' }
} else {
    Write-Note "Resource group '$ResourceGroup' preserved (existing-RG strategy)."
}

# 3.7 Local staged secrets (optional).
if ($CleanLocal -and $PublishDir) {
    if (Confirm-Action "remove staged secrets under '$PublishDir'") {
        foreach ($rel in @('secure.config', 'Warewolf License.secureconfig', (Join-Path 'Settings' $ElasticsearchBiteName))) {
            $path = Join-Path $PublishDir $rel
            if (Test-Path -LiteralPath $path) {
                Write-Step "Removing staged '$rel'"
                if (-not $DryRun) { Remove-Item -LiteralPath $path -Force }
            }
        }
        Write-Ok 'Local staged secrets cleaned (Resources left in place).'
    } else { Write-Note 'Local cleanup skipped by operator.' }
}

# ════════════════════════════════════════════════════════════════════════════
# Phase 4 — Leak check (re-query everything that should be gone)
# ════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 4  Leak check'

$leaks = New-Object System.Collections.Generic.List[string]

if (-not $DryRun) {
    foreach ($k in $toDelete) {
        $st = switch ($k) {
            'keyVault'       { Get-ResourceState 'keyVault'       @('keyvault','show','--name',$KeyVaultName,'-o','json') }
            'appInsights'    { Get-ResourceState 'appInsights' `
                                   @('monitor','app-insights','component','show','--app',$AppInsightsName,'--resource-group',$ResourceGroup,'-o','json') `
                                   @('resource','show','--name',$AppInsightsName,'--resource-group',$ResourceGroup,'--resource-type','microsoft.insights/components','-o','json') }
            'functionApp'    { Get-ResourceState 'functionApp'    @('functionapp','show','--name',$AppName,'--resource-group',$ResourceGroup,'-o','json') }
            'storageAccount' { Get-ResourceState 'storageAccount' @('storage','account','show','--name',$StorageAccount,'--resource-group',$ResourceGroup,'-o','json') }
            default          { $null }
        }
        if ($st -and $st.Exists) { $leaks.Add($k) }
    }
    # Key Vault must also be gone from the soft-deleted list.
    if ($toDelete -contains 'keyVault') {
        $sd = Invoke-Az @('keyvault','list-deleted','--query',"[?name=='$KeyVaultName'].name",'-o','tsv') -AllowFail
        if ($sd) { $leaks.Add('keyVault (soft-deleted; purge incomplete)') }
    }
    # Entra app must no longer resolve by display name.
    if ($entraOwned) {
        $still = Invoke-Az @('ad','app','list','--display-name',$EntraAppDisplayName,'--query','[0].appId','-o','tsv') -AllowFail
        if ($still) { $leaks.Add("entraApp ($EntraAppDisplayName still resolves; deletion may be queued)") }
    }
}

if ($leaks.Count -eq 0) {
    Write-Ok ($DryRun ? 'DryRun: leak check skipped.' : 'No leaks — every owned artifact is gone.')
} else {
    Write-Note 'Potential leaks detected (re-run shortly; some deletes are async):'
    foreach ($l in $leaks) { Write-Host "      ! $l" -ForegroundColor Red }
}

# ── Rollback summary file (real runs only) ──────────────────────────────────
if (-not $DryRun) {
    $logDir = if ($SummaryPath) { Split-Path -Parent $SummaryPath } else { $ScriptDir }
    $rollbackSummary = Join-Path $logDir "rollback-WwExecutionEngine-$runStamp.summary.json"
    [ordered]@{
        timestampUtc  = (Get-Date).ToUniversalTime().ToString('o')
        runId         = $RunId
        resourceGroup = $ResourceGroup
        deleted       = @($deleted)
        preserved     = @($skipped)
        leaks         = @($leaks)
    } | ConvertTo-Json -Depth 6 | Set-Content -Path $rollbackSummary -Encoding UTF8
    Write-Ok "Rollback summary written to $rollbackSummary"
}

Write-Phase 'Rollback complete'
Write-Host "  Deleted  : $([string]::Join(', ', $deleted))" -ForegroundColor White
Write-Host "  Preserved: $([string]::Join(', ', $skipped))" -ForegroundColor White
Write-Host ''
