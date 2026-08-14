#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end load test for the Warewolf Execution Engine + QueueProcessor path: publishes N
    messages to a RabbitMQ/LavinMQ queue, watches the Container App replicas drain them, and
    reconciles the worker logs, the engine logs and the database rows against what was published.

.DESCRIPTION
    Reproduces RUN 2 (2026-08-13): 100 messages, 6 replicas, 100/100 succeeded, 0 dead-lettered,
    ~90 seconds. Every default is that run's value - see WwLoadTest.Defaults.psd1 - so pressing
    Enter through the prompts repeats it exactly.

    WHAT THIS SCRIPT IS FOR
    Not "did it work" - the queue drains either way. The contract is 2xx -> ack and non-2xx ->
    dead-letter AND ack, so a run that discarded a third of its messages leaves an empty queue and
    replicas scaled back to zero, looking identical to a clean one. RUN 1 did exactly that: 68 of
    100 processed, 32 dead-lettered, queue empty, no errors anywhere obvious. The only way to tell
    the two apart is to reconcile a manifest of what was PUBLISHED against three independent
    sources of what happened - and that is what this script does.

    THE FOUR BUCKETS
    Every published message lands in exactly one, and the four must sum to the published count:

        Clean     - a database row, no dead-letter.            The workflow ran and committed.
        DlqOnly   - dead-lettered, no database row.            It never reached the workflow.
        Both      - a database row AND a dead-letter.          The workflow COMMITTED and the
                                                              response was lost. Replaying this
                                                              message duplicates committed work.
        Neither   - no row, no dead-letter.                    Silently lost. The worst outcome and
                                                              the one a drained queue hides.

    PHASES
        0  Azure login                     - who, which subscription, which tenant
        1  Mode                            - use an existing deployment, or deploy first
        2  Targets                         - prompt for each value, RUN 2 default pre-filled
        3  Pre-flight                      - Engine / ACA / KEDA / Broker / Database, then a gate
        4  Baseline                        - optional queue purge, and always a row watermark
        5  Pre-warm                        - Consumption-plan cold start is 64 s; do not skip
        6  Publish                         - N unique messages plus a manifest
        7  Drain                           - poll until ROWS stop rising, not until the queue empties
        8  Report                          - worker logs, engine logs, database, reconciled
        9  Verdict                         - PASS/FAIL with the failing criteria named

    NOTHING IS TRUNCATED. The baseline takes a MAX(JobLogId) watermark and every query counts only
    above it, which isolates the run without destroying anyone else's data.

.PARAMETER MessageCount
    How many valid messages to publish. This is the knob the run is sized by.

.PARAMETER FailureCount
    Additional messages with an EMPTY body, which fail deterministically in the workflow's stored
    procedure. Use to exercise the dead-letter path on purpose; these are EXPECTED to dead-letter
    and are scored as passes when they do.

.PARAMETER Mode
    Existing     - verify and run against what is already deployed (default; only mutations are the
                   optional purge and the published messages)
    DeployWorker - deploy the QueueProcessor Container App first, then run
    DeployAll    - deploy the Execution Engine and the QueueProcessor first, then run
    Omit it and the script asks.

.PARAMETER SqlConnectionString
    Resolution order: this parameter, then $env:WWLOADTEST_SQLCONNECTION, then a masked prompt,
    then the database phases are SKIPPED and marked as such. Never logged; only the server and
    database names are displayed.

.EXAMPLE
    # The reviewer's default: reproduce RUN 2 interactively, pressing Enter through every prompt.
    .\Invoke-WwQueueLoadTest.ps1

.EXAMPLE
    # A quick 20-message smoke test against the existing deployment, no questions asked.
    .\Invoke-WwQueueLoadTest.ps1 -MessageCount 20 -Mode Existing -Yes

.EXAMPLE
    # Full 100, with 5 deliberate failures to prove the dead-letter path still works.
    .\Invoke-WwQueueLoadTest.ps1 -MessageCount 100 -FailureCount 5 -PurgeQueues -Yes

.EXAMPLE
    # Deploy the worker at a different replica ceiling, then run. Mutates the shared resource group.
    .\Invoke-WwQueueLoadTest.ps1 -Mode DeployWorker -MaxReplicas 8 -MessageCount 100

.EXAMPLE
    # CI / unattended: every value from the defaults file, no prompts, exit code carries the verdict.
    .\Invoke-WwQueueLoadTest.ps1 -MessageCount 100 -NonInteractive
#>
[CmdletBinding()]
param(
    # ── Run shape ──────────────────────────────────────────────────────────────
    # NULLABLE ON PURPOSE. A plain [int] that is not supplied defaults to 0, which is
    # indistinguishable from someone deliberately passing 0 - so every unbound int would silently
    # override its RUN 2 default with zero, and the run would publish nothing at zero replicas.
    # $null means "not supplied, use the default"; 0 means 0.
    [ValidateRange(1, 100000)][Nullable[int]] $MessageCount,
    [ValidateRange(0, 100000)][Nullable[int]] $FailureCount,
    [ValidateSet('Existing', 'DeployWorker', 'DeployAll')][string] $Mode,
    [string] $RunLabel,

    # ── Targets (each prompts with the RUN 2 default when omitted) ─────────────
    [string] $ResourceGroup,
    [string] $EngineAppName,
    [string] $EngineAppId,
    [string] $EngineTenantId,
    [string] $EngineAppInsightsName,
    [string] $WorkerAppName,
    [string] $QueueName,
    [string] $DeadLetterQueue,
    [ValidateRange(0, 1000)][Nullable[int]] $MaxReplicas,
    [ValidateRange(1, 1000)][Nullable[int]] $MaxConcurrency,

    # ── Broker ─────────────────────────────────────────────────────────────────
    [string] $AmqpUri,
    [string] $BrokerSourceBitePath,
    [string] $WorkerPublishPath,

    # ── Database ───────────────────────────────────────────────────────────────
    [string] $SqlConnectionString,
    [string] $JobTable,
    [switch] $SkipDatabase,

    # ── Behaviour ──────────────────────────────────────────────────────────────
    [switch] $PurgeQueues,
    [switch] $SkipPreWarm,
    [switch] $Yes,
    [switch] $NonInteractive,
    [string] $OutputDir,
    [string] $DefaultsPath,

    # Dot-source the pure helpers for unit testing. No Azure, no broker, no database.
    [switch] $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ═════════════════════════════════════════════════════════════════════════════
# Pure helpers - unit-testable, no external dependencies
# ═════════════════════════════════════════════════════════════════════════════

function Get-WwLoadTestDefaults {
    <#
        Loads WwLoadTest.Defaults.psd1. Import-PowerShellDataFile is used rather than dot-sourcing
        because it parses the file as DATA - a defaults file that someone has slipped executable
        code into simply fails to load instead of running it.
    #>
    [CmdletBinding()]
    param([string] $Path)

    if (-not $Path) { $Path = Join-Path $PSScriptRoot 'WwLoadTest.Defaults.psd1' }
    if (-not (Test-Path -LiteralPath $Path)) { throw "Defaults file not found: $Path" }
    return Import-PowerShellDataFile -LiteralPath $Path
}

function Resolve-WwEngineConcurrency {
    <#
        Concurrent engine requests = maxReplicas x WORKER__MAXCONCURRENCY.

        This is THE number that decides whether the run succeeds. The engine is on a Consumption
        (Y1) plan with roughly 1.5 GB per instance and each concurrent workflow execution costs
        ~32 MB of loaded activity tree. Measured against wwengine-e2e-ldi413:

            6  -> 24/24 clean
            8  -> 24/24 clean, max 4.7 s
            10 -> 26/30, 'Insufficient memory to continue the execution of the program'

        Both operands matter. Halving the replicas while doubling MaxConcurrency changes nothing.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int] $MaxReplicas,
        [Parameter(Mandatory)][int] $MaxConcurrency
    )
    if ($MaxReplicas -lt 0 -or $MaxConcurrency -lt 1) {
        throw "Invalid scale: MaxReplicas=$MaxReplicas MaxConcurrency=$MaxConcurrency."
    }
    return $MaxReplicas * $MaxConcurrency
}

function Read-WwSetting {
    <#
        One prompt, pre-filled with the RUN 2 default. Enter accepts it.

        -Provided wins over everything so an explicit parameter is never second-guessed, and
        -NonInteractive silently takes the default so the same script runs unattended.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Prompt,
        $Default,
        $Provided,
        [switch] $NonInteractive,
        [switch] $AsInt
    )
    # 0 and $false are legitimate provided values; only $null and '' mean "not supplied".
    $hasProvided = ($null -ne $Provided) -and ($Provided -isnot [string] -or $Provided -ne '')
    if ($hasProvided) { return $(if ($AsInt) { [int]$Provided } else { $Provided }) }
    if ($NonInteractive) { return $(if ($AsInt) { [int]$Default } else { $Default }) }

    $answer = Read-Host "  $Prompt [$Default]"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $(if ($AsInt) { [int]$Default } else { $Default }) }
    if ($AsInt) {
        $parsed = 0
        if (-not [int]::TryParse($answer.Trim(), [ref]$parsed)) { throw "'$answer' is not a whole number." }
        return $parsed
    }
    return $answer.Trim()
}

