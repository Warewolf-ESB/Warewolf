#Requires -Version 7.0
<#
.SYNOPSIS
    Manual, one-pass deploy of the Warewolf Execution Engine followed by the RabbitMQ
    QueueProcessor, wiring the second from what the first produced.

.DESCRIPTION
    A THIN WRAPPER over two orchestrators. It provisions nothing itself and re-implements
    nothing they already do (infra, auth, staging, encryption, image build, KEDA rules):

        Deploy-WwExecutionEngine.ps1     ->  the engine Function App
        Deploy-WwQueueProcessor.ps1      ->  one Container App per RabbitMQ trigger

    Edit the VARIABLE BLOCK below, then run the script. Every value the operator supplies
    is a placeholder of the form '<...>'; the run aborts up front listing any that are
    still unedited. Values the orchestrators DERIVE (the engine endpoint, the App Insights
    name, the Entra app name/ids) are never asked for - they are read back.

    Flow:
        Step 1  Preflight      - az present + logged in, subscription selected
        Step 2  Publish paths  - extract .zip inputs, validate the package contents
        Step 3  Broker secret  - resolve the versionless Key Vault uri KEDA reads
        Step 4  Engine         - confirm, then Deploy-WwExecutionEngine.ps1
        Step 5  Capture        - read the engine's two output files, assert, write handover
        Step 6  QueueProcessor - confirm, then Deploy-WwQueueProcessor.ps1

    THE HANDOVER CONTRACT - why step 5 exists.
    The engine deploy writes TWO machine-readable outputs, and this script is wired from
    them rather than by re-querying Azure:

      1. $LogDir\deploy-WwExecutionEngine-<stamp>.summary.json   (Save-DeploySummary)
           status, dryRun, runId, resourceTags, created, endpoint, appName,
           appInsightsName, entraAppDisplayName, keyVault, appSettings (MASKED)
      2. <ScriptDir>\Configure-WwExecutionAuth.output.json       (Configure-WwExecutionAuth.ps1)
           ClientId, AppObjectId, SpObjectId, Audience, Issuer, AppRoles[].id,
           EntraAppDisplayName, FunctionAppName

    Four properties of those files drive the assertions in step 5:
      * The summary is rewritten after EVERY phase with status in-progress|completed|failed,
        so "the newest summary" can easily be an aborted run. status/dryRun/appName are asserted.
      * The auth output is at a FIXED path in this folder and is OVERWRITTEN BY EVERY RUN.
        It is not per-run and not in $LogDir. FunctionAppName is asserted before any value
        from it is trusted. It is absent under -SkipAuthProvisioning and under -DryRun.
      * The summary's appInsightsConnectionString is MASKED, so the QueueProcessor's
        -AppInsightsConnectionString must still come from `az monitor app-insights`.
      * endpoint / appInsightsName / entraAppDisplayName are authoritative - never hand-build
        https://<app>.azurewebsites.net, never ask the operator to name the Entra app.

    This script then merges both into ONE consolidated file:

        $LogDir\deploy-both-<stamp>.handover.json

    so downstream steps - the Warewolf_QueueProcessor app-role grant, Get-WwExecutionToken*.ps1,
    Rollback-WwExecutionEngine.ps1 -SummaryPath - read one file instead of re-deriving values.
    Secrets stay masked and the App Insights connection string is NOT written to it.

.PARAMETER EncryptResources
    FIRST RUN ONLY for a given set of sources. Forwards -EncryptResources to the engine, which
    converts workflow/Elasticsearch/persistence sources from plain/DPAPI to WFAES. On later
    deploys leave this OFF - the sources are already encrypted and are staged as-is, though
    -KeyVaultName/-KeyVaultSecretName are still passed so the runtime can decrypt them.

.PARAMETER SkipEngine
    Deploy only the QueueProcessor, against an already-deployed engine. Step 5 still runs and
    still asserts, resolving the handover values from the existing summary + auth output.

.PARAMETER SkipQueueProcessor
    Deploy only the engine. The handover file is still written.

.PARAMETER NonInteractive
    Skip both y/N confirmation gates and forward -NonInteractive to the children.

.PARAMETER DryRun
    Forward -DryRun to both children so each prints its own plan and changes nothing. The
    engine produces no auth output and only a *.dryrun.summary.json in this mode, so step 5
    cannot assert against a real run; it emits clearly-marked SYNTHETIC handover values purely
    so the QueueProcessor's plan phase can be reached. Never treat a dry-run handover file as real.

.EXAMPLE
    # First deploy of a new source set - encrypt once, confirm each phase.
    .\Deploy-WwEngineAndQueueProcessor.ps1 -EncryptResources

.EXAMPLE
    # See both plans without touching Azure.
    .\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive

.EXAMPLE
    # Redeploy only the workers against the engine that is already up.
    .\Deploy-WwEngineAndQueueProcessor.ps1 -SkipEngine

.NOTES
    Usage runbook, including the app-role grant that consumes the handover file:
        docs/Deploy-Both-RunGuide.md
    Reference runbook this script was derived from:
        docs/prompt-assets/deploy-both-reference-runbook.ps1
