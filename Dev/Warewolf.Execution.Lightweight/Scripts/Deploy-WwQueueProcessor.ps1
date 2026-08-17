#Requires -Version 7.0
<#
.SYNOPSIS
    Deploys Warewolf.Execution.QueueProcessor to Azure Container Apps - ONE Container App
    per RabbitMQ queue-trigger, autoscaled 0..N by the KEDA rabbitmq scaler.

.DESCRIPTION
    Self-contained orchestrator, mirroring Deploy-WwJobProcessor.ps1:

      Phase 0  Preflight        - tooling, az login, containerapp extension, plan validation
      Phase 1  Infrastructure   - resource group, ACR, ACA environment
      Phase 2  Image            - build + push once, reused by every trigger
      Phase 3  Per trigger      - stage .bite pair, create/update the Container App,
                                  system MI, Key Vault-ref secret, env vars, KEDA rule
      Phase 4  Verify           - revision health
      Phase 5  Summary          - per-app summary JSON for rollback/authorization

    HOW THE TRIGGER FILE IS POINTED AT THIS SCRIPT - three mutually exclusive modes:

      -TriggerFilePath <file>                     one Container App
      -TriggerPath <folder> [-TriggerFilter]      one Container App PER matching file
      -TriggerManifestPath <json>                 one per manifest entry, with overrides

    -TriggerId narrows a folder/manifest to a single trigger (the per-trigger cutover
    path). Zero matches is a HARD ERROR, never a silent no-op.

    Scale settings are DERIVED from the trigger file, so Concurrency stays a single
    source of truth:
        maxReplicas = Concurrency
        value       = Prefetch x MaxConcurrency     (KEDA target: messages per replica)
        minReplicas = 0 (Elastic, default) | maxReplicas (Fixed) | 1 (Warm)

    OPTIMUM SHAPE: minReplicas 0, maxReplicas = Concurrency, and Prefetch == MaxConcurrency
    (both 1 unless a workflow is measured safe to run concurrently). Dispatch is serial per
    channel - measured in Phase 0, deliveries ~2.2s apart with no overlap - so a bigger
    prefetch does not add throughput; it raises the KEDA target, which DELAYS scale-out, and
    leaves more buffered messages to nack on drain. Scale OUT, not UP. A prefetch above the
    in-flight cap is reported as an advisory at plan time.

    CONFIG DELIVERY: the trigger and EVERY source it references are BAKED INTO THE IMAGE,
    staged into a fresh temp tree before `az acr build`:

        <staging>/Settings/triggers/{TriggerId}.bite
        <staging>/Settings/sources/{QueueSourceId}.bite
        <staging>/Settings/sources/{QueueSinkId}.bite      (when the sink differs)

    Baked rather than volume-mounted or fetched at startup because this worker scales to
    zero: any share mount or blob round-trip would be paid on every 0->1 scale, and would add
    a runtime dependency that can stop a replica starting. The image tag therefore pins the
    config version, and a trigger edit means a new revision - which is the audit trail.

.EXAMPLE
    # Dry run for ONE trigger - prints the plan, changes nothing.
    .\Deploy-WwQueueProcessor.ps1 -ResourceGroup DEV2 -Location southafricanorth `
      -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
      -PublishPath D:\QueueProcessor\Publish `
      -TriggerFilePath 'C:\ProgramData\Warewolf\Triggers\Queue\1ac40da8-....bite' `
      -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
      -EngineBaseUrl https://wwengine.azurewebsites.net `
      -EngineResourceAppId 11111111-2222-3333-4444-555555555555 `
      -RabbitMqSecretUri https://kv-warewolf.vault.azure.net/secrets/rabbitmq-uri `
      -DryRun

.EXAMPLE
    # ALL triggers in a folder - one Container App each, sharing one image + environment.
    .\Deploy-WwQueueProcessor.ps1 -ResourceGroup DEV2 -Location southafricanorth `
      -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
      -Image acrwarewolf.azurecr.io/warewolf/queueprocessor@sha256:abc... `
      -TriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' `
      -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
      -EngineBaseUrl https://wwengine.azurewebsites.net `
      -EngineResourceAppId 11111111-2222-3333-4444-555555555555 `
      -KeyVaultName kv-warewolf -KeyVaultSecretName wwaeskey `
      -RabbitMqSecretUri https://kv-warewolf.vault.azure.net/secrets/rabbitmq-uri `
      -NonInteractive

.NOTES
    After deploying, EACH app's managed identity must be granted the engine app role
    Warewolf_QueueProcessor, and secure.config needs a PER-WORKFLOW Execute row for each
    trigger's WorkflowName. See Deploy-EndToEnd-Runbook.md section 8.

    Plan: docs/QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md
#>
[CmdletBinding()]
param(
    # ── Targeting ────────────────────────────────────────────────────────────
    [string] $ResourceGroup,
    [string] $Location,
    [string] $AcaEnvironment,
    [string] $AcrName,

    # ── Image: build from PublishPath, or reuse a pre-built digest/tag ────────
    [string] $PublishPath,
    [string] $Image,
    [string] $ImageRepository = 'warewolf/queueprocessor',
    [string] $ImageTag,
    [string] $DockerfilePath,

    [string] $AppNamePrefix = 'wwqp-',

    # ── Trigger input (mutually exclusive) ───────────────────────────────────
    [string] $TriggerFilePath,
    [string] $TriggerPath,
    # Server-written trigger files are named {triggerId}.bite (EnvironmentVariables.QueueTriggersPath),
    # so the default matches every .bite in the folder rather than a 'triggers*' prefix.
    [string] $TriggerFilter = '*.bite',
    [string] $TriggerManifestPath,
    [string] $TriggerId,

    # Folder (or file) holding the RabbitMQ source .bite files referenced by the staged
    # triggers - BOTH QueueSourceId and QueueSinkId. Every referenced source is copied into
    # Settings\sources\{sourceId}.bite and baked into the image, so a replica resolves its
    # broker with no network dependency at cold start. Prompted when not supplied.
    [string] $QueueSourcePath,

    # ── Engine wiring ────────────────────────────────────────────────────────
    [string] $EngineBaseUrl,
    [string] $EngineResourceAppId,
    [string] $EngineScope,
    [string] $EngineTenantId,
    # How long the worker waits for the engine before giving up. A timeout that fires while the
    # engine is still working is NOT a safe failure: the engine completes the workflow anyway, so
    # the side effects happen and the retry repeats them.
    #
    # Sized from measured engine latency (2026-08-11), not guessed:
    #   burst at the DEPLOYED concurrency (3+1 replicas, Prefetch=1) - max 11.5s
    #   controlled test at concurrency 4                             - max 10.0s
    #   controlled stress at concurrency 10                          - 153s completed; 3 > 200s
    # 180s covers the worst SUCCESSFUL observation under 2.5x overload with headroom.
    #
    # Raised from 45s, which was under-sized: one engine call exceeded it during the 2026-08-11 run,
    # the delivery was left unacked, and with Prefetch=1 the consumer stalled permanently.
    #
    # HARD CEILING is the ENGINE's own functionTimeout (host.json, 00:10:00 = 600s). At or above
    # that the engine kills the invocation first and the worker waits for a reply that never comes.
    [int]    $EngineTimeoutSeconds = 180,

    # ── Scaling (derived from the trigger unless overridden) ──────────────────
    [ValidateSet('Elastic', 'Fixed', 'Warm')]
    [string] $ScalingMode = 'Elastic',
    [int]    $MaxReplicas,
    [int]    $MinReplicas = -1,
    [int]    $TargetQueueLength,
    [int]    $MaxConcurrency = 1,
    [string] $Cpu = '0.5',
    [string] $Memory = '1.0Gi',
    # Both track EngineTimeoutSeconds and must keep the nesting
    #   engine (180) <= drain (210) < termination (240)
    # so a scale-in never SIGKILLs a replica whose engine call would still have completed.
    [int]    $ShutdownGraceSeconds = 210,
    [int]    $TerminationGracePeriodSeconds = 240,

    # Attempts allowed for a delivery whose engine call fails at TRANSPORT level (timeout, socket
    # error) before it is dead-lettered instead of retried. Emitted as WORKER__MAXDELIVERYATTEMPTS.
    #
    # ONLY 1 AND 2 ARE MEANINGFUL and the worker clamps anything higher: attempts are counted with
    # the AMQP `redelivered` flag, which is a BOOLEAN - the broker records that a message has been
    # delivered before, not how many times. Counting further would require republishing the message
    # with an incremented header instead of requeueing it, changing queue order and message identity.
    #
    #   1 = dead-letter on the first transport failure, never retry
    #   2 = requeue once, dead-letter if it fails again (default)
    #
    # Before this existed the worker left a failed delivery UNACKED, and with Prefetch=1 the broker
    # then delivered nothing further - one engine timeout stalled the consumer permanently
    # (measured 2026-08-11: 34 messages stranded behind a single unacked message).
    [ValidateRange(1, 2)]
    [int]    $MaxDeliveryAttempts = 2,

    # Retry an engine HTTP 500 instead of dead-lettering it. Emitted as
    # WORKER__RETRYENGINEINTERNALERRORS. OFF by default, deliberately.
    #
    # 408/429/502/503/504 are ALWAYS retried and are unaffected by this switch - in each of those the
    # workflow provably never ran. 500 is different because this engine overloads it: a genuine
    # workflow error, a WOLF-8418 authorization denial, and host memory exhaustion all surface as
    # 500, and only the last is worth retrying. Turning this on buys resilience to exhaustion at the
    # cost of spending a delivery attempt on messages that can never succeed.
    #
    # Prefer capping concurrency (trigger Concurrency) so the host never exhausts. Measured
    # 2026-08-12 on an Azure Functions Consumption plan: concurrency 6 and 8 both scored 24/24,
    # while 10 produced "Insufficient memory to continue the execution of the program".
    [switch] $RetryEngineInternalErrors,

    # Seconds to pause after granting the app identity its roles, so Key Vault's DATA plane converges
    # before the first replica starts. The control-plane check in Assert-RoleAssigned is NOT enough:
    # measured 2026-08-11, a verified 'Key Vault Secrets User' still produced 403 ForbiddenByRbac with
    # "Assignment: (not found)" on getSecret, crash-looping the replica. 0 disables the wait.
    [int]    $RbacPropagationSeconds = 60,

    # ── Secrets / identity ───────────────────────────────────────────────────
    [string] $KeyVaultName,
    [string] $KeyVaultSecretName,
    [string] $RabbitMqSecretUri,

    # Copy the broker URI out of Key Vault at DEPLOY time and store it as a plain Container App
    # secret, instead of wiring a runtime 'keyvaultref'. Needed where the tenant enforces Continuous
    # Access Evaluation: ACA's secret-sync path cannot answer a CAE claims challenge and fails with
    # "401 AKV10203 ... CaeAuthorizationFailed" on every sync, so KEDA never reads the queue depth.
    # Key Vault remains the source of truth; the cost is that rotation needs a redeploy.
    [switch] $InlineRabbitMqSecret,
    [switch] $UseSsl,
    [switch] $EncryptStagedSettings,

    # ── Logging toggles (engine parity) ──────────────────────────────────────
    [ValidateSet('TRACE', 'DEBUG', 'INFO', 'WARN', 'ERROR', 'FATAL', 'OFF')]
    [string] $ExecutionLogLevel = 'INFO',
    [switch] $EnableAppInsights,
    [string] $AppInsightsConnectionString,

    # ── Ops ──────────────────────────────────────────────────────────────────
    [switch] $DryRun,
    [switch] $NonInteractive,
    [string] $LogDir,
    [switch] $ContinueOnTriggerError,
    [switch] $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ═════════════════════════════════════════════════════════════════════════════
# Helpers (same names/semantics as Deploy-WwExecutionEngine.ps1)
# ═════════════════════════════════════════════════════════════════════════════

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('=' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('=' * 76) -ForegroundColor Cyan
}

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }
function Write-Bad  { param([string] $Msg) Write-Host "  [x] $Msg" -ForegroundColor Red }

