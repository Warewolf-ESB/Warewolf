#Requires -Version 7.0
<#
.SYNOPSIS
    Validates the standard G:\Deployment staging layout, publishes both apps if needed, and emits the
    manifest that Invoke-WwE2EVerification.ps1 consumes.

.DESCRIPTION
    Works against the FIXED staging layout this team maintains, rather than generating a throwaway tree:

        G:\Deployment\apps\ExecutionEngine                     engine publish      (published here, or supply it)
        G:\Deployment\apps\QueueProcessor                      worker publish      (published here, or supply it)
        G:\Deployment\settings\secure.config                   engine
        G:\Deployment\settings\Deploy-WwExecutionEngine.authconfig.json   engine
        G:\Deployment\settings\ElasticsearchLoggingSource.bite  engine  (worker: NOT SUPPORTED - see below)
        G:\Deployment\settings\Warewolf License.secureconfig    engine
        G:\Deployment\sources                                   RabbitMQ sources referenced by triggers
        G:\Deployment\triggers                                  RabbitMQ queue-triggers
        C:\ProgramData\Warewolf\Resources                        workflows / resources

    Triggers and sources are treated as YOUR authored inputs and are never modified. Every Azure
    resource still gets a unique run suffix, so two reviewers cannot collide on Function App, storage,
    Container App, image-repository or Key Vault-secret names.

    QUEUE NAMES, however, come from the trigger files as authored, so they are NOT suffixed. If another
    Container App already consumes those queues it will compete for messages - and if it forwards to a
    stopped or older engine it will dead-letter its share while the queue still drains, which looks
    like a clean run. This script checks for exactly that and reports it. Use -GenerateTriggers to work
    on isolated copies with suffixed queue names instead.

    ELASTICSEARCH: the ENGINE supports it (WAREWOLF_LOGGING_CONFIG's "elasticsearch" field
    [WOLF-8516; formerly ENABLEELASTICSEARCHLOGGING] + Settings/
    ElasticsearchLoggingSource.bite, which must carry that exact filename). The QUEUEPROCESSOR does
    NOT - there is no Elasticsearch logger in the worker and no parameter on
    Deploy-WwQueueProcessor.ps1; its source catalog deliberately SKIPS non-RabbitMQ sources. Wiring it
    would need new worker code. This script therefore enables ES for the engine only, and says so.

.PARAMETER GenerateTriggers
    Generate isolated, suffixed copies of the triggers (new TriggerIds, new queue names, generated
    plaintext source) into <StageRoot>\triggers-e2e-<suffix> instead of using the authored ones.
    Use when another deployment already consumes the authored queues.

.PARAMETER Publish
    Run `dotnet publish` into the two publish directories. SEQUENTIAL: the projects share transitive
    project references, so parallel builds contend on the same obj\ output.

.EXAMPLE
    # Standard layout, publish both apps
    .\New-WwE2EStaging.ps1 -Publish

