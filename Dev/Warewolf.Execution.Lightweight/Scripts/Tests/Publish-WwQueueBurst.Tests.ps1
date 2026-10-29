#Requires -Version 7.0

<#
    Pester 5 test suite for Publish-WwQueueBurst.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Publish-WwQueueBurst.Tests.ps1

    Design notes (same conventions as Deploy-WwQueueProcessor.Tests.ps1)
    ---------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the manifest-building helpers with no broker connection and no
      filesystem writes, so the invariants the reconciliation depends on - unique transaction ids,
      distinct bodies, correct counts - are testable without CloudAMQP.
    * The invariants tested here are not cosmetic. A duplicate txn makes every downstream bucket
      meaningless, and identical bodies make duplicate EXECUTION undetectable in the database.
#>

BeforeAll {
    $script:PublishScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Publish-WwQueueBurst.ps1'
    . $script:PublishScript -LoadFunctionsOnly
}

Describe 'Get-WwBurstQueueSlug' {

    It 'takes the leading letters of the queue name, lower-cased' {
        Get-WwBurstQueueSlug -QueueName 'order-success-queue' | Should -Be 'orders'
    }

    It 'truncates to the requested length' {
        Get-WwBurstQueueSlug -QueueName 'abcdefghijkl' -Length 4 | Should -Be 'abcd'
    }

    It 'returns the whole thing when it is shorter than the length' {
        Get-WwBurstQueueSlug -QueueName 'ab' | Should -Be 'ab'
    }

    It 'falls back to a placeholder for a queue name with no letters' {
        # Substring(0,6) on the empty string throws. Discovering that at message 1 of a 100-message
        # run, after the pre-warm has already executed, is an expensive way to learn it.
        { Get-WwBurstQueueSlug -QueueName '123-456' } | Should -Not -Throw
        Get-WwBurstQueueSlug -QueueName '123-456' | Should -Be 'queue'
    }
}

Describe 'New-WwBurstTxn' {

    It 'encodes label, slug, kind and a zero-padded index' {
        New-WwBurstTxn -Label 'R2' -Slug 'orders' -Kind 'success' -Index 7 |
            Should -Match '^R2-orders-S-0007-[0-9a-f]{6}$'
    }

    It 'marks a failure message with F' {
        New-WwBurstTxn -Label 'R2' -Slug 'orders' -Kind 'failure' -Index 1 |
            Should -Match '^R2-orders-F-0001-'
    }

    It 'differs between calls with identical inputs' {
        # Without the random tail, re-running a burst under the same label reproduces the previous
        # run's transaction ids - and the report would then reconcile this run against stale log
        # lines and report a clean pass built entirely from the previous run's evidence.
        $a = New-WwBurstTxn -Label 'R2' -Slug 'orders' -Kind 'success' -Index 1
        $b = New-WwBurstTxn -Label 'R2' -Slug 'orders' -Kind 'success' -Index 1
        $a | Should -Not -Be $b
    }
}

Describe 'New-WwBurstBody' {

    It 'produces valid JSON carrying the caller-supplied MandateId' {
        # MandateId, not txn, is what the body carries: it is the DATABASE reconciliation key, read
        # back by Invoke-WwQueueLoadTest.ps1 as JSON_VALUE(MessageContent, '$.MandateId'). The txn
        # rides on the AMQP CorrelationId instead and reconciles the dead-letter queue.
        $o = New-WwBurstBody -MandateId 'MID-1' | ConvertFrom-Json
        $o.MandateId | Should -Be 'MID-1'
    }

    It 'defaults to the 19-field Mandate document' {
        $o = New-WwBurstBody -MandateId 'MID-1' | ConvertFrom-Json
        @($o.PSObject.Properties.Name).Count | Should -Be 19
        $o.PSObject.Properties.Name | Should -Contain 'ClearWinnerStrategy'
    }

    It 'produces the flat 16-field profiler request for -Shape Profiler' {
        # The body profiler.profile.request binds (ProfilerServices\ProfilerWrapper\
        # RequestForProfiling). One field per workflow input, flat - a nested document would not
        # bind. Pinning the exact field set is what stops an input being dropped silently, which
        # surfaces only as "Invalid Calculate" deep inside the workflow at run time.
        $o = New-WwBurstBody -MandateId 'MID-2' -Shape Profiler | ConvertFrom-Json
        $o.MandateId | Should -Be 'MID-2'
        @($o.PSObject.Properties.Name) | Should -Be @(
            'ForPaymentCycle', 'MandateCanTrack', 'MandateDRI', 'MandateId', 'RelatedMandates',
            'ResultDate', 'ResultDebitRunId', 'ResultMethod', 'ResultPaymentCycle',
            'ResultPresentedOnDate', 'ResultType', 'ScoreThisResult', 'IsFixedExtract',
            'SourcePaymentId', 'RequestType', 'FASGroup')
    }

    It 'pins ScoreThisResult true and leaves RequestType/FASGroup empty on the profiler shape' {
        # ScoreThisResult=false skips the scoring branch entirely (GetMandateDRIs IsEqual 'true'),
        # so a randomised value would quietly stop exercising most of the workflow. RequestType and
        # FASGroup are empty in every confirmed-good request sample.
        $o = New-WwBurstBody -MandateId 'MID-3' -Shape Profiler | ConvertFrom-Json
        $o.ScoreThisResult | Should -Be 'true'
        $o.RequestType     | Should -Be ''
        $o.FASGroup        | Should -Be ''
    }

    It 'gives the profiler shape a distinct MandateId per call, so database rows stay attributable' {
        $ids = 1..20 | ForEach-Object { (New-WwBurstBody -MandateId "MID-$_" -Shape Profiler | ConvertFrom-Json).MandateId }
        @($ids | Select-Object -Unique).Count | Should -Be 20
    }
}

