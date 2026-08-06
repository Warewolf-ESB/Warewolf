#Requires -Version 7.0
<#
.SYNOPSIS
    Runs Test-ShovelBridgeE2E.ps1 in -DestinationMode ExternalServiceBus (real Azure
    Service Bus) with -RabbitMqMode External pointed at the Warewolf DevOps RabbitMQ
    broker reachable through its ngrok/management URL (rabbitmq.warewolf.online) —
    the manual equivalent of the "RabbitMQ Shovel -> Service Bus E2E Test
    (ExternalServiceBus mode, Azure)" job in Dev/.azure/pipeline-CLOUD.yml, but using
    the standing DevOps broker instead of a local choco RabbitMQ on the agent.

    SCOPE (default): proves publish -> RabbitMQ source queue -> Shovel -> Azure Service
    Bus queue ARRIVAL. It does NOT itself execute a workflow on the Lightweight engine.

    SCOPE (-VerifyWorkflowExecution): proves the FULL pipeline, including actual workflow
    EXECUTION — publish -> RabbitMQ -> Shovel -> Service Bus queue -> the Lightweight
    engine's in-process secure Service Bus trigger (ServiceBusWorkflowTriggerFunction,
    "Model A" — see docs/ServiceBusSecureTrigger-Architecture.md) -> workflow runs -> result
    polled back from the engine's own GET /secure/servicebus-result/{correlationId}
    endpoint. This REQUIRES the target engine (-EngineBaseUrl) to already have that trigger
    configured and enabled (ServiceBusConnection__fullyQualifiedNamespace,
    WAREWOLF_SERVICEBUS_TRIGGER_QUEUE = -ServiceBusQueueName, WAREWOLF_ENTRA_TENANT_ID,
    WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE, and a Service Bus Data Receiver role grant for the
    engine's managed identity on the namespace) — this script does NOT provision that; it
    is a separate, reviewed operator step (see the provisioning script referenced in
    docs/ServiceBusSecureTrigger-Architecture.md) precisely because it mutates a live engine
    deployment's auth configuration.

.NOTES
    Prereqs already verified against the DevOps broker: testuser has the 'administrator'
    tag + full configure/write/read on vhost '/', and rabbitmq_shovel /
    rabbitmq_shovel_management are enabled. You still need: `az login` to a subscription
    that can manage the Service Bus namespace below.
#>
[CmdletBinding()]
param(
    # ── Azure Service Bus (real) ────────────────────────────────────────────────
    [string] $SubscriptionId,                                   # optional: az account set
    [string] $ServiceBusNamespace   = 'WarewolfShovelBridgeTesting',
    [string] $ServiceBusResourceGroup = 'DEV2',
    [string] $ServiceBusQueueName   = 'wwexecution-queue-e2e',
    [string] $ServiceBusSendRule    = 'shovel-e2e-send',
    [string] $ServiceBusListenRule  = 'shovel-e2e-listen',

    # ── DevOps RabbitMQ (ngrok) ─────────────────────────────────────────────────
    [string] $RabbitMqManagementUri = 'https://rabbitmq.warewolf.online',
    [string] $RabbitMqUsername      = 'testuser',
    [string] $RabbitMqPassword      = 'test123',
    [string] $RabbitMqVHost         = '/',

    [int]    $HarnessTimeoutSeconds = 120,
    [switch] $SkipTeardown,

    # ── Diagnostic-only TLS fallback ─────────────────────────────────────────────
    # Appends '&verify=verify_none' to the Shovel's dest-uri, disabling ALL AMQP 1.0
    # peer certificate validation for the RabbitMQ -> Service Bus hop. Use this ONLY
    # to confirm/unblock the documented advanced.config wildcard-hostname-check issue
    # (see docs/ShovelBridge-Architecture.md "Security") on a broker whose config you
    # cannot change (e.g. rabbitmq.warewolf.online) — NOT recommended otherwise, and
    # never for production use.
    [switch] $DestUriVerifyNone,

    # ── Erlang/OTP 26+ CA trust store fix (recommended over -DestUriVerifyNone) ──────
    # Appends '&cacertfile=<path>' to the Shovel's dest-uri, pointing its AMQP 1.0 TLS
    # client at an explicit CA bundle on the broker host (e.g.
    # /etc/ssl/certs/ca-certificates.crt on the official RabbitMQ Docker image). Fixes
    # brokers on Erlang/OTP 26+ that crash-loop with '{cacerts, undefined}' because OTP
    # 26 no longer falls back to an implicit system CA store for verify_peer TLS. Unlike
    # -DestUriVerifyNone this keeps full certificate validation — prefer it whenever the
    # broker's log shows '{cacerts, undefined}'. See docs/ShovelBridge-Architecture.md
    # "Security". Mutually exclusive with -DestUriVerifyNone.
    [string] $DestUriCaCertFile,

    # ── Full-pipeline "did the workflow actually execute" verification (additive) ──────
    # See docs/ServiceBusSecureTrigger-Architecture.md. Requires -EngineBaseUrl /
    # -WorkflowName / -MessageAuthToken, and that the target engine already has the secure
    # Service Bus trigger provisioned (see .SYNOPSIS) — this script only provisions the
    # Shovel's Send-only SAS rule, never the engine's own listen-side config.
    [switch] $VerifyWorkflowExecution,
    [string] $WorkflowName,
    [string] $WorkflowInputsJson,
    [string] $CorrelationId,
    [securestring] $MessageAuthToken,
    [string] $Jti,
    [string] $EngineBaseUrl,
    [securestring] $ResultPollAuthToken,
    [int]    $ResultTimeoutSeconds = 90
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($DestUriVerifyNone -and -not [string]::IsNullOrWhiteSpace($DestUriCaCertFile)) {
    throw 'Supply either -DestUriVerifyNone or -DestUriCaCertFile, not both: verify_none disables all peer certificate validation, making an explicit CA bundle moot. Prefer -DestUriCaCertFile — it keeps full certificate validation.'
}

if ($VerifyWorkflowExecution) {
    if ([string]::IsNullOrWhiteSpace($WorkflowName)) { throw '-WorkflowName is required when -VerifyWorkflowExecution.' }
    if ($null -eq $MessageAuthToken -or $MessageAuthToken.Length -eq 0) { throw '-MessageAuthToken is required when -VerifyWorkflowExecution.' }
    if ([string]::IsNullOrWhiteSpace($EngineBaseUrl)) { throw '-EngineBaseUrl is required when -VerifyWorkflowExecution.' }

    # The secure Service Bus trigger's queue is a distinct convention from Model B's
    # 'wwexecution-queue(-e2e)' — default to it here UNLESS the caller explicitly passed
    # their own -ServiceBusQueueName (which must then match WAREWOLF_SERVICEBUS_TRIGGER_QUEUE
    # as configured on -EngineBaseUrl).
    if (-not $PSBoundParameters.ContainsKey('ServiceBusQueueName')) {
        $ServiceBusQueueName = 'wwexecution-secure-trigger-queue-e2e'
    }
    if ([string]::IsNullOrWhiteSpace($CorrelationId)) {
        $CorrelationId = [Guid]::NewGuid().ToString('N')
    }
    Write-Host "VerifyWorkflowExecution is set: targeting queue '$ServiceBusQueueName' (must match WAREWOLF_SERVICEBUS_TRIGGER_QUEUE on '$EngineBaseUrl'), correlationId '$CorrelationId'." -ForegroundColor Cyan
}

# Resolve the real test script relative to THIS script's own location, not a hardcoded
# clone path — Test-ShovelBridgeE2E.ps1 lives alongside this wrapper in the same folder,
# so this works from any clone/checkout path (required for teammates to run this).
$testScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
if (-not (Test-Path -LiteralPath $testScript)) { throw "Test-ShovelBridgeE2E.ps1 not found at $testScript" }

# ── 0. Azure login sanity ───────────────────────────────────────────────────────
$acct = az account show 2>$null | ConvertFrom-Json
if (-not $acct) { throw "Not logged in to Azure CLI. Run 'az login' (and 'az account set --subscription <id>') first." }
if ($SubscriptionId) { az account set --subscription $SubscriptionId | Out-Null }
Write-Host "Azure: $($acct.name) ($($acct.id))" -ForegroundColor Cyan

# ── 1. Ensure the namespace still allows local (SAS) auth — the Shovel has no OAuth
#       client, so disableLocalAuth=true would break its dest-uri (see ShovelBridge doc). ─
$disableLocalAuth = az servicebus namespace show --name $ServiceBusNamespace --resource-group $ServiceBusResourceGroup --query disableLocalAuth -o tsv
if ($LASTEXITCODE -ne 0) { throw "Cannot read namespace '$ServiceBusNamespace' (rg '$ServiceBusResourceGroup'). Wrong subscription, or no access." }
if ($disableLocalAuth -eq 'true') {
    Write-Host "Restoring disableLocalAuth=false on '$ServiceBusNamespace' ..." -ForegroundColor Yellow
    az servicebus namespace update --name $ServiceBusNamespace --resource-group $ServiceBusResourceGroup --disable-local-auth false | Out-Null
}

# ── 2. Idempotently ensure the queue + Send-only SAS rule exist. A Listen-only SAS rule
#       is also provisioned UNLESS -VerifyWorkflowExecution, whose target queue is consumed
#       exclusively via the engine's own Managed Identity subscription — a second, SAS-based
#       Listen consumer on that queue would compete with it (see Test-ShovelBridgeE2E.ps1's
#       own -VerifyWorkflowExecution help). ─────────────────────────────────────────────
function Test-AzExists([string[]] $CliArgs) {
    $ErrorActionPreference = 'Continue'
    $null = & az @CliArgs -o json 2>&1
    return $LASTEXITCODE -eq 0
}
$ns = $ServiceBusNamespace; $rg = $ServiceBusResourceGroup; $q = $ServiceBusQueueName
if (-not (Test-AzExists @('servicebus','queue','show','--name',$q,'--namespace-name',$ns,'--resource-group',$rg))) {
    az servicebus queue create --name $q --namespace-name $ns --resource-group $rg | Out-Null
}
if (-not (Test-AzExists @('servicebus','queue','authorization-rule','show','--name',$ServiceBusSendRule,'--namespace-name',$ns,'--resource-group',$rg,'--queue-name',$q))) {
    az servicebus queue authorization-rule create --name $ServiceBusSendRule --namespace-name $ns --resource-group $rg --queue-name $q --rights Send | Out-Null
}
if (-not $VerifyWorkflowExecution) {
    if (-not (Test-AzExists @('servicebus','queue','authorization-rule','show','--name',$ServiceBusListenRule,'--namespace-name',$ns,'--resource-group',$rg,'--queue-name',$q))) {
        az servicebus queue authorization-rule create --name $ServiceBusListenRule --namespace-name $ns --resource-group $rg --queue-name $q --rights Listen | Out-Null
    }
}

# ── 3. Build the Shovel dest-uri (Send-only). The harness Listen connection string is
#       only needed in the default (arrival-proof) mode. ─────────────────────────────
$sendKey = az servicebus queue authorization-rule keys list --name $ServiceBusSendRule --namespace-name $ns --resource-group $rg --queue-name $q --query primaryKey -o tsv
if ([string]::IsNullOrWhiteSpace($sendKey)) { throw "Failed to read primary key for '$ServiceBusSendRule'." }
$policyEnc = [Uri]::EscapeDataString($ServiceBusSendRule)
$keyEnc    = [Uri]::EscapeDataString($sendKey)
$destUri   = "amqps://${policyEnc}:${keyEnc}@${ns}.servicebus.windows.net:5671/?sasl=plain"
if ($DestUriVerifyNone) {
    $destUri += '&verify=verify_none'
    Write-Host "DestUriVerifyNone is set: disabling ALL AMQP 1.0 peer certificate validation on the Shovel's dest-uri (diagnostic use only — see docs/ShovelBridge-Architecture.md 'Security')." -ForegroundColor Yellow
}
if (-not [string]::IsNullOrWhiteSpace($DestUriCaCertFile)) {
    $destUri += '&cacertfile=' + [Uri]::EscapeDataString($DestUriCaCertFile)
    Write-Host "DestUriCaCertFile is set: pointing the Shovel's dest-uri at CA bundle '$DestUriCaCertFile' on the broker host (Erlang/OTP 26+ '{cacerts, undefined}' fix)." -ForegroundColor Yellow
}

if ($VerifyWorkflowExecution) {
    Write-Host "Provisioned queue '$q' on '$ns' (Send='$ServiceBusSendRule'; no Listen rule — the engine's own Managed Identity subscription is the only consumer)." -ForegroundColor Green
} else {
    $listenConnStr = az servicebus queue authorization-rule keys list --name $ServiceBusListenRule --namespace-name $ns --resource-group $rg --queue-name $q --query primaryConnectionString -o tsv
    if ([string]::IsNullOrWhiteSpace($listenConnStr)) { throw "Failed to read connection string for '$ServiceBusListenRule'." }
    Write-Host "Provisioned queue '$q' on '$ns' (Send='$ServiceBusSendRule', Listen='$ServiceBusListenRule')." -ForegroundColor Green
}

# ── 4. Run the real E2E test against the DevOps RabbitMQ broker ────────────────────
$rabbitPwd  = ConvertTo-SecureString $RabbitMqPassword -AsPlainText -Force

$testArgs = @{
    DestinationMode                   = 'ExternalServiceBus'
    DestinationQueueName              = $ServiceBusQueueName
    ExternalShovelDestUri             = $destUri
    RabbitMqMode                      = 'External'
    ExternalRabbitMqManagementUri     = $RabbitMqManagementUri
    ExternalRabbitMqUsername          = $RabbitMqUsername
    ExternalRabbitMqPassword          = $rabbitPwd
    ExternalRabbitMqVHost             = $RabbitMqVHost
    HarnessTimeoutSeconds             = $HarnessTimeoutSeconds
}
if ($SkipTeardown) { $testArgs.SkipTeardown = $true }

if ($VerifyWorkflowExecution) {
    $testArgs.VerifyWorkflowExecution = $true
    $testArgs.WorkflowName            = $WorkflowName
    $testArgs.CorrelationId           = $CorrelationId
    $testArgs.MessageAuthToken        = $MessageAuthToken
    $testArgs.EngineBaseUrl           = $EngineBaseUrl
    $testArgs.ResultTimeoutSeconds    = $ResultTimeoutSeconds
    if ($ResultPollAuthToken) { $testArgs.ResultPollAuthToken = $ResultPollAuthToken }
    if (-not [string]::IsNullOrWhiteSpace($WorkflowInputsJson)) { $testArgs.WorkflowInputsJson = $WorkflowInputsJson }
    if (-not [string]::IsNullOrWhiteSpace($Jti)) { $testArgs.Jti = $Jti }
} else {
    $testArgs.ExternalServiceBusConnectionString = ConvertTo-SecureString $listenConnStr -AsPlainText -Force
}

& $testScript @testArgs
