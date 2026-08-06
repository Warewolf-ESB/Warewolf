#Requires -Version 7.0

<#
    Pester 5 test suite for Deploy-WwQueueProcessor.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Deploy-WwQueueProcessor.Tests.ps1

    Design notes (identical conventions to Deploy-WwJobProcessor.Tests.ps1)
    -----------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no cloud/filesystem action, so the
      derivation logic (slug, scale settings, trigger reading, guards) is unit-testable
      without an Azure CLI or a broker.
    * `az` is shadowed with a tracking *function* (functions win over applications) so the
      suite behaves identically with or without the Azure CLI installed.
    * The trigger fixture is the REAL production shape (a `$type`/`$id` Dev2JsonSerializer
      payload with a `#{...}` release token variant), so the guards are pinned against what
      the Server actually writes rather than an idealised file.
#>

BeforeAll {
    $script:DeployScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwQueueProcessor.ps1'

    # ── Fixtures: a real trigger definition + its RabbitMQ source ───────────────
    $script:FixtureDir = Join-Path ([System.IO.Path]::GetTempPath()) "wwqp-tests-$([Guid]::NewGuid())"
    New-Item -ItemType Directory -Force $script:FixtureDir | Out-Null

    $script:TriggerJson = @'
{
  "$id": "1",
  "$type": "Warewolf.Trigger.Queue.TriggerQueue, Warewolf.Trigger.Queue",
  "TriggerId": "1ac40da8-3b56-45f8-a1aa-00e6864db38b",
  "Name": "MandateCollectionSuccessTrigger",
  "QueueSourceId": "0b142714-8f6d-41b7-9832-2aefa8c731ec",
  "QueueName": "profiler.mandatecollectionsuccess.request",
  "WorkflowName": "ProfilerWrapper\\Queue\\MandateCollectionSuccessConsume",
  "Concurrency": 5,
  "Prefetch": "10",
  "QueueSinkId": "0b142714-8f6d-41b7-9832-2aefa8c731ec",
  "DeadLetterQueue": "profiler.mandatecollectionsuccess.error.request",
  "MapEntireMessage": true,
  "Options": [
    { "$id": "2", "$type": "Warewolf.Options.OptionBool, Warewolf.Data", "Name": "Durable", "Value": true }
  ],
  "Inputs": [
    { "$id": "3", "$type": "Warewolf.Core.ServiceInput, Warewolf.Core", "Name": "PayloadRequest", "Value": "" }
  ]
}
'@
    Set-Content (Join-Path $script:FixtureDir 'triggers-mandate.bite') $script:TriggerJson -Encoding UTF8

    # Same file with the release token still unsubstituted.
    $script:TriggerJson.Replace('"Concurrency": 5,', '"Concurrency": #{WwConcurrency},') |
        Set-Content (Join-Path $script:FixtureDir 'triggers-token.bite') -Encoding UTF8

    # A second, disabled trigger (Concurrency 0) for the disabled-app case.
    $script:TriggerJson.Replace('"Concurrency": 5,', '"Concurrency": 0,').
        Replace('MandateCollectionSuccessTrigger', 'DisabledTrigger').
        Replace('1ac40da8-3b56-45f8-a1aa-00e6864db38b', '2bd51eb9-4c67-56g9-b2bb-11f7975ec49c') |
        Set-Content (Join-Path $script:FixtureDir 'other-disabled.bite') -Encoding UTF8

    Set-Content (Join-Path $script:FixtureDir '0b142714-8f6d-41b7-9832-2aefa8c731ec.bite') @'
<Source ID="0b142714-8f6d-41b7-9832-2aefa8c731ec" Name="Warewolf DevOps RabbitMQ Source" ResourceType="RabbitMQSource" ConnectionString="HostName=server.ngrok.io;Port=20313;UserName=testuser;Password=test123;VirtualHost=/" Type="RabbitMQSource" />
'@ -Encoding UTF8
}

AfterAll {
    if ($script:FixtureDir -and (Test-Path $script:FixtureDir)) {
        Remove-Item $script:FixtureDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Describe 'Deploy-WwQueueProcessor — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'rejects an invalid -ScalingMode via ValidateSet' {
        { & $script:DeployScript -ScalingMode 'Turbo' -LoadFunctionsOnly } | Should -Throw
    }

    It 'rejects an invalid -ExecutionLogLevel via ValidateSet' {
        { & $script:DeployScript -ExecutionLogLevel 'LOUD' -LoadFunctionsOnly } | Should -Throw
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:DeployScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Preflight'

        foreach ($fn in @('Get-AppNameSlug', 'Resolve-AppNameCollision', 'Read-TriggerFile',
                          'Resolve-SourceFile', 'Get-ScaleSettings', 'Assert-TimeoutNesting',
                          'New-StagingDirectory', 'Invoke-Az', 'Read-Required')) {
            Get-Command $fn -CommandType Function -ErrorAction SilentlyContinue |
                Should -Not -BeNullOrEmpty -Because "$fn must be dot-sourceable for unit testing"
        }
    }
}

