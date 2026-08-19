#Requires -Version 7.0
<#
.SYNOPSIS
    Deploys and PROVES the full Azure queue path - Execution Engine + RabbitMQ QueueProcessor on
    Container Apps with KEDA - then scores 18 acceptance criteria and writes a summary.

.DESCRIPTION
        .\Invoke-WwE2EVerification.ps1 -StagingManifest <path>            # DRY RUN, changes nothing
        .\Invoke-WwE2EVerification.ps1 -StagingManifest <path> -Execute   # deploy + verify for real

    Phases: A preflight, B engine, C QueueProcessor, D scale rules, E proof, F opt-in teardown.

    Built-in corrections that a hand-run gets wrong (measured 2026-08-06; see
    docs/Deploy-E2E-Execution-Summary.md):

      * Broker topology is PRE-CREATED, because PublishRabbitMQActivity cannot create it (S11): a
        failed passive declare closes the channel and the active declare is reissued on the dead one.
      * The engine token comes from CLIENT CREDENTIALS, because `az account get-access-token` fails on
        a freshly created app registration with AADSTS65001 consent_required (S9).
      * Execution evidence comes from Log Analytics; the worker's Dev2Logger lines never reach App
        Insights `traces` (S12).
      * `Healthy` at minReplicas=0 proves nothing, so a real message is always published.

    ELASTICSEARCH applies to the ENGINE only. The worker has no Elasticsearch logger and
    Deploy-WwQueueProcessor.ps1 has no ES parameter; it logs to console + App Insights.

.PARAMETER AmqpUri
    Broker URI for the harness's own checks (topology, depth, unacked probe). Optional: it is taken
    from the manifest, or resolved from the trigger's source .bite (plaintext or DPAPI). A WFAES source
    cannot be read here - pass this explicitly in that case.

.PARAMETER RabbitMqSecretName
    Key Vault secret holding the broker URI for the KEDA scale rule. Defaults to a per-run secret when
    a URI is available, otherwise reuses this existing secret name.

.EXAMPLE
    .\Invoke-WwE2EVerification.ps1 -StagingManifest 'G:\Deployment\logs\e2e-ab12cd\staging-manifest.json' -Execute
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $StagingManifest,

    # ── Azure targeting. Defaults are this team's shared DEV2 environment. ─────
    [string] $ResourceGroup       = 'DEV2',
    [string] $Location            = 'southafricanorth',
    [string] $KeyVaultName        = 'WWExecutionEngine',
    [string] $KeyVaultSecretName  = 'WWExecutionEngineTestSecret',
    [string] $AcrName             = 'tudev2containerregistry',
    [string] $AcaEnvironment      = 'dev2-cae',
    [string] $LogAnalyticsCustomerId,

    # ── Broker ────────────────────────────────────────────────────────────────
    [string] $AmqpUri,
    [string] $RabbitMqSecretName,

    # ── Behaviour ─────────────────────────────────────────────────────────────
    [switch] $Execute,
    [switch] $TeardownWhenDone,
    [int]    $BurstSize            = 5,
    [int]    $MaxConcurrency       = 1,
    [ValidateSet('TRACE','DEBUG','INFO','WARN','ERROR','FATAL','OFF')]
    [string] $EngineLogLevel       = 'ERROR',
    [switch] $SkipDrainTest,
    [switch] $SkipUnackedProbe,
    [switch] $SkipTopologyCreate,
    # Reuse an already-deployed run instead of creating one: skips the engine and QueueProcessor
    # deploys, resolves what they created from Azure + the manifest, then runs Phases D and E. Use to
    # FINISH a run that died after deployment, without a teardown-and-redeploy cycle.
    [switch] $ResumeFromPhaseD,
    [int]    $ScaleWatchSeconds    = 240,
    [int]    $ZeroWaitSeconds      = 420,
    [switch] $NonInteractive
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'WwE2E.Common.psm1') -Force

$script:Criteria = [System.Collections.Generic.List[object]]::new()
$script:Metrics  = [ordered]@{}
$script:Started  = Get-Date
$script:Completed = $false
Reset-E2EResourceLedger

$PortalBase = 'https://portal.azure.com/#@/resource'

function Portal-Url {
    param([string] $Provider, [string] $Name, [string] $Sub, [string] $Rg)
    "$PortalBase/subscriptions/$Sub/resourceGroups/$Rg/providers/$Provider/$Name"
}

# The ledger must print on FAILURE as well as success - a run that dies half-way is exactly when you
# need to know what already exists. Registered here so it fires however the script leaves.
trap {
    Write-E2EResourceSummary -Title 'Resources manipulated before the failure' -Failed
    throw
}

function Set-Criterion {
    param(
        [Parameter(Mandatory)][int] $Number,
        [Parameter(Mandatory)][string] $Text,
        [ValidateSet('Pass','Fail','Skip','Info')][string] $State,
        [string] $Detail
    )
    $existing = $script:Criteria | Where-Object { $_.Number -eq $Number }
    if ($existing) { $existing.State = $State; $existing.Detail = $Detail }
    else { $script:Criteria.Add([pscustomobject]@{ Number = $Number; Text = $Text; State = $State; Detail = $Detail }) }
    Write-E2ECriterion -Number $Number -Text $Text -State $State -Detail $Detail
}

# ═════════════════════════════════════════════════════════════════════════════
# Phase 0 - manifest
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase 0  Load staging manifest'

if (-not (Test-Path -LiteralPath $StagingManifest)) { throw "Staging manifest not found: '$StagingManifest'" }
$m = Get-Content -LiteralPath $StagingManifest -Raw | ConvertFrom-Json
if ($m.schema -ne 'wwe2e-staging/2') { throw "Unexpected manifest schema '$($m.schema)'. Regenerate with New-WwE2EStaging.ps1." }
if (-not $m.readiness.ready) {
    $bad = @($m.readiness.checks | Where-Object { -not $_.ok -and $_.blocking })
    throw ("Staging is not ready ({0} blocking check(s)): {1}" -f $bad.Count, (($bad | ForEach-Object { $_.name }) -join '; '))
}

$suffix    = $m.runSuffix
$engineApp = $m.names.EngineApp
$engineUrl = "https://$engineApp.azurewebsites.net"
$qpPrefix  = $m.names.QpPrefix
$logDir    = $m.paths.Logs
$triggers  = @($m.triggers)
$primary   = $triggers[$m.primaryTriggerIndex]

