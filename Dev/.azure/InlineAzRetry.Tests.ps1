#Requires -Version 7.0
<#
    Pester 5 suite: every inline `az` step in the Dev/.azure pipelines retries transient failures.

        Invoke-Pester -Path ./InlineAzRetry.Tests.ps1

    Build 30844 lost a whole run to one ARM "Service Unavailable" page in a two-line
    `az functionapp config appsettings set` step, because inline steps called az bare while only the
    deploy scripts went through the shared retry policy (Scripts/WwAzCli.psm1). Each such step now
    opens with

        Import-Module '<Scripts>\WwAzCli.psm1' -Force
        Set-Alias -Name az -Value Invoke-WwAz -Scope Script

    and this suite fails any step that calls az without it - including a NEW step added later.
    Offline: YAML is parsed, nothing runs. Requires powershell-yaml; skipped (not failed) without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:Files = 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml', 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml',
                    'pipeline-CLOUD.yml', 'pipeline-LOADTEST.yml'
    $script:ModulePath = Join-Path $PSScriptRoot '..\Warewolf.Execution.Lightweight\Scripts\WwAzCli.psm1'
    $script:RepoModule     = '$(Build.SourcesDirectory)\Dev\Warewolf.Execution.Lightweight\Scripts\WwAzCli.psm1'
    $script:ArtifactModule = '$(Agent.BuildDirectory)\DeployScripts\Scripts\WwAzCli.psm1'

    # A bare az invocation on a non-comment line (not "az" inside a word, a flag or a message).
    $script:AzCall   = '(?m)^(?!\s*#).*?(^|[\s(=|;{])az\s+[a-z]'
    $script:AliasRx  = '(?m)^\s*Set-Alias -Name az -Value Invoke-WwAz -Scope Script\s*$'

    function script:Get-Body($Step) {
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        if ($Step['inputs'] -is [System.Collections.IDictionary] -and $Step['inputs'].Contains('inlineScript')) { return [string]$Step['inputs']['inlineScript'] }
        return $null
    }

    # One record per inline step that calls az: file, job, the job's steps and this step's index.
    $script:AzSteps = [System.Collections.Generic.List[object]]::new()
    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        foreach ($f in $script:Files) {
            $doc  = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath (Join-Path $PSScriptRoot $f) -Raw)
            $jobs = if ($doc.Contains('jobs')) { $doc['jobs'] } else { $doc['stages'] | ForEach-Object { $_['jobs'] } }
            foreach ($job in $jobs) {
                $steps = @($job['steps'])
                for ($i = 0; $i -lt $steps.Count; $i++) {
                    $body = Get-Body $steps[$i]
                    if ($body -and $body -match $script:AzCall) {
                        $script:AzSteps.Add([pscustomobject]@{
                            File = $f; Job = [string]$job['job']; Steps = $steps; Index = $i
                            Step = $steps[$i]; Body = $body; Id = "$f / $($job['job']) / $($steps[$i]['displayName'])"
                        })
                    }
                }
            }
        }
    }

    function script:Get-Failures([scriptblock] $Check) {
        foreach ($s in $script:AzSteps) { $why = & $Check $s; if ($why) { "[$($s.Id)] $why" } }
    }
}

Describe 'Inline az steps retry transient failures (build 30844)' -Skip:(-not $script:HaveYaml) {

    It 'finds every inline az step (guards against a vacuous pass)' {
        # 6 Dev + 8 UAT + 7 CLOUD + 9 LOADTEST at the time of writing.
        $script:AzSteps.Count | Should -BeGreaterOrEqual 30
    }

    It 'imports WwAzCli.psm1 and aliases az to Invoke-WwAz before the first az call' {
        Get-Failures {
            param($s)
            $lines = $s.Body -split "`n"
            $alias = [Array]::FindIndex($lines, [Predicate[string]] { param($l) $l -match $script:AliasRx })
            $first = [Array]::FindIndex($lines, [Predicate[string]] { param($l) $l -match $script:AzCall })
            $import = [Array]::FindIndex($lines, [Predicate[string]] { param($l) $l -match "^\s*Import-Module '[^']*\\WwAzCli\.psm1' -Force\s*$" })
            if ($import -lt 0) { return 'no Import-Module of WwAzCli.psm1' }
            if ($alias -lt 0)  { return 'no az -> Invoke-WwAz alias' }
            if ($import -gt $alias) { return 'alias set before the module is imported' }
            if ($alias -gt $first)  { return "az is called (line $($first + 1)) before the alias is set" }
        } | Should -BeNullOrEmpty
    }

    It 'runs under PowerShell 7 (WwAzCli.psm1 is #Requires -Version 7.0)' {
        Get-Failures {
            param($s)
            if ($s.Step.Contains('powershell')) { return "'powershell:' is Windows PowerShell 5.1 - use 'pwsh:'" }
            if ($s.Step.Contains('task') -and $s.Step['inputs']['scriptType'] -ne 'pscore') { return "scriptType is '$($s.Step['inputs']['scriptType'])', not pscore" }
        } | Should -BeNullOrEmpty
    }

    It 'loads the module from a path that exists in that job (checkout, or the DeployScripts artifact)' {
        Get-Failures {
            param($s)
            $before = @($s.Steps[0..$s.Index])
            $checkedOut = [bool]($before | Where-Object { $_.Contains('checkout') -and $_['checkout'] -eq 'self' })
            $artifact   = [bool]($before | Where-Object { $_['task'] -like 'DownloadPipelineArtifact@*' -and $_['inputs']['artifact'] -eq 'DeployScripts' -and $_['inputs']['path'] -eq '$(Agent.BuildDirectory)\DeployScripts' })
            if ($s.Body.Contains($script:RepoModule)) { if (-not $checkedOut) { return 'imports from the checkout, but the job has no checkout: self before it' } }
            elseif ($s.Body.Contains($script:ArtifactModule)) { if (-not $artifact) { return 'imports from the DeployScripts artifact, but the job never downloads it before this step' } }
            else { return 'imports WwAzCli.psm1 from an unrecognised path' }
        } | Should -BeNullOrEmpty
    }

    It 'never lets a child script inherit the alias (it would change that script''s own az wrapper)' {
        Get-Failures {
            param($s)
            $afterAlias = ($s.Body -split $script:AliasRx, 2)[1]
            if ($afterAlias -match '(?m)^\s*&\s') { return "invokes a child script after the alias: $($Matches[0].Trim())" }
        } | Should -BeNullOrEmpty
    }

    It 'every inline az step is syntactically valid PowerShell' {
        Get-Failures {
            param($s)
            $expanded = [regex]::Replace($s.Body, '\$\([A-Za-z_][A-Za-z0-9_.]*\)', 'MACRO')
            $tokens = $null; $errors = $null
            [void][System.Management.Automation.Language.Parser]::ParseInput($expanded, [ref]$tokens, [ref]$errors)
            if ($errors) { return ($errors | ForEach-Object { "line $($_.Extent.StartLineNumber): $($_.Message)" }) -join '; ' }
        } | Should -BeNullOrEmpty
    }

    It 'the module the steps import exists and exports Invoke-WwAz' {
        Test-Path -LiteralPath $script:ModulePath | Should -BeTrue
        $m = Import-Module $script:ModulePath -Force -PassThru
        try { $m.ExportedFunctions.Keys | Should -Contain 'Invoke-WwAz' } finally { Remove-Module $m -ErrorAction SilentlyContinue }
    }
}