Describe 'Deploy-WwQueueProcessor — Read-TriggerFile' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'reads the real $type/$id payload and projects the deploy-relevant fields' {
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')

        $t.Name          | Should -Be 'MandateCollectionSuccessTrigger'
        $t.QueueName     | Should -Be 'profiler.mandatecollectionsuccess.request'
        $t.Concurrency   | Should -Be 5
        $t.Prefetch      | Should -Be 10
        $t.QueueSourceId | Should -Be '0b142714-8f6d-41b7-9832-2aefa8c731ec'
        $t.DeadLetterQueue | Should -Be 'profiler.mandatecollectionsuccess.error.request'
    }

    It 'normalises backslash workflow separators to forward slashes' {
        # Per-segment URL escaping splits on '/', so a backslash would become %5C and the
        # engine route would not resolve.
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $t.WorkflowName | Should -Be 'ProfilerWrapper/Queue/MandateCollectionSuccessConsume'
        $t.WorkflowName | Should -Not -Match '\\'
    }

    It 'throws an actionable error when a release token is unsubstituted' {
        # maxReplicas derives from Concurrency, so a token here would silently deploy the
        # wrong capacity.
        { Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-token.bite') } |
            Should -Throw -ExpectedMessage '*unsubstituted release token*'
    }

    It 'throws when the trigger is not plaintext JSON (DPAPI / WFAES blob)' {
        $blob = Join-Path $script:FixtureDir 'dpapi.bite'
        Set-Content $blob ([Convert]::ToBase64String([byte[]](1..16))) -Encoding UTF8
        { Read-TriggerFile -Path $blob } | Should -Throw -ExpectedMessage '*not plaintext JSON*'
    }

    It 'defaults Prefetch to 1 when absent' {
        $noPrefetch = Join-Path $script:FixtureDir 'no-prefetch.bite'
        ($script:TriggerJson -replace '"Prefetch": "10",', '') | Set-Content $noPrefetch -Encoding UTF8
        (Read-TriggerFile -Path $noPrefetch).Prefetch | Should -Be 1
    }
}