function Test-CommandExists {
    param([string] $Name)
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Read-Required {
    param([string] $Name, [string] $Current, [string] $Hint)
    if (-not [string]::IsNullOrWhiteSpace($Current)) { return $Current }
    if ($NonInteractive) {
        throw "Required value '$Name' was not supplied. Pass -$Name <value> (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do { $v = Read-Host "  Enter $Name$hintText" } while ([string]::IsNullOrWhiteSpace($v))
    return $v.Trim()
}

function Invoke-Az {
    <#
        -Mutating : changes cloud state; echoed (not run) under -DryRun.
        -AllowFail: non-zero exit returns $null instead of throwing (probes).
    #>
    param(
        [Parameter(Mandatory)][string[]] $AzArgs,
        [switch] $Mutating,
        [switch] $AllowFail,
        # Set when any argument carries a credential. The command is still executed in full, but the
        # echoed and thrown text is redacted. Without this, '--secrets name=<amqp uri with password>'
        # would be printed by the DryRun echo AND embedded in the failure message of every throw.
        [switch] $Sensitive
    )

    $display = if ($Sensitive) {
        (($AzArgs | ForEach-Object { if ($_ -match '=') { ($_ -split '=', 2)[0] + '=***REDACTED***' } else { $_ } }) -join ' ')
    } else { $AzArgs -join ' ' }

    if ($Mutating -and $DryRun) {
        Write-Host "      [DRYRUN] az $display" -ForegroundColor DarkGray
        return $null
    }
    # '2>&1' is kept so a failure message is available for the throw, but stderr MUST NOT reach the
    # returned VALUE. `az` writes warnings to stderr, and every `az containerapp ...` call emits
    #   WARNING: The behavior of this command has been altered by the following extension: containerapp
    # With a naive '2>&1' capture, a read like
    #   containerapp show --query identity.principalId -o tsv
    # returns a TWO-element array (the warning as an ErrorRecord, then the id). Callers doing
    # "$result".Trim() then produce 'WARNING: ... <guid>', which was silently passed as
    # --assignee-object-id: the role assignment failed, -AllowFail hid it, and the deploy died two
    # steps later on a Key Vault secret it could not resolve. Splitting the streams fixes every
    # value-returning read in this script, not just that one.
    # Retry TRANSPORT failures. Observed three times against this subscription:
    #   ('Connection aborted.', ConnectionResetError(10054, 'An existing connection was forcibly
    #    closed by the remote host'))
    # each time killing a deploy mid-flight and leaving a half-configured Container App. Every az
    # call this script makes is idempotent (create/update with the same arguments, secret set,
    # registry set, role assignment create), so replaying one is safe.
    #
    # Deliberately narrow: only transport/throttling patterns match. A genuine ResourceNotFound -
    # which is how the -AllowFail probes detect "does not exist" - does NOT match, so probes still
    # return $null on the first attempt instead of stalling.
    $transient = 'Connection aborted|ConnectionResetError|10054|Read timed out|timed out|' +
                 'ServiceUnavailable|Gateway Time-?out|TooManyRequests|Too many requests|' +
                 'ServerTimeout|temporarily unavailable'
    $maxTries = 4

    for ($try = 1; $try -le $maxTries; $try++) {
        $raw = & az @AzArgs 2>&1
        $exit = $LASTEXITCODE
        # stderr must not reach the returned VALUE - see the note above about az warnings.
        $stdout = @($raw | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })

        if ($exit -eq 0) {
            if ($stdout.Count -eq 0) { return $null }
            if ($stdout.Count -eq 1) { return $stdout[0] }
            return $stdout
        }

        $text = ($raw | ForEach-Object { "$_" }) -join ' '
        if ($try -lt $maxTries -and $text -match $transient) {
            $wait = 5 * $try
            Write-Note ("az $($AzArgs[0..1] -join ' ') hit a transient transport error " +
                        "(attempt $try/$maxTries) - retrying in ${wait}s.")
            Start-Sleep -Seconds $wait
            continue
        }

        if ($AllowFail) { return $null }
        throw "az $display failed: $text"
    }
}

function Get-AppNameSlug {
    <#
        wwqp- + slug of the trigger name (fallback queue name), constrained to ACA's
        32-char limit. When truncation happens a 4-char hash of the TriggerId is appended
        so two long, similarly-named triggers cannot collide.

        -ForceHash appends that same TriggerId hash even when the slug fits. Needed because
        the trigger's display Name is NOT unique - Warewolf names triggers after their logical
        purpose and lets the QUEUE distinguish them, so two triggers routinely share a Name
        (e.g. 'OrderQueue' on order-success-queue and order-failure-queue). TriggerId is the
        only guaranteed-unique field, so it is the last resort in Resolve-AppNameCollision.
    #>
    param([string] $Name, [string] $Fallback, [string] $Id, [string] $Prefix, [switch] $ForceHash)

    $basis = if ([string]::IsNullOrWhiteSpace($Name)) { $Fallback } else { $Name }
    $slug = ($basis.ToLowerInvariant() -replace '[^a-z0-9]+', '-').Trim('-')
    if ([string]::IsNullOrWhiteSpace($slug)) { $slug = 'trigger' }

    $budget = 32 - $Prefix.Length
    if ($slug.Length -le $budget -and -not $ForceHash) {
        return "$Prefix$slug"
    }

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Id))
        $suffix = -join (($hashBytes[0..1] | ForEach-Object { $_.ToString('x2') }))
    }
    finally { $sha.Dispose() }

    $keep = $budget - ($suffix.Length + 1)
    if ($slug.Length -lt $keep) { $keep = $slug.Length }
    return "$Prefix$($slug.Substring(0, [Math]::Max(1, $keep)))-$suffix"
}

function Assert-RoleAssigned {
    <#
        Creates a role assignment and then PROVES it exists before reporting success.

        Why the proof matters: 'role assignment create' was previously called with -AllowFail, which
        swallows the error and returns $null, while the caller printed "Granted ..." unconditionally.
        A transient failure therefore produced a log that claimed the grant had been made, and the
        real symptom surfaced much later and somewhere else entirely - ACA rejecting a keyvaultref
        secret it could not resolve. Never report a grant that has not been read back.

        Retries because role-assignment writes can fail transiently and because the subsequent read
        is eventually consistent. Throws on genuine failure so the deploy stops at the cause.
    #>
    param(
        [string] $PrincipalId,
        [string] $Role,
        [string] $Scope,
        [string] $ScopeLabel,
        [int]    $MaxAttempts = 3
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        Invoke-Az -AzArgs @('role', 'assignment', 'create',
                            '--assignee-object-id', $PrincipalId,
                            '--assignee-principal-type', 'ServicePrincipal',
                            '--role', $Role,
                            '--scope', $Scope,
                            '-o', 'none') -Mutating -AllowFail | Out-Null

        if ($DryRun) { Write-Ok "Would grant '$Role' on '$ScopeLabel' to the app identity."; return }

        # Project the plain role names and filter in PowerShell. Do NOT use a JMESPath filter such
        # as "[?roleDefinitionName=='$Role'].id": `az` on Windows is a .cmd shim, and a --query
        # containing SINGLE QUOTES is mangled by cmd.exe before Python ever sees it (the same defect
        # produces "].name was unexpected at this time" for [?starts_with(name,'x')].name). The
        # mangled query makes az exit non-zero, -AllowFail turns that into $null, and the grant then
        # looks absent when it is actually present - a false negative that blocks a correct deploy.
        $roles = Invoke-Az -AzArgs @('role', 'assignment', 'list',
                                     '--assignee', $PrincipalId,
                                     '--scope', $Scope,
                                     '--query', '[].roleDefinitionName',
                                     '-o', 'tsv') -AllowFail
        $found = @($roles) | ForEach-Object { "$_".Trim() } | Where-Object { $_ -eq $Role }
        if ($found) {
            Write-Ok "Granted '$Role' on '$ScopeLabel' to the app identity (verified)."
            return
        }

        if ($attempt -lt $MaxAttempts) {
            Write-Note ("'$Role' on '$ScopeLabel' did not read back (attempt $attempt/$MaxAttempts) - " +
                        'retrying in 15s.')
            Start-Sleep -Seconds 15
        }
    }

    throw ("Could not grant '$Role' to the app identity on '$ScopeLabel' after $MaxAttempts attempts. " +
           'Creating role assignments needs Owner or User Access Administrator at that scope. ' +
           'Without it the deploy cannot continue: ACA resolves the keyvaultref secret with this ' +
           'identity and rejects the revision, and the image pull would fail too.')
}

