#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end deployment orchestrator for the Warewolf Workflow Execution
    Engine Azure Function App (Warewolf.Execution.Lightweight).

.DESCRIPTION
    A THIN ORCHESTRATOR.  It provisions Azure infrastructure, configures auth,
    stages + encrypts the package contents, applies the engine's environment
    variables, and deploys an ALREADY-PUBLISHED package to the Function App.

    It does NOT build/publish the project.  You publish once yourself, e.g.:

        dotnet publish Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish

    then point this script at that folder (or a .zip of it) via -PublishPath.

    Flow:
        Phase 0    Pre-flight   - az CLI + login, resolve subscription/tenant
        Phase 0.5  PLAN         - resolve EVERY decision (targeting, publish source,
                                  feature toggles, all env-var values, secure.config
                                  classification, encryption/Key Vault, Elasticsearch),
                                  print a full masked summary, and ask once to proceed
                                  BEFORE anything is created or changed
        Phase 1    Infra        - resource group, storage, Function App, App Insights
        Phase 2    Auth         - Entra ID + Easy Auth (Configure-WwExecutionAuth.ps1)
        Phase 3    Stage        - secure.config (validate / auto-encrypt plaintext),
                                  workflow resources (+ optional WFAES encryption),
                                  Elasticsearch source (+ WFAES encryption),
                                  executionengine.settings.json (Key Vault topology +
                                  ServiceBusTrigger tunables — WOLF-8516),
                                  servicebustrigger.bite (queue/workflow selection —
                                  WOLF-8517), environment variables
        Phase 4    Deploy       - publish the package dir to the Function App
                                  (func, falling back to az zip-deploy)
        Phase 5    Verify       - endpoint banner + optional HTTP probe

    A timestamped transcript log and a masked *.summary.json are written for
    every real (non -DryRun) run.

    Inputs follow a "params first, prompt if missing" model.  REQUIRED targeting
    parameters have NO defaults (breaking change — see NOTES).

.PARAMETER SubscriptionId
    Azure subscription GUID.  Resolved from `az account show` when omitted.

.PARAMETER TenantId
    Microsoft Entra tenant GUID.  Resolved from `az account show` when omitted.

.PARAMETER ResourceGroup
    Resource group to create / use.  REQUIRED (no default).

.PARAMETER Location
    Azure region (e.g. southafricanorth).  REQUIRED (no default).

.PARAMETER StorageAccount
    Storage account name (3-24 lowercase chars).  REQUIRED (no default).

.PARAMETER AppName
    Function App name (globally unique).  REQUIRED (no default).

.PARAMETER PublishPath
    Path to the ALREADY-PUBLISHED package: either a folder, or a .zip of the
    publish output (extracted to a sibling folder, which becomes the package
    directory).  REQUIRED (no default).

.PARAMETER PublishMethod
    Auto (default) | Zip | Func.  Auto and Zip both use az zip-deploy (config-zip),
    the correct method for the PRE-BUILT publish artifact this orchestrator deploys.
    Func uses 'func azure functionapp publish --dotnet-isolated --no-build' and is
    ADVANCED/opt-in only — 'func' expects a project source directory and fails on a
    pre-built package ("Can't determine project language… Worker runtime 'None'").

.PARAMETER AppInsightsName
    Application Insights resource name.  Default: <AppName>-ai.

.PARAMETER EnableAppInsights
    Provision Application Insights and enable worker telemetry (WOLF-8516: sets
    "appInsights":true in the WAREWOLF_LOGGING_CONFIG JSON app setting, plus
    WAREWOLF_APPINSIGHTS_CONNECTION_STRING).

.PARAMETER SkipAuthProvisioning
    Skip Entra ID + Easy Auth provisioning.

.PARAMETER AuthConfigPath
    JSON file describing GroupPermissions and UserAssignments for
    Configure-WwExecutionAuth.ps1.

.PARAMETER SecureConfigPath
    Path to the secure.config to deploy.  An already-AES-encrypted file is
    validated (must be decryptable by the engine) and staged as-is; a PLAINTEXT
    JSON file is validated then AES-encrypted automatically before staging.

.PARAMETER WorkflowsSourcePath
    Folder of workflow .bite resource files, staged into <PublishDir>/Resources.

.PARAMETER EncryptResources
    OPTIONAL — default FALSE.  When set, this run encrypts ALL deployable sources
    (workflow .bite ConnectionStrings, the Elasticsearch source, and any others)
    from plain/DPAPI to WFAES via the Key Vault key.  Encrypt ONCE: on subsequent
    deploys leave this off — the sources are already encrypted and are staged
    as-is.  (You must still pass -KeyVaultName/-KeyVaultSecretName for already-
    encrypted sources so the engine can decrypt them at runtime — see below.)

.PARAMETER VerifyDecryption
    OPTIONAL — default FALSE.  After encryption, verify IN MEMORY that the engine's
    Key Vault key decrypts every value (no plaintext is written to disk).  Only
    meaningful together with -EncryptResources.

.PARAMETER KeyVaultName
    Key Vault the engine uses to decrypt WFAES sources at runtime.  REQUIRED when
    -EncryptResources is set, and also whenever your deployed sources are already
    WFAES-encrypted (so the runtime managed-identity role is wired).  WOLF-8516:
    written into the staged Settings/executionengine.settings.json file, NOT an
    App Setting any more — HostEnvironmentConfig has no env-var fallback for it.

.PARAMETER KeyVaultSecretName
    Key Vault secret holding the AES key material.  REQUIRED whenever -KeyVaultName
    is in play (no default — you must name the secret explicitly).  WOLF-8516:
    written into Settings/executionengine.settings.json alongside -KeyVaultName —
    see that parameter's note.

.PARAMETER GenerateNewKey
    Generate a fresh AES key (Encrypt-Config -GenerateKeys); implied for a new vault.