Describe 'Deploy-WwQueueProcessor — Get-AppNameSlug' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'slugs a trigger name into a lowercase, hyphenated app name' {
        Get-AppNameSlug -Name 'Orders Trigger' -Fallback 'q' -Id 'id-1' -Prefix 'wwqp-' |
            Should -Be 'wwqp-orders-trigger'
    }

    It 'falls back to the queue name when the trigger has no name' {
        Get-AppNameSlug -Name '' -Fallback 'orders.queue' -Id 'id-1' -Prefix 'wwqp-' |
            Should -Be 'wwqp-orders-queue'
    }

    It 'never exceeds the 32-character ACA limit' {
        $name = Get-AppNameSlug -Name 'MandateCollectionSuccessTrigger' -Fallback 'q' `
                                -Id '1ac40da8-3b56-45f8-a1aa-00e6864db38b' -Prefix 'wwqp-'
        $name.Length | Should -BeLessOrEqual 32
    }

    It 'appends a TriggerId hash when truncating, so long similar names cannot collide' {
        $a = Get-AppNameSlug -Name 'AVeryLongTriggerNameThatWillDefinitelyBeTruncatedHere' `
                             -Fallback 'q' -Id 'trigger-a' -Prefix 'wwqp-'
        $b = Get-AppNameSlug -Name 'AVeryLongTriggerNameThatWillDefinitelyBeTruncatedHere' `
                             -Fallback 'q' -Id 'trigger-b' -Prefix 'wwqp-'

        $a | Should -Not -Be $b
        $a.Length | Should -BeLessOrEqual 32
        $b.Length | Should -BeLessOrEqual 32
    }

    It 'produces a deterministic name for the same trigger' {
        # Not named $args: that is a PowerShell automatic variable, and assigning to it inside a
        # scope that later calls a function can change how arguments are bound.
        $splat = @{ Name = 'Same'; Fallback = 'q'; Id = 'id-9'; Prefix = 'wwqp-' }
        (Get-AppNameSlug @splat) | Should -Be (Get-AppNameSlug @splat)
    }

    It '-ForceHash appends the TriggerId hash even when the slug already fits' {
        $plain  = Get-AppNameSlug -Name 'Short' -Fallback 'q' -Id 'id-1' -Prefix 'wwqp-'
        $forced = Get-AppNameSlug -Name 'Short' -Fallback 'q' -Id 'id-1' -Prefix 'wwqp-' -ForceHash

        $plain  | Should -Be 'wwqp-short'
        $forced | Should -Match '^wwqp-short-[0-9a-f]{4}$'
        $forced.Length | Should -BeLessOrEqual 32
    }

    It '-ForceHash keeps names distinct for identical short names with different ids' {
        $a = Get-AppNameSlug -Name 'OrderQueue' -Fallback 'q' -Id 'id-a' -Prefix 'wwqp-' -ForceHash
        $b = Get-AppNameSlug -Name 'OrderQueue' -Fallback 'q' -Id 'id-b' -Prefix 'wwqp-' -ForceHash
        $a | Should -Not -Be $b
    }
}

Describe 'Deploy-WwQueueProcessor — Resolve-AppNameCollision' {
    <#
        Why this exists: a trigger's display Name is NOT unique. Warewolf names triggers after
        their logical purpose and lets the QUEUE distinguish them, so two triggers legitimately
        share a Name. Because `az containerapp create` on an existing name UPDATES that app, an
        unresolved collision would leave one queue with no worker at all - and the symptom looks
        like a KEDA scaling fault, not a naming one.
    #>

    BeforeAll {
        . $script:DeployScript -LoadFunctionsOnly

        # Must be defined in BeforeAll, not in the Describe body: the Describe body runs during
        # Pester's DISCOVERY pass, so a function declared there does not exist when the It blocks
        # actually run.
        function script:New-Planned {
            param([string] $Name, [string] $QueueName, [string] $TriggerId)
            [pscustomobject]@{
                AppName = Get-AppNameSlug -Name $Name -Fallback $QueueName -Id $TriggerId -Prefix 'wwqp-'
                Trigger = [pscustomobject]@{ Name = $Name; QueueName = $QueueName; TriggerId = $TriggerId }
            }
        }
    }

    It 'promotes the queue name when two triggers share a display name' {
        # The exact case that blocked the live Phase C deploy.
        $planned = @(
            (script:New-Planned -Name 'OrderQueue' -QueueName 'order-success-queue' -TriggerId '03fb9052'),
            (script:New-Planned -Name 'OrderQueue' -QueueName 'order-failure-queue' -TriggerId '12345678')
        )
        $planned[0].AppName | Should -Be $planned[1].AppName -Because 'both derive from the same Name'

        $out = Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-'

        $out[0].AppName | Should -Be 'wwqp-order-success-queue'
        $out[1].AppName | Should -Be 'wwqp-order-failure-queue'
        ($out | Select-Object -ExpandProperty AppName -Unique).Count | Should -Be 2
    }

    It 'leaves already-unique names untouched' {
        # Regression guard: the readable, Name-derived form must survive when there is no clash,
        # otherwise every existing deployment would be renamed into a new app.
        $planned = @(
            (script:New-Planned -Name 'AlphaTrigger' -QueueName 'alpha-queue' -TriggerId 'id-a'),
            (script:New-Planned -Name 'BetaTrigger'  -QueueName 'beta-queue'  -TriggerId 'id-b')
        )
        $out = Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-'

        $out[0].AppName | Should -Be 'wwqp-alphatrigger'
        $out[1].AppName | Should -Be 'wwqp-betatrigger'
    }

    It 'falls through to the TriggerId hash when Name AND QueueName both match' {
        # Two triggers on the same queue: the queue cannot separate them, so only the id can.
        $planned = @(
            (script:New-Planned -Name 'Same' -QueueName 'shared-queue' -TriggerId 'id-one'),
            (script:New-Planned -Name 'Same' -QueueName 'shared-queue' -TriggerId 'id-two')
        )
        $out = Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-'

        $out[0].AppName | Should -Not -Be $out[1].AppName
        $out[0].AppName | Should -Match '^wwqp-shared-queue-[0-9a-f]{4}$'
        $out[1].AppName | Should -Match '^wwqp-shared-queue-[0-9a-f]{4}$'
    }

    It 'keeps disambiguated names inside the 32-character ACA limit' {
        $long = 'profiler.mandatecollectionsuccess.request.overflow'
        $planned = @(
            (script:New-Planned -Name 'Dup' -QueueName $long -TriggerId 'id-a'),
            (script:New-Planned -Name 'Dup' -QueueName $long -TriggerId 'id-b')
        )
        $out = Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-'

        $out[0].AppName | Should -Not -Be $out[1].AppName
        foreach ($o in $out) { $o.AppName.Length | Should -BeLessOrEqual 32 }
    }

    It 'is idempotent — resolving twice changes nothing' {
        $planned = @(
            (script:New-Planned -Name 'OrderQueue' -QueueName 'order-success-queue' -TriggerId 'id-a'),
            (script:New-Planned -Name 'OrderQueue' -QueueName 'order-failure-queue' -TriggerId 'id-b')
        )
        $once  = @((Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-') | ForEach-Object { $_.AppName })
        $twice = @((Resolve-AppNameCollision -Planned $planned -Prefix 'wwqp-') | ForEach-Object { $_.AppName })
        ($twice -join ',') | Should -Be ($once -join ',')
    }
}

Describe 'Deploy-WwQueueProcessor — Get-ScaleSettings' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'derives maxReplicas from Concurrency and the KEDA target from MaxConcurrency ALONE' {
        # The target must NOT include Prefetch. Prefetch only buffers inside a replica (dispatch is
        # serial per channel), so dividing the backlog by it starves parallelism below saturation:
        # this trigger has Prefetch 10, so the old 'Prefetch x MaxConcurrency' target of 10 meant a
        # 9-message backlog ran on ONE replica where on-prem's 5 always-running workers would have
        # used 5. See Get-ScaleSettings for the worked comparison.
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.MaxReplicas       | Should -Be 5    # Concurrency
        $s.TargetQueueLength | Should -Be 1    # MaxConcurrency 1 - NOT Prefetch (10)
        $s.MinReplicas       | Should -Be 0    # Elastic is the standard
        $s.Mode              | Should -Be 'Elastic'
    }

    It 'reaches on-prem parallelism below saturation, which the old target could not' {
        # Regression lock for the throughput defect: Concurrency 5 with a 5-message backlog must
        # fan out to 5 replicas (5 concurrent executions), matching 5 always-running QueueWorkers.
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $backlog  = 5
        $replicas = [Math]::Min([Math]::Ceiling($backlog / $s.TargetQueueLength), $s.MaxReplicas)
        $replicas | Should -Be 5 -Because 'ceil(5/1) capped at 5 must use every permitted replica'
    }

    It 'treats Concurrency = 0 as a disabled app (min = max = 0)' {
        # Parity with WorkerMonitor.cs:55-58, where Concurrency 0 means the trigger is not run.
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'other-disabled.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.MaxReplicas | Should -Be 0
        $s.MinReplicas | Should -Be 0
    }

    It 'honours a manifest scalingMode override and flags it as an exception' {
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $override = [pscustomobject]@{ scalingMode = 'Warm'; justification = 'latency SLA' }
        $s = Get-ScaleSettings -Trigger $t -Override $override

        $s.Mode          | Should -Be 'Warm'
        $s.MinReplicas   | Should -Be 1
        $s.MaxReplicas   | Should -Be 5
        $s.IsException   | Should -BeTrue
        $s.Justification | Should -Be 'latency SLA'
    }

    It 'sets min = max for the Fixed (exact on-prem parity) mode' {
        . $script:DeployScript -LoadFunctionsOnly -ScalingMode Fixed
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.MinReplicas | Should -Be 5
        $s.MaxReplicas | Should -Be 5
    }

    It 'scales the KEDA target with MaxConcurrency so a replica is not under-fed' {
        # A replica that executes 4 at once needs 4 queued messages to justify the NEXT replica.
        # Still independent of Prefetch (10 on this fixture).
        . $script:DeployScript -LoadFunctionsOnly -MaxConcurrency 4
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.TargetQueueLength | Should -Be 4    # MaxConcurrency 4, Prefetch ignored
    }

    It 'still honours an explicit -TargetQueueLength, trading parallelism for fewer cold starts' {
        # The old Prefetch-based behaviour remains reachable on purpose: batching a backlog onto
        # fewer replicas is a legitimate choice for short messages where cold starts dominate.
        . $script:DeployScript -LoadFunctionsOnly -TargetQueueLength 10
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.TargetQueueLength | Should -Be 10
    }

    It 'honours a manifest targetQueueLength override per trigger' {
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override ([pscustomobject]@{ targetQueueLength = 7 })

        $s.TargetQueueLength | Should -Be 7
    }

    It 'lets an explicit -MaxReplicas override the trigger and marks it an exception' {
        . $script:DeployScript -LoadFunctionsOnly -MaxReplicas 12
        $t = Read-TriggerFile -Path (Join-Path $script:FixtureDir 'triggers-mandate.bite')
        $s = Get-ScaleSettings -Trigger $t -Override $null

        $s.MaxReplicas | Should -Be 12
        $s.IsException | Should -BeTrue
    }
}

Describe 'Deploy-WwQueueProcessor — Assert-TimeoutNesting' {

    It 'accepts engine <= drain < termination' {
        . $script:DeployScript -LoadFunctionsOnly -EngineTimeoutSeconds 45 `
            -ShutdownGraceSeconds 60 -TerminationGracePeriodSeconds 90
        { Assert-TimeoutNesting } | Should -Not -Throw
    }

    It 'rejects an engine timeout longer than the drain window (duplicate executions)' {
        . $script:DeployScript -LoadFunctionsOnly -EngineTimeoutSeconds 120 `
            -ShutdownGraceSeconds 60 -TerminationGracePeriodSeconds 90
        { Assert-TimeoutNesting } | Should -Throw -ExpectedMessage '*duplicate workflow executions*'
    }

    It 'rejects a drain window that is not inside the termination grace period' {
        . $script:DeployScript -LoadFunctionsOnly -EngineTimeoutSeconds 45 `
            -ShutdownGraceSeconds 90 -TerminationGracePeriodSeconds 90
        { Assert-TimeoutNesting } | Should -Throw -ExpectedMessage '*SIGKILL*'
    }
}

