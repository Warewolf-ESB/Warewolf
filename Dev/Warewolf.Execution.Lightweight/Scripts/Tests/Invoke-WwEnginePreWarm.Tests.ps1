#Requires -Version 7.0

<#
    Pester 5 test suite for Invoke-WwEnginePreWarm.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Invoke-WwEnginePreWarm.Tests.ps1

    Design notes (same conventions as Monitor-RabbitMqShovel.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the pure helpers with no HTTP/network action.
    * No LIVE engine call is made anywhere in this suite. One test does drive the
      real Phase A/B code path, but only against the non-routable RFC 5735 address
      10.255.255.1, which cannot leave the host - see the 'non-fatal contract'
      Describe for why that regression has to run the script for real.
    * This is deliberately IN ADDITION TO, not a replacement for, manual
      validation against a real Consumption-plan engine (see
      docs/ShovelBridge-Architecture.md's 2026-08-16 entry for why this script
      exists).
#>

BeforeAll {
    $script:PreWarmScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Invoke-WwEnginePreWarm.ps1'
}

Describe 'Invoke-WwEnginePreWarm — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:PreWarmScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running Phase A/B' {
        $out = (. $script:PreWarmScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase A'
        $out | Should -Not -Match 'Phase B'
        Get-Command Get-WarmupUri              -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Format-WarmupRequestBody   -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Update-LatencyStreak       -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Test-WarmupRoundClean      -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command ConvertTo-WarmupResult     -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-WarmupConcurrencyLadder -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Format-WarmupCodeSummary   -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'requires -EngineBaseUrl and -AccessToken when not -LoadFunctionsOnly or -DryRun' {
        { & $script:PreWarmScript } | Should -Throw '*EngineBaseUrl*'
        $token = ConvertTo-SecureString 'dummy' -AsPlainText -Force
        { & $script:PreWarmScript -AccessToken $token } | Should -Throw '*EngineBaseUrl*'
        { & $script:PreWarmScript -EngineBaseUrl 'https://example.azurewebsites.net' } | Should -Throw '*AccessToken*'
    }

    It 'DryRun requires no live call and prints the phase plan, including the ramp ladder' {
        $token = ConvertTo-SecureString 'dummy' -AsPlainText -Force
        $out = (& $script:PreWarmScript -EngineBaseUrl 'https://example.azurewebsites.net' -AccessToken $token `
                    -TargetConcurrency 20 -ConcurrentRounds 3 -DryRun) 6>&1 | Out-String
        $out | Should -Match '\[DryRun\] Phase A'
        $out | Should -Match '\[DryRun\] Phase B'
        $out | Should -Match 'ramping concurrency 5 -> 10 -> 20'
    }
}

# ---------------------------------------------------------------------------
# Regression: the 2026-08-24 pipeline-LOADTEST failure.
#
# 'Warm up UAT engine before the load-test burst' exited 1 on an Invoke-WebRequest
# -TimeoutSec expiry inside Phase B, killing the load-test job before it started.
# -SkipHttpErrorCheck suppresses non-2xx STATUS CODES only; a timeout is a
# TaskCanceledException, so it bypassed that switch, was relayed out of the parallel
# runspace (which runs at 'Continue' - child runspaces do not inherit
# $ErrorActionPreference) and was turned terminating by the script's own 'Stop'.
#
# These two assert the pieces of the fix that are structural rather than behavioural,
# because a unit test cannot easily force the parallel-runspace path.
# ---------------------------------------------------------------------------
Describe 'Invoke-WwEnginePreWarm — timeout resilience (AST)' {
    BeforeAll {
        $script:Ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:PreWarmScript, [ref]$null, [ref]$null)

        $script:WebCalls = @($script:Ast.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'Invoke-WebRequest'
        }, $true))
    }

    It 'has an Invoke-WebRequest in each of Phase A and Phase B' {
        $script:WebCalls.Count | Should -Be 2
    }

    It 'wraps every Invoke-WebRequest in a try/catch so a timeout is a result, not an error record' {
        foreach ($call in $script:WebCalls) {
            $node    = $call
            $inTry   = $false
            while ($node) {
                if ($node -is [System.Management.Automation.Language.TryStatementAst]) { $inTry = $true; break }
                $node = $node.Parent
            }
            $inTry | Should -BeTrue -Because 'an unguarded Invoke-WebRequest timeout is exactly what failed the 2026-08-24 load-test build'
        }
    }

    It 'passes -ErrorAction Stop on every Invoke-WebRequest so the catch actually fires' {
        # Inside a ForEach-Object -Parallel runspace $ErrorActionPreference is 'Continue',
        # so without this the exception is non-terminating and skips the catch entirely.
        foreach ($call in $script:WebCalls) {
            $text = $call.Extent.Text
            $text | Should -Match '-ErrorAction\s+Stop'
        }
    }
}

# ---------------------------------------------------------------------------
# The non-fatal contract, asserted end to end.
#
# Scripts/README.md and pipeline-LOADTEST.yml both promise an incomplete warm-up only
# WARNS. Before the fix that promise was false for any transport-level failure. The
# only faithful way to assert it is to run the script for real and read its exit code,
# so this drives Phase A and Phase B against 10.255.255.1 - an RFC 5735 non-routable
# address, so nothing leaves the host and no engine is touched.
#
# Asserts on the VERDICT and the exit code, not on TIMEOUT specifically: some networks
# reject a non-routable connect immediately ('no route to host') rather than hanging
# until -TimeoutSec, and both must be non-fatal.
# ---------------------------------------------------------------------------
Describe 'Invoke-WwEnginePreWarm — non-fatal contract' {
    BeforeAll {
        # -AccessToken is a [securestring], which cannot be passed on a `pwsh -File`
        # command line, so the run goes through a wrapper. The wrapper is not a workaround
        # for the test's sake - it deliberately reproduces the SHAPE of the failing ADO
        # step: `$ErrorActionPreference = 'Stop'` followed by `& <script>`. That is the
        # combination that turned a relayed parallel-runspace error terminating and exited
        # the step with code 1.
        $script:Wrapper = Join-Path ([IO.Path]::GetTempPath()) "prewarm-nonfatal-$([guid]::NewGuid()).ps1"
        @'
$ErrorActionPreference = 'Stop'
$token = ConvertTo-SecureString 'unused-non-routable-target' -AsPlainText -Force
& $args[0] -EngineBaseUrl 'http://10.255.255.1:81' -AccessToken $token `
           -TimeoutSec 1 -TargetConcurrency 2 -ConcurrentRounds 1 -MaxSequential 1
'@ | Set-Content -LiteralPath $script:Wrapper -Encoding utf8
    }

    AfterAll {
        if ($script:Wrapper) { Remove-Item -LiteralPath $script:Wrapper -Force -ErrorAction SilentlyContinue }
    }

    It 'exits 0 and warns when every call fails, instead of failing the caller''s stage' {
        $pwshPath = (Get-Process -Id $PID).Path
        $out = & $pwshPath -NoLogo -NoProfile -NonInteractive -File $script:Wrapper $script:PreWarmScript 2>&1 | Out-String

        $LASTEXITCODE | Should -Be 0 -Because 'a warm-up OUTCOME must never fail the caller''s pipeline stage'
        $out | Should -Match 'the final round still had failures'
        $out | Should -Not -Match 'OK=2/6'
    }
}

Describe 'ConvertTo-WarmupResult' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'maps a timeout exception to the Code 0 sentinel and flags TimedOut' {
        $ex = [System.Threading.Tasks.TaskCanceledException]::new(
            'The request was canceled due to the configured HttpClient.Timeout of 240 seconds elapsing.')
        $r = ConvertTo-WarmupResult -ElapsedMs 240100 -Exception $ex
        $r.Code     | Should -Be 0
        $r.Ms       | Should -Be 240100
        $r.TimedOut | Should -BeTrue
        $r.Body     | Should -Match 'HttpClient.Timeout'
    }

    It 'maps a non-timeout exception to Code 0 without flagging TimedOut' {
        $r = ConvertTo-WarmupResult -ElapsedMs 12 -Exception 'No such host is known.'
        $r.Code     | Should -Be 0
        $r.TimedOut | Should -BeFalse
        $r.Body     | Should -Be 'No such host is known.'
    }

    It 'collapses whitespace and truncates a long exception message to 220 characters' {
        $r = ConvertTo-WarmupResult -ElapsedMs 1 -Exception ("x`r`n  y" + ('z' * 500))
        $r.Body.Length | Should -Be 220
        $r.Body        | Should -Match '^x y'
    }

    It 'maps a 200 response to its status code and an empty body' {
        $r = ConvertTo-WarmupResult -ElapsedMs 900 -Response ([pscustomobject]@{ StatusCode = 200; Content = 'ignored' })
        $r.Code     | Should -Be 200
        $r.TimedOut | Should -BeFalse
        $r.Body     | Should -BeNullOrEmpty
    }

    It 'keeps a truncated body for a non-200 response, so a 502 stays diagnosable' {
        $r = ConvertTo-WarmupResult -ElapsedMs 900 -Response ([pscustomobject]@{ StatusCode = 502; Content = ('a' * 500) })
        $r.Code        | Should -Be 502
        $r.Body.Length | Should -Be 220
    }
}

Describe 'Get-WarmupConcurrencyLadder' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'ramps the pipeline-LOADTEST target of 20 over 3 rounds as 5 -> 10 -> 20' {
        Get-WarmupConcurrencyLadder -TargetConcurrency 20 -Rounds 3 | Should -Be @(5, 10, 20)
    }

    It 'ramps the queue-processor target of 6 over 3 rounds as 2 -> 3 -> 6' {
        Get-WarmupConcurrencyLadder -TargetConcurrency 6 -Rounds 3 | Should -Be @(2, 3, 6)
    }

    It 'never drops below concurrency 1' {
        Get-WarmupConcurrencyLadder -TargetConcurrency 1 -Rounds 3 | Should -Be @(1, 1, 1)
    }

    It 'is a single full-concurrency round when only one round is asked for' {
        , (Get-WarmupConcurrencyLadder -TargetConcurrency 20 -Rounds 1) | Should -HaveCount 1
        (Get-WarmupConcurrencyLadder -TargetConcurrency 20 -Rounds 1)[0] | Should -Be 20
    }

    It 'always finishes at the full target, so the final-round verdict still means "clean at target"' {
        foreach ($target in 1, 2, 3, 6, 7, 20, 33, 100) {
            foreach ($rounds in 1, 2, 3, 5) {
                $ladder = @(Get-WarmupConcurrencyLadder -TargetConcurrency $target -Rounds $rounds)
                $ladder      | Should -HaveCount $rounds
                $ladder[-1]  | Should -Be $target
            }
        }
    }

    It 'returns nothing when asked for no rounds' {
        @(Get-WarmupConcurrencyLadder -TargetConcurrency 20 -Rounds 0) | Should -HaveCount 0
    }
}

Describe 'Format-WarmupCodeSummary' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'renders the Code 0 sentinel as TIMEOUT rather than a misleading "0x3"' {
        $results = @(
            [pscustomobject]@{ Code = 200; TimedOut = $false }
            [pscustomobject]@{ Code = 200; TimedOut = $false }
            [pscustomobject]@{ Code = 502; TimedOut = $false }
            [pscustomobject]@{ Code = 0;   TimedOut = $true }
            [pscustomobject]@{ Code = 0;   TimedOut = $true }
        )
        Format-WarmupCodeSummary -Results $results | Should -Be '200x2 502x1 TIMEOUTx2'
    }

    It 'distinguishes a non-timeout transport failure as ERROR' {
        $results = @([pscustomobject]@{ Code = 0; TimedOut = $false })
        Format-WarmupCodeSummary -Results $results | Should -Be 'ERRORx1'
    }

    It 'does not index-error on a round that produced no results at all' {
        Format-WarmupCodeSummary -Results @() | Should -Be '(no results)'
    }
}