#>
[CmdletBinding()]
param(
    [switch] $EncryptResources,
    [switch] $SkipEngine,
    [switch] $SkipQueueProcessor,
    [switch] $NonInteractive,
    [switch] $DryRun,

    # Test hook, matching the two orchestrators: when dot-sourced with -LoadFunctionsOnly the
    # script defines its helper functions and returns BEFORE any cloud/filesystem action, so
    # Pester can unit-test them.
    [switch] $LoadFunctionsOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ══════════════════════════════════════════════════════════════════════════════
# VARIABLE BLOCK — edit everything in <angle brackets>, then run.
# ══════════════════════════════════════════════════════════════════════════════

# ── Identity / subscription ───────────────────────────────────────────────────
# Leave both empty to take them from `az account show`.
$Sub           = ''
$TenantId      = ''

# ── Reused foundation (this script NEVER creates or deletes these) ────────────
$Rg            = '<resource group>'
$Loc           = '<resource location>'
$Kv            = '<key vault name>'                       # vault holding the WFAES key + broker uri
$KvSecret      = '<key vault secret name>'                # secret holding the AES key material
$Acr           = '<azure container registry>'
$AcaEnv        = '<azure container apps environment>'
$RabbitSecretName = 'rabbitmq-uri'                        # secret KEDA reads for the AMQP uri

# ── Staging ───────────────────────────────────────────────────────────────────
$Stage         = '<local staging directory>'
$LogDir        = "$Stage\logs\deploy-both"                # dedicated: keeps this run's summaries unambiguous
$SecureConfig  = "$Stage\settings\secure.config"
$AuthConfig    = "$Stage\settings\Deploy-WwExecutionEngine.authconfig.json"
$LicenseConfig = "$Stage\settings\Warewolf License.secureconfig"
$WorkflowsSrc  = "$Stage\resources"                       # workflow .bite files
$SourceDir     = "$Stage\sources"                         # RabbitMQ source .bite files ({sourceId}.bite)
$TriggerDir    = "$Stage\triggers"                        # queue trigger .bite files, one Container App each

# ── Engine: created by this run ───────────────────────────────────────────────
$EngineApp     = '<engine app name>'                      # globally unique Function App name
$EngineStorage = '<engine app storage name>'              # 3-24 lowercase chars
$EnginePublish = '<path to engine publish folder or .zip>'
# DERIVED, not operator-chosen — declared here only so the plan banner can show what is expected.
# Both are re-read from the engine's own outputs in step 5 and the read-back value wins.
$EngineAi      = "$EngineApp-ai"                          # orchestrator default for -AppInsightsName
$EngineAuthApp = "$EngineApp-auth"                        # Configure-WwExecutionAuth auto-derives this

# ── QueueProcessor: created by this run ───────────────────────────────────────
$QpPublish     = '<path to queueprocessor publish folder or .zip>'
$QpPrefix      = '<queue processor app name prefix>'      # e.g. wwqp-  ; one app per trigger
$ImageRepo     = '<queue processor image repository name>'
$DockerFile    = "$Stage\build\Dockerfile"

# ── Resolved at runtime — do NOT fill these in ────────────────────────────────
$RabbitSecretUri = $null   # step 3, from the vault uri
$AiConn          = $null   # step 5, az monitor app-insights (the summary's copy is masked)
# Everything else the engine produces arrives in step 5 as one object, $HO:
#   RunId (also the rollback tag wwx-test-run=<runId>), Endpoint, AppInsightsName,
#   EntraAppDisplayName, ClientId, SpObjectId, Audience, QueueProcessorRoleId,
#   SummaryPath, AuthOutputPath, Notes

# ══════════════════════════════════════════════════════════════════════════════
# PARAMETERS THIS WRAPPER DOES NOT PASS
# Listed so nothing is invisible to whoever edits this script next. Uncomment a
# variable AND add it to the matching splat in step 4 / step 6 to use it.
# ══════════════════════════════════════════════════════════════════════════════

# ── Deploy-WwExecutionEngine.ps1 — 38 of 58 parameters not passed ─────────────
# $PublishMethod              = 'Auto'        # Auto|Zip|Func. Auto/Zip both use az zip-deploy; Func is advanced/opt-in.
# $SkipAuthProvisioning       = $false        # Skips Entra+EasyAuth. NOT usable here: step 5 needs the auth output file.
# $GenerateNewKey             = $false        # Generate a fresh AES key. Implied for a new vault; destructive on an existing one.
# $EnableElasticsearch        = $false        # Elasticsearch logging; needs $ElasticsearchSourcePath + Key Vault.
# $ElasticsearchSourcePath    = ''            # Must be named exactly 'ElasticsearchLoggingSource.bite'.
# $EnablePersistence          = $false        # Hangfire suspend/resume; needs the two persistence files below.
# $PersistenceSettingsPath    = ''            # persistencesettings.json
# $PersistenceDbSourcePath    = ''            # persistencesettingsdbsource.bite
# $DeployJobProcessor         = $false        # Companion poller/reaper Function App (Deploy-WwJobProcessor.ps1).
# $JobProcessorAppName        = ''
# $JobProcessorPublishPath    = ''            # MUST differ from the engine publish output.
# $JobProcessorStorageAccount = ''
# $EngineResumeScope          = ''            # api://<engine-app-id>/.default for the job processor MI.
# $DeployServiceBusWorker     = $false        # Companion Service Bus-triggered Function App.
# $ServiceBusWorkerAppName    = ''
# $ServiceBusWorkerPublishPath= ''            # MUST differ from the engine publish output.
# $ServiceBusWorkerStorageAccount = ''
# $WwExecutionScope           = ''            # api://<engine-app-id>/.default for the SB worker MI.
# $DeployRabbitMqTriggers     = $false        # The engine's OWN queue-processor deploy. Deliberately NOT used:
#                                             # this wrapper makes the two calls separately so the handover
#                                             # values are asserted between them. Do not enable both paths.
# $QueueTriggerPath           = ''            # (-DeployRabbitMqTriggers only) — see $TriggerDir above.
# $QueueTriggerFilter         = '*.bite'      # (-DeployRabbitMqTriggers only)
# $QueueTriggerFilePath       = ''            # (-DeployRabbitMqTriggers only)
# $QueueTriggerManifestPath   = ''            # (-DeployRabbitMqTriggers only)
# $QueueSourcePath            = ''            # (-DeployRabbitMqTriggers only) — see $SourceDir above.
# $AcaEnvironment             = ''            # (-DeployRabbitMqTriggers only) — passed to the QP call instead.
# $AcrName                    = ''            # (-DeployRabbitMqTriggers only) — passed to the QP call instead.
# $QueueProcessorPublishPath  = ''            # (-DeployRabbitMqTriggers only) — see $QpPublish above.
# $QueueProcessorImage        = ''            # (-DeployRabbitMqTriggers only) — reuse a pre-built digest.
# $QueueEngineResourceAppId   = ''            # (-DeployRabbitMqTriggers only) — step 5 resolves this instead.
# $RabbitMqSecretUri          = ''            # (-DeployRabbitMqTriggers only) — step 3 resolves this instead.
# $QueueScalingMode           = 'Elastic'     # (-DeployRabbitMqTriggers only)
# $ContinueOnQueueTriggerError= $false        # (-DeployRabbitMqTriggers only)
# $EnableConsoleLogging       = $null         # ENABLECONSOLELOGGING app setting.
# $ExecutionLogLevel          = 'INFO'        # TRACE..OFF. Engine default is INFO; left at the orchestrator default.
# $LicenseCheckEnabled        = $null         # WAREWOLF_LICENSE_CHECK_ENABLED (engine default true).
# $StructuredLogs             = $null         # Structured (JSON) log output.
# $AlignHostJsonLogLevel      = $false        # Also rewrite published host.json logLevel; host-process verbosity only.
# $LoadFunctionsOnly          = $false        # Pester test hook — dot-sources helpers and returns. Never for a deploy.

# ── Deploy-WwQueueProcessor.ps1 — 23 of 47 parameters not passed ──────────────
# $Image                      = ''            # Reuse a pre-built digest/tag instead of building from $QpPublish.
# $ImageTag                   = ''            # Defaults to a content/stamp-derived tag.
# $TriggerFilePath            = ''            # ONE trigger instead of the folder. Mutually exclusive with $TriggerDir.
# $TriggerFilter              = '*.bite'      # Server-written triggers are {triggerId}.bite; default matches all.
# $TriggerManifestPath        = ''            # Manifest with per-trigger overrides. Mutually exclusive with $TriggerDir.
# $TriggerId                  = ''            # Narrow a folder/manifest to one trigger (per-trigger cutover).
# $EngineScope                = ''            # Defaults to api://<EngineResourceAppId>/.default.
# $EngineTimeoutSeconds       = 180           # Sized from measured engine latency; ceiling is the engine's own 600s.
# $MaxReplicas                = 0             # DERIVED from the trigger's Concurrency. Override only with cause.
# $MinReplicas                = -1            # -1 = derive from $ScalingMode (Elastic->0, Fixed->max, Warm->1).
# $TargetQueueLength          = 0             # DERIVED as Prefetch x MaxConcurrency. Raising it DELAYS scale-out.
# $MaxConcurrency             = 1             # In-flight workflows per replica. Scale OUT, not UP.
# $Cpu                        = '0.5'
# $Memory                     = '1.0Gi'
# $ShutdownGraceSeconds       = 210           # Must stay engine(180) <= drain(210) < termination(240).
# $TerminationGracePeriodSeconds = 240
# $MaxDeliveryAttempts        = 2             # 1 or 2 only; counted with the AMQP redelivered boolean.
# $RetryEngineInternalErrors  = $false        # OFF deliberately: the engine overloads 500 (workflow error,
#                                             # WOLF-8418 denial, and OOM all surface as 500).
# $RbacPropagationSeconds     = 60            # Wait after the role grant so Key Vault's DATA plane converges.
# $UseSsl                     = $false        # AMQPS to the broker.
# $ExecutionLogLevel          = 'INFO'        # Engine parity; left at the orchestrator default.
# $ContinueOnTriggerError     = $false        # Keep going when one trigger's app fails. Off = fail the run.
# $LoadFunctionsOnly          = $false        # Pester test hook. Never for a deploy.

# ══════════════════════════════════════════════════════════════════════════════
# Helpers
# ══════════════════════════════════════════════════════════════════════════════

function Write-WwHead { param([string]$Text) Write-Host ''; Write-Host "═══ $Text " -ForegroundColor Cyan }
function Write-WwOk   { param([string]$Text) Write-Host "  [ok]   $Text" -ForegroundColor Green }
function Write-WwNote { param([string]$Text) Write-Host "  [note] $Text" -ForegroundColor DarkGray }
function Write-WwWarn { param([string]$Text) Write-Host "  [warn] $Text" -ForegroundColor Yellow }

function Test-WwAppName {
    <#
        A Container App name: lowercase alphanumerics and hyphens, 2-63 chars, no leading or
        trailing hyphen. Used as a sanity gate before anything is written to the handover file -
        a run once recorded az's echoed command line here, and every downstream step (the
        app-role grant, revision checks) was then driven by a string that is not an app name.
    #>
    # -cmatch, not -match: PowerShell's -match is case-INSENSITIVE, so an uppercase name would
    # pass a [a-z0-9] pattern. Container App names are lowercase-only.
    param([string] $Name)
    return ($Name -cmatch '^[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$')
}