Describe 'Deploy-WwQueueProcessor — Resolve-SourceFile' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'resolves {sourceId}.bite directly' {
        $path = Resolve-SourceFile -SourceId '0b142714-8f6d-41b7-9832-2aefa8c731ec' -SearchPath $script:FixtureDir
        (Split-Path $path -Leaf) | Should -Be '0b142714-8f6d-41b7-9832-2aefa8c731ec.bite'
    }

    It 'falls back to matching the ID attribute for an operator-named file' {
        $dir = Join-Path $script:FixtureDir 'named'
        New-Item -ItemType Directory -Force $dir | Out-Null
        Set-Content (Join-Path $dir 'My RabbitMQ Source.bite') `
            '<Source ID="0b142714-8f6d-41b7-9832-2aefa8c731ec" ConnectionString="HostName=h;Port=1;UserName=u;Password=p;VirtualHost=/" />' -Encoding UTF8

        $path = Resolve-SourceFile -SourceId '0b142714-8f6d-41b7-9832-2aefa8c731ec' -SearchPath $dir
        (Split-Path $path -Leaf) | Should -Be 'My RabbitMQ Source.bite'
    }

    It 'throws an actionable error when the source cannot be found' {
        { Resolve-SourceFile -SourceId ([Guid]::NewGuid()) -SearchPath $script:FixtureDir } |
            Should -Throw -ExpectedMessage '*was not found*'
    }
}

Describe 'Deploy-WwQueueProcessor — New-StagingDirectory' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'creates a FRESH temp dir with a Settings folder, never touching the publish output' {
        $a = New-StagingDirectory -AppName 'wwqp-a'
        $b = New-StagingDirectory -AppName 'wwqp-a'

        try {
            $a | Should -Not -Be $b -Because 'each trigger stages into its own directory'
            $a | Should -Match 'wwqueueprocessor-stage-wwqp-a'
            (Test-Path (Join-Path $a 'Settings')) | Should -BeTrue
            $a.StartsWith([System.IO.Path]::GetTempPath()) | Should -BeTrue
        }
        finally {
            Remove-Item $a, $b -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'creates BOTH the triggers and sources sub-folders the worker expects' {
        # The layout is the contract with QueueProcessorOptions: Settings\triggers\{id}.bite and
        # Settings\sources\{id}.bite. Both are created even when only one is populated, so a
        # baked image fails with a clear "no trigger files" error rather than a missing-directory
        # exception at cold start.
        $dir = New-StagingDirectory -AppName 'wwqp-layout'
        try {
            (Test-Path (Join-Path $dir 'Settings\triggers')) | Should -BeTrue
            (Test-Path (Join-Path $dir 'Settings\sources'))  | Should -BeTrue
        }
        finally {
            Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Deploy-WwQueueProcessor — staged layout and tenant wiring' {

    BeforeAll { $script:QpText = Get-Content -LiteralPath $script:DeployScript -Raw }

    It 'stages sources into Settings\sources, not the Settings root' {
        # A source in the Settings root only resolves through the legacy fallback; new
        # deployments must write the dedicated folder so a source can never be mistaken for a
        # trigger during discovery.
        # -BeLike, not -Match: the path literal contains a backslash, and escaping it through both
        # PowerShell string parsing and the regex engine is error-prone.
        $script:QpText | Should -BeLike "*Join-Path `$ctx 'Settings\sources'*"
        $script:QpText | Should -Match 'Join-Path \$sourcesDir "\$\(\$t\.QueueSourceId\)\.bite"'
    }

    It 'stages the dead-letter sink source too when it differs from the work source' {
        # DeadLetterOptions can point at a DIFFERENT broker; if that source is not staged the
        # replica starts and then cannot dead-letter, losing the failure path only.
        $script:QpText | Should -Match 'Join-Path \$sourcesDir "\$\(\$t\.QueueSinkId\)\.bite"'
    }

    It 'bakes the staged Settings into the BUILD CONTEXT, and builds from that context' {
        # THE defect this replaced: settings were staged into a per-app temp dir, encrypted,
        # verified - and then deleted, without ever entering the build context. The image shipped
        # with empty Settings folders and every container exited 2 with
        #   "No trigger files matching '*.bite' were found in '/app/Settings/triggers'"
        # Configuration verification could not catch it; only a running replica could.
        $script:QpText | Should -Match 'function New-ImageBuildContext'
        # The context, not the repo root, is what az builds.
        $script:QpText | Should -Match "'--file', \`$dockerfile, \`$buildContext"
        $script:QpText | Should -Not -Match "'--file', \`$dockerfile, \`$contextRoot"
    }

    It 'stages EVERY planned trigger into the one shared image' {
        # One image serves all apps; each selects its trigger via QUEUE__TRIGGERID. Staging only
        # the current trigger would leave the other apps unable to find theirs.
        $script:QpText | Should -Match 'foreach \(\$p in \$Planned\)'
        $script:QpText | Should -Match 'QUEUE__TRIGGERID='
    }

    It 'refuses to build when the staged trigger count does not match the plan' {
        # Cheap guard for the exact failure above: an image that cannot start must never be pushed.
        $script:QpText | Should -Match '\$stagedTriggers\.Count -ne \$Planned\.Count'
        $script:QpText | Should -Match 'Refusing to build'
    }

    It 'refuses a publish folder with no entry DLL' {
        # The image runs 'dotnet Warewolf.Execution.QueueProcessor.dll'. The deploy machine is
        # WINDOWS, so its publish contains the .exe apphost and NOT the extensionless Linux apphost -
        # an ENTRYPOINT of "./Warewolf.Execution.QueueProcessor" fails with "no such file or
        # directory". The .dll is present in any framework-dependent publish, so the muxer entrypoint
        # is OS-agnostic; this guard catches a publish folder that has neither.
        $script:QpText | Should -Match 'Warewolf\.Execution\.QueueProcessor\.dll'
        $script:QpText | Should -Match 'would.*start and immediately fail|Publish the QueueProcessor'
    }

    It 'never mutates the operator''s publish output' {
        # -PublishPath is the operator's artefact. Staging copies FROM it into a temp context; it is
        # never written to, and the context is always removed afterwards.
        # -Path (wildcard-expanding), not -LiteralPath, which would fail on the trailing '*'.
        $script:QpText | Should -Match 'Copy-Item -Path \(Join-Path \$PublishRoot ''\*''\) -Destination \$ctx'
        $script:QpText | Should -Match 'Remove-Item -LiteralPath \$buildContext -Recurse -Force'
    }

    It 'tells the worker where the sources are via QUEUE__SOURCESSUBPATH' {
        $script:QpText | Should -Match 'QUEUE__SOURCESSUBPATH=sources'
        $script:QpText | Should -Match 'QUEUE__TRIGGERSSUBPATH=triggers'
    }

    It 'defaults EngineTenantId from the az context and then requires it' {
        # Blank is legal ONLY for a system-assigned managed identity. Leaving it unset for any
        # other credential fails at the first message with 'Invalid tenant id provided', which
        # reads like a missing app role rather than missing configuration.
        $script:QpText | Should -Match 'az account show --query tenantId'
        $script:QpText | Should -Match "Read-Required -Name 'EngineTenantId'"
    }

    It 'resolves every referenced source at PLAN time, before any app is created' {
        # A source missing for trigger #3 must not surface after apps for #1 and #2 already exist.
        # The plan resolves all of them and the deploy loop only consumes $sourcePlan.
        $script:QpText | Should -Match '\$sourcePlan\s*=\s*@\{\}'
        $script:QpText | Should -Match 'Unresolved RabbitMQ source\(s\); nothing was deployed'
        # The staging step must READ the plan, not re-resolve.
        $script:QpText | Should -Match '\$SourcePlan\[\$t\.QueueSourceId\]'
        $script:QpText | Should -Match '\$SourcePlan\[\$t\.QueueSinkId\]'
    }

    It 'encrypts triggers WHOLE-FILE and sources by ATTRIBUTE' {
        # Attribute mode has no <Source> element to find in a JSON trigger, so it SILENTLY SKIPS
        # it - leaving the trigger (including its stored UserName/Password) plaintext in the image
        # layer, and leaving a whole-file DPAPI trigger unreadable on Linux.
        $script:QpText | Should -Match "-ieq\s+'triggers'"
        $script:QpText | Should -Match "encArgs\['WholeFile'\]\s*=\s*\`$true"
    }

    It 'verifies every staged file round-trips before building the image' {
        # A file that encrypts but does not decrypt is a cold-start failure; catch it while the
        # image can still not be built.
        $script:QpText | Should -Match '-VerifyOnly'
        $script:QpText | Should -Match 'refusing to build the image'
    }

    It 'grants the app identity AcrPull so a NEW registry can be pulled from' {
        # --registry-identity system means ACA pulls with the MI, which needs AcrPull. A long-lived
        # ACR may already have broad grants that hide this; a newly created one never does, and the
        # symptom (revision never healthy, image-pull failure) reads like a bad build.
        # Asserted through the verifying helper, which is the only path that grants roles.
        $script:QpText | Should -Match "-Role 'AcrPull'"
        $script:QpText | Should -Match '-Scope "\$acrId"\.Trim\(\)'
    }

    It 'grants the identity roles BEFORE setting the keyvaultref secret' {
        # Ordering is load-bearing and gets it wrong LATE. 'containerapp secret set' with a
        # keyvaultref makes ACA resolve the secret immediately using the app identity, and rejects
        # the whole revision if it cannot:
        #   Field 'configuration.secrets' is invalid ... Unable to get value using Managed identity
        #   system for secret rabbitmq-connection. Error: unable to fetch secret
        # The grants used to run at the END of the per-trigger block, so this only ever surfaced on
        # a real run - after the image was built, pushed AND the Container App created.
        $grantIdx  = $script:QpText.IndexOf('Grant-AppIdentityRoles -PrincipalId')
        $secretIdx = $script:QpText.IndexOf("'containerapp', 'secret', 'set'")

        $grantIdx  | Should -BeGreaterThan 0 -Because 'the grant helper must be invoked in the deploy loop'
        $secretIdx | Should -BeGreaterThan 0 -Because 'the keyvaultref secret must be set'
        $grantIdx  | Should -BeLessThan $secretIdx -Because 'the identity must be able to read the secret before ACA resolves it'
    }

    It 'ensures the registry is wired on the UPDATE path, not only on create' {
        # --registry-server/--registry-identity are create-only flags. An app whose create was
        # interrupted - or one created via -Image, which skips those flags - has NO
        # configuration.registries entry, so ACA pulls anonymously and every image update fails
        #   UNAUTHORIZED: authentication required ... scope=repository:<repo>:pull
        # even though the identity holds AcrPull. Verified live: the working app had
        # server=<acr>.azurecr.io identity=system, the broken one had no registry at all, and no
        # amount of re-running could repair it.
        $script:QpText | Should -Match "'containerapp', 'registry', 'set'"
        $script:QpText | Should -Match "'--identity', 'system'"

        $regIdx    = $script:QpText.IndexOf("'containerapp', 'registry', 'set'")
        $commonIdx = $script:QpText.IndexOf('$common = @(')
        $regIdx | Should -BeLessThan $commonIdx -Because 'the registry must be wired before the image-bearing update'
    }

    It 'grants an EXISTING app''s identity before touching its image' {
        # 'containerapp update --image' validates the registry pull SYNCHRONOUSLY and rejects the
        # revision when the identity lacks AcrPull:
        #   Field 'template.containers.<app>.image' is invalid ... UNAUTHORIZED: authentication
        #   required ... scope=repository:<repo>:pull
        # Without this pre-grant, an app left half-deployed by an interrupted run (identity present,
        # never granted) could NEVER be repaired by re-running - the update failed before the grant.
        # Hit for real after a transient connection reset killed a run mid-create.
        $preGrantIdx = $script:QpText.IndexOf('if ($existingPrincipalId) { Grant-AppIdentityRoles')
        $commonIdx   = $script:QpText.IndexOf('$common = @(')

        $preGrantIdx | Should -BeGreaterThan 0 -Because 'an existing app must be granted before its image is set'
        $commonIdx   | Should -BeGreaterThan 0
        $preGrantIdx | Should -BeLessThan $commonIdx -Because 'the grant must precede the image-bearing update'
    }

    It 'offers an inline secret mode for CAE-enforcing tenants' {
        # A runtime keyvaultref makes the ACA control plane fetch the secret with the app identity. In
        # a tenant that enforces Continuous Access Evaluation that fetch fails on every sync with
        #   401 AKV10203 ... CaeAuthorizationFailed
        # because ACA's sync path cannot answer the claims challenge - so KEDA never reads the queue
        # depth and replicas stay at 0. Observed live on BOTH apps. The worker's own Key Vault access
        # is unaffected: the Azure SDK inside the container does implement the exchange.
        $script:QpText | Should -Match '\[switch\] \$InlineRabbitMqSecret'
        $script:QpText | Should -Match 'CaeAuthorizationFailed'
        # Reads from Key Vault with the OPERATOR's credentials, then sets a plain secret.
        $script:QpText | Should -Match "'keyvault', 'secret', 'show', '--vault-name'"
        $script:QpText | Should -Match 'rabbitmq-connection=\$\('
        # And it must fail loudly rather than deploy an app whose scaler cannot authenticate.
        $script:QpText | Should -Match 'Without the broker URI the KEDA rule cannot authenticate'
    }

    It 'never echoes or throws a secret value' {
        # Invoke-Az echoes the command under -DryRun and embeds it in every throw. Passing
        # '--secrets rabbitmq-connection=<amqp uri with password>' through that unredacted would put
        # live broker credentials into the console and into failure messages.
        $script:QpText | Should -Match '\[switch\] \$Sensitive'
        $script:QpText | Should -Match 'REDACTED'
        $script:QpText | Should -Match '\[DRYRUN\] az \$display'
        $script:QpText | Should -Match 'throw "az \$display failed'
        $script:QpText | Should -Not -Match 'throw "az \$\(\$AzArgs -join'
        # The inline secret call is the one that must opt in.
        $script:QpText | Should -Match "-Mutating -Sensitive"
    }

    It 'retries transport failures but not genuine errors' {
        # ConnectionResetError(10054) killed three deploys mid-flight against this subscription,
        # each time leaving a half-configured Container App. Every az call here is idempotent, so
        # replaying is safe. The pattern must stay NARROW: -AllowFail probes detect "does not exist"
        # via ResourceNotFound, and if that matched the transient set every probe would stall for
        # ~30s before returning $null.
        $script:QpText | Should -Match 'ConnectionResetError'
        $script:QpText | Should -Match '\$transient'
        $script:QpText | Should -Match 'transient transport error'
        $script:QpText | Should -Not -Match "transient\s*=\s*'.*ResourceNotFound"
    }

    It 'keeps az stderr out of returned values' {
        # `az` writes warnings to stderr and EVERY `az containerapp ...` call emits
        #   "WARNING: The behavior of this command has been altered by the following extension:
        #    containerapp"
        # A naive '2>&1' capture makes a single-value read like
        #   containerapp show --query identity.principalId -o tsv
        # return TWO elements, so "$result".Trim() yields 'WARNING: ... <guid>'. That mangled string
        # was passed as --assignee-object-id: the role assignment silently failed and the deploy died
        # two steps later on an unresolvable Key Vault secret. Reproduced live before fixing.
        $script:QpText | Should -Match '\$stdout\s*=\s*@\(\$raw \| Where-Object'
        $script:QpText | Should -Match 'ErrorRecord'
        # The failure path must still surface the stderr text, or diagnosing a real az error is blind.
        $script:QpText | Should -Match '\$text = \(\$raw \| ForEach-Object'
        $script:QpText | Should -Match 'failed: \$text'
    }

    It 'reads every role assignment back instead of trusting the create call' {
        # The create runs with -AllowFail, which returns $null on error. The old code printed
        # "Granted ..." unconditionally straight afterwards, so a transient failure produced a log
        # claiming success while no assignment existed - and the real symptom appeared much later as
        # ACA refusing to resolve the keyvaultref secret. Verified live: the app identity held only
        # AcrPull while the log claimed both roles.
        $script:QpText | Should -Match 'function Assert-RoleAssigned'
        $script:QpText | Should -Match "'role', 'assignment', 'list'"
        $script:QpText | Should -Match "roleDefinitionName=='\`$Role'"
        $script:QpText | Should -Match '\(verified\)'
        # And it must be fatal, not advisory - continuing produces a confusing downstream failure.
        $script:QpText | Should -Match 'Could not grant .*after \$MaxAttempts attempts'
        # Both grants must go through the verifying helper.
        ([regex]::Matches($script:QpText, 'Assert-RoleAssigned -PrincipalId')).Count |
            Should -Be 2 -Because 'AcrPull and Key Vault Secrets User must both be verified'
    }

    It 'retries the keyvaultref secret so RBAC propagation cannot fail the deploy' {
        # Azure RBAC is eventually consistent, so even in the correct order the first attempt can
        # lose a race with propagation. A retry is used rather than a blind sleep so the common
        # case stays fast.
        $script:QpText | Should -Match 'maxAttempts'
        $script:QpText | Should -Match 'RBAC grant to propagate'
    }

    It 'grants the app identity read access to the KEDA secret vault' {
        # Without this the app deploys Healthy and never leaves 0 replicas, which looks
        # identical to "no messages".
        $script:QpText | Should -Match "-Role 'Key Vault Secrets User'"
        $script:QpText | Should -Match "'--assignee-principal-type', 'ServicePrincipal'"
        # Scoped to the single vault, never the subscription.
        $script:QpText | Should -Match '-Scope "\$vaultId"\.Trim\(\)'
    }

    It 'does not shadow the automatic $pid variable' {
        # $pid is PowerShell's process id; assigning it inside a scope that later calls a
        # function is a subtle footgun.
        $script:QpText | Should -Not -Match '\$pid\s*='
    }

    It 'advises when Prefetch exceeds MaxConcurrency instead of silently inflating the KEDA target' {
        # value = Prefetch x MaxConcurrency, so an oversized prefetch DELAYS scale-out and leaves
        # more buffered messages to nack on drain. Advisory, not fatal - the trigger stays the
        # single source of truth.
        $script:QpText | Should -Match 'Prefetch == MaxConcurrency'
    }
}

