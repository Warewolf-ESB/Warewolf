#Requires -Version 7.0

<#
    Pester 5 test suite for Invoke-WwQueueLoadTest.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Invoke-WwQueueLoadTest.Tests.ps1

    Design notes (same conventions as Deploy-WwQueueProcessor.Tests.ps1)
    ---------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the decision logic with no Azure CLI, no broker and no database,
      so the parts that decide whether a run is a PASS are testable in isolation.
    * The pre-flight and verdict tests are the point of this suite. Both encode failures that were
      observed for real and that LOOK LIKE PASSES: a competing consumer draining half the queue, and
      a run that published nothing at all reporting no failures.
#>

BeforeAll {
    $script:LoadTestScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Invoke-WwQueueLoadTest.ps1'
    $script:DefaultsPath   = Join-Path (Split-Path $PSScriptRoot -Parent) 'WwLoadTest.Defaults.psd1'
    . $script:LoadTestScript -LoadFunctionsOnly

    function New-Manifest {
        param([int] $Success = 0, [int] $Failure = 0, [string] $Prefix = 'T')
        $m = @()
        for ($i = 1; $i -le $Success; $i++) { $m += [pscustomobject]@{ txn = "$Prefix-S-$i"; kind = 'success' } }
        for ($i = 1; $i -le $Failure; $i++) { $m += [pscustomobject]@{ txn = "$Prefix-F-$i"; kind = 'failure' } }
        , $m
    }

    function New-Preflight {
        param(
            [string] $EngineState = 'Running',
            [bool]   $WorkerExists = $true,
            [int]    $MaxReplicas = 6,
            [bool]   $HasRabbitRule = $true,
            [string] $KedaQueue = 'order-success-queue',
            [string] $QueueName = 'order-success-queue',
            [bool]   $QueueExists = $true,
            [object[]] $Competing = @()
        )
        [pscustomobject]@{
            QueueName = $QueueName
            Engine    = [pscustomobject]@{ Name = 'wwengine-e2e-ldi413'; State = $EngineState }
            Worker    = [pscustomobject]@{ Name = 'wwqp4-ordersuccessqueue'; Exists = $WorkerExists; MaxReplicas = $MaxReplicas }
            Keda      = [pscustomobject]@{ HasRabbitRule = $HasRabbitRule; QueueName = $KedaQueue }
            Broker    = [pscustomobject]@{ QueueExists = $QueueExists }
            CompetingConsumers = $Competing
        }
    }
}

Describe 'Get-WwLoadTestDefaults' {

    It 'loads the RUN 2 defaults file' {
        $d = Get-WwLoadTestDefaults -Path $script:DefaultsPath
        $d.MaxReplicas    | Should -Be 6
        $d.MaxConcurrency | Should -Be 1
        $d.MessageCount   | Should -Be 100
        $d.QueueName      | Should -Be 'order-success-queue'
    }

    It 'carries the timeout chain in the required order' {
        $d = Get-WwLoadTestDefaults -Path $script:DefaultsPath
        $d.EngineTimeoutSeconds | Should -BeLessOrEqual $d.ShutdownGraceSeconds
        $d.ShutdownGraceSeconds | Should -BeLessThan    $d.TerminationGracePeriodSeconds
    }

    It 'contains no secret-looking values' {
        # This file is committed and read by reviewers. A password reaching it is a leak, not a bug.
        $raw = Get-Content -LiteralPath $script:DefaultsPath -Raw
        $raw | Should -Not -Match '(?i)password\s*='
        $raw | Should -Not -Match '(?i)\bpwd\s*='
        $raw | Should -Not -Match '(?i)AccountKey='
    }

    It 'throws a clear error for a missing file' {
        { Get-WwLoadTestDefaults -Path 'X:\nope\missing.psd1' } | Should -Throw '*not found*'
    }
}