function Invoke-AzJson {
    <#
        Runs az, returns the parsed JSON.

        DELIBERATELY REFUSES --query, matching WwE2E.Common.psm1's Invoke-E2EAzJson. On Windows
        `az` is a .cmd shim and cmd.exe mangles JMESPath. An earlier version of this script tried
        to QUOTE the expression rather than avoid it, which made az echo its own resolved command
        line to stdout:

            G:\...\Scripts>  "C:\Program Files\...\python.exe" -IBm azure.cli containerapp list ...

        That echo was captured as the Container App name list and written into the handover file,
        so the app-role grant and every revision check downstream were driven by a string that is
        not an app name. Filter in PowerShell instead - it is also easier to read.
    #>
    param(
        [Parameter(Mandatory)][string[]] $AzArgs,
        [switch] $AllowFail
    )
    if ($AzArgs -contains '--query') {
        throw 'Invoke-AzJson does not accept --query (cmd.exe mangles JMESPath). Filter in PowerShell.'
    }
    $call = @($AzArgs)
    if ($call -notcontains '-o' -and $call -notcontains '--output') { $call += @('-o', 'json') }

    $global:LASTEXITCODE = 0
    $raw = & az @call 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($raw | Out-String))) {
        if ($AllowFail) { return $null }
        throw ("az {0} failed (exit {1})." -f ($AzArgs -join ' '), $LASTEXITCODE)
    }
    try { return ($raw | ConvertFrom-Json) }
    catch {
        if ($AllowFail) { return $null }
        throw ("az {0} did not return JSON: {1}" -f ($AzArgs -join ' '), ($raw | Out-String).Trim())
    }
}

function Test-WwPlaceholder {
    param([object]$Value)
    if ($null -eq $Value) { return $true }
    $s = [string]$Value
    return ([string]::IsNullOrWhiteSpace($s) -or $s -match '^\s*<.+>\s*$' -or $s -match '<[^>]+>')
}

# Collects EVERY unedited placeholder and reports them together, so the operator fixes the
# variable block once instead of discovering them one failed run at a time.
function Assert-WwRequired {
    param([hashtable]$Values)
    $bad = @($Values.GetEnumerator() | Where-Object { Test-WwPlaceholder $_.Value } | ForEach-Object { "  {0,-18} = {1}" -f $_.Key, $_.Value })
    if ($bad.Count) {
        throw ("Edit the variable block - {0} value(s) are still placeholders:`n{1}" -f $bad.Count, ($bad -join "`n"))
    }
}

function Get-WwMasked {
    param([string]$Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 8) { return '********' }
    return ('{0}...{1}' -f $Value.Substring(0, 4), $Value.Substring($Value.Length - 4))
}