Describe 'Get-WarmupUri' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'joins a base URL and route with exactly one slash' {
        Get-WarmupUri -BaseUrl 'https://example.azurewebsites.net' -Route 'Secure/rabbit/RabbitProcess' |
            Should -Be 'https://example.azurewebsites.net/Secure/rabbit/RabbitProcess'
    }

    It 'tolerates a trailing slash on the base URL and a leading slash on the route' {
        Get-WarmupUri -BaseUrl 'https://example.azurewebsites.net/' -Route '/Secure/rabbit/RabbitProcess' |
            Should -Be 'https://example.azurewebsites.net/Secure/rabbit/RabbitProcess'
    }
}

Describe 'Format-WarmupRequestBody' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'produces the inputParameters envelope WorkflowFunctionHelper.cs expects' {
        $json = Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1'
        $obj  = $json | ConvertFrom-Json
        $obj.inputParameters.message | Should -Be 'WARMUP-seq-1'
    }

    # The default body carries ONLY 'message', which is RabbitProcess's sole DataList input.
    # Aimed at any other workflow it binds nothing that workflow declares, so every warm-up
    # call fails on the workflow's own missing variables - a real-looking failure that says
    # nothing about engine warmth. Doing exactly that against the profiler's
    # RequestForProfiling on 2026-09-14 produced 119/119 "Invalid Calculate" failures that
    # were initially mistaken for a broken workflow fixture.
    It 'sends the supplied inputs instead of the default message input' {
        $json = Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' `
                    -InputParameters @{ MandateDRI = '22'; ResultPaymentCycle = '202401' }
        $obj  = $json | ConvertFrom-Json
        $obj.inputParameters.MandateDRI         | Should -Be '22'
        $obj.inputParameters.ResultPaymentCycle | Should -Be '202401'
        $obj.inputParameters.PSObject.Properties.Name | Should -Not -Contain 'message'
    }

    It 'falls back to the default message input when -InputParameters is empty or absent' {
        foreach ($empty in @($null, @{})) {
            $obj = (Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-2' -InputParameters $empty) | ConvertFrom-Json
            $obj.inputParameters.message | Should -Be 'WARMUP-seq-2'
        }
    }

    # ── '{guid}' per-call expansion ────────────────────────────────────────────────
    # pipeline-LOADTEST.yml reuses the BURST's own VerifyWorkflowInputsJson for the warm-up and
    # swaps MandateId to '{guid}'. Per-call uniqueness is a correctness requirement, not tidiness:
    # Profiler.usp_InsertProfilerRecommendation upserts on (MandateId, ForPaymentCycle), so a
    # constant MandateId makes every call after the first a no-op AlreadyExists=1 hit on one hot
    # row - warming a shorter code path than the burst exercises, and serialising Phase B on it.

    It 'replaces a {guid} token with a value the workflow can parse as a uniqueidentifier' {
        $obj = (Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' `
                    -InputParameters @{ MandateId = '{guid}' }) | ConvertFrom-Json
        $parsed = [guid]::Empty
        [guid]::TryParse($obj.inputParameters.MandateId, [ref]$parsed) |
            Should -BeTrue -Because 'MandateId reaches a uniqueidentifier column; an un-substituted token fails the parse'
    }

    It 'issues a DIFFERENT guid on every call' {
        $inputs = @{ MandateId = '{guid}' }
        $ids = 1..5 | ForEach-Object {
            ((Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label "seq-$_" -InputParameters $inputs) |
                ConvertFrom-Json).inputParameters.MandateId
        }
        ($ids | Select-Object -Unique).Count | Should -Be 5
    }

    # The parallel-safety invariant. $InputParameters reaches Phase B's runspaces by REFERENCE
    # ($using:), so expanding in place instead of into a copy would race and leak one call's GUID
    # into another call's body.
    It 'expands into a copy, leaving the caller''s hashtable untouched' {
        $inputs = @{ MandateId = '{guid}'; MandateDRI = '22' }
        $null = Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' -InputParameters $inputs
        $inputs['MandateId'] | Should -BeExactly '{guid}' -Because 'a mutated source hashtable would make every later call reuse the first call''s GUID'
    }

    It 'leaves values without a token, and non-string values, exactly as supplied' {
        $obj = (Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' -InputParameters @{
                    ResultPaymentCycle = '202401'
                    ScoreThisResult    = 'true'
                    SourcePaymentId    = 1234
                    MandateCanTrack    = $false
                }) | ConvertFrom-Json
        $obj.inputParameters.ResultPaymentCycle | Should -BeExactly '202401'
        $obj.inputParameters.ScoreThisResult    | Should -BeExactly 'true'
        $obj.inputParameters.SourcePaymentId    | Should -Be 1234
        $obj.inputParameters.MandateCanTrack    | Should -BeFalse
    }

    It 'gives repeated tokens in ONE value the same guid, so a single call keeps one identity' {
        $obj = (Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' `
                    -InputParameters @{ Pair = '{guid}|{guid}' }) | ConvertFrom-Json
        $left, $right = $obj.inputParameters.Pair.Split('|')
        $left | Should -BeExactly $right
    }

    # Guards the whole 16-field contract the pipeline actually sends, not just the token: the
    # defect this suite now covers was the warm-up binding NONE of these (build 30690's
    # "Scalar value { MandateDRI } is NULL: { DebitRunID }" on all 14 Phase A calls).
    It 'binds every field of a RequestForProfiling-shaped payload' {
        $inputs = @{
            ForPaymentCycle = '202402'; MandateCanTrack = 'false'; MandateDRI = '22'
            MandateId = '{guid}'; RelatedMandates = '5B9800B6-2183-EB11-A812-000D3ABE1AD1'
            ResultDate = '2024-01-27'; ResultDebitRunId = '22'; ResultMethod = 'DebiCheck'
            ResultPaymentCycle = '202401'; ResultPresentedOnDate = '2024-01-22'
            ResultType = 'Success'; ScoreThisResult = 'true'; IsFixedExtract = 'false'
            SourcePaymentId = '1234'; RequestType = ''; FASGroup = ''
        }
        $obj = (Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1' -InputParameters $inputs) | ConvertFrom-Json
        @($obj.inputParameters.PSObject.Properties.Name).Count | Should -Be 16
        $obj.inputParameters.MandateDRI         | Should -BeExactly '22'
        $obj.inputParameters.ResultPaymentCycle | Should -BeExactly '202401'
        $obj.inputParameters.PSObject.Properties.Name | Should -Not -Contain 'message'
    }
}