Describe 'Resolve-WwEngineConcurrency' {

    It 'multiplies replicas by worker concurrency' {
        Resolve-WwEngineConcurrency -MaxReplicas 6 -MaxConcurrency 1 | Should -Be 6
        Resolve-WwEngineConcurrency -MaxReplicas 3 -MaxConcurrency 4 | Should -Be 12
    }

    It 'treats both operands as significant' {
        # Halving replicas while doubling concurrency changes nothing. Tuning one alone is the
        # mistake this function exists to make visible.
        Resolve-WwEngineConcurrency -MaxReplicas 6  -MaxConcurrency 1 |
            Should -Be (Resolve-WwEngineConcurrency -MaxReplicas 3 -MaxConcurrency 2)
    }

    It 'is zero for a parked app' {
        Resolve-WwEngineConcurrency -MaxReplicas 0 -MaxConcurrency 1 | Should -Be 0
    }

    It 'rejects nonsensical scale' {
        { Resolve-WwEngineConcurrency -MaxReplicas -1 -MaxConcurrency 1 } | Should -Throw '*Invalid scale*'
        { Resolve-WwEngineConcurrency -MaxReplicas 6  -MaxConcurrency 0 } | Should -Throw '*Invalid scale*'
    }
}

Describe 'Read-WwSetting' {

    It 'prefers an explicitly provided value over the default' {
        Read-WwSetting -Prompt 'x' -Default 'dflt' -Provided 'mine' -NonInteractive | Should -Be 'mine'
    }

    It 'falls back to the default when nothing is provided' {
        Read-WwSetting -Prompt 'x' -Default 'dflt' -Provided $null -NonInteractive | Should -Be 'dflt'
        Read-WwSetting -Prompt 'x' -Default 'dflt' -Provided ''    -NonInteractive | Should -Be 'dflt'
    }

    It 'honours a provided zero rather than treating it as absent' {
        # -FailureCount 0 is a real instruction ("no deliberate failures"), not a missing value.
        Read-WwSetting -Prompt 'x' -Default 5 -Provided 0 -NonInteractive -AsInt | Should -Be 0
    }

    It 'returns an int when asked' {
        Read-WwSetting -Prompt 'x' -Default 6 -Provided $null -NonInteractive -AsInt | Should -BeOfType [int]
    }

    It 'never prompts in non-interactive mode' {
        # A prompt here would hang CI forever rather than fail.
        Mock Read-Host { throw 'Read-Host must not be called' }
        { Read-WwSetting -Prompt 'x' -Default 'd' -NonInteractive } | Should -Not -Throw
    }
}

Describe 'Numeric parameters are nullable' {

    # REGRESSION. A plain [int] parameter that is not supplied defaults to 0, and Read-WwSetting
    # honours an explicit 0 - correctly, because -FailureCount 0 is a real instruction. Together
    # those two correct behaviours meant every unbound int overrode its RUN 2 default with zero:
    # the first live run resolved MaxReplicas=0 and MaxConcurrency=0 and died at
    # "Invalid scale: MaxReplicas=0 MaxConcurrency=0". Nullable is what separates "not supplied"
    # from "zero".
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:LoadTestScript, [ref]$null, [ref]$null)
        $script:Params = @{}
        foreach ($p in $ast.ParamBlock.Parameters) {
            $script:Params[$p.Name.VariablePath.UserPath] = $p.StaticType.FullName
        }
    }

    # StaticType.FullName is assembly-qualified for the generic argument
    # ("System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=...]]"), so match the shape
    # rather than pinning a string that changes with the runtime version.
    It 'declares <_> as a nullable int' -ForEach @('MessageCount', 'FailureCount', 'MaxReplicas', 'MaxConcurrency') {
        $script:Params[$_] | Should -BeLike 'System.Nullable*System.Int32*'
    }

    It 'leaves no bare [int] parameter that a caller might omit' {
        @($script:Params.GetEnumerator() | Where-Object { $_.Value -eq 'System.Int32' }).Count | Should -Be 0
    }
}

Describe 'Get-WwMaskedConnectionString' {

    It 'extracts server, database and SQL login without the password' {
        $r = Get-WwMaskedConnectionString -ConnectionString 'Server=tcp:sql1.database.windows.net,1433;Database=jobs;User ID=svc;Password=Sup3rSecret!;'
        $r.Server   | Should -Be 'tcp:sql1.database.windows.net,1433'
        $r.Database | Should -Be 'jobs'
        $r.Auth     | Should -Be 'SqlLogin (svc)'
    }

    It 'never returns the password in any field' {
        $r = Get-WwMaskedConnectionString -ConnectionString 'Server=s;Database=d;User ID=u;Password=Sup3rSecret!;'
        ($r | ConvertTo-Json) | Should -Not -Match 'Sup3rSecret'
    }

    It 'recognises the Data Source / Initial Catalog spelling' {
        $r = Get-WwMaskedConnectionString -ConnectionString 'Data Source=localhost;Initial Catalog=jobs;Integrated Security=True;'
        $r.Server   | Should -Be 'localhost'
        $r.Database | Should -Be 'jobs'
        $r.Auth     | Should -Be 'Integrated'
    }

    It 'returns null for an empty string' {
        Get-WwMaskedConnectionString -ConnectionString '' | Should -BeNullOrEmpty
    }
}