Describe 'New-WwBurstManifest' {

    It 'produces SuccessCount + FailureCount entries' {
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 30 -FailureCount 5
        @($m).Count | Should -Be 35
        @($m | Where-Object kind -eq 'success').Count | Should -Be 30
        @($m | Where-Object kind -eq 'failure').Count | Should -Be 5
    }

    It 'gives every message a unique transaction id' {
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 100
        @($m | ForEach-Object txn | Select-Object -Unique).Count | Should -Be 100
    }

    It 'gives every success message distinct content' {
        # Identical bodies would make duplicate EXECUTION invisible in the database: two rows with
        # the same content are indistinguishable from one message processed once.
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 100
        @($m | Where-Object kind -eq 'success' | ForEach-Object body | Select-Object -Unique).Count | Should -Be 100
    }

    It 'gives failure messages an empty body and zero bytes' {
        # The empty body IS the failure mechanism: MapEntireMessage + EmptyIsNull makes [[message]]
        # null and usp_jobs1_LogStart raises. A non-empty "failure" body would quietly succeed.
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 0 -FailureCount 3
        foreach ($f in $m) {
            $f.body  | Should -BeNullOrEmpty
            $f.bytes | Should -Be 0
        }
    }

    It 'records the byte count of each success body' {
        $m = New-WwBurstManifest -QueueName 'q-orders' -Label 'R2' -SuccessCount 1
        $m[0].bytes | Should -Be $m[0].body.Length
    }

    It 'returns an empty collection when both counts are zero' {
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 0 -FailureCount 0
        @($m).Count | Should -Be 0
    }

    It 'stamps the queue name on every entry' {
        $m = New-WwBurstManifest -QueueName 'order-success-queue' -Label 'R2' -SuccessCount 2 -FailureCount 2
        @($m | Where-Object { $_.queue -ne 'order-success-queue' }).Count | Should -Be 0
    }
}

Describe 'Read-WwBurstDataFile' {

    BeforeAll {
        function script:New-DataFile {
            param([string] $Json)
            $p = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.json')
            Set-Content -LiteralPath $p -Value $Json
            $p
        }
    }

    It 'returns one compact body per record, in file order, with the field values exactly as written' {
        $p = New-DataFile '[{"MandateId":"B","ForPaymentCycle":"202610"},{"MandateId":"A","ForPaymentCycle":"202611"}]'
        $r = @(Read-WwBurstDataFile -Path $p)
        $r.Count | Should -Be 2
        @($r.MandateId) | Should -Be @('B', 'A')
        $r[0].Body | Should -Be '{"MandateId":"B","ForPaymentCycle":"202610"}'
    }

    It 'keeps empty values - an empty RelatedMandates is real data, not a missing field' {
        $r = @(Read-WwBurstDataFile -Path (New-DataFile '[{"MandateId":"A","RelatedMandates":"","FASGroup":""}]'))
        ($r[0].Body | ConvertFrom-Json).PSObject.Properties.Name | Should -Contain 'RelatedMandates'
        ($r[0].Body | ConvertFrom-Json).RelatedMandates | Should -Be ''
    }

    It 'returns a single record for a one-element array' {
        @(Read-WwBurstDataFile -Path (New-DataFile '[{"MandateId":"A"}]')).Count | Should -Be 1
    }

    It 'gives a record with no MandateId a null key rather than failing' {
        (@(Read-WwBurstDataFile -Path (New-DataFile '[{"ForPaymentCycle":"202610"}]')))[0].MandateId | Should -BeNullOrEmpty
    }

    It 'throws for a missing file' {
        { Read-WwBurstDataFile -Path (Join-Path $TestDrive 'nope.json') } | Should -Throw '*not found*'
    }

    It 'throws for an empty array' {
        { Read-WwBurstDataFile -Path (New-DataFile '[]') } | Should -Throw '*contains no records*'
    }

    It 'throws naming the record when an element is not a JSON object' {
        { Read-WwBurstDataFile -Path (New-DataFile '[{"MandateId":"A"},"oops"]') } | Should -Throw '*Record 2*not a JSON object*'
    }
}