function Get-WwMaskedConnectionString {
    <#
        Server + database + auth mode, and NOTHING else. The full string is never printed, never
        written to an artefact and never passed on a command line.
    #>
    [CmdletBinding()]
    param([string] $ConnectionString)

    if ([string]::IsNullOrWhiteSpace($ConnectionString)) { return $null }
    $parts = @{}
    foreach ($kv in ($ConnectionString -split ';')) {
        if ($kv -match '^\s*([^=]+)=(.*)$') { $parts[$Matches[1].Trim().ToLowerInvariant()] = $Matches[2].Trim() }
    }
    $server = $parts['server']; if (-not $server) { $server = $parts['data source'] }
    $db     = $parts['database']; if (-not $db) { $db = $parts['initial catalog'] }
    $auth   = if ($parts['authentication']) { $parts['authentication'] }
              elseif ($parts['integrated security'] -in @('true', 'sspi', 'yes')) { 'Integrated' }
              elseif ($parts['user id'] -or $parts['uid']) { 'SqlLogin (' + ($parts['user id'] ?? $parts['uid']) + ')' }
              else { 'unknown' }

    [pscustomobject]@{ Server = $server; Database = $db; Auth = $auth }
}

function Get-WwBrokerSourceDiagnosis {
    <#
        Says WHY a RabbitMQ source .bite could not yield a broker URI.

        Resolve-E2EBrokerUri returns $null for six different reasons - missing file, unreadable XML,
        no ConnectionString, WFAES, undecryptable DPAPI, and a connection string with no HostName -
        and a caller that picks one of them to put in the error message will eventually pick wrong.
        An earlier version asserted "a WFAES-encrypted source needs the Key Vault key" for a path
        that was simply MISTYPED, sending the operator after a Key Vault problem that did not exist.

        The 'did you mean' list is the whole point: a wrong filename is by far the most common
        failure here, and the fix is visible the moment the neighbouring .bite files are listed.
    #>
    [CmdletBinding()]
    param([string] $Path)

    function New-Diag { param($Reason, $Detail, $Hint = $null)
        [pscustomobject]@{ Reason = $Reason; Detail = $Detail; Hint = $Hint }
    }

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return New-Diag 'NoPath' 'No source path was supplied.' 'Pass -BrokerSourceBitePath or -AmqpUri.'
    }

    if (-not (Test-Path -LiteralPath $Path)) {
        $dir = Split-Path -Parent $Path
        $hint = if ($dir -and (Test-Path -LiteralPath $dir)) {
            $siblings = @(Get-ChildItem -LiteralPath $dir -Filter '*.bite' -File -ErrorAction SilentlyContinue |
                            ForEach-Object Name)
            if ($siblings.Count -gt 0) { "Files in '$dir': " + ($siblings -join ', ') }
            else { "Directory '$dir' exists but contains no .bite files." }
        }
        elseif ($dir) { "Directory '$dir' does not exist either." }
        else { $null }
        return New-Diag 'FileNotFound' "The file does not exist: '$Path'." $hint
    }

    try { $xml = [xml](Get-Content -LiteralPath $Path -Raw) }
    catch { return New-Diag 'NotXml' "The file is not readable as XML: $($_.Exception.Message)" `
                            'A queue TRIGGER is JSON; the broker SOURCE is XML with a <Source> element.' }

    $cs = $null
    if ($xml.PSObject.Properties['Source'] -and $xml.Source) {
        $node = $xml.Source
        if ($node.PSObject.Properties['ConnectionString']) { $cs = [string]$node.ConnectionString }
    }
    if ([string]::IsNullOrWhiteSpace($cs)) {
        return New-Diag 'NoConnectionString' 'The <Source> element has no ConnectionString attribute.' `
                        'Confirm this is a RabbitMQSource export and not another resource type.'
    }

    if ($cs.StartsWith('WFAES::', [StringComparison]::OrdinalIgnoreCase)) {
        return New-Diag 'WfAesEncrypted' 'The ConnectionString is WFAES-encrypted.' `
                        ('Decrypt it with Encrypt-Config.ps1 -Decrypt (Key Vault key required), or pass -AmqpUri.')
    }

    if ($cs -notmatch '(?i)HostName=') {
        return New-Diag 'DpapiUndecryptable' 'The ConnectionString looks DPAPI-encrypted and could not be decrypted here.' `
                        ('DPAPI is Windows- and machine/user-scoped, so it only decrypts on the machine and account ' +
                         'that created it. Re-export the source as plaintext, or pass -AmqpUri.')
    }

    if ($cs -notmatch '(?i)(^|;)\s*HostName\s*=\s*[^;]+') {
        return New-Diag 'NoHostName' 'The ConnectionString is readable but has no HostName.' $null
    }

    # Reached when nothing is demonstrably wrong. Says so, rather than inventing a fault: the
    # function is also useful on its own to check a source before a run, and "no URI was produced"
    # would be a false accusation against a perfectly good file.
    return New-Diag 'LooksResolvable' 'The source parses and carries a HostName; nothing here explains a failure.' `
                    'If resolution still failed, check UserName / Password / Port / VirtualHost in the ConnectionString.'
}

function Test-WwRequiredPath {
    <#
        Validates a path AT PROMPT TIME rather than several hundred lines later.

        A mistyped publish directory or source file used to surface in Phase 3, after the Azure and
        engine checks had already run - so the operator waited through the slow part to be told about
        a typo they made at the start.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Label,
        [string] $Path,
        [ValidateSet('File', 'Directory')][string] $Kind = 'File',
        [string] $MustContain
    )
    if ([string]::IsNullOrWhiteSpace($Path)) { return "$Label was not supplied." }
    if (-not (Test-Path -LiteralPath $Path)) {
        $parent = if ($Kind -eq 'Directory') { Split-Path -Parent $Path } else { Split-Path -Parent $Path }
        $hint = ''
        if ($parent -and (Test-Path -LiteralPath $parent)) {
            $near = @(Get-ChildItem -LiteralPath $parent -ErrorAction SilentlyContinue |
                        Select-Object -First 12 | ForEach-Object Name)
            if ($near.Count) { $hint = "  Found in '$parent': " + ($near -join ', ') }
        }
        return "$Label does not exist: '$Path'.$hint"
    }
    if ($MustContain -and -not (Test-Path -LiteralPath (Join-Path $Path $MustContain))) {
        return "$Label ('$Path') does not contain '$MustContain'."
    }
    return $null
}

function ConvertTo-WwDateTimeOrNull {
    <#
        Parses one of the ISO-8601 timestamp strings the SQL layer returns, or yields $null.

        NOT [datetime]::TryParse($s, [ref]$x). PowerShell cannot bind [ref] to an untyped variable
        holding $null and fails overload resolution outright:

            Cannot find an overload for "TryParse" and the argument count: "2".

        That threw in Phase 8 - after a full run had already been published, drained and reported -
        so the expensive part was done and the numbers were lost. Hence a helper with a test.

        Timestamps are converted to strings in SQL (CONVERT style 126) rather than returned as
        datetimes, because Windows PowerShell 5.1's ConvertTo-Json renders a DateTime as
        "/Date(1234567890)/", which arrives here as an unparseable string.
    #>
    [CmdletBinding()]
    param([string] $Value)

    if ([string]::IsNullOrWhiteSpace($Value)) { return $null }
    return ($Value -as [datetime])
}

function Get-WwPercentile {
    <#
        Nearest-rank percentile. Returns $null for an empty set rather than 0, because 0 ms would
        read as an implausibly fast run rather than as an absence of data.
    #>
    [CmdletBinding()]
    param([double[]] $Values, [ValidateRange(0, 100)][double] $Percentile = 50)

    $v = @($Values | Where-Object { $null -ne $_ } | Sort-Object)
    if ($v.Count -eq 0) { return $null }
    $rank = [math]::Ceiling(($Percentile / 100.0) * $v.Count)
    if ($rank -lt 1) { $rank = 1 }
    return $v[[int]$rank - 1]
}

function Get-WwReconciliationBuckets {
    <#
        Classifies every PUBLISHED message into exactly one of four buckets, and separately reports
        anything found in the database or the dead-letter queue that was never published.

        Driven off the published manifest rather than off the observed rows, because the message
        that matters most - published, never processed, no trace anywhere - exists in NO observed
        source and can only be found by its absence.
    #>
    [CmdletBinding()]
    param(
        [object[]] $Manifest,
        [string[]] $DbTxns,
        [string[]] $DlqTxns
    )
    $db  = [System.Collections.Generic.HashSet[string]]::new([string[]]@($DbTxns  | Where-Object { $_ }), [StringComparer]::OrdinalIgnoreCase)
    $dlq = [System.Collections.Generic.HashSet[string]]::new([string[]]@($DlqTxns | Where-Object { $_ }), [StringComparer]::OrdinalIgnoreCase)

    $clean = [System.Collections.Generic.List[object]]::new()
    $dlqOnly = [System.Collections.Generic.List[object]]::new()
    $both = [System.Collections.Generic.List[object]]::new()
    $neither = [System.Collections.Generic.List[object]]::new()
    $wrongOutcome = [System.Collections.Generic.List[object]]::new()

    $publishedTxns = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

    foreach ($m in @($Manifest | Where-Object { $_ })) {
        $txn = [string]$m.txn
        [void]$publishedTxns.Add($txn)
        $inDb  = $db.Contains($txn)
        $inDlq = $dlq.Contains($txn)

        $bucket = if ($inDb -and $inDlq) { 'Both' }
                  elseif ($inDb)         { 'Clean' }
                  elseif ($inDlq)        { 'DlqOnly' }
                  else                   { 'Neither' }

        $row = [pscustomobject]@{ Txn = $txn; Kind = [string]$m.kind; Bucket = $bucket }
        switch ($bucket) {
            'Both'    { $both.Add($row) }
            'Clean'   { $clean.Add($row) }
            'DlqOnly' { $dlqOnly.Add($row) }
            'Neither' { $neither.Add($row) }
        }

        # A message published as a deliberate failure is SUPPOSED to land in DlqOnly. Scoring it as
        # a defect would make -FailureCount unusable; scoring a success that dead-lettered as fine
        # would hide the real thing. So expectation is per-message, from the manifest.
        $expected = if ($row.Kind -eq 'failure') { 'DlqOnly' } else { 'Clean' }
        if ($bucket -ne $expected) {
            $wrongOutcome.Add([pscustomobject]@{ Txn = $txn; Kind = $row.Kind; Expected = $expected; Actual = $bucket })
        }
    }

    $unexpected = @(@($db) + @($dlq) | Select-Object -Unique | Where-Object { -not $publishedTxns.Contains($_) })

    [pscustomobject]@{
        Published    = $publishedTxns.Count
        Clean        = $clean.ToArray()
        DlqOnly      = $dlqOnly.ToArray()
        Both         = $both.ToArray()
        Neither      = $neither.ToArray()
        WrongOutcome = $wrongOutcome.ToArray()
        Unexpected   = $unexpected
    }
}

function Get-WwRunVerdict {
    <#
        PASS only when every published message reached its EXPECTED terminal state exactly once.

        Deliberately strict, and deliberately explicit about which criterion failed. A verdict that
        says 'FAIL' without naming the reason sends the reader back to the raw logs, which is the
        work this script exists to avoid.

        Note the zero-published guard. With nothing published every 'no failures' condition is
        vacuously true, and an unguarded verdict would report a clean PASS for a run that never
        happened - the single most misleading output this script could produce.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Buckets,
        [int] $DuplicateDbRows = 0,
        [int] $Redelivered = 0,
        [int] $UnknownOutcome = 0,
        [switch] $DatabaseSkipped
    )
    $reasons = [System.Collections.Generic.List[string]]::new()

    if ($Buckets.Published -eq 0) {
        return [pscustomobject]@{ Pass = $false; Reasons = @('Nothing was published - this is not a pass.') }
    }

    if ($Buckets.Neither.Count -gt 0) {
        $reasons.Add("$($Buckets.Neither.Count) message(s) LOST: no database row and no dead-letter. " +
                     'Published, never processed, no trace.')
    }
    if ($Buckets.Both.Count -gt 0) {
        $reasons.Add("$($Buckets.Both.Count) message(s) both committed AND dead-lettered - the workflow " +
                     'finished but the response was lost. Replaying these duplicates committed work.')
    }
    if ($Buckets.WrongOutcome.Count -gt 0) {
        $unexpectedDlq = @($Buckets.WrongOutcome | Where-Object { $_.Expected -eq 'Clean' -and $_.Actual -eq 'DlqOnly' }).Count
        if ($unexpectedDlq -gt 0) { $reasons.Add("$unexpectedDlq valid message(s) dead-lettered.") }
        $failedToFail = @($Buckets.WrongOutcome | Where-Object { $_.Expected -eq 'DlqOnly' -and $_.Actual -eq 'Clean' }).Count
        if ($failedToFail -gt 0) { $reasons.Add("$failedToFail deliberate-failure message(s) SUCCEEDED - the failure path did not trigger.") }
    }
    if ($DuplicateDbRows -gt 0) {
        $reasons.Add("$DuplicateDbRows duplicate database row(s): a message executed more than once.")
    }
    if ($Redelivered -gt 0) {
        $reasons.Add("$Redelivered message(s) were redelivered. Not fatal, but the first attempt failed.")
    }
    if ($UnknownOutcome -gt 0) {
        $reasons.Add("$UnknownOutcome delivery(ies) have no terminal outcome line in the worker logs.")
    }
    if ($Buckets.Unexpected.Count -gt 0) {
        $reasons.Add("$($Buckets.Unexpected.Count) transaction id(s) observed that this run never published - " +
                     'stale data above the watermark, or a competing publisher.')
    }
    if ($DatabaseSkipped) {
        $reasons.Add('Database reconciliation was SKIPPED, so "committed" is unproven. Not a pass on its own.')
    }

    [pscustomobject]@{ Pass = ($reasons.Count -eq 0); Reasons = $reasons.ToArray() }
}

function Get-WwPreflightBlockers {
    <#
        Turns the pre-flight observations into a list of reasons NOT to start. Anything returned
        here stops the run before a single message is published, because every one of these
        produces a result that looks like a product defect and is not.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)] $Preflight)

    $blockers = [System.Collections.Generic.List[string]]::new()

    if ($Preflight.Engine) {
        if ($Preflight.Engine.State -and $Preflight.Engine.State -ne 'Running') {
            $blockers.Add("Engine '$($Preflight.Engine.Name)' is '$($Preflight.Engine.State)', not Running. " +
                          'A stopped Function App returns 403 Site Disabled on every route, which reads as an auth failure.')
        }
    }
    else { $blockers.Add('Engine could not be resolved.') }

    if ($Preflight.Worker) {
        if (-not $Preflight.Worker.Exists) {
            $blockers.Add("Container App '$($Preflight.Worker.Name)' does not exist in the resource group.")
        }
        elseif ($Preflight.Worker.MaxReplicas -eq 0) {
            $blockers.Add("Container App '$($Preflight.Worker.Name)' is parked at maxReplicas 0 and will never consume.")
        }
    }
    else { $blockers.Add('Worker Container App could not be resolved.') }

    if ($Preflight.Keda) {
        if (-not $Preflight.Keda.HasRabbitRule) {
            $blockers.Add("No 'rabbitmq' KEDA scale rule on the worker - it will not scale off queue depth.")
        }
        elseif ($Preflight.Keda.QueueName -ne $Preflight.QueueName) {
            $blockers.Add("KEDA rule targets queue '$($Preflight.Keda.QueueName)' but the run publishes to " +
                          "'$($Preflight.QueueName)'. The worker will never wake up.")
        }
    }

    # The most damaging false pass there is: a second app on the same queue takes its share of the
    # messages, and if it forwards to a stopped or older engine it dead-letters AND acks them. The
    # queue still drains, the run still looks clean, and half the messages never executed.
    foreach ($c in @($Preflight.CompetingConsumers | Where-Object { $_ })) {
        $blockers.Add("Competing consumer '$($c.App)' is also bound to queue '$($c.Queue)' " +
                      "(min=$($c.MinReplicas) max=$($c.MaxReplicas)). Stop it or park it at maxReplicas 0.")
    }

    if ($Preflight.Broker -and -not $Preflight.Broker.QueueExists) {
        $blockers.Add("Queue '$($Preflight.QueueName)' does not exist on the broker. PublishRabbitMQActivity " +
                      'cannot create it - declare the exchange, queue and binding first.')
    }

    return , $blockers.ToArray()
}

function Invoke-WwSqlQuery {
    <#
        Runs a query and returns rows as objects.

        WHY A CHILD powershell.exe: Microsoft.Data.SqlClient needs its native SNI companion and will
        not load in a standalone PowerShell 7 process. Windows PowerShell 5.1 has System.Data.SqlClient
        built into the framework, so the query runs there.

        The connection string travels in an ENVIRONMENT VARIABLE on the child process, never as a
        command-line argument - arguments are visible in the process list to every user on the box.

        DateTimes are converted to strings in SQL, not here: Windows PowerShell 5.1's ConvertTo-Json
        renders a DateTime as "/Date(1234567890)/", which silently becomes an unusable string on the
        way back.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $ConnectionString,
        [Parameter(Mandatory)][string] $Query,
        [int] $TimeoutSec = 60
    )
    $child = @'
$ErrorActionPreference = 'Stop'
try {
    $c = New-Object System.Data.SqlClient.SqlConnection($env:WWLT_CS)
    $c.Open()
    try {
        $cmd = $c.CreateCommand()
        $cmd.CommandText    = $env:WWLT_QUERY
        $cmd.CommandTimeout = [int]$env:WWLT_TIMEOUT
        $dt = New-Object System.Data.DataTable
        (New-Object System.Data.SqlClient.SqlDataAdapter($cmd)).Fill($dt) | Out-Null
        $rows = @()
        foreach ($r in $dt.Rows) {
            $o = @{}
            foreach ($col in $dt.Columns) {
                $v = $r[$col]
                $o[$col.ColumnName] = if ($v -is [DBNull]) { $null } else { $v }
            }
            $rows += [pscustomobject]$o
        }
        @{ ok = $true; rows = $rows } | ConvertTo-Json -Depth 5 -Compress
    } finally { $c.Close() }
} catch {
    @{ ok = $false; error = $_.Exception.Message } | ConvertTo-Json -Compress
}
'@
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) "wwlt-sql-$([guid]::NewGuid().ToString('N')).ps1"
    Set-Content -LiteralPath $tmp -Value $child -Encoding UTF8
    try {
        $env:WWLT_CS = $ConnectionString; $env:WWLT_QUERY = $Query; $env:WWLT_TIMEOUT = "$TimeoutSec"
        $out = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $tmp 2>&1 | Out-String
    }
    finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
        Remove-Item Env:\WWLT_CS, Env:\WWLT_QUERY, Env:\WWLT_TIMEOUT -ErrorAction SilentlyContinue
    }

    $json = ($out -split "`n" | Where-Object { $_.Trim().StartsWith('{') } | Select-Object -Last 1)
    if (-not $json) { throw "SQL query produced no parseable result. Raw output: $($out.Trim())" }
    $result = $json | ConvertFrom-Json
    if (-not $result.ok) { throw "SQL query failed: $($result.error)" }
    return , @($result.rows | Where-Object { $null -ne $_ })
}

