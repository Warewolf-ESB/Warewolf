#Requires -Version 7.0

<#
    Pester 5 test suite for Publish-WwProfilerExact.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Publish-WwProfilerExact.Tests.ps1

    Design notes (same conventions as Publish-WwQueueBurst.Tests.ps1)
    ---------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the pure helpers with no broker connection, no child process,
      and no filesystem writes beyond what a test itself performs into $TestDrive.
    * These two functions are the whole self-isolation mechanism: Get-WwProfilerExactChildArgs
      decides what the isolated child receives, and Get-WwProfilerExactChildResult decides whether
      the child's run counts as a success. Neither one ever touches RabbitMQ.Client, which is
      exactly why they - unlike the publish itself - can be exercised without a broker.
#>

BeforeAll {
    $script:PublishScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Publish-WwProfilerExact.ps1'
    . $script:PublishScript -LoadFunctionsOnly
}

Describe 'Format-WwProfilerExactBody' {

    # The engine's Service Bus trigger binds inputs ONLY from the {"inputs":{...}} envelope. A flat
    # body is rejected as Malformed by pre-WOLF-8517 builds (what dead-lettered 4998/5000 messages
    # on 2026-09-14), and on current builds executes against an EMPTY DATALIST - so the failure
    # looks like a broken workflow rather than an unbound payload. This is the regression guard.
    It 'wraps the fields in the inputs envelope, never emitting them flat' {
        $obj = (Format-WwProfilerExactBody -Fields ([ordered]@{ MandateDRI = '22'; MandateId = 'abc' })) | ConvertFrom-Json
        $obj.inputs.MandateDRI | Should -Be '22'
        $obj.inputs.MandateId  | Should -Be 'abc'
        $obj.PSObject.Properties.Name | Should -Be @('inputs')
    }

    It 'preserves every field of the fixture, including the empty ones' {
        $fields = [ordered]@{ ForPaymentCycle = '202402'; RequestType = ''; FASGroup = '' }
        $obj = (Format-WwProfilerExactBody -Fields $fields) | ConvertFrom-Json
        $obj.inputs.PSObject.Properties.Name | Should -Be @('ForPaymentCycle', 'RequestType', 'FASGroup')
        $obj.inputs.RequestType | Should -Be ''
    }
}

Describe 'Get-WwProfilerExactChildArgs' {

    It 'always starts with -NoProfile -File and the script path' {
        $args = Get-WwProfilerExactChildArgs -ScriptPath 'C:\scripts\Publish-WwProfilerExact.ps1' `
            -BoundParameters @{} -ResultPath 'C:\tmp\result.json'
        $args[0..2] | Should -Be @('-NoProfile', '-File', 'C:\scripts\Publish-WwProfilerExact.ps1')
    }

    It 'forwards an ordinary bound parameter as a -Name Value pair' {
        $args = Get-WwProfilerExactChildArgs -ScriptPath 's.ps1' -BoundParameters @{ Count = 5000 } -ResultPath 'r.json'
        $idx = [array]::IndexOf($args, '-Count')
        $idx | Should -BeGreaterThan -1
        $args[$idx + 1] | Should -Be '5000'
    }

    It 'forwards a present switch as a bare flag' {
        $args = Get-WwProfilerExactChildArgs -ScriptPath 's.ps1' -BoundParameters @{ Verbose = [switch]$true } -ResultPath 'r.json'
        ($args | Where-Object { $_ -eq '-Verbose' }).Count | Should -Be 1
    }

    It 'omits a bound switch that is not present' {
        $args = Get-WwProfilerExactChildArgs -ScriptPath 's.ps1' -BoundParameters @{ Verbose = [switch]$false } -ResultPath 'r.json'
        $args | Should -Not -Contain '-Verbose'
    }

    It 'never forwards AsChildProcess, ResultPath or LoadFunctionsOnly from the bound parameters' {
        # These three are launcher-only plumbing. If the caller's own bound parameters (however
        # that could happen) leaked through, a stale -ResultPath or an early -AsChildProcess ahead
        # of the launcher's own would point the child at the wrong file or double up the flag.
        $args = Get-WwProfilerExactChildArgs -ScriptPath 's.ps1' -ResultPath 'r.json' -BoundParameters @{
            AsChildProcess    = [switch]$true
            ResultPath        = 'whatever-the-caller-passed.json'
            LoadFunctionsOnly = [switch]$true
            Url               = 'https://example.com/'
        }
        $args | Should -Not -Contain '-LoadFunctionsOnly'
        ($args | Where-Object { $_ -eq '-AsChildProcess' }).Count | Should -Be 1
        ($args | Where-Object { $_ -eq '-ResultPath' }).Count | Should -Be 1
    }

    It 'always appends -AsChildProcess and -ResultPath with the given path at the end' {
        $args = Get-WwProfilerExactChildArgs -ScriptPath 's.ps1' -BoundParameters @{} -ResultPath 'C:\tmp\r-123.json'
        $args[-3] | Should -Be '-AsChildProcess'
        $args[-2] | Should -Be '-ResultPath'
        $args[-1] | Should -Be 'C:\tmp\r-123.json'
    }
}

Describe 'Get-WwProfilerExactChildResult' {

    It 'returns the parsed object when the result file exists' {
        # The child writes this file BEFORE attempting the teardown that reliably stack-overflows
        # it, so its presence - not the child's exit code - is what "the publish finished" means.
        $path = Join-Path $TestDrive 'result.json'
        [pscustomobject]@{ Queue = 'profiler.profile.request'; Total = 5000 } |
            ConvertTo-Json | Set-Content -LiteralPath $path -Encoding utf8

        $result = Get-WwProfilerExactChildResult -ResultPath $path
        $result.Queue | Should -Be 'profiler.profile.request'
        $result.Total | Should -Be 5000
    }

    It 'throws a clear, actionable error when the result file is missing' {
        # Missing means the child died BEFORE finishing the publish - a real failure, unlike the
        # expected post-publish teardown crash, and it must not be mistaken for one.
        $path = Join-Path $TestDrive 'never-written.json'
        { Get-WwProfilerExactChildResult -ResultPath $path } | Should -Throw '*crashed or was killed*'
    }
}

Describe 'Publish-WwProfilerExact.ps1 parameter guards' {

    It 'does not prompt when dot-sourced for helpers only' {
        # A [Parameter(Mandatory)] here would block an unattended Invoke-Pester on a console prompt.
        { . $script:PublishScript -LoadFunctionsOnly } | Should -Not -Throw
    }

    It 'requires a Url' {
        { & $script:PublishScript -QueueName 'q' -Label 'L' -ManifestPath 'x.json' -ScriptDir $PSScriptRoot } |
            Should -Throw '*-Url is required*'
    }

    It 'requires a QueueName' {
        { & $script:PublishScript -Url 'https://example.com/?MandateId=1' -Label 'L' -ManifestPath 'x.json' -ScriptDir $PSScriptRoot } |
            Should -Throw '*-QueueName is required*'
    }

    It 'requires a ManifestPath' {
        { & $script:PublishScript -Url 'https://example.com/?MandateId=1' -QueueName 'q' -Label 'L' -ScriptDir $PSScriptRoot } |
            Should -Throw '*-ManifestPath is required*'
    }

    It 'requires -ResultPath when -AsChildProcess is set' {
        # Exercises the worker branch's own guard directly - no broker or child process needed,
        # since this throw happens before Import-Module or any RabbitMQ.Client use.
        { & $script:PublishScript -Url 'https://example.com/?MandateId=1' -QueueName 'q' -Label 'L' `
              -ManifestPath 'x.json' -ScriptDir $PSScriptRoot -AsChildProcess } |
            Should -Throw '*-ResultPath is required*'
    }
}