function Wait-RbacDataPlaneSettle {
    <#
        Pauses after the role grants so Key Vault's DATA plane can catch up before the first replica
        starts.

        WHY THIS IS NOT REDUNDANT WITH Assert-RoleAssigned: that function retries until the assignment
        reads back from the CONTROL plane (`az role assignment list`). Key Vault's data plane is a
        separate, later-converging cache. Measured 2026-08-11: the control plane confirmed
        'Key Vault Secrets User', the container started, and `getSecret` still returned

            403 Forbidden ... "Caller is not authorized to perform action on resource."
            Action: 'Microsoft.KeyVault/vaults/secrets/getSecret/action'   Assignment: (not found)

        so the replica crash-looped at Program.cs startup until propagation completed.

        WHAT THIS DOES AND DOES NOT GUARANTEE: it shrinks the race, it does not remove it - the data
        plane offers no "is it effective yet" probe the operator can run on the identity's behalf, and
        we cannot authenticate AS the managed identity from here. If a replica still loses the race it
        crash-loops and ACA restarts it, which self-heals; the cost is a slow, alarming-looking first
        start. The complete fix is retry-with-backoff inside KeyVaultSecretManager.InitializeAsync,
        which is shared with the engine and therefore a separate decision.

        Set -RbacPropagationSeconds 0 to skip (e.g. redeploying an app whose identity already holds
        the roles, where nothing has changed and there is nothing to propagate).
    #>
    param([string] $PrincipalId)

    if ($DryRun) { Write-Ok "Would wait ${RbacPropagationSeconds}s for Key Vault data-plane RBAC propagation."; return }
    if (-not $PrincipalId) { return }
    if ($RbacPropagationSeconds -le 0) {
        Write-Note 'Skipping the RBAC data-plane settle wait (-RbacPropagationSeconds 0).'
        return
    }

    Write-Step ("Waiting ${RbacPropagationSeconds}s for Key Vault data-plane RBAC to converge " +
                '(control plane is already verified; the data plane lags and a replica that ' +
                'loses the race crash-loops until it catches up)')
    Start-Sleep -Seconds $RbacPropagationSeconds
    Write-Ok 'RBAC settle wait complete.'
}

function Grant-AppIdentityRoles {
    <#
        Grants the Container App's system-assigned identity the two roles it cannot work without.
        MUST be called after the app exists (the identity does not exist before that) and BEFORE
        'containerapp secret set' with a keyvaultref, because that call resolves the secret
        immediately and rejects the revision if the identity cannot read it.

        Both grants are scoped to a single resource and are read/pull-only, and both are idempotent.
    #>
    param([string] $PrincipalId)

    if (-not $PrincipalId) {
        if ($RabbitMqSecretUri) {
            Write-Note ('No managed identity principalId resolved, so the Key Vault and AcrPull ' +
                        'grants were skipped. The image pull and the scale rule will both fail.')
        }
        return
    }

    $appPrincipalId = "$PrincipalId".Trim()

    # ── PULL THE IMAGE ───────────────────────────────────────────────────────
    # The app is created with '--registry-identity system', so ACA pulls using this managed identity
    # rather than a stored credential. That needs AcrPull on the registry. A long-lived ACR often
    # already has broad grants that mask this; a NEWLY created one never does, and the symptom is a
    # revision that never becomes healthy - easily misread as a bad build or a wrong tag.
    if ($AcrName) {
        $acrId = Invoke-Az -AzArgs @('acr', 'show', '--name', $AcrName,
                                     '--query', 'id', '-o', 'tsv') -AllowFail
        if (-not $acrId) {
            Write-Note ("Could not resolve the ACR id for '$AcrName'; grant AcrPull to " +
                        "$appPrincipalId manually or the image pull will fail.")
        }
        else {
            Assert-RoleAssigned -PrincipalId $appPrincipalId -Role 'AcrPull' `
                                -Scope "$acrId".Trim() -ScopeLabel $AcrName
        }
    }

    # ── READ THE KEDA SECRET ─────────────────────────────────────────────────
    # The secret and the scale rule are wired as 'keyvaultref:<uri>,identityref:system', so ACA
    # resolves the broker URI USING THIS IDENTITY. Scoped to the single VAULT (not the subscription)
    # and to the read-only 'Key Vault Secrets User' role.
    if ($RabbitMqSecretUri) {
        # https://<vault>.vault.azure.net/secrets/<name>[/<version>] -> <vault>
        $vaultName = ([Uri]$RabbitMqSecretUri).Host.Split('.')[0]

        $vaultId = Invoke-Az -AzArgs @('keyvault', 'show', '--name', $vaultName,
                                       '--query', 'id', '-o', 'tsv') -AllowFail
        if (-not $vaultId) {
            Write-Note ("Could not resolve the Key Vault id for '$vaultName'; grant " +
                        "'Key Vault Secrets User' to $appPrincipalId manually or the scale rule " +
                        'cannot read the broker URI (replicas will stay at 0).')
        }
        else {
            Assert-RoleAssigned -PrincipalId $appPrincipalId -Role 'Key Vault Secrets User' `
                                -Scope "$vaultId".Trim() -ScopeLabel $vaultName
        }
    }
}

function Resolve-AppNameCollision {
    <#
        One Container App per trigger. `az containerapp create` on an existing name UPDATES it,
        so two triggers deriving the same name would silently leave one queue unserved - a fault
        that reads as a KEDA problem, not a naming one.

        Escalates only as far as needed, so unique names keep their readable form:
          1. slug(Name)                        - untouched when already unique
          2. slug(QueueName)                   - the queue is what actually distinguishes the apps
          3. slug(QueueName) + TriggerId hash   - guaranteed unique

        Mutates AppName on the planned entries and returns them. The caller's fail-loud guard
        stays in place as a backstop for the impossible case (identical TriggerIds).
    #>
    param([object[]] $Planned, [string] $Prefix)

    for ($pass = 1; $pass -le 2; $pass++) {
        $collided = @($Planned | Group-Object AppName | Where-Object { $_.Count -gt 1 })
        if (-not $collided) { break }

        foreach ($group in $collided) {
            foreach ($entry in $group.Group) {
                $t = $entry.Trigger
                $splat = @{
                    Name     = $t.QueueName      # promote the queue to primary basis
                    Fallback = $t.Name
                    Id       = $t.TriggerId
                    Prefix   = $Prefix
                }
                if ($pass -eq 2) { $splat['ForceHash'] = $true }
                $entry.AppName = Get-AppNameSlug @splat
            }
        }
    }

    return $Planned
}

function Read-TriggerFile {
    <#
        Reads a trigger .bite (plaintext JSON here - the deploy machine is where
        substitution has already happened) and projects the fields the deploy needs.
        Fails loudly on an unsubstituted release token: deriving maxReplicas from a
        token would silently deploy the wrong capacity.
    #>
    param([Parameter(Mandatory)][string] $Path)

    $raw = Get-Content -LiteralPath $Path -Raw

    if ($raw -match '#\{') {
        throw ("Trigger file '$Path' still contains an unsubstituted release token ('#{...'). " +
               'The release pipeline must substitute variables such as Concurrency before this ' +
               'script runs, because -MaxReplicas is derived from Concurrency.')
    }

    if ($raw.TrimStart().StartsWith('WFAES::') -or -not $raw.TrimStart().StartsWith('{')) {
        throw ("Trigger file '$Path' is not plaintext JSON. Pass the DECRYPTED trigger definition " +
               'to this script (it re-encrypts on staging when -EncryptStagedSettings is used). ' +
               'Windows DPAPI blobs cannot be read in the Linux container either.')
    }

    try { $json = $raw | ConvertFrom-Json }
    catch { throw "Trigger file '$Path' is not valid JSON: $($_.Exception.Message)" }

    $prefetch = 1
    if ($json.Prefetch -and [int]::TryParse([string]$json.Prefetch, [ref] $prefetch) -eq $false) { $prefetch = 1 }
    if ($json.Prefetch) { $prefetch = [Math]::Max(1, [int]$json.Prefetch) }

    return [pscustomobject]@{
        Path            = $Path
        TriggerId       = [string]$json.TriggerId
        Name            = [string]$json.Name
        QueueName       = [string]$json.QueueName
        WorkflowName    = ([string]$json.WorkflowName) -replace '\\', '/'
        Concurrency     = [int]$json.Concurrency
        Prefetch        = $prefetch
        QueueSourceId   = [string]$json.QueueSourceId
        QueueSinkId     = [string]$json.QueueSinkId
        DeadLetterQueue = [string]$json.DeadLetterQueue
    }
}