if ($LoadFunctionsOnly) { return }

# ═════════════════════════════════════════════════════════════════════════════
# Run
# ═════════════════════════════════════════════════════════════════════════════

Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force

function Write-Head { param([string] $T)
    Write-Host ''
    Write-Host ('=' * 78) -ForegroundColor Cyan
    Write-Host "  $T" -ForegroundColor Cyan
    Write-Host ('=' * 78) -ForegroundColor Cyan
}
function Write-Sub  { param([string] $T) Write-Host ''; Write-Host "-- $T" -ForegroundColor Yellow }
function Write-Ok   { param([string] $T) Write-Host "  [+] $T" -ForegroundColor Green }
function Write-Note { param([string] $T) Write-Host "  [-] $T" -ForegroundColor DarkYellow }
function Write-Bad  { param([string] $T) Write-Host "  [x] $T" -ForegroundColor Red }
function Write-Kv   { param([string] $K, $V, [string] $Colour = 'Gray')
    Write-Host ('    {0,-30} {1}' -f $K, $V) -ForegroundColor $Colour
}

$d = Get-WwLoadTestDefaults -Path $DefaultsPath
if ($NonInteractive) { $Yes = $true }

Write-Head 'Warewolf queue load test'
Write-Host '  Reproduces RUN 2 (2026-08-13): 100 messages, 6 replicas, 100/100, 0 dead-lettered.'
Write-Host '  Press Enter at any prompt to accept the RUN 2 default shown in brackets.'