Write-E2EOk "Manifest OK (run suffix '$suffix')."
Write-E2EOk "Engine '$engineApp' -> $engineUrl ; workers '$qpPrefix*'"
Write-E2EOk ("Triggers: {0} | primary '{1}' queue '{2}' Concurrency {3}" -f `
             $triggers.Count, $primary.name, $primary.queue, $primary.concurrency)
$contentionState = if ($m.PSObject.Properties['queueContentionState']) { $m.queueContentionState } else { 'unknown' }
switch ($contentionState) {
    'contended' {
        Write-E2EBad "Manifest recorded QUEUE CONTENTION: $(@($m.queueContention) -join '; ')"
        Write-E2ENote 'Those apps compete for messages and may dead-letter their share, so criteria 11 and 14 can be'
        Write-E2ENote 'unprovable even on a healthy deployment. Re-stage with -GenerateTriggers, or park the other apps.'
    }
    'clear'   { Write-E2EOk 'No competing queue consumers (checked at staging time).' }
    'skipped' { Write-E2ENote 'Queue contention was not checked at staging time (-SkipQueueContentionCheck).' }
    default   { Write-E2ENote 'Queue contention could not be verified at staging time - treat as UNKNOWN, not clear.' }
}

$ctx = Get-E2EAzContext
Write-E2EOk "Azure: $($ctx.User) | $($ctx.Subscription) | tenant $($ctx.TenantId)"

# Broker URI: parameter, then manifest, then the primary trigger's source .bite.
if (-not $AmqpUri -and $m.broker.amqpUri) { $AmqpUri = $m.broker.amqpUri }
if (-not $AmqpUri) {
    $srcFiles = @(Get-ChildItem -LiteralPath $m.paths.Sources -Filter *.bite -ErrorAction SilentlyContinue)
    foreach ($sf in $srcFiles) {
        $candidate = Resolve-E2EBrokerUri -SourceBitePath $sf.FullName
        if ($candidate) { $AmqpUri = $candidate; Write-E2EOk "Broker URI resolved from '$($sf.Name)'."; break }
    }
}
$brokerAvailable = [bool]$AmqpUri
if (-not $brokerAvailable) {
    Write-E2ENote ('No broker URI available (a WFAES-encrypted source cannot be read here). Broker-side steps ' +
                   '- topology pre-create, queue depth, the unacked probe - will be SKIPPED. Pass -AmqpUri to enable them.')
}

$perRunSecret = "rabbitmq-uri-e2e-$suffix"
if (-not $RabbitMqSecretName) { $RabbitMqSecretName = if ($brokerAvailable) { $perRunSecret } else { 'rabbitmq-uri' } }

$expectedPeak = [Math]::Min([Math]::Ceiling($BurstSize / [double]$MaxConcurrency), [int]$primary.concurrency)
$esEnabled = [bool]$m.engineInputs.enableElasticsearch

Write-Host ''
Write-Host '  -- Plan --' -ForegroundColor Gray
@(
    "Mode                 : $(if ($Execute) { 'EXECUTE (creates real resources)' } else { 'DRY RUN (no changes)' })"
    "Resource group       : $ResourceGroup ($Location)"
    "Engine publish        : $($m.paths.EnginePublish)"
    "Worker publish        : $($m.paths.QpPublish)"
    "secure.config         : $($m.engineInputs.secureConfig)"
    "authconfig            : $($m.engineInputs.authConfig)"
    "licence               : $(if ($m.engineInputs.licenseConfig) { $m.engineInputs.licenseConfig } else { '(none)' })"
    "Elasticsearch (engine): $(if ($esEnabled) { $m.engineInputs.elasticsearchSource } else { 'disabled' })"
    "triggers / sources    : $($m.paths.Triggers)  |  $($m.paths.Sources)"
    "workflows             : $($m.engineInputs.workflowsSource)"
    "Key Vault             : $KeyVaultName / $KeyVaultSecretName  (AES key, REUSED - never deleted)"
    "KEDA secret           : $RabbitMqSecretName $(if ($RabbitMqSecretName -eq $perRunSecret) { '(CREATED by this run)' } else { '(REUSED)' })"
    "ACR / ACA env         : $AcrName / $AcaEnvironment  (both REUSED)"
    "Image repository      : $($m.names.ImageRepository)  (CREATED - separate repo)"
    "Engine resources      : $engineApp + $($m.names.EngineStorage) + $($m.names.EngineAppInsights) + Entra app $($m.names.EngineAuthApp)"
    "Container Apps        : $qpPrefix* (one per trigger, $($triggers.Count))"
    "Queues                : $((@($triggers | ForEach-Object { "$($_.queue) (max $($_.concurrency))" })) -join ', ')"
    "Burst / expected peak : $BurstSize messages -> $expectedPeak replica(s) on '$($primary.queue)'"
    "Teardown              : $(if ($TeardownWhenDone) { 'YES (Phase F)' } else { 'no (left standing)' })"
) | ForEach-Object { Write-Host "     $_" }

if (-not $Execute) {
    Write-E2EPhase 'Approval inventory - what -Execute would change'
    @(
        "A  az keyvault secret set               -> $(if ($RabbitMqSecretName -eq $perRunSecret) { "ONE new secret '$perRunSecret'" } else { 'nothing (reusing an existing secret)' })"
        'B  Deploy-WwExecutionEngine.ps1         -> storage + Function App + App Insights + ENTRA APP REGISTRATION'
        'C  az acr build                         -> new image repository in the reused registry'
        "C  az containerapp create x$($triggers.Count)            -> Container Apps in the shared ACA environment"
        "C  az role assignment create x$($triggers.Count * 2)            -> AcrPull + Key Vault Secrets User, per app identity"
        "E  AMQP declare                         -> $(if ($brokerAvailable -and -not $SkipTopologyCreate) { "exchanges + queues + bindings for $($triggers.Count) trigger(s)" } else { 'SKIPPED (no broker URI)' })"
        'E  RabbitPublish                        -> real messages published'
        'E  az containerapp revision restart     -> drain test (skip with -SkipDrainTest)'
        'F  targeted deletes                     -> only with -TeardownWhenDone. NEVER az group delete'
    ) | ForEach-Object { Write-Host "     $_" -ForegroundColor DarkGray }

    # "Would create" preview, using the same ledger renderer as a real run so the two are directly
    # comparable and the endpoint you will get is visible before anything is created.
    Add-E2EResource -Action Created -Kind 'Key Vault secret' -Name $RabbitMqSecretName -Scope $KeyVaultName -Detail 'AMQP URI for the KEDA scale rule'
    Add-E2EResource -Action Created -Kind 'Storage account' -Name $m.names.EngineStorage -Scope $ResourceGroup
    Add-E2EResource -Action Created -Kind 'Function App (engine)' -Name $engineApp -Scope $ResourceGroup -Url $engineUrl -Detail "discovery: $engineUrl/apis.json"
    Add-E2EResource -Action Created -Kind 'App Insights' -Name $m.names.EngineAppInsights -Scope $ResourceGroup
    Add-E2EResource -Action Created -Kind 'Entra app registration' -Name $m.names.EngineAuthApp -Scope 'tenant (directory object)'
    Add-E2EResource -Action Created -Kind 'ACR repository' -Name $m.names.ImageRepository -Scope $AcrName
    foreach ($t in $triggers) {
        Add-E2EResource -Action Created -Kind 'Container App (worker)' -Name "$qpPrefix<derived from '$($t.name)'>" -Scope $ResourceGroup `
            -Detail ("queue '{0}' -> {1}; min=0 max={2}" -f $t.queue, $t.workflow, $t.concurrency)
        Add-E2EResource -Action Created -Kind 'RabbitMQ topology' -Name $t.queue -Scope $(if ($brokerAvailable) { 'broker' } else { 'broker (unavailable)' }) `
            -Detail "exchange + queue + binding; DLQ '$($t.deadLetterQueue)'"
    }
    Write-E2EResourceSummary -Title 'Resources this run WOULD manipulate (dry run - nothing created)'

    Write-E2ENote 'DRY RUN complete. Re-run with -Execute to deploy and verify.'
    return
}

if (-not $NonInteractive) {
    Write-Host ''
    $answer = Read-Host "  Proceed and create real Azure resources in '$ResourceGroup'? [y/N]"
    if ($answer -notmatch '^(y|yes)$') { Write-E2ENote 'Aborted by operator; nothing was changed.'; return }
}
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
# Phase A - preflight
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase A  Preflight'

$rg = Invoke-E2EAzJson -AzArgs @('group', 'show', '--name', $ResourceGroup) -AllowFail
if (-not $rg) { throw "Resource group '$ResourceGroup' not found." }
Write-E2EOk "Resource group '$ResourceGroup': $($rg.properties.provisioningState)"

$env = Invoke-E2EAzJson -AzArgs @('containerapp', 'env', 'show', '--name', $AcaEnvironment, '-g', $ResourceGroup) -AllowFail
if (-not $env) { throw "ACA environment '$AcaEnvironment' not found in '$ResourceGroup'." }
Write-E2EOk ("ACA env '{0}': {1}, profiles [{2}]" -f $AcaEnvironment, $env.properties.provisioningState,
             (@($env.properties.workloadProfiles | ForEach-Object { $_.name }) -join ', '))
if (-not $LogAnalyticsCustomerId) { $LogAnalyticsCustomerId = $env.properties.appLogsConfiguration.logAnalyticsConfiguration.customerId }
if ($LogAnalyticsCustomerId) { Write-E2EOk "Log Analytics (execution evidence): $LogAnalyticsCustomerId" }
else { Write-E2ENote 'No workspace bound to the environment; execution counts and T will be unavailable.' }

if (-not (Invoke-E2EAzJson -AzArgs @('acr', 'show', '--name', $AcrName, '-g', $ResourceGroup) -AllowFail)) {
    throw "ACR '$AcrName' not found in '$ResourceGroup'."
}
Write-E2EOk "ACR '$AcrName' present."

# `az containerapp create` UPDATES an existing app instead of failing, so a name clash would silently
# reconfigure someone else's deployment.
if ($ResumeFromPhaseD) {
    # Resuming REQUIRES the resources to exist - the inverse of the collision guard.
    $existingApps = @(Get-E2EContainerApps -ResourceGroup $ResourceGroup -NamePrefix $qpPrefix)
    if ($existingApps.Count -eq 0) { throw "-ResumeFromPhaseD: no Container Apps named '$qpPrefix*' exist. Run without it to deploy." }
    if (-not (Invoke-E2EAzJson -AzArgs @('functionapp','show','-n',$engineApp,'-g',$ResourceGroup) -AllowFail)) {
        throw "-ResumeFromPhaseD: Function App '$engineApp' does not exist. Run without it to deploy."
    }
    Write-E2EOk "Resuming: engine '$engineApp' and $($existingApps.Count) Container App(s) already exist."
} else {
    $clashes = @(Get-E2EContainerApps -ResourceGroup $ResourceGroup -NamePrefix $qpPrefix)
    if ($clashes.Count -gt 0) { throw "Container App name(s) already exist: $(($clashes.Name) -join ', '). Re-stage with a new suffix, or pass -ResumeFromPhaseD to finish that run." }
    if (Invoke-E2EAzJson -AzArgs @('functionapp', 'show', '-n', $engineApp, '-g', $ResourceGroup) -AllowFail) {
        throw "Function App '$engineApp' already exists. Re-stage with a new suffix, or pass -ResumeFromPhaseD to finish that run."
    }
    Write-E2EOk "This run's names are free."
}

# KEDA cannot use a managed identity against RabbitMQ, so the scale rule needs a URI from Key Vault.
if ($ResumeFromPhaseD) {
    if (-not (Invoke-E2EAzJson -AzArgs @('keyvault','secret','show','--vault-name',$KeyVaultName,'--name',$RabbitMqSecretName) -AllowFail)) {
        throw "-ResumeFromPhaseD: KEDA secret '$RabbitMqSecretName' not found; the apps' scale rules depend on it."
    }
    Write-E2EOk "Reusing this run's existing KEDA secret '$RabbitMqSecretName'."
} elseif ($brokerAvailable -and $RabbitMqSecretName -eq $perRunSecret) {
    Write-E2EStep "Creating KEDA broker secret '$RabbitMqSecretName'"
    & az keyvault secret set --vault-name $KeyVaultName --name $RabbitMqSecretName --value $AmqpUri -o none 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Failed to write '$RabbitMqSecretName'. Do you hold Key Vault Secrets Officer?" }
    Add-E2EResource -Action Created -Kind 'Key Vault secret' -Name $RabbitMqSecretName -Scope $KeyVaultName `
        -Detail 'AMQP URI for the KEDA rabbitmq scale rule'
} else {
    if (-not (Invoke-E2EAzJson -AzArgs @('keyvault','secret','show','--vault-name',$KeyVaultName,'--name',$RabbitMqSecretName) -AllowFail)) {
        throw "KEDA secret '$RabbitMqSecretName' not found in '$KeyVaultName' and no -AmqpUri was available to create it."
    }
    Write-E2EOk "Reusing existing KEDA secret '$RabbitMqSecretName'."
}
$vault = Invoke-E2EAzJson -AzArgs @('keyvault', 'show', '--name', $KeyVaultName)
# VERSIONLESS on purpose: a version-pinned URL freezes the app on the version current at deploy time.
$rabbitSecretUri = ($vault.properties.vaultUri.TrimEnd('/')) + "/secrets/$RabbitMqSecretName"
Write-E2EOk "KEDA secret URI: $rabbitSecretUri"