function Confirm-WwStep {
    param([string]$Title, [hashtable]$Params, [string[]]$Mask = @())
    Write-Host ''
    Write-Host "  About to run: $Title" -ForegroundColor White
    foreach ($k in ($Params.Keys | Sort-Object)) {
        $v = if ($Mask -contains $k) { Get-WwMasked ([string]$Params[$k]) } else { $Params[$k] }
        Write-Host ("    -{0,-28} {1}" -f $k, $v)
    }
    if ($NonInteractive) { Write-WwNote 'NonInteractive: proceeding without confirmation.'; return }
    $answer = Read-Host '  Proceed? (y/N)'
    if ($answer -notmatch '^(y|yes)$') { throw "Aborted by operator at: $Title" }
}

# & on a .ps1 does not set $LASTEXITCODE by itself, but the child's own native az calls do -
# and a stale non-zero value from an earlier call would read as a failure. Reset, then check both.
function Invoke-WwChild {
    param([string]$Path, [hashtable]$Params, [string]$Label)
    if (-not (Test-Path -LiteralPath $Path)) { throw "$Label not found at $Path" }
    Write-WwHead "$Label"

    # Set-StrictMode is DYNAMICALLY SCOPED: without this, the -Version Latest set at the top of
    # this script is inherited by the child, which is then run under rules it was never written
    # for. Deploy-WwQueueProcessor.ps1 sets no StrictMode of its own, and under Latest a bare
    # .Count throws "The property 'Count' cannot be found on this object" for $null, a string,
    # an int, a FileInfo and an EMPTY ARRAY (only hashtables and PSCustomObjects are exempt) -
    # so its Phase 0 died on `$triggerSet.Count`. Deploy-WwExecutionEngine.ps1 was unaffected
    # only because it sets its own StrictMode and is written to satisfy it.
    # Turning it off HERE covers the child (scopes below inherit it) and leaves this script's
    # own scope strict.
    Set-StrictMode -Off

    $global:LASTEXITCODE = 0
    & $Path @Params
    if ($LASTEXITCODE -ne 0) { throw "$Label exited with code $LASTEXITCODE." }
    Write-WwOk "$Label completed."
}

function Expand-WwPublishPath {
    <#
        A .zip is extracted to a sibling folder and the FOLDER is returned.
        Deploy-WwQueueProcessor.ps1 accepts a directory only; the engine accepts either,
        but extracting here means the .bite guard below can inspect both the same way.
    #>
    param([string]$Path, [string]$Label)
    if (-not (Test-Path -LiteralPath $Path)) { throw "$Label not found: $Path" }
    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer) { return $item.FullName }
    if ($item.Extension -ne '.zip') { throw "$Label must be a folder or a .zip, got '$($item.Extension)': $Path" }

    $dest = Join-Path $item.DirectoryName $item.BaseName
    if (Test-Path -LiteralPath $dest) {
        Write-WwNote "$Label already extracted at $dest - reusing it."
    } else {
        Write-WwNote "$Label is a zip; extracting to $dest"
        Expand-Archive -LiteralPath $item.FullName -DestinationPath $dest -Force
    }
    return (Get-Item -LiteralPath $dest).FullName
}

function Assert-WwPackage {
    param([string]$Dir, [string]$Assembly, [string]$Label)
    if (-not (Test-Path -LiteralPath (Join-Path $Dir $Assembly))) {
        throw "$Label at '$Dir' does not contain $Assembly - wrong publish output?"
    }
    Write-WwOk "$Label : $Assembly present."
}

# Set-StrictMode -Version Latest throws on a missing property, and these two JSON files are
# written by other scripts that may add or drop fields. Read every property through this.
function Get-WwProp {
    param([object]$Object, [string]$Name, [object]$Default = $null)
    if ($null -eq $Object) { return $Default }
    $p = $Object.PSObject.Properties[$Name]
    if ($null -eq $p -or $null -eq $p.Value) { return $Default }
    return $p.Value
}

function New-WwEngineParams {
    <#
        Builds the Deploy-WwExecutionEngine.ps1 splat. Opt-in parameters are ADDED, never
        passed as $false/empty: a [nullable[bool]] the operator never set must stay absent
        so the orchestrator applies its own default or prompt.
    #>
    param(
        [Parameter(Mandatory)][hashtable] $Context,
        [bool] $EncryptResources,
        [bool] $NonInteractive,
        [bool] $DryRun
    )
    $p = @{
        SubscriptionId      = $Context.Sub
        TenantId            = $Context.TenantId
        ResourceGroup       = $Context.Rg
        Location            = $Context.Loc
        AppName             = $Context.EngineApp
        StorageAccount      = $Context.EngineStorage
        PublishPath         = $Context.EnginePublish
        AuthConfigPath      = $Context.AuthConfig
        SecureConfigPath    = $Context.SecureConfig
        LicenseConfigPath   = $Context.LicenseConfig
        WorkflowsSourcePath = $Context.WorkflowsSrc
        KeyVaultName        = $Context.Kv
        KeyVaultSecretName  = $Context.KvSecret
        VerifyDecryption    = $true
        # The App Insights component the capture step reads must be ASKED FOR here, and named,
        # or the capture finds nothing and the worker would get an empty connection string.
        EnableAppInsights   = $true
        AppInsightsName     = $Context.EngineAi
        LogDir              = $Context.LogDir
    }
    if ($EncryptResources) { $p.EncryptResources = $true }   # encrypt ONCE per source set
    if ($NonInteractive)   { $p.NonInteractive   = $true }
    if ($DryRun)           { $p.DryRun           = $true }
    return $p
}

function New-WwQpParams {
    <#
        Builds the Deploy-WwQueueProcessor.ps1 splat from the handover values.
        -EnableAppInsights is omitted entirely when no connection string was resolved -
        passing it with an empty string configures a worker that cannot emit telemetry
        and hides the misconfiguration.
    #>
    param(
        [Parameter(Mandatory)][hashtable] $Context,
        [string] $AppInsightsConnectionString,
        [bool] $NonInteractive,
        [bool] $DryRun
    )
    $p = @{
        ResourceGroup         = $Context.Rg
        Location              = $Context.Loc
        AcaEnvironment        = $Context.AcaEnv
        AcrName               = $Context.Acr
        PublishPath           = $Context.QpPublish
        ImageRepository       = $Context.ImageRepo
        AppNamePrefix         = $Context.QpPrefix
        TriggerPath           = $Context.TriggerDir
        QueueSourcePath       = $Context.SourceDir
        EngineBaseUrl         = $Context.EngineUrl
        EngineResourceAppId   = $Context.EngineAppId
        EngineTenantId        = $Context.TenantId
        KeyVaultName          = $Context.Kv
        KeyVaultSecretName    = $Context.KvSecret
        EncryptStagedSettings = $true
        RabbitMqSecretUri     = $Context.RabbitSecretUri
        InlineRabbitMqSecret  = $true
        ScalingMode           = 'Elastic'
        DockerfilePath        = $Context.DockerFile   # NOT -Dockerfile; that binds only by prefix abbreviation
        LogDir                = $Context.LogDir
    }
    if ($AppInsightsConnectionString) {
        $p.EnableAppInsights           = $true
        $p.AppInsightsConnectionString = $AppInsightsConnectionString
    }
    if ($NonInteractive) { $p.NonInteractive = $true }
    if ($DryRun)         { $p.DryRun         = $true }
    return $p
}