# ── Phase 0: Azure login ─────────────────────────────────────────────────────
Write-Head 'Phase 0 - Azure login'
try { $az = Get-E2EAzContext }
catch { Write-Bad 'Not signed in to Azure. Run: az login'; throw }

Write-Kv 'Signed in as'   $az.User        'White'
Write-Kv 'Subscription'   "$($az.Subscription)  ($($az.SubscriptionId))" 'White'
Write-Kv 'Tenant'         $az.TenantId    'White'
Write-Ok  'Azure CLI authenticated.'
Write-Note 'Confirm the SUBSCRIPTION above is the intended one - the defaults target a shared resource group.'

# ── Phase 1: mode ────────────────────────────────────────────────────────────
Write-Head 'Phase 1 - Deployment mode'

if (-not $Mode) {
    if ($NonInteractive) { $Mode = 'Existing' }
    else {
        Write-Host '  1) Existing      - verify and run against what is already deployed  (recommended)'
        Write-Host '  2) DeployWorker  - deploy the QueueProcessor Container App first'
        Write-Host '  3) DeployAll     - deploy the Execution Engine AND the QueueProcessor first'
        Write-Host ''
        Write-Note 'Options 2 and 3 CREATE AND MODIFY Azure resources in a shared resource group.'
        $choice = Read-Host '  Choose [1]'
        $Mode = switch ($choice.Trim()) { '2' { 'DeployWorker' } '3' { 'DeployAll' } default { 'Existing' } }
    }
}
Write-Ok "Mode: $Mode"

# ── Phase 2: targets ─────────────────────────────────────────────────────────
Write-Head 'Phase 2 - Targets'
Write-Host '  Enter accepts the RUN 2 value.'
Write-Host ''

$ni = [bool]$NonInteractive
$cfg = [ordered]@{}
$cfg.ResourceGroup         = Read-WwSetting 'Resource group'            $d.ResourceGroup         $ResourceGroup         -NonInteractive:$ni
$cfg.EngineAppName         = Read-WwSetting 'Engine Function App'       $d.EngineAppName         $EngineAppName         -NonInteractive:$ni
$cfg.EngineAppId           = Read-WwSetting 'Engine Entra app id'       $d.EngineAppId           $EngineAppId           -NonInteractive:$ni
$cfg.EngineTenantId        = Read-WwSetting 'Engine tenant id'          $d.EngineTenantId        $EngineTenantId        -NonInteractive:$ni
$cfg.EngineAppInsightsName = Read-WwSetting 'Engine App Insights'       $d.EngineAppInsightsName $EngineAppInsightsName -NonInteractive:$ni
$cfg.WorkerAppName         = Read-WwSetting 'Worker Container App'      $d.WorkerAppName         $WorkerAppName         -NonInteractive:$ni
$cfg.QueueName             = Read-WwSetting 'Queue name'                $d.QueueName             $QueueName             -NonInteractive:$ni
$cfg.DeadLetterQueue       = Read-WwSetting 'Dead-letter queue'         $d.DeadLetterQueue       $DeadLetterQueue       -NonInteractive:$ni
$cfg.MaxReplicas           = Read-WwSetting 'Max replicas'              $d.MaxReplicas           $MaxReplicas           -NonInteractive:$ni -AsInt
$cfg.MaxConcurrency        = Read-WwSetting 'Worker MaxConcurrency'     $d.MaxConcurrency        $MaxConcurrency        -NonInteractive:$ni -AsInt
$cfg.MessageCount          = Read-WwSetting 'Messages to publish'       $d.MessageCount          $MessageCount          -NonInteractive:$ni -AsInt
$cfg.FailureCount          = Read-WwSetting 'Deliberate failures'       $d.FailureCount          $FailureCount          -NonInteractive:$ni -AsInt
$cfg.JobTable              = Read-WwSetting 'Job table'                 $d.JobTable              $JobTable              -NonInteractive:$ni
$cfg.WorkerPublishPath     = Read-WwSetting 'QueueProcessor publish'    $d.WorkerPublishPath     $WorkerPublishPath     -NonInteractive:$ni
$cfg.BrokerSourceBitePath  = Read-WwSetting 'RabbitMQ source .bite'     $d.BrokerSourceBitePath  $BrokerSourceBitePath  -NonInteractive:$ni

$concurrency = Resolve-WwEngineConcurrency -MaxReplicas $cfg.MaxReplicas -MaxConcurrency $cfg.MaxConcurrency

# Validate the local paths NOW, while the operator is still looking at the prompt they typed them
# into. These used to fail in Phase 3, after the Azure and engine checks had already run.
$pathErrors = @(
    Test-WwRequiredPath -Label 'QueueProcessor publish path' -Path $cfg.WorkerPublishPath `
                        -Kind Directory -MustContain 'RabbitMQ.Client.dll'
    $(if (-not $AmqpUri) { Test-WwRequiredPath -Label 'RabbitMQ source .bite' -Path $cfg.BrokerSourceBitePath -Kind File })
) | Where-Object { $_ }

if ($pathErrors.Count -gt 0) {
    Write-Host ''
    foreach ($e in $pathErrors) { Write-Bad $e }
    throw ("$($pathErrors.Count) path(s) could not be found. The defaults assume the G:\Deployment staging " +
           'layout; pass -WorkerPublishPath / -BrokerSourceBitePath (or -AmqpUri) for a different machine.')
}

if (-not $RunLabel) { $RunLabel = 'loadtest-' + (Get-Date).ToUniversalTime().ToString('yyyyMMdd-HHmmss') }
if (-not $OutputDir) {
    $logRoot = if (Test-Path -LiteralPath $d.LogRoot) { $d.LogRoot } else { Join-Path ([System.IO.Path]::GetTempPath()) 'ww-queue-runs' }
    $OutputDir = Join-Path $logRoot $RunLabel
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$manifestPath = Join-Path $OutputDir 'manifest.json'

# ── Database connection string ───────────────────────────────────────────────
if (-not $SkipDatabase) {
    if (-not $SqlConnectionString) { $SqlConnectionString = [Environment]::GetEnvironmentVariable($d.SqlConnectionEnvVar) }
    if (-not $SqlConnectionString -and -not $NonInteractive) {
        Write-Host ''
        Write-Note "No -SqlConnectionString and `$env:$($d.SqlConnectionEnvVar) is not set."
        Write-Host  '  Enter one to reconcile database rows, or press Enter to SKIP the database phases.'
        $secure = Read-Host '  SQL connection string' -AsSecureString
        $plain  = [System.Net.NetworkCredential]::new('', $secure).Password
        if ($plain) { $SqlConnectionString = $plain }
    }
    if (-not $SqlConnectionString) {
        $SkipDatabase = $true
        Write-Note 'Database phases SKIPPED. "Committed" will be unproven and the verdict cannot be a clean PASS.'
    }
}