.PARAMETER EnableElasticsearch
    Enable Elasticsearch logging.  Requires -ElasticsearchSourcePath and Key Vault
    (the source's ConnectionString is WFAES-encrypted before deploy).

.PARAMETER EnablePerformanceCounters
    Register the Application Insights PerformanceCollectorModule (process memory/CPU
    counters, feeding App Insights' performanceCounters table). Only meaningful when
    -EnableAppInsights is also true. Off by default — opt-in per deployment.

.PARAMETER ElasticsearchSourcePath
    Path to the user-supplied Elasticsearch source.  The file MUST be named
    exactly 'ElasticsearchLoggingSource.bite' (the engine reads that exact path).

.PARAMETER EnableConsoleLogging
    Sets "console":true in the WAREWOLF_LOGGING_CONFIG JSON app setting (WOLF-8516;
    formerly the standalone ENABLECONSOLELOGGING env var).

.PARAMETER ExecutionLogLevel
    EXECUTIONLOGLEVEL value (TRACE|DEBUG|INFO|WARN|ERROR|FATAL|OFF).  Default INFO.
    Drives the engine's console + App Insights logging in code (the isolated worker
    does NOT read host.json).

.PARAMETER AlignHostJsonLogLevel
    OPT-IN.  Also rewrite the published host.json logLevel (default + Warewolf.*) to
    the EXECUTIONLOGLEVEL-mapped MEL level.  NOT required for the engine's own
    logging — it only tunes the Functions HOST process verbosity.  Default: off.

.PARAMETER ServiceBusMaxConcurrentCalls
    OPT-IN.  Sets host.json's extensions.serviceBus.maxConcurrentCalls — bounds how many
    Service Bus messages the Functions host DISPATCHES to the trigger concurrently, upstream
    of and independent from ServiceBusTriggerOptions.MaxConcurrentExecutions' own semaphore
    gate inside the trigger's own code (WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS).
    Left at the SDK default (unset) unless explicitly supplied.  See WOLF-8512,
    docs/ShovelBridge-Architecture.md.

    INERT ON ITS OWN: dynamic concurrency overrides every static concurrency setting in
    host.json, so pair this with -DisableDynamicConcurrency or the cap does nothing.

.PARAMETER FunctionAppScaleLimit
    OPT-IN.  Caps Consumption-plan scale-out (siteConfig.functionAppScaleLimit), applied to a
    newly created AND a pre-existing app.  Set it BELOW the message broker's connection limit:
    the RabbitMQ connection cache is keyed per broker endpoint, so a single-broker workload
    holds ONE connection PER INSTANCE regardless of the cache's maxConnections — instance count
    is what spends the broker's budget.  Measured: 48 instances against a 40-connection vhost
    produced 'NOT_ALLOWED - ... connection limit (40) is reached' and dead-lettered the losers.
    Leave unset for the Azure default (effectively unbounded).

.PARAMETER DisableDynamicConcurrency
    OPT-IN.  Pins host.json's concurrency.dynamicConcurrencyEnabled to false, creating the
    concurrency block if absent.  REQUIRED for -ServiceBusMaxConcurrentCalls (and the
    trigger's own MaxConcurrentExecutions) to actually bound concurrency: with dynamic
    concurrency on, the Functions host ignores those caps and scales the trigger on its own
    (measured: Concurrency 170 against a requested 2, saturating the workflow's SQL database).
    Default: off, i.e. leave whatever host.json ships with.

.PARAMETER ServiceBusTriggerExecutionTimeoutSeconds
.PARAMETER ServiceBusTriggerMaxConcurrentExecutions
.PARAMETER ServiceBusTriggerSlotWaitTimeoutSeconds
.PARAMETER ServiceBusTriggerSettlementTimeoutSeconds
    OPT-IN, deploy-time-static tunables for the in-process Service Bus workflow trigger
    (ServiceBusTriggerOptions — executionTimeoutSeconds/maxConcurrentExecutions/
    slotWaitTimeoutSeconds/settlementTimeoutSeconds respectively). WOLF-8516: these no
    longer have ANY env-var fallback — they are read SOLELY from the staged
    Settings/executionengine.settings.json file's serviceBusTrigger section (see
    ServiceBusTriggerOptions.FromEnvironment). When any one of these params is
    supplied, this script stages that section (independent of
    -KeyVaultName/-EncryptResources — the two concerns share the same file but are
    otherwise unrelated); an unsupplied field is left null in the staged JSON and the
    engine falls back to its own hardcoded default for that field only. Note
    ServiceBusMaxConcurrentCalls above is a DIFFERENT, upstream dispatch-level cap
    (host.json) — these tune the trigger's OWN in-process semaphore/timeouts, not the
    host's dispatch.

.PARAMETER ServiceBusTriggerQueueName
.PARAMETER ServiceBusTriggerWorkflowName
    WOLF-8517 (corrected). Which queue the in-process Service Bus trigger listens on, and
    which workflow every message on that queue executes — staged into
    Settings/servicebustrigger.bite (ServiceBusTriggerDefinition.cs) in Phase 3, THE SAME WAY
    the ServiceBusTrigger tunables above are staged into executionengine.settings.json. Both
    are REQUIRED together, or both omitted: ServiceBusTriggerDefinition.Load() refuses to
    start an enabled trigger with either field blank, so a deploy-plan-time throw here is
    cheaper than a cold-start failure discovered later. When neither is supplied, the file
    already present in -PublishPath (the checked-in dev/sample value) ships unchanged — this
    keeps local/dev behaviour exactly as before.

    NOTE: earlier versions of this design tried to write this file to an ALREADY-DEPLOYED
    Function App's file system after the fact (Enable-ServiceBusSecureTrigger.ps1's Kudu VFS
    write). That cannot work when WEBSITE_RUN_FROM_PACKAGE=1 (the normal zip-deploy mode this
    script uses) mounts wwwroot read-only — confirmed live against WarewolfServer-UAT
    (2026-09-12): the write failed with UnauthorizedAccessException. Baking the file into the
    package at deploy time (here) is the only mechanism that actually works.

.PARAMETER LicenseCheckEnabled
    WAREWOLF_LICENSE_CHECK_ENABLED (engine default: true).

.PARAMETER LicenseConfigPath
    Path to a 'Warewolf License.secureconfig' file.  When supplied it is copied to
    the publish root as 'Warewolf License.secureconfig' (the engine's license gate
    reads it from there).  Prompted interactively; OPTIONAL — if omitted, the
    license check (default ON) may fail at startup.  See
    https://warewolf.io/knowledge-base/articles/security-encryption/.

.PARAMETER StructuredLogs
    JSON console output (default true) — sets "structuredLogs" in the
    WAREWOLF_LOGGING_CONFIG JSON app setting (WOLF-8516; formerly the standalone
    STRUCTURED_LOGS env var).

.PARAMETER UpgradeRuntimeVersion
    OPT-IN.  Reconcile an EXISTING Function App's runtime to dotnet-isolated 10 /
    netFrameworkVersion v10.0 (what the engine's net10.0 payload requires).  A new
    app is always created on the correct runtime; an app created by an earlier run
    keeps the runtime it was created with, so a v8.0 host would load the net10.0
    worker and die at startup with "You must install or update .NET to run this
    application."  Changing it RESTARTS the app, which is why it is not automatic:
    without this switch a mismatch is reported in the Phase 0.5 plan and warned
    about in Phase 1, but nothing is changed.

.PARAMETER LogDir
    Directory for the transcript + summary.  Default: <PublishDir>\..\deploy-logs.

.PARAMETER NonInteractive
    Never prompt; missing required values cause an early, clear throw.

.PARAMETER DryRun
    Print every mutating action without executing it.  Read-only probes still run;
    no transcript/summary is written.

.NOTES
    BREAKING CHANGE: -ResourceGroup, -Location, -StorageAccount, -AppName and
    -PublishPath (and -KeyVaultSecretName when encrypting) no longer have defaults.
    Non-interactive callers MUST pass them explicitly.

    FIXED / NON-CONFIGURABLE env vars: ASPNETCORE_ENVIRONMENT is always
    'Production'; the three security kill-switches now merged into the
    WAREWOLF_SECURITY_FLAGS JSON app setting (bypassSecureConfig, superAdminEnabled,
    skipFailureToRetrieveSecret) are always left unset (all default 'false') and are
    not exposed as parameters.  The development-only DEBUG_* settings (now merged
    into WAREWOLF_DEBUG_CONFIG) have also been removed, and WEBSITE_INSTANCE_ID /
    AZURE_FUNCTIONS_ENVIRONMENT remain platform-managed.

    WOLF-8516: AZURE_KEYVAULT_NAME, KEYVAULT_SECRET_NAME are no longer App Settings —
    they're written into the staged Settings/executionengine.settings.json file
    (Phase 3). EXECUTIONLOGLEVEL stays a standalone App Setting; the other 5 logging
    toggles (console/appInsights/elasticsearch/performanceCounters/structuredLogs)
    are merged into one WAREWOLF_LOGGING_CONFIG JSON App Setting. Entra identity
    (tenantId/audience/clientId) is written by Configure-WwExecutionAuth.ps1 as one
    WAREWOLF_ENTRA_CONFIG JSON App Setting instead of 3 separate ones. See
    Infrastructure/HostEnvironmentConfig.cs, Logging/LoggingConfiguration.cs,
    Auth/Models/EntraIdentityOptions.cs, and Infrastructure/SecurityFlags.cs.

    Prerequisites: PowerShell 7+, Azure CLI (az, logged in), and — only when
    publishing via func — Azure Functions Core Tools.  DPAPI-encrypted .bite
    sources can only be re-encrypted on the machine that created them.
#>

[CmdletBinding()]
param(
    # ── Targeting (REQUIRED — no defaults) ───────────────────────────────────
    [string] $SubscriptionId,
    [string] $TenantId,
    [string] $ResourceGroup,
    [string] $Location,
    [string] $StorageAccount,
    [string] $AppName,

    # ── Runtime ──────────────────────────────────────────────────────────────
    # OPT-IN.  A Function App is created on the correct runtime; one that ALREADY
    # exists keeps whatever it was created with, and changing it restarts the app.
    [switch] $UpgradeRuntimeVersion,

    # OPT-IN. Caps how many instances the Consumption plan may scale out to
    # (siteConfig.functionAppScaleLimit). Exists because INSTANCE COUNT, not per-instance
    # configuration, is what consumes a BROKER CONNECTION BUDGET: the RabbitMQ connection
    # cache is keyed per broker endpoint (RabbitMqCacheKey.For(host, port, user, vhost,
    # password)), so a single-broker workload holds exactly ONE cached connection PER
    # INSTANCE no matter what maxConnections says. Measured on wwuat3 2026-09-15: 48
    # instances against a CloudAMQP vhost capped at 40 connections produced
    # "NOT_ALLOWED - access to vhost '...' refused: connection limit (40) is reached" and
    # dead-lettered every message whose instance lost the race. Set this BELOW the broker's
    # connection limit, with headroom for connections that linger up to the cache's
    # idleTimeout while instances recycle. Sampled under sustained load, a cap of 30 plateaus at
    # 32 of 40 connections and stays there — a margin of EIGHT, so this is near the ceiling rather
    # than conservative, and the plateau tracks the instance cap rather than message volume
    # (identical at 5,000 and 15,000 messages). Left unset (Azure default, effectively unbounded)
    # unless supplied. This is a DEPLOY-TIME setting on purpose: set by hand it is silently
    # reverted by the next deploy, exactly as documented for the persistence flag in
    # docs/ShovelBridge-Architecture.md.
    [ValidateRange(1, 1000)]
    [nullable[int]] $FunctionAppScaleLimit,

    # ── Publish source (REQUIRED — folder or .zip) ───────────────────────────
    [string] $PublishPath,
    [ValidateSet('Auto', 'Func', 'Zip')]
    [string] $PublishMethod = 'Auto',

    # ── Application Insights ─────────────────────────────────────────────────
    [string] $AppInsightsName,
    [nullable[bool]] $EnableAppInsights,

    # ── Auth provisioning ────────────────────────────────────────────────────
    [switch] $SkipAuthProvisioning,
    [string] $AuthConfigPath,

    # ── Package inputs ───────────────────────────────────────────────────────
    [string] $SecureConfigPath,
    [string] $WorkflowsSourcePath,
    [string] $LicenseConfigPath,          # 'Warewolf License.secureconfig'; prompted, optional

    # ── Encryption / Key Vault ───────────────────────────────────────────────
    [nullable[bool]] $EncryptResources,
    [switch] $VerifyDecryption,           # OPT-IN: in-memory decrypt verification (default off)
    [string] $KeyVaultName,
    [string] $KeyVaultSecretName,        # no default — name the secret explicitly
    [switch] $GenerateNewKey,

    # ── Elasticsearch logging ────────────────────────────────────────────────
    [nullable[bool]] $EnableElasticsearch,
    [string] $ElasticsearchSourcePath,

    # ── Application Insights performance counters ─────────────────────────────
    [nullable[bool]] $EnablePerformanceCounters,

    # ── Suspend/resume persistence (Hangfire) ─────────────────────────────────
    # The resume route reads Config.Persistence; the DbSource ConnectionString is
    # WFAES-encrypted exactly like the Elasticsearch source. Both files are PROMPTED
    # when persistence is enabled and a path is not passed.
    [nullable[bool]] $EnablePersistence,
    [string] $PersistenceSettingsPath,       # persistencesettings.json
    [string] $PersistenceDbSourcePath,       # persistencesettingsdbsource.bite

    # ── ExecutionEngineJobProcessor (optional companion deploy) ───────────────
    # When -DeployJobProcessor, after the engine deploy this calls
    # Deploy-WwJobProcessor.ps1 for the poller/reaper Function App, passing the
    # shared context (subscription/tenant/RG/location/Key Vault/persistence pair);
    # the child prompts for anything not supplied here.
    # JobProcessorPublishPath MUST be a SEPARATE publish output from the engine's
    # (it is a different csproj / different Function App); the plan phase resolves
    # it and fails loudly if it collides with the engine PublishPath.
    [switch] $DeployJobProcessor,
    [string] $JobProcessorAppName,
    [string] $JobProcessorPublishPath,
    [string] $JobProcessorStorageAccount,
    [string] $EngineResumeScope,             # MI token scope the processor uses (api://<engine-app-id>/.default)

    # ── Warewolf.Execution.ServiceBusWorker (optional companion deploy) ───────
    # When -DeployServiceBusWorker, after the engine deploy this calls
    # Deploy-WwExecutionServiceBusWorker.ps1 for the Service Bus-triggered
    # Function App (the engine's other first-class trigger path alongside
    # HTTPS), passing the shared context (subscription/tenant/RG/location);
    # the child prompts for anything not supplied here, including the Service
    # Bus namespace/queue it provisions.
    # ServiceBusWorkerPublishPath MUST be a SEPARATE publish output from the
    # engine's (it is a different csproj / different Function App); the plan
    # phase resolves it and fails loudly if it collides with the engine
    # PublishPath.
    [switch] $DeployServiceBusWorker,
    [string] $ServiceBusWorkerAppName,
    [string] $ServiceBusWorkerPublishPath,
    [string] $ServiceBusWorkerStorageAccount,
    # WOLF-8517: Deploy-WwExecutionServiceBusWorker.ps1's -WorkflowName is now REQUIRED (the
    # message body no longer names the workflow) — forwarded here so -NonInteractive companion
    # deploys don't fail on a missing prompt; when omitted, the child script prompts for it
    # interactively same as its other unforwarded parameters.
    [string] $ServiceBusWorkerWorkflowName,
    [string] $WwExecutionScope,               # MI token scope the worker uses (api://<engine-app-id>/.default)

    # ── RabbitMQ queue triggers (optional companion deploy) ───────────────────
    # When -DeployRabbitMqTriggers, after the engine deploy this calls
    # Deploy-WwQueueProcessor.ps1 ONCE PER TRIGGER FILE resolved from
    # -QueueTriggerPath / -QueueTriggerFilePath / -QueueTriggerManifestPath, so one
    # command provisions the engine AND a Container App per queue trigger. Each app is
    # autoscaled 0..Concurrency by the KEDA rabbitmq scaler (maxReplicas is derived from
    # the trigger's Concurrency; the KEDA target from its Prefetch).
    # QueueProcessorPublishPath MUST be a SEPARATE publish output from both the engine's
    # and the JobProcessor's; the plan phase resolves it and fails loudly on a collision.
    # Docs: docs/Deploy-EndToEnd-Runbook.md section 8.
    [switch] $DeployRabbitMqTriggers,
    [string] $QueueTriggerPath,                       # folder of trigger .bite files
    [string] $QueueTriggerFilter = '*.bite',
    [string] $QueueTriggerFilePath,                   # or exactly one file
    [string] $QueueTriggerManifestPath,               # or a manifest with per-trigger overrides
    [string] $QueueSourcePath,                        # folder holding {QueueSourceId}.bite
    [string] $AcaEnvironment,
    [string] $AcrName,
    [string] $QueueProcessorPublishPath,
    [string] $QueueProcessorImage,                    # or reuse a pre-built digest
    [string] $QueueEngineResourceAppId,               # engine Entra app id for the worker's token
    [string] $RabbitMqSecretUri,                      # Key Vault secret holding the broker URI (KEDA)
    [ValidateSet('Elastic', 'Fixed', 'Warm')]
    [string] $QueueScalingMode = 'Elastic',
    [switch] $ContinueOnQueueTriggerError,

    # ── Other logging / feature env vars ─────────────────────────────────────
    [nullable[bool]] $EnableConsoleLogging,
    [ValidateSet('TRACE', 'DEBUG', 'INFO', 'WARN', 'ERROR', 'FATAL', 'OFF')]
    [string] $ExecutionLogLevel = 'INFO',
    [nullable[bool]] $LicenseCheckEnabled,
    [nullable[bool]] $StructuredLogs,
    [switch] $AlignHostJsonLogLevel,      # opt-in: also rewrite host.json logLevel (host-process only)

    # RabbitMQ publish connection cache (WAREWOLF_RABBITMQ_CACHE). Reuses the AMQP connection
    # across PublishRabbitMQActivity executions instead of opening one per publish, which removes
    # ~6 of the 8 broker round-trips a publish costs. Measured 4.5x on a remote broker; a 500-row
    # recordset publish drops from ~11 min to ~2.5 min. See docs/RabbitMQ-ConnectionCache-Plan.md
    # and docs/RabbitMQ-ConnectionCache-Benchmark.md.
    #
    # ON BY DEFAULT FOR EVERY ENGINE, by explicit decision (2026-09-13): optimum RabbitMQ publish
    # throughput is wanted on every instance, not opted into per deployment. The engine itself still
    # defaults to OFF when the variable is absent - that safety net is unchanged and is what a host
    # other than this script (the on-prem server, a hand-made app) continues to get.
    #
    # Pass '' (empty) to omit the setting entirely, which leaves the engine on the uncached path it
    # has always used. Do that only to isolate a suspected caching fault; it is not the normal state.
    #
    # WAREWOLF_RABBITMQ_CACHE is the ONLY environment variable controlling this feature; all eight
    # knobs live inside it (RabbitMqCacheOptions.FileShape). Every field is stated explicitly rather
    # than left to the engine's defaults, so what a deployed app is running is readable from the
    # Function App's settings blade without cross-referencing source.
    #
    # These values are the configuration MEASURED FASTEST, not a guess:
    #   enabled + channelPool     19,262 msg/s  (0.03 ms/publish, 1 connection, 1 channel)
    #   enabled only               3.3 msg/s    (294.8 ms/publish - channel.open+close is 99.6% of it)
    #   neither                    0.7 msg/s    (1340.7 ms/publish - full handshake per message)
    #
    #   channelPool              - reuses channels instead of one open+close per publish. This is the
    #                              big one: with the connection cached it is ~99.6% of what remains.
    #   maxChannelsPerConnection - ceiling, not a target. The pool only ever grows to the peak
    #                              concurrency actually seen (measured: 1 channel sequential, 8 for
    #                              8 concurrent threads), so 32 is headroom that is never allocated.
    #   topologyTtl              - skips the two passive declares while fresh. Each expiry costs one
    #                              publish ~2 extra round-trips (measured 324.7 ms -> 481.1 ms), which
    #                              matters proportionally MORE once pooling has removed everything else.
    #   maxConnections           - per-broker connection ceiling; one broker needs one.
    #   idleTimeout/maxLifetime/sweepInterval - connection hygiene. No measurable effect on throughput;
    #                              they exist so a cached socket is released rather than held forever.
    #
    # channelPool SHIPS OFF (2026-09-14 review). RabbitMqCacheOptions still accepts it and
    # RabbitMqChannelPool is correct - the cap defect that let _live drift below zero, and with it the
    # unbounded channel growth, is fixed and covered by RabbitMqChannelPoolTests - but the plan of
    # record (docs/RabbitMQ-ConnectionCache-Plan.md, decision 2) is "connection only, a fresh IModel
    # per publish", and reversing that in the deploy default rather than in the plan is how a decision
    # nobody reviewed ends up on every engine. Flip it to true here once the plan is amended.
    #
    # Be aware of what that costs today: the benchmark above is 3.3 msg/s without the pool against
    # 19,262 msg/s with it. The connection cache alone still removes ~6 of the 8 round-trips (0.7 ->
    # 3.3 msg/s); the pool is what removes the other two, and they are 99.6% of what is left.
    [string] $RabbitMqCacheConfig = '{"enabled":true,"channelPool":false,"maxChannelsPerConnection":32,"idleTimeout":"00:05:00","maxLifetime":"00:30:00","topologyTtl":"00:00:30","maxConnections":4,"sweepInterval":"00:01:00"}',
    [nullable[int]] $ServiceBusMaxConcurrentCalls,  # opt-in: host.json serviceBus dispatch-concurrency cap (WOLF-8512)
    # OPT-IN. Pins host.json's concurrency.dynamicConcurrencyEnabled to false. REQUIRED for
    # -ServiceBusMaxConcurrentCalls to have any effect: dynamic concurrency overrides every
    # static concurrency setting in host.json (see Update-HostJsonDynamicConcurrency). Not
    # implied by -ServiceBusMaxConcurrentCalls on purpose — flipping a host-wide scaling mode
    # as a side effect of a per-binding cap is exactly the kind of unreviewed default change
    # the -RabbitMqCacheConfig note above warns about — but omitting it alongside that cap
    # WARNS at plan time.
    [switch] $DisableDynamicConcurrency,

    # ── ServiceBusTrigger tunables (WOLF-8516 — file-only, no env-var fallback) ──
    [nullable[int]] $ServiceBusTriggerExecutionTimeoutSeconds,
    [nullable[int]] $ServiceBusTriggerMaxConcurrentExecutions,
    [nullable[int]] $ServiceBusTriggerSlotWaitTimeoutSeconds,
    [nullable[int]] $ServiceBusTriggerSettlementTimeoutSeconds,

    # ── ServiceBusTrigger queue/workflow selection (WOLF-8517 — file-only, deploy-time-baked) ──
    [string] $ServiceBusTriggerQueueName,
    [string] $ServiceBusTriggerWorkflowName,

    # ── Logging output ───────────────────────────────────────────────────────
    [string] $LogDir,

    # ── Control ──────────────────────────────────────────────────────────────
    [switch] $NonInteractive,
    [switch] $DryRun,

    # Test hook: when dot-sourced with -LoadFunctionsOnly, the script defines its
    # helper functions and returns BEFORE any cloud/filesystem action, so the
    # Pester suite (Deploy-WwExecutionEngine.Tests.ps1) can unit-test the helpers.
    [switch] $LoadFunctionsOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# .NET runtime the Function App must run on
# ════════════════════════════════════════════════════════════════════════════
# These MUST track <TargetFramework> in Warewolf.Execution.Lightweight.csproj (net10.0)
# and the AzureFunctionsVersion (v4) beside it. The host does not adapt to the payload:
# a v8.0 host loading a net10.0 worker fails at startup with "You must install or update
# .NET to run this application." before a single function is indexed — the Function App
# equivalent of the base-image mismatch documented in the QueueProcessor's Dockerfile.
# The companion scripts (Deploy-WwJobProcessor.ps1, Deploy-WwExecutionServiceBusWorker.ps1)
# carry the same three constants; all four projects target net10.0, so they move together.
$script:TargetFunctionsRuntime    = 'dotnet-isolated'
$script:TargetRuntimeVersion      = '10'      # `functionapp create --runtime-version`
$script:TargetNetFrameworkVersion = 'v10.0'   # `siteConfig.netFrameworkVersion` on an existing app
$script:TargetFunctionsVersion    = '4'       # `functionapp create --functions-version`

# Resolved for real in Phase 0.5. Seeded here so Save-DeploySummary is StrictMode-safe no
# matter how early a run dies (the summary is the rollback's record of intent).
$script:RuntimePlan = [pscustomobject]@{
    Current = $null; Target = $script:TargetNetFrameworkVersion; Action = 'create'; Message = '(not yet resolved)'
}

# ════════════════════════════════════════════════════════════════════════════
# Helpers
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
    <#
        Prompt for a yes/no with a default.  In -NonInteractive mode the default
        is returned without prompting so unattended runs never block.
    #>
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
    <#
        Resolve a tri-state [nullable[bool]] feature flag:
          * explicit $true/$false  -> used as-is
          * $null + interactive     -> prompt (with $Default)
          * $null + non-interactive -> $Default
    #>
    param([string] $Name, [nullable[bool]] $Current, [bool] $Default, [string] $Prompt)
    if ($null -ne $Current) { return [bool]$Current }
    if ($NonInteractive)    { return $Default }
    $msg = if ($Prompt) { $Prompt } else { "Enable $Name" }
    return (Confirm-Yes $msg $Default)
}

function Get-MaskedValue {
    <#
        Mask a value for display/logging.  Secret settings show only a short hint.
    #>
    param([string] $Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 6) { return '******' }
    return ('{0}…(masked, len={1})' -f $Value.Substring(0, 3), $Value.Length)
}

function Format-AzArgsForLog {
    <#
        Render an az argument array as a single line, masking secret values so
        they never reach the console, the -DryRun echo, a thrown error, or the
        transcript.  A NAME=VALUE token whose NAME contains SECRET/PASSWORD/TOKEN/
        CONNECTION is redacted — except names ending in _NAME (e.g.
        KEYVAULT_SECRET_NAME, which is a name, not a secret).  Values following a
        --password/--client-secret/--secret flag are masked too.
    #>
    param([string[]] $Arguments)
    $flagRegex = '(?i)^(--password|--client-secret|--secret)$'
    # A NAME=VALUE setting is a secret only when the name's trailing token is
    # PASSWORD/SECRET/TOKEN/KEY (preceded by '_' or start) or it contains
    # CONNECTION[_]STRING.  This redacts e.g. DEBUG_PRINCIPAL_TOKEN and
    # *_SECRET, but NOT flag-like names such as SkipFailureToRetrieveSecret or
    # KEYVAULT_SECRET_NAME.
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
    $out = & az @Args 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFail) { return $null }
        throw "az CLI failed ($LASTEXITCODE): az $(Format-AzArgsForLog $Args)`n$($out | Out-String)"
    }
    return $out
}

function Invoke-ChildScript {
    <#
        Invoke a sibling script via the call operator, honouring -DryRun by
        printing the planned call.
    #>
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

function Initialize-SecureConfigCrypto {
    <#
        Compile (once) a self-contained replica of
        Dev2.Services.Security.SecurityEncryption so the script can classify,
        validate, and AES-encrypt a secure.config exactly as the engine reads it.

        MUST stay byte-compatible with
        Dev/Dev2.Infrastructure/Services/Security/SecurityEncryption.cs
        (fixed passphrase/salt/IV, PBKDF1 via PasswordDeriveBytes, AES-256-CBC,
        zero padding).  AES == Rijndael with the default 128-bit block, so Aes
        is used in place of the obsolete RijndaelManaged.
    #>
    if ('WwSecureConfigCrypto' -as [type]) { return }
    Add-Type -Language CSharp -TypeDefinition @'
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class WwSecureConfigCrypto
{
    const string InitVector = "@1B2c3D4e5F6g7H8";
    const string PassPhrase = "Pas5pr@se";
    const string SaltValue  = "s@1tValue";
    const string HashAlgorithm = "SHA1";
    const int PasswordIterations = 2;
    const int KeySize = 256;

    static byte[] DeriveKey()
    {
        var salt = Encoding.ASCII.GetBytes(SaltValue);
#pragma warning disable 612,618
        var pdb = new PasswordDeriveBytes(PassPhrase, salt, HashAlgorithm, PasswordIterations);
        return pdb.GetBytes(KeySize / 8);
#pragma warning restore 612,618
    }

    public static string Encrypt(string plainText)
    {
        var iv   = Encoding.ASCII.GetBytes(InitVector);
        var data = Encoding.UTF8.GetBytes(plainText);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;
        aes.KeySize = KeySize;
        using var enc = aes.CreateEncryptor(DeriveKey(), iv);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        var iv   = Encoding.ASCII.GetBytes(InitVector);
        var data = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;
        aes.KeySize = KeySize;
        using var dec = aes.CreateDecryptor(DeriveKey(), iv);
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);
        return sr.ReadToEnd();
    }
}
'@
}

function Test-SecureConfig {
    <#
        Classify a secure.config file and validate that the engine will be able
        to load it.  Returns a PSCustomObject:
            Kind  = 'Plaintext' | 'Encrypted' | 'Invalid'
            Valid = $true when the (decrypted) content is a usable config

        A config is "usable" when it parses as JSON containing SecretKey and/or
        WindowsGroupPermissions — the shape SecureConfigLoader deserialises.
    #>
    param([Parameter(Mandatory)][string] $Path)
    Initialize-SecureConfigCrypto

    $raw = Get-Content -LiteralPath $Path -Raw

    function Test-IsUsableJson([string] $text) {
        $clean = $text.TrimEnd([char]0).Trim()
        # Cheap pre-check: only attempt a JSON parse when the content actually
        # looks like JSON. Encrypted (Base64) / binary content would otherwise make
        # ConvertFrom-Json throw a caught-but-transcript-logged TerminatingError.
        if (-not ($clean.StartsWith('{') -or $clean.StartsWith('['))) { return $false }
        try { $obj = $clean | ConvertFrom-Json } catch { return $false }
        if ($null -eq $obj) { return $false }
        $names = @($obj.PSObject.Properties.Name)
        return ($names -contains 'SecretKey') -or ($names -contains 'WindowsGroupPermissions')
    }

    # 1. Plaintext JSON (the engine's TryDecrypt passes it through unchanged).
    if (Test-IsUsableJson $raw) {
        return [pscustomobject]@{ Kind = 'Plaintext'; Valid = $true }
    }

    # 2. AES-encrypted — must decrypt to usable JSON with the engine's fixed key.
    try {
        $decrypted = [WwSecureConfigCrypto]::Decrypt($raw)
        if (Test-IsUsableJson $decrypted) {
            return [pscustomobject]@{ Kind = 'Encrypted'; Valid = $true }
        }
        return [pscustomobject]@{ Kind = 'Encrypted'; Valid = $false }
    } catch {
        return [pscustomobject]@{ Kind = 'Invalid'; Valid = $false }
    }
}

