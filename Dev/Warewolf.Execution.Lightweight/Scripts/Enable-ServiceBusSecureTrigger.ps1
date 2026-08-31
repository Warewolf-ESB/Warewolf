#Requires -Version 7.0
<#
.SYNOPSIS
    Idempotently enables the Lightweight engine's in-process "secure Service Bus trigger"
    (Model A — ServiceBusWorkflowTriggerFunction, see docs/ServiceBusSecureTrigger-Architecture.md)
    on an ALREADY-DEPLOYED Function App: creates the trigger queue, grants the app's managed
    identity the Service Bus Data Receiver role, and sets the four app settings the trigger
    binding and token validator require.

    ⚠ THIS SCRIPT IS NOT RUN AUTOMATICALLY BY ANY CI JOB OR OTHER SCRIPT IN THIS REPO. ⚠
    It is a deliberately separate, reviewed operator step because it mutates a live Function
    App's authentication/authorization surface. Run it yourself (ideally with -DryRun first)
    against your own target app — never assume it has already been run against a shared
    deployment (e.g. WarewolfServer-UAT in DEV2) without checking
    `az functionapp config appsettings list` first.

.DESCRIPTION
    Without this step, a freshly-deployed Lightweight engine Function App has the
    ServiceBusWorkflowTrigger function *code* present, but the [ServiceBusTrigger] attribute's
    "%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%" indirection cannot resolve (no such app setting), the
    listen connection has nothing to bind to (no ServiceBusConnection__fullyQualifiedNamespace),
    and EntraBearerTokenValidator reports IsEnabled=false (no tenant/audience configured) — so
    the trigger unconditionally dead-letters every message with reason InvalidToken. This script
    closes exactly that gap:

      1. Verifies the Function App and Service Bus namespace both exist and are reachable.
      2. Idempotently creates -TriggerQueueName on the namespace, with dead-lettering and
         -MaxDeliveryCount configured, if it doesn't already exist.
      3. Reads the Function App's system-assigned managed identity (must already exist — this
         script does not assign one, since removing/replacing an existing identity on a live,
         possibly-shared app is a separate, higher-risk decision left to the operator).
      4. Idempotently grants that identity the "Azure Service Bus Data Receiver" RBAC role,
         scoped to the namespace (checks for an existing assignment first).
      5. Sets the four app settings the trigger needs: ServiceBusConnection__fullyQualifiedNamespace,
         WAREWOLF_SERVICEBUS_TRIGGER_QUEUE, WAREWOLF_ENTRA_TENANT_ID, WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE.

    NOT done by this script (separate, out-of-scope operator steps):
      * Creating the Entra App Registration -EntraServiceBusAudience refers to, or minting any
        client credentials/tokens from it. This script only wires the engine to VALIDATE tokens
        against that audience — producing them is the caller's own concern (see
        docs/ServiceBusSecureTrigger-Architecture.md "Topology").
      * Granting any caller's identity permission, via secure.config, to actually run a given
        workflow. IWorkflowPolicyMatcher denies-by-default; a caller with a validly-signed token
        for the right audience but no secure.config permission row still gets a Denied outcome.
      * host.json's "serviceBus": { "autoCompleteMessages": false } — already committed in this
        repo's host.json and deployed with the app's own published output; nothing to set here.

.PARAMETER FunctionAppName
    REQUIRED. The already-deployed Lightweight engine Function App name, e.g. WarewolfServer-UAT.
.PARAMETER ResourceGroup
    REQUIRED. Resource group containing -FunctionAppName and (typically) -ServiceBusNamespace.
.PARAMETER ServiceBusNamespace
    REQUIRED. Service Bus namespace to provision the trigger queue on.
.PARAMETER ServiceBusNamespaceResourceGroup
    Resource group containing -ServiceBusNamespace, if different from -ResourceGroup.