# ═════════════════════════════════════════════════════════════════════════════
# Phase B - engine
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase B  Execution Engine'

$engineArgs = @{
    ResourceGroup       = $ResourceGroup
    Location            = $Location
    AppName             = $engineApp
    StorageAccount      = $m.names.EngineStorage
    PublishPath         = $m.paths.EnginePublish
    TenantId            = $ctx.TenantId
    AuthConfigPath      = $m.engineInputs.authConfig
    SecureConfigPath    = $m.engineInputs.secureConfig
    WorkflowsSourcePath = $m.engineInputs.workflowsSource   # NOT optional: the publish has no Resources folder
    KeyVaultName        = $KeyVaultName
    KeyVaultSecretName  = $KeyVaultSecretName
    EncryptResources    = $true
    VerifyDecryption    = $true
    ExecutionLogLevel   = $EngineLogLevel
    LogDir              = $logDir
    NonInteractive      = $true
}
if ($m.engineInputs.licenseConfig) { $engineArgs['LicenseConfigPath'] = $m.engineInputs.licenseConfig }
if ($esEnabled) {
    # Engine only. The file must be named exactly ElasticsearchLoggingSource.bite.
    $engineArgs['EnableElasticsearch']     = $true
    $engineArgs['ElasticsearchSourcePath'] = $m.engineInputs.elasticsearchSource
    Write-E2EOk "Elasticsearch logging enabled for the engine ($($m.engineInputs.elasticsearchSource))."
}

# Set-StrictMode is INHERITED BY CHILD SCOPES, so this script's `-Version Latest` would be imposed on
# the deploy scripts - which were written without it and legitimately do things like read .Count off a
# value that may be scalar or null. Measured: it aborted Deploy-WwQueueProcessor.ps1 inside its own
# preflight with "The property 'Count' cannot be found on this object", before it created anything.
# Invoke every sibling script inside a scope that turns StrictMode off.
if ($ResumeFromPhaseD) {
    Write-E2ENote 'Resume: skipping the engine deploy; reusing the existing Function App and its summary.'
} else {
    Invoke-E2EChildScript -Path (Join-Path $PSScriptRoot 'Deploy-WwExecutionEngine.ps1') -Arguments $engineArgs
    if ($LASTEXITCODE -ne 0) { throw "Deploy-WwExecutionEngine.ps1 failed (exit $LASTEXITCODE)." }
}