Describe 'Get-WwBrokerSourceDiagnosis' {

    # REGRESSION. Resolve-E2EBrokerUri returns $null for six different reasons. The original error
    # message picked ONE of them - "a WFAES-encrypted source needs the Key Vault key" - and asserted
    # it unconditionally. A reviewer who mistyped 'RabbitMQAshley.bite' for
    # 'RabbitMQSourceAshley.bite' was told to go and find a Key Vault key for a file that did not
    # exist. An assumed cause is worse than no cause.

    BeforeAll {
        $script:Fx = Join-Path ([System.IO.Path]::GetTempPath()) "wwlt-diag-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Force $script:Fx | Out-Null

        Set-Content (Join-Path $script:Fx 'plain.bite') `
            '<Source ID="1" Name="s" ResourceType="RabbitMQSource" ConnectionString="HostName=h;Port=5672;UserName=u;Password=p;VirtualHost=/" />'
        Set-Content (Join-Path $script:Fx 'wfaes.bite') `
            '<Source ID="1" Name="s" ResourceType="RabbitMQSource" ConnectionString="WFAES::uHDZGhSWiqEEcIaLC7L5Xi4MM" />'
        Set-Content (Join-Path $script:Fx 'nocs.bite') `
            '<Source ID="1" Name="s" ResourceType="RabbitMQSource" />'
        Set-Content (Join-Path $script:Fx 'dpapi.bite') `
            '<Source ID="1" Name="s" ResourceType="RabbitMQSource" ConnectionString="AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAA" />'
        Set-Content (Join-Path $script:Fx 'trigger.bite') '{ "QueueName": "q", "Concurrency": 6 }'
    }
    AfterAll { if (Test-Path $script:Fx) { Remove-Item $script:Fx -Recurse -Force -ErrorAction SilentlyContinue } }

    It 'reports FileNotFound for a path that does not exist' {
        (Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'nope.bite')).Reason | Should -Be 'FileNotFound'
    }

    It 'lists the neighbouring .bite files so a typo is visible' {
        # The actual failure: 'RabbitMQAshley.bite' typed for 'RabbitMQSourceAshley.bite'. Listing
        # the directory turns a Key Vault goose-chase into an obvious one-character fix.
        $d = Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'RabbitMQAshley.bite')
        $d.Reason | Should -Be 'FileNotFound'
        $d.Hint   | Should -Match 'plain\.bite'
    }

    It 'says so when the containing directory is missing too' {
        (Get-WwBrokerSourceDiagnosis -Path 'X:\definitely\not\here\x.bite').Hint | Should -Match 'does not exist either'
    }

    It 'reports WfAesEncrypted only when the value really is WFAES' {
        $d = Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'wfaes.bite')
        $d.Reason | Should -Be 'WfAesEncrypted'
        $d.Hint   | Should -Match 'Encrypt-Config'
    }

    It 'reports DpapiUndecryptable for a base64 blob it cannot read here' {
        $d = Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'dpapi.bite')
        $d.Reason | Should -Be 'DpapiUndecryptable'
        $d.Hint   | Should -Match 'machine'
    }

    It 'reports NoConnectionString when the attribute is absent' {
        (Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'nocs.bite')).Reason | Should -Be 'NoConnectionString'
    }

    It 'reports NotXml when handed a JSON trigger by mistake' {
        $d = Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'trigger.bite')
        $d.Reason | Should -Be 'NotXml'
        $d.Hint   | Should -Match 'trigger'
    }

    It 'reports NoPath for an empty path' {
        (Get-WwBrokerSourceDiagnosis -Path '').Reason | Should -Be 'NoPath'
    }

    It 'clears a source that is plainly readable instead of inventing a fault' {
        # The helper doubles as a pre-flight check on a source, so a good file must come back clean
        # rather than with a manufactured reason.
        (Get-WwBrokerSourceDiagnosis -Path (Join-Path $script:Fx 'plain.bite')).Reason | Should -Be 'LooksResolvable'
    }

    It 'never reports WfAesEncrypted for a file that does not exist' {
        # The exact false diagnosis that started this: a missing file blamed on Key Vault encryption.
        (Get-WwBrokerSourceDiagnosis -Path 'X:\nope\missing.bite').Reason | Should -Not -Be 'WfAesEncrypted'
    }
}