# ---------------------------------------------------------------------------
# Phase B re-materialises Format-WarmupRequestBody from its OWN SOURCE TEXT inside
# ForEach-Object -Parallel (${function:Format-WarmupRequestBody} = $using:bodyFn), where no
# other function from the script scope exists. So the moment that function calls a sibling
# helper, every Phase B call throws CommandNotFoundException - and because Phase B records
# failures as results rather than errors, it would surface as a uniformly failing warm-up
# rather than as a broken script. Asserted structurally because it cannot be caught by
# exercising the function directly in this runspace, where the siblings DO exist.
# ---------------------------------------------------------------------------
Describe 'Format-WarmupRequestBody — parallel self-containment (AST)' {
    BeforeAll {
        . $script:PreWarmScript -LoadFunctionsOnly
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:PreWarmScript, [ref]$null, [ref]$null)

        $script:BodyFnAst = $ast.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Format-WarmupRequestBody'
        }, $true) | Select-Object -First 1

        # Every other helper the script defines - none of these may be called from inside it.
        $script:SiblingNames = @($ast.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst]
        }, $true) | ForEach-Object Name | Where-Object { $_ -ne 'Format-WarmupRequestBody' })
    }

    It 'is defined in the script (guards against a rename breaking this assertion)' {
        $script:BodyFnAst | Should -Not -BeNullOrEmpty
        $script:SiblingNames.Count | Should -BeGreaterThan 0
    }

    It 'calls none of the script''s other helper functions' {
        $called = @($script:BodyFnAst.FindAll({
            param($n) $n -is [System.Management.Automation.Language.CommandAst]
        }, $true) | ForEach-Object { $_.GetCommandName() } | Where-Object { $_ })

        foreach ($name in $called) {
            $script:SiblingNames | Should -Not -Contain $name `
                -Because "Phase B re-materialises this function alone, so calling '$name' would throw CommandNotFoundException in every parallel warm-up call"
        }
    }

    It 'still produces a correct body when re-materialised from source text alone' {
        # Reproduces Phase B's own mechanism end to end, in a runspace where the siblings
        # genuinely are absent.
        $bodyFn = ${function:Format-WarmupRequestBody}.ToString()
        $inputs = @{ MandateId = '{guid}'; MandateDRI = '22' }

        $ids = 1..6 | ForEach-Object -Parallel {
            ${function:Format-WarmupRequestBody} = $using:bodyFn
            $json = Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label "c1-$_" -InputParameters $using:inputs
            $obj  = $json | ConvertFrom-Json
            if ($obj.inputParameters.MandateDRI -ne '22') { throw 'MandateDRI was not bound in the parallel runspace' }
            $obj.inputParameters.MandateId
        } -ThrottleLimit 6

        @($ids).Count                      | Should -Be 6
        ($ids | Select-Object -Unique).Count | Should -Be 6 -Because 'a shared-state leak across runspaces would repeat a GUID'
        $ids | Should -Not -Contain '{guid}'
        $inputs['MandateId']               | Should -BeExactly '{guid}'
    }
}

Describe 'Update-LatencyStreak' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'extends the streak on a fast 200' {
        Update-LatencyStreak -CurrentStreak 2 -StatusCode 200 -ElapsedMs 100 -ThresholdMs 5000 | Should -Be 3
    }

    It 'resets the streak on a non-200' {
        Update-LatencyStreak -CurrentStreak 3 -StatusCode 500 -ElapsedMs 100 -ThresholdMs 5000 | Should -Be 0
    }

    It 'resets the streak on the Code 0 timeout sentinel' {
        Update-LatencyStreak -CurrentStreak 3 -StatusCode 0 -ElapsedMs 240000 -ThresholdMs 5000 | Should -Be 0
    }

    It 'resets the streak on a slow 200 (at or over the threshold)' {
        Update-LatencyStreak -CurrentStreak 3 -StatusCode 200 -ElapsedMs 5000 -ThresholdMs 5000 | Should -Be 0
    }

    It 'starts a new streak of 1 from zero on a fast 200' {
        Update-LatencyStreak -CurrentStreak 0 -StatusCode 200 -ElapsedMs 50 -ThresholdMs 5000 | Should -Be 1
    }
}

Describe 'Test-WarmupRoundClean' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'is clean when every result is 200 and the count matches' {
        $results = 1..6 | ForEach-Object { [pscustomobject]@{ Code = 200; Ms = 42 } }
        Test-WarmupRoundClean -Results $results -ExpectedCount 6 | Should -BeTrue
    }

    It 'is not clean when any result is not 200' {
        $results = @(
            [pscustomobject]@{ Code = 200; Ms = 42 }
            [pscustomobject]@{ Code = 503; Ms = 42 }
        )
        Test-WarmupRoundClean -Results $results -ExpectedCount 2 | Should -BeFalse
    }

    It 'is not clean when a call timed out (the Code 0 sentinel)' {
        $results = @(
            [pscustomobject]@{ Code = 200; Ms = 42;     TimedOut = $false }
            [pscustomobject]@{ Code = 0;   Ms = 240100; TimedOut = $true }
        )
        Test-WarmupRoundClean -Results $results -ExpectedCount 2 | Should -BeFalse
    }

    It 'is not clean when fewer results came back than expected (e.g. a dropped call)' {
        $results = 1..5 | ForEach-Object { [pscustomobject]@{ Code = 200; Ms = 42 } }
        Test-WarmupRoundClean -Results $results -ExpectedCount 6 | Should -BeFalse
    }

    It 'is not clean, and does not throw, when a round produced no results at all' {
        Test-WarmupRoundClean -Results @() -ExpectedCount 6 | Should -BeFalse
    }
}