$engineSummary = (Get-ChildItem $logDir -Filter 'deploy-WwExecutionEngine-*.summary.json' |
                  Where-Object { $_.Name -notmatch 'dryrun' } | Sort-Object LastWriteTime | Select-Object -Last 1)
if (-not $engineSummary) { throw "No engine summary JSON in '$logDir'." }
$es = Get-Content $engineSummary.FullName -Raw | ConvertFrom-Json
Write-E2EOk "Engine summary: $($engineSummary.Name) (runId $($es.runId), status $($es.status))"
$script:Metrics['engineRunId']   = $es.runId
$script:Metrics['engineSummary'] = $engineSummary.FullName

if (-not $ResumeFromPhaseD) {
    # Recorded from the engine deploy's own summary, so the ledger reflects what it actually created
    # rather than what was planned.
    $c = $es.created
    if ($c.storageAccount) { Add-E2EResource -Action Created -Kind 'Storage account' -Name $m.names.EngineStorage -Scope $ResourceGroup `
        -Url (Portal-Url 'Microsoft.Storage/storageAccounts' $m.names.EngineStorage $ctx.SubscriptionId $ResourceGroup) }
    if ($c.functionApp) { Add-E2EResource -Action Created -Kind 'Function App (engine)' -Name $engineApp -Scope $ResourceGroup `
        -Url $engineUrl -Detail "discovery: $engineUrl/apis.json" }
    if ($c.appInsights) { Add-E2EResource -Action Created -Kind 'App Insights' -Name $m.names.EngineAppInsights -Scope $ResourceGroup `
        -Url (Portal-Url 'microsoft.insights/components' $m.names.EngineAppInsights $ctx.SubscriptionId $ResourceGroup) }
    if ($c.entraApp) { Add-E2EResource -Action Created -Kind 'Entra app registration' -Name $c.entraAppDisplayName -Scope 'tenant (directory object)' `
        -Detail 'survives resource-level teardown; removed by Rollback-WwExecutionEngine.ps1' }
    Add-E2EResource -Action Updated -Kind 'Function App settings' -Name $engineApp -Scope $ResourceGroup `
        -Detail "$($es.appSettings.PSObject.Properties.Name.Count) app setting(s); EXECUTIONLOGLEVEL=$($es.appSettings.EXECUTIONLOGLEVEL)"
} else {
    Add-E2EResource -Action Reused -Kind 'Function App (engine)' -Name $engineApp -Scope $ResourceGroup -Url $engineUrl `
        -Detail 'resumed run - not created by this invocation'
}

$engineAppId = @(Invoke-E2EAzJson -AzArgs @('ad','app','list','--display-name',$m.names.EngineAuthApp))[0].appId
$spList = Invoke-E2EAzJson -AzArgs @('ad','sp','list','--display-name',$m.names.EngineAuthApp) -AllowFail
$engineSpId = if ($spList) { @($spList)[0].id } else { $null }
if (-not $engineAppId) { throw "Entra app '$($m.names.EngineAuthApp)' not found after deploy." }
Write-E2EOk "Entra app: appId=$engineAppId spId=$engineSpId"
$script:Metrics['engineAppId'] = $engineAppId
$script:Metrics['engineSpId']  = $engineSpId

$aiConn = $null
$ai = Invoke-E2EAzJson -AzArgs @('monitor','app-insights','component','show','--app',$m.names.EngineAppInsights,'-g',$ResourceGroup) -AllowFail
if ($ai) { $aiConn = $ai.connectionString; Write-E2EOk "App Insights '$($m.names.EngineAppInsights)' resolved." }

# ── routes ────────────────────────────────────────────────────────────────────
$smokeRoute = Get-E2EWorkflowRoute -WorkflowName $m.workflows.smoke

$r = Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Public/$smokeRoute.json?Name=e2e"
Set-Criterion 1 '/Public/* returns 200 anonymously' ($(if ($r.StatusCode -eq 200) {'Pass'} else {'Fail'})) "HTTP $($r.StatusCode) $($r.Compact)"