Describe 'Test-WwRequiredPath' {

    BeforeAll {
        $script:Px = Join-Path ([System.IO.Path]::GetTempPath()) "wwlt-path-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Force (Join-Path $script:Px 'pub') | Out-Null
        Set-Content (Join-Path $script:Px 'pub\RabbitMQ.Client.dll') 'x'
        Set-Content (Join-Path $script:Px 'a-file.bite') 'x'
    }
    AfterAll { if (Test-Path $script:Px) { Remove-Item $script:Px -Recurse -Force -ErrorAction SilentlyContinue } }

    It 'returns null when the path is valid' {
        Test-WwRequiredPath -Label 'Pub' -Path (Join-Path $script:Px 'pub') -Kind Directory | Should -BeNullOrEmpty
    }

    It 'returns null when the required content is present' {
        Test-WwRequiredPath -Label 'Pub' -Path (Join-Path $script:Px 'pub') -Kind Directory `
            -MustContain 'RabbitMQ.Client.dll' | Should -BeNullOrEmpty
    }

    It 'catches a publish directory with no RabbitMQ.Client.dll' {
        # Without the DLL the failure is BrokerUnreachableException - "none of the specified endpoints
        # were reachable" - which reads as a dead broker rather than a wrong folder.
        Test-WwRequiredPath -Label 'Pub' -Path $script:Px -Kind Directory -MustContain 'RabbitMQ.Client.dll' |
            Should -Match "does not contain 'RabbitMQ.Client.dll'"
    }

    It 'lists neighbouring entries when the path is missing' {
        Test-WwRequiredPath -Label 'Source' -Path (Join-Path $script:Px 'wrong-name.bite') |
            Should -Match 'a-file\.bite'
    }

    It 'reports an unsupplied path' {
        Test-WwRequiredPath -Label 'Source' -Path '' | Should -Match 'was not supplied'
    }
}

Describe 'ConvertTo-WwDateTimeOrNull' {

    # REGRESSION. The original code used [datetime]::TryParse($str, [ref]$x) against variables
    # initialised to $null. PowerShell cannot bind [ref] to an untyped $null and fails overload
    # resolution outright - "Cannot find an overload for TryParse and the argument count: 2" - which
    # threw in Phase 8, AFTER a full run had been published, drained and reported.

    It 'parses the ISO-8601 form SQL CONVERT style 126 emits' {
        $r = ConvertTo-WwDateTimeOrNull '2026-08-13T07:45:12.345'
        $r      | Should -BeOfType [datetime]
        $r.Year | Should -Be 2026
        $r.Hour | Should -Be 7
    }

    It 'parses a value carrying a Z suffix' {
        (ConvertTo-WwDateTimeOrNull '2026-08-13T07:45:12Z') | Should -BeOfType [datetime]
    }

    It 'returns null for null, empty and whitespace' {
        ConvertTo-WwDateTimeOrNull $null  | Should -BeNullOrEmpty
        ConvertTo-WwDateTimeOrNull ''     | Should -BeNullOrEmpty
        ConvertTo-WwDateTimeOrNull '   '  | Should -BeNullOrEmpty
    }

    It 'returns null rather than throwing on an unparseable value' {
        # A NULL FinishedAtUtc is normal for a row still in flight; it must not abort the report.
        { ConvertTo-WwDateTimeOrNull 'not-a-date' } | Should -Not -Throw
        ConvertTo-WwDateTimeOrNull 'not-a-date' | Should -BeNullOrEmpty
    }

    It 'returns null for the /Date(...)/ form Windows PowerShell 5.1 ConvertTo-Json emits' {
        # Why the SQL layer converts timestamps to strings rather than returning datetimes.
        ConvertTo-WwDateTimeOrNull '/Date(1786000000000)/' | Should -BeNullOrEmpty
    }

    It 'is used instead of a [datetime]::TryParse call anywhere in the script' {
        # Matched against the AST, not the raw text: the helper's own doc comment names the broken
        # pattern on purpose, and a text match flagged that explanation as the defect.
        #
        # Scoped to datetime deliberately. [int]::TryParse in Read-WwSetting is CORRECT - its $parsed
        # is initialised to 0, so it is already typed and [ref] binds. The defect is specific to a
        # [ref] on a variable still holding $null.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:LoadTestScript, [ref]$null, [ref]$null)
        $calls = $ast.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Member.Value -eq 'TryParse' -and
            $n.Expression -is [System.Management.Automation.Language.TypeExpressionAst] -and
            $n.Expression.TypeName.Name -match '^(datetime|System\.DateTime)$'
        }, $true)
        @($calls).Count | Should -Be 0
    }
}

Describe 'Warm-up rows are excluded from the run' {

    # The pre-warm executes the REAL workflow and commits REAL rows, and it drives real engine HTTP
    # requests. Two different contaminations, needing two different mechanisms - both asserted here
    # because neither is visible until a full run has already completed.

    BeforeAll { $script:Source = Get-Content -LiteralPath $script:LoadTestScript -Raw }

    It 'takes a baseline watermark before pre-warming and re-reads it after' {
        $script:Source | Should -Match 'baselineWatermark'
        # The re-read must come after the pre-warm invocation, not before it.
        $preWarmAt   = $script:Source.IndexOf('Invoke-WwEnginePreWarm.ps1')
        $rereadAt    = $script:Source.IndexOf('Run watermark')
        $rereadAt | Should -BeGreaterThan $preWarmAt
    }

    It 'reports how many rows the pre-warm wrote instead of absorbing them silently' {
        $script:Source | Should -Match 'Pre-warm wrote'
    }

    It 'clamps the report window so warm-up engine requests are excluded' {
        # Without the clamp, the one minute of slack on $startUtc reaches back into the pre-warm on a
        # warm engine and folds its requests into the run's result codes and latency percentiles.
        $script:Source | Should -Match 'preWarmEndUtc'
        $script:Source | Should -Match 'startUtc -lt \$preWarmEndUtc'
    }

    It 'never deletes rows' {
        # The watermark isolates the run without destroying anyone else's data.
        $script:Source | Should -Not -Match '(?i)\bTRUNCATE\s+TABLE\b'
        $script:Source | Should -Not -Match '(?i)\bDELETE\s+FROM\b'
    }
}

Describe 'Get-WwPercentile' {

    It 'computes nearest-rank percentiles' {
        $v = 1..100 | ForEach-Object { [double]$_ }
        Get-WwPercentile -Values $v -Percentile 50 | Should -Be 50
        Get-WwPercentile -Values $v -Percentile 95 | Should -Be 95
        Get-WwPercentile -Values $v -Percentile 100 | Should -Be 100
    }

    It 'handles a single value' {
        Get-WwPercentile -Values @(42.0) -Percentile 95 | Should -Be 42
    }

    It 'returns null rather than zero for an empty set' {
        # 0 ms would read as an implausibly fast run rather than as an absence of measurements.
        Get-WwPercentile -Values @() -Percentile 95 | Should -BeNullOrEmpty
    }
}

Describe 'Get-WwReconciliationBuckets' {

    It 'puts a message with a row and no dead-letter in Clean' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 3) -DbTxns @('T-S-1','T-S-2','T-S-3') -DlqTxns @()
        $b.Clean.Count   | Should -Be 3
        $b.DlqOnly.Count | Should -Be 0
        $b.Both.Count    | Should -Be 0
        $b.Neither.Count | Should -Be 0
    }

    It 'puts a message with a dead-letter and no row in DlqOnly' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 2) -DbTxns @() -DlqTxns @('T-S-1','T-S-2')
        $b.DlqOnly.Count | Should -Be 2
    }

    It 'puts a message with BOTH in Both' {
        # The engine committed and the response was lost. Replaying duplicates committed work, so
        # this must never be collapsed into either of its neighbours.
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('T-S-1') -DlqTxns @('T-S-1')
        $b.Both.Count    | Should -Be 1
        $b.Clean.Count   | Should -Be 0
        $b.DlqOnly.Count | Should -Be 0
    }

    It 'puts a message with no trace at all in Neither' {
        # The failure a drained queue hides, and the only one findable purely by absence.
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 5) -DbTxns @('T-S-1','T-S-2') -DlqTxns @()
        $b.Neither.Count | Should -Be 3
    }

    It 'always sums the four buckets to the published count' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 10 -Failure 5) `
                -DbTxns @('T-S-1','T-S-2','T-S-3') -DlqTxns @('T-S-3','T-F-1','T-F-2')
        ($b.Clean.Count + $b.DlqOnly.Count + $b.Both.Count + $b.Neither.Count) | Should -Be $b.Published
        $b.Published | Should -Be 15
    }

    It 'expects a deliberate failure to land in DlqOnly and scores it as correct' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Failure 3) -DbTxns @() -DlqTxns @('T-F-1','T-F-2','T-F-3')
        $b.DlqOnly.Count      | Should -Be 3
        $b.WrongOutcome.Count | Should -Be 0
    }

    It 'flags a deliberate failure that succeeded' {
        # The failure path silently stopped working. Without this the run would report a clean pass.
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Failure 1) -DbTxns @('T-F-1') -DlqTxns @()
        $b.WrongOutcome.Count       | Should -Be 1
        $b.WrongOutcome[0].Expected | Should -Be 'DlqOnly'
        $b.WrongOutcome[0].Actual   | Should -Be 'Clean'
    }

    It 'flags a valid message that dead-lettered' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @() -DlqTxns @('T-S-1')
        $b.WrongOutcome.Count       | Should -Be 1
        $b.WrongOutcome[0].Expected | Should -Be 'Clean'
    }

    It 'reports observed transaction ids that were never published' {
        # Stale rows above the watermark, or a competing publisher on the same queue.
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('T-S-1','STRANGER') -DlqTxns @()
        $b.Unexpected.Count | Should -Be 1
        $b.Unexpected[0]    | Should -Be 'STRANGER'
    }

    It 'matches transaction ids case-insensitively' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('t-s-1') -DlqTxns @()
        $b.Clean.Count | Should -Be 1
    }

    It 'handles an empty manifest without throwing' {
        $b = Get-WwReconciliationBuckets -Manifest @() -DbTxns @() -DlqTxns @()
        $b.Published | Should -Be 0
    }
}