function Resolve-WwEngineHandover {
    <#
        Reads the engine deploy's TWO outputs and asserts them. Throws rather than returning
        a partial result: every failure below would otherwise produce a worker that starts,
        consumes messages, and fails every engine call.

        Does NOT resolve the App Insights connection string - the summary's copy is masked, so
        that one value needs an az call and is passed in separately by the caller.
    #>
    param(
        [Parameter(Mandatory)][string] $LogDir,
        [Parameter(Mandatory)][string] $AuthOutputPath,
        [Parameter(Mandatory)][string] $ExpectedAppName,
        [string] $ExpectedAuthAppName
    )
    $notes = @()

    # ── Engine summary ────────────────────────────────────────────────────────
    # Capture the item BEFORE reading .FullName: under Set-StrictMode -Version Latest,
    # (<nothing>).FullName raises a property-not-found error instead of returning $null,
    # which would replace the message below with a confusing one.
    $newest = Get-ChildItem -LiteralPath $LogDir -Filter 'deploy-WwExecutionEngine-*.summary.json' -ErrorAction SilentlyContinue |
              Where-Object Name -notmatch 'dryrun' | Sort-Object LastWriteTime | Select-Object -Last 1
    if (-not $newest) {
        throw "No non-dry-run engine summary in $LogDir. Deploy the engine first, or point -LogDir at the run that did."
    }
    $summaryPath = $newest.FullName
    $S = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json

    # Save-DeploySummary rewrites this after EVERY phase, so the newest is not necessarily a good one.
    $status = Get-WwProp $S 'status'
    if ($status -ne 'completed') {
        throw ("Engine deploy did not complete: status={0} lastPhase={1} error={2}  ({3})" -f
               $status, (Get-WwProp $S 'lastPhase' '?'), (Get-WwProp $S 'error' 'none'), $summaryPath)
    }
    if (Get-WwProp $S 'dryRun' $false) {
        throw "Newest summary is a dry run; refusing to wire the QueueProcessor from it. ($summaryPath)"
    }
    $appName = Get-WwProp $S 'appName'
    if ($appName -ne $ExpectedAppName) {
        throw "Summary is for app '$appName', expected '$ExpectedAppName'. ($summaryPath)"
    }

    # ── Auth output ───────────────────────────────────────────────────────────
    if (-not (Test-Path -LiteralPath $AuthOutputPath)) {
        throw "Auth output not found at $AuthOutputPath - was the engine deployed with -SkipAuthProvisioning?"
    }
    $A = Get-Content -LiteralPath $AuthOutputPath -Raw | ConvertFrom-Json

    # ONE fixed file, overwritten by EVERY engine deploy - prove it is this app's before trusting it.
    $authFor = Get-WwProp $A 'FunctionAppName'
    if ($authFor -ne $ExpectedAppName) {
        throw ("{0} belongs to '{1}', not '{2}'. It is a single fixed file overwritten by every deploy - re-read it immediately after the engine run." -f
               $AuthOutputPath, $authFor, $ExpectedAppName)
    }

    $entraName = Get-WwProp $A 'EntraAppDisplayName'
    if ($ExpectedAuthAppName -and $entraName -ne $ExpectedAuthAppName) {
        $notes += "Entra app display name differs from the derived default: $entraName"
    }

    $clientId = Get-WwProp $A 'ClientId'
    if (-not $clientId) {
        throw "ClientId missing from $AuthOutputPath - refusing to deploy a worker that cannot authenticate to the engine."
    }
    $spId   = Get-WwProp $A 'SpObjectId'
    if (-not $spId) { $notes += 'SpObjectId missing from the auth output; the app-role grant needs it resolved by hand.' }

    $roleId = (@(Get-WwProp $A 'AppRoles' @()) | Where-Object { (Get-WwProp $_ 'value') -eq 'Warewolf_QueueProcessor' } |
               Select-Object -First 1 | ForEach-Object { Get-WwProp $_ 'id' })
    if (-not $roleId) { $notes += 'authconfig does not define the Warewolf_QueueProcessor app role; the optional grant has nothing to assign.' }

    return [pscustomobject]@{
        SummaryPath          = $summaryPath
        RunId                = Get-WwProp $S 'runId'
        Endpoint             = Get-WwProp $S 'endpoint'          # authoritative - never hand-built
        AppInsightsName      = Get-WwProp $S 'appInsightsName'
        EntraAppDisplayName  = $entraName
        ClientId             = $clientId
        SpObjectId           = $spId
        Audience             = Get-WwProp $A 'Audience'
        QueueProcessorRoleId = $roleId
        AuthOutputPath       = $AuthOutputPath
        Notes                = $notes
    }
}