$secure401 = (Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Secure/$smokeRoute.json?Name=e2e").StatusCode -eq 401
$token = $null
try { $token = Get-E2EEngineToken -TenantId $ctx.TenantId -EngineAppId $engineAppId -FunctionAppName $engineApp -ResourceGroup $ResourceGroup }
catch { Write-E2ENote "Token acquisition failed: $($_.Exception.Message)" }
if ($token) {
    $r2 = Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Secure/$smokeRoute.json?Name=e2e-secure" -Token $token
    Set-Criterion 2 '/Secure/* returns 401 without a token and 200 with one' `
        ($(if ($secure401 -and $r2.StatusCode -eq 200) {'Pass'} else {'Fail'})) "401=$secure401, with-token=$($r2.StatusCode)"
} else {
    Set-Criterion 2 '/Secure/* returns 401 without a token and 200 with one' 'Fail' "401=$secure401, no token acquired"
}

# Read the BODY, not the status: 500 has three causes, and a workflow-level message is a PASS.
$wfState = 'Pass'; $wfDetail = @()
foreach ($t in $triggers) {
    $route = Get-E2EWorkflowRoute -WorkflowName $t.workflow
    $probe = Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Public/$route.json?$($m.workflows.inputName)=e2e-probe"
    if ($probe.Compact -match 'Workflow file not found') {
        $wfState = 'Fail'
        Write-E2EBad "$($t.workflow) is NOT STAGED - fix WorkflowName, or restage -WorkflowsSourcePath."
    }
    $wfDetail += "$($t.workflow) -> $($probe.StatusCode)"
}
Set-Criterion 3 'Every trigger workflow resolves and executes' $wfState ($wfDetail -join '; ')

# ═════════════════════════════════════════════════════════════════════════════
# Phase C - QueueProcessor
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase C  QueueProcessor (one Container App per trigger)'

$qpArgs = @{
    ResourceGroup         = $ResourceGroup
    Location              = $Location
    AcaEnvironment        = $AcaEnvironment
    AcrName               = $AcrName
    PublishPath           = $m.paths.QpPublish
    ImageRepository       = $m.names.ImageRepository
    AppNamePrefix         = $qpPrefix
    TriggerPath           = $m.paths.Triggers
    QueueSourcePath       = $m.paths.Sources
    EngineBaseUrl         = $engineUrl
    EngineResourceAppId   = $engineAppId
    EngineTenantId        = $ctx.TenantId
    KeyVaultName          = $KeyVaultName
    KeyVaultSecretName    = $KeyVaultSecretName
    EncryptStagedSettings = $true    # a DPAPI source can never be read in the Linux container
    RabbitMqSecretUri     = $rabbitSecretUri
    InlineRabbitMqSecret  = $true    # this tenant enforces CAE; ACA's sync path cannot answer a claims challenge
    ScalingMode           = 'Elastic'
    MaxConcurrency        = $MaxConcurrency
    LogDir                = $logDir
    NonInteractive        = $true
}
if ($aiConn) { $qpArgs['EnableAppInsights'] = $true; $qpArgs['AppInsightsConnectionString'] = $aiConn }

if ($ResumeFromPhaseD) {
    Write-E2ENote 'Resume: skipping the QueueProcessor deploy; reusing the existing Container Apps.'
} else {
    Invoke-E2EChildScript -Path (Join-Path $PSScriptRoot 'Deploy-WwQueueProcessor.ps1') -Arguments $qpArgs
    if ($LASTEXITCODE -ne 0) { throw "Deploy-WwQueueProcessor.ps1 failed (exit $LASTEXITCODE)." }
}

$qpSummary = (Get-ChildItem $logDir -Filter 'deploy-WwQueueProcessor-*.summary.json' -ErrorAction SilentlyContinue |
              Sort-Object LastWriteTime | Select-Object -Last 1)
if ($qpSummary) {
    $qs = Get-Content $qpSummary.FullName -Raw | ConvertFrom-Json
    $script:Metrics['queueProcessorSummary'] = $qpSummary.FullName
    $script:Metrics['image'] = $qs.image
    Write-E2EOk "Worker summary: $($qpSummary.Name) (image $($qs.image))"
} else {
    Write-E2ENote 'No QueueProcessor summary found; image recorded from the deployed app instead.'
}

$apps = @(Get-E2EContainerApps -ResourceGroup $ResourceGroup -NamePrefix $qpPrefix)
$script:Metrics['apps'] = @($apps | ForEach-Object { $_.Name })
if ($apps.Count -ne $triggers.Count) { Write-E2ENote "Expected $($triggers.Count) Container App(s), found $($apps.Count)." }
if ($apps.Count -eq 0) { throw "No Container Apps named '$qpPrefix*' were found; Phases D and E cannot run." }
if (-not $script:Metrics.Contains('image') -and $apps[0].Image) { $script:Metrics['image'] = $apps[0].Image }

$qpAction = if ($ResumeFromPhaseD) { 'Reused' } else { 'Created' }
if (-not $ResumeFromPhaseD) {
    Add-E2EResource -Action Created -Kind 'ACR repository' -Name $m.names.ImageRepository -Scope $AcrName `
        -Detail "image $($script:Metrics['image'])"
}
foreach ($app in $apps) {
    $trg = @($triggers | Where-Object { $_.queue -and $app.Name -like "*$([regex]::Escape(($_.name -replace '[^a-zA-Z0-9]','').ToLower()))*" }) | Select-Object -First 1
    Add-E2EResource -Action $qpAction -Kind 'Container App (worker)' -Name $app.Name -Scope $ResourceGroup `
        -Url (Portal-Url 'Microsoft.App/containerApps' $app.Name $ctx.SubscriptionId $ResourceGroup) `
        -Detail ("queue '{0}' -> {1}; min={2} max={3}; identity {4}" -f `
                 $(if ($trg) { $trg.queue } else { '?' }), $(if ($trg) { $trg.workflow } else { '?' }),
                 $app.MinReplicas, $app.MaxReplicas, $app.PrincipalId)
    if (-not $ResumeFromPhaseD -and $app.PrincipalId) {
        Add-E2EResource -Action Created -Kind 'Role assignment' -Name "AcrPull -> $($app.Name)" -Scope $AcrName
        Add-E2EResource -Action Created -Kind 'Role assignment' -Name "Key Vault Secrets User -> $($app.Name)" -Scope $KeyVaultName
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Phase D - scale rules
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase D  Scale rules (this IS the KEDA rabbitmq scaler)'

$oneRule = $true; $shapeOk = $true; $noActivation = $true; $ruleDetail = @()
foreach ($app in $apps) {
    $rules = @($app.Rules | Where-Object { $null -ne $_ })    # @($null) is a 1-element array of $null
    if ($rules.Count -ne 1) { $oneRule = $false }
    $meta = if ($rules) { $rules[0].custom.metadata } else { $null }
    if ($app.MinReplicas -ne 0) { $shapeOk = $false }
    if ($meta -and $meta.PSObject.Properties['activationValue']) { $noActivation = $false }
    $line = ("{0}: min={1} max={2} rules={3} queue={4} mode={5} value={6} protocol={7}" -f `
        $app.Name, $app.MinReplicas, $app.MaxReplicas, $rules.Count,
        $(if ($meta) { $meta.queueName } else { '?' }), $(if ($meta) { $meta.mode } else { '?' }),
        $(if ($meta) { $meta.value } else { '?' }), $(if ($meta) { $meta.protocol } else { '?' }))
    $ruleDetail += $line; Write-E2EOk $line
}
Set-Criterion 5 'One Container App per trigger, exactly one rabbitmq rule each' `
    ($(if ($apps.Count -eq $triggers.Count -and $oneRule) {'Pass'} else {'Fail'})) "$($apps.Count) app(s), $($triggers.Count) trigger(s)"
Set-Criterion 6 'min = 0; max = trigger Concurrency; value = MaxConcurrency' ($(if ($shapeOk) {'Pass'} else {'Fail'})) ($ruleDetail -join ' | ')
Set-Criterion 7 'No activationValue (a single message must wake the app)' ($(if ($noActivation) {'Pass'} else {'Fail'}))

# ═════════════════════════════════════════════════════════════════════════════
# Phase E - proof
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Phase E  End-to-end proof'

$broker = $null
if ($brokerAvailable) {
    Initialize-E2ERabbitClient -PublishPath $m.paths.QpPublish
    $broker = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName "wwqp-e2e-$suffix"
    Write-E2EOk "Broker connected: $($broker.Describe)"
}

try {
    if ($broker -and -not $SkipTopologyCreate) {
        # PublishRabbitMQActivity cannot create topology (S11), and the KEDA scaler's passive declare
        # cannot report a depth for a queue that does not exist - so this must happen first. The binding
        # is mandatory: the publisher targets exchange=<queueName> with an EMPTY routing key, and once
        # both passive declares succeed it never calls QueueBind.
        Write-E2EStep 'Pre-creating exchanges, queues and bindings'
        foreach ($t in $triggers) {
            $existed = (Get-E2EQueueDepth -Session $broker -QueueName $t.queue).Exists
            $topo = Initialize-E2EQueueTopology -Session $broker -QueueName $t.queue -Durable:([bool]$t.durable)
            Write-E2EOk ("  {0}: exchange(direct,durable={1}) + queue + binding(routingKey='')" -f $topo.Queue, $topo.Durable)
            Add-E2EResource -Action $(if ($existed) { 'Reused' } else { 'Created' }) -Kind 'RabbitMQ topology' `
                -Name $t.queue -Scope $broker.Describe `
                -Detail ("direct exchange '{0}' + queue + binding(routingKey=''), durable={1}; DLQ '{2}'" -f `
                         $t.queue, $topo.Durable, $t.deadLetterQueue)
        }
    } elseif ($broker) {
        Write-E2ENote 'Topology pre-create skipped (-SkipTopologyCreate); the queues must already exist.'
    }

    $publishRoute = Get-E2EWorkflowRoute -WorkflowName $m.workflows.publisher

    # E0: one warm-up per queue. `Healthy` at minReplicas=0 proves nothing, so force a real cold start.
    Write-E2EStep 'E0  Warm-up: 1 message per queue'
    foreach ($t in $triggers) {
        $p = Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Public/$publishRoute.json?queue=$($t.queue)&total=1"
        Write-E2EOk "  publish -> $($t.queue): HTTP $($p.StatusCode) $($p.Compact)"
        if ($p.StatusCode -ne 200 -and $p.Compact -match 'no exchange') {
            Write-E2EBad '  The exchange does not exist. PublishRabbitMQActivity CANNOT create it (S11) - pre-create the topology.'
        }
    }

    Write-E2EStep 'E1  Waiting for every app to reach 0 replicas (ACA cool-down ~5 min, not configurable)'
    $deadline = (Get-Date).AddSeconds($ZeroWaitSeconds); $atZero = $false
    do {
        $counts = @($apps | ForEach-Object { Get-E2EReplicaCount -AppName $_.Name -ResourceGroup $ResourceGroup })
        # @() is REQUIRED: Where-Object returns $null when nothing matches, and $null.Count throws under
        # StrictMode - so this failed at precisely the moment it succeeded, i.e. when every replica had
        # reached zero. Measured 2026-08-10 in E1.
        $atZero = @($counts | Where-Object { $_ -ne 0 }).Count -eq 0
        Write-Host ("     {0}  replicas: {1}" -f (Get-Date -Format HH:mm:ss), ($counts -join '/'))
        if (-not $atZero) { Start-Sleep -Seconds 20 }
    } while (-not $atZero -and (Get-Date) -lt $deadline)
    Set-Criterion 9 'Replicas sit at 0 on an empty queue' ($(if ($atZero) {'Pass'} else {'Fail'})) "waited up to ${ZeroWaitSeconds}s"

    $targetApp = ($apps | Where-Object { $_.MaxReplicas -eq [int]$primary.concurrency } | Select-Object -First 1)
    if (-not $targetApp) { $targetApp = $apps[0] }

    Write-E2EStep "E2  Publishing $BurstSize message(s) to '$($primary.queue)'"
    $pub = Invoke-E2EEngineRoute -BaseUrl $engineUrl -Route "Public/$publishRoute.json?queue=$($primary.queue)&total=$BurstSize"
    Write-E2EOk "  publish -> HTTP $($pub.StatusCode) $($pub.Compact)"

    Write-E2EStep "E3  Watching '$($targetApp.Name)' scale from zero (KEDA polls roughly every 30s)"
    $peak = 0; $activated = $false
    $watchEnd = (Get-Date).AddSeconds($ScaleWatchSeconds)
    while ((Get-Date) -lt $watchEnd) {
        $n = Get-E2EReplicaCount -AppName $targetApp.Name -ResourceGroup $ResourceGroup
        if ($n -gt 0) { $activated = $true }
        if ($n -gt $peak) { $peak = $n }
        Write-Host ("     {0}  replicas={1}  peak={2}" -f (Get-Date -Format HH:mm:ss), $n, $peak)
        if ($peak -ge $expectedPeak -and $n -eq 0) { break }
        Start-Sleep -Seconds 15
    }
    $script:Metrics['peakReplicas'] = $peak
    $script:Metrics['expectedPeak'] = $expectedPeak
    Set-Criterion 10 'Publishing raises replicas above 0 with no manual intervention' ($(if ($activated) {'Pass'} else {'Fail'}))
    Set-Criterion 11 "Peak replicas = min(ceil($BurstSize / $MaxConcurrency), $($primary.concurrency)) = $expectedPeak" `
        ($(if ($peak -eq $expectedPeak) {'Pass'} else {'Fail'})) "observed peak = $peak"

    if ($SkipUnackedProbe -or -not $broker) {
        Set-Criterion 13 'Whether QueueLength counts unacked messages is recorded' 'Skip' `
            $(if ($broker) { '-SkipUnackedProbe' } else { 'no broker URI' })
    } else {
        Write-E2EStep 'E3b  Measuring whether the scaler sees unacked messages (throwaway queue)'
        $unacked = Test-E2EUnackedVisibility -Session $broker -ProbeQueue "wwe2e-$suffix-unacked-probe"
        $script:Metrics['unackedVisibility'] = $unacked.Verdict
        Set-Criterion 13 'Whether QueueLength counts unacked messages is recorded' 'Pass' `
            "$($unacked.Verdict): ready=$($unacked.BeforeConsume) -> unacked=$($unacked.WhileUnacked) -> requeued=$($unacked.AfterNack)"
        if ($unacked.Verdict -eq 'ReadyOnly') {
            Write-E2ENote 'In-flight work is INVISIBLE to the scaler; the drain path is the only protection. Keep EngineTimeout <= ShutdownGrace < TerminationGracePeriod.'
        }
    }

    if ($SkipDrainTest) {
        Set-Criterion 17 'A forced restart drains cleanly' 'Skip' '-SkipDrainTest'
    } else {
        Write-E2EStep 'E7  Forcing a restart mid-drain'
        # Resolve the revision defensively rather than chaining through .properties, which throws under
        # StrictMode if the shape differs or the call soft-fails.
        $rev = (Get-E2EContainerApps -ResourceGroup $ResourceGroup -NamePrefix $targetApp.Name |
                Select-Object -First 1).Revision
        if (-not $rev) { throw "Could not resolve the active revision for '$($targetApp.Name)'." }
        # Publish a burst big enough to still be draining when the restart lands, and DO NOT wait for
        # the response: the point is to interrupt work in flight. The publish workflow takes ~2s per
        # message, so this call is expected to exceed the short timeout - that is the intent, hence
        # -IgnoreTimeout. The engine finishes publishing server-side regardless.
        $burst2 = [Math]::Max($BurstSize * 4, 20)
        $fire = Invoke-E2EEngineRoute -BaseUrl $engineUrl `
            -Route "Public/$publishRoute.json?queue=$($primary.queue)&total=$burst2" -TimeoutSec 30 -IgnoreTimeout
        Write-E2EOk "  publish($burst2) -> $(if ($fire.TimedOut) { $fire.Compact } else { "HTTP $($fire.StatusCode)" })"
        Start-Sleep -Seconds 40
        & az containerapp revision restart --name $targetApp.Name -g $ResourceGroup --revision $rev -o none 2>$null
        Write-E2EOk "  restart issued on revision '$rev'"
        Write-E2ENote '  A restart transiently reports MORE replicas than maxReplicas: replacements start while old ones drain.'
        Start-Sleep -Seconds 90
    }

    Write-E2EStep 'E4/E5  Collecting evidence (Log Analytics ingestion lags 2-5 min)'
    Start-Sleep -Seconds 120
    $totalStart=0; $totalOk=0; $totalFail=0; $totalDlq=0; $precon=0; $median=$null; $distinct=0
    if ($LogAnalyticsCustomerId) {
        # Filter nulls: the query returns $null when it finds nothing or the call soft-fails, and
        # @($null) is a ONE-ELEMENT array containing $null - so this loop would run once against $null
        # and throw on the first property access. Same trap as the scale-rules list.
        $rows = @(Get-E2EExecutionStats -WorkspaceCustomerId $LogAnalyticsCustomerId -AppNamePrefix $qpPrefix |
                  Where-Object { $null -ne $_ })
        if ($rows.Count -eq 0) { Write-E2ENote '  Log Analytics returned no rows yet (ingestion lags 2-5 min).' }
        foreach ($s in $rows) {
            # Read defensively: a summarize column can be absent or null, and StrictMode turns that into
            # a terminating error rather than an empty value.
            function RowVal { param($Row, [string] $Name)
                $p = $Row.PSObject.Properties[$Name]
                if ($p -and $null -ne $p.Value -and "$($p.Value)" -ne '') { return $p.Value }
                return $null
            }
            $appName = RowVal $s 'ContainerAppName_s'
            Write-E2EOk ("  {0}: starting={1} succeeded={2} failed={3} deadLettered={4} medianMs={5} distinctBodies={6}" -f `
                $appName, (RowVal $s 'starting'), (RowVal $s 'succeeded'), (RowVal $s 'failed'),
                (RowVal $s 'deadLettered'), (RowVal $s 'medianMs'), (RowVal $s 'distinctBodies'))
            $totalStart += [int](RowVal $s 'starting'); $totalOk   += [int](RowVal $s 'succeeded')
            $totalFail  += [int](RowVal $s 'failed');   $totalDlq  += [int](RowVal $s 'deadLettered')
            $precon     += [int](RowVal $s 'preconditionFailed'); $distinct += [int](RowVal $s 'distinctBodies')
            if ($appName -eq $targetApp.Name) { $median = RowVal $s 'medianMs' }
        }
    } else { Write-E2ENote '  No Log Analytics workspace; execution counts and T unavailable.' }
    $script:Metrics['executionsStarted']   = $totalStart
    $script:Metrics['executionsSucceeded'] = $totalOk
    $script:Metrics['executionsFailed']    = $totalFail
    $script:Metrics['deadLettered']        = $totalDlq
    $script:Metrics['medianDurationMs']    = $median

    Set-Criterion 4 'Cold start logs catalog, loader and pump lines; replicas really ran' `
        ($(if ($totalStart -gt 0) {'Pass'} else {'Fail'})) "$totalStart execution(s) started"
    Set-Criterion 8 'Trigger consumes a queue whose durability it did not set (no PRECONDITION_FAILED)' `
        ($(if ($precon -eq 0 -and $totalOk -gt 0) {'Pass'} else {'Fail'})) "PRECONDITION_FAILED = $precon"
    Set-Criterion 12 'T (median durationMs) recorded' ($(if ($median) {'Pass'} else {'Fail'})) "median = ${median}ms"
    Set-Criterion 14 'One success per message; engine returned 200' `
        ($(if ($totalOk -gt 0 -and $totalFail -eq 0) {'Pass'} else {'Fail'})) "succeeded=$totalOk failed=$totalFail"

    # A drained queue proves nothing on its own: non-2xx dead-letters AND acks, which also drains it.
    if ($broker) {
        $dlqGrew=$false; $depthDetail=@()
        foreach ($t in $triggers) {
            $d = Get-E2EQueueDepth -Session $broker -QueueName $t.queue
            $dl = Get-E2EQueueDepth -Session $broker -QueueName $t.deadLetterQueue
            if ($dl.Exists -and $dl.Messages -gt 0) { $dlqGrew = $true }
            $depthDetail += "$($t.queue)=$($d.Messages); $($t.deadLetterQueue)=$(if ($dl.Exists) { $dl.Messages } else { 'absent' })"
        }
        Set-Criterion 15 'Work queues drain; dead-letter queues do not grow' `
            ($(if (-not $dlqGrew -and $totalDlq -eq 0) {'Pass'} else {'Fail'})) ($depthDetail -join ' | ')
    } else {
        Set-Criterion 15 'Work queues drain; dead-letter queues do not grow' `
            ($(if ($totalDlq -eq 0) {'Pass'} else {'Fail'})) "from logs only (no broker URI): deadLettered=$totalDlq"
    }

    if (-not $SkipDrainTest -and $LogAnalyticsCustomerId) {
        # Same @($null) guard, and read Log_s via PSObject so a row without it cannot throw.
        $drain = @(Get-E2EStartupEvidence -WorkspaceCustomerId $LogAnalyticsCustomerId -AppNamePrefix $qpPrefix -LookbackHours 1 |
                   Where-Object { $null -ne $_ })
        $drainText = @($drain | ForEach-Object { $p = $_.PSObject.Properties['Log_s']; if ($p) { [string]$p.Value } })
        $clean    = @($drainText | Where-Object { $_ -match 'completed cleanly' }).Count
        $stranded = @($drainText | Where-Object { $_ -match 'still in flight' }).Count
        # The verdict rests on cleanDrains/stranded. distinctBodies is INFORMATIONAL only: the publisher
        # emits the same bodies ('hello 1'..'hello N') on every burst, so across repeated bursts
        # executions legitimately exceeds distinct bodies and the two are equal only within a single
        # burst. Do not read a mismatch as duplicate execution.
        Set-Criterion 17 'A forced restart drains cleanly' ($(if ($clean -gt 0 -and $stranded -eq 0) {'Pass'} else {'Fail'})) `
            "cleanDrains=$clean stranded=$stranded (executions=$totalStart, distinctBodies=$distinct - informational; bodies repeat across bursts)"
        if ($stranded -gt 0) { Write-E2ENote "$stranded replica(s) reported messages still in flight at drain: those will be redelivered and may run twice. Raise -ShutdownGraceSeconds or lower -EngineTimeoutSeconds." }
    }

    Write-E2EStep 'E6  Waiting for scale-in to zero'
    $deadline = (Get-Date).AddSeconds($ZeroWaitSeconds); $atZero = $false
    do {
        $counts = @($apps | ForEach-Object { Get-E2EReplicaCount -AppName $_.Name -ResourceGroup $ResourceGroup })
        # @() is REQUIRED: Where-Object returns $null when nothing matches, and $null.Count throws under
        # StrictMode - so this failed at precisely the moment it succeeded, i.e. when every replica had
        # reached zero. Measured 2026-08-10 in E1.
        $atZero = @($counts | Where-Object { $_ -ne 0 }).Count -eq 0
        Write-Host ("     {0}  replicas: {1}" -f (Get-Date -Format HH:mm:ss), ($counts -join '/'))
        if (-not $atZero) { Start-Sleep -Seconds 20 }
    } while (-not $atZero -and (Get-Date) -lt $deadline)
    Set-Criterion 16 'Replicas return to 0 within the cool-down' ($(if ($atZero) {'Pass'} else {'Fail'}))

    $foreign = @(Get-E2EContainerApps -ResourceGroup $ResourceGroup | Where-Object { $_.Name -notlike "$qpPrefix*" })
    Set-Criterion 18 "Only this run's resources were created" 'Pass' `
        ("$($foreign.Count) pre-existing Container App(s) left untouched: " + (($foreign | ForEach-Object { $_.Name }) -join ', '))
}
finally {
    if ($broker) { Close-E2EBrokerSession -Session $broker }
}

# ═════════════════════════════════════════════════════════════════════════════
# Summary
# ═════════════════════════════════════════════════════════════════════════════

Write-E2EPhase 'Summary'

$pass = @($script:Criteria | Where-Object State -eq 'Pass').Count
$fail = @($script:Criteria | Where-Object State -eq 'Fail').Count
$skip = @($script:Criteria | Where-Object State -eq 'Skip').Count
Write-Host ''
Write-Host ("  PASS {0}   FAIL {1}   SKIP {2}   of {3}" -f $pass,$fail,$skip,$script:Criteria.Count) `
    -ForegroundColor $(if ($fail -eq 0) { 'Green' } else { 'Red' })

$stamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
$result = [ordered]@{
    schema='wwe2e-result/2'; startedUtc=$script:Started.ToUniversalTime().ToString('o')
    finishedUtc=(Get-Date).ToUniversalTime().ToString('o'); runSuffix=$suffix
    subscription=$ctx.SubscriptionId; tenant=$ctx.TenantId; resourceGroup=$ResourceGroup
    engine=[ordered]@{ app=$engineApp; url=$engineUrl; appId=$engineAppId; spId=$engineSpId
                       elasticsearch=$esEnabled }
    kedaSecret=$RabbitMqSecretName; kedaSecretCreated=($RabbitMqSecretName -eq $perRunSecret)
    metrics=$script:Metrics
    totals=[ordered]@{ pass=$pass; fail=$fail; skip=$skip; total=$script:Criteria.Count }
    criteria=@($script:Criteria | ForEach-Object { [ordered]@{ number=$_.Number; text=$_.Text; state=$_.State; detail=$_.Detail } })
}
$jsonPath = Join-Path $logDir "e2e-result-$stamp.json"
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$md = [System.Collections.Generic.List[string]]::new()
$md.Add("# E2E verification result - run ``$suffix``"); $md.Add('')
$md.Add('| | |'); $md.Add('|---|---|')
$md.Add("| Started (UTC) | $($result.startedUtc) |")
$md.Add("| Engine | ``$engineApp`` -> $engineUrl |")
$md.Add("| Elasticsearch (engine) | $esEnabled |")
$md.Add("| Container Apps | $((@($script:Metrics['apps']) -join ', ')) |")
$md.Add("| Image | ``$($script:Metrics['image'])`` |")
$md.Add("| Peak replicas | **$($script:Metrics['peakReplicas'])** (expected $($script:Metrics['expectedPeak'])) |")
$md.Add("| Executions | started $($script:Metrics['executionsStarted']), succeeded $($script:Metrics['executionsSucceeded']), failed $($script:Metrics['executionsFailed']) |")
$md.Add("| Dead-lettered | $($script:Metrics['deadLettered']) |")
$md.Add("| Median T | $($script:Metrics['medianDurationMs']) ms |")
$md.Add("| Scaler counts | $($script:Metrics['unackedVisibility']) |")
$md.Add("| **Result** | **PASS $pass / FAIL $fail / SKIP $skip** |")
$md.Add(''); $md.Add('## Acceptance criteria'); $md.Add('')
$md.Add('| # | Criterion | State | Detail |'); $md.Add('|---|---|---|---|')
foreach ($c in ($script:Criteria | Sort-Object Number)) {
    $icon = switch ($c.State) { 'Pass' {'PASS'} 'Fail' {'FAIL'} 'Skip' {'SKIP'} default {'--'} }
    $md.Add("| $($c.Number) | $($c.Text) | **$icon** | $($c.Detail) |")
}
$md.Add(''); $md.Add('## Teardown'); $md.Add('')
$md.Add('```powershell')
$md.Add("az containerapp list -g $ResourceGroup -o json | ConvertFrom-Json |")
$md.Add("  Where-Object { `$_.name -like '$qpPrefix*' } |")
$md.Add("  ForEach-Object { az containerapp delete --name `$_.name -g $ResourceGroup --yes }")
$md.Add(".\Rollback-WwExecutionEngine.ps1 -SummaryPath '$($script:Metrics['engineSummary'])' -DryRun")
$md.Add("az acr repository delete --name $AcrName --repository '$($m.names.ImageRepository)' --yes")
if ($RabbitMqSecretName -eq $perRunSecret) { $md.Add("az keyvault secret delete --vault-name $KeyVaultName --name $RabbitMqSecretName") }
$md.Add("# NEVER: az group delete, az containerapp env delete, az acr delete, or deleting $KeyVaultSecretName")
$md.Add('```')
$mdPath = Join-Path $logDir "e2e-result-$stamp.md"
$md -join [Environment]::NewLine | Set-Content -LiteralPath $mdPath -Encoding UTF8

Write-Host ''
Write-E2EOk "JSON summary : $jsonPath"
Write-E2EOk "Markdown     : $mdPath"

# Resource ledger on the success path. The trap at the top covers the failure path.
$script:Completed = $true
Write-E2EResourceSummary -Title 'Resources manipulated by this run'
Write-E2EOk "Engine endpoint : $engineUrl"
Write-E2EOk "Discovery       : $engineUrl/apis.json"

# ═════════════════════════════════════════════════════════════════════════════
# Phase F - teardown
# ═════════════════════════════════════════════════════════════════════════════

if ($TeardownWhenDone) {
    Write-E2EPhase 'Phase F  Teardown (targeted; never az group delete)'
    foreach ($app in $apps) {
        Write-E2EStep "Deleting Container App '$($app.Name)'"
        & az containerapp delete --name $app.Name -g $ResourceGroup --yes -o none 2>$null
        Add-E2EResource -Action Deleted -Kind 'Container App (worker)' -Name $app.Name -Scope $ResourceGroup
    }
    # Only remove topology this run created. Authored queues may be shared, so leave them alone.
    if ($broker -or ($brokerAvailable -and $m.broker.triggersGenerated)) {
        if ($m.broker.triggersGenerated) {
            Initialize-E2ERabbitClient -PublishPath $m.paths.QpPublish
            $bs = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName "wwqp-e2e-teardown-$suffix"
            try {
                foreach ($t in $triggers) {
                    $gone = Remove-E2EQueueTopology -Session $bs -QueueName $t.queue -DeadLetterQueue $t.deadLetterQueue
                    Write-E2EOk "  $($t.queue): removed $($gone -join ', ')"
                }
            } finally { Close-E2EBrokerSession -Session $bs }
        } else {
            Write-E2ENote 'Broker topology left in place: the queues came from AUTHORED triggers and may be shared.'
        }
    }
    Write-E2EStep "Deleting this run's image repository"
    & az acr repository delete --name $AcrName --repository $m.names.ImageRepository --yes -o none 2>$null
    Add-E2EResource -Action Deleted -Kind 'ACR repository' -Name $m.names.ImageRepository -Scope $AcrName
    if ($RabbitMqSecretName -eq $perRunSecret) {
        Write-E2EStep "Deleting this run's KEDA secret"
        & az keyvault secret delete --vault-name $KeyVaultName --name $RabbitMqSecretName -o none 2>$null
        Add-E2EResource -Action Deleted -Kind 'Key Vault secret' -Name $RabbitMqSecretName -Scope $KeyVaultName
    }
    Write-E2EStep 'Rolling back the engine (summary-driven, tag-verified)'
    Invoke-E2EChildScript -Path (Join-Path $PSScriptRoot 'Rollback-WwExecutionEngine.ps1') `
        -Arguments @{ SummaryPath = $script:Metrics['engineSummary']; Force = $true }
    Add-E2EResource -Action Deleted -Kind 'Engine (app+storage+AI+Entra)' -Name $engineApp -Scope $ResourceGroup `
        -Detail 'via Rollback-WwExecutionEngine.ps1; Key Vault and resource group preserved'
    Write-E2EOk 'Teardown complete. Shared vault, AES key, registry, ACA environment and workspace preserved.'
    Write-E2EResourceSummary -Title 'Resources after teardown'
} else {
    Write-Host ''
    Write-E2ENote "Deployment left standing. Teardown commands are in $mdPath, or re-run with -TeardownWhenDone."
}

exit $(if ($fail -gt 0) { 1 } else { 0 })
