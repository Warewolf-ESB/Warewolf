<#
    Default target values for Invoke-WwQueueLoadTest.ps1, taken from RUN 2 (2026-08-13) - the
    configuration that processed 100/100 messages with zero dead-letters in ~90 seconds.

    Presented as the pre-filled answer to every prompt so a reviewer can press Enter through the
    whole confirmation and reproduce that run exactly.

    NOTHING SECRET LIVES HERE, and nothing secret may ever be added.
    This file is committed to source control and read by anyone reviewing the branch. The broker
    password and the SQL password are resolved at run time - see BrokerSecretUri / SqlConnectionEnvVar
    below. A default is a value you would be happy to read out in a review meeting; if it is not, it
    belongs in Key Vault or an environment variable.

    RUN 1 is recorded next to RUN 2 wherever the two differed, because the difference is the point:
    RUN 1 used 10 replicas and lost 32 messages, RUN 2 used 6 and lost none. Fewer replicas was both
    more reliable AND faster (~90 s versus ~4 min), so treat MaxReplicas as a ceiling to justify
    raising, not a dial to turn up.
#>
@{

    # ── Azure targeting ────────────────────────────────────────────────────────
    ResourceGroup                 = 'DEV2'
    Location                      = 'southafricanorth'
    AcaEnvironment                = 'dev2-cae'
    AcrName                       = 'tudev2containerregistry'

    # ── Execution Engine (Azure Functions, Consumption/Y1 plan) ────────────────
    EngineAppName                 = 'wwengine-e2e-ldi413'
    EngineAppId                   = 'e0029ee1-e9a1-42b4-98e5-6585954340f8'
    EngineTenantId                = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1'
    EngineAppInsightsName         = 'wwengine-e2e-ldi413-ai'

    # host.json sets routePrefix:"" - there is NO /api segment. A 404 on every route means
    # someone added it back.
    WorkflowRoute                 = 'Secure/rabbit/RabbitProcess.json'

    # AuditExecutionLogger only ever writes ERROR/FATAL, so at the default ERROR level a
    # SUCCESSFUL execution logs nothing and engine-side evidence is silently empty.
    RequiredEngineLogLevel        = 'INFO'

    # ── QueueProcessor (Azure Container Apps + KEDA) ───────────────────────────
    WorkerAppName                 = 'wwqp4-ordersuccessqueue'
    WorkerAppNamePrefix           = 'wwqp4-'
    ImageRepository               = 'warewolf/queueprocessor-ldi413'
    TriggerId                     = '03fb9052-7fe4-4e8b-ac18-53779b0ebcba'

    # ── Scale ──────────────────────────────────────────────────────────────────
    # maxReplicas is NOT a deploy flag - Deploy-WwQueueProcessor.ps1 derives it from the trigger's
    # Concurrency field. To change it, edit a COPY of the trigger .bite and redeploy.
    #
    # Concurrent engine requests = MaxReplicas x MaxConcurrency. Measured against this engine:
    #     concurrency  6  ->  24/24 clean
    #     concurrency  8  ->  24/24 clean, max 4.7 s
    #     concurrency 10  ->  26/30, 'Insufficient memory to continue the execution of the program'
    # Memory rose 396 MB -> 712 MB across 10 concurrent executions (~32 MB each) against the
    # ~1.5 GB a Consumption instance gets. 6 leaves clear margin.
    MaxReplicas                   = 6        # RUN 1 used 10 and lost 32 of 100 messages
    MaxConcurrency                = 1
    KedaValue                     = 1        # messages per replica: replicas = ceil(queueLength / value)

    # ── Timeouts ───────────────────────────────────────────────────────────────
    # The ordering constraint is EngineTimeout <= ShutdownGrace < TerminationGracePeriod. KEDA counts
    # only READY messages, so work already in flight is invisible to the scaler and a replica can be
    # scaled away underneath a running message; the drain path is the only thing protecting it.
    EngineTimeoutSeconds          = 180      # was 45, which cut off legitimate slow executions
    ShutdownGraceSeconds          = 210
    TerminationGracePeriodSeconds = 240
    MaxDeliveryAttempts           = 2        # clamped to 1-2: the AMQP redelivered flag is a boolean
    RetryEngineInternalErrors     = $false   # 500 also carries workflow errors and authz denials

    # ── Broker (LavinMQ on CloudAMQP - AMQP 0-9-1, not RabbitMQ) ───────────────
    BrokerHost                    = 'ostrich-01.lmq.cloudamqp.com'
    BrokerPort                    = 5672
    BrokerVirtualHost             = 'bmkzdabu'
    BrokerUser                    = 'bmkzdabu'
    # The password comes from here, never from this file.
    BrokerSecretUri               = 'https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri'
    BrokerSourceBitePath          = 'G:\Deployment\sources\RabbitMQSourceAshley.bite'

    QueueName                     = 'order-success-queue'
    DeadLetterQueue               = 'order-success-queue-errors'

    # ── Database ───────────────────────────────────────────────────────────────
    # The connection string is WFAES-encrypted inside the source .bite and cannot be read without the
    # Key Vault AES key, so it is NOT defaulted here. Resolution order at run time:
    #     1. -SqlConnectionString
    #     2. $env:WWLOADTEST_SQLCONNECTION      <- set this once and every run picks it up
    #     3. an interactive prompt (masked)
    #     4. skip the database phases entirely and mark them SKIP
    SqlConnectionEnvVar           = 'WWLOADTEST_SQLCONNECTION'
    SqlSourceBitePath             = 'C:\ProgramData\Warewolf\Resources\rabbit\NewSqlServerSource\NewSqlServerSource (Local Backup).bite'
    JobTable                      = 'dbo.jobs1'
    JobIdColumn                   = 'JobLogId'

    # ── Key Vault ──────────────────────────────────────────────────────────────
    KeyVaultName                  = 'WWExecutionEngine'
    KeyVaultSecretName            = 'WWExecutionEngineTestSecret'

    # ── Run shape ──────────────────────────────────────────────────────────────
    MessageCount                  = 100
    FailureCount                  = 0
    DrainTimeoutSeconds           = 900
    DrainPollSeconds              = 10
    # Rows must stop rising for this long before the run is called finished. An empty queue is NOT
    # the signal: 2xx->ack and non-2xx->dead-letter+ack both drain it.
    DrainQuietSeconds             = 45

    # ── Local layout ───────────────────────────────────────────────────────────
    StageRoot                     = 'G:\Deployment'
    WorkerPublishPath             = 'G:\Deployment\apps\QueueProcessor'
    LogRoot                       = 'G:\Deployment\logs'
}