function Resolve-TriggerSet {
    <#
        Implements the three pointing modes + -TriggerId narrowing. Returns an array of
        trigger descriptors, each optionally carrying manifest overrides.
    #>
    $modes = @(
        [bool]$TriggerFilePath
        [bool]$TriggerPath
        [bool]$TriggerManifestPath
    ) | Where-Object { $_ }

    if ($modes.Count -gt 1) {
        throw ('-TriggerFilePath, -TriggerPath and -TriggerManifestPath are mutually exclusive; ' +
               'pass exactly one.')
    }

    $result = @()

    if ($TriggerManifestPath) {
        if (-not (Test-Path -LiteralPath $TriggerManifestPath)) {
            throw "Trigger manifest not found: '$TriggerManifestPath'."
        }
        $manifest = Get-Content -LiteralPath $TriggerManifestPath -Raw | ConvertFrom-Json
        if (-not $manifest.triggers -or $manifest.triggers.Count -eq 0) {
            throw "Trigger manifest '$TriggerManifestPath' declares no triggers."
        }
        foreach ($entry in $manifest.triggers) {
            $descriptor = Read-TriggerFile -Path $entry.file
            $result += [pscustomobject]@{
                Trigger  = $descriptor
                Override = $entry
            }
        }
    }
    elseif ($TriggerFilePath) {
        if (-not (Test-Path -LiteralPath $TriggerFilePath)) {
            throw "Trigger file not found: '$TriggerFilePath'."
        }
        $result += [pscustomobject]@{ Trigger = (Read-TriggerFile -Path $TriggerFilePath); Override = $null }
    }
    else {
        $folder = Read-Required -Name 'TriggerPath' -Current $TriggerPath `
                                -Hint 'folder containing the queue-trigger .bite files'
        if (-not (Test-Path -LiteralPath $folder)) {
            throw "Trigger path not found: '$folder'."
        }
        $files = @(Get-ChildItem -LiteralPath $folder -Filter $TriggerFilter -File |
                   Sort-Object Name | Select-Object -ExpandProperty FullName)
        if ($files.Count -eq 0) {
            throw ("No trigger files matching '$TriggerFilter' were found in '$folder'. " +
                   'Refusing to deploy nothing - check the folder and -TriggerFilter.')
        }
        foreach ($file in $files) {
            $result += [pscustomobject]@{ Trigger = (Read-TriggerFile -Path $file); Override = $null }
        }
    }

    if ($TriggerId) {
        $result = @($result | Where-Object { $_.Trigger.TriggerId -ieq $TriggerId })
        if ($result.Count -eq 0) {
            throw "No trigger matched -TriggerId '$TriggerId'."
        }
    }

    return $result
}

function Resolve-SourceFile {
    <#
        Finds {sourceId}.bite, falling back to a scan matched on the ID attribute so an
        operator-named source file still resolves.
    #>
    param([Parameter(Mandatory)][string] $SourceId, [Parameter(Mandatory)][string] $SearchPath)

    if (Test-Path -LiteralPath $SearchPath -PathType Leaf) { return $SearchPath }

    $direct = Join-Path $SearchPath "$SourceId.bite"
    if (Test-Path -LiteralPath $direct) { return $direct }

    foreach ($candidate in (Get-ChildItem -LiteralPath $SearchPath -Filter '*.bite' -File -ErrorAction SilentlyContinue)) {
        try {
            $xml = [xml](Get-Content -LiteralPath $candidate.FullName -Raw)
            if ($xml.Source.ID -ieq $SourceId) { return $candidate.FullName }
        }
        catch { continue }
    }

    throw ("RabbitMQ source '$SourceId' was not found under '$SearchPath'. Stage it as " +
           "'$SourceId.bite' or pass -QueueSourcePath pointing at the folder that holds it.")
}

function Get-ScaleSettings {
    <#
        The Concurrency -> replicas mapping, plus the KEDA target derived from Prefetch.
        Overrides are exception paths and are reported so a deviation is visible afterwards.
    #>
    param([Parameter(Mandatory)] $Trigger, $Override)

    $mode = if ($Override -and $Override.scalingMode) { [string]$Override.scalingMode } else { $ScalingMode }

    $max = if ($MaxReplicas -gt 0) { $MaxReplicas }
           elseif ($Override -and $Override.maxReplicas) { [int]$Override.maxReplicas }
           else { [int]$Trigger.Concurrency }

    $concurrencyPerReplica = if ($Override -and $Override.maxConcurrency) { [int]$Override.maxConcurrency }
                             else { $MaxConcurrency }

    # KEDA target ('value'): the divisor in ceil(queueLength / value), so it decides how many
    # READY messages must queue up before another replica is added.
    #
    # It must be what a replica can EXECUTE concurrently (MaxConcurrency), NOT what it can HOLD
    # (Prefetch x MaxConcurrency). Prefetch only buffers - dispatch is serial per channel - so
    # dividing by it under-parallelises against the on-prem behaviour it replaces, and only below
    # saturation, which is where bursty queues actually live:
    #
    #   Concurrency 10, Prefetch 3, 5 ready messages
    #     on-prem : 5 of the 10 always-running workers each take one -> 5 concurrent
    #     value=3 : ceil(5/3) = 2 replicas                           -> 2 concurrent  (SLOWER)
    #     value=1 : ceil(5/1) = 5 replicas                           -> 5 concurrent  (parity)
    #
    # At saturation both agree (30 messages -> 10 replicas either way), which is why this stayed
    # hidden. Trading cold starts for parallelism is a deliberate choice, so it remains available
    # through -TargetQueueLength / the manifest override rather than being the default.
    $target = if ($TargetQueueLength -gt 0) { $TargetQueueLength }
              elseif ($Override -and $Override.targetQueueLength) { [int]$Override.targetQueueLength }
              else { [Math]::Max(1, $concurrencyPerReplica) }

    $min = if ($MinReplicas -ge 0) { $MinReplicas }
           else {
               switch ($mode) {
                   'Fixed' { $max }
                   'Warm'  { 1 }
                   default { 0 }
               }
           }

    # Concurrency 0 means DISABLED on-prem (WorkerMonitor.cs:55-58); express it as a
    # zero-replica app rather than silently deploying a live consumer.
    if ([int]$Trigger.Concurrency -eq 0 -and $MaxReplicas -le 0) {
        $max = 0
        $min = 0
    }

    return [pscustomobject]@{
        Mode                  = $mode
        MinReplicas           = $min
        MaxReplicas           = $max
        TargetQueueLength     = $target
        MaxConcurrency        = $concurrencyPerReplica
        Cpu                   = if ($Override -and $Override.cpu) { [string]$Override.cpu } else { $Cpu }
        Memory                = if ($Override -and $Override.memory) { [string]$Override.memory } else { $Memory }
        IsException           = ($mode -ne 'Elastic') -or ($MaxReplicas -gt 0) -or ($MinReplicas -ge 0)
        Justification         = if ($Override -and $Override.justification) { [string]$Override.justification } else { '' }
    }
}

function Assert-TimeoutNesting {
    <#
        ENGINE__TIMEOUTSECONDS <= WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds.
        Without this, an in-flight engine call can outlive the SIGTERM drain window and the
        message is redelivered after the workflow already ran - a duplicate execution on
        every scale-in.
    #>
    if ($EngineTimeoutSeconds -gt $ShutdownGraceSeconds) {
        throw ("Timeout ordering invalid: -EngineTimeoutSeconds ($EngineTimeoutSeconds) must be <= " +
               "-ShutdownGraceSeconds ($ShutdownGraceSeconds), otherwise a scale-in produces " +
               'duplicate workflow executions.')
    }
    if ($ShutdownGraceSeconds -ge $TerminationGracePeriodSeconds) {
        throw ("Timeout ordering invalid: -ShutdownGraceSeconds ($ShutdownGraceSeconds) must be < " +
               "-TerminationGracePeriodSeconds ($TerminationGracePeriodSeconds), otherwise SIGKILL " +
               'arrives mid-drain.')
    }

    # The chain has a FOURTH member that lives on the engine, not here: host.json's functionTimeout
    # (00:10:00 = 600s). Waiting longer than the engine is willing to run is not a longer timeout,
    # it is a guaranteed one - the engine kills the invocation and the worker waits for a reply that
    # can never arrive, burning the whole budget before retrying.
    $engineFunctionTimeoutSeconds = 600
    if ($EngineTimeoutSeconds -ge $engineFunctionTimeoutSeconds) {
        throw ("Timeout ordering invalid: -EngineTimeoutSeconds ($EngineTimeoutSeconds) must be < the " +
               "engine's functionTimeout (${engineFunctionTimeoutSeconds}s, host.json). Above it the " +
               'engine aborts the invocation first and the worker can never receive a response.')
    }
    if ($EngineTimeoutSeconds -gt ($engineFunctionTimeoutSeconds * 0.75)) {
        Write-Note ("-EngineTimeoutSeconds ($EngineTimeoutSeconds) is within 25% of the engine's " +
                    "functionTimeout (${engineFunctionTimeoutSeconds}s). Leave room for the engine to " +
                    'return an error rather than being killed mid-flight.')
    }
}

function Protect-StagedSettings {
    <#
        WFAES-encrypts every staged .bite under $SettingsRoot and proves each one decrypts again.

        TWO modes, because the file types are shaped differently:
          sources\*.bite   XML with a <Source ConnectionString="..."> attribute -> ATTRIBUTE mode
          triggers\*.bite  JSON with no <Source> element                        -> WHOLE-FILE mode

        Attribute mode SILENTLY SKIPS a trigger (no <Source> element), which would leave the
        trigger - including its stored UserName/Password - PLAINTEXT inside the image layer, and
        would leave a whole-file DPAPI trigger unreadable on Linux. Both modes emit the same WFAES::
        payload under the same key, so the worker decrypts either with one KEYVAULT__SECRETNAME.
    #>
    param([Parameter(Mandatory)][string] $SettingsRoot)

    if (-not $KeyVaultName -or -not $KeyVaultSecretName) {
        throw '-EncryptStagedSettings requires -KeyVaultName and -KeyVaultSecretName.'
    }
    $encryptScript = Join-Path $ScriptDir 'Encrypt-Config.ps1'

    foreach ($file in (Get-ChildItem -LiteralPath $SettingsRoot -File -Recurse)) {
        $isTrigger = (Split-Path (Split-Path $file.FullName -Parent) -Leaf) -ieq 'triggers'
        $modeLabel = if ($isTrigger) { 'whole-file' } else { 'attribute' }

        if ($DryRun) {
            Write-Host ("      [DRYRUN] & Encrypt-Config.ps1 -FilePath '$($file.Name)' ($modeLabel)") -ForegroundColor DarkGray
            continue
        }

        $encArgs = @{
            FilePath       = $file.FullName
            VaultName      = $KeyVaultName
            SecretName     = $KeyVaultSecretName
            NonInteractive = $true
            NoBackup       = $true
        }
        if ($isTrigger) { $encArgs['WholeFile'] = $true }

        & $encryptScript @encArgs | Out-Null
        if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
            throw ("Encrypt-Config.ps1 ($modeLabel) failed for '$($file.Name)' ($LASTEXITCODE). " +
                   'The staged file would be deployed unencrypted or unreadable - refusing to continue.')
        }

        # Prove the container's key can actually read it back, IN MEMORY, before the image is
        # built. A file that encrypts but does not decrypt is a cold-start failure that would
        # otherwise only surface after deployment.
        & $encryptScript @encArgs -VerifyOnly | Out-Null
        if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
            throw ("Encrypt-Config.ps1 verify ($modeLabel) failed for '$($file.Name)' ($LASTEXITCODE). " +
                   'The Key Vault key does not round-trip this file - refusing to build the image.')
        }
    }
    Write-Ok 'Staged settings encrypted + verified (WFAES::): sources by attribute, triggers whole-file.'
}

function New-ImageBuildContext {
    <#
        Builds the docker context: a COPY of -PublishPath with Settings/{triggers,sources}
        populated for EVERY trigger in this deploy.

        Why every trigger goes into ONE image: the image is built once and shared by all the
        Container Apps; each app selects its own trigger at runtime through QUEUE__TRIGGERID. A
        per-app image would mean one ~6-minute build per trigger for no benefit.

        Why a copy: -PublishPath is the operator's publish output and must never be mutated
        (deploy-isolation rule). Staging into a copy also means a failed run leaves nothing behind.

        This replaces the previous behaviour, which staged each trigger into a per-app temp dir,
        encrypted it, verified it - and then DELETED it without ever adding it to the build context.
        The image therefore shipped with empty Settings folders and every container exited 2 with
        "No trigger files matching '*.bite' were found in '/app/Settings/triggers'".
    #>
    param(
        [Parameter(Mandatory)][string]    $PublishRoot,
        [Parameter(Mandatory)][object[]]  $Planned,
        [Parameter(Mandatory)][hashtable] $SourcePlan
    )

    # The image runs 'dotnet Warewolf.Execution.QueueProcessor.dll'. Fail here if the publish is not
    # a framework-dependent .NET publish, rather than shipping an image that cannot start. (Note the
    # extensionless Linux apphost is NOT required - a Windows publish only produces the .exe.)
    $entryDll = Join-Path $PublishRoot 'Warewolf.Execution.QueueProcessor.dll'
    if (-not (Test-Path -LiteralPath $entryDll)) {
        throw ("'$PublishRoot' does not contain Warewolf.Execution.QueueProcessor.dll. " +
               'The image entrypoint is "dotnet Warewolf.Execution.QueueProcessor.dll", so it would ' +
               'start and immediately fail. Publish the QueueProcessor to -PublishPath first.')
    }

    $ctx = New-StagingDirectory -AppName 'image'
    # -Path, NOT -LiteralPath: the trailing '*' must be expanded as a wildcard. -LiteralPath takes it
    # literally and fails with "Cannot find path '<publish>\*' because it does not exist".
    Copy-Item -Path (Join-Path $PublishRoot '*') -Destination $ctx -Recurse -Force

    $triggersDir = Join-Path $ctx 'Settings\triggers'
    $sourcesDir  = Join-Path $ctx 'Settings\sources'

    foreach ($p in $Planned) {
        $t = $p.Trigger
        # Names are the ID with a .bite extension and LOWER-CASE folders: the worker resolves these
        # on a case-sensitive filesystem.
        Copy-Item -LiteralPath $t.Path -Destination (Join-Path $triggersDir "$($t.TriggerId).bite") -Force

        # Paths come from $SourcePlan, resolved and validated at plan time, so this cannot discover
        # a missing source after apps have already been created.
        Copy-Item -LiteralPath $SourcePlan[$t.QueueSourceId] `
                  -Destination (Join-Path $sourcesDir "$($t.QueueSourceId).bite") -Force

        if ($t.QueueSinkId -and $t.QueueSinkId -ne $t.QueueSourceId) {
            Copy-Item -LiteralPath $SourcePlan[$t.QueueSinkId] `
                      -Destination (Join-Path $sourcesDir "$($t.QueueSinkId).bite") -Force
        }
    }

    # Fail here rather than shipping an image that cannot start.
    $stagedTriggers = @(Get-ChildItem -LiteralPath $triggersDir -Filter '*.bite' -File)
    if ($stagedTriggers.Count -ne $Planned.Count) {
        throw ("Staged $($stagedTriggers.Count) trigger file(s) for $($Planned.Count) planned app(s) in " +
               "'$triggersDir'. The image would start and immediately exit 2 with " +
               '"No trigger files matching ...". Refusing to build.')
    }
    Write-Ok ("Build context staged: $($stagedTriggers.Count) trigger(s) + " +
              "$(@(Get-ChildItem -LiteralPath $sourcesDir -Filter '*.bite' -File).Count) source(s) " +
              'baked into Settings/ (publish output untouched).')
    return $ctx
}

function New-StagingDirectory {
    <#
        A FRESH OS-temp directory per trigger. The publish output is never mutated
        (deploy-isolation rule, Hangfire decision #12).
    #>
    param([Parameter(Mandatory)][string] $AppName)
    # The stamp is only second-resolution, so a short unique suffix is appended: two calls
    # for the same app within one second must never share a staging directory (a shared dir
    # would let one trigger's staged .bite files leak into another's image payload).
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $unique = ([Guid]::NewGuid().ToString('N')).Substring(0, 6)
    $dir = Join-Path ([System.IO.Path]::GetTempPath()) "wwqueueprocessor-stage-$AppName-$stamp-$unique"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    # Layout must match QueueProcessorOptions (Settings / triggers / sources). Both sub-folders
    # are created even when only one is populated, so the baked image never fails on a missing
    # directory - it fails on the clear "no trigger files" error instead.
    New-Item -ItemType Directory -Force -Path (Join-Path $dir 'Settings') | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir 'Settings\triggers') | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir 'Settings\sources') | Out-Null
    return $dir
}

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ═════════════════════════════════════════════════════════════════════════════
# Phase 0 - Preflight + plan
# ═════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 0  Preflight and plan'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) was not found on PATH. Install it (>= 2.55) and re-run.'
}

$ResourceGroup       = Read-Required -Name 'ResourceGroup' -Current $ResourceGroup
$Location            = Read-Required -Name 'Location' -Current $Location -Hint 'e.g. southafricanorth'
$AcaEnvironment      = Read-Required -Name 'AcaEnvironment' -Current $AcaEnvironment -Hint 'Container Apps environment name'
$EngineBaseUrl       = Read-Required -Name 'EngineBaseUrl' -Current $EngineBaseUrl -Hint 'https://<engine>.azurewebsites.net'
$EngineResourceAppId = Read-Required -Name 'EngineResourceAppId' -Current $EngineResourceAppId -Hint "the engine's Easy Auth clientId"
$QueueSourcePath     = Read-Required -Name 'QueueSourcePath' -Current $QueueSourcePath -Hint 'folder holding the {QueueSourceId}.bite source files'

if (-not $Image) {
    $PublishPath = Read-Required -Name 'PublishPath' -Current $PublishPath `
                                 -Hint 'publish output of Warewolf.Execution.QueueProcessor (or pass -Image)'
    if (-not (Test-Path -LiteralPath $PublishPath)) {
        throw "PublishPath not found: '$PublishPath'."
    }
    $AcrName = Read-Required -Name 'AcrName' -Current $AcrName -Hint 'Azure Container Registry name (no suffix)'
}

Assert-TimeoutNesting

$effectiveScope = if ($EngineScope) { $EngineScope } else { "api://$EngineResourceAppId/.default" }
$triggerSet = Resolve-TriggerSet

# ENGINE__TENANTID must be SET, not left to chance. A blank tenant is legal only for a
# system-assigned managed identity; for anything else - a user-assigned identity, the az CLI
# credential, a client secret - it makes every credential in the chain fail with 'Invalid tenant
# id provided', which surfaces at the FIRST MESSAGE as an auth error and reads like a missing app
# role rather than missing configuration.
#
# Resolved here, AFTER Resolve-TriggerSet, on purpose: argument-shape errors (mutually exclusive
# modes, an unsubstituted release token, a filter matching nothing) are cheap and deterministic,
# so they must fail before anything queries Azure or prompts. Default from the logged-in context
# so the common case needs no parameter, and only prompt when that lookup yields nothing.
if (-not $EngineTenantId) {
    $EngineTenantId = (az account show --query tenantId -o tsv 2>$null)
    if ($LASTEXITCODE -ne 0) { $EngineTenantId = $null }
    if ($EngineTenantId) {
        $EngineTenantId = ([string]$EngineTenantId).Trim()
        if ($EngineTenantId) {
            Write-Note "EngineTenantId defaulted from the current az context: $EngineTenantId"
        }
    }
}
$EngineTenantId = Read-Required -Name 'EngineTenantId' -Current $EngineTenantId `
                                -Hint 'Entra tenant id the QueueProcessor acquires its engine token from'

if (-not $LogDir) { $LogDir = Join-Path $ScriptDir 'logs' }
if (-not $DryRun) { New-Item -ItemType Directory -Force -Path $LogDir | Out-Null }

Write-Host ''
Write-Host '  Plan' -ForegroundColor Yellow
Write-Host ("    {0,-28}: {1}" -f 'Resource group', $ResourceGroup)
Write-Host ("    {0,-28}: {1}" -f 'Location', $Location)
Write-Host ("    {0,-28}: {1}" -f 'ACA environment', $AcaEnvironment)
Write-Host ("    {0,-28}: {1}" -f 'Image', ($Image ? $Image : "build from $PublishPath -> $AcrName"))
Write-Host ("    {0,-28}: {1}" -f 'Engine', "$EngineBaseUrl (scope $effectiveScope)")
Write-Host ("    {0,-28}: {1}" -f 'Key Vault (WFAES)', ($KeyVaultName ? "$KeyVaultName / $KeyVaultSecretName" : 'not configured - staged files must be plaintext'))
Write-Host ("    {0,-28}: {1}" -f 'RabbitMQ secret (KEDA)', ($RabbitMqSecretUri ? $RabbitMqSecretUri : 'NOT SET - the scale rule cannot authenticate'))
Write-Host ("    {0,-28}: {1}" -f 'Timeouts', "engine ${EngineTimeoutSeconds}s <= drain ${ShutdownGraceSeconds}s < termination ${TerminationGracePeriodSeconds}s")
Write-Host ("    {0,-28}: {1}" -f 'Triggers resolved', $triggerSet.Count)

$plannedApps = @()
foreach ($item in $triggerSet) {
    $t = $item.Trigger
    $plannedApps += [pscustomobject]@{
        AppName  = Get-AppNameSlug -Name $t.Name -Fallback $t.QueueName -Id $t.TriggerId -Prefix $AppNamePrefix
        Trigger  = $t
        Scale    = Get-ScaleSettings -Trigger $t -Override $item.Override
        Override = $item.Override
    }
}

# Names must be settled BEFORE anything is printed, or the plan would advertise names the
# deploy will not use.
$namesBefore = @($plannedApps | ForEach-Object { $_.AppName })
$plannedApps = @(Resolve-AppNameCollision -Planned $plannedApps -Prefix $AppNamePrefix)

for ($i = 0; $i -lt $plannedApps.Count; $i++) {
    if ($plannedApps[$i].AppName -ne $namesBefore[$i]) {
        Write-Note ("Trigger '$($plannedApps[$i].Trigger.Name)': app name '$($namesBefore[$i])' collided with " +
                    "another trigger's, so it was derived from the queue instead -> " +
                    "'$($plannedApps[$i].AppName)'. Trigger display names are not unique; the queue is.")
    }
}

foreach ($planned in $plannedApps) {
    $t = $planned.Trigger
    $scale = $planned.Scale

    $note = if ($scale.MaxReplicas -eq 0) { '  (DISABLED: Concurrency = 0)' }
            elseif ($scale.IsException) { "  (EXCEPTION: $($scale.Mode)$( $scale.Justification ? " - $($scale.Justification)" : '' ))" }
            else { '' }

    Write-Host ("      - {0,-34} queue='{1}' max={2} min={3} value={4} prefetch={5}{6}" -f `
        $planned.AppName, $t.QueueName, $scale.MaxReplicas, $scale.MinReplicas, $scale.TargetQueueLength, $t.Prefetch, $note)

    # Optimum for a queue-per-app worker is Prefetch == MaxConcurrency. Dispatch is serial per
    # channel, so a larger prefetch does not raise throughput - the extra messages just park in
    # the replica, where they are invisible to the KEDA scaler (protocol=amqp counts READY
    # messages only) and are nacked back to the queue on a drain, causing redelivery. The KEDA
    # target is no longer inflated by Prefetch, so this is purely a parked-message warning.
    # Advisory, not fatal: the trigger stays the single source of truth.
    if ([int]$t.Prefetch -gt [int]$scale.MaxConcurrency -and $scale.MaxReplicas -gt 0) {
        $parked = [int]$t.Prefetch - [int]$scale.MaxConcurrency
        Write-Note ("Trigger '$($t.Name)': Prefetch=$($t.Prefetch) exceeds MaxConcurrency=$($scale.MaxConcurrency), " +
                    "so up to $parked message(s) park in each replica - invisible to the scaler and " +
                    'nacked (redelivered) if that replica drains. ' +
                    'Optimum is Prefetch == MaxConcurrency; scale OUT (maxReplicas) rather than UP (prefetch).')
    }
}

