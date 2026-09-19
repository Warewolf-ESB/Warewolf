#Requires -Version 7.0
<#
.SYNOPSIS
    Idempotently enables the Lightweight engine's in-process Service Bus trigger
    (ServiceBusWorkflowTriggerFunction, see docs/ServiceBusSecureTrigger-Architecture.md)
    on an ALREADY-DEPLOYED Function App: creates the trigger queue, grants the app's managed
    identity the Service Bus Data Receiver role, and sets the app settings the trigger binding
    requires.

    ⚠ EXACTLY ONE CI JOB RUNS THIS SCRIPT; FOR EVERY OTHER APP IT IS A MANUAL OPERATOR STEP. ⚠
    Dev/.azure/ProfilerService-CI-UAT-LightweightExecutionEngine.yml runs it against
    'wwprofilerlw' on every deploy ("Enable Service Bus Secure Trigger" step, added after build
    30756 spent 301s watching a bridged message sit unconsumed because neither the RBAC grant nor
    the app setting had ever been applied to that rig). pipeline-CLOUD.yml ('warewolfserver') and
    pipeline-LOADTEST.yml ('WarewolfServer-UAT') deliberately still do NOT — it mutates a live
    Function App's authentication/authorization surface, so automating it per-app is a reviewed
    decision, not a default. For any app other than wwprofilerlw, run it yourself (ideally with
    -DryRun first) — never assume it has already been run against a shared deployment without
    checking `az functionapp config appsettings list` first.

    PRIVILEGE NOTE: Phase 2 writes a role assignment, which Contributor cannot do
    (Microsoft.Authorization/roleAssignments/write is excluded from it). The profiler pipeline's
    service principal was therefore granted 'Role Based Access Control Administrator' on the DEV2
    resource group, condition-constrained (ABAC v2.0) to the 'Azure Service Bus Data Receiver'
    role definition and nothing else. Any other automation of this script needs the equivalent.

.DESCRIPTION
    Without this step, a freshly-deployed Lightweight engine Function App has the
    ServiceBusWorkflowTrigger function *code* present, but the [ServiceBusTrigger] attribute's
    "%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%" indirection cannot resolve (no queue name in
    Settings/servicebustrigger.bite) and the listen connection has nothing to bind to (no
    ServiceBusConnection__fullyQualifiedNamespace) — so the trigger never fires. This script
    closes exactly that gap:

      1. Verifies the Function App and Service Bus namespace both exist and are reachable.
      2. Idempotently creates -TriggerQueueName on the namespace, with dead-lettering and
         -MaxDeliveryCount configured, if it doesn't already exist.
      3. Reads the Function App's system-assigned managed identity (must already exist — this
         script does not assign one, since removing/replacing an existing identity on a live,
         possibly-shared app is a separate, higher-risk decision left to the operator).
      4. Idempotently grants that identity the "Azure Service Bus Data Receiver" RBAC role,
         scoped to the namespace (checks for an existing assignment first). THIS ROLE GRANT IS
         THE ENGINE'S ENTIRE TRUST BOUNDARY for this trigger: the trigger executes ANY
         well-formed message that arrives on the queue, unconditionally, with no per-message
         caller token and no secure.config permission check (see
         docs/ServiceBusSecureTrigger-Architecture.md "Threat model coverage"). Controlling who
         holds "Azure Service Bus Data Sender" on this namespace/queue is therefore the only
         access control that matters — scope it as tightly as your deployment allows.
      5. Sets the app setting the trigger needs: ServiceBusConnection__fullyQualifiedNamespace.

    NOT done by this script (separate, out-of-scope operator steps):
      * Restricting who may send to -TriggerQueueName. This script does not grant "Azure
        Service Bus Data Sender" to anyone — that assignment is the actual authorization
        decision for this trigger and is left to the operator to scope deliberately.
      * host.json's "serviceBus": { "autoCompleteMessages": false } — already committed in this
        repo's host.json and deployed with the app's own published output; nothing to set here.
      * WOLF-8517: which QUEUE the trigger listens on (its NAME here is just the queue this
        script provisions/grants RBAC on) and which WORKFLOW every message on it executes are
        selected by Settings/servicebustrigger.bite, staged at DEPLOY TIME by
        Deploy-WwExecutionEngine.ps1's -ServiceBusTriggerQueueName/-ServiceBusTriggerWorkflowName
        (see that script). This script originally (WOLF-8517's first pass) tried to write that
        file into an already-deployed app's file system afterwards via the Kudu VFS API — that
        cannot work once WEBSITE_RUN_FROM_PACKAGE=1 mounts wwwroot read-only (the normal
        zip-deploy mode Deploy-WwExecutionEngine.ps1 uses), confirmed live against
        WarewolfServer-UAT (2026-09-12): UnauthorizedAccessException. -TriggerQueueName here
        MUST match whatever -ServiceBusTriggerQueueName that deploy was given, or the trigger
        listens on a queue nothing is provisioned/authorized for.

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

    [switch] $NonInteractive,
    [switch] $DryRun,

    # Testing hook: define this script's helper functions and return without contacting
    # Azure, so Tests/Enable-ServiceBusSecureTrigger.Tests.ps1 can dot-source and unit-test
    # them. The mandatory parameters above must still be supplied (any placeholder will do).
    [switch] $LoadFunctionsOnly
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
    # '2>&1' is kept so the throw above can quote stderr, but stderr MUST NOT reach the returned
    # VALUE: every value-reading caller below pipes this straight into ConvertFrom-Json. az writes
    # warnings to stderr, and 'role assignment list --assignee <guid>' emits
    #   WARNING: Failed to query <guid> by invoking Graph API. If you don't have permission to
    #   query Graph API, please specify --assignee-object-id and --assignee-principal-type.
    # whenever the caller cannot read Microsoft Graph - which a pipeline service principal signed
    # in with --allow-no-subscriptions cannot. az still EXITS 0 there (it falls back to treating
    # the GUID as an object id), so the command succeeds and only the parse dies: build 30759
    # failed Phase 2 on "Unexpected character encountered while parsing value: W" - the W of
    # WARNING. Merged stderr arrives as ErrorRecords, so dropping those leaves stdout intact.
    # Same split, same reason, as Deploy-WwQueueProcessor.ps1's Invoke-Az.
    return @($output | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })
}