function New-WwHandoverObject {
    <#
        The consolidated file downstream steps read (app-role grant, token scripts, rollback).
        DELIBERATELY OMITTED: the App Insights connection string, the broker uri VALUE, and any
        Key Vault secret value. Names and uris only.
    #>
    param(
        [Parameter(Mandatory)][hashtable] $Context,
        [Parameter(Mandatory)][hashtable] $Handover,
        [string[]] $QueueProcessorApps = @(),
        [string[]] $Notes = @(),
        [bool] $Synthetic,
        [bool] $DryRun
    )
    return [ordered]@{
        timestampUtc   = (Get-Date).ToUniversalTime().ToString('o')
        producedBy     = 'Deploy-WwEngineAndQueueProcessor.ps1'
        synthetic      = $Synthetic
        dryRun         = $DryRun
        runId          = $Handover.RunId
        resourceTags   = @("wwx-test-run=$($Handover.RunId)")
        subscriptionId = $Context.Sub
        tenantId       = $Context.TenantId
        engine = [ordered]@{
            appName             = $Context.EngineApp
            endpoint            = $Handover.Endpoint
            resourceGroup       = $Context.Rg
            location            = $Context.Loc
            appInsightsName     = $Handover.AppInsightsName
            entraAppDisplayName = $Handover.EntraAppDisplayName
            summaryPath         = $Handover.SummaryPath
        }
        entra = [ordered]@{
            clientId                = $Handover.ClientId
            spObjectId              = $Handover.SpObjectId
            audience                = $Handover.Audience
            queueProcessorAppRoleId = $Handover.QueueProcessorRoleId
            authOutputPath          = $Handover.AuthOutputPath
        }
        keyVault = [ordered]@{
            name              = $Context.Kv
            secretName        = $Context.KvSecret
            rabbitMqSecretUri = $Context.RabbitSecretUri
        }
        queueProcessor = [ordered]@{
            appNamePrefix   = $Context.QpPrefix
            acaEnvironment  = $Context.AcaEnv
            acrName         = $Context.Acr
            imageRepository = $Context.ImageRepo
            triggerPath     = $Context.TriggerDir
            apps            = $QueueProcessorApps
        }
        notes = $Notes
    }
}

if ($LoadFunctionsOnly) { return }   # helpers are defined; nothing below this line runs

# ══════════════════════════════════════════════════════════════════════════════
# Path resolution
# ══════════════════════════════════════════════════════════════════════════════

$ScriptDir     = $PSScriptRoot
$EngineScript  = Join-Path $ScriptDir 'Deploy-WwExecutionEngine.ps1'
$QpScript      = Join-Path $ScriptDir 'Deploy-WwQueueProcessor.ps1'
# FIXED path, OVERWRITTEN BY EVERY ENGINE RUN — never per-run, never in $LogDir.
$AuthOutPath   = Join-Path $ScriptDir 'Configure-WwExecutionAuth.output.json'

# The orchestrators resolve their own helpers as SIBLINGS of themselves, and do NOT check for
# them up front — a missing one surfaces mid-phase, after resources have been created. On a
# machine where the Scripts folder was extracted from a zip rather than cloned, a partial
# extraction is the likely cause, so check the whole set here, before anything runs.
$requiredSiblings = [ordered]@{
    'Deploy-WwExecutionEngine.ps1'   = 'the engine orchestrator'
    'Deploy-WwQueueProcessor.ps1'    = 'the QueueProcessor orchestrator'
    'Configure-WwExecutionAuth.ps1'  = 'Entra + Easy Auth provisioning; also writes the auth output this script reads'
    'Setup-ApplicationInsights.ps1'  = 'App Insights provisioning (this wrapper always passes -EnableAppInsights)'
    'Encrypt-Config.ps1'             = 'secure.config validation/encryption, and the QueueProcessor -EncryptStagedSettings pass'
}
$missing = @($requiredSiblings.GetEnumerator() |
             Where-Object { -not (Test-Path -LiteralPath (Join-Path $ScriptDir $_.Key)) } |
             ForEach-Object { "  {0,-32} {1}" -f $_.Key, $_.Value })
if ($missing.Count) {
    throw ("{0} required script(s) missing from '{1}':`n{2}`n`nExtract the COMPLETE Scripts folder - these are resolved as siblings at run time." -f
           $missing.Count, $ScriptDir, ($missing -join "`n"))
}

# Soft dependencies: the engine degrades gracefully without these, but says so only in passing.
foreach ($opt in @(
        @{ f = 'Generate-WorkflowIndex.ps1'; why = 'without it the engine disk-scans workflows at startup instead of reading an index' }
        @{ f = 'Rollback-WwExecutionEngine.ps1'; why = 'needed only to tear this deployment down again' })) {
    if (-not (Test-Path -LiteralPath (Join-Path $ScriptDir $opt.f))) {
        Write-Warning ("{0} not found in {1} - {2}." -f $opt.f, $ScriptDir, $opt.why)
    }
}

$RunStamp      = Get-Date -Format 'yyyyMMdd-HHmmss'
$HandoverPath  = $null   # set once $LogDir is validated

# ══════════════════════════════════════════════════════════════════════════════
# Step 1 — Preflight
# ══════════════════════════════════════════════════════════════════════════════

Write-WwHead 'Step 1 - Preflight'

if ($SkipEngine -and $SkipQueueProcessor) { throw 'Both -SkipEngine and -SkipQueueProcessor were given; nothing to do.' }

$required = @{
    Rg = $Rg; Loc = $Loc; Kv = $Kv; KvSecret = $KvSecret; Stage = $Stage; LogDir = $LogDir
    SecureConfig = $SecureConfig; AuthConfig = $AuthConfig; LicenseConfig = $LicenseConfig
    WorkflowsSrc = $WorkflowsSrc; SourceDir = $SourceDir; TriggerDir = $TriggerDir
    EngineApp = $EngineApp; EngineStorage = $EngineStorage; EnginePublish = $EnginePublish
}
if (-not $SkipQueueProcessor) {
    $required += @{ Acr = $Acr; AcaEnv = $AcaEnv; QpPublish = $QpPublish; QpPrefix = $QpPrefix
                    ImageRepo = $ImageRepo; DockerFile = $DockerFile }
}
Assert-WwRequired $required
Write-WwOk 'Variable block filled in.'

if (-not (Get-Command az -ErrorAction SilentlyContinue)) { throw 'Azure CLI (az) is not on PATH.' }

$account = $null
try { $account = az account show -o json 2>$null | ConvertFrom-Json } catch { }
if (-not $account) { throw 'Not signed in to Azure. Run `az login` in this shell first (this script will not do it unattended).' }

if (-not $Sub)      { $Sub      = $account.id }
if (-not $TenantId) { $TenantId = $account.tenantId }
az account set --subscription $Sub | Out-Null
Write-WwOk "Subscription $Sub  tenant $TenantId"

if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Force -Path $LogDir | Out-Null }
$HandoverPath = Join-Path $LogDir "deploy-both-$RunStamp.handover.json"
Write-WwOk "Log directory $LogDir"

# ══════════════════════════════════════════════════════════════════════════════
# Step 2 — Publish paths
# ══════════════════════════════════════════════════════════════════════════════

Write-WwHead 'Step 2 - Publish paths'