.EXAMPLE
    # Isolated queues, because the authored ones are already in use
    .\New-WwE2EStaging.ps1 -Publish -GenerateTriggers `
        -AmqpUri 'amqp://user:pass@broker:5672/'

.NOTES
    Companion: Invoke-WwE2EVerification.ps1, docs\E2E-Harness-README.md.
#>
[CmdletBinding()]
param(
    # ── Fixed staging layout ──────────────────────────────────────────────────
    [string] $StageRoot            = 'G:\Deployment',
    [string] $EnginePublishPath,
    [string] $QueueProcessorPublishPath,
    [string] $SecureConfigPath,
    [string] $AuthConfigPath,
    [string] $ElasticsearchSourcePath,
    [string] $LicenseConfigPath,
    [string] $QueueSourcePath,
    [string] $TriggerPath,
    [string] $WorkflowsSourcePath  = 'C:\ProgramData\Warewolf\Resources',
    [string] $LogDir,

    [string] $RunSuffix,
    [string] $RepoRoot,

    # ── Broker (only needed for -GenerateTriggers, or to override the KEDA secret) ──
    [string] $AmqpUri,

    # ── Isolated-trigger generation ───────────────────────────────────────────
    [switch] $GenerateTriggers,
    [string] $SuccessWorkflow    = 'rabbit\RabbitProcess',
    [string] $FailureWorkflow    = 'rabbit\RabbitProcessFailure',
    [string] $InputName          = 'message',
    [int]    $SuccessConcurrency = 3,
    [int]    $FailureConcurrency = 1,
    [int]    $Prefetch           = 1,
    [bool]   $QueueDurable       = $false,

    # ── Verification extras ───────────────────────────────────────────────────
    [string] $PublisherWorkflow  = 'rabbit\RabbitPublish',
    [string] $SmokeWorkflow      = 'Hello World',
    [switch] $EnableElasticsearch,

    # ── Contention check ──────────────────────────────────────────────────────
    [string] $ResourceGroup      = 'DEV2',
    [switch] $SkipQueueContentionCheck,

    # ── Ops ───────────────────────────────────────────────────────────────────
    [switch] $Publish,
    [switch] $Force,
    [switch] $DryRun,
    [switch] $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'WwE2E.Common.psm1') -Force

# ═════════════════════════════════════════════════════════════════════════════
# Resolve the layout
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 0  Resolve staging layout'

# Repo root is INFERRED then VALIDATED, never assumed. These scripts are routinely copied outside the
# repo (e.g. to G:\Deployment\Scripts), where $PSScriptRoot\..\..\.. resolves to something like 'G:\' -
# which then made the staging root look "inside the repo" and tripped the credential guard below.
function Test-RepoRoot { param([string] $Path)
    if (-not $Path -or -not (Test-Path -LiteralPath $Path)) { return $false }
    (Test-Path -LiteralPath (Join-Path $Path 'Dev\Warewolf.Execution.QueueProcessor')) -and
    (Test-Path -LiteralPath (Join-Path $Path 'Dev\Warewolf.Execution.Lightweight'))
}

if ($RepoRoot) {
    if (-not (Test-RepoRoot $RepoRoot)) { throw "-RepoRoot '$RepoRoot' does not look like the Warewolf repo (expected Dev\Warewolf.Execution.Lightweight and ...QueueProcessor beneath it)." }
} else {
    $candidate = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..') -ErrorAction SilentlyContinue)?.Path
    $RepoRoot = if (Test-RepoRoot $candidate) { $candidate } else { $null }
}

if ($RepoRoot) {
    $lightweightRoot   = Join-Path $RepoRoot 'Dev\Warewolf.Execution.Lightweight'
    $queueProcessorDir = Join-Path $RepoRoot 'Dev\Warewolf.Execution.QueueProcessor'
    Write-E2EOk "Repo root: $RepoRoot"
} else {
    $lightweightRoot = $null; $queueProcessorDir = $null
    Write-E2ENote ('Repo not detected from this script location, so -Publish and -GenerateTriggers are ' +
                   'unavailable (both need the csproj / trigger templates). Pass -RepoRoot to enable them. ' +
                   'Verifying an existing publish needs no repo.')
}

if (-not $RunSuffix) { $RunSuffix = New-E2ERunSuffix }
if ($RunSuffix -notmatch '^[a-z0-9]{4,8}$') { throw "RunSuffix must be 4-8 lowercase alphanumerics; got '$RunSuffix'." }

$StageRoot = [System.IO.Path]::GetFullPath($StageRoot)
# Only meaningful when a repo was actually detected.
if ($RepoRoot -and $StageRoot.StartsWith($RepoRoot, [StringComparison]::OrdinalIgnoreCase) -and -not $Force) {
    throw "StageRoot '$StageRoot' is inside the repo. The manifest can hold broker credentials; keep it out of git, or pass -Force."
}

# Every path defaults off $StageRoot, so a single -StageRoot relocates the whole layout.
if (-not $EnginePublishPath)         { $EnginePublishPath         = Join-Path $StageRoot 'apps\ExecutionEngine' }
if (-not $QueueProcessorPublishPath) { $QueueProcessorPublishPath = Join-Path $StageRoot 'apps\QueueProcessor' }
if (-not $SecureConfigPath)          { $SecureConfigPath          = Join-Path $StageRoot 'settings\secure.config' }
if (-not $AuthConfigPath)            { $AuthConfigPath            = Join-Path $StageRoot 'settings\Deploy-WwExecutionEngine.authconfig.json' }
if (-not $ElasticsearchSourcePath)   { $ElasticsearchSourcePath   = Join-Path $StageRoot 'settings\ElasticsearchLoggingSource.bite' }
if (-not $LicenseConfigPath)         { $LicenseConfigPath         = Join-Path $StageRoot 'settings\Warewolf License.secureconfig' }
if (-not $QueueSourcePath)           { $QueueSourcePath           = Join-Path $StageRoot 'sources' }
if (-not $TriggerPath)               { $TriggerPath               = Join-Path $StageRoot 'triggers' }
if (-not $LogDir)                    { $LogDir                    = Join-Path $StageRoot "logs\e2e-$RunSuffix" }

$names = [ordered]@{
    RunSuffix         = $RunSuffix
    EngineApp         = "wwengine-e2e-$RunSuffix"
    EngineStorage     = "stwwe2e$RunSuffix"
    EngineAppInsights = "wwengine-e2e-$RunSuffix-ai"
    EngineAuthApp     = "wwengine-e2e-$RunSuffix-auth"
    QpPrefix          = "wwqpe2e$RunSuffix-"
    ImageRepository   = "warewolf/queueprocessor-e2e-$RunSuffix"
}
if ($names.EngineStorage.Length -gt 24) { throw "Derived storage name '$($names.EngineStorage)' exceeds 24 chars; use a shorter -RunSuffix." }

Write-Host ''
Write-Host '  -- Paths (item numbers match the agreed layout) --' -ForegroundColor Gray
@(
    "1. Engine publish        : $EnginePublishPath"
    "2. Worker publish        : $QueueProcessorPublishPath"
    "3. authconfig  (engine)  : $AuthConfigPath"
    "4. Elasticsearch source  : $ElasticsearchSourcePath"
    "5. secure.config (engine): $SecureConfigPath"
    "6. Licence     (engine)  : $LicenseConfigPath"
    "7. RabbitMQ sources      : $QueueSourcePath"
    "8. RabbitMQ triggers     : $TriggerPath"
    "9. Workflows / resources : $WorkflowsSourcePath"
    "   Logs + summaries      : $LogDir"
) | ForEach-Object { Write-Host "     $_" }

Write-Host ''
Write-Host '  -- Azure names (run-suffixed, so concurrent reviewers cannot collide) --' -ForegroundColor Gray
$names.GetEnumerator() | ForEach-Object { Write-Host ("     {0,-18}: {1}" -f $_.Key, $_.Value) }

if ($DryRun) { Write-Host ''; Write-E2ENote 'DryRun: nothing was created or published.'; return }

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
# Publish
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 1  Application publishes'

if ($Publish -and -not $RepoRoot) {
    throw '-Publish needs the repo (for the csproj files) but it was not detected from this script location. Pass -RepoRoot, or supply an existing publish in the two publish directories.'
}

$projects = @(
    @{ Name = 'ExecutionEngine'; Csproj = if ($lightweightRoot)   { Join-Path $lightweightRoot   'Warewolf.Execution.Lightweight.csproj' }   else { $null }; Out = $EnginePublishPath;         Dll = 'Warewolf.Execution.Lightweight.dll' }
    @{ Name = 'QueueProcessor';  Csproj = if ($queueProcessorDir) { Join-Path $queueProcessorDir 'Warewolf.Execution.QueueProcessor.csproj' } else { $null }; Out = $QueueProcessorPublishPath; Dll = 'Warewolf.Execution.QueueProcessor.dll' }
)

foreach ($proj in $projects) {
    $already = Test-Path -LiteralPath (Join-Path $proj.Out $proj.Dll)
    if (-not $already -and -not $proj.Csproj) {
        throw ("$($proj.Name) is not published at '$($proj.Out)' and the repo was not detected, so it cannot be " +
               'built here. Supply a framework-dependent publish there, or pass -RepoRoot.')
    }
    if ($Publish) {
        Write-E2EStep "dotnet publish $($proj.Name) -> $($proj.Out)"
        New-Item -ItemType Directory -Force -Path $proj.Out | Out-Null
        & dotnet publish $proj.Csproj -c Release -o $proj.Out --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $($proj.Name) (exit $LASTEXITCODE)." }
        Write-E2EOk "$($proj.Name) published."
    } elseif ($already) {
        $stamp = (Get-Item (Join-Path $proj.Out $proj.Dll)).LastWriteTime
        Write-E2EOk "$($proj.Name): existing publish reused (built $stamp)."
    } elseif ($NonInteractive) {
        throw ("$($proj.Name) is not published at '$($proj.Out)'. Re-run with -Publish, or place a " +
               'framework-dependent publish there (a release build from warewolf.io/release-notes works).')
    } else {
        Write-E2ENote "$($proj.Name) is NOT published at '$($proj.Out)'."
        $answer = Read-Host "  Publish it now? [Y/n]"
        if ($answer -match '^(n|no)$') {
            throw "$($proj.Name) publish is required before verification. Supply it at '$($proj.Out)' and re-run."
        }
        New-Item -ItemType Directory -Force -Path $proj.Out | Out-Null
        & dotnet publish $proj.Csproj -c Release -o $proj.Out --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $($proj.Name) (exit $LASTEXITCODE)." }
        Write-E2EOk "$($proj.Name) published."
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Engine settings
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 2  Engine settings'

if (Test-Path -LiteralPath $SecureConfigPath) {
    $sc = Get-Content -LiteralPath $SecureConfigPath -Raw | ConvertFrom-Json
    $grants = @($sc.WindowsGroupPermissions)
    Write-E2EOk "secure.config: $($grants.Count) permission row(s)."
    $globalExec = @($grants | Where-Object { $_.IsServer -and $_.Execute })
    if ($globalExec.Count -gt 0) {
        Write-E2EOk ("  global Execute: {0}" -f (($globalExec | ForEach-Object { $_.WindowsGroup }) -join ', '))
        if ('Public' -in ($globalExec | ForEach-Object { $_.WindowsGroup })) {
            Write-E2EOk "  'Public' holds global Execute, so the worker identity executes without an app role."
        }
    } else {
        Write-E2ENote '  No global Execute row: trigger workflows will return 500 unless each has an Execute-bearing resource row (WOLF-8418 wraps denials as 500, not 403).'
    }
} else {
    Write-E2EBad "secure.config not found at '$SecureConfigPath'."
}

if (Test-Path -LiteralPath $AuthConfigPath) {
    try {
        $ac = Get-Content -LiteralPath $AuthConfigPath -Raw | ConvertFrom-Json
        $groups = @($ac.GroupPermissions.PSObject.Properties.Name)
        Write-E2EOk "authconfig: $($groups.Count) group(s) -> $($groups -join ', ')"
        if ('Warewolf_QueueProcessor' -notin $groups) {
            Write-E2ENote "  authconfig does not define 'Warewolf_QueueProcessor'; the optional app-role grant would have nothing to assign (the grant is defence in depth, not a prerequisite)."
        }
    } catch { Write-E2EBad "authconfig is not valid JSON: $($_.Exception.Message)" }
} else {
    Write-E2EBad "authconfig not found at '$AuthConfigPath'."
}

$licenseOk = Test-Path -LiteralPath $LicenseConfigPath
if ($licenseOk) { Write-E2EOk 'Warewolf licence present.' }
else { Write-E2ENote "No licence at '$LicenseConfigPath'. The engine licence check defaults to ON, so startup may fail." }

# Elasticsearch: engine only. The filename must be exactly ElasticsearchLoggingSource.bite - the engine
# reads that literal path under Settings\.
$esOk = Test-Path -LiteralPath $ElasticsearchSourcePath
$esLeaf = Split-Path $ElasticsearchSourcePath -Leaf
if ($EnableElasticsearch) {
    if (-not $esOk) { throw "-EnableElasticsearch was set but '$ElasticsearchSourcePath' does not exist." }
    if ($esLeaf -ne 'ElasticsearchLoggingSource.bite') {
        throw "Elasticsearch source must be named exactly 'ElasticsearchLoggingSource.bite' (found '$esLeaf'); the engine reads that literal filename."
    }
    Write-E2EOk "Elasticsearch logging ENABLED for the engine ($esLeaf)."
} elseif ($esOk) {
    Write-E2ENote "Elasticsearch source present but logging is OFF. Pass -EnableElasticsearch to stage and enable it."
}
Write-E2ENote ('The QUEUEPROCESSOR has no Elasticsearch logger and Deploy-WwQueueProcessor.ps1 has no ES ' +
               'parameter, so ES applies to the ENGINE only. The worker logs to console + App Insights; ' +
               'its source catalog deliberately skips non-RabbitMQ sources.')

# ═════════════════════════════════════════════════════════════════════════════
# Triggers and sources
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 3  Triggers and RabbitMQ sources'

$sourceIdForGenerated = $null

if ($GenerateTriggers) {
    if (-not $AmqpUri) { throw '-GenerateTriggers needs -AmqpUri, because it generates the RabbitMQ source too.' }
    if (-not $RepoRoot) { throw '-GenerateTriggers needs the repo (for the trigger/source templates) but it was not detected. Pass -RepoRoot.' }
    $u = [Uri]$AmqpUri
    $ui = $u.UserInfo.Split(':', 2)
    if ($ui.Count -lt 2) { throw 'AmqpUri must include user:password.' }
    $vhost = if ($u.AbsolutePath -in @('', '/')) { '/' } else { $u.AbsolutePath.TrimStart('/') }

    $TriggerPath     = Join-Path $StageRoot "triggers-e2e-$RunSuffix"
    $QueueSourcePath = Join-Path $StageRoot "sources-e2e-$RunSuffix"
    New-Item -ItemType Directory -Force -Path $TriggerPath, $QueueSourcePath | Out-Null

    $sourceTemplate = Get-ChildItem (Join-Path $queueProcessorDir 'Settings\sources') -Filter *.bite | Select-Object -First 1
    if (-not $sourceTemplate) { throw "No source template under '$queueProcessorDir\Settings\sources'." }
    $sourceIdForGenerated = [guid]::NewGuid().ToString()
    $xml = [xml](Get-Content -LiteralPath $sourceTemplate.FullName -Raw)
    $xml.Source.SetAttribute('ID', $sourceIdForGenerated)
    $xml.Source.SetAttribute('Name', "WwE2E RabbitMQ Source $RunSuffix")
    # PLAINTEXT on purpose: a Studio-exported source is usually DPAPI, which is machine-scoped and can
    # never be read inside the Linux worker. -EncryptStagedSettings converts this to WFAES at deploy.
    $xml.Source.SetAttribute('ConnectionString',
        "HostName=$($u.Host);Port=$($u.Port);UserName=$($ui[0]);Password=$($ui[1]);VirtualHost=$vhost")
    $xml.Source.DisplayName = "WwE2E RabbitMQ Source $RunSuffix"
    if ($xml.Source.VersionInfo) { $xml.Source.VersionInfo.SetAttribute('ResourceId', $sourceIdForGenerated) }
    $xml.Save((Join-Path $QueueSourcePath "$sourceIdForGenerated.bite"))
    Write-E2EOk "Generated plaintext source $sourceIdForGenerated.bite"

    $triggerTemplate = Get-ChildItem (Join-Path $queueProcessorDir 'Settings\triggers') -Filter *.bite | Select-Object -First 1
    if (-not $triggerTemplate) { throw "No trigger template under '$queueProcessorDir\Settings\triggers'." }
    $tmpl = Get-Content -LiteralPath $triggerTemplate.FullName -Raw

    foreach ($spec in @(
        @{ Name = "WwE2ESuccess$RunSuffix"; Queue = "wwe2e-$RunSuffix-success"; Workflow = $SuccessWorkflow; Concurrency = $SuccessConcurrency }
        @{ Name = "WwE2EFailure$RunSuffix"; Queue = "wwe2e-$RunSuffix-failure"; Workflow = $FailureWorkflow; Concurrency = $FailureConcurrency }
    )) {
        $t = $tmpl | ConvertFrom-Json
        $id = [guid]::NewGuid().ToString()
        $t.Id = $id; $t.TriggerId = $id
        $t.Name = $spec.Name; $t.QueueName = $spec.Queue; $t.WorkflowName = $spec.Workflow
        $t.Concurrency = $spec.Concurrency; $t.Prefetch = $Prefetch
        $t.QueueSourceId = $sourceIdForGenerated; $t.QueueSinkId = $sourceIdForGenerated
        $t.DeadLetterQueue = "$($spec.Queue)-errors"; $t.ResourceId = [guid]::NewGuid().ToString()
        $t.UserName = $null; $t.Password = $null       # credentials belong in the SOURCE, not the trigger
        foreach ($set in @($t.Options, $t.DeadLetterOptions)) {
            $d = $set | Where-Object { $_.Name -eq 'Durable' } | Select-Object -First 1
            if ($d) { $d.Value = $QueueDurable }
        }
        $fi = $t.Inputs | Select-Object -First 1
        if ($fi) { $fi.Name = $InputName; $fi.FullName = $InputName }
        $t | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $TriggerPath "$id.bite") -Encoding UTF8
        Write-E2EOk ("Generated trigger '{0}' -> queue '{1}' (Concurrency {2} => maxReplicas)" -f $spec.Name, $spec.Queue, $spec.Concurrency)
    }
    Write-E2ENote "Using ISOLATED triggers in '$TriggerPath' (authored triggers untouched)."
} else {
    Write-E2EOk "Using AUTHORED triggers in '$TriggerPath' (never modified)."
}

$triggerFiles = @(Get-ChildItem -LiteralPath $TriggerPath -Filter *.bite -ErrorAction SilentlyContinue)
if ($triggerFiles.Count -eq 0) { throw "No trigger .bite files found in '$TriggerPath'. Zero matches is a hard error, never a silent no-op." }

$sourceFiles = @(Get-ChildItem -LiteralPath $QueueSourcePath -Filter *.bite -ErrorAction SilentlyContinue)
$sourceIds = @{}
foreach ($sf in $sourceFiles) {
    try {
        $sx = [xml](Get-Content -LiteralPath $sf.FullName -Raw)
        if ($sx.Source -and $sx.Source.ID) { $sourceIds[$sx.Source.ID] = $sf.Name }
    } catch { Write-E2ENote "  '$($sf.Name)' is not parseable as a source: $($_.Exception.Message)" }
}
Write-E2EOk "Sources available: $($sourceIds.Count) (matched by ID, so the filename does not matter)"

$triggers = [System.Collections.Generic.List[object]]::new()
$unresolved = [System.Collections.Generic.List[string]]::new()
foreach ($tf in $triggerFiles) {
    $t = Read-E2ETrigger -Path $tf.FullName
    foreach ($w in (Test-E2ETriggerSupported -Trigger $t)) { Write-E2ENote "  $w" }

    # Both QueueSourceId AND QueueSinkId must resolve, or the deploy aborts before creating anything.
    foreach ($needed in @($t.QueueSourceId, $t.QueueSinkId) | Where-Object { $_ } | Select-Object -Unique) {
        if (-not $sourceIds.ContainsKey($needed)) { $unresolved.Add("$($t.Name) -> source '$needed'") }
    }
    $wfPath = Join-Path $WorkflowsSourcePath "$($t.WorkflowName).bite"
    $wfStaged = Test-Path -LiteralPath $wfPath

    Write-E2EOk ("{0,-26} queue='{1}' wf='{2}' conc={3} prefetch={4} durable={5} wfStaged={6}" -f `
                 $t.Name, $t.QueueName, $t.WorkflowName, $t.Concurrency, $t.Prefetch, $t.Durable, $wfStaged)
    if (-not $wfStaged) {
        Write-E2EBad ("  workflow '{0}' is NOT under '{1}'. A worker would consume the message, POST to a route " +
                      'that does not exist, then DEAD-LETTER AND ACK it - the queue drains and nothing runs.' -f `
                      $t.WorkflowName, $WorkflowsSourcePath)
    }

    $triggers.Add([ordered]@{
        triggerId = $t.TriggerId; file = $t.Path; name = $t.Name; queue = $t.QueueName
        workflow = $t.WorkflowName; concurrency = $t.Concurrency; prefetch = $t.Prefetch
        deadLetterQueue = $t.DeadLetterQueue; durable = $t.Durable
        queueSourceId = $t.QueueSourceId; workflowStaged = $wfStaged
    })
}
if ($unresolved.Count -gt 0) {
    Write-E2EBad "Unresolved RabbitMQ source(s): $($unresolved -join '; ')"
    Write-E2ENote "  The deploy would abort with 'Unresolved RabbitMQ source(s); nothing was deployed' BEFORE creating anything."
}

# Primary trigger for the burst test: the highest Concurrency gives the most informative peak.
$primaryIndex = 0
for ($i = 1; $i -lt $triggers.Count; $i++) { if ($triggers[$i].concurrency -gt $triggers[$primaryIndex].concurrency) { $primaryIndex = $i } }
Write-E2EOk ("Primary trigger for the scale test: '{0}' (Concurrency {1})" -f $triggers[$primaryIndex].name, $triggers[$primaryIndex].concurrency)

# ── queue contention ──────────────────────────────────────────────────────────
# The real hazard when queue names are not suffixed: another Container App with a rabbitmq scale rule
# on the same queue competes for messages, and if it points at a stopped/older engine it dead-letters
# its share while the queue still drains - indistinguishable from a clean run without this check.
$contention = [System.Collections.Generic.List[string]]::new()
# Tri-state, NOT a boolean: 'clear' | 'contended' | 'unknown'. An earlier version reported "no
# contention" when the check had actually thrown, which is a false pass - the exact failure mode this
# check exists to prevent.
$contentionState = 'unknown'
if ($SkipQueueContentionCheck) {
    $contentionState = 'skipped'
    Write-E2ENote 'Queue contention check skipped (-SkipQueueContentionCheck).'
} else {
    try {
        $hits = @(Get-E2EQueueConsumers -ResourceGroup $ResourceGroup -QueueName @($triggers.queue))
        foreach ($h in $hits) { $contention.Add("$($h.App) -> '$($h.Queue)' (min=$($h.MinReplicas) max=$($h.MaxReplicas))") }
        $contentionState = if ($contention.Count -gt 0) { 'contended' } else { 'clear' }
        if ($contentionState -eq 'contended') {
            Write-E2EBad "QUEUE CONTENTION: existing Container App(s) already consume this run's queues:"
            $contention | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
            Write-E2ENote '  They compete for messages, and any pointing at a stopped or older engine DEAD-LETTERS AND ACKS'
            Write-E2ENote '  its share - so the queue still drains and the run looks clean while half the messages never ran.'
            Write-E2ENote '  Fix by one of:'
            Write-E2ENote '    - re-run with -GenerateTriggers   (isolated, suffixed queues; recommended)'
            Write-E2ENote '    - park the other apps            az containerapp update -n <app> -g <rg> --min-replicas 0 --max-replicas 0'
            Write-E2ENote '    - delete them if they are stale  az containerapp delete -n <app> -g <rg> --yes'
        } else {
            Write-E2EOk 'No existing Container App consumes these queues.'
        }
    } catch {
        $contentionState = 'unknown'
        Write-E2EBad "Queue contention check FAILED (result unknown, not clear): $($_.Exception.Message)"
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Readiness + manifest
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 4  Readiness'

$checks = [System.Collections.Generic.List[object]]::new()
function Add-Check { param([string]$Name,[bool]$Ok,[string]$Detail,[bool]$Blocking=$true)
    $checks.Add([pscustomobject]@{ Name=$Name; Ok=$Ok; Detail=$Detail; Blocking=$Blocking }) }

Add-Check '1. Engine publish present' (Test-Path (Join-Path $EnginePublishPath 'Warewolf.Execution.Lightweight.dll')) $EnginePublishPath
Add-Check '2. Worker publish present' (Test-Path (Join-Path $QueueProcessorPublishPath 'Warewolf.Execution.QueueProcessor.dll')) $QueueProcessorPublishPath
$biteInPublish = @(Get-ChildItem $QueueProcessorPublishPath -Recurse -Filter *.bite -ErrorAction SilentlyContinue).Count
Add-Check '2. Worker publish carries no .bite' ($biteInPublish -eq 0) "found $biteInPublish (must be 0, or committed config could leak into the image)"
Add-Check '2. Publish paths differ' ($EnginePublishPath -ne $QueueProcessorPublishPath) 'the engine deploy fails at plan time on a collision'
Add-Check '2. RabbitMQ.Client available' (Test-Path (Join-Path $QueueProcessorPublishPath 'RabbitMQ.Client.dll')) 'harness needs it for topology + queue depth'
Add-Check '3. authconfig present' (Test-Path $AuthConfigPath) $AuthConfigPath
Add-Check '5. secure.config present' (Test-Path $SecureConfigPath) $SecureConfigPath
Add-Check '6. Licence present' $licenseOk 'engine licence check defaults to ON' $false
Add-Check '4. Elasticsearch source present' $esOk 'engine only; required only with -EnableElasticsearch' ([bool]$EnableElasticsearch)
Add-Check '7. Sources resolve for every trigger' ($unresolved.Count -eq 0) ($unresolved -join '; ')
Add-Check '8. Trigger files found' ($triggers.Count -gt 0) "$($triggers.Count) trigger(s) in $TriggerPath"
Add-Check '9. Workflows source exists' (Test-Path -LiteralPath $WorkflowsSourcePath) $WorkflowsSourcePath
foreach ($t in $triggers) { Add-Check "9. Workflow '$($t.workflow)' staged" $t.workflowStaged "under $WorkflowsSourcePath" }
foreach ($wf in @($PublisherWorkflow, $SmokeWorkflow)) {
    Add-Check "9. Workflow '$wf' staged" (Test-Path -LiteralPath (Join-Path $WorkflowsSourcePath "$wf.bite")) 'needed by the verification itself'
}
$aspNetRoot = 'C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App'
$aspNetOk = (Test-Path $aspNetRoot) -and [bool](Get-ChildItem $aspNetRoot -Directory -ErrorAction SilentlyContinue | Where-Object Name -like '8.*')
Add-Check 'Microsoft.AspNetCore.App 8.x present' $aspNetOk 'supplies System.Threading.RateLimiting for RabbitMQ.Client 7.x'
# Only 'clear' is a pass. 'contended' and 'unknown' are both reported as not-OK so a failed check can
# never masquerade as a clean one.
Add-Check 'No queue contention' ($contentionState -eq 'clear') `
    $(switch ($contentionState) {
        'contended' { "CONTENDED: $($contention -join '; ')" }
        'unknown'   { 'could NOT be verified - treat as unknown, not clear' }
        'skipped'   { 'not checked (-SkipQueueContentionCheck)' }
        default     { '' }
      }) $false

foreach ($c in $checks) {
    if ($c.Ok) { Write-E2EOk $c.Name } elseif ($c.Blocking) { Write-E2EBad "$($c.Name) -- $($c.Detail)" } else { Write-E2ENote "$($c.Name) -- $($c.Detail)" }
}
$blocking = @($checks | Where-Object { -not $_.Ok -and $_.Blocking })

$manifest = [ordered]@{
    schema     = 'wwe2e-staging/2'
    createdUtc = (Get-Date).ToUniversalTime().ToString('o')
    runSuffix  = $RunSuffix
    repoRoot   = $RepoRoot
    stageRoot  = $StageRoot
    names      = $names
    paths      = [ordered]@{
        EnginePublish    = $EnginePublishPath
        QpPublish        = $QueueProcessorPublishPath
        Settings         = (Join-Path $StageRoot 'settings')
        Sources          = $QueueSourcePath
        Triggers         = $TriggerPath
        Logs             = $LogDir
    }
    engineInputs = [ordered]@{
        secureConfig        = $SecureConfigPath
        authConfig          = $AuthConfigPath
        licenseConfig       = if ($licenseOk) { $LicenseConfigPath } else { $null }
        elasticsearchSource = if ($esOk) { $ElasticsearchSourcePath } else { $null }
        enableElasticsearch = [bool]$EnableElasticsearch
        workflowsSource     = $WorkflowsSourcePath
    }
    broker     = [ordered]@{
        amqpUri            = $AmqpUri                 # may be null when using authored sources
        generatedSourceId  = $sourceIdForGenerated
        triggersGenerated  = [bool]$GenerateTriggers
    }
    triggers            = @($triggers)
    primaryTriggerIndex = $primaryIndex
    workflows  = [ordered]@{ publisher = $PublisherWorkflow; smoke = $SmokeWorkflow; inputName = $InputName }
    queueContention      = @($contention)
    queueContentionState = $contentionState
    readiness  = [ordered]@{
        ready  = ($blocking.Count -eq 0)
        checks = @($checks | ForEach-Object { [ordered]@{ name=$_.Name; ok=$_.Ok; blocking=$_.Blocking; detail=$_.Detail } })
    }
}

$manifestPath = Join-Path $LogDir 'staging-manifest.json'
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-E2EPhase 'Staging complete'
Write-Host "  Stage root : $StageRoot"
Write-Host "  Manifest   : $manifestPath"
Write-Host "  Run suffix : $RunSuffix"
if ($AmqpUri) { Write-Host ''; Write-E2ENote 'The manifest contains the broker password. Keep it out of git.' }

if ($blocking.Count -gt 0) {
    Write-Host ''
    Write-E2EBad "$($blocking.Count) blocking readiness check(s) failed:"
    $blocking | ForEach-Object { Write-Host "      - $($_.Name): $($_.Detail)" -ForegroundColor Red }
    exit 1
}

Write-Host ''
Write-E2EOk 'Staging is READY. Next step:'
Write-Host "      .\Invoke-WwE2EVerification.ps1 -StagingManifest '$manifestPath'" -ForegroundColor Cyan
Write-Host '      (dry run; add -Execute to deploy for real)' -ForegroundColor DarkGray
exit 0