Describe 'Deploy-WwQueueProcessor — DryRun end-to-end' {

    BeforeAll {
        # Shadow az with a tracking function (functions win over applications), so the suite
        # runs identically with or without the Azure CLI.
        $global:azCalls = @()
        function global:az {
            $global:azCalls += ,($args -join ' ')
            $global:LASTEXITCODE = 0
            # The script defaults EngineTenantId from the logged-in context, so the stub must
            # answer that read - otherwise these tests would only ever cover the explicit
            # -EngineTenantId path and the defaulting branch would go untested.
            if (($args -join ' ') -match 'account show.*tenantId') { return 'tid-from-az-context' }
            return ''
        }
    }

    AfterAll { Remove-Item Function:\az -ErrorAction SilentlyContinue }

    It 'resolves one trigger, derives the plan, and performs NO mutating az call' {
        $global:azCalls = @()

        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $out | Should -Match 'Triggers resolved\s+:\s+1'
        $out | Should -Match 'wwqp-mandatecollectionsucce'
        # value=1 (MaxConcurrency), NOT 10 (Prefetch): the backlog divisor must be what a replica
        # can execute, so a 5-message burst uses all 5 replicas instead of batching onto one.
        $out | Should -Match 'max=5 min=0 value=1 prefetch=10'
        $out | Should -Match '\[DRYRUN\] az containerapp create'
        $out | Should -Match 'KEDA rule set: ceil\(queueLength / 1\) capped at 5'

        # Every state-changing call must be echoed, not executed.
        ($global:azCalls | Where-Object { $_ -match 'containerapp create|containerapp update|group create' }).Count |
            Should -Be 0 -Because '-DryRun must not mutate anything'
    }

    It 'passes env vars ON CREATE so the first revision is not born broken' {
        # QueueProcessorOptions is validated with DataAnnotations at startup, so a revision created
        # before --set-env-vars runs dies with
        #   FATAL: OptionsValidationException ... 'Engine:ResourceAppId is required'
        # Observed on every deploy: revision --0000001 crash-looped until the follow-up update
        # produced a healthy revision. The end state was fine, which is exactly why it went unnoticed -
        # the logs carried a fatal error indistinguishable from a real misconfiguration.
        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $create = ($out -split "`n" | Where-Object { $_ -match 'az containerapp create' }) -join ' '
        $create | Should -Match '--env-vars'
        $create | Should -Match 'ENGINE__RESOURCEAPPID=app-1' -Because 'the missing value that killed revision 1 must be present at create'
        $create | Should -Match 'QUEUE__TRIGGERID='
    }

    It 'creates the app with NO --ingress flag at all' {
        # A queue worker takes no inbound HTTP, and ingress is off by default when the flag is
        # omitted. Regression lock: the script used to pass '--ingress disabled', which the CLI
        # rejects outright ("'disabled' is not a valid value for '--ingress'. Allowed values:
        # internal, external") - it failed the real create AFTER the image had been built and
        # pushed, so it cost a full build cycle to discover.
        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $create = ($out -split "`n" | Where-Object { $_ -match 'az containerapp create' }) -join ' '
        $create | Should -Not -BeNullOrEmpty
        $create | Should -Not -Match '--ingress' -Because 'omitting the flag is the only correct way to have no ingress'
        $create | Should -Match '--system-assigned' -Because 'the MI is required for ACR pull and Key Vault'
    }

    It 'fans out over a folder, one Container App per trigger' {
        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerPath $script:FixtureDir -TriggerFilter 'triggers-mandate.bite' `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $out | Should -Match 'Triggers resolved\s+:\s+1'
    }

    It 'throws when the trigger filter matches nothing (never a silent no-op)' {
        {
            & $script:DeployScript `
                -ResourceGroup RG -Location southafricanorth `
                -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
                -TriggerPath $script:FixtureDir -TriggerFilter 'nothing-here*.bite' `
                -QueueSourcePath $script:FixtureDir `
                -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
                -DryRun -NonInteractive
        } | Should -Throw -ExpectedMessage '*Refusing to deploy nothing*'
    }

    It 'aborts the whole run when a referenced source is not staged, creating nothing' {
        # Point -QueueSourcePath at a folder with NO source .bite. The failure must come from the
        # plan, with no containerapp call attempted - a partially deployed fleet is far worse than
        # a clean refusal.
        $empty = Join-Path ([System.IO.Path]::GetTempPath()) ("wwqp-nosrc-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Force -Path $empty | Out-Null
        $global:azCalls = @()
        try {
            {
                & $script:DeployScript `
                    -ResourceGroup RG -Location southafricanorth `
                    -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
                    -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
                    -QueueSourcePath $empty `
                    -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
                    -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
                    -DryRun -NonInteractive
            } | Should -Throw -ExpectedMessage '*Unresolved RabbitMQ source*'

            ($global:azCalls | Where-Object { $_ -match 'containerapp' }).Count |
                Should -Be 0 -Because 'the plan must fail before any Container App work begins'
        }
        finally {
            Remove-Item $empty -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'lists the sources it will stage, with the triggers that reference each' {
        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $out | Should -Match 'Sources to stage\s+:\s+1'
        $out | Should -Match '0b142714-8f6d-41b7-9832-2aefa8c731ec'
        $out | Should -Match 'MandateCollectionSuccessTrigger \(queue\)'
    }

    It 'rejects mutually exclusive trigger-pointing modes' {
        {
            & $script:DeployScript `
                -ResourceGroup RG -Location southafricanorth `
                -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
                -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
                -TriggerPath $script:FixtureDir `
                -QueueSourcePath $script:FixtureDir `
                -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
                -DryRun -NonInteractive
        } | Should -Throw -ExpectedMessage '*mutually exclusive*'
    }

    It 'fails loudly on an unsubstituted release token before creating anything' {
        {
            & $script:DeployScript `
                -ResourceGroup RG -Location southafricanorth `
                -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
                -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-token.bite') `
                -QueueSourcePath $script:FixtureDir `
                -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
                -DryRun -NonInteractive
        } | Should -Throw -ExpectedMessage '*unsubstituted release token*'
    }

    It 'fails loudly when a required value is missing under -NonInteractive' {
        {
            & $script:DeployScript -Location southafricanorth -AcaEnvironment aca-test `
                -Image 'acr.azurecr.io/wwqp@sha256:abc' `
                -TriggerFilePath (Join-Path $script:FixtureDir 'triggers-mandate.bite') `
                -QueueSourcePath $script:FixtureDir `
                -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
                -DryRun -NonInteractive
        } | Should -Throw -ExpectedMessage "*Required value 'ResourceGroup'*"
    }

    It 'deploys a Concurrency = 0 trigger as a disabled app with no scale rule' {
        $out = & $script:DeployScript `
            -ResourceGroup RG -Location southafricanorth `
            -AcaEnvironment aca-test -Image 'acr.azurecr.io/wwqp@sha256:abc' `
            -TriggerFilePath (Join-Path $script:FixtureDir 'other-disabled.bite') `
            -QueueSourcePath $script:FixtureDir `
            -EngineBaseUrl 'https://engine' -EngineResourceAppId 'app-1' `
            -RabbitMqSecretUri 'https://kv.vault.azure.net/secrets/rabbit' `
            -DryRun -NonInteractive 6>&1 | Out-String

        $out | Should -Match 'DISABLED: Concurrency = 0'
        $out | Should -Not -Match 'KEDA rule set'
    }
}