# Fail-loud backstop: after two disambiguation passes a duplicate can only mean two triggers
# share a TriggerId, which no rename can fix.
$dupes = $plannedApps | Group-Object AppName | Where-Object { $_.Count -gt 1 }
if ($dupes) {
    throw ("Derived Container App name collision that disambiguation could not resolve: " +
           "$($dupes.Name -join ', '). This means two triggers share a TriggerId. " +
           'Fix the trigger files, or pass a distinct -AppNamePrefix per deploy.')
}

# ── Resolve EVERY referenced source NOW, at plan time ─────────────────────────
# QueueSourceId (work queue) and QueueSinkId (dead-letter) are what the worker looks up in its
# cached source catalog, so a source that is not staged means a replica that cannot start - or,
# for a sink-only gap, one that starts and then cannot dead-letter.
#
# Resolved here rather than inside the deploy loop ON PURPOSE: previously a missing source for
# trigger #3 surfaced only after Container Apps for #1 and #2 had already been created, leaving a
# half-deployed fleet. Failing in the plan keeps the run all-or-nothing.
$sourcePlan = @{}          # sourceId -> resolved file path
$sourceRefs = @{}          # sourceId -> list of "<trigger> (<role>)" for the plan output
$missingSources = @()

foreach ($planned in $plannedApps) {
    $t = $planned.Trigger

    $refs = @( @{ Id = $t.QueueSourceId; Role = 'queue' } )
    if ($t.QueueSinkId -and $t.QueueSinkId -ne $t.QueueSourceId) {
        $refs += @{ Id = $t.QueueSinkId; Role = 'dead-letter' }
    }

    foreach ($ref in $refs) {
        if (-not $ref.Id) {
            $missingSources += "trigger '$($t.Name)' has no $($ref.Role) source id"
            continue
        }

        if (-not $sourceRefs.ContainsKey($ref.Id)) { $sourceRefs[$ref.Id] = @() }
        $sourceRefs[$ref.Id] += "$($t.Name) ($($ref.Role))"

        if ($sourcePlan.ContainsKey($ref.Id)) { continue }

        try {
            $sourcePlan[$ref.Id] = Resolve-SourceFile -SourceId $ref.Id -SearchPath $QueueSourcePath
        }
        catch {
            $missingSources += "$($ref.Id) - $($ref.Role) source for trigger '$($t.Name)': $($_.Exception.Message)"
        }
    }
}