.PARAMETER TriggerQueueName
    Queue the trigger listens on. Default: wwexecution-secure-trigger-queue-e2e (a distinct,
    test-scoped name — NOT the production default 'wwexecution-secure-trigger-queue' documented
    in docs/ServiceBusSecureTrigger-Architecture.md — to avoid colliding with any other
    deployment sharing the same namespace). Must match whatever
    Test-ShovelBridgeE2E.ps1/Run-ShovelBridgeE2E-DevOpsRabbit.ps1 pass as
    -DestinationQueueName/-ServiceBusQueueName when using -VerifyWorkflowExecution.
.PARAMETER MaxDeliveryCount
    Max delivery attempts before the broker dead-letters a message. Default: 10.
.PARAMETER LockDuration
    ISO 8601 duration a delivery's lock is held before Service Bus considers it abandoned and
    redelivers. Default: PT5M (5 minutes). WOLF-8512: this, together with -MaxDeliveryCount, is
    the delivery budget that must exceed ServiceBusTriggerOptions.ClaimStaleAfter (see
    docs/ServiceBusSecureTrigger-Architecture.md's "Failure classification" section) - otherwise
    a redelivery can never actually take over a stale claim left by a dead/hung attempt before
    the queue exhausts -MaxDeliveryCount and dead-letters the message with no result ever
    recorded. This script asserts that invariant below and fails loudly if it does not hold,
    using host.json's own functionTimeout (+ 1 minute margin) as its estimate of
    ClaimStaleAfter's runtime-resolved default. Applied via `az servicebus queue update` (not
    only at creation) so it converges on an already-provisioned queue too.
.PARAMETER EntraTenantId
    REQUIRED. Entra tenant id — sets WAREWOLF_ENTRA_TENANT_ID. Must match the tenant that
    issues tokens for -EntraServiceBusAudience.
.PARAMETER EntraServiceBusAudience
    REQUIRED. Expected `aud` claim for tokens carried in Service-Bus-triggered messages — sets
    WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE. Deliberately separate from the general HTTP audience
    (see docs/ServiceBusSecureTrigger-Architecture.md "Configuration reference"). The Entra App
    Registration this refers to must already exist — this script does not create one.
.PARAMETER NonInteractive
    Skip the "does this look right?" confirmation prompt before applying mutating changes.
.PARAMETER DryRun
    Print every command this script would run, without executing any mutating one. Strongly
    recommended for the first run against any shared/live Function App.

.NOTES
    Prereqs: `az login` to a subscription that can manage both -FunctionAppName and
    -ServiceBusNamespace, with Owner/User Access Administrator (or equivalent) on the scope
    used for the role assignment step.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $FunctionAppName,
    [Parameter(Mandatory)] [string] $ResourceGroup,
    [Parameter(Mandatory)] [string] $ServiceBusNamespace,
    [string] $ServiceBusNamespaceResourceGroup,

    [string] $TriggerQueueName = 'wwexecution-secure-trigger-queue-e2e',
    [int]    $MaxDeliveryCount = 10,
    [string] $LockDuration = 'PT5M',

    [Parameter(Mandatory)] [string] $EntraTenantId,
    [Parameter(Mandatory)] [string] $EntraServiceBusAudience,

    [switch] $NonInteractive,
    [switch] $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ServiceBusNamespaceResourceGroup)) {
    $ServiceBusNamespaceResourceGroup = $ResourceGroup
}