if (-not $SkipEngine) {
    $EnginePublish = Expand-WwPublishPath -Path $EnginePublish -Label 'Engine publish'
    Assert-WwPackage -Dir $EnginePublish -Assembly 'Warewolf.Execution.Lightweight.dll' -Label 'Engine publish'
}

if (-not $SkipQueueProcessor) {
    $QpPublish = Expand-WwPublishPath -Path $QpPublish -Label 'QueueProcessor publish'
    Assert-WwPackage -Dir $QpPublish -Assembly 'Warewolf.Execution.QueueProcessor.dll' -Label 'QueueProcessor publish'

    # A stray .bite in the publish output is baked into the image alongside the staged
    # Settings tree and competes with the trigger the deploy actually intends to run.
    $strays = @(Get-ChildItem -LiteralPath $QpPublish -Recurse -Filter *.bite -ErrorAction SilentlyContinue)
    if ($strays.Count -gt 0) {
        throw ("QueueProcessor publish output contains {0} .bite file(s); it must contain none. Remove:`n{1}" -f
               $strays.Count, (($strays | ForEach-Object { '  ' + $_.FullName }) -join "`n"))
    }
    Write-WwOk 'QueueProcessor publish : no stray .bite files.'

    foreach ($d in @(@{p=$TriggerDir;l='Trigger folder'}, @{p=$SourceDir;l='Queue source folder'})) {
        if (-not (Test-Path -LiteralPath $d.p)) { throw "$($d.l) not found: $($d.p)" }
    }

    # Deploy-WwQueueProcessor.ps1 otherwise defaults this to a path two levels above the Scripts
    # folder inside a source checkout, which does not exist where the scripts came from a zip.
    # The image build is Phase 2 of that script - failing here saves a partial run.
    if (-not (Test-Path -LiteralPath $DockerFile)) {
        throw "Dockerfile not found: $DockerFile. It ships with the QueueProcessor source, not with the Scripts folder - copy it next to the publish output and point `$DockerFile at it."
    }
    Write-WwOk "Dockerfile $DockerFile"
    $triggerCount = @(Get-ChildItem -LiteralPath $TriggerDir -Filter '*.bite').Count
    if ($triggerCount -eq 0) { throw "No *.bite trigger files in $TriggerDir - the QueueProcessor deploy would have nothing to create." }
    Write-WwOk "$triggerCount trigger file(s) in $TriggerDir (one Container App each)."
}

# ══════════════════════════════════════════════════════════════════════════════
# Step 3 — Broker secret uri (versionless — KEDA re-reads it)
# ══════════════════════════════════════════════════════════════════════════════

if (-not $SkipQueueProcessor) {
    Write-WwHead 'Step 3 - Broker secret'
    $secret = Invoke-AzJson -AzArgs @('keyvault', 'secret', 'show', '--vault-name', $Kv, '--name', $RabbitSecretName) -AllowFail
    if (-not $secret) { throw "Secret '$RabbitSecretName' not found in vault '$Kv' (or no data-plane access to read it)." }
    if (-not $secret.attributes.enabled) { throw "Secret '$RabbitSecretName' in vault '$Kv' is disabled; KEDA could not read the queue depth." }

    $vault    = Invoke-AzJson -AzArgs @('keyvault', 'show', '--name', $Kv) -AllowFail
    $vaultUri = $vault.properties.vaultUri
    if (-not $vaultUri) { throw "Could not resolve the vault uri for '$Kv'." }
    $RabbitSecretUri = ($vaultUri.TrimEnd('/')) + "/secrets/$RabbitSecretName"
    Write-WwOk "Broker secret uri $RabbitSecretUri"
}

# ══════════════════════════════════════════════════════════════════════════════
# Step 4 — Deploy the Execution Engine
# ══════════════════════════════════════════════════════════════════════════════

# Every value the two splats and the handover file need, in one place.
$Ctx = @{
    Sub = $Sub; TenantId = $TenantId; Rg = $Rg; Loc = $Loc
    Kv = $Kv; KvSecret = $KvSecret; Acr = $Acr; AcaEnv = $AcaEnv
    LogDir = $LogDir; SecureConfig = $SecureConfig; AuthConfig = $AuthConfig
    LicenseConfig = $LicenseConfig; WorkflowsSrc = $WorkflowsSrc
    SourceDir = $SourceDir; TriggerDir = $TriggerDir
    EngineApp = $EngineApp; EngineStorage = $EngineStorage; EnginePublish = $EnginePublish
    EngineAi = $EngineAi; EngineAuthApp = $EngineAuthApp
    QpPublish = $QpPublish; QpPrefix = $QpPrefix; ImageRepo = $ImageRepo; DockerFile = $DockerFile
    RabbitSecretUri = $RabbitSecretUri
    EngineUrl = $null; EngineAppId = $null     # filled by step 5
}