function Set-FunctionAppSettingsViaFile {
    <#
        Writes app settings as `--settings @<file>` - ONE temp file and ONE `@<path>`
        argument PER setting - instead of passing "KEY=VALUE" pairs as raw arguments.

        WHY: Invoke-Az calls az via `& az @CliArgs` (PowerShell's call operator splatting
        against az.cmd, a batch wrapper on Windows). Any argument containing embedded
        double quotes - e.g. a JSON-valued app setting - is silently stripped of those
        quotes when PowerShell/cmd.exe re-serialise the array for the child process. The
        az call still reports success while the value stored in Azure is unparseable JSON,
        so the affected setting silently fails closed at runtime. Observed live 2026-09-03
        writing a JSON-valued app setting by hand this same way.

        ONE FILE PER SETTING matters: az's `@file` substitution replaces a single argument
        with the file's raw content as ONE token; a shared file holding several KEY=VALUE
        lines is read as one token and split only on the FIRST '=', collapsing every later
        setting into the first one's value.

        This mirrors Configure-WwExecutionAuth.ps1's Set-FunctionAppSettings, which
        documents the same quote-stripping hazard for its own JSON-valued setting.
    #>
    param(
        [Parameter(Mandatory)][string]   $FunctionAppName,
        [Parameter(Mandatory)][string]   $ResourceGroup,
        [Parameter(Mandatory)][string[]] $Settings
    )

    $azArgs = @(
        'functionapp', 'config', 'appsettings', 'set',
        '--name', $FunctionAppName,
        '--resource-group', $ResourceGroup,
        '--settings'
    )

    # A dry run only ever PRINTS the command, so pass the literal KEY=VALUE pairs: the
    # operator needs to eyeball the values, and opaque @<tempfile> paths (deleted straight
    # afterwards) would tell them nothing. The quote-stripping hazard only matters when az
    # actually runs.
    if ($DryRun) {
        Invoke-Az -Mutating ($azArgs + $Settings) | Out-Null
        return
    }

    $tempFiles = [System.Collections.Generic.List[string]]::new()
    try {
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        $fileArgs = foreach ($setting in $Settings) {
            $tempFile = [System.IO.Path]::GetTempFileName()
            $tempFiles.Add($tempFile)
            [System.IO.File]::WriteAllText($tempFile, $setting, $utf8NoBom)
            "@$tempFile"
        }

        Invoke-Az -Mutating ($azArgs + $fileArgs) | Out-Null
    } finally {
        foreach ($f in $tempFiles) { Remove-Item $f -Force -ErrorAction SilentlyContinue }
    }
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

# Dot-source hook for the Pester suite: define the helper functions above and stop, without
# touching Azure or emitting a phase banner. Mirrors Configure-WwExecutionAuth.ps1's and
# Deploy-WwExecutionEngine.ps1's -LoadFunctionsOnly.
if ($LoadFunctionsOnly) { return }

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
Write-Host "    3. Set app setting on '$FunctionAppName': ServiceBusConnection__fullyQualifiedNamespace." -ForegroundColor White
Write-Host ''
Write-Note "This does NOT stage Settings/servicebustrigger.bite (queue/workflow selection) — that is now baked in at deploy time by Deploy-WwExecutionEngine.ps1's -ServiceBusTriggerQueueName/-ServiceBusTriggerWorkflowName. Verify '$FunctionAppName' was deployed with -ServiceBusTriggerQueueName '$TriggerQueueName' — a mismatch means the trigger listens on a queue this run's RBAC grant doesn't cover, or that this queue has no workflow assigned to it at all."
Write-Note "This does NOT grant 'Azure Service Bus Data Sender' on '$ServiceBusNamespace' to anyone — see this script's .DESCRIPTION for why that assignment is the actual authorization decision for this trigger, left to you to scope deliberately."

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

Write-Phase 'Phase 2  RBAC — Azure Service Bus Data Receiver (the trigger''s ENTIRE trust boundary)'

$namespaceScope = "/subscriptions/$($acct.id)/resourceGroups/$ServiceBusNamespaceResourceGroup/providers/Microsoft.ServiceBus/namespaces/$ServiceBusNamespace"
# -AllowFail: a probe that cannot list assignments (no reader rights on the scope) must fall
# through to the grant below rather than claim the role is already there. Invoke-Az returns
# NOTHING in that case - stderr is filtered out of its value - and ConvertFrom-Json cannot
# bind an empty string, so the emptiness is checked here instead of being parsed.
$assignmentJson = (Invoke-Az -AllowFail @('role', 'assignment', 'list', '--assignee', $principalId, '--scope', $namespaceScope, '-o', 'json') | Out-String).Trim()
$existingAssignment = if ($assignmentJson) { $assignmentJson | ConvertFrom-Json } else { @() }
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
Write-Note "REMINDER: ServiceBusWorkflowTriggerFunction executes ANY well-formed message on '$TriggerQueueName' unconditionally — no per-message token, no secure.config permission check. Whoever holds 'Azure Service Bus Data Sender' on '$ServiceBusNamespace' (or on '$TriggerQueueName' specifically) can run any workflow this engine can name. Scope that role assignment deliberately; this script does not grant it."

Write-Phase 'Phase 3  App settings'

$fullyQualifiedNamespace = "$ServiceBusNamespace.servicebus.windows.net"
Set-FunctionAppSettingsViaFile -FunctionAppName $FunctionAppName -ResourceGroup $ResourceGroup -Settings @(
    "ServiceBusConnection__fullyQualifiedNamespace=$fullyQualifiedNamespace"
)
if ($DryRun) { Write-Note "App settings would be applied to '$FunctionAppName' (not applied — DryRun)." } else { Write-Ok "App settings applied to '$FunctionAppName'." }

Write-Phase 'Done'
if ($DryRun) {
    Write-Note 'DRY RUN complete — nothing was actually changed. Re-run without -DryRun to apply.'
} else {
    Write-Ok "'$FunctionAppName' is now configured to listen on '$TriggerQueueName' via the Service Bus trigger."
    Write-Note "Remaining, separate steps: (1) scope who holds 'Azure Service Bus Data Sender' on '$ServiceBusNamespace'/'$TriggerQueueName' deliberately — that grant is this trigger's entire authorization boundary; (2) confirm '$FunctionAppName' was (or will be) deployed with Deploy-WwExecutionEngine.ps1 -ServiceBusTriggerQueueName '$TriggerQueueName' -ServiceBusTriggerWorkflowName '<workflow>' so servicebustrigger.bite actually selects a workflow — this script no longer sets it; restart/re-warm the Function App if app settings changes are not picked up live."
}