function Write-Phase { param([string] $Title) Write-Host ''; Write-Host ('═' * 76) -ForegroundColor Cyan; Write-Host "  $Title" -ForegroundColor Cyan; Write-Host ('═' * 76) -ForegroundColor Cyan }
function Write-Step  { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok    { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note  { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function Invoke-Az {
    param([Parameter(Mandatory)][string[]] $CliArgs, [switch] $Mutating, [switch] $AllowFail)
    $rendered = ($CliArgs -join ' ')
    if ($Mutating -and $DryRun) {
        Write-Note "[DryRun] az $rendered"
        return $null
    }
    Write-Step "az $rendered"
    $prevPref = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & az @CliArgs 2>&1
    $ErrorActionPreference = $prevPref
    if ($LASTEXITCODE -ne 0 -and -not $AllowFail) {
        throw "az $rendered failed (exit $LASTEXITCODE): $($output -join [Environment]::NewLine)"
    }
    return $output
}

function Get-EstimatedClaimStaleAfter {
    <#
        WOLF-8512: mirrors (approximately - this is a deploy-time sanity check, not the runtime
        source of truth) ServiceBusTriggerOptions.ResolveClaimStaleAfter's tier-3-plus-margin
        behaviour: read host.json's own functionTimeout and add one minute of lock-renewal
        margin. Deliberately does NOT replicate the full five-tier resolution (explicit env
        override / AzureFunctionsJobHost__functionTimeout app setting / WEBSITE_SKU fallback) -
        this script cannot see the target Function App's own environment at author-time, and
        every environment this repo deploys to sets functionTimeout explicitly in host.json
        (see that property's own doc comment, R6), so tier 3 always wins in practice today. If
        that ever stops being true, this estimate and the real runtime value can diverge -
        which is exactly why this is a fail-loud sanity check, not a substitute for verifying
        the live ClaimStaleAfter value the deployed app actually resolves.
    #>
    $hostJsonPath = Join-Path $PSScriptRoot '..' 'host.json'
    $hostJsonPath = [System.IO.Path]::GetFullPath($hostJsonPath)
    if (-not (Test-Path -LiteralPath $hostJsonPath)) {
        throw "Cannot verify the MaxDeliveryCount/LockDuration invariant - host.json not found at '$hostJsonPath'."
    }
    $hostJson = Get-Content -LiteralPath $hostJsonPath -Raw | ConvertFrom-Json
    $functionTimeoutRaw = $hostJson.functionTimeout
    if ([string]::IsNullOrWhiteSpace($functionTimeoutRaw)) {
        throw "Cannot verify the MaxDeliveryCount/LockDuration invariant - host.json has no functionTimeout set at '$hostJsonPath'."
    }
    return [TimeSpan]::Parse($functionTimeoutRaw) + [TimeSpan]::FromMinutes(1)
}

function Assert-DeliveryBudgetExceedsClaimStaleAfter {
    <#
        WOLF-8512 root cause B: MaxDeliveryCount x LockDuration is the delivery budget Service
        Bus gives a message before dead-lettering it. If that budget is <= ClaimStaleAfter, a
        redelivery can NEVER actually take over a stale claim left by a dead/hung attempt - the
        queue exhausts MaxDeliveryCount and dead-letters the message first, with no result ever
        recorded (see docs/ServiceBusSecureTrigger-Architecture.md's "Failure classification").
        Fails the deploy loudly rather than silently provisioning a queue that cannot self-heal.
    #>
    param(
        [Parameter(Mandatory)][int]      $MaxDeliveryCount,
        [Parameter(Mandatory)][string]   $LockDuration,
        [Parameter(Mandatory)][TimeSpan] $ClaimStaleAfter
    )
    $lockDurationSpan = [System.Xml.XmlConvert]::ToTimeSpan($LockDuration)
    $deliveryBudget = [TimeSpan]::FromTicks($lockDurationSpan.Ticks * $MaxDeliveryCount)
    if ($deliveryBudget -le $ClaimStaleAfter) {
        throw "Invariant violated: MaxDeliveryCount ($MaxDeliveryCount) x LockDuration ($LockDuration = $lockDurationSpan) = $deliveryBudget, which is NOT greater than the estimated ClaimStaleAfter ($ClaimStaleAfter). A redelivery could never take over a stale claim before the queue dead-letters the message with no result ever recorded (WOLF-8512 root cause B). Raise -MaxDeliveryCount and/or -LockDuration, or lower the engine's functionTimeout in host.json."
    }
    Write-Ok "Delivery budget check: MaxDeliveryCount ($MaxDeliveryCount) x LockDuration ($lockDurationSpan) = $deliveryBudget > estimated ClaimStaleAfter ($ClaimStaleAfter)."
}

Write-Phase 'Enable-ServiceBusSecureTrigger — Phase 0  Pre-flight'

if ($DryRun) { Write-Note 'DRY RUN: no mutating az command below will actually be executed.' }

Assert-DeliveryBudgetExceedsClaimStaleAfter -MaxDeliveryCount $MaxDeliveryCount -LockDuration $LockDuration -ClaimStaleAfter (Get-EstimatedClaimStaleAfter)

$acct = az account show 2>$null | ConvertFrom-Json
if (-not $acct) { throw "Not logged in to Azure CLI. Run 'az login' (and 'az account set --subscription <id>') first." }
Write-Ok "Azure: $($acct.name) ($($acct.id))"

$app = Invoke-Az @('functionapp', 'show', '--name', $FunctionAppName, '--resource-group', $ResourceGroup, '-o', 'json') | Out-String | ConvertFrom-Json
if (-not $app) { throw "Function App '$FunctionAppName' not found in resource group '$ResourceGroup'." }
Write-Ok "Function App '$FunctionAppName' found (state: $($app.state))."

$identity = $app.identity
if (-not $identity -or -not $identity.principalId -or $identity.type -notmatch 'SystemAssigned') {
    throw "Function App '$FunctionAppName' has no system-assigned managed identity. This script deliberately does not assign one on a live/shared app — enable it yourself first with: az functionapp identity assign --name $FunctionAppName --resource-group $ResourceGroup"
}
$principalId = $identity.principalId
Write-Ok "System-assigned managed identity: $principalId"

$ns = Invoke-Az @('servicebus', 'namespace', 'show', '--name', $ServiceBusNamespace, '--resource-group', $ServiceBusNamespaceResourceGroup, '-o', 'json') | Out-String | ConvertFrom-Json
if (-not $ns) { throw "Service Bus namespace '$ServiceBusNamespace' not found in resource group '$ServiceBusNamespaceResourceGroup'." }
Write-Ok "Service Bus namespace '$ServiceBusNamespace' found."

Write-Host ''
Write-Host '  This run will (idempotently):' -ForegroundColor White
Write-Host "    1. Create (or converge) queue '$TriggerQueueName' on '$ServiceBusNamespace' (dead-lettering on, max delivery $MaxDeliveryCount, lock duration $LockDuration)." -ForegroundColor White
Write-Host "    2. Grant principal '$principalId' the 'Azure Service Bus Data Receiver' role on '$ServiceBusNamespace' if not already granted." -ForegroundColor White
Write-Host "    3. Set app settings on '$FunctionAppName': ServiceBusConnection__fullyQualifiedNamespace, WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=$TriggerQueueName, WAREWOLF_ENTRA_TENANT_ID, WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE=$EntraServiceBusAudience." -ForegroundColor White
Write-Host ''
Write-Note "This does NOT create the Entra App Registration for '$EntraServiceBusAudience', mint any tokens, or grant secure.config permission to run any workflow — see this script's .DESCRIPTION."

if (-not $NonInteractive -and -not $DryRun) {
    $confirm = Read-Host "Proceed with these changes against '$FunctionAppName' / '$ServiceBusNamespace'? (y/N)"
    if ($confirm -notin @('y', 'Y', 'yes', 'Yes')) { throw 'Aborted by user.' }
}

Write-Phase 'Phase 1  Trigger queue'

function Test-AzExists([string[]] $CliArgs) {
    $prevPref = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $null = & az @CliArgs -o json 2>&1
    $ErrorActionPreference = $prevPref
    return $LASTEXITCODE -eq 0
}

if (Test-AzExists @('servicebus', 'queue', 'show', '--name', $TriggerQueueName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ServiceBusNamespaceResourceGroup)) {
    # WOLF-8512 (R3): converge an ALREADY-provisioned queue's MaxDeliveryCount/LockDuration
    # onto the values this run was invoked with, via `update` rather than `create` (`create`
    # against an existing queue fails). Without this, a queue provisioned by an earlier run of
    # this script (or manually) before -LockDuration existed could silently keep the Service
    # Bus service default lock duration - far tighter than the delivery budget this script now
    # asserts, defeating the whole point of the invariant check above.
    Invoke-Az -Mutating @(
        'servicebus', 'queue', 'update',
        '--name', $TriggerQueueName,
        '--namespace-name', $ServiceBusNamespace,
        '--resource-group', $ServiceBusNamespaceResourceGroup,
        '--max-delivery-count', $MaxDeliveryCount,
        '--lock-duration', $LockDuration
    ) | Out-Null
    if ($DryRun) { Write-Note "Queue '$TriggerQueueName' already exists — max-delivery-count/lock-duration would be converged to $MaxDeliveryCount/$LockDuration (not applied — DryRun)." } else { Write-Ok "Queue '$TriggerQueueName' already exists — converged max-delivery-count/lock-duration to $MaxDeliveryCount/$LockDuration." }
} else {
    Invoke-Az -Mutating @(
        'servicebus', 'queue', 'create',
        '--name', $TriggerQueueName,
        '--namespace-name', $ServiceBusNamespace,
        '--resource-group', $ServiceBusNamespaceResourceGroup,
        '--enable-dead-lettering-on-message-expiration', 'true',
        '--max-delivery-count', $MaxDeliveryCount,
        '--lock-duration', $LockDuration
    ) | Out-Null
    if ($DryRun) { Write-Note "Queue '$TriggerQueueName' would be created (not applied — DryRun)." } else { Write-Ok "Queue '$TriggerQueueName' created." }
}

Write-Phase 'Phase 2  RBAC — Azure Service Bus Data Receiver'

$namespaceScope = "/subscriptions/$($acct.id)/resourceGroups/$ServiceBusNamespaceResourceGroup/providers/Microsoft.ServiceBus/namespaces/$ServiceBusNamespace"
$existingAssignment = Invoke-Az -AllowFail @('role', 'assignment', 'list', '--assignee', $principalId, '--scope', $namespaceScope, '-o', 'json') | Out-String | ConvertFrom-Json
$hasReceiverRole = @($existingAssignment) | Where-Object { $_.roleDefinitionName -eq 'Azure Service Bus Data Receiver' }

if ($hasReceiverRole) {
    Write-Ok "Principal '$principalId' already has 'Azure Service Bus Data Receiver' on '$ServiceBusNamespace'."
} else {
    Invoke-Az -Mutating @(
        'role', 'assignment', 'create',
        '--assignee-object-id', $principalId,
        '--assignee-principal-type', 'ServicePrincipal',
        '--role', 'Azure Service Bus Data Receiver',
        '--scope', $namespaceScope
    ) | Out-Null
    if ($DryRun) { Write-Note "'Azure Service Bus Data Receiver' on '$ServiceBusNamespace' would be granted to '$principalId' (not applied — DryRun)." } else { Write-Ok "Granted 'Azure Service Bus Data Receiver' on '$ServiceBusNamespace' to '$principalId'." }
}

Write-Phase 'Phase 3  App settings'

$fullyQualifiedNamespace = "$ServiceBusNamespace.servicebus.windows.net"
Invoke-Az -Mutating @(
    'functionapp', 'config', 'appsettings', 'set',
    '--name', $FunctionAppName,
    '--resource-group', $ResourceGroup,
    '--settings',
    "ServiceBusConnection__fullyQualifiedNamespace=$fullyQualifiedNamespace",
    "WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=$TriggerQueueName",
    "WAREWOLF_ENTRA_TENANT_ID=$EntraTenantId",
    "WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE=$EntraServiceBusAudience"
) | Out-Null
if ($DryRun) { Write-Note "App settings would be applied to '$FunctionAppName' (not applied — DryRun)." } else { Write-Ok "App settings applied to '$FunctionAppName'." }

Write-Phase 'Done'
if ($DryRun) {
    Write-Note 'DRY RUN complete — nothing was actually changed. Re-run without -DryRun to apply.'
} else {
    Write-Ok "'$FunctionAppName' is now configured to listen on '$TriggerQueueName' via the secure Service Bus trigger."
    Write-Note 'Remaining, separate operator steps: (1) ensure an Entra App Registration issuing tokens for the configured audience exists; (2) grant the calling identity a secure.config permission row for whichever workflow(s) it will run; (3) restart/re-warm the Function App if app settings changes are not picked up live.'
}
