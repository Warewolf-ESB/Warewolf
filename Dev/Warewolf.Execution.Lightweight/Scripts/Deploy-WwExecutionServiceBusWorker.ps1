#Requires -Version 7.0
<#
.SYNOPSIS
    Deploys the Warewolf Execution Service Bus worker — the dedicated Azure Function
    App (WwExecutionServiceBusWorker, published from
    Warewolf.Execution.ServiceBusWorker) that is triggered by
    an Azure Service Bus queue message and calls the Execution Engine's /secure or
    /public route on the message's behalf. Also provisions the Service Bus namespace,
    queue and dead-letter/authorization settings this worker (and the "shovel bridge"
    RabbitMQ producer feeding it) require.

.DESCRIPTION
    A self-contained orchestrator that mirrors Deploy-WwJobProcessor.ps1's conventions
    (Write-Phase helpers, Invoke-Az wrapper, masked summary + transcript, "params first,
    prompt if missing", -DryRun) but is scoped to the Service Bus worker:

      * NO persistence settings / Key Vault / DbSource encryption (the worker holds no
        secrets of its own — it authenticates OUTBOUND to the engine via Managed Identity,
        exactly like the AzureFunction/JobProcessor daemons, and inbound Service Bus
        listen auth is Managed Identity by default too).
      * PROVISIONS the Service Bus namespace + queue (with dead-lettering and a
        configurable max delivery count) if they do not already exist.
      * Grants the worker's system-assigned managed identity the "Azure Service Bus
        Data Receiver" RBAC role on the queue, and wires the identity-based trigger
        connection (ServiceBusConnection__fullyQualifiedNamespace +
        ServiceBusConnection__credential=managedidentity) — no SAS secret needed for
        the Function App itself.
      * OPTIONALLY creates a Send-only SAS authorization rule scoped to the queue
        (default name: shovel-send) and prints (masked) its primary connection string —
        this is the credential a RabbitMQ Shovel plugin uses as its AMQP 1.0 destination
        (see docs/ShovelBridge-Architecture.md). The worker itself never needs this SAS
        rule; it exists purely for the RabbitMQ-side producer.
      * Applies the worker's app settings (WwExecution:* — see WwExecutionOptions.cs)
        so it can call the engine.

    ROLE REGISTRATION is a SEPARATE operator step (mirrors the engine/JobProcessor
    pattern): grant the worker's system-assigned managed identity the engine app role
    Warewolf_ClientApps with
        Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity `
            -DaemonFunctionAppName <thisApp> -DaemonFunctionAppResourceGroup <rg> `
            -AppRolesToAssign Warewolf_ClientApps
    and add the matching secure.config Execute row for the workflow(s) the worker calls
    (or rely on the Public group when using the "public" message route). See
    docs/KB-ClientApps-Configuration.md section 2.6.

    There is deliberately NO companion Rollback-WwExecutionServiceBusWorker.ps1 (same as
    the JobProcessor) — the run summary JSON records exactly what this run created for a
    manual/az-CLI teardown; see the "created" block in the summary file.

.PARAMETER SubscriptionId
    Azure subscription. Resolved from `az account show` when omitted.
.PARAMETER TenantId
    Entra tenant. Resolved from `az account show` when omitted.
.PARAMETER ResourceGroup
    REQUIRED (no default). Target resource group.
.PARAMETER Location
    REQUIRED (no default). Azure region, e.g. southafricanorth.
.PARAMETER StorageAccount
    REQUIRED (no default). Storage account backing the Function App (3-24 lowercase).
.PARAMETER AppName
    REQUIRED (no default). Globally-unique Function App name for the worker.
.PARAMETER PublishPath
    REQUIRED. Folder or .zip of the worker's Release publish output
    (dotnet publish WwExecutionServiceBusWorker.csproj -c Release -o <path>).
.PARAMETER PublishMethod
    Auto (zip-deploy, default) | Func | Zip.
.PARAMETER AppInsightsName / EnableAppInsights
    Application Insights (defaults to "<AppName>-ai"; enabled by default).
.PARAMETER ServiceBusNamespace
    REQUIRED (no default). Service Bus namespace name (3-50 chars, globally unique).
.PARAMETER ServiceBusQueueName
    Queue the worker's ServiceBusTrigger listens on. Default: wwexecution-queue.
    WOLF-8517: written into Settings/servicebustrigger.bite in the local publish/staging
    directory (Phase 5, before it is zipped and deployed), which
    WorkflowQueueTrigger.cs's [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%", ...)]
    resolves via Program.cs injecting that file's QueueName into configuration at cold start
    — no longer a plain app setting. Overriding this param still actually retargets the
    trigger, not just queue provisioning.
.PARAMETER WorkflowName
    REQUIRED (WOLF-8517). The workflow every message on -ServiceBusQueueName executes —
    written into the same Settings/servicebustrigger.bite as -ServiceBusQueueName. The
    message body no longer names the workflow, so this is now the ONLY place that selects
    it for this worker.
.PARAMETER ServiceBusSku
    Service Bus namespace SKU. Default: Standard (required for topics/RBAC parity;
    Basic also works for a single queue but cannot host authorization rules per-queue
    on some tiers — Standard is the safe default).
.PARAMETER ServiceBusMaxDeliveryCount
    Max delivery attempts before the broker dead-letters a message. Default: 10.
.PARAMETER ServiceBusLockDurationSeconds
    Peek-lock duration for the queue. Default: 300 (5 minutes — generous headroom for
    the worker's engine HTTP call + retry).
.PARAMETER ServiceBusTriggerMaxConcurrentCalls
    Max messages the worker processes concurrently. Default: 16 (matches host.json's
    committed extensions.serviceBus.maxConcurrentCalls). Applied as the
    AzureFunctionsJobHost__extensions__serviceBus__maxConcurrentCalls app setting — the
    standard Azure Functions override convention for host.json values, so it takes effect
    without editing/republishing host.json. NOTE: this override path is unconfirmed against
    a live Function App for the serviceBus extension specifically in this codebase (other
    host.json sections have confirmed prior art, see ShovelBridge-Architecture.md); the
    committed host.json value remains the source of truth if it does not take effect.
.PARAMETER ServiceBusTriggerPrefetchCount
    Messages pre-fetched per replica ahead of maxConcurrentCalls. Default: 0 (matches
    host.json). Same override mechanism/caveat as ServiceBusTriggerMaxConcurrentCalls.
.PARAMETER ServiceBusTriggerMaxAutoLockRenewalMinutes
    How long the worker keeps auto-renewing a message's peek-lock while processing it.
    Default: 5 (matches host.json's 00:05:00). Same override mechanism/caveat.
.PARAMETER ServiceBusTriggerAutoCompleteMessages
    Whether the host auto-completes a message on successful return. Default: true
    (matches host.json — WorkflowQueueTrigger relies on this rather than explicit
    message-actions completion). Same override mechanism/caveat.
.PARAMETER CreateShovelSendRule / ShovelSendRuleName
    When set (default: true), creates a Send-only queue-scoped SAS authorization rule
    for the RabbitMQ Shovel plugin's AMQP 1.0 destination credential. The connection
    string is printed masked at the end of the run; retrieve the real value with:
        az servicebus queue authorization-rule keys list --resource-group <rg> `
            --namespace-name <ns> --queue-name <queue> --name <ShovelSendRuleName> `
            --query primaryConnectionString -o tsv
.PARAMETER UseManagedIdentityForServiceBus
    Default: true. When true, the worker listens via its system-assigned managed
    identity (ServiceBusConnection__fullyQualifiedNamespace +
    ServiceBusConnection__credential=managedidentity) — no SAS secret in app settings.
    When false, a Listen-scoped SAS authorization rule is created instead and its
    connection string is applied as the plain ServiceBusConnection app setting
    (development only — prefer Managed Identity in production).
.PARAMETER TriggerPath
    Server-written queue trigger .bite (or a folder of them) naming the workflow to run and
    how to map a message onto its inputs. Staged into the deployed package under
    Settings/triggers and pointed at by the WwTrigger__Path app setting.

    REQUIRED for the shovel-bridge path. Messages forwarded by a RabbitMQ Shovel are the
    producer's own payloads and carry no {"workflow":...} envelope - the shovel forwards bytes
    verbatim and cannot add one. Without a trigger the worker runs envelope-only and
    dead-letters every such message WHILE STILL DRAINING THE QUEUE, so queue depth alone looks
    identical to success. Omit it only for a worker whose producers emit the envelope contract.
.PARAMETER TriggerFilter
    Filter applied when -TriggerPath is a folder. Default: triggers*.bite (same contract as
    Deploy-WwQueueProcessor.ps1). Ignored when -TriggerPath names a single file.
.PARAMETER TriggerId
    Selects one trigger when -TriggerPath resolves to several. Without it a multi-trigger
    folder is rejected at plan time rather than guessed at.
.PARAMETER TriggerRoute
    Engine route for trigger-driven executions: secure (default) or public.
.PARAMETER StripTriggerCredentials
    Rewrites the STAGED copy of a plaintext trigger with UserName/Password nulled. This worker
    never uses a trigger's broker credentials - it consumes from Service Bus, not RabbitMQ - so
    shipping them into the Function App is avoidable exposure. OPT-IN, because it re-serializes
    the file and so no longer matches what the Server wrote byte-for-byte; the plan warns when
    credentials are present and this is off. Silently skipped for encrypted triggers.
.PARAMETER WwExecutionBaseUrl
    REQUIRED. Execution Engine base URL, e.g. https://WWExecutionEngine.azurewebsites.net.
.PARAMETER WwExecutionTenantId / WwExecutionResourceAppId / WwExecutionScope
    Entra tenant + the engine's app-registration id (drives token audience/scope).
    WwExecutionTenantId defaults to -TenantId when omitted.
.PARAMETER WwExecutionFunctionKey
    Optional x-functions-key for /services/* routes (blank unless the worker calls that route).
.PARAMETER WwExecutionManagedIdentityClientId
    Optional user-assigned MI client id for calling the engine. Leave blank for
    system-assigned (recommended default).
.PARAMETER LogDir / NonInteractive / DryRun / LoadFunctionsOnly
    Logging output dir; unattended mode; preview-without-change; test hook.

.NOTES
    Publish first (this script does NOT build):
      dotnet publish Dev/Warewolf.Execution.ServiceBusWorker/Warewolf.Execution.ServiceBusWorker.csproj -c Release -o D:\SbWorker\Publish
#>
[CmdletBinding()]
param(
    # Targeting (REQUIRED — no defaults; prompted when interactive)
    [string] $SubscriptionId,
    [string] $TenantId,
    [string] $ResourceGroup,
    [string] $Location,
    [string] $StorageAccount,
    [string] $AppName,
    # Publish source (folder or .zip)
    [string] $PublishPath,
    [ValidateSet('Auto','Func','Zip')] [string] $PublishMethod = 'Auto',
    # Application Insights
    [string] $AppInsightsName,
    [nullable[bool]] $EnableAppInsights,
    # Service Bus provisioning
    [string] $ServiceBusNamespace,
    [string] $ServiceBusQueueName = 'wwexecution-queue',
    [string] $WorkflowName,
    [ValidateSet('Basic','Standard','Premium')] [string] $ServiceBusSku = 'Standard',
    [nullable[int]] $ServiceBusMaxDeliveryCount,
    [nullable[int]] $ServiceBusLockDurationSeconds,
    # Service Bus trigger binding options (host.json extensions.serviceBus overrides)
    [nullable[int]] $ServiceBusTriggerMaxConcurrentCalls,
    [nullable[int]] $ServiceBusTriggerPrefetchCount,
    [nullable[int]] $ServiceBusTriggerMaxAutoLockRenewalMinutes,
    [nullable[bool]] $ServiceBusTriggerAutoCompleteMessages,
    [nullable[bool]] $CreateShovelSendRule,
    [string] $ShovelSendRuleName = 'shovel-send',
    [nullable[bool]] $UseManagedIdentityForServiceBus,
    # WwExecution (engine caller) settings
    [string] $WwExecutionBaseUrl,
    [string] $WwExecutionTenantId,
    [string] $WwExecutionResourceAppId,
    [string] $WwExecutionScope,
    [string] $WwExecutionFunctionKey,
    [string] $WwExecutionManagedIdentityClientId,
    # Queue trigger — supplies the workflow + input mapping for messages with no envelope
    [string] $TriggerPath,
    [string] $TriggerFilter = 'triggers*.bite',
    [string] $TriggerId,
    [ValidateSet('secure', 'public')] [string] $TriggerRoute = 'secure',
    [switch] $StripTriggerCredentials,
    # Accepted for caller compatibility only - IGNORED (warns). See 'Feature toggles' below.
    [nullable[bool]] $EnableConsoleLogging,
    # Logging output / control
    [string] $LogDir,
    [switch] $NonInteractive,
    [switch] $DryRun,
    # OPT-IN. A new app is created on the correct runtime; one that ALREADY exists keeps
    # whatever it was created with, and changing it restarts the app.
    [switch] $UpgradeRuntimeVersion,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# .NET runtime the Function App must run on
# ════════════════════════════════════════════════════════════════════════════
# MUST track <TargetFramework> in Warewolf.Execution.ServiceBusWorker.csproj (net10.0). The host does not
# adapt to the payload: a v8.0 host loading a net10.0 worker fails at startup with "You
# must install or update .NET to run this application." before a single function is
# indexed. Deploy-WwExecutionEngine.ps1 carries the same constants — all these projects
# target net10.0, so they move together.
$script:TargetFunctionsRuntime    = 'dotnet-isolated'
$script:TargetRuntimeVersion      = '10'      # `functionapp create --runtime-version`
$script:TargetNetFrameworkVersion = 'v10.0'   # `siteConfig.netFrameworkVersion` on an existing app
$script:TargetFunctionsVersion    = '4'       # `functionapp create --functions-version`

# Resolved for real in the plan phase. Seeded here so Save-DeploySummary is StrictMode-safe
# no matter how early a run dies.
$script:RuntimePlan = [pscustomobject]@{
    Current = $null; Target = $script:TargetNetFrameworkVersion; Action = 'create'; Message = '(not yet resolved)'
}

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (kept byte-for-byte consistent with Deploy-WwJobProcessor.ps1)
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
    param([string] $Message, [bool] $DefaultYes = $true)
    if ($NonInteractive) { return $DefaultYes }
    $suffix = if ($DefaultYes) { '[Y/n]' } else { '[y/N]' }
    $answer = Read-Host "  $Message $suffix"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $DefaultYes }
    return $answer -imatch '^y'
}

function Read-Required {
    <#
        Return $Current if set; otherwise prompt (interactive) or throw
        (non-interactive) for a value the pipeline cannot proceed without.
    #>
    param([string] $Name, [string] $Current, [string] $Hint)
    if (-not [string]::IsNullOrWhiteSpace($Current)) { return $Current }
    if ($NonInteractive) {
        throw "Required value '$Name' was not supplied. Pass -$Name <value> (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText"
    } while ([string]::IsNullOrWhiteSpace($v))
    return $v.Trim()
}

function Resolve-Toggle {
    param([string] $Name, [nullable[bool]] $Current, [bool] $Default, [string] $Prompt)
    if ($null -ne $Current) { return [bool]$Current }
    if ($NonInteractive)    { return $Default }
    $msg = if ($Prompt) { $Prompt } else { "Enable $Name" }
    return (Confirm-Yes $msg $Default)
}

function Get-MaskedValue {
    param([string] $Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 6) { return '******' }
    return ('{0}…(masked, len={1})' -f $Value.Substring(0, 3), $Value.Length)
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
        Thin wrapper over the az CLI.
          -Mutating  : changes cloud state; skipped (echoed) under -DryRun.
          -AllowFail : non-zero exit returns $null instead of throwing (probes).
        Logged/echoed args are secret-redacted via Format-AzArgsForLog.
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
    # Merging stderr into the output stream via 2>&1 does NOT stop it being promoted
    # to a terminating NativeCommandError while the script-level
    # $ErrorActionPreference is 'Stop' - Windows PowerShell 5.1 converts merged
    # stderr lines to ErrorRecord objects and still honours 'Stop' for them, so an
    # expected-on-first-run "EntityNotFound" from an -AllowFail probe (e.g. a
    # 'show' command used to check whether a resource already exists) would abort
    # the whole script before $LASTEXITCODE below is ever checked. Scope
    # $ErrorActionPreference to 'Continue' for just this call (function-local,
    # reverts automatically on return) so only the exit code decides success/fail.
    $ErrorActionPreference = 'Continue'
    $out = & az @Args 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFail) { return $null }
        throw "az CLI failed ($LASTEXITCODE): az $(Format-AzArgsForLog $Args)`n$($out | Out-String)"
    }
    return $out
}

function Set-StagedServiceBusTriggerConfig {
    <#
        WOLF-8517: overwrites Settings/servicebustrigger.bite in the LOCAL publish/staging
        directory — BEFORE Phase 5 zips and deploys it — with this run's queue name and
        workflow name. The value shipped in the publish output (the project's own
        Settings/servicebustrigger.bite) is a local-dev sample, not what any real deployment
        should run. Contains no secret (Service Bus auth is Managed-Identity-only), so this
        is always plain JSON, never WFAES-encrypted.
    #>
    param(
        [Parameter(Mandatory)][string] $StagingDir,
        [Parameter(Mandatory)][string] $QueueName,
        [Parameter(Mandatory)][string] $WorkflowName
    )
    $path = Join-Path $StagingDir 'Settings\servicebustrigger.bite'
    if ($DryRun) {
        Write-Host "      [DRYRUN] Write '$path': QueueName=$QueueName, WorkflowName=$WorkflowName" -ForegroundColor DarkGray
        return
    }
    $settingsDir = Split-Path $path -Parent
    if (-not (Test-Path -LiteralPath $settingsDir)) {
        New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null
    }
    $json = [ordered]@{ QueueName = $QueueName; WorkflowName = $WorkflowName } | ConvertTo-Json
    Set-Content -LiteralPath $path -Value $json -Encoding UTF8
}

function Invoke-ChildScript {
    param(
        [Parameter(Mandatory)][string] $Path,
        [hashtable] $Parameters = @{},
        [string] $Label
    )
    $name = if ($Label) { $Label } else { Split-Path $Path -Leaf }
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required sibling script not found: $Path"
    }
    if ($DryRun) {
        $rendered = ($Parameters.GetEnumerator() | ForEach-Object { "-$($_.Key)" }) -join ' '
        Write-Host "      [DRYRUN] & '$name' $rendered" -ForegroundColor DarkGray
        return
    }
    Write-Step "Invoking $name"
    & $Path @Parameters
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
        throw "$name exited with code $LASTEXITCODE."
    }
}

function Save-DeploySummary {
    <#
        Write the run summary to $summaryPath crash-safely and incrementally (atomic
        temp -> Move-Item), after every phase and from the outer catch, so a manual
        teardown always has an authoritative record of what THIS run created.
    #>
    param(
        [ValidateSet('in-progress', 'completed', 'failed')]
        [string] $Status = 'in-progress',
        [string] $ErrorMessage
    )
    if ([string]::IsNullOrWhiteSpace($summaryPath)) { return }

    $maskedSettings = [ordered]@{}
    foreach ($k in $appSettings.Keys) {
        $maskedSettings[$k] = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    }
    $aiConnMasked = if ($enableAppInsights) {
        if ([string]::IsNullOrEmpty($aiConnectionString)) { '<pending — created/read on a real run>' }
        elseif ($aiConnectionString -like '<pending*')     { $aiConnectionString }
        else                                               { Get-MaskedValue $aiConnectionString }
    } else { $null }
    $shovelConnMasked = if ($doCreateShovelRule) {
        if ([string]::IsNullOrEmpty($shovelConnectionString)) { '<pending — created/read on a real run>' } else { Get-MaskedValue $shovelConnectionString }
    } else { $null }

    $summary = [ordered]@{
        timestampUtc    = (Get-Date).ToUniversalTime().ToString('o')
        status          = $Status
        lastPhase       = $script:DeployLastPhase
        error           = $ErrorMessage
        dryRun          = [bool]$DryRun
        runId           = $runId
        resourceTags    = $ResourceTags
        created         = $created
        subscriptionId  = $SubscriptionId
        tenantId        = $TenantId
        resourceGroup   = $ResourceGroup
        location        = $Location
        storageAccount  = $StorageAccount
        appName         = $AppName
        # What this run did about the .NET runtime. An operator reading a summary after a
        # failed startup needs to see 'warn' here without re-deriving it from the log.
        runtime         = [ordered]@{
            runtime            = $script:TargetFunctionsRuntime
            runtimeVersion     = $script:TargetRuntimeVersion
            functionsVersion   = $script:TargetFunctionsVersion
            netFrameworkTarget = $script:TargetNetFrameworkVersion
            netFrameworkBefore = $script:RuntimePlan.Current
            action             = $script:RuntimePlan.Action
        }
        appInsightsName = ($enableAppInsights ? $AppInsightsName : $null)
        appInsightsConnectionString = $aiConnMasked
        endpoint        = $baseUrl
        publishPath     = $PublishPath
        publishDir      = $StagingDir
        publishMethod   = $PublishMethod
        appInsights     = $enableAppInsights
        serviceBusNamespace       = $ServiceBusNamespace
        serviceBusQueueName       = $ServiceBusQueueName
        workflowName              = $WorkflowName
        serviceBusSku             = $ServiceBusSku
        serviceBusMaxDeliveryCount = $maxDeliveryCount
        serviceBusLockDurationSeconds = $lockDurationSeconds
        serviceBusTriggerMaxConcurrentCalls = $triggerMaxConcurrentCalls
        serviceBusTriggerPrefetchCount = $triggerPrefetchCount
        serviceBusTriggerMaxAutoLockRenewalDuration = $triggerMaxAutoLockRenewalIso
        serviceBusTriggerAutoCompleteMessages = $triggerAutoCompleteMessages
        useManagedIdentityForServiceBus = $useMiForServiceBus
        shovelSendRuleCreated  = $doCreateShovelRule
        shovelSendRuleName     = ($doCreateShovelRule ? $ShovelSendRuleName : $null)
        shovelSendConnectionString = $shovelConnMasked
        appSettings     = $maskedSettings
    }
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# ════════════════════════════════════════════════════════════════════════════
# Path resolution
# ════════════════════════════════════════════════════════════════════════════

$ScriptDir         = $PSScriptRoot
$AppInsightsScript = Join-Path $ScriptDir 'Setup-ApplicationInsights.ps1'

function Resolve-FunctionRuntimePlan {
    <#
        Decide what THIS run does about the Function App's .NET runtime, from the app's
        CURRENT netFrameworkVersion. Pure — no az calls — so the decision is made once at
        plan time, printed, and then replayed in Phase 1.

        $CurrentNetFrameworkVersion is $null/empty in TWO very different situations, and
        -AppExists separates them: the app does not exist yet, or the app DOES exist but
        `functionapp show` returned a payload whose siteConfig.netFrameworkVersion could not
        be read. Conflating them made this report success on an app it had never checked.

          'create'  - no app yet; `functionapp create` pins the target runtime.
          'none'    - already on the target.
          'upgrade' - mismatch AND -UpgradeRuntimeVersion; Phase 1 runs `config set`.
          'warn'    - mismatch WITHOUT the switch; report loudly, change nothing.
          'unknown' - app exists, runtime unreadable. Report it, change nothing, and NEVER
                      claim it is on the target: `functionapp create` does not run for an
                      existing app, so an unread runtime is an unreconciled one.

        'warn' is the default because `config set` RESTARTS the app, and a deploy must not
        restart a running environment on its own inference.
    #>
    param(
        [string] $CurrentNetFrameworkVersion,
        [switch] $Upgrade,
        [switch] $AppExists,
        [string] $TargetNetFrameworkVersion = $script:TargetNetFrameworkVersion
    )
    $current = if ([string]::IsNullOrWhiteSpace($CurrentNetFrameworkVersion)) { $null }
               else { $CurrentNetFrameworkVersion.Trim() }

    $action = if (-not $current -and $AppExists)                { 'unknown' }
              elseif (-not $current)                            { 'create'  }
              elseif ($current -ieq $TargetNetFrameworkVersion) { 'none'    }
              elseif ($Upgrade)                                 { 'upgrade' }
              else                                              { 'warn'    }

    $message = switch ($action) {
        'create'  { "will be created on $script:TargetFunctionsRuntime $script:TargetRuntimeVersion ($TargetNetFrameworkVersion, Functions v$script:TargetFunctionsVersion)" }
        'none'    { "$current — already correct" }
        'upgrade' { "$current -> $TargetNetFrameworkVersion (-UpgradeRuntimeVersion; the app RESTARTS)" }
        'warn'    { "$current, but this package needs $TargetNetFrameworkVersion — NOT changed (pass -UpgradeRuntimeVersion)" }
        'unknown' { "COULD NOT BE READ; this package needs $TargetNetFrameworkVersion — verify with ``az functionapp config show``" }
    }

    [pscustomobject]@{
        Current = $current
        Target  = $TargetNetFrameworkVersion
        Action  = $action
        Message = $message
    }
}

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) not found. Install: https://aka.ms/InstallAzureCli'
}
$acct = az account show -o json 2>$null | ConvertFrom-Json
if (-not $acct) {
    throw 'Not logged in to Azure. Run: az login'
}
if (-not $SubscriptionId) { $SubscriptionId = $acct.id }
if (-not $TenantId)       { $TenantId       = $acct.tenantId }
Write-Ok "Subscription $SubscriptionId (tenant $TenantId)"

# ════════════════════════════════════════════════════════════════════════════
# Phase 0.5 — PLAN  (resolve every decision; no cloud/state change yet)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0.5  Plan (resolve all settings before any change)'

$runId        = "wwsb-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$ResourceTags = @("wwx-test-run=$runId", 'wwx-purpose=servicebusworker-shovel-bridge')
$created      = [ordered]@{}

# ── Targeting (required) ──────────────────────────────────────────────────────
$AppName        = Read-Required -Name 'AppName'        -Current $AppName        -Hint 'globally-unique Function App name'
$ResourceGroup  = Read-Required -Name 'ResourceGroup'  -Current $ResourceGroup
$Location       = Read-Required -Name 'Location'       -Current $Location        -Hint 'e.g. southafricanorth'
$StorageAccount = Read-Required -Name 'StorageAccount' -Current $StorageAccount  -Hint '3-24 lowercase chars'
if (-not $AppInsightsName) { $AppInsightsName = "$AppName-ai" }

# ── Publish source (folder or .zip) ───────────────────────────────────────────
$PublishPath = Read-Required -Name 'PublishPath' -Current $PublishPath -Hint 'folder or .zip of the Release publish output'
if (-not (Test-Path -LiteralPath $PublishPath)) {
    throw "PublishPath not found: $PublishPath"
}
$publishItem  = Get-Item -LiteralPath $PublishPath
$publishIsZip = (-not $publishItem.PSIsContainer) -and ($publishItem.Extension -ieq '.zip')
if ($publishItem.PSIsContainer) {
    $PublishDir = $publishItem.FullName
} elseif ($publishIsZip) {
    $PublishDir = Join-Path $publishItem.DirectoryName $publishItem.BaseName
} else {
    throw "PublishPath must be a folder or a .zip file: $PublishPath"
}

if (-not $LogDir) { $LogDir = Join-Path (Split-Path $PublishDir -Parent) 'deploy-logs' }

# ── Feature toggles ────────────────────────────────────────────────────────────
$enableAppInsights = Resolve-Toggle -Name 'EnableAppInsights'       -Current $EnableAppInsights       -Default $true -Prompt 'Provision + enable Application Insights?'
# No console-logging toggle: Warewolf.Execution.ServiceBusWorker never reads one. It does not use
# Warewolf.Execution.Lightweight's LoggingConfiguration, so ENABLECONSOLELOGGING /
# ENABLEAPPLICATIONINSIGHTS (and their WOLF-8516 successor WAREWOLF_LOGGING_CONFIG) were app settings
# nothing consumed, and the prompt asked the operator a question with no effect.
if ($null -ne $EnableConsoleLogging) {
    Write-Note '-EnableConsoleLogging is ignored: the Service Bus worker has no console-logging toggle (it logs through the Functions host).'
}
$doCreateShovelRule = Resolve-Toggle -Name 'CreateShovelSendRule'   -Current $CreateShovelSendRule    -Default $true -Prompt 'Create a Send-only SAS rule on the queue for the RabbitMQ Shovel plugin?'
$useMiForServiceBus = Resolve-Toggle -Name 'UseManagedIdentityForServiceBus' -Current $UseManagedIdentityForServiceBus -Default $true -Prompt 'Use Managed Identity for the worker''s Service Bus LISTEN connection (recommended)?'

# ── Service Bus provisioning inputs ─────────────────────────────────────────────
$ServiceBusNamespace = Read-Required -Name 'ServiceBusNamespace' -Current $ServiceBusNamespace -Hint 'globally-unique Service Bus namespace name'
$WorkflowName        = Read-Required -Name 'WorkflowName'        -Current $WorkflowName        -Hint 'the workflow every message on the queue runs (WOLF-8517)'
$maxDeliveryCount    = if ($null -ne $ServiceBusMaxDeliveryCount)    { [int]$ServiceBusMaxDeliveryCount }    else { 10 }
$lockDurationSeconds = if ($null -ne $ServiceBusLockDurationSeconds) { [int]$ServiceBusLockDurationSeconds } else { 300 }

# ── Service Bus trigger binding options (host.json extensions.serviceBus overrides) ────
# Defaults match host.json's committed values exactly, so a deploy with no overrides
# behaves identically to today.
$triggerMaxConcurrentCalls   = if ($null -ne $ServiceBusTriggerMaxConcurrentCalls)   { [int]$ServiceBusTriggerMaxConcurrentCalls }   else { 16 }
$triggerPrefetchCount        = if ($null -ne $ServiceBusTriggerPrefetchCount)        { [int]$ServiceBusTriggerPrefetchCount }        else { 0 }
$triggerMaxAutoLockRenewalMinutes = if ($null -ne $ServiceBusTriggerMaxAutoLockRenewalMinutes) { [int]$ServiceBusTriggerMaxAutoLockRenewalMinutes } else { 5 }
$triggerMaxAutoLockRenewalIso = [TimeSpan]::FromMinutes($triggerMaxAutoLockRenewalMinutes).ToString('hh\:mm\:ss')
$triggerAutoCompleteMessages  = if ($null -ne $ServiceBusTriggerAutoCompleteMessages) { [bool]$ServiceBusTriggerAutoCompleteMessages } else { $true }

# ── WwExecution (engine caller) settings ────────────────────────────────────────
$WwExecutionBaseUrl      = Read-Required -Name 'WwExecutionBaseUrl'      -Current $WwExecutionBaseUrl      -Hint 'Execution Engine base URL, e.g. https://<engine>.azurewebsites.net'
$WwExecutionTenantId     = if (-not [string]::IsNullOrWhiteSpace($WwExecutionTenantId))     { $WwExecutionTenantId }     else { $TenantId }
$WwExecutionResourceAppId = Read-Required -Name 'WwExecutionResourceAppId' -Current $WwExecutionResourceAppId -Hint "engine's app-registration (client) id"

# ── Build the environment-variable (app settings) map ──────────────────────────
$appSettings = [ordered]@{}
$secretSettingNames = New-Object System.Collections.Generic.HashSet[string]

$appSettings['FUNCTIONS_WORKER_RUNTIME']  = 'dotnet-isolated'

# WwExecution:* — bound to WwExecutionOptions (see WwExecutionOptions.cs); the worker
# uses double-underscore config-key nesting exactly like local.settings.json.
$appSettings['WwExecution__BaseUrl']       = $WwExecutionBaseUrl
$appSettings['WwExecution__TenantId']      = $WwExecutionTenantId
$appSettings['WwExecution__ResourceAppId'] = $WwExecutionResourceAppId
if (-not [string]::IsNullOrWhiteSpace($WwExecutionScope)) {
    $appSettings['WwExecution__Scope'] = $WwExecutionScope
}
if (-not [string]::IsNullOrWhiteSpace($WwExecutionFunctionKey)) {
    $appSettings['WwExecution__FunctionKey'] = $WwExecutionFunctionKey
    $secretSettingNames.Add('WwExecution__FunctionKey') | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($WwExecutionManagedIdentityClientId)) {
    $appSettings['WwExecution__ManagedIdentityClientId'] = $WwExecutionManagedIdentityClientId
}
# UseClientSecretFallback is intentionally NEVER set here — Managed Identity only for
# a deployed worker. Local dev's local.settings.json is where the secret fallback lives.
$appSettings['WwExecution__UseClientSecretFallback'] = 'false'

# WOLF-8517: the queue name and workflow are no longer app settings — they are written into
# Settings/servicebustrigger.bite in the local staging directory in Phase 5 (before it is
# zipped and deployed), via Set-StagedServiceBusTriggerConfig. WorkflowQueueTrigger.cs's
# [ServiceBusTrigger] attribute still binds via the %WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%
# %AppSetting% indirection, but Program.cs now resolves that key from the staged file at
# cold start rather than from an app setting.

# ── Queue trigger (WwTrigger:*) ────────────────────────────────────────────────
# Resolved HERE so the plan states the workflow that will actually run, and so the states a
# trigger can be in fail at PLAN time rather than at app startup - or, worse, silently at
# runtime, where an unusable trigger dead-letters every message while the queue still drains.
$script:TriggerFiles        = @()
$script:TriggerStagePaths   = @()
$script:TriggerHasCreds     = $false
$script:TriggerSummary      = 'not configured (envelope-only: every message must name its own workflow)'
$triggerStageFolder         = 'Settings/triggers'

if (-not [string]::IsNullOrWhiteSpace($TriggerPath)) {
    if (Test-Path -LiteralPath $TriggerPath -PathType Leaf) {
        $script:TriggerFiles = @((Resolve-Path -LiteralPath $TriggerPath).Path)
    }
    elseif (Test-Path -LiteralPath $TriggerPath -PathType Container) {
        $script:TriggerFiles = @(
            Get-ChildItem -LiteralPath $TriggerPath -Filter $TriggerFilter -File |
                Sort-Object Name | ForEach-Object { $_.FullName })
        if ($script:TriggerFiles.Count -eq 0) {
            throw "No trigger files matching '$TriggerFilter' were found in '$TriggerPath'. Refusing to deploy a worker with no trigger."
        }
    }
    else {
        throw "-TriggerPath not found (neither a file nor a folder): '$TriggerPath'."
    }

    foreach ($tf in $script:TriggerFiles) {
        $raw = (Get-Content -LiteralPath $tf -Raw).Trim()

        # A Windows DPAPI blob cannot be decrypted anywhere but the account that wrote it. The
        # same guard the QueueProcessor applies - see docs/ShovelBridge-Architecture.md.
        if ($raw.StartsWith('AQAAANCMnd8BFdER')) {
            throw "Trigger '$tf' is a Windows DPAPI blob. It cannot be decrypted in the Function App - re-stage it as plaintext JSON or WFAES."
        }
        # An unsubstituted release token is not valid JSON; the parse error would not say why.
        if ($raw.Contains('#{')) {
            throw "Trigger '$tf' still contains an unsubstituted release token ('#{...}'). The release pipeline must substitute it before staging."
        }
        if ($raw.StartsWith('WFAES::')) {
            Write-Note "Trigger '$([System.IO.Path]::GetFileName($tf))' is WFAES-encrypted. The worker is not wired for Key Vault decryption - stage it as plaintext JSON."
        }
        elseif ($raw.StartsWith('{')) {
            try {
                $parsed = $raw | ConvertFrom-Json
                if ($parsed.PSObject.Properties.Name -contains 'UserName' -and -not [string]::IsNullOrWhiteSpace($parsed.UserName)) {
                    $script:TriggerHasCreds = $true
                }
            } catch {
                throw "Trigger '$tf' is not valid JSON: $($_.Exception.Message)"
            }
        }
    }

    if ($script:TriggerFiles.Count -gt 1 -and [string]::IsNullOrWhiteSpace($TriggerId)) {
        throw ("$($script:TriggerFiles.Count) trigger files matched '$TriggerFilter' under '$TriggerPath'. " +
               'Pass -TriggerId to say which one this worker serves - refusing to guess, because the wrong ' +
               'workflow dead-letters every message while the queue still drains.')
    }

    $script:TriggerStagePaths = @($script:TriggerFiles | ForEach-Object { "$triggerStageFolder/$([System.IO.Path]::GetFileName($_))" })

    # Point at the FILE when there is exactly one: a Server-written trigger is named by its GUID
    # and would not match the triggers*.bite folder filter.
    $appSettings['WwTrigger__Path']  = ($script:TriggerFiles.Count -eq 1) ? $script:TriggerStagePaths[0] : $triggerStageFolder
    $appSettings['WwTrigger__Route'] = $TriggerRoute
    if ($script:TriggerFiles.Count -gt 1) { $appSettings['WwTrigger__Filter'] = $TriggerFilter }
    if (-not [string]::IsNullOrWhiteSpace($TriggerId)) { $appSettings['WwTrigger__TriggerId'] = $TriggerId }

    $script:TriggerSummary = "$($script:TriggerFiles.Count) file(s) -> $($appSettings['WwTrigger__Path']) (route=$TriggerRoute)"
}

# Service Bus trigger binding options — the standard Azure Functions host.json override
# convention (AzureFunctionsJobHost__extensions__<section>__<setting>). Defaults match
# host.json's committed values, so an unoverridden deploy behaves identically to today.
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__maxConcurrentCalls']       = "$triggerMaxConcurrentCalls"
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__prefetchCount']            = "$triggerPrefetchCount"
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__maxAutoLockRenewalDuration'] = $triggerMaxAutoLockRenewalIso
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__autoCompleteMessages']     = ($triggerAutoCompleteMessages ? 'true' : 'false')

# ── Runtime decision (resolved HERE so the plan can state it, applied in Phase 1) ──
# `functionapp show` is a READ, so it runs under -DryRun too and the plan tells the truth
# about an app that already exists rather than assuming a fresh create. StrictMode-safe:
# no app, or a payload without siteConfig, both resolve to $null ("will be created").
$existingNetFx = $null
$appShowJson   = Invoke-Az @('functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
$appShowFound  = [bool]$appShowJson
if ($appShowJson) {
    try {
        $appShow    = ($appShowJson | Out-String) | ConvertFrom-Json
        $siteConfig = $appShow.PSObject.Properties['siteConfig']
        if ($siteConfig -and $siteConfig.Value) {
            $netFxProp = $siteConfig.Value.PSObject.Properties['netFrameworkVersion']
            if ($netFxProp) { $existingNetFx = [string]$netFxProp.Value }
        }
    }
    catch { $existingNetFx = $null }
}
$script:RuntimePlan = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion $existingNetFx `
                                                 -Upgrade:$UpgradeRuntimeVersion `
                                                 -AppExists:$appShowFound

# ── Settings summary + single confirmation ─────────────────────────────────────
Write-Host ''
Write-Host '  ── Resolved deployment settings ──────────────────────────────────────' -ForegroundColor White
Write-Host ("    {0,-28}: {1}" -f 'SubscriptionId', $SubscriptionId)
Write-Host ("    {0,-28}: {1}" -f 'TenantId', $TenantId)
Write-Host ("    {0,-28}: {1}" -f 'ResourceGroup', $ResourceGroup)
Write-Host ("    {0,-28}: {1}" -f 'Location', $Location)
Write-Host ("    {0,-28}: {1}" -f 'StorageAccount', $StorageAccount)
Write-Host ("    {0,-28}: {1}" -f 'AppName', $AppName)
Write-Host ("    {0,-28}: {1}" -f 'Runtime', ("{0} {1} (Functions v{2}) — {3}" -f
    $script:TargetFunctionsRuntime, $script:TargetRuntimeVersion, $script:TargetFunctionsVersion, $script:RuntimePlan.Message))
Write-Host ("    {0,-28}: {1}" -f 'PublishPath', $PublishPath)
Write-Host ("    {0,-28}: {1}" -f 'PublishDir', ($publishIsZip ? "$PublishDir  (extracted from zip)" : $PublishDir))
Write-Host ("    {0,-28}: {1}" -f 'PublishMethod', $PublishMethod)
Write-Host ("    {0,-28}: {1}" -f 'App Insights', ($enableAppInsights ? "enabled ($AppInsightsName)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Service Bus namespace', "$ServiceBusNamespace ($ServiceBusSku)")
Write-Host ("    {0,-28}: {1}" -f 'Service Bus queue', "$ServiceBusQueueName (maxDelivery=$maxDeliveryCount, lock=${lockDurationSeconds}s, DLQ on)")
Write-Host ("    {0,-28}: {1}" -f 'Workflow', "$WorkflowName (Settings/servicebustrigger.bite — WOLF-8517)")
Write-Host ("    {0,-28}: {1}" -f 'Trigger binding', "maxConcurrentCalls=$triggerMaxConcurrentCalls, prefetchCount=$triggerPrefetchCount, maxAutoLockRenewal=$triggerMaxAutoLockRenewalIso, autoComplete=$triggerAutoCompleteMessages")
Write-Host ("    {0,-28}: {1}" -f 'Queue trigger', $script:TriggerSummary)
Write-Host ("    {0,-28}: {1}" -f 'Worker listen auth', ($useMiForServiceBus ? 'Managed Identity (recommended)' : 'SAS connection string (dev only)'))
Write-Host ("    {0,-28}: {1}" -f 'Shovel Send SAS rule', ($doCreateShovelRule ? "$ShovelSendRuleName (for the RabbitMQ Shovel bridge)" : 'not created'))
Write-Host ("    {0,-28}: {1}" -f 'LogDir', $LogDir)
Write-Host ("    {0,-28}: {1}" -f 'Run tag', "wwx-test-run=$runId")
Write-Host ("    {0,-28}: {1}" -f 'DryRun', $DryRun)
Write-Host ''
Write-Host '  ── Environment variables (App Settings) to apply ─────────────────────' -ForegroundColor White
foreach ($k in $appSettings.Keys) {
    $shown = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    Write-Host ("    {0,-34}= {1}" -f $k, $shown)
}
Write-Host ("    {0,-34}= {1}" -f 'ServiceBusConnection*', ($useMiForServiceBus ? '(identity-based — set in Phase 2)' : '(SAS connection string — set in Phase 2)'))
if ($enableAppInsights) {
    Write-Host ("    {0,-34}= {1}" -f 'APPLICATIONINSIGHTS_CONNECTION_STRING', '(auto-read from the App Insights resource)')
}
Write-Host ''
Write-Note 'Role registration is a SEPARATE step: grant this app''s managed identity the engine'
Write-Note 'role Warewolf_ClientApps (Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon'
Write-Note '-DaemonUseManagedIdentity -AppRolesToAssign Warewolf_ClientApps) + the matching'
Write-Note 'secure.config Execute row for the workflow(s) called. See docs/KB-ClientApps-Configuration.md 2.6.'
Write-Host ''

# Surfaced BEFORE the confirmation, not buried in the settings table: continuing past a
# runtime mismatch produces a deploy that reports success and a worker that never starts.
if ($script:RuntimePlan.Action -eq 'warn') {
    Write-Note ("RUNTIME MISMATCH: Function App '{0}' runs {1}, but this package targets {2}. " -f
                $AppName, $script:RuntimePlan.Current, $script:RuntimePlan.Target)
    Write-Note ("  The deploy will SUCCEED and the worker will then fail at startup. Re-run with " +
                '-UpgradeRuntimeVersion to reconcile the app (it restarts), or fix it out of band ' +
                ("with: az functionapp config set -n {0} -g {1} --net-framework-version {2}" -f
                 $AppName, $ResourceGroup, $script:RuntimePlan.Target))
}
elseif ($script:RuntimePlan.Action -eq 'unknown') {
    Write-Note ("RUNTIME UNVERIFIED: Function App '{0}' exists, but its netFrameworkVersion could not " -f $AppName)
    Write-Note ("  be read, so this run CANNOT tell whether it matches the {0} this package needs. " -f $script:RuntimePlan.Target)
    Write-Note ("  Nothing will be changed. If it is behind, the deploy still SUCCEEDS and the worker " +
                "then fails at startup. Check it with: " +
                ("az functionapp config show -n {0} -g {1} --query netFrameworkVersion" -f
                 $AppName, $ResourceGroup))
}

if (-not (Confirm-Yes 'Proceed with this deployment?' $true)) {
    Write-Note 'Aborted by user.'
    return
}

# ════════════════════════════════════════════════════════════════════════════
# Begin work — transcript log + incremental summary (dry-run and real runs).
# ════════════════════════════════════════════════════════════════════════════

$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$transcriptOn = $false
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$logInfix    = if ($DryRun) { 'dryrun.' } else { '' }
$logFile     = Join-Path $LogDir "deploy-WwExecutionServiceBusWorker-$runStamp.${logInfix}log"
$summaryPath = Join-Path $LogDir "deploy-WwExecutionServiceBusWorker-$runStamp.${logInfix}summary.json"
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

$baseUrl                = "https://$AppName.azurewebsites.net"
$StagingDir             = $PublishDir
$aiConnectionString     = $null
$shovelConnectionString = $null
$script:DeployLastPhase = 'Phase 0.5  Plan'

Save-DeploySummary -Status 'in-progress'

try {
    Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Infrastructure (resource group, storage, function app, App Insights)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Infrastructure (resource group, storage, function app, App Insights)'
    $script:DeployLastPhase = 'Phase 1  Infrastructure'

    # 1.1 Resource group
    $rgExists = (Invoke-Az @('group', 'exists', '--name', $ResourceGroup)) -join ''
    if ($rgExists -eq 'true') {
        Write-Ok "Resource group '$ResourceGroup' already exists."
        $created['resourceGroup'] = $false
    } else {
        $created['resourceGroup'] = $true
        Write-Step "Creating resource group '$ResourceGroup' in '$Location'"
        Invoke-Az (@('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--tags') + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Resource group created.'
    }

    # 1.2 Storage account
    $stExists = Invoke-Az @('storage', 'account', 'show', '--name', $StorageAccount, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($stExists) {
        Write-Ok "Storage account '$StorageAccount' already exists."
        $created['storageAccount'] = $false
    } else {
        $created['storageAccount'] = $true
        Write-Step "Creating storage account '$StorageAccount'"
        Invoke-Az (@(
            'storage', 'account', 'create',
            '--name', $StorageAccount, '--resource-group', $ResourceGroup, '--location', $Location,
            '--sku', 'Standard_LRS', '--kind', 'StorageV2', '--min-tls-version', 'TLS1_2', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Storage account created.'
    }

    # 1.3 Function App (Consumption Y1, .NET 10 isolated, Functions v4)
    $appExists = Invoke-Az @('functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($appExists) {
        Write-Ok "Function App '$AppName' already exists."
        $created['functionApp'] = $false

        # An existing app keeps the runtime it was CREATED with — `functionapp create` is the
        # only place --runtime-version applies, and this branch never runs it. Replay the
        # plan-phase decision so a long-lived app does not silently stay a version behind the
        # payload being deployed onto it.
        switch ($script:RuntimePlan.Action) {
            'upgrade' {
                Write-Step ("Upgrading Function App runtime {0} -> {1} (the app restarts)" -f
                            $script:RuntimePlan.Current, $script:RuntimePlan.Target)
                Invoke-Az @(
                    'functionapp', 'config', 'set',
                    '--name', $AppName, '--resource-group', $ResourceGroup,
                    '--net-framework-version', $script:RuntimePlan.Target, '-o', 'none'
                ) -Mutating | Out-Null
                Write-Ok "Function App runtime set to $($script:RuntimePlan.Target)."
            }
            'warn' {
                Write-Note (("Function App runtime left at {0}; this package targets {1}. " -f
                             $script:RuntimePlan.Current, $script:RuntimePlan.Target) +
                            'Re-run with -UpgradeRuntimeVersion to reconcile it.')
            }
            'none' {
                Write-Ok "Function App runtime is $($script:RuntimePlan.Target)."
            }
            default {
                # 'unknown' (and 'create', which cannot legitimately reach an EXISTING app). Reporting
                # Ok here was the bug: `functionapp create` is the only thing that pins the runtime and
                # it does not run on this branch, so an unread runtime has been reconciled by nothing.
                Write-Note ("Function App runtime NOT verified (current value could not be read); this " +
                            ("package targets {0}. Nothing was changed. Check it with: " -f $script:RuntimePlan.Target) +
                            ("az functionapp config show -n {0} -g {1} --query netFrameworkVersion" -f
                             $AppName, $ResourceGroup))
            }
        }
    } else {
        $created['functionApp'] = $true
        Write-Step ("Creating Function App '{0}' (Consumption Y1, {1} {2}, Functions v{3})" -f
                    $AppName, $script:TargetFunctionsRuntime, $script:TargetRuntimeVersion, $script:TargetFunctionsVersion)
        Invoke-Az (@(
            'functionapp', 'create',
            '--name', $AppName, '--resource-group', $ResourceGroup, '--consumption-plan-location', $Location,
            '--storage-account', $StorageAccount, '--runtime', $script:TargetFunctionsRuntime,
            '--runtime-version', $script:TargetRuntimeVersion,
            '--functions-version', $script:TargetFunctionsVersion,
            '--https-only', 'true', '--os-type', 'Windows', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Function App created.'
    }

    # 1.4 Application Insights
    if ($enableAppInsights) {
        $aiPreExists = [bool](Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail)
        $created['appInsights'] = (-not $aiPreExists)
        Invoke-ChildScript -Path $AppInsightsScript -Label 'Setup-ApplicationInsights.ps1' -Parameters @{
            ResourceGroup   = $ResourceGroup
            FunctionAppName = $AppName
            Location        = $Location
            AppInsightsName = $AppInsightsName
            # The STANDARD name (WorkerTelemetry.cs reads it) - not the engine's WAREWOLF_ one: this
            # worker wants the Functions host pipeline, which writes the requests/exceptions tables.
            ConnectionStringSettingName = 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            SkipLoggingConfigFlag       = $true
        }
        if ($created['appInsights'] -and -not $DryRun) {
            Invoke-Az (@('resource', 'tag', '--resource-group', $ResourceGroup, '--name', $AppInsightsName, '--resource-type', 'microsoft.insights/components', '--tags') + $ResourceTags) -Mutating | Out-Null
        }
        $aiConnectionString = (Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '--query', 'connectionString', '-o', 'tsv') -AllowFail) -join ''
        if ([string]::IsNullOrWhiteSpace($aiConnectionString)) { $aiConnectionString = '<pending — created/read on a real run>' }
        Write-Ok ("Application Insights configured ({0} existing component)." -f ($aiPreExists ? 'reused' : 'created'))
    } else {
        Write-Note 'Application Insights disabled.'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Service Bus namespace, queue, dead-lettering, auth rules
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Service Bus namespace + queue provisioning'
    $script:DeployLastPhase = 'Phase 2  Service Bus provisioning'

    # 2.1 Namespace
    $nsExists = Invoke-Az @('servicebus', 'namespace', 'show', '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($nsExists) {
        Write-Ok "Service Bus namespace '$ServiceBusNamespace' already exists."
        $created['serviceBusNamespace'] = $false
    } else {
        $created['serviceBusNamespace'] = $true
        Write-Step "Creating Service Bus namespace '$ServiceBusNamespace' ($ServiceBusSku)"
        Invoke-Az (@(
            'servicebus', 'namespace', 'create',
            '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--location', $Location,
            '--sku', $ServiceBusSku, '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Service Bus namespace created.'
    }

    # 2.2 Queue — dead-lettering on max delivery count exhaustion, configurable lock duration.
    $queueExists = Invoke-Az @('servicebus', 'queue', 'show', '--name', $ServiceBusQueueName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    $lockDurationIso = "PT${lockDurationSeconds}S"
    if ($queueExists) {
        Write-Ok "Queue '$ServiceBusQueueName' already exists."
        $created['serviceBusQueue'] = $false
    } else {
        $created['serviceBusQueue'] = $true
        Write-Step "Creating queue '$ServiceBusQueueName' (maxDeliveryCount=$maxDeliveryCount, lockDuration=$lockDurationIso, DLQ-on-exceed-max-delivery)"
        Invoke-Az @(
            'servicebus', 'queue', 'create',
            '--name', $ServiceBusQueueName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup,
            '--max-delivery-count', "$maxDeliveryCount", '--lock-duration', $lockDurationIso,
            '--enable-dead-lettering-on-message-expiration', 'true'
        ) -Mutating | Out-Null
        Write-Ok 'Queue created.'
    }

    # 2.3 Listen auth for the worker — Managed Identity (recommended) or SAS fallback.
    if ($useMiForServiceBus) {
        Write-Note 'Worker listen auth: Managed Identity — RBAC role assignment happens in Phase 3 (after the identity exists).'
    } else {
        $listenRuleName = 'wwexecutionworker-listen'
        $listenRuleExists = [bool](Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'show', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '-o', 'json') -AllowFail)
        $created['serviceBusListenRule'] = (-not $listenRuleExists)
        if (-not $listenRuleExists) {
            Write-Step "Creating Listen-only SAS authorization rule '$listenRuleName' on the queue"
            Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'create', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--rights', 'Listen') -Mutating | Out-Null
        }
        $script:listenConnectionString = if ($DryRun) { '<dryrun-listen-connection-string>' } else {
            (Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'keys', 'list', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--query', 'primaryConnectionString', '-o', 'tsv') -AllowFail) -join ''
        }
        Write-Ok "Listen SAS rule ready (connection string applied to app settings in Phase 3)."
    }

    # 2.4 Send-only SAS rule for the RabbitMQ Shovel plugin's AMQP 1.0 destination.
    # Least-privilege by design: the Shovel only ever needs to PUBLISH to this queue.
    if ($doCreateShovelRule) {
        $shovelRuleExists = [bool](Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'show', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '-o', 'json') -AllowFail)
        $created['shovelSendRule'] = (-not $shovelRuleExists)
        if (-not $shovelRuleExists) {
            Write-Step "Creating Send-only SAS authorization rule '$ShovelSendRuleName' on the queue (for the RabbitMQ Shovel bridge)"
            Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'create', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--rights', 'Send') -Mutating | Out-Null
        } else {
            Write-Ok "Send-only SAS rule '$ShovelSendRuleName' already exists."
        }
        $shovelConnectionString = if ($DryRun) { '<dryrun-shovel-connection-string>' } else {
            (Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'keys', 'list', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--query', 'primaryConnectionString', '-o', 'tsv') -AllowFail) -join ''
        }
        Write-Ok 'Shovel Send SAS rule ready — see the masked summary at the end for retrieval instructions.'
    } else {
        Write-Note 'Shovel Send SAS rule not requested (-CreateShovelSendRule:$false).'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Managed identity + Service Bus RBAC
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Managed identity + Service Bus RBAC'
    $script:DeployLastPhase = 'Phase 3  Managed identity + RBAC'

    Write-Step 'Enabling system-assigned managed identity on the Function App'
    $idJson = Invoke-Az @('functionapp', 'identity', 'assign', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -Mutating
    $funcPrincipalId = if ($DryRun) { '<dryrun-principal-id>' } else { ($idJson | ConvertFrom-Json).principalId }

    if ($useMiForServiceBus) {
        $nsId = if ($DryRun) { '<dryrun-namespace-id>' } else { (Invoke-Az @('servicebus', 'namespace', 'show', '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--query', 'id', '-o', 'tsv')) -join '' }
        Write-Step 'Assigning RBAC: Azure Service Bus Data Receiver -> Function App managed identity (namespace scope)'
        Invoke-Az @('role', 'assignment', 'create', '--role', 'Azure Service Bus Data Receiver', '--assignee', $funcPrincipalId, '--scope', $nsId) -Mutating | Out-Null
        Write-Ok 'RBAC assigned. (Propagation can take a couple of minutes before the trigger connects successfully.)'
    } else {
        Write-Note 'Managed Identity RBAC skipped (worker uses the SAS Listen connection string instead).'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Apply app settings
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Apply app settings'
    $script:DeployLastPhase = 'Phase 4  App settings'

    if ($useMiForServiceBus) {
        $appSettings['ServiceBusConnection__fullyQualifiedNamespace'] = "$ServiceBusNamespace.servicebus.windows.net"
        $appSettings['ServiceBusConnection__credential']              = 'managedidentity'
        if (-not [string]::IsNullOrWhiteSpace($WwExecutionManagedIdentityClientId)) {
            $appSettings['ServiceBusConnection__clientId'] = $WwExecutionManagedIdentityClientId
        }
    } else {
        $appSettings['ServiceBusConnection'] = $script:listenConnectionString
        $secretSettingNames.Add('ServiceBusConnection') | Out-Null
    }

    Write-Step 'Applying environment variables (App Settings)'
    $settingsArgs = @($appSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" })
    Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '--name', $AppName, '--resource-group', $ResourceGroup, '--settings') + $settingsArgs + @('-o', 'none')) -Mutating | Out-Null
    Write-Ok "$($appSettings.Count) app setting(s) applied."

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 5 — Deploy the package directory to the Function App
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Deploy package to Function App'
    $script:DeployLastPhase = 'Phase 5  Deploy package'

    # WOLF-8517: overwrite Settings/servicebustrigger.bite in the staging directory with THIS
    # run's queue/workflow BEFORE it is zipped/published below — must happen before both the
    # func-publish and zip-deploy branches, since both publish from $StagingDir.
    Set-StagedServiceBusTriggerConfig -StagingDir $StagingDir -QueueName $ServiceBusQueueName -WorkflowName $WorkflowName
    if ($DryRun) { Write-Note "Settings/servicebustrigger.bite would be written into the staging directory (not applied — DryRun): QueueName=$ServiceBusQueueName, WorkflowName=$WorkflowName." } else { Write-Ok "Settings/servicebustrigger.bite staged: QueueName=$ServiceBusQueueName, WorkflowName=$WorkflowName." }
    # -- Stage the queue trigger INTO the package --------------------------
    # Must happen before the zip is built: WwTrigger__Path is resolved by the worker against
    # its own content root, so the trigger has to travel inside the deployed package.
    if ($script:TriggerFiles.Count -gt 0) {
        $triggerDest = Join-Path $StagingDir 'Settings/triggers'
        if ($DryRun) {
            Write-Host "      [DRYRUN] stage $($script:TriggerFiles.Count) trigger file(s) -> '$triggerDest'" -ForegroundColor DarkGray
        } else {
            New-Item -ItemType Directory -Force -Path $triggerDest | Out-Null
            foreach ($tf in $script:TriggerFiles) {
                $destFile = Join-Path $triggerDest ([System.IO.Path]::GetFileName($tf))
                $raw      = (Get-Content -LiteralPath $tf -Raw).Trim()

                if ($StripTriggerCredentials -and $raw.StartsWith('{')) {
                    # Re-serialized, so no longer byte-identical to what the Server wrote. Only the
                    # fields this worker actually reads matter (WorkflowName / Inputs /
                    # MapEntireMessage), and it never authenticates to RabbitMQ.
                    $obj = $raw | ConvertFrom-Json
                    foreach ($field in @('UserName', 'Password')) {
                        if ($obj.PSObject.Properties.Name -contains $field) { $obj.$field = $null }
                    }
                    ($obj | ConvertTo-Json -Depth 64) | Set-Content -LiteralPath $destFile -Encoding utf8NoBOM
                    Write-Ok "Staged $([System.IO.Path]::GetFileName($tf)) (broker credentials stripped)."
                } else {
                    Copy-Item -LiteralPath $tf -Destination $destFile -Force
                    Write-Ok "Staged $([System.IO.Path]::GetFileName($tf))."
                }
            }
        }
        if ($script:TriggerHasCreds -and -not $StripTriggerCredentials) {
            Write-Note 'The staged trigger carries plaintext broker credentials this worker never uses.'
            Write-Note 'Pass -StripTriggerCredentials to null them in the staged copy.'
        }
    }

    $useFunc = $false
    switch ($PublishMethod) {
        'Func' {
            if (-not (Test-CommandExists 'func')) { throw "PublishMethod 'Func' requires Azure Functions Core Tools (func). Install: https://aka.ms/azfunc-install" }
            $useFunc = $true
        }
        'Auto' { $useFunc = $false }
        'Zip'  { $useFunc = $false }
    }

    if ($useFunc) {
        if ($DryRun) {
            Write-Host "      [DRYRUN] (cd '$StagingDir') func azure functionapp publish $AppName --dotnet-isolated --no-build" -ForegroundColor DarkGray
        } else {
            Push-Location $StagingDir
            try {
                & func azure functionapp publish $AppName --dotnet-isolated --no-build
                if ($LASTEXITCODE -ne 0) { throw "func publish failed ($LASTEXITCODE)." }
            } finally { Pop-Location }
            Write-Ok 'Deployed via func.'
        }
    } else {
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecutionservicebusworker-$AppName-$runStamp.zip"
        if ($DryRun) {
            Write-Host "      [DRYRUN] Compress-Archive '$StagingDir\*' -> '$zipPath'" -ForegroundColor DarkGray
            Write-Host "      [DRYRUN] az functionapp deployment source config-zip --name $AppName --resource-group $ResourceGroup --src '$zipPath'" -ForegroundColor DarkGray
        } else {
            if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
            Write-Step 'Creating deployment package'
            Compress-Archive -Path (Join-Path $StagingDir '*') -DestinationPath $zipPath -Force
            try {
                Invoke-Az @('functionapp', 'deployment', 'source', 'config-zip', '--name', $AppName, '--resource-group', $ResourceGroup, '--src', $zipPath) -Mutating | Out-Null
                Write-Ok 'Deployed via az zip-deploy.'
            } finally { if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force } }
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 6 — Verify
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 6  Verify'
    $script:DeployLastPhase = 'Phase 6  Verify'

    Write-Ok "Function App: $baseUrl (Service Bus-triggered — no public HTTP route to probe)"
    Write-Note "Confirm the trigger registered: az functionapp function list --name $AppName --resource-group $ResourceGroup -o table"
    Write-Note "Confirm queue depth: az servicebus queue show --name $ServiceBusQueueName --namespace-name $ServiceBusNamespace --resource-group $ResourceGroup --query countDetails"

    Save-DeploySummary -Status 'completed'
    Write-Ok "Summary written to $summaryPath"

    Write-Phase ($DryRun ? 'Dry-run complete (no cloud changes made)' : 'Deployment complete')
    Write-Host "  App              : $AppName" -ForegroundColor White
    Write-Host "  RG               : $ResourceGroup" -ForegroundColor White
    Write-Host "  Endpoint         : $baseUrl" -ForegroundColor White
    Write-Host "  Service Bus queue: $ServiceBusNamespace / $ServiceBusQueueName" -ForegroundColor White
    Write-Host "  Workflow         : $WorkflowName" -ForegroundColor White
    Write-Host "  Summary          : $summaryPath" -ForegroundColor White
    Write-Host ''
    if ($doCreateShovelRule) {
        Write-Note "RabbitMQ Shovel destination credential — retrieve with:"
        Write-Note "  az servicebus queue authorization-rule keys list --resource-group $ResourceGroup ``"
        Write-Note "      --namespace-name $ServiceBusNamespace --queue-name $ServiceBusQueueName --name $ShovelSendRuleName ``"
        Write-Note "      --query primaryConnectionString -o tsv"
        Write-Note "Convert to the Shovel's AMQP 1.0 destination URI per docs/ShovelBridge-Architecture.md."
    }
    Write-Note 'NEXT: register this app''s managed identity for the engine role Warewolf_ClientApps'
    Write-Note '(see docs/KB-ClientApps-Configuration.md 2.6) before the worker can call the engine.'
    Write-Host ''
}
catch {
    $script:DeployStatus = 'failed'
    $errMsg = $_.Exception.Message
    Write-Note "Deployment FAILED in '$script:DeployLastPhase': $errMsg"
    try {
        Save-DeploySummary -Status 'failed' -ErrorMessage $errMsg
        Write-Note "Failure summary written to $summaryPath."
    } catch {
        Write-Note "Could not write failure summary: $($_.Exception.Message)"
    }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
}