# ── Optional deploy ──────────────────────────────────────────────────────────
if ($Mode -ne 'Existing') {
    Write-Head "Phase 2b - Deploy ($Mode)"
    Write-Note 'This creates and modifies Azure resources.'
    if (-not $Yes) {
        $go = Read-Host "  Type 'deploy' to continue"
        if ($go -ne 'deploy') { throw 'Deployment declined.' }
    }

    if ($Mode -eq 'DeployAll') {
        Write-Sub 'Execution Engine'
        Write-Note ('Engine deployment has many required targeting parameters (storage account, auth config, ' +
                    'licence, Key Vault). Run Deploy-WwExecutionEngine.ps1 directly - see docs/LoadTest-Guide.md ' +
                    'for the exact RUN 2 invocation - then re-run this script with -Mode DeployWorker.')
        throw 'Mode DeployAll requires the engine to be deployed separately. See docs/LoadTest-Guide.md.'
    }

    Write-Sub 'QueueProcessor Container App'
    Write-Note ("maxReplicas is derived from the trigger's Concurrency field, not from a flag. " +
                "To run at $($cfg.MaxReplicas) replicas, the trigger .bite must say Concurrency: $($cfg.MaxReplicas).")
    Write-Note 'Edit a COPY of the trigger - never the staged original. See docs/LoadTest-Guide.md, "Setting the ceiling".'
    throw ('Automated worker deployment is intentionally not performed here. Run Deploy-WwQueueProcessor.ps1 ' +
           'with the trigger copy, then re-run this script with -Mode Existing.')
}

# ── Phase 3: pre-flight ──────────────────────────────────────────────────────
Write-Head 'Phase 3 - Pre-flight'

$preflight = [ordered]@{ QueueName = $cfg.QueueName }

# Engine
Write-Sub 'Execution Engine'
$engineJson = Invoke-E2EAzJson -AzArgs @('functionapp', 'show', '-g', $cfg.ResourceGroup, '-n', $cfg.EngineAppName) -AllowFail
if (-not $engineJson) {
    $preflight.Engine = $null
    Write-Bad "Function App '$($cfg.EngineAppName)' not found in '$($cfg.ResourceGroup)'."
}
else {
    $settings = @(Invoke-E2EAzJson -AzArgs @('functionapp', 'config', 'appsettings', 'list',
                                             '-g', $cfg.ResourceGroup, '-n', $cfg.EngineAppName) -AllowFail)
    $logLevel = ($settings | Where-Object { $_.name -eq 'EXECUTIONLOGLEVEL' } | Select-Object -First 1).value
    $preflight.Engine = [pscustomobject]@{
        Name     = $cfg.EngineAppName
        Host     = $engineJson.defaultHostName
        State    = $engineJson.state
        Sku      = $(if ($engineJson.PSObject.Properties['appServicePlanId'] -and $engineJson.appServicePlanId) {
                        ($engineJson.appServicePlanId -replace '.*/', '') } else { '(unknown)' })
        LogLevel = $logLevel
        AppId    = $cfg.EngineAppId
    }
    Write-Kv 'Function App'  $preflight.Engine.Name
    Write-Kv 'URL'           "https://$($preflight.Engine.Host)"
    Write-Kv 'Workflow route' "https://$($preflight.Engine.Host)/$($d.WorkflowRoute)"
    Write-Kv 'State'         $preflight.Engine.State $(if ($preflight.Engine.State -eq 'Running') { 'Green' } else { 'Red' })
    Write-Kv 'Plan'          $preflight.Engine.Sku
    Write-Kv 'Entra app id'  $preflight.Engine.AppId
    Write-Kv 'EXECUTIONLOGLEVEL' ($logLevel ?? '(not set)') $(if ($logLevel -eq $d.RequiredEngineLogLevel) { 'Green' } else { 'DarkYellow' })
    if ($logLevel -ne $d.RequiredEngineLogLevel) {
        Write-Note ("Engine-side evidence needs EXECUTIONLOGLEVEL=$($d.RequiredEngineLogLevel). " +
                    'AuditExecutionLogger writes only ERROR/FATAL, so successful executions log NOTHING at ERROR ' +
                    'and the report will correctly show zero engine lines for healthy messages.')
    }
}

# Worker + KEDA
Write-Sub 'Container App and KEDA scale rule'
$apps = @(Get-E2EContainerApps -ResourceGroup $cfg.ResourceGroup | Where-Object { $_.Name -eq $cfg.WorkerAppName })
if ($apps.Count -eq 0) {
    $preflight.Worker = [pscustomobject]@{ Name = $cfg.WorkerAppName; Exists = $false }
    $preflight.Keda   = $null
    Write-Bad "Container App '$($cfg.WorkerAppName)' not found."
}
else {
    $app = $apps[0]
    $raw = Invoke-E2EAzJson -AzArgs @('containerapp', 'show', '-g', $cfg.ResourceGroup, '-n', $cfg.WorkerAppName) -AllowFail
    $tmpl = $raw.properties.template
    $envVars = @()
    if ($tmpl.containers -and $tmpl.containers.Count -gt 0) { $envVars = @($tmpl.containers[0].env) }
    function Get-Env { param([string] $N) (($envVars | Where-Object { $_.name -eq $N }) | Select-Object -First 1).value }

    $preflight.Worker = [pscustomobject]@{
        Name        = $app.Name
        Exists      = $true
        Image       = $app.Image
        Revision    = $app.Revision
        MinReplicas = $app.MinReplicas
        MaxReplicas = $app.MaxReplicas
        GracePeriod = $tmpl.terminationGracePeriodSeconds
        Timeout     = Get-Env 'ENGINE__TIMEOUTSECONDS'
        Shutdown    = Get-Env 'WORKER__SHUTDOWNGRACESECONDS'
        Concurrency = Get-Env 'WORKER__MAXCONCURRENCY'
        Delivery    = Get-Env 'WORKER__MAXDELIVERYATTEMPTS'
        RetryFives  = Get-Env 'WORKER__RETRYENGINEINTERNALERRORS'
        EngineUrl   = Get-Env 'ENGINE__BASEURL'
    }
    Write-Kv 'Container App'    $preflight.Worker.Name
    Write-Kv 'Image'            $preflight.Worker.Image
    Write-Kv 'Active revision'  $preflight.Worker.Revision
    Write-Kv 'Replicas min/max' "$($preflight.Worker.MinReplicas) / $($preflight.Worker.MaxReplicas)"
    Write-Kv 'Engine base URL'  ($preflight.Worker.EngineUrl ?? '(not set)')
    Write-Kv 'ENGINE__TIMEOUTSECONDS'          ($preflight.Worker.Timeout     ?? '(not set)')
    Write-Kv 'WORKER__SHUTDOWNGRACESECONDS'    ($preflight.Worker.Shutdown    ?? '(not set)')
    Write-Kv 'terminationGracePeriodSeconds'   ($preflight.Worker.GracePeriod ?? '(not set)')
    Write-Kv 'WORKER__MAXCONCURRENCY'          ($preflight.Worker.Concurrency ?? '(not set)')
    Write-Kv 'WORKER__MAXDELIVERYATTEMPTS'     ($preflight.Worker.Delivery    ?? '(not set)')
    Write-Kv 'WORKER__RETRYENGINEINTERNALERRORS' ($preflight.Worker.RetryFives ?? '(not set)')

    # The ordering constraint. KEDA counts only READY messages, so in-flight work is invisible to
    # the scaler and a replica can be scaled away underneath a running message. The drain path is
    # the only protection, and it only works if these three are ordered.
    $t = [int]($preflight.Worker.Timeout ?? 0); $s = [int]($preflight.Worker.Shutdown ?? 0); $g = [int]($preflight.Worker.GracePeriod ?? 0)
    if ($t -gt 0 -and $s -gt 0 -and $g -gt 0) {
        if ($t -le $s -and $s -lt $g) { Write-Ok "Timeout chain ordered correctly: $t <= $s < $g" }
        else { Write-Bad "Timeout chain WRONG: EngineTimeout=$t ShutdownGrace=$s TerminationGrace=$g. Required: timeout <= grace < termination." }
    }

    $rabbitRule = $null
    foreach ($rule in @($app.Rules | Where-Object { $_ })) {
        if ($rule.PSObject.Properties['custom'] -and $rule.custom.type -eq 'rabbitmq') { $rabbitRule = $rule; break }
    }
    $preflight.Keda = [pscustomobject]@{
        HasRabbitRule = ($null -ne $rabbitRule)
        RuleName      = $(if ($rabbitRule) { $rabbitRule.name } else { $null })
        QueueName     = $(if ($rabbitRule) { $rabbitRule.custom.metadata.queueName } else { $null })
        Value         = $(if ($rabbitRule) { $rabbitRule.custom.metadata.value } else { $null })
        Mode          = $(if ($rabbitRule -and $rabbitRule.custom.metadata.PSObject.Properties['mode']) { $rabbitRule.custom.metadata.mode } else { $null })
    }
    Write-Kv 'KEDA rule'        ($preflight.Keda.RuleName  ?? '(none)') $(if ($preflight.Keda.HasRabbitRule) { 'Gray' } else { 'Red' })
    Write-Kv 'KEDA queueName'   ($preflight.Keda.QueueName ?? '(none)')
    Write-Kv 'KEDA value'       ($preflight.Keda.Value     ?? '(none)')
    Write-Kv 'Scaling formula'  "replicas = ceil(queueLength / $($preflight.Keda.Value)) capped at $($preflight.Worker.MaxReplicas)"
}

