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

    It 'produces valid JSON carrying the txn' {
        $body = New-WwBurstBody -Label 'R2' -QueueName 'order-success-queue' -Txn 'T-1' -Index 3
        $o = $body | ConvertFrom-Json
        $o.txn     | Should -Be 'T-1'
        $o.queue   | Should -Be 'order-success-queue'
        $o.orderId | Should -Be 'R2-S-0003'
        $o.amount  | Should -Be 103
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