Describe 'Get-WwRunVerdict' {

    It 'passes a run where every message reached its expected state' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 100) -DbTxns (1..100 | ForEach-Object { "T-S-$_" }) -DlqTxns @()
        (Get-WwRunVerdict -Buckets $b).Pass | Should -BeTrue
    }

    It 'passes a mixed run where the deliberate failures dead-lettered' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 30 -Failure 5) `
                -DbTxns (1..30 | ForEach-Object { "T-S-$_" }) -DlqTxns (1..5 | ForEach-Object { "T-F-$_" })
        (Get-WwRunVerdict -Buckets $b).Pass | Should -BeTrue
    }

    It 'FAILS a run that published nothing' {
        # Every "no failures" condition is vacuously true on an empty run. Unguarded, this is the
        # single most misleading output the script could produce: a clean PASS for a run that never
        # happened.
        $b = Get-WwReconciliationBuckets -Manifest @() -DbTxns @() -DlqTxns @()
        $v = Get-WwRunVerdict -Buckets $b
        $v.Pass      | Should -BeFalse
        $v.Reasons[0] | Should -Match 'Nothing was published'
    }

    It 'FAILS when messages were lost' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 10) -DbTxns @('T-S-1') -DlqTxns @()
        $v = Get-WwRunVerdict -Buckets $b
        $v.Pass | Should -BeFalse
        ($v.Reasons -join ' ') | Should -Match 'LOST'
    }

    It 'FAILS when a message both committed and dead-lettered' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('T-S-1') -DlqTxns @('T-S-1')
        $v = Get-WwRunVerdict -Buckets $b
        $v.Pass | Should -BeFalse
        ($v.Reasons -join ' ') | Should -Match 'duplicates committed work'
    }

    It 'FAILS on duplicate database rows' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 2) -DbTxns @('T-S-1','T-S-2') -DlqTxns @()
        $v = Get-WwRunVerdict -Buckets $b -DuplicateDbRows 1
        $v.Pass | Should -BeFalse
        ($v.Reasons -join ' ') | Should -Match 'executed more than once'
    }

    It 'FAILS on deliveries with no terminal outcome' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('T-S-1') -DlqTxns @()
        (Get-WwRunVerdict -Buckets $b -UnknownOutcome 3).Pass | Should -BeFalse
    }

    It 'FAILS when the database was skipped, because "committed" is then unproven' {
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 1) -DbTxns @('T-S-1') -DlqTxns @()
        $v = Get-WwRunVerdict -Buckets $b -DatabaseSkipped
        $v.Pass | Should -BeFalse
        ($v.Reasons -join ' ') | Should -Match 'SKIPPED'
    }

    It 'names every failing criterion rather than just the first' {
        # A verdict that says FAIL without naming the reason sends the reader back to the raw logs,
        # which is the work this script exists to avoid.
        $b = Get-WwReconciliationBuckets -Manifest (New-Manifest -Success 10) -DbTxns @('T-S-1') -DlqTxns @('T-S-1')
        $v = Get-WwRunVerdict -Buckets $b -DuplicateDbRows 2 -UnknownOutcome 1
        $v.Reasons.Count | Should -BeGreaterThan 2
    }
}

Describe 'Get-WwPreflightBlockers' {

    It 'passes a healthy deployment' {
        (Get-WwPreflightBlockers -Preflight (New-Preflight)).Count | Should -Be 0
    }

    It 'blocks a stopped engine' {
        # A stopped Function App returns 403 Site Disabled on every route, which reads as an
        # authorization failure and sends the reader off investigating Entra.
        $b = Get-WwPreflightBlockers -Preflight (New-Preflight -EngineState 'Stopped')
        $b.Count | Should -Be 1
        $b[0] | Should -Match 'not Running'
    }

    It 'blocks a missing Container App' {
        (Get-WwPreflightBlockers -Preflight (New-Preflight -WorkerExists $false))[0] | Should -Match 'does not exist'
    }

    It 'blocks a worker parked at maxReplicas 0' {
        (Get-WwPreflightBlockers -Preflight (New-Preflight -MaxReplicas 0))[0] | Should -Match 'never consume'
    }

    It 'blocks a missing rabbitmq scale rule' {
        (Get-WwPreflightBlockers -Preflight (New-Preflight -HasRabbitRule $false))[0] | Should -Match 'will not scale'
    }

    It 'blocks a KEDA rule pointing at a different queue' {
        # The worker sits at zero replicas while the queue fills, and the run times out looking like
        # a dead engine.
        $b = Get-WwPreflightBlockers -Preflight (New-Preflight -KedaQueue 'some-other-queue')
        $b[0] | Should -Match 'never wake up'
    }

    It 'blocks a competing consumer on the same queue' {
        # The most damaging false pass: the second app takes its share and, pointed at a stopped or
        # older engine, dead-letters AND acks them. The queue drains, replicas scale to zero, and the
        # run looks clean while half the messages never executed.
        $b = Get-WwPreflightBlockers -Preflight (New-Preflight -Competing @(
                [pscustomobject]@{ App = 'wwqp2-ordersuccessqueue'; Queue = 'order-success-queue'; MinReplicas = 0; MaxReplicas = 3 }))
        $b.Count | Should -Be 1
        $b[0] | Should -Match 'Competing consumer'
    }

    It 'blocks a queue that does not exist on the broker' {
        # PublishRabbitMQActivity cannot create one: its passive declare closes the channel and the
        # active declare is then reissued on that dead channel.
        (Get-WwPreflightBlockers -Preflight (New-Preflight -QueueExists $false))[0] | Should -Match 'does not exist on the broker'
    }

    It 'reports every blocker, not just the first' {
        $b = Get-WwPreflightBlockers -Preflight (New-Preflight -EngineState 'Stopped' -MaxReplicas 0 -QueueExists $false)
        $b.Count | Should -BeGreaterOrEqual 3
    }

    It 'blocks when the engine could not be resolved at all' {
        $p = New-Preflight
        $p.Engine = $null
        (Get-WwPreflightBlockers -Preflight $p)[0] | Should -Match 'could not be resolved'
    }
}