Describe 'New-WwBurstManifest with -Records (data file)' {

    BeforeAll {
        $script:Records = @(
            [pscustomobject]@{ MandateId = 'M1'; Body = '{"MandateId":"M1"}' }
            [pscustomobject]@{ MandateId = 'M2'; Body = '{"MandateId":"M2"}' }
            [pscustomobject]@{ MandateId = 'M3'; Body = '{"MandateId":"M3"}' }
        )
    }

    It 'publishes exactly one success message per record, carrying the record body verbatim' {
        $m = New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -Records $script:Records
        @($m).Count | Should -Be 3
        @($m.body) | Should -Be @('{"MandateId":"M1"}', '{"MandateId":"M2"}', '{"MandateId":"M3"}')
        @($m | Where-Object kind -ne 'success').Count | Should -Be 0
    }

    It 'keys each entry by its record''s own MandateId, not a generated one' {
        $m = New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -Records $script:Records
        @($m.mandateId) | Should -Be @('M1', 'M2', 'M3')
    }

    It 'ignores -SuccessCount and -BodyShape when records are supplied' {
        $m = New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -SuccessCount 50 -BodyShape Profiler -Records $script:Records
        @($m).Count | Should -Be 3
    }

    It 'still gives every message a unique transaction id' {
        $m = New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -Records $script:Records
        @($m.txn | Select-Object -Unique).Count | Should -Be 3
    }

    It 'records each body''s byte count' {
        $m = New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -Records $script:Records
        foreach ($e in $m) { $e.bytes | Should -Be $e.body.Length }
    }

    It 'does not reject repeated MandateIds - the data file is the caller''s own key set (the pipeline checks it separately)' {
        $dup = @($script:Records[0], $script:Records[0])
        { New-WwBurstManifest -QueueName 'profiler-queue' -Label 'L' -Records $dup } | Should -Not -Throw
    }
}

Describe 'Publish-WwQueueBurst.ps1 -DataFilePath guards' {

    It 'fails on a missing data file before any broker connection is attempted' {
        { & $script:PublishScript -QueueName 'q' -Label 'L' -ManifestPath 'x.json' -AmqpUri 'amqp://u:p@localhost:1/' `
              -DataFilePath (Join-Path $TestDrive 'absent.json') } | Should -Throw '*not found*'
    }

    It 'does not treat -SuccessCount 0 as nothing to publish when a data file supplies the records' {
        $p = Join-Path $TestDrive 'one.json'
        Set-Content -LiteralPath $p -Value '[{"MandateId":"A"}]'
        # Past the "Nothing to publish" guard the script tries the (unreachable) broker; any error
        # other than that guard proves the records were counted.
        $err = try { & $script:PublishScript -QueueName 'q' -Label 'L' -ManifestPath (Join-Path $TestDrive 'm.json') `
                         -AmqpUri 'amqp://u:p@127.0.0.1:1/' -SuccessCount 0 -DataFilePath $p -PublishPath $TestDrive; $null } catch { $_.Exception.Message }
        $err | Should -Not -BeLike '*Nothing to publish*'
    }
}

Describe 'Publish-WwQueueBurst.ps1 parameter guards' {

    It 'rejects a run with nothing to publish' {
        { & $script:PublishScript -QueueName 'q' -Label 'L' -ManifestPath 'x.json' `
              -SuccessCount 0 -FailureCount 0 -AmqpUri 'amqp://u:p@localhost:5672/' } |
            Should -Throw '*Nothing to publish*'
    }

    It 'requires a queue name' {
        { & $script:PublishScript -Label 'L' -ManifestPath 'x.json' -AmqpUri 'amqp://u:p@localhost:5672/' } |
            Should -Throw '*-QueueName is required*'
    }

    It 'requires a manifest path' {
        { & $script:PublishScript -QueueName 'q' -Label 'L' -AmqpUri 'amqp://u:p@localhost:5672/' } |
            Should -Throw '*-ManifestPath is required*'
    }

    It 'does not prompt when dot-sourced for helpers only' {
        # A [Parameter(Mandatory)] here would block an unattended Invoke-Pester on a console prompt.
        { . $script:PublishScript -LoadFunctionsOnly } | Should -Not -Throw
    }
}