if ($missingSources.Count -gt 0) {
    throw ("Unresolved RabbitMQ source(s); nothing was deployed:" + [Environment]::NewLine +
           '  - ' + ($missingSources -join ([Environment]::NewLine + '  - ')) + [Environment]::NewLine +
           "Searched '-QueueSourcePath' = '$QueueSourcePath'. Each trigger's QueueSourceId (and " +
           'QueueSinkId when it differs) must resolve to a source .bite there, either named ' +
           "'{sourceId}.bite' or carrying a matching ID attribute.")
}

Write-Host ("    {0,-28}: {1}" -f 'Sources to stage', $sourcePlan.Count)
foreach ($sid in ($sourcePlan.Keys | Sort-Object)) {
    Write-Host ("      - {0}  <- {1}   [{2}]" -f `
        $sid, (Split-Path $sourcePlan[$sid] -Leaf), ($sourceRefs[$sid] -join ', '))
}

if (-not $RabbitMqSecretUri) {
    Write-Note 'No -RabbitMqSecretUri: the KEDA rule will be created WITHOUT broker auth and cannot scale.'
}

$totalCores = ($plannedApps |
    ForEach-Object { [double]$_.Scale.Cpu * [int]$_.Scale.MaxReplicas } |
    Measure-Object -Sum).Sum
Write-Host ("    {0,-28}: {1}" -f 'Peak cores (max x cpu)', ([Math]::Round($totalCores, 2)))
Write-Note 'Confirm this fits the ACA environment/subscription core quota before a real run.'

if (-not $DryRun -and -not $NonInteractive) {
    $answer = Read-Host "  Proceed to deploy $($plannedApps.Count) Container App(s)? [y/N]"
    if ($answer -notmatch '^y') { Write-Note 'Aborted by operator.'; return }
}

# ═════════════════════════════════════════════════════════════════════════════
# Phase 1 - Infrastructure
# ═════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 1  Infrastructure (resource group, ACR, ACA environment)'

Write-Step 'Ensuring the containerapp CLI extension is present'
Invoke-Az -AzArgs @('extension', 'add', '--name', 'containerapp', '--upgrade', '--only-show-errors') -Mutating -AllowFail | Out-Null

Write-Step "Ensuring resource group '$ResourceGroup'"
Invoke-Az -AzArgs @('group', 'create', '--name', $ResourceGroup, '--location', $Location, '-o', 'none') -Mutating | Out-Null

if (-not $Image) {
    Write-Step "Ensuring ACR '$AcrName'"
    $acrExists = Invoke-Az -AzArgs @('acr', 'show', '--name', $AcrName, '--query', 'name', '-o', 'tsv') -AllowFail
    if (-not $acrExists) {
        Invoke-Az -AzArgs @('acr', 'create', '--name', $AcrName, '--resource-group', $ResourceGroup,
                            '--sku', 'Basic', '--location', $Location, '-o', 'none') -Mutating | Out-Null
    }
}

Write-Step "Ensuring ACA environment '$AcaEnvironment'"
$envExists = Invoke-Az -AzArgs @('containerapp', 'env', 'show', '--name', $AcaEnvironment,
                                 '--resource-group', $ResourceGroup, '--query', 'name', '-o', 'tsv') -AllowFail
if (-not $envExists) {
    Invoke-Az -AzArgs @('containerapp', 'env', 'create', '--name', $AcaEnvironment,
                        '--resource-group', $ResourceGroup, '--location', $Location, '-o', 'none') -Mutating | Out-Null
}
Write-Ok 'Infrastructure ready.'

# ═════════════════════════════════════════════════════════════════════════════
# Phase 2 - Image (built once, reused by every trigger)
# ═════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 2  Container image'

if ($Image) {
    Write-Ok "Reusing supplied image: $Image"
    $resolvedImage = $Image
}
else {
    if (-not $ImageTag) { $ImageTag = Get-Date -Format 'yyyyMMdd-HHmmss' }
    $resolvedImage = "$AcrName.azurecr.io/$ImageRepository`:$ImageTag"

    $dockerfile = if ($DockerfilePath) { $DockerfilePath }
                  else { Join-Path (Split-Path -Parent (Split-Path -Parent $ScriptDir)) 'Warewolf.Execution.QueueProcessor\Dockerfile' }
    if (-not (Test-Path -LiteralPath $dockerfile)) {
        throw "Dockerfile not found at '$dockerfile'. Pass -DockerfilePath explicitly."
    }

    # The context is a STAGED COPY of the publish output with Settings/ baked in - not the repo.
    # All triggers go into the single shared image; each Container App picks its own via
    # QUEUE__TRIGGERID.
    $buildContext = New-ImageBuildContext -PublishRoot $PublishPath -Planned $plannedApps -SourcePlan $sourcePlan
    try {
        if ($EncryptStagedSettings) {
            Protect-StagedSettings -SettingsRoot (Join-Path $buildContext 'Settings')
        }
        else {
            Write-Note ('Staged settings are NOT encrypted (-EncryptStagedSettings not passed): the ' +
                        'trigger and source credentials will sit in plaintext inside the image layer.')
        }

        # ACR build keeps the deploy machine free of a Docker daemon requirement.
        Write-Step "Building $resolvedImage in ACR (context '$buildContext')"
        Invoke-Az -AzArgs @('acr', 'build', '--registry', $AcrName, '--image', "$ImageRepository`:$ImageTag",
                            '--file', $dockerfile, $buildContext, '-o', 'none') -Mutating | Out-Null
        Write-Ok "Image built: $resolvedImage"
    }
    finally {
        # Always remove it: the context holds decrypted-then-re-encrypted credentials and, when
        # -EncryptStagedSettings is omitted, plaintext ones.
        if (-not $DryRun) { Remove-Item -LiteralPath $buildContext -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Phase 3/4 - Per-trigger deploy + verify
# ═════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 3  Deploy one Container App per trigger'

$results = @()

foreach ($planned in $plannedApps) {
    $t = $planned.Trigger
    $scale = $planned.Scale
    $appName = $planned.AppName

    Write-Host ''
    Write-Step "Trigger '$($t.Name)' -> app '$appName' (queue '$($t.QueueName)')"

    try {
        # Settings are already baked into the shared image (Phase 2). This app only needs its own
        # QUEUE__TRIGGERID so the worker picks the right trigger out of /app/Settings/triggers.

        # ── Environment variables ────────────────────────────────────────────
        $envVars = @(
            "QUEUE__SETTINGSPATH=/app/Settings"
            "QUEUE__TRIGGERID=$($t.TriggerId)"
            "QUEUE__TRIGGERSSUBPATH=triggers"
            "QUEUE__SOURCESSUBPATH=sources"
            "QUEUE__TRIGGERFILTER=*.bite"
            "ENGINE__BASEURL=$EngineBaseUrl"
            "ENGINE__RESOURCEAPPID=$EngineResourceAppId"
            "ENGINE__SCOPE=$effectiveScope"
            "ENGINE__TIMEOUTSECONDS=$EngineTimeoutSeconds"
            "WORKER__MAXCONCURRENCY=$($scale.MaxConcurrency)"
            "WORKER__SHUTDOWNGRACESECONDS=$ShutdownGraceSeconds"
            "WORKER__MAXDELIVERYATTEMPTS=$MaxDeliveryAttempts"
            "WORKER__RETRYENGINEINTERNALERRORS=$($RetryEngineInternalErrors.IsPresent.ToString().ToLowerInvariant())"
            "EXECUTIONLOGLEVEL=$ExecutionLogLevel"
            "ENABLECONSOLELOGGING=true"
            "ENABLEAPPLICATIONINSIGHTS=$($EnableAppInsights.IsPresent.ToString().ToLowerInvariant())"
        )
        if ($EngineTenantId) { $envVars += "ENGINE__TENANTID=$EngineTenantId" }
        if ($KeyVaultName)   { $envVars += "KEYVAULT__NAME=$KeyVaultName" }
        if ($KeyVaultSecretName) { $envVars += "KEYVAULT__SECRETNAME=$KeyVaultSecretName" }
        if ($UseSsl)         { $envVars += 'RABBITMQ__USESSL=true' }
        if ($EnableAppInsights -and $AppInsightsConnectionString) {
            $envVars += "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$AppInsightsConnectionString"
        }

        $exists = Invoke-Az -AzArgs @('containerapp', 'show', '--name', $appName,
                                      '--resource-group', $ResourceGroup, '--query', 'name', '-o', 'tsv') -AllowFail

        # When the app ALREADY exists, grant its identity the roles BEFORE touching the image.
        # 'containerapp update --image' validates the registry pull SYNCHRONOUSLY and rejects the
        # revision if the identity lacks AcrPull:
        #   Field 'template.containers.<app>.image' is invalid ... UNAUTHORIZED: authentication
        #   required ... scope=repository:<repo>:pull
        # That happens before any later grant could run, so a partially-deployed app (created by an
        # interrupted run, identity present but ungranted) could never be repaired by re-running.
        # 'create' does not hit this because ACA provisions the first revision asynchronously and
        # retries the pull, by which time the grant below has landed.
        if ($exists) {
            $existingPrincipalId = Invoke-Az -AzArgs @('containerapp', 'show', '--name', $appName,
                                                       '--resource-group', $ResourceGroup,
                                                       '--query', 'identity.principalId', '-o', 'tsv') -AllowFail
            if ($existingPrincipalId) { Grant-AppIdentityRoles -PrincipalId $existingPrincipalId }

            # Ensure the registry is wired for managed-identity pull. 'create' passes
            # --registry-server/--registry-identity, but the UPDATE path never did - so an app whose
            # create was interrupted (or one created with -Image, which skips those flags) carries NO
            # configuration.registries entry at all. ACA then pulls anonymously and every image
            # update fails with UNAUTHORIZED even though the identity holds AcrPull, which makes the
            # app permanently unrepairable by re-running. Idempotent.
            if ($AcrName) {
                Invoke-Az -AzArgs @('containerapp', 'registry', 'set', '--name', $appName,
                                    '--resource-group', $ResourceGroup,
                                    '--server', "$AcrName.azurecr.io",
                                    '--identity', 'system', '-o', 'none') -Mutating -AllowFail | Out-Null
                Write-Ok "Registry '$AcrName.azurecr.io' wired for managed-identity pull."
            }
        }

        $common = @(
            '--name', $appName, '--resource-group', $ResourceGroup,
            '--image', $resolvedImage,
            '--cpu', $scale.Cpu, '--memory', $scale.Memory,
            '--min-replicas', $scale.MinReplicas, '--max-replicas', $scale.MaxReplicas
        )

        if (-not $exists) {
            # NO --ingress flag. A queue worker takes no inbound HTTP, and ingress is disabled by
            # default when the flag is omitted. '--ingress disabled' is NOT valid: the CLI accepts
            # only 'internal' or 'external' and fails the create with
            #   "'disabled' is not a valid value for '--ingress'".
            # --env-vars ON CREATE, not only in the later update. The worker validates its options at
            # startup (DataAnnotations on QueueProcessorOptions), so a revision created without them
            # dies immediately:
            #   FATAL: OptionsValidationException ... 'Engine:ResourceAppId is required'
            # That first revision then crash-looped on every single deploy until the follow-up
            # 'update --set-env-vars' produced a second, healthy revision. Harmless in the end, but it
            # filled the logs with a fatal error that looks exactly like a real misconfiguration.
            $createArgs = @('containerapp', 'create') + $common + @(
                '--environment', $AcaEnvironment,
                '--system-assigned',
                '--env-vars') + $envVars + @(
                '-o', 'none'
            )
            if (-not $Image) { $createArgs += @('--registry-server', "$AcrName.azurecr.io", '--registry-identity', 'system') }
            Invoke-Az -AzArgs $createArgs -Mutating | Out-Null
            Write-Ok "Created Container App '$appName'."
        }
        else {
            Invoke-Az -AzArgs (@('containerapp', 'update') + $common + @('-o', 'none')) -Mutating | Out-Null
            Write-Ok "Updated Container App '$appName'."
        }

        # ── Resolve the app identity FIRST ───────────────────────────────────
        # Everything below needs it, and the Key Vault grant in particular MUST land before the
        # secret is set (see the retry note on the secret itself).
        $principalId = Invoke-Az -AzArgs @('containerapp', 'show', '--name', $appName,
                                           '--resource-group', $ResourceGroup,
                                           '--query', 'identity.principalId', '-o', 'tsv') -AllowFail

        Grant-AppIdentityRoles -PrincipalId $principalId
        Wait-RbacDataPlaneSettle -PrincipalId $principalId

        # ── Secret (Key Vault reference) + env vars + KEDA rule ──────────────
        # ORDER IS LOAD-BEARING: 'secret set' with a keyvaultref makes ACA resolve the secret
        # IMMEDIATELY, using the app identity, and it rejects the whole revision if it cannot.
        # Granting afterwards fails hard with
        #   Field 'configuration.secrets' is invalid ... Unable to get value using Managed identity
        #   system for secret rabbitmq-connection. Error: unable to fetch secret
        # Azure RBAC is also eventually consistent, so even in the right order the first attempt can
        # lose a race with propagation - hence the retry rather than a blind sleep.
        if ($RabbitMqSecretUri -and $InlineRabbitMqSecret) {
            # ── CAE workaround: copy the value at DEPLOY time instead of referencing it ────────
            # A keyvaultref makes the ACA control plane fetch the secret with the app's managed
            # identity on every sync. Where the tenant enforces Continuous Access Evaluation that
            # fetch fails, repeatedly:
            #   401 AKV10203: Continuous access evaluation check failed. Please extract the claims
            #   challenge from the www-authenticate header to fetch a new token (CaeAuthorizationFailed)
            # ACA's sync path does not implement the claims-challenge exchange. Note the WORKER's own
            # Key Vault access is unaffected - the Azure SDK inside the container does implement it,
            # which is why the replica still decrypts its .bite files while this sync fails.
            #
            # Key Vault stays the source of truth; only delivery changes from a runtime reference to
            # a deploy-time copy, read here with the OPERATOR's credentials (which are CAE-capable).
            # TRADE-OFF: rotating the secret in Key Vault no longer propagates on its own - the apps
            # must be redeployed. That is why this is opt-in rather than the default.
            $secretName = ([Uri]$RabbitMqSecretUri).Segments[-1].Trim('/')
            $vaultName  = ([Uri]$RabbitMqSecretUri).Host.Split('.')[0]
            $uriValue   = Invoke-Az -AzArgs @('keyvault', 'secret', 'show', '--vault-name', $vaultName,
                                              '--name', $secretName, '--query', 'value', '-o', 'tsv')
            if (-not "$uriValue".Trim()) {
                throw ("Could not read '$secretName' from Key Vault '$vaultName' for -InlineRabbitMqSecret. " +
                       'Without the broker URI the KEDA rule cannot authenticate and replicas stay at 0.')
            }

            Invoke-Az -AzArgs @('containerapp', 'secret', 'set', '--name', $appName,
                                '--resource-group', $ResourceGroup,
                                '--secrets', "rabbitmq-connection=$("$uriValue".Trim())",
                                '-o', 'none') -Mutating -Sensitive | Out-Null
            Write-Ok "Secret 'rabbitmq-connection' set inline from Key Vault '$vaultName' (no runtime keyvaultref, so CAE cannot block it)."
        }
        elseif ($RabbitMqSecretUri) {
            $secretArgs = @('containerapp', 'secret', 'set', '--name', $appName,
                            '--resource-group', $ResourceGroup,
                            '--secrets', "rabbitmq-connection=keyvaultref:$RabbitMqSecretUri,identityref:system",
                            '-o', 'none')
            $attempt = 0
            $maxAttempts = 6          # ~2.5 min total, comfortably beyond typical RBAC propagation
            while ($true) {
                $attempt++
                try {
                    Invoke-Az -AzArgs $secretArgs -Mutating | Out-Null
                    if ($attempt -gt 1) { Write-Ok "Key Vault secret reference accepted (attempt $attempt)." }
                    break
                }
                catch {
                    if ($attempt -ge $maxAttempts -or $DryRun) { throw }
                    Write-Note ("Key Vault reference not resolvable yet (attempt $attempt/$maxAttempts) - " +
                                'waiting 25s for the RBAC grant to propagate.')
                    Start-Sleep -Seconds 25
                }
            }
        }

        Invoke-Az -AzArgs (@('containerapp', 'update', '--name', $appName, '--resource-group', $ResourceGroup,
                             '--set-env-vars') + $envVars + @('-o', 'none')) -Mutating | Out-Null

        # ACTUALLY APPLY the termination grace period. It was previously validated against
        # ShutdownGraceSeconds and printed in the plan, but never sent to Azure - so every deployment
        # silently ran on ACA's 30s default while the plan claimed a longer drain budget, and SIGKILL
        # could arrive mid-drain. It is a TEMPLATE property, not an env var, so --set-env-vars cannot
        # carry it; it needs its own flag.
        Invoke-Az -AzArgs @('containerapp', 'update', '--name', $appName, '--resource-group', $ResourceGroup,
                            '--termination-grace-period', "$TerminationGracePeriodSeconds",
                            '-o', 'none') -Mutating | Out-Null
        Write-Ok "Termination grace period set to ${TerminationGracePeriodSeconds}s (drain budget ${ShutdownGraceSeconds}s)."

        if ($scale.MaxReplicas -gt 0 -and $RabbitMqSecretUri) {
            $scaleArgs = @(
                'containerapp', 'update', '--name', $appName, '--resource-group', $ResourceGroup,
                '--scale-rule-name', 'rabbitmq-backlog',
                '--scale-rule-type', 'rabbitmq',
                '--scale-rule-metadata', "queueName=$($t.QueueName)", 'mode=QueueLength',
                                         "value=$($scale.TargetQueueLength)", 'protocol=amqp',
                '--scale-rule-auth', 'host=rabbitmq-connection',
                '-o', 'none'
            )
            Invoke-Az -AzArgs $scaleArgs -Mutating | Out-Null
            Write-Ok "KEDA rule set: ceil(queueLength / $($scale.TargetQueueLength)) capped at $($scale.MaxReplicas)."
        }
        elseif ($scale.MaxReplicas -eq 0) {
            Write-Note 'Concurrency = 0: app deployed with 0 replicas (disabled). No scale rule added.'
        }

        $results += [pscustomobject]@{
            trigger        = $t.Name
            triggerId      = $t.TriggerId
            app            = $appName
            queue          = $t.QueueName
            workflow       = $t.WorkflowName
            minReplicas    = $scale.MinReplicas
            maxReplicas    = $scale.MaxReplicas
            targetLength   = $scale.TargetQueueLength
            prefetch       = $t.Prefetch
            scalingMode    = $scale.Mode
            principalId    = ($principalId ? "$principalId".Trim() : $null)
            image          = $resolvedImage
            status         = 'deployed'
        }
    }
    catch {
        Write-Bad "Trigger '$($t.Name)' failed: $($_.Exception.Message)"
        $results += [pscustomobject]@{
            trigger = $t.Name; triggerId = $t.TriggerId; app = $appName
            queue = $t.QueueName; status = 'failed'; error = $_.Exception.Message
        }

        if (-not $ContinueOnTriggerError) {
            Write-Bad 'Aborting (fail-fast). Pass -ContinueOnTriggerError to deploy the remaining triggers.'
            break
        }
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Phase 5 - Summary
# ═════════════════════════════════════════════════════════════════════════════
Write-Phase 'Phase 5  Summary'

$results | Format-Table trigger, app, queue, minReplicas, maxReplicas, targetLength, status -AutoSize

$failed = @($results | Where-Object { $_.status -eq 'failed' })

if (-not $DryRun) {
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $summaryPath = Join-Path $LogDir "deploy-WwQueueProcessor-$stamp.summary.json"
    [pscustomobject]@{
        timestampUtc          = (Get-Date).ToUniversalTime().ToString('o')
        resourceGroup         = $ResourceGroup
        location              = $Location
        acaEnvironment        = $AcaEnvironment
        image                 = $resolvedImage
        engineBaseUrl         = $EngineBaseUrl
        engineScope           = $effectiveScope
        deployRabbitMqTriggers = $true
        queueProcessorApps    = $results
    } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
    Write-Ok "Summary written: $summaryPath"
}

# ── Resources manipulated, with reachable URLs ───────────────────────────────
# Printed on every run, including a partial one, because a half-finished fan-out is exactly when you
# need to know which apps exist. Driven off $results, which is also what the summary JSON records.
$verb = $DryRun ? 'WOULD CREATE' : 'CREATED'
# This script has no -SubscriptionId parameter, so resolve it for the portal links rather than
# emitting 'subscriptions//resourceGroups'. -AllowFail keeps a broken link from failing the deploy.
$subIdForLinks = Invoke-Az -AzArgs @('account', 'show', '--query', 'id', '-o', 'tsv') -AllowFail
$subIdForLinks = "$subIdForLinks".Trim()
Write-Host ''
Write-Host '  -- Resources manipulated --' -ForegroundColor Cyan
Write-Host "     $verb" -ForegroundColor Green
Write-Host "       ACR repository: $ImageRepository  (image $resolvedImage)" -ForegroundColor Green
foreach ($r in $results) {
    # A FAILED entry carries only trigger/triggerId/app/queue/status/error - the scale and identity
    # fields never got assigned - so every optional field is probed before printing.
    $failedRow = $r.status -eq 'failed'
    $has = { param($n) [bool]$r.PSObject.Properties[$n] -and $null -ne $r.PSObject.Properties[$n].Value }
    Write-Host ("       {0}Container App: {1}" -f $(if ($failedRow) { 'FAILED  ' } else { '' }), $r.app) `
               -ForegroundColor $(if ($failedRow) { 'Red' } else { 'Green' })
    Write-Host ("         trigger '{0}' queue '{1}'{2}" -f $r.trigger, $r.queue,
                $(if (& $has 'workflow') { " -> $($r.workflow)" } else { '' })) -ForegroundColor DarkGray
    if (& $has 'maxReplicas') {
        Write-Host ("         scale: min={0} max={1} value={2} prefetch={3} mode={4}" -f `
                    $r.minReplicas, $r.maxReplicas, $r.targetLength, $r.prefetch, $r.scalingMode) -ForegroundColor DarkGray
    }
    if (& $has 'principalId') {
        Write-Host ("         identity {0}  (granted AcrPull + Key Vault Secrets User)" -f $r.principalId) -ForegroundColor DarkGray
    }
    if ($failedRow -and (& $has 'error')) {
        Write-Host ("         error: {0}" -f $r.error) -ForegroundColor Red
    } else {
        Write-Host ("         Portal : https://portal.azure.com/#@/resource/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.App/containerApps/{2}" -f `
                    $subIdForLinks, $ResourceGroup, $r.app) -ForegroundColor Blue
    }
}
Write-Host '     REUSED (pre-existing - teardown will NOT remove these)' -ForegroundColor Gray
Write-Host "       ACA environment : $AcaEnvironment" -ForegroundColor Gray
Write-Host "       Container registry: $AcrName" -ForegroundColor Gray
Write-Host "       Resource group  : $ResourceGroup" -ForegroundColor Gray
if ($KeyVaultName) { Write-Host "       Key Vault       : $KeyVaultName (secret $KeyVaultSecretName)" -ForegroundColor Gray }
Write-Host ''
Write-Host '     ENDPOINTS' -ForegroundColor Blue
Write-Host "       Engine          : $EngineBaseUrl" -ForegroundColor Blue
Write-Host "       Engine scope    : $effectiveScope" -ForegroundColor Blue
foreach ($r in ($results | Where-Object { $_.PSObject.Properties['workflow'] -and $_.workflow })) {
    Write-Host ("       Workflow posted : {0}/secure/{1}.json" -f $EngineBaseUrl.TrimEnd('/'), $r.workflow) -ForegroundColor Blue
}
Write-Host ''

Write-Note 'NEXT (required): grant EACH app''s managed identity the engine app role Warewolf_QueueProcessor.'
Write-Note 'A per-workflow secure.config row is NOT required, and adding one can LOCK THE WORKER OUT: a'
Write-Note 'workflow uses the resource role map exclusively once it has any Execute-bearing resource entry,'
Write-Note 'otherwise it inherits the global (IsServer=true) map - which the app role above satisfies. Only'
Write-Note 'add a resource row if you intend to NARROW access, and then it must name Warewolf_QueueProcessor.'
Write-Note 'See Deploy-EndToEnd-Runbook.md section 8 (8d) for the loop.'

if ($failed.Count -gt 0) {
    throw "$($failed.Count) of $($results.Count) trigger deploy(s) failed - see the table above."
}

# Probing reads run with -AllowFail (e.g. 'containerapp show' for an app that does not exist yet),
# which leaves $LASTEXITCODE non-zero even on a fully successful run. Without this a caller - CI, or
# a wrapper checking $LASTEXITCODE - would read a clean deploy as a failure.
$global:LASTEXITCODE = 0
exit 0