function Protect-SecureConfig {
    <#
        AES-encrypt a plaintext secure.config (using the engine's fixed key) and
        write the Base64 result to $OutPath with no trailing newline.
    #>
    param([Parameter(Mandatory)][string] $InPath, [Parameter(Mandatory)][string] $OutPath)
    Initialize-SecureConfigCrypto
    $raw       = Get-Content -LiteralPath $InPath -Raw
    $encrypted = [WwSecureConfigCrypto]::Encrypt($raw)
    [System.IO.File]::WriteAllText($OutPath, $encrypted, (New-Object System.Text.UTF8Encoding $false))
}

function Convert-ToMelLevel {
    <#
        Map the engine's Dev2 EXECUTIONLOGLEVEL to the Microsoft.Extensions.Logging
        level used by host.json (mirrors LoggingConfiguration.MelMinimumLevel).
    #>
    param([Parameter(Mandatory)][string] $ExecutionLogLevel)
    switch ($ExecutionLogLevel.ToUpperInvariant()) {
        'TRACE' { 'Trace' }
        'DEBUG' { 'Debug' }
        'INFO'  { 'Information' }
        'WARN'  { 'Warning' }
        'ERROR' { 'Error' }
        'FATAL' { 'Critical' }
        'OFF'   { 'None' }
        default { 'Information' }
    }
}

function Read-ExecutionLogLevel {
    <#
        Prompt for EXECUTIONLOGLEVEL (validated against the Dev2 level set).
        Returns $Current unchanged in -NonInteractive mode or on empty input.
    #>
    param([string] $Current = 'INFO')
    if ($NonInteractive) { return $Current }
    $levels = @('TRACE', 'DEBUG', 'INFO', 'WARN', 'ERROR', 'FATAL', 'OFF')
    while ($true) {
        $v = Read-Host "  EXECUTIONLOGLEVEL ($($levels -join '/')) [$Current]"
        if ([string]::IsNullOrWhiteSpace($v)) { return $Current }
        $u = $v.Trim().ToUpperInvariant()
        if ($levels -contains $u) { return $u }
        Write-Host "    (invalid; choose one of $($levels -join ', '))" -ForegroundColor Yellow
    }
}

function Update-HostJsonLogLevel {
    <#
        Align host.json's logging.logLevel with the chosen EXECUTIONLOGLEVEL:
        sets "default" and every "Warewolf.Execution.Lightweight.*" category to
        the mapped MEL level, leaving Microsoft/Host framework categories intact.
        Returns the mapped level.  No-op (returns the level) when host.json has no
        logging.logLevel section.
    #>
    param([Parameter(Mandatory)][string] $HostJsonPath, [Parameter(Mandatory)][string] $ExecutionLogLevel)
    $mel  = Convert-ToMelLevel $ExecutionLogLevel
    $json = Get-Content -LiteralPath $HostJsonPath -Raw | ConvertFrom-Json
    $logging = $json.PSObject.Properties['logging']
    if (-not $logging -or -not $logging.Value.PSObject.Properties['logLevel']) { return $mel }
    $ll = $logging.Value.logLevel
    $ll.default = $mel
    foreach ($name in @($ll.PSObject.Properties.Name)) {
        if ($name -like 'Warewolf.Execution.Lightweight.*') { $ll.$name = $mel }
    }
    $json | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $HostJsonPath -Encoding UTF8
    return $mel
}

function Update-HostJsonServiceBusConcurrency {
    <#
        Set/overwrite host.json's extensions.serviceBus.maxConcurrentCalls — bounds how many
        Service Bus messages the Functions host dispatches to the trigger concurrently, ahead
        of and independent from ServiceBusTriggerOptions.MaxConcurrentExecutions' own semaphore
        gate inside the trigger's own code (see WOLF-8512, docs/ShovelBridge-Architecture.md).
        No-op (returns $false) when host.json has no extensions.serviceBus section. Preserves
        every other property already under extensions.serviceBus (autoCompleteMessages,
        maxAutoLockRenewalDuration, ...).
    #>
    param([Parameter(Mandatory)][string] $HostJsonPath, [Parameter(Mandatory)][int] $MaxConcurrentCalls)
    $json = Get-Content -LiteralPath $HostJsonPath -Raw | ConvertFrom-Json
    $extensions = $json.PSObject.Properties['extensions']
    if (-not $extensions -or -not $extensions.Value.PSObject.Properties['serviceBus']) { return $false }
    $sb = $extensions.Value.serviceBus
    if ($sb.PSObject.Properties['maxConcurrentCalls']) {
        $sb.maxConcurrentCalls = $MaxConcurrentCalls
    } else {
        $sb | Add-Member -NotePropertyName 'maxConcurrentCalls' -NotePropertyValue $MaxConcurrentCalls
    }
    $json | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $HostJsonPath -Encoding UTF8
    return $true
}

function Update-HostJsonDynamicConcurrency {
    <#
        Set host.json's concurrency.dynamicConcurrencyEnabled.

        WHY THIS EXISTS. Azure Functions' DYNAMIC CONCURRENCY OVERRIDES EVERY STATIC
        CONCURRENCY SETTING IN host.json, extensions.serviceBus.maxConcurrentCalls included.
        While dynamicConcurrencyEnabled is true, -ServiceBusMaxConcurrentCalls is written to
        the file, loads correctly, and is then ignored by the host — the cap silently does not
        apply. Measured on wwuat3 2026-09-15: a deploy carrying maxConcurrentCalls=2 AND
        MaxConcurrentExecutions=2 ran at "Host.Functions.ServiceBusWorkflowTrigger
        (Concurrency: 170)", which saturated the 1-vCore serverless SQL database behind the
        workflow ("The request limit for the database is 120 and has been reached" x10,646 in
        two minutes) and dead-lettered the run.

        Unlike Update-HostJsonServiceBusConcurrency — which no-ops when extensions.serviceBus
        is absent, because absent there means "this binding is not configured" — a MISSING
        concurrency block is CREATED. Its absence means "whatever the Functions host defaults
        to", not "not applicable", and the entire point of this call is to pin the value
        rather than inherit it. Preserves snapshotPersistenceEnabled and any sibling property.
    #>
    param([Parameter(Mandatory)][string] $HostJsonPath, [Parameter(Mandatory)][bool] $Enabled)
    $json = Get-Content -LiteralPath $HostJsonPath -Raw | ConvertFrom-Json
    if ($json.PSObject.Properties['concurrency']) {
        $c = $json.concurrency
        if ($c.PSObject.Properties['dynamicConcurrencyEnabled']) {
            $c.dynamicConcurrencyEnabled = $Enabled
        } else {
            $c | Add-Member -NotePropertyName 'dynamicConcurrencyEnabled' -NotePropertyValue $Enabled
        }
    } else {
        $json | Add-Member -NotePropertyName 'concurrency' `
                           -NotePropertyValue ([pscustomobject]@{ dynamicConcurrencyEnabled = $Enabled })
    }
    $json | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $HostJsonPath -Encoding UTF8
    return $true
}

function Save-DeploySummary {
    <#
        Write the run summary to $summaryPath — CRASH-SAFELY and INCREMENTALLY.
        Called after every phase AND from the outer catch, so that NO MATTER WHERE
        the run stops (including a mid-phase failure such as the Phase 4 deploy) the
        rollback always has an authoritative record of what THIS run created vs.
        found pre-existing.  Writes atomically (temp file -> Move-Item) so a crash
        during the write itself cannot leave a truncated/corrupt summary.

        Status: in-progress | completed | failed.  lastPhase + error let an operator
        (and the rollback) recognise a partial/failed run.

        Reads the run-state script variables in scope at call time; all of them are
        initialised before the first call, so this is safe under Set-StrictMode.
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
        authProvisioned = (-not $SkipAuthProvisioning)
        entraAppDisplayName = ($SkipAuthProvisioning ? $null : "$AppName-auth")
        secureConfig    = ($SecureConfigPath ? $secureConfigKind : 'none')
        licenseConfig   = ($LicenseConfigPath ? 'staged' : 'none')
        workflowsSource = $WorkflowsSourcePath
        encryptResources = $doEncryptResources
        verifyDecryption = [bool]$VerifyDecryption
        elasticsearch   = $enableEs
        performanceCounters = $enablePerfCounters
        dynamicConcurrency  = (-not $DisableDynamicConcurrency)
        functionAppScaleLimit = $FunctionAppScaleLimit
        persistence     = $enablePersistence
        persistenceSettings = ($enablePersistence ? $PersistenceSettingsPath : $null)
        persistenceDbSource = ($enablePersistence ? $PersistenceDbSourcePath : $null)
        deployJobProcessor  = [bool]$DeployJobProcessor
        jobProcessorAppName = ($DeployJobProcessor ? $JobProcessorAppName : $null)
        deployServiceBusWorker  = [bool]$DeployServiceBusWorker
        serviceBusWorkerAppName = ($DeployServiceBusWorker ? $ServiceBusWorkerAppName : $null)
        serviceBusWorkerWorkflowName = ($DeployServiceBusWorker ? $ServiceBusWorkerWorkflowName : $null)
        deployRabbitMqTriggers = [bool]$DeployRabbitMqTriggers
        queueProcessorApps  = ($DeployRabbitMqTriggers ? $script:QueueProcessorApps : $null)
        keyVault        = ($kvRequired ? @{ name = $KeyVaultName; secret = $KeyVaultSecretName } : $null)
        serviceBusTrigger = ($triggerConfigRequired ? @{ queueName = $ServiceBusTriggerQueueName; workflowName = $ServiceBusTriggerWorkflowName } : $null)
        appSettings     = $maskedSettings
    }
    # Atomic write: render to a temp sibling, then replace — a crash mid-write
    # can never leave the rollback a half-written summary.
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# ════════════════════════════════════════════════════════════════════════════
# Path resolution
# ════════════════════════════════════════════════════════════════════════════

$ScriptDir   = $PSScriptRoot

$ConfigureAuthScript = Join-Path $ScriptDir 'Configure-WwExecutionAuth.ps1'
$AppInsightsScript   = Join-Path $ScriptDir 'Setup-ApplicationInsights.ps1'
$EncryptScript       = Join-Path $ScriptDir 'Encrypt-Config.ps1'
$WorkflowIndexScript = Join-Path $ScriptDir 'Generate-WorkflowIndex.ps1'
$AuthOutputPath      = Join-Path $ScriptDir 'Configure-WwExecutionAuth.output.json'

$ElasticsearchBiteName = 'ElasticsearchLoggingSource.bite'

# Suspend/resume persistence pair (staged into Settings\ exactly like the ES source).
$PersistenceSettingsName = 'persistencesettings.json'
$PersistenceDbSourceName = 'persistencesettingsdbsource.bite'

# Companion JobProcessor deploy (invoked only when -DeployJobProcessor).
$JobProcessorScript = Join-Path $ScriptDir 'Deploy-WwJobProcessor.ps1'

# Companion ServiceBusWorker deploy (invoked only when -DeployServiceBusWorker).
$ServiceBusWorkerScript = Join-Path $ScriptDir 'Deploy-WwExecutionServiceBusWorker.ps1'

# Companion QueueProcessor deploy (invoked only when -DeployRabbitMqTriggers), once per
# resolved trigger file.
$QueueProcessorScript = Join-Path $ScriptDir 'Deploy-WwQueueProcessor.ps1'

# Test hook: stop here when only the helper functions are wanted (Pester).
function Grant-RoleAssignment {
    <#
        Creates a role assignment, retrying while Microsoft Entra replicates.

        WHY --assignee-object-id AND --assignee-principal-type, NOT --assignee:
        `--assignee` makes az RESOLVE the principal through Microsoft Graph first. A
        system-assigned managed identity enabled seconds earlier is not yet visible there, so the
        call fails with

            Cannot find user or service principal in graph database for '<principalId>'.
            If the assignee is an appId, make sure the corresponding service principal is created
            with 'az ad sp create --id <principalId>'

        The advice in that message is a red herring here - the service principal DOES exist, it has
        simply not replicated. Passing the object id and its type skips the lookup entirely, which
        is what Deploy-WwQueueProcessor.ps1's Assert-RoleAssigned already does.

        The write itself can still fail transiently, hence the retry.
    #>
    param(
        [Parameter(Mandatory)][string] $PrincipalId,
        [Parameter(Mandatory)][string] $Role,
        [Parameter(Mandatory)][string] $Scope,
        [ValidateSet('ServicePrincipal', 'User', 'Group')]
        [string] $PrincipalType = 'ServicePrincipal',
        [string] $Label,
        [int]    $MaxAttempts   = 10,
        [int]    $DelaySeconds  = 15
    )
    $what = if ($Label) { $Label } else { "$PrincipalType $PrincipalId" }
    $azArgs = @('role', 'assignment', 'create',
                '--role', $Role,
                '--assignee-object-id', $PrincipalId,
                '--assignee-principal-type', $PrincipalType,
                '--scope', $Scope, '-o', 'json')

    if ($DryRun) {
        Invoke-Az $azArgs -Mutating | Out-Null      # echoes the [DRYRUN] line
        Write-Ok "Would grant '$Role' to $what."
        return
    }

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $res = Invoke-Az $azArgs -Mutating -AllowFail
        if ($res) {
            Write-Ok ("Granted '{0}' to {1}{2}." -f $Role, $what,
                      $(if ($attempt -gt 1) { " (attempt $attempt)" } else { '' }))
            return
        }
        if ($attempt -lt $MaxAttempts) {
            Write-Note ("Role assignment attempt {0}/{1} failed - Entra replication lag; retrying in {2}s." -f
                        $attempt, $MaxAttempts, $DelaySeconds)
            Start-Sleep -Seconds $DelaySeconds
        }
    }
    throw (("Failed to grant '{0}' on '{1}' to {2} after {3} attempts ({4}s). " +
            "If the identity was only just created, re-running the deploy usually succeeds.") -f
           $Role, $Scope, $what, $MaxAttempts, ($MaxAttempts * $DelaySeconds))
}

function Resolve-CurrentPrincipal {
    <#
        Object id + RBAC principal type of whoever `az` is currently logged in as,
        or $null when it cannot be resolved.

        WHY NOT `az ad signed-in-user show`: that is a Microsoft Graph /me call, valid
        ONLY for a DELEGATED (interactive user) token. Under `az login --service-principal`
        - how Dev/.azure/pipeline-LOADTEST.yml authenticates its agent - Graph rejects it:

            /me request is only valid with delegated authentication flow.

        which is exactly what failed build 30696, the first run to reach this path after
        -EncryptResources was switched on. `az account show` reports WHICH KIND of identity
        holds the session (user.type), so branch on that and look the object id up the way
        that identity supports.

        Returns $null rather than throwing when the lookup fails (e.g. a service principal
        without the Graph directory read needed to look ITSELF up, or a managed-identity
        login whose user.name is not a real appId) - the caller decides whether that is fatal.
    #>
    $acctJson = Invoke-Az @('account', 'show', '-o', 'json') -AllowFail
    if (-not $acctJson) { return $null }
    $user = ($acctJson -join "`n" | ConvertFrom-Json).user
    if (-not $user) { return $null }

    if ($user.type -eq 'servicePrincipal') {
        # user.name is the appId (client id) for an SP login; --id accepts it.
        $oid = (Invoke-Az @('ad', 'sp', 'show', '--id', $user.name, '--query', 'id', '-o', 'tsv') -AllowFail) -join ''
        if (-not $oid) { return $null }
        return @{ ObjectId = $oid.Trim(); PrincipalType = 'ServicePrincipal'; Display = "the service principal '$($user.name)'" }
    }

    $oid = (Invoke-Az @('ad', 'signed-in-user', 'show', '--query', 'id', '-o', 'tsv') -AllowFail) -join ''
    if (-not $oid) { return $null }
    return @{ ObjectId = $oid.Trim(); PrincipalType = 'User'; Display = "the signed-in user ($($user.name))" }
}

function Resolve-FunctionRuntimePlan {
    <#
        Decide what THIS run does about the Function App's .NET runtime, from the
        app's CURRENT netFrameworkVersion.  Pure — no az calls — so the decision is
        made once at plan time (Phase 0.5), printed, and then replayed in Phase 1.

        $CurrentNetFrameworkVersion is $null/empty in TWO very different situations, and
        -AppExists is what separates them: the app does not exist yet, or the app DOES
        exist but `functionapp show` returned a payload whose siteConfig.netFrameworkVersion
        could not be read.  Conflating them is why this used to report success on an app
        whose runtime it had never determined — see 'unknown' below.

          Action 'create'  - no app yet; `functionapp create` pins the target runtime.
          Action 'none'    - already on the target.
          Action 'upgrade' - mismatch AND -UpgradeRuntimeVersion; Phase 1 will run
                             `functionapp config set --net-framework-version`.
          Action 'warn'    - mismatch WITHOUT the switch; report loudly, change nothing.
          Action 'unknown' - the app exists but its runtime could not be read.  Report it and
                             change nothing.  NEVER claim the app is on the target: `functionapp
                             create` is not going to run for an existing app, so an unread
                             runtime is an UNRECONCILED runtime, and saying otherwise produces
                             exactly the deploy-reports-success/engine-dies-at-startup outcome
                             this whole check exists to prevent.

        Why 'warn' and not 'upgrade' by default: `config set` restarts the app, and a
        restart is not something a deploy should do to a running environment on its own
        inference.  The cost of the warning path is explicit — the run continues and the
        engine will fail at startup — so the message has to say exactly that.
    #>
    param(
        [string] $CurrentNetFrameworkVersion,
        [switch] $Upgrade,
        [switch] $AppExists,
        [string] $TargetNetFrameworkVersion = $script:TargetNetFrameworkVersion
    )
    $current = if ([string]::IsNullOrWhiteSpace($CurrentNetFrameworkVersion)) { $null }
               else { $CurrentNetFrameworkVersion.Trim() }

    $action = if (-not $current -and $AppExists)                        { 'unknown' }
              elseif (-not $current)                                    { 'create'  }
              elseif ($current -ieq $TargetNetFrameworkVersion)         { 'none'    }
              elseif ($Upgrade)                                         { 'upgrade' }
              else                                                      { 'warn'    }

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

if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) is not installed or not on PATH. Install: https://aka.ms/installazurecliwindows'
}

# NOTE: capture first, then parse — piping a $null (failed/not-logged-in) result
# straight into ConvertFrom-Json throws "Cannot bind argument ... because it is null".
$accountJson = Invoke-Az @('account', 'show', '-o', 'json') -AllowFail
$account = if ($accountJson) { $accountJson | ConvertFrom-Json } else { $null }
if (-not $account) {
    throw "Not logged in to Azure CLI. Run 'az login' first."
}
Write-Ok "Azure account: $($account.user.name) | Subscription: $($account.name)"

if (-not $SubscriptionId) { $SubscriptionId = $account.id }
if (-not $TenantId)       { $TenantId       = $account.tenantId }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0.5 — PLAN  (resolve every decision; no cloud/state change yet)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0.5  Plan (resolve all settings before any change)'

# Run identity + tags so Rollback-WwExecutionEngine.ps1 can target ONLY what this
# run created (and never a pre-existing resource).  $created records, per resource,
# whether THIS run created it ($true) or found it already present ($false).
$runId        = "wwx-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$ResourceTags = @("wwx-test-run=$runId", 'wwx-purpose=exec-engine-live-test')
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
$publishItem      = Get-Item -LiteralPath $PublishPath
$publishIsZip     = (-not $publishItem.PSIsContainer) -and ($publishItem.Extension -ieq '.zip')
if ($publishItem.PSIsContainer) {
    $PublishDir = $publishItem.FullName
} elseif ($publishIsZip) {
    $PublishDir = Join-Path $publishItem.DirectoryName $publishItem.BaseName
} else {
    throw "PublishPath must be a folder or a .zip file: $PublishPath"
}

if (-not $LogDir) { $LogDir = Join-Path (Split-Path $PublishDir -Parent) 'deploy-logs' }

# ── Feature toggles ────────────────────────────────────────────────────────────
$enableAppInsights   = Resolve-Toggle -Name 'EnableAppInsights'   -Current $EnableAppInsights   -Default $true  -Prompt 'Provision + enable Application Insights?'
$enableConsole       = Resolve-Toggle -Name 'EnableConsoleLogging' -Current $EnableConsoleLogging -Default $true  -Prompt 'Enable console logging?'
$enableEs            = Resolve-Toggle -Name 'EnableElasticsearch' -Current $EnableElasticsearch -Default $false -Prompt 'Enable Elasticsearch logging?'
$enablePerfCounters  = Resolve-Toggle -Name 'EnablePerformanceCounters' -Current $EnablePerformanceCounters -Default $false -Prompt 'Register the Application Insights PerformanceCollectorModule (process memory/CPU counters)?'
$enablePersistence   = Resolve-Toggle -Name 'EnablePersistence'   -Current $EnablePersistence   -Default $false -Prompt 'Enable suspend/resume persistence (Hangfire) — stage the persistence settings pair?'
$doEncryptResources  = Resolve-Toggle -Name 'EncryptResources'    -Current $EncryptResources    -Default $false -Prompt 'Encrypt ALL sources (workflows + Elasticsearch + others) now? (encrypt once; leave off if already encrypted)'
$licenseCheck        = Resolve-Toggle -Name 'LicenseCheckEnabled' -Current $LicenseCheckEnabled  -Default $true  -Prompt 'Enable license/subscription check?'
$structuredLogs      = Resolve-Toggle -Name 'StructuredLogs'      -Current $StructuredLogs       -Default $true  -Prompt 'Structured (JSON) console logs?'

# Security toggles below are intentionally NOT exposed: they are always disabled.
#   BYPASS_SECURE_CONFIG / WAREWOLF_SUPER_ADMIN_ENABLED / SkipFailureToRetrieveSecret.

# EXECUTIONLOGLEVEL — prompt when not explicitly supplied (drives both the app
# setting and the host.json logLevel alignment below).
if (-not $PSBoundParameters.ContainsKey('ExecutionLogLevel')) {
    $ExecutionLogLevel = Read-ExecutionLogLevel -Current $ExecutionLogLevel
}

# ── Key Vault requirement (decoupled from "this run encrypts") ──────────────────
# KV wiring (app settings + the Function App MI's "Secrets User" role) is needed
# whenever a vault is in play, so the engine can DECRYPT WFAES sources at runtime:
#   * -EncryptResources -> this run (re)encrypts; the vault is mandatory, and
#   * -KeyVaultName supplied (without encrypting) -> sources are ALREADY encrypted
#     and the engine still needs the vault at runtime.
# Encryption itself (dev "Secrets Officer" role + key generation + verification) is
# gated separately on $doEncryptResources.
$kvRequired = $doEncryptResources -or (-not [string]::IsNullOrWhiteSpace($KeyVaultName))
if ($kvRequired) {
    $KeyVaultName       = Read-Required -Name 'KeyVaultName'       -Current $KeyVaultName       -Hint 'Key Vault the engine decrypts WFAES sources with (3-24 chars)'
    $KeyVaultSecretName = Read-Required -Name 'KeyVaultSecretName' -Current $KeyVaultSecretName -Hint 'AES key secret name'
}

# ── ServiceBusTrigger tunables (decoupled from Key Vault — WOLF-8516) ───────────
# Staged into the SAME executionengine.settings.json file as keyVaultName/
# keyVaultSecretName (Phase 3.5c below) whenever ANY of these is supplied, since
# ServiceBusTriggerOptions now reads them SOLELY from that file (no env-var
# fallback) — independent of whether this deploy also requires a Key Vault.
$sbtOverrides = [ordered]@{
    executionTimeoutSeconds  = $ServiceBusTriggerExecutionTimeoutSeconds
    maxConcurrentExecutions  = $ServiceBusTriggerMaxConcurrentExecutions
    slotWaitTimeoutSeconds   = $ServiceBusTriggerSlotWaitTimeoutSeconds
    settlementTimeoutSeconds = $ServiceBusTriggerSettlementTimeoutSeconds
}
$sbtRequired = @($sbtOverrides.Values | Where-Object { $null -ne $_ }).Count -gt 0

# ── ServiceBusTrigger queue/workflow selection (WOLF-8517 — corrected) ─────────
# Staged into Settings/servicebustrigger.bite (Phase 3 below), replacing WOLF-8517's
# original post-deploy Kudu VFS write (Enable-ServiceBusSecureTrigger.ps1's now-removed
# Phase 4), which cannot work once WEBSITE_RUN_FROM_PACKAGE=1 mounts wwwroot read-only —
# confirmed live against WarewolfServer-UAT (2026-09-12): UnauthorizedAccessException.
# Required together or not at all: ServiceBusTriggerDefinition.Load() refuses to start an
# enabled trigger with either field blank, so failing at plan time is cheaper than
# discovering it at cold start.
$triggerConfigProvided = @(@($ServiceBusTriggerQueueName, $ServiceBusTriggerWorkflowName) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($triggerConfigProvided.Count -eq 1) {
    throw "-ServiceBusTriggerQueueName and -ServiceBusTriggerWorkflowName must be supplied together (or neither) — got only $(if ([string]::IsNullOrWhiteSpace($ServiceBusTriggerQueueName)) { '-ServiceBusTriggerWorkflowName' } else { '-ServiceBusTriggerQueueName' }). ServiceBusTriggerDefinition.Load() refuses to start an enabled trigger with either field blank."
}
$triggerConfigRequired = $triggerConfigProvided.Count -eq 2

# ── Elasticsearch source (user-supplied, exact filename) ───────────────────────
if ($enableEs) {
    $ElasticsearchSourcePath = Read-Required -Name 'ElasticsearchSourcePath' -Current $ElasticsearchSourcePath -Hint "path to $ElasticsearchBiteName"
    if (-not (Test-Path -LiteralPath $ElasticsearchSourcePath -PathType Leaf)) {
        throw "ElasticsearchSourcePath not found (must be a file): $ElasticsearchSourcePath"
    }
    $esLeaf = Split-Path $ElasticsearchSourcePath -Leaf
    if ($esLeaf -ine $ElasticsearchBiteName) {
        throw "Elasticsearch source must be named exactly '$ElasticsearchBiteName' (the engine reads that exact path); got '$esLeaf'."
    }
}

# ── Persistence settings pair (prompted when enabled; exact filenames) ─────────
if ($enablePersistence) {
    $PersistenceSettingsPath = Read-Required -Name 'PersistenceSettingsPath' -Current $PersistenceSettingsPath -Hint "path to $PersistenceSettingsName"
    if (-not (Test-Path -LiteralPath $PersistenceSettingsPath -PathType Leaf)) {
        throw "PersistenceSettingsPath not found (must be a file): $PersistenceSettingsPath"
    }
    $psLeaf = Split-Path $PersistenceSettingsPath -Leaf
    if ($psLeaf -ine $PersistenceSettingsName) {
        throw "Persistence settings must be named exactly '$PersistenceSettingsName' (the engine reads that exact path); got '$psLeaf'."
    }
    $PersistenceDbSourcePath = Read-Required -Name 'PersistenceDbSourcePath' -Current $PersistenceDbSourcePath -Hint "path to $PersistenceDbSourceName"
    if (-not (Test-Path -LiteralPath $PersistenceDbSourcePath -PathType Leaf)) {
        throw "PersistenceDbSourcePath not found (must be a file): $PersistenceDbSourcePath"
    }
    $dbLeaf = Split-Path $PersistenceDbSourcePath -Leaf
    if ($dbLeaf -ine $PersistenceDbSourceName) {
        throw "Persistence DbSource must be named exactly '$PersistenceDbSourceName' (the engine reads that exact path); got '$dbLeaf'."
    }
}

# ── JobProcessor companion — its publish output MUST differ from the engine's ──
# The processor is a SEPARATE Function App built from a DIFFERENT csproj
# (Warewolf.Execution.EngineJobProcessor). Sharing a publish/upload directory with
# the engine would zip the engine's binaries and upload them to the processor app —
# a silently-wrong deploy. Resolve + validate the processor publish path up-front so
# it fails at PLAN time (before the engine is even deployed), not deep in the child.
if ($DeployJobProcessor) {
    $JobProcessorPublishPath = Read-Required -Name 'JobProcessorPublishPath' -Current $JobProcessorPublishPath -Hint 'folder or .zip of the JobProcessor Release publish output — MUST differ from the engine PublishPath'
    if (-not (Test-Path -LiteralPath $JobProcessorPublishPath)) {
        throw "JobProcessorPublishPath not found: $JobProcessorPublishPath"
    }
    $jpItem = Get-Item -LiteralPath $JobProcessorPublishPath
    $jpDir  =
        if     ($jpItem.PSIsContainer)         { $jpItem.FullName }
        elseif ($jpItem.Extension -ieq '.zip') { Join-Path $jpItem.DirectoryName $jpItem.BaseName }
        else   { throw "JobProcessorPublishPath must be a folder or a .zip file: $JobProcessorPublishPath" }
    $engineFull = ([System.IO.Path]::GetFullPath($PublishDir)).TrimEnd('\','/')
    $jpFull     = ([System.IO.Path]::GetFullPath($jpDir)).TrimEnd('\','/')
    if ($jpFull -ieq $engineFull) {
        throw ("JobProcessorPublishPath resolves to the SAME directory as the engine PublishPath ('$engineFull'). " +
               "The processor is a different Function App built from Warewolf.Execution.EngineJobProcessor and MUST publish to its own directory. " +
               "Publish it separately, e.g. dotnet publish Warewolf.Execution.EngineJobProcessor -c Release -o <different-path>.")
    }
}

# ── ServiceBusWorker companion — its publish output MUST differ from the engine's ──
# The worker is a SEPARATE Function App built from a DIFFERENT csproj
# (Warewolf.Execution.ServiceBusWorker). Sharing a publish/upload directory with
# the engine would zip the engine's binaries and upload them to the worker app —
# a silently-wrong deploy. Resolve + validate the worker publish path up-front so
# it fails at PLAN time (before the engine is even deployed), not deep in the child.
if ($DeployServiceBusWorker) {
    $ServiceBusWorkerPublishPath = Read-Required -Name 'ServiceBusWorkerPublishPath' -Current $ServiceBusWorkerPublishPath -Hint 'folder or .zip of the ServiceBusWorker Release publish output — MUST differ from the engine PublishPath'
    if (-not (Test-Path -LiteralPath $ServiceBusWorkerPublishPath)) {
        throw "ServiceBusWorkerPublishPath not found: $ServiceBusWorkerPublishPath"
    }
    $sbwItem = Get-Item -LiteralPath $ServiceBusWorkerPublishPath
    $sbwDir  =
        if     ($sbwItem.PSIsContainer)         { $sbwItem.FullName }
        elseif ($sbwItem.Extension -ieq '.zip') { Join-Path $sbwItem.DirectoryName $sbwItem.BaseName }
        else   { throw "ServiceBusWorkerPublishPath must be a folder or a .zip file: $ServiceBusWorkerPublishPath" }
    $engineFull2 = ([System.IO.Path]::GetFullPath($PublishDir)).TrimEnd('\','/')
    $sbwFull     = ([System.IO.Path]::GetFullPath($sbwDir)).TrimEnd('\','/')
    if ($sbwFull -ieq $engineFull2) {
        throw ("ServiceBusWorkerPublishPath resolves to the SAME directory as the engine PublishPath ('$engineFull2'). " +
               "The worker is a different Function App built from Warewolf.Execution.ServiceBusWorker and MUST publish to its own directory. " +
               "Publish it separately, e.g. dotnet publish Warewolf.Execution.ServiceBusWorker -c Release -o <different-path>.")
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Plan-time validation for the RabbitMQ queue-trigger companion deploy.
# Runs BEFORE the engine is deployed so an operator sees the whole fan-out (and any
# mistake) up front rather than after the engine is already live.
# ─────────────────────────────────────────────────────────────────────────────
$script:QueueTriggerFiles  = @()
$script:QueueProcessorApps = @()

if ($DeployRabbitMqTriggers) {
    if (-not (Test-Path -LiteralPath $QueueProcessorScript)) {
        throw "Deploy-WwQueueProcessor.ps1 not found at '$QueueProcessorScript'."
    }

    $modeCount = @([bool]$QueueTriggerPath, [bool]$QueueTriggerFilePath, [bool]$QueueTriggerManifestPath |
                   Where-Object { $_ }).Count
    if ($modeCount -gt 1) {
        throw ('-QueueTriggerPath, -QueueTriggerFilePath and -QueueTriggerManifestPath are mutually ' +
               'exclusive; pass exactly one.')
    }

    # Resolve the trigger set now, so "zero triggers" fails at plan time.
    if ($QueueTriggerFilePath) {
        if (-not (Test-Path -LiteralPath $QueueTriggerFilePath)) {
            throw "QueueTriggerFilePath not found: '$QueueTriggerFilePath'."
        }
        $script:QueueTriggerFiles = @((Get-Item -LiteralPath $QueueTriggerFilePath).FullName)
    }
    elseif ($QueueTriggerManifestPath) {
        if (-not (Test-Path -LiteralPath $QueueTriggerManifestPath)) {
            throw "QueueTriggerManifestPath not found: '$QueueTriggerManifestPath'."
        }
        $qpManifest = Get-Content -LiteralPath $QueueTriggerManifestPath -Raw | ConvertFrom-Json
        $script:QueueTriggerFiles = @($qpManifest.triggers | ForEach-Object { $_.file })
        if ($script:QueueTriggerFiles.Count -eq 0) {
            throw "Trigger manifest '$QueueTriggerManifestPath' declares no triggers."
        }
    }
    else {
        $QueueTriggerPath = Read-Required -Name 'QueueTriggerPath' -Current $QueueTriggerPath `
                                          -Hint 'folder containing the queue-trigger .bite files'
        if (-not (Test-Path -LiteralPath $QueueTriggerPath)) {
            throw "QueueTriggerPath not found: '$QueueTriggerPath'."
        }
        $script:QueueTriggerFiles = @(Get-ChildItem -LiteralPath $QueueTriggerPath -Filter $QueueTriggerFilter -File |
                                      Sort-Object Name | Select-Object -ExpandProperty FullName)
        if ($script:QueueTriggerFiles.Count -eq 0) {
            throw ("No trigger files matching '$QueueTriggerFilter' were found in '$QueueTriggerPath'. " +
                   'Refusing to run a queue-trigger deploy that would deploy nothing.')
        }
    }

    # An unsubstituted release token means maxReplicas would be derived from a token, so the
    # app would silently get the wrong capacity. Fail here, not in the child.
    foreach ($qpFile in $script:QueueTriggerFiles) {
        $qpRaw = Get-Content -LiteralPath $qpFile -Raw
        if ($qpRaw -match '#\{') {
            throw ("Trigger file '$qpFile' still contains an unsubstituted release token ('#{...'). " +
                   'The release pipeline must substitute Concurrency before the deploy runs.')
        }
    }

    $QueueSourcePath = Read-Required -Name 'QueueSourcePath' -Current $QueueSourcePath `
                                     -Hint 'folder holding the {QueueSourceId}.bite RabbitMQ source files'
    $AcaEnvironment  = Read-Required -Name 'AcaEnvironment' -Current $AcaEnvironment `
                                     -Hint 'Container Apps environment for the queue workers'

    # Publish-path isolation: the worker is a different project (and a Linux container), so
    # sharing a publish directory with the engine or the JobProcessor would ship the wrong bits.
    if (-not $QueueProcessorImage) {
        $QueueProcessorPublishPath = Read-Required -Name 'QueueProcessorPublishPath' `
            -Current $QueueProcessorPublishPath `
            -Hint 'folder of the Warewolf.Execution.QueueProcessor publish output (or pass -QueueProcessorImage)'
        if (-not (Test-Path -LiteralPath $QueueProcessorPublishPath)) {
            throw "QueueProcessorPublishPath not found: $QueueProcessorPublishPath"
        }
        $AcrName = Read-Required -Name 'AcrName' -Current $AcrName -Hint 'Azure Container Registry name'

        $qpFull = ([System.IO.Path]::GetFullPath($QueueProcessorPublishPath)).TrimEnd('\', '/')
        $engineFullForQp = ([System.IO.Path]::GetFullPath($PublishDir)).TrimEnd('\', '/')
        if ($qpFull -ieq $engineFullForQp) {
            throw ("QueueProcessorPublishPath resolves to the SAME directory as the engine PublishPath " +
                   "('$engineFullForQp'). The queue worker is a different project " +
                   '(Warewolf.Execution.QueueProcessor) and MUST publish to its own directory.')
        }
        if ($DeployJobProcessor -and $JobProcessorPublishPath) {
            $jpFullForQp = ([System.IO.Path]::GetFullPath($JobProcessorPublishPath)).TrimEnd('\', '/')
            if ($qpFull -ieq $jpFullForQp) {
                throw ("QueueProcessorPublishPath resolves to the SAME directory as " +
                       "JobProcessorPublishPath ('$jpFullForQp'). Each app must publish separately.")
            }
        }
    }
}

# ── secure.config classification (validate now, before any change) ─────────────
$secureConfigKind = $null
if ($SecureConfigPath) {
    if (-not (Test-Path -LiteralPath $SecureConfigPath -PathType Leaf)) {
        throw "SecureConfigPath not found: $SecureConfigPath"
    }
    $sc = Test-SecureConfig -Path $SecureConfigPath
    $secureConfigKind = $sc.Kind
    if ($sc.Kind -eq 'Invalid' -or (-not $sc.Valid)) {
        throw ("secure.config at '$SecureConfigPath' cannot be loaded by the engine: it is neither valid " +
               "plaintext JSON nor AES-decryptable to a usable config (needs SecretKey / WindowsGroupPermissions). " +
               "Re-export it from Warewolf Studio or supply a valid file.")
    }
}

# ── Warewolf License.secureconfig (prompted; optional — copied to the publish root) ──
# https://warewolf.io/knowledge-base/articles/security-encryption/
if (-not $LicenseConfigPath -and -not $NonInteractive) {
    $resp = Read-Host "  Path to 'Warewolf License.secureconfig' (Enter to skip)"
    if (-not [string]::IsNullOrWhiteSpace($resp)) { $LicenseConfigPath = $resp.Trim() }
}
if ($LicenseConfigPath -and -not (Test-Path -LiteralPath $LicenseConfigPath -PathType Leaf)) {
    throw "LicenseConfigPath not found (must be a file): $LicenseConfigPath"
}

# ── Build the environment-variable (app settings) map ──────────────────────────
$appSettings = [ordered]@{}
$secretSettingNames = New-Object System.Collections.Generic.HashSet[string]

# ASPNETCORE_ENVIRONMENT is FIXED to Production (never user-configurable).
$appSettings['ASPNETCORE_ENVIRONMENT']        = 'Production'
$appSettings['EXECUTIONLOGLEVEL']             = $ExecutionLogLevel
# WOLF-8516: ENABLECONSOLELOGGING/STRUCTURED_LOGS/ENABLEAPPLICATIONINSIGHTS/
# ENABLEELASTICSEARCHLOGGING/ENABLEPERFORMANCECOUNTERS merged into one JSON app
# setting (WAREWOLF_LOGGING_CONFIG) — see Logging/LoggingConfiguration.cs. Rotating
# several logging knobs together is now one atomic `az functionapp config
# appsettings set` call instead of five.
$loggingConfigValue = [ordered]@{
    console             = [bool]$enableConsole
    appInsights         = [bool]$enableAppInsights
    elasticsearch       = [bool]$enableEs
    performanceCounters = [bool]$enablePerfCounters
    structuredLogs      = [bool]$structuredLogs
}
$appSettings['WAREWOLF_LOGGING_CONFIG']       = ($loggingConfigValue | ConvertTo-Json -Compress)
$appSettings['WAREWOLF_LICENSE_CHECK_ENABLED'] = ($licenseCheck ? 'true' : 'false')

# RabbitMQ publish connection cache. Omitted entirely when -RabbitMqCacheConfig is empty, because
# an ABSENT variable is the engine's documented "uncached" default - writing '{"enabled":false}'
# would mean the same thing but leave a misleading setting on the app.
if (-not [string]::IsNullOrWhiteSpace($RabbitMqCacheConfig)) {
    # Validated HERE, not at the engine. RabbitMqCacheOptions.FromJson treats unparseable JSON as
    # DISABLED and only warns, so a typo would produce a deploy that reports success and an engine
    # that silently never caches. Failing now is the only way that becomes visible.
    try { $null = $RabbitMqCacheConfig | ConvertFrom-Json }
    catch { throw "RabbitMqCacheConfig is not valid JSON: $($_.Exception.Message)`nValue: $RabbitMqCacheConfig" }

    $appSettings['WAREWOLF_RABBITMQ_CACHE'] = $RabbitMqCacheConfig
}

# QueueName MUST also be a real app setting, not just staged in servicebustrigger.bite: on
# dotnet-isolated, [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")] is resolved by
# the OUT-OF-PROCESS Functions Host at function-indexing time, before the isolated worker
# process (where HostBuilderExtensions.ConfigureWarewolf's in-memory config injection runs)
# ever starts - that injection can never reach the host's own resolution, so the function
# fails to index ("'%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%' does not resolve to a value") and the
# trigger is permanently disabled regardless of how correctly servicebustrigger.bite is staged.
# WorkflowName has no such constraint (read via DI inside the worker process, see
# ServiceBusWorkflowTriggerFunction), so only QueueName needs this.
if ($triggerConfigRequired) {
    $appSettings['WAREWOLF_SERVICEBUS_TRIGGER_QUEUE'] = $ServiceBusTriggerQueueName
}
# NOTE: BYPASS_SECURE_CONFIG, WAREWOLF_SUPER_ADMIN_ENABLED and
# SkipFailureToRetrieveSecret (merged into WAREWOLF_SECURITY_FLAGS — WOLF-8516) are
# deliberately NOT set here (not shown in logs/summary, not created on the Function
# App). The engine defaults them to disabled/false when absent; an admin can add
# WAREWOLF_SECURITY_FLAGS manually in the Function App only if a specific override
# is ever required.

# WOLF-8516: AZURE_KEYVAULT_NAME/KEYVAULT_SECRET_NAME are no longer set as App
# Settings — they're written into the staged Settings/executionengine.settings.json
# file instead (see Phase 3, "3.5c" below). HostEnvironmentConfig no longer reads
# these env vars at all.

# ── Runtime decision (resolved HERE so the plan can state it, applied in Phase 1) ──
# `functionapp show` is a READ, so it runs under -DryRun too and the plan tells the truth
# about an app that already exists rather than assuming a fresh create. StrictMode-safe:
# no app, or a payload without siteConfig, both resolve to $null — but they are NOT the same
# decision, so whether the app exists is tracked separately and passed to the planner. A
# payload we could not read the runtime out of yields 'unknown', never a silent "it's fine".
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

# ── Settings summary + single confirmation (item 8) ────────────────────────────
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
Write-Host ("    {0,-28}: {1}" -f 'Auth provisioning', ($SkipAuthProvisioning ? 'skipped' : 'enabled'))
Write-Host ("    {0,-28}: {1}" -f 'secure.config', ($SecureConfigPath ? "$SecureConfigPath [$secureConfigKind]" : '(none — open access)'))
Write-Host ("    {0,-28}: {1}" -f 'License config', ($LicenseConfigPath ? $LicenseConfigPath : '(none — license check may fail)'))
Write-Host ("    {0,-28}: {1}" -f 'Workflows source', ($WorkflowsSourcePath ? $WorkflowsSourcePath : '(none)'))
Write-Host ("    {0,-28}: {1}" -f 'Encrypt sources (this run)', ($doEncryptResources ? 'YES (workflows + ES + others)' : 'no (staged as-is; assumed already encrypted)'))
Write-Host ("    {0,-28}: {1}" -f 'Verify decryption', ($doEncryptResources ? ($VerifyDecryption ? 'yes (in-memory)' : 'no') : 'n/a'))
Write-Host ("    {0,-28}: {1}" -f 'Elasticsearch logging', ($enableEs ? "enabled ($ElasticsearchSourcePath)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'AI performance counters', ($enablePerfCounters ? 'enabled' : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Persistence (Hangfire)', ($enablePersistence ? "enabled ($PersistenceDbSourcePath)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Deploy JobProcessor', ($DeployJobProcessor ? "yes -> Deploy-WwJobProcessor.ps1$($JobProcessorAppName ? " ($JobProcessorAppName)" : '')" : 'no'))
if ($DeployJobProcessor) {
    Write-Host ("    {0,-28}: {1}" -f 'JobProcessor PublishPath', "$JobProcessorPublishPath  (separate from engine PublishDir)")
}
Write-Host ("    {0,-28}: {1}" -f 'Deploy ServiceBusWorker', ($DeployServiceBusWorker ? "yes -> Deploy-WwExecutionServiceBusWorker.ps1$($ServiceBusWorkerAppName ? " ($ServiceBusWorkerAppName)" : '')" : 'no'))
if ($DeployServiceBusWorker) {
    Write-Host ("    {0,-28}: {1}" -f 'ServiceBusWorker PublishPath', "$ServiceBusWorkerPublishPath  (separate from engine PublishDir)")
    Write-Host ("    {0,-28}: {1}" -f 'ServiceBusWorker WorkflowName', ($ServiceBusWorkerWorkflowName ? $ServiceBusWorkerWorkflowName : '(not supplied — child script will prompt)'))
}
Write-Host ("    {0,-28}: {1}" -f 'Deploy RabbitMQ triggers', ($DeployRabbitMqTriggers ? "yes -> Deploy-WwQueueProcessor.ps1 ($($script:QueueTriggerFiles.Count) trigger(s): $((($script:QueueTriggerFiles | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_) }) -join ', ')))" : 'no'))
if ($DeployRabbitMqTriggers) {
    Write-Host ("    {0,-28}: {1}" -f 'QueueProcessor image', ($QueueProcessorImage ? $QueueProcessorImage : "build from $QueueProcessorPublishPath -> $AcrName"))
    Write-Host ("    {0,-28}: {1}" -f 'ACA environment', $AcaEnvironment)
    Write-Host ("    {0,-28}: {1}" -f 'Queue scaling mode', $QueueScalingMode)
}
if ($kvRequired) {
    $kvPurpose = $doEncryptResources ? 'encrypt now + runtime decrypt' : 'runtime decrypt of already-encrypted sources'
    Write-Host ("    {0,-28}: {1}" -f 'Key Vault', "$KeyVaultName / secret '$KeyVaultSecretName' ($kvPurpose) -> Settings/executionengine.settings.json")
}
if ($sbtRequired) {
    $sbtSummary = ($sbtOverrides.GetEnumerator() | Where-Object { $null -ne $_.Value } | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ', '
    Write-Host ("    {0,-28}: {1}" -f 'ServiceBusTrigger tunables', "$sbtSummary -> Settings/executionengine.settings.json")
}
if ($triggerConfigRequired) {
    Write-Host ("    {0,-28}: {1}" -f 'ServiceBusTrigger queue/workflow', "QueueName=$ServiceBusTriggerQueueName, WorkflowName=$ServiceBusTriggerWorkflowName -> Settings/servicebustrigger.bite")
}
if ($DisableDynamicConcurrency) {
    Write-Host ("    {0,-28}: {1}" -f 'Dynamic concurrency', 'PINNED OFF -> host.json concurrency.dynamicConcurrencyEnabled=false')
}
if ($FunctionAppScaleLimit) {
    Write-Host ("    {0,-28}: {1}" -f 'Scale-out limit', "$FunctionAppScaleLimit instance(s) -> siteConfig.functionAppScaleLimit")
}
Write-Host ("    {0,-28}: {1}" -f 'LogDir', $LogDir)
Write-Host ("    {0,-28}: {1}" -f 'Run tag', "wwx-test-run=$runId  (rollback targets this tag only)")
Write-Host ("    {0,-28}: {1}" -f 'DryRun', $DryRun)

# A static concurrency cap is INERT while dynamic concurrency is on — the host overrides it.
# Warn rather than silently flip a host-wide scaling mode; see -DisableDynamicConcurrency.
if ($ServiceBusMaxConcurrentCalls -and -not $DisableDynamicConcurrency) {
    Write-Host ''
    Write-Host ("  [!] -ServiceBusMaxConcurrentCalls $ServiceBusMaxConcurrentCalls will NOT take effect: host.json leaves " +
                'concurrency.dynamicConcurrencyEnabled enabled, and dynamic concurrency overrides every static ' +
                'concurrency setting in host.json. Pass -DisableDynamicConcurrency to make the cap apply.') -ForegroundColor Yellow
}
Write-Host ''
Write-Host '  ── Environment variables (App Settings) to apply ─────────────────────' -ForegroundColor White
foreach ($k in $appSettings.Keys) {
    $shown = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    Write-Host ("    {0,-32}= {1}" -f $k, $shown)
}
if ($enableAppInsights) {
    Write-Host ("    {0,-32}= {1}" -f 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING', '(auto-read from the App Insights resource)')
}
if (-not $SkipAuthProvisioning) {
    Write-Host '    WAREWOLF_ENTRA_CONFIG (tenantId/audience/clientId) = (set by Configure-WwExecutionAuth.ps1 — WOLF-8516)'
}
Write-Host ''

# Surfaced BEFORE the confirmation, not buried in the settings table: continuing past a
# runtime mismatch produces a deploy that reports success and an engine that never starts.
if ($script:RuntimePlan.Action -eq 'warn') {
    Write-Note ("RUNTIME MISMATCH: Function App '{0}' runs {1}, but this package targets {2}. " -f
                $AppName, $script:RuntimePlan.Current, $script:RuntimePlan.Target)
    Write-Note ("  The deploy will SUCCEED and the engine will then fail at startup with " +
                "'You must install or update .NET to run this application.' Re-run with " +
                '-UpgradeRuntimeVersion to reconcile the app (it restarts), or fix it out of band ' +
                ("with: az functionapp config set -n {0} -g {1} --net-framework-version {2}" -f
                 $AppName, $ResourceGroup, $script:RuntimePlan.Target))
}
elseif ($script:RuntimePlan.Action -eq 'unknown') {
    Write-Note ("RUNTIME UNVERIFIED: Function App '{0}' exists, but its netFrameworkVersion could not " -f $AppName)
    Write-Note ("  be read, so this run CANNOT tell whether it matches the {0} this package needs. " -f $script:RuntimePlan.Target)
    Write-Note ("  Nothing will be changed. If it is behind, the deploy still SUCCEEDS and the engine " +
                "then fails at startup with 'You must install or update .NET to run this application.' " +
                ("Check it with: az functionapp config show -n {0} -g {1} --query netFrameworkVersion" -f
                 $AppName, $ResourceGroup))
}

if (-not (Confirm-Yes 'Proceed with this deployment?' $true)) {
    Write-Note 'Aborted by user.'
    return
}

# ════════════════════════════════════════════════════════════════════════════
# Begin work — start the transcript log + summary (BOTH dry-run and real runs).
# A dry-run produces the same logs + summary as a real run (dry-run files carry a
# .dryrun. infix and the summary's "dryRun": true flag) so the rollback can be
# exercised from a dry-run summary.
# ════════════════════════════════════════════════════════════════════════════

$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$transcriptOn = $false
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$logInfix    = if ($DryRun) { 'dryrun.' } else { '' }
$logFile     = Join-Path $LogDir "deploy-WwExecutionEngine-$runStamp.${logInfix}log"
$summaryPath = Join-Path $LogDir "deploy-WwExecutionEngine-$runStamp.${logInfix}summary.json"
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

# State Save-DeploySummary needs at ANY failure point (StrictMode-safe defaults).
# $StagingDir is finalised in Phase 3; the rest are refined as the run proceeds.
$baseUrl                = "https://$AppName.azurewebsites.net"
$StagingDir             = $PublishDir
$aiConnectionString     = $null
$script:DeployLastPhase = 'Phase 0.5  Plan'
$script:DeployStatus    = 'in-progress'

# Write an initial summary IMMEDIATELY — before the first mutating action — so even
# an instant failure in Phase 1 leaves the rollback an authoritative (if early)
# record of intent. Subsequent phases overwrite it with the latest created-map.
Save-DeploySummary -Status 'in-progress'

try {
    Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Infrastructure
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Infrastructure (resource group, storage, function app, App Insights)'
    $script:DeployLastPhase = 'Phase 1  Infrastructure'

    # 1.1 Resource group
    # NOTE on the created-map: we record created[x]=$true the moment we DECIDE to
    # create (before the mutating call), not after it returns. So if a create
    # half-succeeds and then throws, the rollback still sees it as a candidate.
    # The rollback re-verifies existence AND the run tag before deleting, so a
    # resource that never actually got created is harmlessly skipped, and a
    # pre-existing one (created=$false) is never touched.
    $rgExists = (Invoke-Az @('group', 'exists', '--name', $ResourceGroup)) -join ''
    if ($rgExists -eq 'true') {
        Write-Ok "Resource group '$ResourceGroup' already exists."
        $created['resourceGroup'] = $false
    } else {
        $created['resourceGroup'] = $true   # intent recorded BEFORE creating (crash-safe rollback)
        Write-Step "Creating resource group '$ResourceGroup' in '$Location'"
        Invoke-Az (@('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--tags') + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Resource group created.'
    }

    # 1.2 Storage account
    # Storage account names are globally unique across the whole subscription (not
    # scoped to a resource group), so an RG-scoped existence check can miss an
    # account that legitimately lives in a different (often shared) RG - the same
    # cross-RG sharing pattern already seen with the App Service Plan. Fall back to
    # a subscription-wide lookup by name before concluding the account is absent.
    $stExists = Invoke-Az @('storage', 'account', 'show', '--name', $StorageAccount, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if (-not $stExists) {
        $stExists = Invoke-Az @('storage', 'account', 'show', '--name', $StorageAccount, '-o', 'json') -AllowFail
        if ($stExists) {
            $stRg = ($stExists | ConvertFrom-Json).resourceGroup
            Write-Note "Storage account '$StorageAccount' exists in a different resource group ('$stRg', not '$ResourceGroup') - treating as read-only, not recreating."
        }
    }
    if ($stExists) {
        Write-Ok "Storage account '$StorageAccount' already exists."
        $created['storageAccount'] = $false
    } else {
        $created['storageAccount'] = $true   # intent recorded BEFORE creating
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

        # An existing app keeps the runtime it was CREATED with — `functionapp create` is
        # the only place --runtime-version applies, and this branch never runs it. Replay
        # the Phase 0.5 decision so a long-lived app does not silently stay a version
        # behind the payload being deployed onto it.
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
        $created['functionApp'] = $true   # intent recorded BEFORE creating
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

    # 1.3b OPTIONAL scale-out ceiling (-FunctionAppScaleLimit). Applied to BOTH branches
    # above: a pre-existing app keeps whatever limit it already had, and a freshly created
    # one defaults to unbounded, so neither is safe to assume. See the parameter's own
    # comment for why instance count — not maxConnections — is what spends the broker's
    # connection budget.
    if ($FunctionAppScaleLimit) {
        Write-Step "Setting Function App scale-out limit -> $FunctionAppScaleLimit instance(s)"
        # `az functionapp config set` has NO --functions-scale-limit argument (it fails with
        # "unrecognized arguments"), and `az functionapp scale config set --maximum-instance-count`
        # targets FLEX Consumption. On a classic Consumption (Y1) plan the knob is the site's
        # siteConfig.functionAppScaleLimit property, reached through the generic resource writer.
        $siteId = Invoke-Az @(
            'functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup,
            '--query', 'id', '-o', 'tsv'
        )
        Invoke-Az @(
            'resource', 'update', '--ids', "$siteId/config/web",
            '--set', "properties.functionAppScaleLimit=$FunctionAppScaleLimit", '-o', 'none'
        ) -Mutating | Out-Null

        # READ BACK, do not assume — but only on a real run, since the write above is -Mutating
        # and is skipped under -DryRun. An az argument this CLI version rejects fails loudly, but
        # a property name it silently ignores does not, and a scale limit that never applied looks
        # identical to one that did until the broker connection budget blows under load thousands
        # of messages later.
        if ($DryRun) {
            Write-Note "Dry-run: scale-out limit not applied, so not verified."
        } else {
            $applied = Invoke-Az @(
                'functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup,
                '--query', 'siteConfig.functionAppScaleLimit', '-o', 'tsv'
            )
            if ("$applied".Trim() -ne "$FunctionAppScaleLimit") {
                throw ("Function App scale-out limit did not apply: asked for $FunctionAppScaleLimit, " +
                       "siteConfig.functionAppScaleLimit reads '$applied'.")
            }
            Write-Ok "Function App scale-out limit set to $FunctionAppScaleLimit (verified)."
        }
    }

    # 1.4 Application Insights
    $aiConnectionString = $null
    if ($enableAppInsights) {
        # Record created-vs-reused BEFORE the child script runs so rollback never
        # deletes an App Insights component that pre-existed this run.
        $aiPreExists = [bool](Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail)
        $created['appInsights'] = (-not $aiPreExists)   # intent recorded BEFORE provisioning
        Invoke-ChildScript -Path $AppInsightsScript -Label 'Setup-ApplicationInsights.ps1' -Parameters @{
            ResourceGroup   = $ResourceGroup
            FunctionAppName = $AppName
            Location        = $Location
            AppInsightsName = $AppInsightsName
        }
        if ($created['appInsights'] -and -not $DryRun) {
            Invoke-Az (@('resource', 'tag', '--resource-group', $ResourceGroup, '--name', $AppInsightsName, '--resource-type', 'microsoft.insights/components', '--tags') + $ResourceTags) -Mutating | Out-Null
        }
        # Resolve the live connection string for the summary (WAREWOLF_APPINSIGHTS_
        # CONNECTION_STRING is set on the app by Setup-ApplicationInsights.ps1). On a
        # dry-run where the component does not yet exist, fall back to a placeholder.
        $aiConnectionString = (Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '--query', 'connectionString', '-o', 'tsv') -AllowFail) -join ''
        if ([string]::IsNullOrWhiteSpace($aiConnectionString)) { $aiConnectionString = '<pending — created/read on a real run>' }
        if ($DryRun) {
            Write-Note ("Application Insights '{0}': dry-run made NO change. A real run would {1}, then set WAREWOLF_APPINSIGHTS_CONNECTION_STRING + ENABLEAPPLICATIONINSIGHTS=true." -f $AppInsightsName, ($aiPreExists ? 'REUSE this existing component' : 'CREATE it (not present)'))
        } else {
            Write-Ok ("Application Insights configured ({0} existing component)." -f ($aiPreExists ? 'reused' : 'created'))
        }
    } else {
        Write-Note 'Application Insights disabled.'
    }

    Save-DeploySummary -Status 'in-progress'   # persist created-map after infrastructure

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Auth provisioning (Entra ID + Easy Auth)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Auth provisioning (Entra ID + Easy Auth)'
    $script:DeployLastPhase = 'Phase 2  Auth provisioning'

    if ($SkipAuthProvisioning) {
        Write-Note 'Auth provisioning skipped (-SkipAuthProvisioning).'
        $created['entraApp'] = $false
    } else {
        # PROBE BEFORE PROVISIONING. Configure-WwExecutionAuth.ps1 is idempotent and happily REUSES an
        # existing registration, so "auth provisioning ran" never meant "this run created the app".
        #
        # This flag is not cosmetic. Rollback-WwExecutionEngine.ps1 treats created.entraApp = true as
        # OWNERSHIP and deletes the registration (Rollback:346-347, :406) - and a directory object
        # survives resource-group teardown, so it is the one artefact whose removal reaches outside the
        # deployment. Setting it unconditionally meant every redeploy of an EXISTING engine wrote a
        # summary arming a later rollback to destroy an app registration it did not create, taking down
        # every client that authenticates against it. Observed 2026-08-11: an in-place redeploy of
        # wwengine-e2e-th2teq reported "CREATED Entra app registration" for an app created days earlier.
        $entraDisplayName = "$AppName-auth"
        $entraPreExists   = $true   # fail-safe default: never ARM a delete on an unproven assumption

        $existingApp = Invoke-Az @('ad', 'app', 'list', '--display-name', $entraDisplayName,
                                   '--only-show-errors', '-o', 'json') -AllowFail
        if ($null -eq $existingApp) {
            Write-Note ("Could not determine whether Entra app '$entraDisplayName' already exists " +
                        '(the Graph probe failed). Recording it as PRE-EXISTING so rollback will not ' +
                        'delete it; pass -IncludeEntraApp to Rollback-WwExecutionEngine.ps1 if this run ' +
                        'really did create it.')
        } else {
            try {
                $entraPreExists = @(($existingApp | Out-String | ConvertFrom-Json)).Count -gt 0
            } catch {
                Write-Note "Entra app probe returned unparseable JSON; treating '$entraDisplayName' as pre-existing."
            }
        }

        $created['entraApp'] = -not $entraPreExists
        $created['entraAppDisplayName'] = $entraDisplayName
        Write-Ok ("Entra app registration '$entraDisplayName': " +
                  ($entraPreExists ? 'PRE-EXISTING (reused; teardown will NOT delete it)'
                                   : 'not present, will be created by this run'))
        $groupPermissions = @{}
        $userAssignments  = @()
        if ($AuthConfigPath) {
            if (-not (Test-Path -LiteralPath $AuthConfigPath)) { throw "AuthConfigPath not found: $AuthConfigPath" }
            Write-Step "Loading auth config from '$AuthConfigPath'"
            $authCfg = Get-Content -LiteralPath $AuthConfigPath -Raw | ConvertFrom-Json -AsHashtable
            if ($authCfg.ContainsKey('GroupPermissions') -and $authCfg.GroupPermissions) { $groupPermissions = [hashtable]$authCfg.GroupPermissions }
            if ($authCfg.ContainsKey('UserAssignments') -and $authCfg.UserAssignments)   { $userAssignments  = @($authCfg.UserAssignments) }
            Write-Ok ("Auth config loaded: {0} group(s), {1} user assignment(s)." -f $groupPermissions.Count, $userAssignments.Count)
        } else {
            Write-Note 'No -AuthConfigPath supplied; Configure-WwExecutionAuth.ps1 will provision auth with empty group/user maps.'
        }

        Invoke-ChildScript -Path $ConfigureAuthScript -Label 'Configure-WwExecutionAuth.ps1' -Parameters @{
            SubscriptionId    = $SubscriptionId
            TenantId          = $TenantId
            ResourceGroupName = $ResourceGroup
            FunctionAppName   = $AppName
            GroupPermissions  = $groupPermissions
            UserAssignments   = $userAssignments
            NonInteractive    = $true
            SkipSmokeTest     = $true
        }
        if (-not $DryRun -and (Test-Path -LiteralPath $AuthOutputPath)) {
            $authOut = Get-Content -LiteralPath $AuthOutputPath -Raw | ConvertFrom-Json
            Write-Ok "Entra app provisioned. ClientId: $($authOut.ClientId)  Audience: $($authOut.Audience)"
        }
    }

    Save-DeploySummary -Status 'in-progress'   # persist created-map after auth provisioning

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Stage package, encrypt, apply environment variables
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Stage package, encrypt, apply environment variables'
    $script:DeployLastPhase = 'Phase 3  Stage package'

    # 3.0 Resolve the STAGING directory — a FRESH, dedicated copy under the OS temp
    # dir that we prepare and (on a real run) upload as the final artifact. The
    # operator's publish OUTPUT is NEVER mutated, so the zip is built cleanly on every
    # run and the engine + JobProcessor always stage in SEPARATE directories.
    #   real run -> removed after a successful upload (Phase 4).
    #   dry run  -> kept as the inspectable preview artifact (path printed at the end).
    $stageSuffix = if ($DryRun) { '-dryrun' } else { '' }
    # UNIQUIFIER — do not drop. $runStamp is SECOND-resolution and $AppName is fixed for a given
    # app, so two runs of the same app starting inside the same second resolved to the SAME
    # directory, and the Remove-Item below then deleted the other run's staging tree while it was
    # still being copied into (or threw on a handle Windows had not yet released). $PID separates
    # concurrent processes; the short token separates sequential runs within one process.
    # The resolved path is printed below and recorded in the summary as publishDir.
    $stageToken  = '{0}-{1}' -f $PID, ([guid]::NewGuid().ToString('N').Substring(0, 6))
    $StagingDir  = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecutionengine-stage-$AppName-$runStamp-$stageToken$stageSuffix"
    Write-Step "$($DryRun ? 'Dry-run: building preview artifact' : 'Staging deploy artifact') in '$StagingDir' (your publish output is left untouched)"
    if (Test-Path -LiteralPath $StagingDir) { Remove-Item -LiteralPath $StagingDir -Recurse -Force }
    New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null
    if ($publishIsZip) {
        Expand-Archive -LiteralPath $PublishPath -DestinationPath $StagingDir -Force
    } else {
        Copy-Item -Path (Join-Path $PublishDir '*') -Destination $StagingDir -Recurse -Force
    }
    Write-Ok "Publish output copied to staging dir '$StagingDir'."
    if (-not (Test-Path -LiteralPath $StagingDir)) {
        throw "Staging directory '$StagingDir' does not exist after resolution."
    }

    # 3.0b OPTIONAL host.json logLevel alignment (-AlignHostJsonLogLevel).
    # NOT needed for the engine's own logging: the isolated worker's console + App
    # Insights pipelines are driven by EXECUTIONLOGLEVEL in code (Program.cs) and do
    # NOT read host.json. This only tunes the Functions HOST process log verbosity.
    if ($AlignHostJsonLogLevel) {
        $hostJsonPath = Join-Path $StagingDir 'host.json'
        $melLevel     = Convert-ToMelLevel $ExecutionLogLevel
        if (Test-Path -LiteralPath $hostJsonPath) {
            Write-Step "Aligning host.json logLevel (default + Warewolf.*) -> '$melLevel'"
            Update-HostJsonLogLevel -HostJsonPath $hostJsonPath -ExecutionLogLevel $ExecutionLogLevel | Out-Null
            Write-Ok 'host.json logLevel aligned.'
        } else {
            Write-Note "host.json not found in staging dir; skipping logLevel alignment."
        }
    } else {
        Write-Note 'host.json logLevel alignment skipped (worker logging is code-driven; pass -AlignHostJsonLogLevel to also tune the host process).'
    }

    # 3.0c OPTIONAL host.json Service Bus maxConcurrentCalls override
    # (-ServiceBusMaxConcurrentCalls). Bounds how many Service Bus messages the Functions
    # host DISPATCHES to the trigger at once — upstream of, and independent from,
    # ServiceBusTriggerOptions.MaxConcurrentExecutions' own semaphore gate inside the
    # trigger's own code (WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS). Left at
    # the SDK default (unset) unless explicitly requested — see WOLF-8512,
    # docs/ShovelBridge-Architecture.md.
    if ($ServiceBusMaxConcurrentCalls) {
        $hostJsonPath = Join-Path $StagingDir 'host.json'
        if (Test-Path -LiteralPath $hostJsonPath) {
            Write-Step "Setting host.json extensions.serviceBus.maxConcurrentCalls -> $ServiceBusMaxConcurrentCalls"
            if (Update-HostJsonServiceBusConcurrency -HostJsonPath $hostJsonPath -MaxConcurrentCalls $ServiceBusMaxConcurrentCalls) {
                Write-Ok 'host.json serviceBus maxConcurrentCalls set.'
            } else {
                Write-Note 'host.json has no extensions.serviceBus section; skipping maxConcurrentCalls override.'
            }
        } else {
            Write-Note 'host.json not found in staging dir; skipping maxConcurrentCalls override.'
        }
    }

    # 3.0d OPTIONAL host.json dynamic-concurrency pin (-DisableDynamicConcurrency).
    # Dynamic concurrency OVERRIDES the static cap applied in 3.0c above, so the two are
    # meaningless together — see Update-HostJsonDynamicConcurrency for the measured 170-vs-2
    # case that motivated this.
    if ($DisableDynamicConcurrency) {
        $hostJsonPath = Join-Path $StagingDir 'host.json'
        if (Test-Path -LiteralPath $hostJsonPath) {
            Write-Step 'Setting host.json concurrency.dynamicConcurrencyEnabled -> false'
            $null = Update-HostJsonDynamicConcurrency -HostJsonPath $hostJsonPath -Enabled $false
            Write-Ok 'host.json dynamicConcurrencyEnabled set to false (static concurrency caps now apply).'
        } else {
            Write-Note 'host.json not found in staging dir; skipping dynamicConcurrencyEnabled pin.'
        }
    }

    # 3.1 secure.config — encrypted: stage as-is; plaintext: AES-encrypt automatically.
    # (Staging is a local file op — performed in BOTH dry-run and real, into $StagingDir.)
    if ($SecureConfigPath) {
        # ── GUARD (8520 item 5): encrypted secure.config with no Key Vault is ALWAYS broken ──
        #
        # WOLF-8516 moved keyVaultName out of App Settings and into the staged
        # Settings/executionengine.settings.json, and HostEnvironmentConfig no longer reads
        # AZURE_KEYVAULT_NAME at all. So omitting -KeyVaultName now leaves keyVaultName NULL in that
        # file, encryption is disabled at runtime, and the engine cannot decrypt the secure.config
        # this very block is about to encrypt.
        #
        # The failure is silent and total, which is why it needs a hard stop rather than a warning:
        # unreadable secure.config -> every /secure/* request denied -> the engine reports the denial
        # as HTTP 500 (WOLF-8418 wraps denials as 500, not 403) -> EngineForwarder classifies 500 as
        # a BusinessFailure -> the message is dead-lettered AND acked. The queue therefore drains,
        # the app scales back to zero, and the run looks clean while discarding every message.
        #
        # Nearly shipped on 2026-09-03 against wwengine-e2e-ldi413: the pre-merge deploy command
        # omitted both parameters and was fine only because the pre-merge engine still read the env
        # vars. Re-running it against merged code would have broken authorization outright.
        if (-not $KeyVaultName -or -not $KeyVaultSecretName) {
            throw (
                "secure.config is being staged from '$SecureConfigPath' but -KeyVaultName / " +
                "-KeyVaultSecretName were not supplied. Since WOLF-8516 the engine reads Key Vault " +
                'topology SOLELY from Settings/executionengine.settings.json, so it would deploy ' +
                'with encryption DISABLED and be unable to decrypt secure.config. Every /secure/* ' +
                'request would then be denied as HTTP 500 and the QueueProcessor would dead-letter ' +
                'and ack every message while the queue drained clean. Pass both parameters, or ' +
                'deploy without -SecureConfigPath.')
        }

        $secureDest = Join-Path $StagingDir 'secure.config'
        if ($secureConfigKind -eq 'Encrypted') {
            Write-Step 'Staging already-encrypted secure.config (validated decryptable; not re-encrypted)'
            Copy-Item -LiteralPath $SecureConfigPath -Destination $secureDest -Force
        } else {
            Write-Step 'Encrypting plaintext secure.config (AES) and staging'
            Protect-SecureConfig -InPath $SecureConfigPath -OutPath $secureDest
        }
        Write-Ok 'secure.config staged.'
    } else {
        Write-Note 'No secure.config supplied; engine runs in open-access mode.'
    }

    # 3.1b Warewolf License.secureconfig — copy to the staging root (license gate).
    if ($LicenseConfigPath) {
        $licenseDest = Join-Path $StagingDir 'Warewolf License.secureconfig'
        Write-Step "Staging license -> '$licenseDest'"
        Copy-Item -LiteralPath $LicenseConfigPath -Destination $licenseDest -Force
        Write-Ok 'Warewolf License.secureconfig staged.'
    } else {
        Write-Note 'No license supplied; engine license check (default ON) may fail at startup.'
    }

    # 3.2 Workflow resources.
    $publishResources = Join-Path $StagingDir 'Resources'
    if ($WorkflowsSourcePath) {
        if (-not (Test-Path -LiteralPath $WorkflowsSourcePath)) { throw "WorkflowsSourcePath not found: $WorkflowsSourcePath" }
        Write-Step "Staging workflow resources -> '$publishResources'"
        if (-not (Test-Path -LiteralPath $publishResources)) { New-Item -ItemType Directory -Path $publishResources -Force | Out-Null }
        Copy-Item -Path (Join-Path $WorkflowsSourcePath '*') -Destination $publishResources -Recurse -Force
        Write-Ok 'Workflow resources staged.'
    } else {
        Write-Note 'No workflow source supplied.'
    }

    # 3.3 Key Vault setup. KV is wired whenever $kvRequired (so the engine can
    # DECRYPT at runtime). The dev-side "Secrets Officer" role + key generation are
    # added ONLY when THIS run encrypts ($doEncryptResources).
    if ($kvRequired) {
        $vaultExists = [bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)
        $created['keyVault'] = (-not $vaultExists)   # intent recorded BEFORE creating
        if (-not $vaultExists) {
            if (-not $doEncryptResources) {
                throw ("Key Vault '$KeyVaultName' does not exist, and -EncryptResources was not set. " +
                       "A vault is required so the engine can decrypt already-encrypted sources at runtime. " +
                       "Run once with -EncryptResources to create the vault + key, or supply an existing vault.")
            }
            Write-Step "Creating Key Vault '$KeyVaultName' (RBAC authorization)"
            Invoke-Az (@('keyvault', 'create', '--name', $KeyVaultName, '--resource-group', $ResourceGroup, '--location', $Location, '--enable-rbac-authorization', 'true', '--tags') + $ResourceTags) -Mutating | Out-Null
            $GenerateNewKey = $true
        }
        Write-Step 'Enabling system-assigned managed identity on the Function App'
        $idJson = Invoke-Az @('functionapp', 'identity', 'assign', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -Mutating
        $funcPrincipalId = if ($DryRun) { '<dryrun-principal-id>' } else { ($idJson | ConvertFrom-Json).principalId }
        $vaultId = if ($DryRun) { '<dryrun-vault-id>' } else { (Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '--query', 'id', '-o', 'tsv')) -join '' }

        # ALWAYS: the engine's managed identity must read the key at runtime.
        # The identity was enabled moments ago, so this retries while Entra replicates.
        Write-Step 'Assigning RBAC: Key Vault Secrets User -> Function App managed identity'
        Grant-RoleAssignment -PrincipalId $funcPrincipalId -Role 'Key Vault Secrets User' `
                             -Scope $vaultId -PrincipalType 'ServicePrincipal' `
                             -Label "the '$AppName' managed identity"

        # ONLY when encrypting now: whoever DRIVES the deploy must reach the key itself
        # (the grant above covers the app's runtime identity, not this session's).
        #
        # Encrypt-Config.ps1 needs GET on the secret, and SET only when this run also
        # GENERATES the key. So try the cheap thing first: when the key is already readable
        # and no new key is being written, the grant below is a no-op - skip it. That matters
        # because `az role assignment create` needs Microsoft.Authorization/roleAssignments/write
        # on the vault, which a build service principal normally does NOT hold; without this
        # probe an ALREADY-ENTITLED CI identity would still fail, just 60s later (six retries)
        # and with a misleading "Entra replication" message.
        if ($doEncryptResources) {
            $keyAlreadyReadable = (-not $GenerateNewKey) -and (-not $DryRun) -and
                [bool](Invoke-Az @('keyvault', 'secret', 'show', '--vault-name', $KeyVaultName, '--name', $KeyVaultSecretName, '-o', 'json') -AllowFail)

            if ($keyAlreadyReadable) {
                Write-Note ("Key Vault secret '$KeyVaultSecretName' is already readable by the current identity; " +
                            "skipping the 'Key Vault Secrets Officer' grant (this run reuses the existing key, so nothing is written).")
            } else {
                # DryRun keeps its placeholder so the PLANNED grant still shows in the echo.
                $principal = if ($DryRun) {
                    @{ ObjectId = '<dryrun-dev-oid>'; PrincipalType = 'User'; Display = 'the signed-in user' }
                } else {
                    Resolve-CurrentPrincipal
                }
                if (-not $principal) {
                    throw ("Cannot reach the encryption key: secret '$KeyVaultSecretName' in vault '$KeyVaultName' is not " +
                           "readable by the current Azure identity, and that identity's object id could not be resolved " +
                           "(see Resolve-CurrentPrincipal - a service principal cannot use the Graph /me endpoint, and may " +
                           "also lack the directory read needed to look itself up).`n" +
                           "Fix: have an admin pre-grant the deploying identity a Key Vault data-plane role on the vault. " +
                           "'Key Vault Secrets User' is enough to encrypt with an EXISTING key; " +
                           "'Key Vault Secrets Officer' is required only when the key must also be created or rotated:`n" +
                           "  az role assignment create --role 'Key Vault Secrets User' --assignee-object-id <object-id> " +
                           "--assignee-principal-type ServicePrincipal --scope $vaultId")
                }
                Write-Step "Assigning RBAC: Key Vault Secrets Officer -> $($principal.Display) (for encryption)"
                Grant-RoleAssignment -PrincipalId $principal.ObjectId -Role 'Key Vault Secrets Officer' `
                                     -Scope $vaultId -PrincipalType $principal.PrincipalType -Label $principal.Display
                if (-not $DryRun) { Write-Note 'RBAC propagation can take ~1-2 min before the secret can be written.'; Start-Sleep -Seconds 30 }
            }
        }
    }

    # Decide whether REAL encryption can run now (only relevant when encrypting;
    # encryption can't be faked — faked output would not decrypt):
    #   real run -> yes (KV exists/created; key generated on first encrypt)
    #   dry run  -> only when the KV AND the secret already exist (a dry-run never
    #               creates the vault or writes a key); otherwise encryption is
    #               deferred to the real run and the source is staged in plaintext.
    $keyReachable = $false
    if ($doEncryptResources) {
        if (-not $DryRun) {
            $keyReachable = $true
        } elseif ([bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)) {
            $keyReachable = [bool](Invoke-Az @('keyvault', 'secret', 'show', '--vault-name', $KeyVaultName, '--name', $KeyVaultSecretName, '-o', 'json') -AllowFail)
        }
    }

    # Local helper: encrypt a path, then OPTIONALLY (-VerifyDecryption) prove the
    # engine's key decrypts it — verification is IN MEMORY (no plaintext on disk).
    function Invoke-EncryptAndVerify {
        param([string] $TargetPath, [string] $Label)
        $encryptParams = @{ FilePath = $TargetPath; VaultName = $KeyVaultName; SecretName = $KeyVaultSecretName; NonInteractive = $true; NoBackup = $true }
        if ($GenerateNewKey) { $encryptParams['GenerateKeys'] = $true }
        Write-Step "Encrypting ($Label) via Key Vault '$KeyVaultName'"
        & $EncryptScript @encryptParams
        if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 ($Label) failed ($LASTEXITCODE)." }
        if ($VerifyDecryption) {
            # Round-trip verify IN MEMORY — never writes plaintext to disk (-VerifyOnly).
            & $EncryptScript -FilePath $TargetPath -VaultName $KeyVaultName -SecretName $KeyVaultSecretName -VerifyOnly -NonInteractive
            if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 (verify $Label) failed ($LASTEXITCODE)." }
            Write-Ok "Verified (in-memory) the engine's Key Vault key decrypts: $Label"
        } else {
            Write-Note "Decrypt verification skipped for $Label (pass -VerifyDecryption to enable)."
        }
        # First encryption generated the key; subsequent calls must reuse it.
        $script:GenerateNewKey = $false
    }

    # 3.4 Workflow resources — encrypt ONLY when -EncryptResources is set; otherwise
    # the staged sources are left AS-IS (assumed already encrypted on a prior run).
    if (-not $doEncryptResources) {
        Write-Note 'Source encryption disabled (-EncryptResources off) — workflow resources staged AS-IS (assumed already encrypted).'
    } elseif (-not (Test-Path -LiteralPath $publishResources)) {
        Write-Note "Encryption requested but no Resources folder at '$publishResources'; nothing to encrypt."
    } elseif (-not $keyReachable) {
        Write-Note 'Workflow resources staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
    } else {
        Write-Step 'Encrypting workflow resource connection strings (WFAES via Key Vault)'
        Invoke-EncryptAndVerify -TargetPath $publishResources -Label 'workflow resources'
        Write-Ok 'Workflow resources encrypted.'
    }

    # 3.5 Elasticsearch source — ALWAYS staged when enabled; encrypted ONLY when
    # -EncryptResources is set (otherwise staged as-is / assumed already encrypted).
    if ($enableEs) {
        $settingsDir  = Join-Path $StagingDir 'Settings'
        $esDest       = Join-Path $settingsDir $ElasticsearchBiteName
        Write-Step "Staging '$ElasticsearchBiteName' -> '$esDest'"
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }
        Copy-Item -LiteralPath $ElasticsearchSourcePath -Destination $esDest -Force
        if (-not $doEncryptResources) {
            Write-Note 'Elasticsearch source staged AS-IS (source encryption disabled; assumed already encrypted).'
        } elseif (-not $keyReachable) {
            Write-Note 'Elasticsearch source staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
        } else {
            Write-Step 'Encrypting Elasticsearch source connection string (WFAES via Key Vault)'
            Invoke-EncryptAndVerify -TargetPath $esDest -Label 'Elasticsearch source'
            Write-Ok 'Elasticsearch source encrypted.'
        }
    }

    # 3.5b Persistence settings pair (suspend/resume). persistencesettings.json is
    # staged AS-IS (flags only); the DbSource ConnectionString is WFAES-encrypted with
    # the SAME pass as the Elasticsearch source — encrypted ONLY when -EncryptResources
    # (otherwise staged as-is / assumed already encrypted). The engine's resume route
    # (and the JobProcessor) decrypt it at runtime through the shared AesDecryptHook.
    if ($enablePersistence) {
        $settingsDir = Join-Path $StagingDir 'Settings'
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }

        $psDest = Join-Path $settingsDir $PersistenceSettingsName
        Write-Step "Staging '$PersistenceSettingsName' -> '$psDest'"
        Copy-Item -LiteralPath $PersistenceSettingsPath -Destination $psDest -Force

        $dbDest = Join-Path $settingsDir $PersistenceDbSourceName
        Write-Step "Staging '$PersistenceDbSourceName' -> '$dbDest'"
        Copy-Item -LiteralPath $PersistenceDbSourcePath -Destination $dbDest -Force
        if (-not $doEncryptResources) {
            Write-Note 'Persistence DbSource staged AS-IS (source encryption disabled; assumed already encrypted).'
        } elseif (-not $keyReachable) {
            Write-Note 'Persistence DbSource staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
        } else {
            Write-Step 'Encrypting persistence DbSource connection string (WFAES via Key Vault)'
            Invoke-EncryptAndVerify -TargetPath $dbDest -Label 'persistence DbSource'
            Write-Ok 'Persistence DbSource encrypted.'
        }
    }

    # 3.5c executionengine.settings.json (WOLF-8516) — deploy-time-static topology
    # (Key Vault name/secret, ServiceBusTrigger tunables). Plain JSON, staged AS-IS
    # (no encryption; these are names/numbers, not secrets — same classification
    # KEYVAULT_SECRET_NAME always had). Overwrites whatever dev-time placeholder
    # shipped in the publish output (Warewolf.Execution.Lightweight/Settings/
    # executionengine.settings.json, all fields null/absent) with the real values
    # for this deployment. HostEnvironmentConfig / ServiceBusTriggerOptions have NO
    # env-var fallback for these fields any more — a deployment without this step,
    # or without -KeyVaultName/-KeyVaultSecretName when encryption is required,
    # ships with encryption disabled; a deployment without any
    # -ServiceBusTrigger* param ships with the trigger's own hardcoded defaults.
    #
    # $kvRequired and $sbtRequired are independent — either alone stages this file
    # (with the other's fields left null), matching whichever concern this
    # particular deployment actually needs to set.
    if ($kvRequired -or $sbtRequired) {
        $settingsDir = Join-Path $StagingDir 'Settings'
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }
        $engineSettingsDest = Join-Path $settingsDir 'executionengine.settings.json'
        Write-Step "Staging 'executionengine.settings.json' -> '$engineSettingsDest' (keyVaultName=$KeyVaultName, keyVaultSecretName=$KeyVaultSecretName)"
        $engineSettingsValue = [ordered]@{
            keyVaultName       = $kvRequired ? $KeyVaultName : $null
            keyVaultSecretName = $kvRequired ? $KeyVaultSecretName : $null
        }
        $logDetail = "keyVaultName=$($engineSettingsValue.keyVaultName), keyVaultSecretName=$($engineSettingsValue.keyVaultSecretName)"
        if ($sbtRequired) {
            $engineSettingsValue['serviceBusTrigger'] = $sbtOverrides
            $logDetail += ", serviceBusTrigger=$($sbtOverrides | ConvertTo-Json -Compress)"
        }
        Write-Step "Staging 'executionengine.settings.json' -> '$engineSettingsDest' ($logDetail)"
        ($engineSettingsValue | ConvertTo-Json) | Set-Content -LiteralPath $engineSettingsDest -Encoding UTF8
        Write-Ok 'executionengine.settings.json staged.'
    }

    # 3.5d Settings/servicebustrigger.bite (WOLF-8517, corrected) — which queue the
    # in-process Service Bus trigger listens on, and which workflow every message on it
    # executes (ServiceBusTriggerDefinition.cs). Plain JSON, staged AS-IS — no secret in it,
    # Service Bus auth is Managed-Identity-only. Overwrites whatever dev/sample value shipped
    # in the publish output (Warewolf.Execution.Lightweight/Settings/servicebustrigger.bite,
    # WorkflowName="Hello World") with the real values for this deployment.
    #
    # Baked in HERE, at deploy time, rather than written to an already-deployed app's file
    # system afterwards: this Function App is normally deployed with
    # WEBSITE_RUN_FROM_PACKAGE=1, which mounts wwwroot read-only, so a post-deploy write
    # (Enable-ServiceBusSecureTrigger.ps1's original Kudu VFS approach) cannot ever succeed —
    # confirmed live against WarewolfServer-UAT (2026-09-12): UnauthorizedAccessException.
    #
    # When $triggerConfigRequired is false (neither param supplied), the checked-in
    # dev/sample file ships unchanged — local/dev behaviour is unaffected.
    if ($triggerConfigRequired) {
        $settingsDir = Join-Path $StagingDir 'Settings'
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }
        $triggerConfigDest = Join-Path $settingsDir 'servicebustrigger.bite'
        Write-Step "Staging 'servicebustrigger.bite' -> '$triggerConfigDest' (QueueName=$ServiceBusTriggerQueueName, WorkflowName=$ServiceBusTriggerWorkflowName)"
        $triggerConfigValue = [ordered]@{
            QueueName    = $ServiceBusTriggerQueueName
            WorkflowName = $ServiceBusTriggerWorkflowName
        }
        ($triggerConfigValue | ConvertTo-Json) | Set-Content -LiteralPath $triggerConfigDest -Encoding UTF8
        Write-Ok 'servicebustrigger.bite staged.'
    }

    # 3.6 Workflow index — generate workflow-index.json over the STAGED Resources so
    # it ships INSIDE the publish zip built in Phase 4. The engine reads it at
    # startup for O(1) lookups and falls back to a disk scan only when absent
    # (WorkflowIndex.cs). Generated AFTER staging + encryption (encryption renames
    # nothing, so keys are identical). Pure LOCAL file op — runs in BOTH dry-run
    # (into the preview artifact) and real, mirroring resource staging above.
    if (Test-Path -LiteralPath $publishResources) {
        if (Test-Path -LiteralPath $WorkflowIndexScript) {
            $workflowIndexPath = Join-Path $publishResources 'workflow-index.json'
            Write-Step "Generating workflow index -> '$workflowIndexPath'"
            & $WorkflowIndexScript -ResourcesDir $publishResources -OutputPath $workflowIndexPath | Out-Null
            if (Test-Path -LiteralPath $workflowIndexPath) {
                $wfIndexCount = @(([System.IO.File]::ReadAllText($workflowIndexPath) | ConvertFrom-Json).PSObject.Properties).Count
                Write-Ok "workflow-index.json generated ($wfIndexCount entr$(if ($wfIndexCount -eq 1) { 'y' } else { 'ies' })) — bundled into the publish zip."
            } else {
                Write-Note 'No .bite/.xml workflows under Resources — no index written (engine will disk-scan at startup).'
            }
        } else {
            Write-Note "Generate-WorkflowIndex.ps1 not found at '$WorkflowIndexScript'; skipping index (engine will disk-scan at startup)."
        }
    } else {
        Write-Note 'No staged Resources folder — workflow index generation skipped.'
    }

    # 3.7 Apply environment variables (App Settings).
    Write-Step 'Applying environment variables (App Settings)'
    # --settings @FILE, never inline "K=V" pairs.
    #
    # On Windows az consumes the inner double quotes of --settings "K={"a":1}", so a JSON value
    # arrives stored as {a:1} - not valid JSON. Hit live on 2026-09-08 deploying 'wwuat', where it
    # made WAREWOLF_ENTRA_CONFIG unparseable and aborted the run with
    #   "WAREWOLF_ENTRA_CONFIG is not valid JSON ... Path 'tenantId'"
    # Configure-WwExecutionAuth.ps1 was fixed the same way for exactly that reason.
    #
    # The inline form ALSO silently corrupted the two JSON settings written here, where nothing
    # verifies the stored value and so nothing complained:
    #   WAREWOLF_LOGGING_CONFIG  -> RawFlags.FromEnvironment() returns AllFalse, so console logging
    #                               and the App Insights SDK are OFF while the deploy reports success
    #   WAREWOLF_RABBITMQ_CACHE  -> RabbitMqCacheOptions.FromJson treats unparseable JSON as DISABLED,
    #                               so the connection cache never engages
    # An @file carries every value verbatim and fixes all three at once.
    #
    # -AsArray is load-bearing: ConvertTo-Json unwraps a single-element array into a bare object,
    # which az accepts and then silently no-ops (it can even create settings literally named
    # name/value). .json extension, not .tmp: az infers the payload format from it.
    $settingsFile = Join-Path ([System.IO.Path]::GetTempPath()) ("ww-appsettings-{0}.json" -f [guid]::NewGuid().ToString('N'))
    try {
        $settingObjects = @($appSettings.GetEnumerator() | ForEach-Object {
            [ordered]@{ name = $_.Key; value = [string]$_.Value; slotSetting = $false }
        })
        # UTF-8 WITHOUT BOM: a BOM makes az reject the payload before it reaches ARM.
        [System.IO.File]::WriteAllText(
            $settingsFile,
            ($settingObjects | ConvertTo-Json -Depth 5 -AsArray),
            (New-Object System.Text.UTF8Encoding $false))

        Invoke-Az @('functionapp', 'config', 'appsettings', 'set',
                    '--name', $AppName, '--resource-group', $ResourceGroup,
                    '--settings', "@$settingsFile", '-o', 'none') -Mutating | Out-Null
    }
    finally {
        # May hold secret values - remove it even if az threw.
        Remove-Item -LiteralPath $settingsFile -Force -ErrorAction SilentlyContinue
    }
    Write-Ok "$($appSettings.Count) app setting(s) applied."

    # ── Read the JSON settings BACK and prove they are still JSON ──────────────
    # Writing them correctly is not the same as their having arrived correctly, and every failure
    # in this area has been silent: the engine treats an unparseable WAREWOLF_RABBITMQ_CACHE as
    # DISABLED and an unparseable WAREWOLF_LOGGING_CONFIG as all-flags-false, both with at most a
    # warning in a log nobody reads. A deploy that reports success while the feature is off is
    # exactly the outcome this whole check exists to make impossible.
    #
    # Deliberately NOT ConvertFrom-Json: PowerShell accepts unquoted property names, so it parses
    # the mangled {enabled:true} quite happily and would report a corrupt value as healthy. The
    # engine uses System.Text.Json, which rejects it - so the quotes are asserted directly.
    $jsonSettingNames = @($appSettings.Keys | Where-Object { "$($appSettings[$_])".TrimStart().StartsWith('{') })
    if ($jsonSettingNames.Count -gt 0 -and -not $DryRun) {
        Write-Step "Verifying $($jsonSettingNames.Count) JSON app setting(s) round-tripped intact"

        $storedByName = @{}
        $storedJson = Invoke-Az @('functionapp', 'config', 'appsettings', 'list',
                                  '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
        if ($storedJson) {
            try {
                # StrictMode-safe: an entry without name/value - an unexpected payload shape, or a
                # stub in a test - must be skipped, never dereferenced. This check is a safety net
                # and must not become a new way for a successful deploy to fail.
                foreach ($entry in @(($storedJson | Out-String) | ConvertFrom-Json)) {
                    if ($entry -and
                        $entry.PSObject.Properties['name'] -and
                        $entry.PSObject.Properties['value']) {
                        $storedByName[[string]$entry.name] = [string]$entry.value
                    }
                }
            }
            catch { $storedByName = @{} }
        }

        if ($storedByName.Count -eq 0) {
            Write-Note 'App settings could not be read back in a recognisable shape; JSON round-trip check skipped.'
        }
        else {
            foreach ($name in $jsonSettingNames) {
                if (-not $storedByName.ContainsKey($name)) {
                    throw "App setting '$name' was applied but is not present on '$AppName'."
                }

                # Deliberately NOT ConvertFrom-Json: PowerShell accepts UNQUOTED property names, so it
                # parses the mangled {enabled:true} happily and would certify a corrupt value as
                # healthy. The engine uses System.Text.Json, which rejects it. az stripping the inner
                # quotes is exactly what produces {a:1}, so their absence IS the defect - assert on
                # the quotes themselves.
                if ($storedByName[$name] -notmatch '"\s*:') {
                    throw ("App setting '$name' was stored WITHOUT quoted property names, so the engine " +
                           "cannot parse it and the feature it controls would be silently OFF." +
                           "`nStored: $($storedByName[$name])")
                }
            }
            Write-Ok "JSON app settings verified: $($jsonSettingNames -join ', ')."
        }
    }

    Save-DeploySummary -Status 'in-progress'   # persist created-map + staging dir before deploy

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Deploy the package directory to the Function App
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Deploy package to Function App'
    $script:DeployLastPhase = 'Phase 4  Deploy package'

    # We deploy an ALREADY-PUBLISHED artifact, so az zip-deploy (config-zip) is the
    # correct, robust method. 'func azure functionapp publish' expects a PROJECT
    # source dir it can language-detect/build and fails on a pre-built package
    # ("Can't determine project language… Worker runtime cannot be 'None'"), so it
    # is NOT used by Auto — only when explicitly requested via -PublishMethod Func.
    $useFunc = $false
    switch ($PublishMethod) {
        'Func' {
            if (-not (Test-CommandExists 'func')) { throw "PublishMethod 'Func' requires Azure Functions Core Tools (func). Install: https://aka.ms/azfunc-install" }
            $useFunc = $true
        }
        'Auto' { $useFunc = $false }   # pre-built artifact -> zip-deploy
        'Zip'  { $useFunc = $false }
    }

    if ($useFunc) {
        # Advanced/opt-in: pass the language + --no-build so func does not try to
        # detect/build the pre-built package.
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
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecution-$AppName-$runStamp.zip"
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

    # Real run: the staging copy has been uploaded — remove it (it lives under the OS
    # temp dir, not the publish tree). The dry-run preview is intentionally KEPT.
    if (-not $DryRun -and (Test-Path -LiteralPath $StagingDir)) {
        Remove-Item -LiteralPath $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Ok "Staging dir removed (publish output was never modified)."
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 5 — Verify
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Verify'
    $script:DeployLastPhase = 'Phase 5  Verify'

    Write-Ok "Function App endpoint: $baseUrl"
    Write-Host "    Discovery (apis.json): $baseUrl/apis.json" -ForegroundColor Gray

    if ($DryRun) {
        Write-Note 'DryRun: skipping HTTP probe.'
    } elseif (Confirm-Yes 'Run a quick HTTP probe against /apis.json now?' $true) {
        try {
            $resp = Invoke-WebRequest -Uri "$baseUrl/apis.json" -Method GET -MaximumRedirection 0 -SkipHttpErrorCheck
            Write-Host "    GET /apis.json -> $([int]$resp.StatusCode)" -ForegroundColor Green
            Write-Note 'A cold start can take a moment; retry if you see 5xx immediately after deploy.'
        } catch {
            Write-Note "Probe could not complete: $($_.Exception.Message)"
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 6 — (optional) ExecutionEngineJobProcessor companion deploy
    # ════════════════════════════════════════════════════════════════════════
    if ($DeployJobProcessor) {
        Write-Phase 'Phase 6  Deploy ExecutionEngineJobProcessor (companion)'
        $script:DeployLastPhase = 'Phase 6  JobProcessor'

        if (-not (Test-Path -LiteralPath $JobProcessorScript)) {
            throw "Deploy-WwJobProcessor.ps1 not found at '$JobProcessorScript'."
        }

        # Pass the shared context; Deploy-WwJobProcessor.ps1 prompts (interactively) for
        # anything omitted here — including its own AppName / PublishPath / StorageAccount.
        # The DbSource is re-staged + (re-)encrypted by the child from the SAME operator
        # source file, so it stands alone even if run separately later.
        $jpParams = [ordered]@{
            SubscriptionId          = $SubscriptionId
            TenantId                = $TenantId
            ResourceGroup           = $ResourceGroup
            Location                = $Location
            EngineResumeBaseUrl     = $baseUrl
            PersistenceSettingsPath = $PersistenceSettingsPath
            PersistenceDbSourcePath = $PersistenceDbSourcePath
            EncryptResources        = [bool]$doEncryptResources
        }
        if ($JobProcessorAppName)        { $jpParams['AppName']        = $JobProcessorAppName }
        if ($JobProcessorPublishPath)    { $jpParams['PublishPath']    = $JobProcessorPublishPath }
        if ($JobProcessorStorageAccount) { $jpParams['StorageAccount'] = $JobProcessorStorageAccount }
        if ($EngineResumeScope)          { $jpParams['EngineResumeScope'] = $EngineResumeScope }
        if ($kvRequired) {
            $jpParams['KeyVaultName']       = $KeyVaultName
            $jpParams['KeyVaultSecretName'] = $KeyVaultSecretName
        }
        if ($VerifyDecryption)  { $jpParams['VerifyDecryption']  = $true }
        if ($enableAppInsights) { $jpParams['EnableAppInsights'] = $true }
        if ($NonInteractive)    { $jpParams['NonInteractive']    = $true }
        if ($DryRun)            { $jpParams['DryRun']            = $true }

        Invoke-ChildScript -Path $JobProcessorScript -Label 'Deploy-WwJobProcessor.ps1' -Parameters $jpParams
        Write-Ok 'JobProcessor companion deploy invoked.'
        Write-Note 'Reminder: grant the JobProcessor MI the engine role Warewolf_JobProcessor (see runbook).'
    }

    # ════════════════════════════════════════════════════════════════════════
    # ════════════════════════════════════════════════════════════════════════
    # Phase 7 — (optional) ServiceBusWorker companion deploy
    # ════════════════════════════════════════════════════════════════════════
    if ($DeployServiceBusWorker) {
        Write-Phase 'Phase 7  Deploy Warewolf.Execution.ServiceBusWorker (companion)'
        $script:DeployLastPhase = 'Phase 7  ServiceBusWorker'

        if (-not (Test-Path -LiteralPath $ServiceBusWorkerScript)) {
            throw "Deploy-WwExecutionServiceBusWorker.ps1 not found at '$ServiceBusWorkerScript'."
        }

        # Pass the shared context; Deploy-WwExecutionServiceBusWorker.ps1 prompts
        # (interactively) for anything omitted here — including its own
        # AppName / PublishPath / StorageAccount / Service Bus namespace+queue.
        $sbwParams = [ordered]@{
            SubscriptionId     = $SubscriptionId
            TenantId           = $TenantId
            ResourceGroup      = $ResourceGroup
            Location           = $Location
            WwExecutionBaseUrl = $baseUrl
        }
        if ($ServiceBusWorkerAppName)        { $sbwParams['AppName']        = $ServiceBusWorkerAppName }
        if ($ServiceBusWorkerPublishPath)    { $sbwParams['PublishPath']    = $ServiceBusWorkerPublishPath }
        if ($ServiceBusWorkerStorageAccount) { $sbwParams['StorageAccount'] = $ServiceBusWorkerStorageAccount }
        if ($ServiceBusWorkerWorkflowName)   { $sbwParams['WorkflowName']   = $ServiceBusWorkerWorkflowName }
        if ($authOut) {
            $sbwParams['WwExecutionTenantId']       = $TenantId
            $sbwParams['WwExecutionResourceAppId']  = $authOut.ClientId
            $sbwParams['WwExecutionScope']          = ($WwExecutionScope ? $WwExecutionScope : "api://$($authOut.ClientId)/.default")
        } elseif ($WwExecutionScope) {
            $sbwParams['WwExecutionScope'] = $WwExecutionScope
        }
        if ($enableAppInsights) { $sbwParams['EnableAppInsights'] = $true }
        if ($NonInteractive)    { $sbwParams['NonInteractive']    = $true }
        if ($DryRun)            { $sbwParams['DryRun']            = $true }

        Invoke-ChildScript -Path $ServiceBusWorkerScript -Label 'Deploy-WwExecutionServiceBusWorker.ps1' -Parameters $sbwParams
        Write-Ok 'ServiceBusWorker companion deploy invoked.'
        Write-Note 'Reminder: grant the ServiceBusWorker MI the engine role Warewolf_ClientApps (see docs/KB-ClientApps-Configuration.md §2.6).'
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 8 — (optional) RabbitMQ QueueProcessors, one Container App per trigger
    # ════════════════════════════════════════════════════════════════════════
    if ($DeployRabbitMqTriggers) {
        Write-Phase 'Phase 8  Deploy RabbitMQ QueueProcessors (companion, one per trigger)'
        $script:DeployLastPhase = 'Phase 8  QueueProcessors'

        # The engine is deployed by now, so its URL is known and can be handed to every worker.
        $queueEngineBaseUrl = "https://$AppName.azurewebsites.net"

        # Prefer an explicitly supplied app id; otherwise derive it from the resume scope
        # (api://<app-id>/.default) that the JobProcessor path already uses. If neither is
        # available the child prompts, rather than guessing an audience.
        $queueEngineAppId = $QueueEngineResourceAppId
        if (-not $queueEngineAppId -and $EngineResumeScope -match 'api://([^/]+)/') {
            $queueEngineAppId = $Matches[1]
        }

        $qpFailures = 0

        foreach ($qpTriggerFile in $script:QueueTriggerFiles) {
            $qpLabel = [System.IO.Path]::GetFileNameWithoutExtension($qpTriggerFile)

            $qpParams = [ordered]@{
                ResourceGroup                 = $ResourceGroup
                Location                      = $Location
                AcaEnvironment                = $AcaEnvironment
                TriggerFilePath               = $qpTriggerFile
                QueueSourcePath               = $QueueSourcePath
                EngineBaseUrl                 = $queueEngineBaseUrl
                ScalingMode                   = $QueueScalingMode
                ExecutionLogLevel             = $ExecutionLogLevel
                # Always pass the tenant explicitly. The worker acquires an app-only engine token
                # with its managed identity, and a BLANK tenant is legal only for a
                # system-assigned MI - anywhere else the credential chain fails with
                # 'Invalid tenant id provided', which looks like a missing app role rather than
                # missing config. $TenantId is already resolved from `az account show` above.
                EngineTenantId                = $TenantId
            }

            if ($queueEngineAppId)          { $qpParams['EngineResourceAppId'] = $queueEngineAppId }
            if ($EngineResumeScope)          { $qpParams['EngineScope']         = $EngineResumeScope }
            if ($QueueProcessorImage)        { $qpParams['Image']               = $QueueProcessorImage }
            if ($QueueProcessorPublishPath)  { $qpParams['PublishPath']         = $QueueProcessorPublishPath }
            if ($AcrName)                    { $qpParams['AcrName']             = $AcrName }
            if ($RabbitMqSecretUri)          { $qpParams['RabbitMqSecretUri']   = $RabbitMqSecretUri }
            if ($kvRequired) {
                $qpParams['KeyVaultName']       = $KeyVaultName
                $qpParams['KeyVaultSecretName'] = $KeyVaultSecretName
                $qpParams['EncryptStagedSettings'] = $true
            }
            if ($enableAppInsights) { $qpParams['EnableAppInsights'] = $true }
            if ($NonInteractive)    { $qpParams['NonInteractive']    = $true }
            if ($DryRun)            { $qpParams['DryRun']            = $true }

            try {
                Invoke-ChildScript -Path $QueueProcessorScript `
                    -Label "Deploy-WwQueueProcessor.ps1 ($qpLabel)" -Parameters $qpParams
                $script:QueueProcessorApps += @{ trigger = $qpLabel; file = $qpTriggerFile; status = 'invoked' }
            }
            catch {
                $qpFailures++
                $script:QueueProcessorApps += @{
                    trigger = $qpLabel; file = $qpTriggerFile; status = 'failed'; error = $_.Exception.Message
                }
                Write-Note "QueueProcessor deploy failed for '$qpLabel': $($_.Exception.Message)"

                if (-not $ContinueOnQueueTriggerError) {
                    throw ("QueueProcessor deploy failed for '$qpLabel' and -ContinueOnQueueTriggerError " +
                           'was not supplied, so the remaining triggers were skipped. The engine deploy ' +
                           'itself completed successfully.')
                }
            }
        }

        Write-Ok "QueueProcessor companion deploy invoked for $($script:QueueTriggerFiles.Count) trigger(s)."
        Write-Note 'Deploy-WwQueueProcessor.ps1 grants EACH app''s managed identity the engine role'
        Write-Note 'Warewolf_QueueProcessor itself; check its summary for any app it could not grant (that'
        Write-Note 'needs AppRoleAssignment.ReadWrite.All). No per-workflow secure.config row is required -'
        Write-Note 'a workflow with no resource-scoped entry inherits the global map (runbook section 8).'

        if ($qpFailures -gt 0) {
            Write-Note "$qpFailures trigger deploy(s) failed - see the summary JSON."
        }
    }

    # ── Final summary — completed status ─────────────────────────────────────
    # (Incremental in-progress summaries were already written after each phase;
    # this records the terminal 'completed' state for BOTH dry-run and real runs.)
    Save-DeploySummary -Status 'completed'
    Write-Ok "Summary written to $summaryPath"

    Write-Phase ($DryRun ? 'Dry-run complete (no cloud changes made)' : 'Deployment complete')
    Write-Host "  App      : $AppName" -ForegroundColor White
    Write-Host "  RG       : $ResourceGroup" -ForegroundColor White
    Write-Host "  Endpoint : $baseUrl" -ForegroundColor White
    Write-Host "  Artifact : $($DryRun ? "$StagingDir  (dry-run preview)" : 'staged under OS temp, uploaded, then removed (publish output untouched)')" -ForegroundColor White
    Write-Host "  Summary  : $summaryPath" -ForegroundColor White
    if (-not $SkipAuthProvisioning -and (Test-Path -LiteralPath $AuthOutputPath)) {
        Write-Host "  Auth out : $AuthOutputPath" -ForegroundColor White
    }

    # ── Resources manipulated, with reachable URLs ───────────────────────────
    # Driven by the SAME `created` map that Rollback-WwExecutionEngine.ps1 consumes, so what is
    # printed here and what teardown will remove can never disagree. Pre-existing resources are
    # listed as REUSED precisely so nobody mistakes them for things this run owns.
    # NOTE the local names: $created is the run's ownership MAP (consumed by the rollback script) and
    # must not be shadowed here.
    $verb = $DryRun ? 'WOULD CREATE' : 'CREATED'
    Write-Host ''
    Write-Host '  -- Resources manipulated --' -ForegroundColor Cyan
    $createdList = [System.Collections.Generic.List[string]]::new()
    $reusedList  = [System.Collections.Generic.List[string]]::new()
    $entraName   = $created['entraAppDisplayName']
    if (-not $entraName) { $entraName = "$AppName-auth" }
    foreach ($pair in @(
        @{ Key = 'resourceGroup';  Kind = 'Resource group';         Name = $ResourceGroup }
        @{ Key = 'storageAccount'; Kind = 'Storage account';        Name = $StorageAccount }
        @{ Key = 'functionApp';    Kind = 'Function App';           Name = $AppName }
        @{ Key = 'appInsights';    Kind = 'App Insights';           Name = $AppInsightsName }
        @{ Key = 'keyVault';       Kind = 'Key Vault';              Name = $KeyVaultName }
        @{ Key = 'entraApp';       Kind = 'Entra app registration'; Name = $entraName }
    )) {
        if (-not $pair.Name) { continue }
        if ($created[$pair.Key]) { $createdList.Add("$($pair.Kind): $($pair.Name)") }
        else                     { $reusedList.Add("$($pair.Kind): $($pair.Name)") }
    }
    if ($createdList.Count) {
        Write-Host "     $verb" -ForegroundColor Green
        $createdList | ForEach-Object { Write-Host "       $_" -ForegroundColor Green }
    }
    if ($reusedList.Count) {
        Write-Host '     REUSED (pre-existing - teardown will NOT remove these)' -ForegroundColor Gray
        $reusedList | ForEach-Object { Write-Host "       $_" -ForegroundColor Gray }
    }
    Write-Host '     UPDATED' -ForegroundColor Cyan
    Write-Host "       Function App settings: $($appSettings.Count) applied to $AppName" -ForegroundColor Cyan
    if (-not $SkipAuthProvisioning) { Write-Host "       Easy Auth + Entra app roles on $AppName" -ForegroundColor Cyan }
    Write-Host ''
    Write-Host '     ENDPOINTS' -ForegroundColor Blue
    Write-Host "       Engine     : $baseUrl" -ForegroundColor Blue
    Write-Host "       Discovery  : $baseUrl/apis.json" -ForegroundColor Blue
    Write-Host "       Public     : $baseUrl/Public/{workflow}.json     (anonymous)" -ForegroundColor Blue
    Write-Host "       Secure     : $baseUrl/Secure/{workflow}.json     (Entra JWT)" -ForegroundColor Blue
    Write-Host "       Services   : $baseUrl/Services/{workflow}.json   (function key)" -ForegroundColor Blue
    Write-Host "       Portal     : https://portal.azure.com/#@/resource/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$AppName" -ForegroundColor Blue
    Write-Host ''
}
catch {
    # CRASH-SAFE summary: whatever phase we failed in, persist the latest created-map
    # so Rollback-WwExecutionEngine.ps1 can still tear down exactly what THIS run
    # created (and nothing it found pre-existing). Then re-throw so the failure and
    # its non-zero exit propagate exactly as before.
    $script:DeployStatus = 'failed'
    $errMsg = $_.Exception.Message
    Write-Note "Deployment FAILED in '$script:DeployLastPhase': $errMsg"
    try {
        Save-DeploySummary -Status 'failed' -ErrorMessage $errMsg
        Write-Note "Failure summary written to $summaryPath (use it with Rollback-WwExecutionEngine.ps1 -SummaryPath)."
    } catch {
        Write-Note "Could not write failure summary: $($_.Exception.Message)"
    }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
}