# Competing consumers
$preflight.CompetingConsumers = @(Get-E2EQueueConsumers -ResourceGroup $cfg.ResourceGroup `
                                     -QueueName @($cfg.QueueName) -ExcludeNamePrefix $cfg.WorkerAppName)

# Broker
Write-Sub 'Queue server'
if (-not $AmqpUri) {
    $AmqpUri = Resolve-E2EBrokerUri -SourceBitePath $cfg.BrokerSourceBitePath
    if (-not $AmqpUri) {
        # Diagnose rather than guess. Resolve-E2EBrokerUri returns $null for six different reasons
        # and an assumed one is worse than none - it sends the reader after the wrong problem.
        $diag = Get-WwBrokerSourceDiagnosis -Path $cfg.BrokerSourceBitePath
        Write-Bad "Broker source unusable [$($diag.Reason)]: $($diag.Detail)"
        if ($diag.Hint) { Write-Note $diag.Hint }
        throw ("Could not resolve the broker URI from '$($cfg.BrokerSourceBitePath)' [$($diag.Reason)]. " +
               'Fix the source, or pass -AmqpUri explicitly.')
    }
}
Initialize-E2ERabbitClient -PublishPath $cfg.WorkerPublishPath
$session = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName 'wwqp-loadtest-preflight'
try {
    $qDepth   = Get-E2EQueueDepth -Session $session -QueueName $cfg.QueueName
    $dlqDepth = Get-E2EQueueDepth -Session $session -QueueName $cfg.DeadLetterQueue
    $preflight.Broker = [pscustomobject]@{
        Uri         = $session.Describe          # never contains the password
        VirtualHost = $session.VirtualHost
        QueueExists = $qDepth.Exists
        QueueDepth  = $qDepth.Messages
        Consumers   = $qDepth.Consumers
        DlqExists   = $dlqDepth.Exists
        DlqDepth    = $dlqDepth.Messages
    }
}
finally { Close-E2EBrokerSession -Session $session }

Write-Kv 'Broker'          $preflight.Broker.Uri
Write-Kv 'Virtual host'    $preflight.Broker.VirtualHost
Write-Kv 'Queue'           "$($cfg.QueueName)  (exists=$($preflight.Broker.QueueExists), ready=$($preflight.Broker.QueueDepth), consumers=$($preflight.Broker.Consumers))"
Write-Kv 'Dead-letter'     "$($cfg.DeadLetterQueue)  (exists=$($preflight.Broker.DlqExists), ready=$($preflight.Broker.DlqDepth))"

# Database
Write-Sub 'Database'
$dbInfo = $null
if ($SkipDatabase) { Write-Note 'SKIPPED - no connection string supplied.' }
else {
    $dbInfo = Get-WwMaskedConnectionString -ConnectionString $SqlConnectionString
    Write-Kv 'Server'        ($dbInfo.Server   ?? '(unparsed)')
    Write-Kv 'Database'      ($dbInfo.Database ?? '(unparsed)')
    Write-Kv 'Auth'          $dbInfo.Auth
    Write-Kv 'Job table'     $cfg.JobTable
    try {
        $probe = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString -Query 'SELECT 1 AS ok;' -TimeoutSec 20
        if (@($probe).Count -gt 0) { Write-Ok 'Reachable (SELECT 1 succeeded).' } else { Write-Bad 'SELECT 1 returned no rows.' }
    }
    catch {
        Write-Bad "Database unreachable: $($_.Exception.Message)"
        $SkipDatabase = $true
        Write-Note 'Continuing with database phases SKIPPED.'
    }
}

# ── Blockers + gate ──────────────────────────────────────────────────────────
Write-Sub 'Planned run'
Write-Kv 'Messages'              "$($cfg.MessageCount) valid + $($cfg.FailureCount) deliberate failure(s)"
Write-Kv 'Max replicas'          $cfg.MaxReplicas
Write-Kv 'Worker MaxConcurrency' $cfg.MaxConcurrency
Write-Kv 'Concurrent engine requests' "$concurrency   (= $($cfg.MaxReplicas) x $($cfg.MaxConcurrency))" 'White'
Write-Kv 'Purge queues first'    $(if ($PurgeQueues) { 'YES' } else { 'no (watermark isolates the run)' })
Write-Kv 'Pre-warm'              $(if ($SkipPreWarm) { 'SKIPPED' } else { "yes, at concurrency $concurrency" })
Write-Kv 'Artefacts'             $OutputDir

if ($concurrency -gt 8) {
    Write-Note ("Concurrency $concurrency exceeds the 8 that was measured clean against a Consumption-plan " +
                'engine. At 10 the engine ran out of memory and lost 4 of 30 requests.')
}

$blockers = Get-WwPreflightBlockers -Preflight ([pscustomobject]$preflight)
if ($blockers.Count -gt 0) {
    Write-Sub 'BLOCKERS'
    foreach ($b in $blockers) { Write-Bad $b }
    throw "Pre-flight found $($blockers.Count) blocker(s). Resolve them before running - each one produces a result that looks like a product defect and is not."
}
Write-Ok 'Pre-flight clean.'

if (-not $Yes) {
    Write-Host ''
    $go = Read-Host "  Publish $($cfg.MessageCount + $cfg.FailureCount) message(s) to '$($cfg.QueueName)'? [y/N]"
    if ($go -notmatch '^(y|yes)$') { throw 'Run declined.' }
}

# ── Phase 4: baseline ────────────────────────────────────────────────────────
Write-Head 'Phase 4 - Baseline'

if ($PurgeQueues) {
    $session = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName 'wwqp-loadtest-purge'
    try {
        foreach ($q in @($cfg.QueueName, $cfg.DeadLetterQueue)) {
            $depth = Get-E2EQueueDepth -Session $session -QueueName $q
            if (-not $depth.Exists) { Write-Note "Queue '$q' does not exist - nothing to purge."; continue }
            $ch = New-E2EChannel -Connection $session.Connection -CancellationToken $session.Ct
            $n  = $ch.QueuePurgeAsync($q, $session.Ct).GetAwaiter().GetResult()
            Write-Ok "Purged '$q': $n message(s)."
        }
    }
    finally { Close-E2EBrokerSession -Session $session }
}
else { Write-Note 'Queues NOT purged. The watermark below isolates this run instead.' }

# Two readings are taken. This one is the BASELINE, before pre-warming; the run watermark is taken
# again after Phase 5. The difference between them is exactly the number of rows the warm-up wrote,
# which is reported rather than silently absorbed.
$baselineWatermark = 0
$watermark         = 0
if (-not $SkipDatabase) {
    $row = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString `
             -Query "SELECT ISNULL(MAX($($d.JobIdColumn)), 0) AS wm FROM $($cfg.JobTable);"
    $baselineWatermark = [int]@($row)[0].wm
    $watermark         = $baselineWatermark
    Write-Ok "Baseline watermark (before pre-warm): $($d.JobIdColumn) = $baselineWatermark"
}

# ── Phase 5: pre-warm ────────────────────────────────────────────────────────
Write-Head 'Phase 5 - Pre-warm'

if ($SkipPreWarm) {
    Write-Note ('SKIPPED. On a Consumption plan the first request after idle took 64,757 ms against ~3,100 ms ' +
                'warm, and every 502/503/504 in RUN 1 came from that window.')
}
else {
    $preWarm = Join-Path $ScriptDir 'Invoke-WwEnginePreWarm.ps1'
    if (-not (Test-Path -LiteralPath $preWarm)) { throw "Pre-warm script not found: $preWarm" }
    & $preWarm -EngineAppName $cfg.EngineAppName -EngineAppId $cfg.EngineAppId `
               -ResourceGroup $cfg.ResourceGroup -TenantId $cfg.EngineTenantId `
               -WorkflowRoute $d.WorkflowRoute -TargetConcurrency $concurrency
}

