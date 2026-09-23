<#
    Pester 5 suite for the Linux-host support in TestRun.ps1.

        Invoke-Pester -Path ./TestRun.Tests.ps1

    TestRun.ps1 is a script, not a module: dot-sourcing it would execute direct/catalog mode.
    So each test loads only the function definitions it needs, lifted out of the script's own
    AST - the code under test is exactly what ships, with no copy to drift.

    Focus: what lets the CI test jobs run on ubuntu-latest agents - the pipeline.yml catalog
    parser accepting '/' coverage paths, native-tool path conversion, the Windows-only host
    switches being skipped (not fatal) off Windows, -WindowStyle only being passed on Windows,
    and the shared FTP / MSSQL fixture definitions that the native and docker paths both use.
    Entirely offline: no docker, no dotnet, no network.
#>

BeforeAll {
    $script:TestRunPath = Join-Path $PSScriptRoot 'TestRun.ps1'
    $tokens = $null; $errors = $null
    $script:TestRunAst = [System.Management.Automation.Language.Parser]::ParseFile($script:TestRunPath, [ref]$tokens, [ref]$errors)
    if ($errors.Count -gt 0) { throw "TestRun.ps1 does not parse: $($errors[0])" }

    # Defines the named functions from TestRun.ps1 in the caller's (test) scope.
    function script:Import-TestRunFunction([string[]]$Name) {
        foreach ($n in $Name) {
            $fn = $script:TestRunAst.Find({ param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq $n }, $true)
            if (-not $fn) { throw "function $n not found in TestRun.ps1" }
            . ([scriptblock]::Create($fn.Extent.Text))
            Set-Item -Path "function:script:$n" -Value (Get-Item "function:$n").ScriptBlock
        }
    }

    # Evaluates a top-level `$script:Name = ...` assignment from TestRun.ps1 and returns its value.
    function script:Get-TestRunScriptVariable([string]$Name) {
        $assign = $script:TestRunAst.Find({ param($a)
            $a -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $a.Left.Extent.Text -eq "`$script:$Name" }, $false)
        if (-not $assign) { throw "`$script:$Name assignment not found in TestRun.ps1" }
        return (& ([scriptblock]::Create($assign.Right.Extent.Text)))
    }

    function script:New-PipelineFixture([string]$JobBody) {
        $path = Join-Path ([System.IO.Path]::GetTempPath()) ("pipeline-" + [Guid]::NewGuid().ToString('N') + '.yml')
        Set-Content -LiteralPath $path -Value "stages:`n- stage: Test`n  jobs:`n$JobBody" -Encoding UTF8
        return $path
    }
}