if (-not $SkipEngine) {
    $engineParams = New-WwEngineParams -Context $Ctx `
                        -EncryptResources:$EncryptResources -NonInteractive:$NonInteractive -DryRun:$DryRun

    Confirm-WwStep -Title 'Deploy-WwExecutionEngine.ps1' -Params $engineParams
    Invoke-WwChild -Path $EngineScript -Params $engineParams -Label 'Deploy-WwExecutionEngine.ps1'
} else {
    Write-WwHead 'Step 4 - Execution Engine'
    Write-WwNote '-SkipEngine: resolving the handover values from the existing outputs instead.'
}

# ══════════════════════════════════════════════════════════════════════════════
# Step 5 — Capture the handover values
# ══════════════════════════════════════════════════════════════════════════════

Write-WwHead 'Step 5 - Capture'

$synthetic = $false
$notes     = @()
$HO        = $null

if ($DryRun -and -not $SkipEngine) {
    # A dry run writes only *.dryrun.summary.json and no auth output at all, so there is
    # nothing real to assert against. Emit obviously-fake values so the QueueProcessor's own
    # plan phase can still be reached, and mark the handover file as synthetic.
    $synthetic = $true
    $zero      = '00000000-0000-0000-0000-000000000000'
    $HO = [pscustomobject]@{
        SummaryPath          = '<dry run - no summary>'
        RunId                = "wwx-dryrun-$RunStamp"
        Endpoint             = "https://$EngineApp.azurewebsites.net"
        AppInsightsName      = $EngineAi
        EntraAppDisplayName  = $EngineAuthApp
        ClientId             = $zero
        SpObjectId           = $zero
        Audience             = "api://$zero"
        QueueProcessorRoleId = $zero
        AuthOutputPath       = '<dry run - not written>'
        Notes                = @()
    }
    $AiConn = $null
    $notes += 'SYNTHETIC dry-run values - not a record of any deployed resource.'
    Write-WwWarn 'Dry run: using SYNTHETIC handover values so the QueueProcessor plan phase can run.'
} else {
    $HO = Resolve-WwEngineHandover -LogDir $LogDir -AuthOutputPath $AuthOutPath `
                                   -ExpectedAppName $EngineApp -ExpectedAuthAppName $EngineAuthApp
    foreach ($n in $HO.Notes) { Write-WwWarn $n }
    $notes += $HO.Notes

    if ($HO.AppInsightsName)     { $EngineAi      = $HO.AppInsightsName }
    if ($HO.EntraAppDisplayName) { $EngineAuthApp = $HO.EntraAppDisplayName }

    Write-WwOk "Summary $($HO.SummaryPath)"
    Write-WwOk "runId $($HO.RunId)  endpoint $($HO.Endpoint)"
    Write-WwOk "Auth output $AuthOutPath"
    Write-WwOk "appId $($HO.ClientId)  spId $($HO.SpObjectId)  roleId $($HO.QueueProcessorRoleId)"

    # ── App Insights connection string ────────────────────────────────────────
    # The summary's copy is MASKED, so it can only come from Azure.
    $ai     = Invoke-AzJson -AzArgs @('monitor', 'app-insights', 'component', 'show', '--app', $EngineAi, '-g', $Rg) -AllowFail
    $AiConn = if ($ai) { $ai.connectionString } else { $null }
    if (-not $AiConn) {
        Write-WwWarn "No App Insights connection string for '$EngineAi'; -EnableAppInsights will be omitted from the QueueProcessor rather than passed empty."
        $notes += "App Insights connection string unresolved for $EngineAi."
    } else {
        Write-WwOk "App Insights $EngineAi connection string resolved (masked: $(Get-WwMasked $AiConn))."
    }
}

$Ctx.EngineUrl   = $HO.Endpoint
$Ctx.EngineAppId = $HO.ClientId
$Ctx.EngineAi    = $EngineAi
$Ctx.EngineAuthApp = $EngineAuthApp

# ── Consolidated handover file ────────────────────────────────────────────────
# Written now so a failure in step 6 still leaves the engine's identifiers recorded, then
# rewritten after step 6 to add the Container App names.
function Write-WwHandover {
    param([string[]]$QpApps = @())
    New-WwHandoverObject -Context $Ctx -Handover @{
                             RunId                = $HO.RunId
                             Endpoint             = $HO.Endpoint
                             AppInsightsName      = $EngineAi
                             EntraAppDisplayName  = $EngineAuthApp
                             SummaryPath          = $HO.SummaryPath
                             ClientId             = $HO.ClientId
                             SpObjectId           = $HO.SpObjectId
                             Audience             = $HO.Audience
                             QueueProcessorRoleId = $HO.QueueProcessorRoleId
                             AuthOutputPath       = $HO.AuthOutputPath
                         } `
                         -QueueProcessorApps $QpApps -Notes $notes `
                         -Synthetic:$synthetic -DryRun:$DryRun |
        ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $HandoverPath -Encoding UTF8
}

Write-WwHandover
Write-WwOk "Handover written to $HandoverPath"

# ══════════════════════════════════════════════════════════════════════════════
# Step 6 — Deploy the QueueProcessor
# ══════════════════════════════════════════════════════════════════════════════

$qpApps = @()

if (-not $SkipQueueProcessor) {
    if (-not $Ctx.EngineUrl)   { throw 'No engine endpoint resolved; refusing to deploy the QueueProcessor.' }
    if (-not $Ctx.EngineAppId) { throw 'No engine app id resolved; refusing to deploy the QueueProcessor.' }

    $qpParams = New-WwQpParams -Context $Ctx -AppInsightsConnectionString $AiConn `
                    -NonInteractive:$NonInteractive -DryRun:$DryRun

    Confirm-WwStep -Title 'Deploy-WwQueueProcessor.ps1' -Params $qpParams -Mask @('AppInsightsConnectionString')
    Invoke-WwChild -Path $QpScript -Params $qpParams -Label 'Deploy-WwQueueProcessor.ps1'

    if (-not $DryRun) {
        # Filter in PowerShell, never with --query (see Invoke-AzJson).
        $allApps = Invoke-AzJson -AzArgs @('containerapp', 'list', '-g', $Rg) -AllowFail
        $qpApps  = @($allApps | Where-Object { $_.name -and $_.name.StartsWith($QpPrefix) } |
                     ForEach-Object { $_.name } | Sort-Object)
        # Every name must look like one, or the handover file poisons the app-role grant downstream.
        foreach ($n in $qpApps) {
            if (-not (Test-WwAppName $n)) {
                throw "Resolved Container App name '$n' is not a valid app name; refusing to write it to the handover file."
            }
        }
        if (-not $qpApps.Count) { Write-WwWarn "No Container Apps found with prefix '$QpPrefix' in '$Rg'." }
        Write-WwOk ("Container Apps: {0}" -f ($qpApps -join ', '))
        Write-WwHandover -QpApps $qpApps
    }
}

# ══════════════════════════════════════════════════════════════════════════════
# Done
# ══════════════════════════════════════════════════════════════════════════════

Write-WwHead 'Done'
Write-Host ("  {0,-16}: {1}" -f 'Handover', $HandoverPath) -ForegroundColor White
Write-Host ("  {0,-16}: {1}" -f 'Engine summary', $HO.SummaryPath) -ForegroundColor White
Write-Host ("  {0,-16}: {1}" -f 'Endpoint', $HO.Endpoint) -ForegroundColor White
Write-Host ("  {0,-16}: {1}" -f 'Run tag', "wwx-test-run=$($HO.RunId)  (Rollback-WwExecutionEngine.ps1 targets this tag)") -ForegroundColor White
if ($qpApps.Count) { Write-Host ("  {0,-16}: {1}" -f 'Queue apps', ($qpApps -join ', ')) -ForegroundColor White }
if ($synthetic)    { Write-WwWarn 'Dry run - the handover file holds SYNTHETIC values.' }
Write-Host ''
Write-WwNote 'NEXT (recommended): grant each QueueProcessor managed identity the engine app role'
Write-WwNote 'Warewolf_QueueProcessor, and add a per-workflow Execute row to secure.config.'
Write-WwNote 'See docs/Deploy-Both-RunGuide.md section 5 - it reads this handover file.'