# The pre-warm finished; nothing it produced belongs to the run.
#
# TWO separate contaminations have to be excluded, and they need different mechanisms:
#
#   DATABASE  - the warm-up executes the REAL workflow and commits REAL rows. The run watermark is
#               re-read HERE, after warming, so every later query counts only rows above it. The
#               warm-up rows are left in place (nothing is ever deleted) and simply fall below it.
#
#   ENGINE    - the App Insights `requests` query is TIME-windowed, not watermarked, so a window
#               that reaches back before this moment would fold the warm-up's own HTTP requests
#               into the run's result-code distribution and latency percentiles. $preWarmEndUtc
#               clamps the window in Phase 6.
#
# The worker logs need neither: the pre-warm calls the engine directly over HTTP and never touches
# the queue, so it produces no worker log lines at all.
$preWarmEndUtc = (Get-Date).ToUniversalTime()

if (-not $SkipDatabase) {
    $row = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString `
             -Query "SELECT ISNULL(MAX($($d.JobIdColumn)), 0) AS wm FROM $($cfg.JobTable);"
    $watermark = [int]@($row)[0].wm

    $warmRows = 0
    if ($watermark -gt $baselineWatermark) {
        $r = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString `
               -Query "SELECT COUNT(*) AS n FROM $($cfg.JobTable) WHERE $($d.JobIdColumn) > $baselineWatermark AND $($d.JobIdColumn) <= $watermark;"
        $warmRows = [int]@($r)[0].n
    }
    Write-Ok "Run watermark: $($d.JobIdColumn) > $watermark"
    if ($warmRows -gt 0) {
        Write-Note ("Pre-warm wrote $warmRows row(s) ($($d.JobIdColumn) $($baselineWatermark + 1)-$watermark). " +
                    'They are EXCLUDED from every count below and are not deleted.')
    }
}

# ── Phase 6: publish ─────────────────────────────────────────────────────────
Write-Head 'Phase 6 - Publish'

# A minute of slack absorbs clock skew between this machine and the log timestamps - but CLAMPED so
# it can never reach back past the pre-warm. Without the clamp, a warm engine finishes Phase 5 in
# under a minute and the slack silently pulls the warm-up's own HTTP requests into the run's
# result-code distribution and latency figures.
$startUtc = (Get-Date).ToUniversalTime().AddMinutes(-1)
if ($preWarmEndUtc -and $startUtc -lt $preWarmEndUtc) {
    $startUtc = $preWarmEndUtc
    Write-Note 'Report window clamped to the end of the pre-warm, so warm-up requests are excluded.'
}
$publish  = Join-Path $ScriptDir 'Publish-WwQueueBurst.ps1'
$burst = & $publish -QueueName $cfg.QueueName -SuccessCount $cfg.MessageCount -FailureCount $cfg.FailureCount `
                    -Label ($RunLabel -replace '[^A-Za-z0-9]', '') -AmqpUri $AmqpUri `
                    -PublishPath $cfg.WorkerPublishPath -ManifestPath $manifestPath

Write-Ok "Published $($burst.Total) message(s) to '$($burst.Queue)'."
Write-Kv 'Manifest' $manifestPath

$manifest = @(Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json)
$distinctTxn  = @($manifest | ForEach-Object txn  | Select-Object -Unique).Count
$distinctBody = @($manifest | Where-Object { $_.kind -eq 'success' } | ForEach-Object body | Select-Object -Unique).Count
Write-Kv 'Distinct transaction ids' "$distinctTxn / $($manifest.Count)"
Write-Kv 'Distinct bodies'          "$distinctBody / $($cfg.MessageCount)"

# ── Phase 7: drain ───────────────────────────────────────────────────────────
Write-Head 'Phase 7 - Drain'
Write-Note 'An empty queue is NOT the finish line: 2xx->ack and non-2xx->dead-letter+ack both drain it.'
Write-Note 'Waiting until the DATABASE ROW COUNT stops rising instead.'

$expectedTotal = $burst.Total
$deadline   = (Get-Date).AddSeconds([int]$d.DrainTimeoutSeconds)
$quiet      = [int]$d.DrainQuietSeconds
$poll       = [int]$d.DrainPollSeconds
$lastRows   = -1
$lastChange = Get-Date
$peak       = 0
$samples    = [System.Collections.Generic.List[object]]::new()

while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds $poll

    $session = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName 'wwqp-loadtest-watch'
    try {
        $qd   = Get-E2EQueueDepth -Session $session -QueueName $cfg.QueueName
        $dlqd = Get-E2EQueueDepth -Session $session -QueueName $cfg.DeadLetterQueue
    }
    finally { Close-E2EBrokerSession -Session $session }

    $replicas = Get-E2EReplicaCount -AppName $cfg.WorkerAppName -ResourceGroup $cfg.ResourceGroup
    if ($replicas -gt $peak) { $peak = $replicas }

    $rows = $null
    if (-not $SkipDatabase) {
        $r = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString `
               -Query "SELECT COUNT(*) AS n FROM $($cfg.JobTable) WHERE $($d.JobIdColumn) > $watermark;"
        $rows = [int]@($r)[0].n
    }

    $samples.Add([pscustomobject]@{
        AtUtc = (Get-Date).ToUniversalTime().ToString('o')
        Queue = $qd.Messages; Dlq = $dlqd.Messages; Replicas = $replicas; Rows = $rows
    })
    Write-Host ('    {0}  queue={1,-5} dlq={2,-4} replicas={3,-3} rows={4}' -f `
        (Get-Date).ToString('HH:mm:ss'), $qd.Messages, $dlqd.Messages, $replicas, ($rows ?? '-'))

    if ($SkipDatabase) {
        # Without the database the queue is all there is. Weaker, and labelled as such.
        if ($qd.Messages -eq 0 -and $replicas -eq 0) { Write-Note 'Queue empty and replicas at zero (database not consulted).'; break }
        continue
    }

    if ($rows -ne $lastRows) { $lastRows = $rows; $lastChange = Get-Date }
    elseif (((Get-Date) - $lastChange).TotalSeconds -ge $quiet -and $qd.Messages -eq 0) {
        Write-Ok "Row count stable at $rows for ${quiet}s and the queue is empty."
        break
    }
    if ($rows -ge $expectedTotal -and $qd.Messages -eq 0 -and $dlqd.Messages -eq 0) {
        Write-Ok "All $expectedTotal message(s) accounted for."
        break
    }
}
$endUtc = (Get-Date).ToUniversalTime().AddMinutes(1)
Write-Kv 'Peak replicas' $peak
$samples | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDir 'drain-samples.json') -Encoding utf8

# ── Phase 8: report ──────────────────────────────────────────────────────────
Write-Head 'Phase 8 - Report'

Write-Note 'Log Analytics ingestion lags 2-5 minutes. Waiting before querying.'
Start-Sleep -Seconds 120

$report = Join-Path $ScriptDir 'Get-WwQueueRunReport.ps1'
& $report -ResourceGroup $cfg.ResourceGroup -AppName @($cfg.WorkerAppName) `
          -AcaEnvironment $d.AcaEnvironment `
          -StartUtc $startUtc -EndUtc $endUtc `
          -IncludeEngine -EngineAppInsightsName $cfg.EngineAppInsightsName `
          -ExpectedManifest $manifestPath -OutputDir $OutputDir -RunLabel $RunLabel

$reportJson = Get-ChildItem -LiteralPath $OutputDir -Filter 'queue-run-report-*.json' |
                Sort-Object LastWriteTime | Select-Object -Last 1
$rpt = if ($reportJson) { Get-Content -LiteralPath $reportJson.FullName -Raw | ConvertFrom-Json } else { $null }