Describe 'ConvertFrom-PipelineYaml - coverage output path' {
    BeforeAll { script:Import-TestRunFunction 'Get-Slug', 'ConvertFrom-PipelineYaml' }

    It 'reads -EngineCoverageFile written with <Separator> separators' -TestCases @(
        @{ Separator = "'/' (Linux job)";    Arg = '"$(Agent.BuildDirectory)/coverage/zip_tool_specs.engine.cobertura.xml"' }
        @{ Separator = "'\' (Windows job)";  Arg = '"$(Agent.BuildDirectory)\coverage\zip_tool_specs.engine.cobertura.xml"' }
    ) {
        param($Separator, $Arg)
        $path = script:New-PipelineFixture @"
  - job: Zip_Tool_Specs
    steps:
    - task: PowerShell@2
      inputs:
        filePath: '`$(Agent.BuildDirectory)/LinuxTests/TestRun.ps1'
        arguments: >-
          -Projects "Warewolf.Tools.Specs"
          -Filter 'TestCategory=Zip'
          -ServerType LightweightExecution
          -EngineCoverageFile $Arg
          -EngineSessionId "zip_tool_specs-session"
        workingDirectory: '`$(Agent.BuildDirectory)/LinuxTests'
"@
        try {
            $catalog = ConvertFrom-PipelineYaml -YamlPath $path
            $catalog.Count | Should -Be 1
            $catalog[0].Output | Should -Be 'zip_tool_specs.engine.cobertura.xml'
            $catalog[0].Type | Should -Be 'EngineSpec'
            $catalog[0].SessionId | Should -Be 'zip_tool_specs-session'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }
}

Describe 'ConvertTo-NativeToolPath' {
    BeforeAll { script:Import-TestRunFunction 'ConvertTo-NativeToolPath' }

    It 'turns backslashes into forward slashes off Windows' {
        ConvertTo-NativeToolPath '.\TestResults\vstest.runsettings' -OnWindows $false | Should -Be './TestResults/vstest.runsettings'
    }

    It 'leaves the path untouched on Windows' {
        ConvertTo-NativeToolPath '.\TestResults\vstest.runsettings' -OnWindows $true | Should -Be '.\TestResults\vstest.runsettings'
    }

    It 'leaves an already-native Linux path untouched' {
        ConvertTo-NativeToolPath '/home/vsts/work/1/TestResults' -OnWindows $false | Should -Be '/home/vsts/work/1/TestResults'
    }
}

Describe 'Get-UnsupportedHostSwitches' {
    BeforeAll { script:Import-TestRunFunction 'Get-UnsupportedHostSwitches' }

    It 'reports the Windows-only host switches off Windows, and nothing else' {
        $unsupported = Get-UnsupportedHostSwitches @('CreateUNCPath', 'StartFTPServer', 'UseRegionalSettings', 'Coverage', 'CreateLocalSchedulerAdmin') -OnWindows $false
        $unsupported | Should -Be @('CreateUNCPath', 'UseRegionalSettings', 'CreateLocalSchedulerAdmin')
    }

    It 'reports nothing on Windows, where every switch is supported' {
        @(Get-UnsupportedHostSwitches @('CreateUNCPath', 'UseRegionalSettings') -OnWindows $true).Count | Should -Be 0
    }

    It 'reports nothing when no Windows-only switch was requested' {
        @(Get-UnsupportedHostSwitches @('StartSFTPServer', 'Coverage') -OnWindows $false).Count | Should -Be 0
    }
}

Describe 'Start-HiddenProcess' {
    BeforeAll { script:Import-TestRunFunction 'Start-HiddenProcess' }

    It 'passes -WindowStyle Hidden on Windows' {
        Mock Start-Process { }
        Start-HiddenProcess @{ FilePath = 'func'; ArgumentList = @('start') } -OnWindows $true
        Should -Invoke Start-Process -Times 1 -Exactly -ParameterFilter { $WindowStyle -eq 'Hidden' -and $PassThru -and $FilePath -eq 'func' }
    }

    It 'omits -WindowStyle off Windows, where Start-Process rejects it' {
        Mock Start-Process { }
        Start-HiddenProcess @{ FilePath = 'func'; ArgumentList = @('start') } -OnWindows $false
        Should -Invoke Start-Process -Times 1 -Exactly -ParameterFilter { -not $PesterBoundParameters.ContainsKey('WindowStyle') -and $PassThru -and $FilePath -eq 'func' }
    }
}

Describe 'Shared dependency fixtures (native and docker paths)' {

    It 'lists every FOR*TESTING folder the File/Folder specs use, with Linux-safe separators' {
        $subdirs = script:Get-TestRunScriptVariable 'FtpTestSubdirs'
        $subdirs.Count | Should -Be 13
        $subdirs | Should -Contain 'FORCOPYFILETESTING'
        $subdirs | Should -Contain 'FORZIPTESTING'
        $subdirs | Should -Contain 'FORTESTING/emptydir'
        ($subdirs | Where-Object { $_ -like '*\*' }) | Should -BeNullOrEmpty
    }

    It 'seeds Dev2TestingDB with FidelityPing in its own batch and creates the testUser login' {
        script:Import-TestRunFunction 'Get-MssqlLoginBatches', 'Get-MssqlSeedBatches'
        $script:MssqlTestUserPassword = script:Get-TestRunScriptVariable 'MssqlTestUserPassword'

        $login = Get-MssqlLoginBatches
        $login[0] | Should -Match "CREATE LOGIN \[testUser\] WITH PASSWORD = 'Ex@mple!23Secure#PWD'"

        $seed = Get-MssqlSeedBatches
        $seed[0].Sql | Should -Match 'CREATE DATABASE \[Dev2TestingDB\]'
        # CREATE/ALTER PROCEDURE must be the only statement in its sqlcmd batch.
        $proc = $seed | Where-Object { $_.Sql -match 'PROCEDURE dbo\.FidelityPing' }
        $proc.Sql | Should -Not -Match ';\s*\S+.*CREATE'
        $proc.Database | Should -Be 'Dev2TestingDB'
    }

    It 'provisions the docker MSSQL path from the same fixture functions as the native path' {
        $fn = $script:TestRunAst.Find({ param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Start-HostMSSQLServer' }, $true)
        $body = $fn.Extent.Text
        $dockerPart = $body.Substring($body.IndexOf('docker run -d --name sqlserver'))
        $dockerPart | Should -Match 'Get-MssqlLoginBatches'
        $dockerPart | Should -Match 'Get-MssqlSeedBatches'
        $dockerPart | Should -Match 'Get-MssqlBacpacPath'
        $dockerPart | Should -Match 'mssql-tools18/bin/sqlcmd'
    }
}