Describe 'Publish-WwProfilerExact.ps1 topology and proof plumbing' {

    It 'forwards -SkipTopologyEnsure to the child process' {
        # The child does the publishing, so an opt-out that stayed in the launcher would silently
        # ensure the topology anyway.
        $bound = @{
            Url = 'https://e/w.json?MandateId=1'; QueueName = 'q'; Label = 'L'
            SkipTopologyEnsure = [switch]$true
        }
        $args = Get-WwProfilerExactChildArgs -ScriptPath 'C:\s.ps1' -BoundParameters $bound -ResultPath 'C:\r.json'
        $args | Should -Contain '-SkipTopologyEnsure'
    }

    It 'passes -RabbitMqManagementUri through the generic walk, which the child ignores' {
        # The PROOF itself is taken in the LAUNCHER, not in the child: the child writes its result
        # file BEFORE tearing the connection down (it is expected to crash there), and the counter
        # must be read only after that teardown has flushed the client's buffered publishes. The
        # child receiving the value is harmless - it simply never uses it.
        $bound = @{
            Url = 'https://e/w.json?MandateId=1'; QueueName = 'q'; Label = 'L'
            RabbitMqManagementUri = 'https://rabbitmq.warewolf.online'
        }
        $args = Get-WwProfilerExactChildArgs -ScriptPath 'C:\s.ps1' -BoundParameters $bound -ResultPath 'C:\r.json'
        # It is forwarded by the generic bound-parameter walk; the child simply ignores it.
        $args | Should -Contain '-RabbitMqManagementUri'
    }

    It 'still never forwards the internal plumbing switches' {
        $bound = @{
            Url = 'https://e/w.json?MandateId=1'; QueueName = 'q'; Label = 'L'
            SkipTopologyEnsure = [switch]$true
            AsChildProcess = [switch]$true
            LoadFunctionsOnly = [switch]$true
        }
        $args = Get-WwProfilerExactChildArgs -ScriptPath 'C:\s.ps1' -BoundParameters $bound -ResultPath 'C:\r.json'
        $args | Should -Not -Contain '-LoadFunctionsOnly'
        # -AsChildProcess is appended exactly once by the launcher itself, never copied from bound.
        (@($args | Where-Object { $_ -eq '-AsChildProcess' })).Count | Should -Be 1
    }
}