# ── Database side ────────────────────────────────────────────────────────────
$dbTxns = @(); $dbStats = $null; $dupRows = 0
if (-not $SkipDatabase) {
    Write-Sub 'Database rows above the watermark'

    $rows = Invoke-WwSqlQuery -ConnectionString $SqlConnectionString -Query @"
SELECT JSON_VALUE(MessageContent, '$.txn') AS Txn,
       Status,
       TRY_CONVERT(int, AttemptNumber) AS AttemptNumber,
       CONVERT(varchar(33), TRY_CONVERT(datetime2(3), StartedAtUtc,   127), 126) AS StartedAtUtc,
       CONVERT(varchar(33), TRY_CONVERT(datetime2(3), ProcessingAtUtc,127), 126) AS ProcessingAtUtc,
       CONVERT(varchar(33), TRY_CONVERT(datetime2(3), FinishedAtUtc,  127), 126) AS FinishedAtUtc
FROM $($cfg.JobTable)
WHERE $($d.JobIdColumn) > $watermark;
"@
    $dbTxns  = @($rows | ForEach-Object { $_.Txn } | Where-Object { $_ })
    $dupRows = @($dbTxns | Group-Object | Where-Object { $_.Count -gt 1 }).Count

    $startToProc = @(); $procToFin = @(); $total = @()
    foreach ($r in $rows) {
        $s = ConvertTo-WwDateTimeOrNull $r.StartedAtUtc
        $p = ConvertTo-WwDateTimeOrNull $r.ProcessingAtUtc
        $f = ConvertTo-WwDateTimeOrNull $r.FinishedAtUtc
        if ($s -and $p) { $startToProc += ($p - $s).TotalMilliseconds }
        if ($p -and $f) { $procToFin   += ($f - $p).TotalMilliseconds }
        if ($s -and $f) { $total       += ($f - $s).TotalMilliseconds }
    }

    $dbStats = [pscustomobject]@{
        Rows              = @($rows).Count
        DistinctTxns      = @($dbTxns | Select-Object -Unique).Count
        DuplicateTxns     = $dupRows
        ByStatus          = @($rows | Group-Object Status | ForEach-Object { [pscustomobject]@{ Status = $_.Name; Count = $_.Count } })
        Reprocessed       = @($rows | Where-Object { $_.AttemptNumber -gt 1 }).Count
        StartToProcAvgMs  = $(if ($startToProc.Count) { [int](($startToProc | Measure-Object -Average).Average) } else { $null })
        ProcToFinishAvgMs = $(if ($procToFin.Count)   { [int](($procToFin   | Measure-Object -Average).Average) } else { $null })
        TotalAvgMs        = $(if ($total.Count)       { [int](($total       | Measure-Object -Average).Average) } else { $null })
        TotalP50Ms        = $(if ($total.Count)       { [int](Get-WwPercentile -Values $total -Percentile 50) } else { $null })
        TotalP95Ms        = $(if ($total.Count)       { [int](Get-WwPercentile -Values $total -Percentile 95) } else { $null })
        TotalMaxMs        = $(if ($total.Count)       { [int](($total | Measure-Object -Maximum).Maximum) } else { $null })
    }

    Write-Kv 'Rows above watermark' $dbStats.Rows
    Write-Kv 'Distinct txns'        $dbStats.DistinctTxns
    Write-Kv 'Duplicate txns'       $dbStats.DuplicateTxns $(if ($dupRows -eq 0) { 'Green' } else { 'Red' })
    Write-Kv 'AttemptNumber > 1'    $dbStats.Reprocessed
    $dbStats.ByStatus | Format-Table -AutoSize | Out-String -Width 80 | Write-Host
    Write-Kv 'Start -> Processing avg' "$($dbStats.StartToProcAvgMs) ms"
    Write-Kv 'Processing -> Finished avg' "$($dbStats.ProcToFinishAvgMs) ms"
    Write-Kv 'Total  avg / p50 / p95 / max' "$($dbStats.TotalAvgMs) / $($dbStats.TotalP50Ms) / $($dbStats.TotalP95Ms) / $($dbStats.TotalMaxMs) ms"
}

# ── Dead-letter contents ─────────────────────────────────────────────────────
Write-Sub 'Dead-letter queue'
$dlqTxns = @()
$session = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName 'wwqp-loadtest-dlq'
try {
    $ch  = New-E2EChannel -Connection $session.Connection -CancellationToken $session.Ct
    $held = [System.Collections.Generic.List[uint64]]::new()
    # Messages are held UNACKED and released by closing the connection, rather than nacked inside
    # the loop. A nack-requeue mid-loop puts the message straight back at the head and the next
    # BasicGet reads it again, so the same few messages get counted over and over - an earlier
    # version reported 60 "distinct" dead-letters that were really the same handful.
    while ($true) {
        $got = $ch.BasicGetAsync($cfg.DeadLetterQueue, $false, $session.Ct).GetAwaiter().GetResult()
        if (-not $got) { break }
        $held.Add($got.DeliveryTag)
        $txn = $got.BasicProperties.CorrelationId
        if ($txn) { $dlqTxns += [string]$txn }
        if ($held.Count -ge ($expectedTotal * 2 + 50)) { Write-Note 'Dead-letter read cap reached.'; break }
    }
}
catch { Write-Note "Could not read the dead-letter queue: $($_.Exception.Message)" }
finally { Close-E2EBrokerSession -Session $session }   # closing requeues everything held unacked

$dlqTxns = @($dlqTxns | Select-Object -Unique)
Write-Kv 'Dead-lettered (distinct txns)' $dlqTxns.Count $(if ($dlqTxns.Count -eq $cfg.FailureCount) { 'Green' } else { 'Red' })

# ── Phase 9: reconciliation and verdict ──────────────────────────────────────
Write-Head 'Phase 9 - Reconciliation'

$buckets = Get-WwReconciliationBuckets -Manifest $manifest -DbTxns $dbTxns -DlqTxns $dlqTxns
$sum = $buckets.Clean.Count + $buckets.DlqOnly.Count + $buckets.Both.Count + $buckets.Neither.Count

Write-Kv 'Published'                          $buckets.Published 'White'
Write-Kv 'Clean    (row, no dead-letter)'     $buckets.Clean.Count   'Green'
Write-Kv 'DlqOnly  (dead-letter, no row)'     $buckets.DlqOnly.Count $(if ($buckets.DlqOnly.Count -eq $cfg.FailureCount) { 'Gray' } else { 'Red' })
Write-Kv 'Both     (row AND dead-letter)'     $buckets.Both.Count    $(if ($buckets.Both.Count -eq 0) { 'Gray' } else { 'Red' })
Write-Kv 'Neither  (LOST)'                    $buckets.Neither.Count $(if ($buckets.Neither.Count -eq 0) { 'Gray' } else { 'Red' })
Write-Kv 'Buckets sum to published'           "$sum / $($buckets.Published)" $(if ($sum -eq $buckets.Published) { 'Green' } else { 'Red' })
Write-Kv 'Unexpected txns'                    $buckets.Unexpected.Count

if ($buckets.Neither.Count -gt 0) {
    Write-Bad ('LOST: ' + ((@($buckets.Neither | ForEach-Object Txn) | Select-Object -First 10) -join ', '))
}
if ($buckets.WrongOutcome.Count -gt 0) {
    Write-Sub 'Outcome mismatches'
    $buckets.WrongOutcome | Select-Object -First 25 | Format-Table -AutoSize | Out-String -Width 140 | Write-Host
}

$verdict = Get-WwRunVerdict -Buckets $buckets `
             -DuplicateDbRows $dupRows `
             -Redelivered   $(if ($rpt) { [int]$rpt.totals.redelivered } else { 0 }) `
             -UnknownOutcome $(if ($rpt) { [int]$rpt.totals.unknown }     else { 0 }) `
             -DatabaseSkipped:$SkipDatabase

Write-Head "VERDICT: $(if ($verdict.Pass) { 'PASS' } else { 'FAIL' })"
if ($verdict.Pass) {
    Write-Ok "All $($buckets.Published) published message(s) reached their expected terminal state exactly once."
}
else {
    foreach ($r in $verdict.Reasons) { Write-Bad $r }
}

# ── Artefacts ────────────────────────────────────────────────────────────────
$summary = [pscustomobject]@{
    runLabel      = $RunLabel
    generatedUtc  = (Get-Date).ToUniversalTime().ToString('o')
    mode          = $Mode
    verdict       = $(if ($verdict.Pass) { 'PASS' } else { 'FAIL' })
    reasons       = $verdict.Reasons
    azure         = [pscustomobject]@{ subscription = $az.Subscription; subscriptionId = $az.SubscriptionId; tenant = $az.TenantId; user = $az.User }
    targets       = [pscustomobject]$cfg
    concurrency   = $concurrency
    preflight     = [pscustomobject]$preflight
    database      = [pscustomobject]@{
        skipped           = [bool]$SkipDatabase
        server            = $dbInfo.Server
        database          = $dbInfo.Database
        baselineWatermark = $baselineWatermark
        watermark         = $watermark
        preWarmRows       = $(if ($warmRows) { $warmRows } else { 0 })   # written by the warm-up, excluded from every count
        stats             = $dbStats
    }
    reportWindow  = [pscustomobject]@{ startUtc = $startUtc.ToString('o'); endUtc = $endUtc.ToString('o'); preWarmEndUtc = $preWarmEndUtc.ToString('o') }
    publish       = $burst
    peakReplicas  = $peak
    buckets       = [pscustomobject]@{
        published = $buckets.Published
        clean = $buckets.Clean.Count; dlqOnly = $buckets.DlqOnly.Count
        both = $buckets.Both.Count;   neither = $buckets.Neither.Count
        wrongOutcome = $buckets.WrongOutcome; neitherTxns = @($buckets.Neither | ForEach-Object Txn)
        unexpected = $buckets.Unexpected
    }
    workerReport  = $(if ($reportJson) { $reportJson.FullName } else { $null })
}
$summaryPath = Join-Path $OutputDir 'loadtest-summary.json'
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $summaryPath -Encoding utf8

Write-Head 'Artefacts'
Write-Ok "Summary  : $summaryPath"
Write-Ok "Manifest : $manifestPath"
Write-Ok "Drain    : $(Join-Path $OutputDir 'drain-samples.json')"
if ($reportJson) { Write-Ok "Report   : $($reportJson.FullName)" }

if (-not $verdict.Pass) { exit 1 }
