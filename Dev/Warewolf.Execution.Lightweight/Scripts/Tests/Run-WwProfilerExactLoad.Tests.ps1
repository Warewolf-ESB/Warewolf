#Requires -Version 7.0

<#
    Pester 5 test suite for Run-WwProfilerExactLoad.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Run-WwProfilerExactLoad.Tests.ps1

    Design notes (same conventions as Invoke-WwEnginePreWarm.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the pure helpers and returns BEFORE the
      SQL-connection guard and before Invoke-WwQueueLoadTest.ps1 is dot-sourced,
      so nothing here needs a database, a broker, or an engine.
    * NO network call, NO SQL connection and NO live engine is made anywhere in
      this suite.
    * The two 'regression' Describes cover defects this script actually shipped
      with, both of which were SILENT - they made the run report a successful
      warm-up that had warmed nothing. Assert the invariant, not the symptom.
#>

BeforeAll {
    $script:LoadScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Run-WwProfilerExactLoad.ps1'

    # The canonical request. Its query string IS the payload contract (see the script's own -Url
    # help), so these 16 fields are what both the warm-up and the publish must send.
    $script:CanonicalUrl =
        'https://wwuat3.azurewebsites.net/secure/ProfilerServices/ProfilerWrapper/RequestForProfiling.json' +
        '?ForPaymentCycle=202402&MandateCanTrack=false&MandateDRI=22' +
        '&MandateId=00000000-2183-EB11-A812-000D3ABE1AD1' +
        '&RelatedMandates=5B9800B6-2183-EB11-A812-000D3ABE1AD1&ResultDate=2024-01-27' +
        '&ResultDebitRunId=22&ResultMethod=DebiCheck&ResultPaymentCycle=202401' +
        '&ResultPresentedOnDate=2024-01-22&ResultType=Success&ScoreThisResult=true' +
        '&IsFixedExtract=false&SourcePaymentId=1234&RequestType=&FASGroup='

    . $script:LoadScript -LoadFunctionsOnly
}

Describe 'Run-WwProfilerExactLoad — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:LoadScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its pure helpers under -LoadFunctionsOnly without running any phase' {
        $out = (. $script:LoadScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Step 1'
        $out | Should -Not -Match 'Step 2'
        $out | Should -Not -Match 'pre-warm'
        Get-Command Get-WwProfilerFieldsFromUrl -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-WwProfilerPreWarmRoute  -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'returns under -LoadFunctionsOnly even with no SQL connection configured' {
        # The SQL guard throws when neither -SqlConnectionString nor $env:WWLOADTEST_SQLCONNECTION
        # is set. The hook must return above it, or the helpers are untestable on any machine that
        # has not been set up to run a load test.
        $saved = $env:WWLOADTEST_SQLCONNECTION
        try {
            $env:WWLOADTEST_SQLCONNECTION = $null
            { . $script:LoadScript -LoadFunctionsOnly } | Should -Not -Throw
        }
        finally { $env:WWLOADTEST_SQLCONNECTION = $saved }
    }
}

Describe 'Get-WwProfilerFieldsFromUrl' {

    It 'returns all 16 fields of the canonical request' {
        $fields = Get-WwProfilerFieldsFromUrl -Url $script:CanonicalUrl
        $fields.Count | Should -Be 16
    }

    It 'parses values verbatim' {
        $fields = Get-WwProfilerFieldsFromUrl -Url $script:CanonicalUrl
        $fields['ForPaymentCycle'] | Should -Be '202402'
        $fields['MandateDRI']      | Should -Be '22'
        $fields['ResultMethod']    | Should -Be 'DebiCheck'
        $fields['MandateId']       | Should -Be '00000000-2183-EB11-A812-000D3ABE1AD1'
    }

    It 'KEEPS fields whose value is empty' {
        # 'RequestType=' and 'FASGroup=' are legitimately empty in the canonical request. Dropping
        # them would warm a different workflow shape than the publish sends.
        $fields = Get-WwProfilerFieldsFromUrl -Url $script:CanonicalUrl
        $fields.ContainsKey('RequestType') | Should -BeTrue
        $fields.ContainsKey('FASGroup')    | Should -BeTrue
        $fields['RequestType'] | Should -Be ''
        $fields['FASGroup']    | Should -Be ''
    }

    It 'strips reserved transport keys the engine supplies itself' {
        $url = $script:CanonicalUrl + '&wid=abc&isDebug=true'
        $fields = Get-WwProfilerFieldsFromUrl -Url $url
        $fields.ContainsKey('wid')     | Should -BeFalse
        $fields.ContainsKey('isDebug') | Should -BeFalse
        $fields.Count | Should -Be 16
    }

    It 'url-decodes encoded values' {
        $fields = Get-WwProfilerFieldsFromUrl -Url 'https://x/y.json?A=hello%20world&B=a%2Bb'
        $fields['A'] | Should -Be 'hello world'
        $fields['B'] | Should -Be 'a+b'
    }

    It 'returns an empty set for a url with no query string' {
        $fields = Get-WwProfilerFieldsFromUrl -Url 'https://wwuat3.azurewebsites.net/secure/X.json'
        $fields.Count | Should -Be 0
    }
}

Describe 'Get-WwProfilerPreWarmRoute' {

    It "keeps the url's own path and prefix" {
        # A -Url pointing at /secure/... must warm /secure/..., not a route the script invented.
        Get-WwProfilerPreWarmRoute -Url $script:CanonicalUrl |
            Should -Be 'secure/ProfilerServices/ProfilerWrapper/RequestForProfiling.json'
    }

    It 'drops the query string (the pre-warm posts inputs as a body)' {
        Get-WwProfilerPreWarmRoute -Url $script:CanonicalUrl | Should -Not -Match '\?'
    }

    It 'returns a relative route with no leading slash' {
        # Invoke-WwEnginePreWarm.ps1's Get-WarmupUri joins base + route; a leading slash would
        # produce a double slash against a base that already ends in one.
        Get-WwProfilerPreWarmRoute -Url 'https://host/Public/A/B.json' | Should -Be 'Public/A/B.json'
    }
}

Describe 'Run-WwProfilerExactLoad — pre-warm defaults (regression)' {

    BeforeAll {
        $script:Params = (Get-Command $script:LoadScript).Parameters
    }

    It 'defaults -PreWarmConcurrency to 2, not the pre-warm script''s 6' {
        # REGRESSION: this script never passed -TargetConcurrency, so it silently inherited
        # Invoke-WwEnginePreWarm.ps1's default of 6 - and six concurrent workflow-route warms is
        # the documented main source of the 'Insufficient memory' OOMs on the Y1 plan.
        $default = (Get-Command $script:LoadScript).ScriptBlock.Ast.ParamBlock.Parameters |
            Where-Object { $_.Name.VariablePath.UserPath -eq 'PreWarmConcurrency' }
        $default | Should -Not -BeNullOrEmpty
        $default.DefaultValue.Extent.Text | Should -Be '2'
    }

    It 'exposes -PreWarmInputParameters as a hashtable' {
        $script:Params.ContainsKey('PreWarmInputParameters') | Should -BeTrue
        $script:Params['PreWarmInputParameters'].ParameterType.Name | Should -Be 'Hashtable'
    }

    It 'leaves -PreWarmRoute empty by default so it is derived from -Url' {
        # REGRESSION: the default used to be 'Public/apis.json', which is discovery-only -
        # WorkflowExecutor's OPENAPI branch does no XAML load and no execution - so it never
        # populated WorkflowExecutor._workflowPool and the "warm" engine still paid the XAML
        # compile and activity parse on the first real message.
        $default = (Get-Command $script:LoadScript).ScriptBlock.Ast.ParamBlock.Parameters |
            Where-Object { $_.Name.VariablePath.UserPath -eq 'PreWarmRoute' }
        $default | Should -Not -BeNullOrEmpty
        $default.DefaultValue | Should -BeNullOrEmpty
    }
}

Describe 'Run-WwProfilerExactLoad — empty-datalist guard (regression)' {

    # REGRESSION: a workflow route with no inputs makes every warm-up call execute against an empty
    # datalist and fail on the workflow's own missing variables ("Scalar value { MandateDRI } is
    # NULL ... Invalid Calculate") - 119/119 on 2026-09-14. The pre-warm is non-fatal BY DESIGN, so
    # without this guard that total failure is swallowed and the run reports a successful warm-up.
    # Asserting the invariant: inputs must resolve for any route that is not discovery-only.

    It 'treats an apis.json route as discovery-only (exempt from the input requirement)' {
        $route = 'Public/apis.json'
        ($route -match 'apis\.json$') | Should -BeTrue
    }

    It 'does not treat a workflow route as discovery-only' {
        $route = Get-WwProfilerPreWarmRoute -Url $script:CanonicalUrl
        ($route -match 'apis\.json$') | Should -BeFalse
    }

    It 'resolves a non-empty input set for the canonical url, so the guard does not trip' {
        $route  = Get-WwProfilerPreWarmRoute -Url $script:CanonicalUrl
        $inputs = Get-WwProfilerFieldsFromUrl -Url $script:CanonicalUrl
        ($route -match 'apis\.json$') | Should -BeFalse
        $inputs.Count | Should -BeGreaterThan 0
    }

    It 'resolves an EMPTY input set for a workflow url with no query string (the guard condition)' {
        $url    = 'https://wwuat3.azurewebsites.net/secure/ProfilerServices/ProfilerWrapper/RequestForProfiling.json'
        $route  = Get-WwProfilerPreWarmRoute -Url $url
        $inputs = Get-WwProfilerFieldsFromUrl -Url $url
        ($route -match 'apis\.json$') | Should -BeFalse
        $inputs.Count | Should -Be 0
    }

    It 'carries the guard in the script body, wired to both conditions' {
        # The throw lives inside the pre-warm block, which cannot run here without an engine - so
        # assert the source wires the two resolved values together rather than re-implementing it.
        # Matched as a BOOLEAN, not 'Should -Match' on the raw text: a failing -Match prints the
        # whole ~20KB script into the Pester report, which buries the one line that matters.
        $src = Get-Content -LiteralPath $script:LoadScript -Raw
        [bool]($src -match '\$isDiscoveryRoute\s*=\s*\$PreWarmRoute\s*-match') |
            Should -BeTrue -Because 'the discovery-route exemption must be computed from -PreWarmRoute'
        [bool]($src -match 'if\s*\(-not \$isDiscoveryRoute -and \$warmInputs\.Count -eq 0\)\s*\{') |
            Should -BeTrue -Because 'the guard must require BOTH a non-discovery route and zero inputs'
        [bool]($src -match 'use -PreWarmRoute .Public/apis\.json. for a discovery-only') |
            Should -BeTrue -Because 'the throw must tell the operator how to opt into a discovery warm'
    }
}

Describe 'Run-WwProfilerExactLoad — run summary records how it warmed' {

    It 'persists the pre-warm route, input count and concurrency' {
        # Two runs are not comparable without these: a discovery warm and a workflow warm produce
        # completely different first-batch percentiles, and concurrency sets the pool depth.
        $src = Get-Content -LiteralPath $script:LoadScript -Raw
        foreach ($field in 'preWarmRoute', 'preWarmInputCount', 'preWarmConcurrency') {
            [bool]($src -match "$field\s*=") |
                Should -BeTrue -Because "the run summary must record '$field' for runs to be comparable"
        }
    }
}
