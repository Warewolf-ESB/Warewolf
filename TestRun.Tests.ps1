<#
    Pester 5 suite for the Linux-host support in TestRun.ps1.

        Invoke-Pester -Path ./TestRun.Tests.ps1

    TestRun.ps1 is a script, not a module: dot-sourcing it would execute direct/catalog mode.
    So each test loads only the function definitions it needs, lifted out of the script's own
    AST - the code under test is exactly what ships, with no copy to drift.

    Focus: what lets the CI test jobs run on ubuntu-latest agents - the pipeline.yml catalog
    parser accepting '/' coverage paths, native-tool path conversion, the Windows-only host
    switches being skipped (not fatal) off Windows, -WindowStyle only being passed on Windows,
    the shared FTP / MSSQL fixture definitions that the native and docker paths both use, and
    the Lightweight engine's Azurite / log handling across mid-run engine restarts.
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

Describe 'Get-TestRunnerCommand - coverage flag' {
    # Regression guard for the "Other Specs" dual-coverage collision (CI build 30818,
    # 2026-09-24): every EngineSpec job wraps the Lightweight engine in its own
    # `dotnet-coverage collect -- func start` for engine-side coverage (CoverageDir/
    # EngineSessionId), and separately -Coverage defaults to $true, which used to make
    # this function also add --collect "Code Coverage" to the SpecFlow test-host's own
    # `dotnet test` line. Two concurrent, unisolated coverage profiler sessions on the
    # same bare-metal Linux agent corrupted the engine's assembly loading at startup
    # ("Error building configuration in an external startup class ... Index not found"),
    # permanently 503-ing it for the rest of every retry. The fix passes -Coverage:$false
    # on every EngineSpec job in pipeline.yml; this test locks in the mechanism that
    # relies on -- that Get-TestRunnerCommand actually honors CollectCoverage=$false.
    BeforeAll {
        script:Import-TestRunFunction 'Get-TestRunnerCommand'
        # Get-TestRunnerCommand branches on the script-level $UseDotnetTest (set from
        # $IsLinux at the top of TestRun.ps1) to pick the Linux `dotnet test` invocation
        # over the Windows vstest.console.exe one. Force the Linux branch here since
        # that's the one every EngineSpec CI job on ubuntu-latest actually runs.
        $global:UseDotnetTest = $true
    }
    AfterAll { Remove-Variable -Name UseDotnetTest -Scope Global -ErrorAction SilentlyContinue }

    It 'omits --collect "Code Coverage" when CollectCoverage is false' {
        $line = Get-TestRunnerCommand -AssemblyPaths @('Warewolf.Security.Specs.dll') -CollectCoverage $false -ResultsDir '/tmp/results'
        $line | Should -Not -Match 'Code Coverage'
    }

    It 'includes --collect "Code Coverage" when CollectCoverage is true' {
        $line = Get-TestRunnerCommand -AssemblyPaths @('Warewolf.Security.Specs.dll') -CollectCoverage $true -ResultsDir '/tmp/results'
        $line | Should -Match '--collect "Code Coverage"'
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

Describe 'Move-ToNumberedLog' {
    BeforeAll { script:Import-TestRunFunction 'Move-ToNumberedLog' }
    BeforeEach {
        $script:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("logs-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:logDir | Out-Null
    }
    AfterEach { Remove-Item -LiteralPath $script:logDir -Recurse -Force -ErrorAction SilentlyContinue }

    It 'keeps each engine generation as name(N).ext instead of letting the next start truncate it' {
        $log = Join-Path $script:logDir 'warewolf-server.log'
        'first'  | Set-Content -LiteralPath $log
        Move-ToNumberedLog $log
        'second' | Set-Content -LiteralPath $log
        Move-ToNumberedLog $log

        Test-Path -LiteralPath $log | Should -BeFalse
        Get-Content -LiteralPath (Join-Path $script:logDir 'warewolf-server(1).log') | Should -Be 'first'
        Get-Content -LiteralPath (Join-Path $script:logDir 'warewolf-server(2).log') | Should -Be 'second'
    }

    It 'does nothing when the log does not exist yet (first engine start)' {
        Move-ToNumberedLog (Join-Path $script:logDir 'warewolf-server.log')
        @(Get-ChildItem -LiteralPath $script:logDir).Count | Should -Be 0
    }
}

# CI build 30791 "Other Specs": Stop-Engine killed the Azurite that Start-Azurite had started,
# but AzureWebJobsStorage stayed set, so every retry's Start-Azurite skipped ("already set") and
# the engine ran against a dead 127.0.0.1:10000 on all 7 attempts.
Describe 'Azurite ownership of AzureWebJobsStorage across engine restarts' {
    BeforeAll {
        script:Import-TestRunFunction 'Start-Azurite', 'Stop-Engine', 'Start-HiddenProcess'
        function script:Write-Warn([string]$m) { }
        function script:Resolve-AzuriteExe { $null }   # no Azurite: exercises the skip/fallback decisions only
    }
    BeforeEach {
        $script:savedStorage = $env:AzureWebJobsStorage
        $script:_sessionId = ''
        $script:_coverageProcess = $null
        $script:_serverProcess = $null
        Mock Get-Process { }   # keep Stop-Engine's by-name sweep off real processes
        Mock Start-HiddenProcess { }
    }
    AfterEach {
        if ($script:savedStorage) { $env:AzureWebJobsStorage = $script:savedStorage }
        else { Remove-Item Env:AzureWebJobsStorage -ErrorAction SilentlyContinue }
    }

    It 'Start-Azurite does not skip on its own stale value once its Azurite has exited' {
        $script:_azuriteOwnsStorage = $true
        $script:_azuriteProcess = [pscustomobject]@{ HasExited = $true }
        $env:AzureWebJobsStorage = 'UseDevelopmentStorage=true'
        Mock Resolve-AzuriteExe { $null }

        Start-Azurite

        Should -Invoke Resolve-AzuriteExe -Times 1 -Exactly   # went on to (re)start Azurite
        $env:AzureWebJobsStorage | Should -BeNullOrEmpty      # fallback leaves no dead endpoint behind
        $script:_azuriteOwnsStorage | Should -BeFalse
    }

    It 'Start-Azurite respects a caller-supplied AzureWebJobsStorage' {
        $script:_azuriteOwnsStorage = $false
        $env:AzureWebJobsStorage = 'DefaultEndpointsProtocol=https;AccountName=real'
        Mock Resolve-AzuriteExe { $null }

        Start-Azurite

        Should -Invoke Resolve-AzuriteExe -Times 0 -Exactly
        $env:AzureWebJobsStorage | Should -Be 'DefaultEndpointsProtocol=https;AccountName=real'
    }

    It 'Start-Azurite reuses its own Azurite while it is still running' {
        $script:_azuriteOwnsStorage = $true
        $script:_azuriteProcess = [pscustomobject]@{ HasExited = $false }
        $env:AzureWebJobsStorage = 'UseDevelopmentStorage=true'

        Start-Azurite

        Should -Invoke Start-HiddenProcess -Times 0 -Exactly
        $env:AzureWebJobsStorage | Should -Be 'UseDevelopmentStorage=true'
        $script:_azuriteOwnsStorage | Should -BeTrue
    }

    It 'Stop-Engine clears the AzureWebJobsStorage value that pointed at the Azurite it killed' {
        $script:_azuriteOwnsStorage = $true
        $script:_azuriteProcess = $null
        $env:AzureWebJobsStorage = 'UseDevelopmentStorage=true'

        Stop-Engine

        $env:AzureWebJobsStorage | Should -BeNullOrEmpty
        $script:_azuriteOwnsStorage | Should -BeFalse
    }

    It 'Stop-Engine leaves a caller-supplied AzureWebJobsStorage alone' {
        $script:_azuriteOwnsStorage = $false
        $env:AzureWebJobsStorage = 'DefaultEndpointsProtocol=https;AccountName=real'

        Stop-Engine

        $env:AzureWebJobsStorage | Should -Be 'DefaultEndpointsProtocol=https;AccountName=real'
    }
}

# CI build 30903 "Not Parallelizable Unit Tests": every retry re-ran all 38 assemblies (~30s of
# testhost + coverage start-up each, ~19 min a pass) for 6 Dev2.Runtime.Tests failures, and
# re-tried the skipped tests too, so the job hit its 45-minute timeout on the third pass.
Describe 'Retry selection from the TRX' {
    BeforeAll {
        script:Import-TestRunFunction 'Get-FailedTestResults', 'Get-FailedTestNames', 'Get-FailedTestAssemblies'

        # Writes a minimal TRX: one UnitTestResult + TestDefinitions/UnitTest per row.
        function script:New-TrxFixture([object[]]$Rows) {
            $dir = Join-Path ([System.IO.Path]::GetTempPath()) ("trx-" + [Guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $dir | Out-Null
            $results = ''; $defs = ''
            foreach ($r in $Rows) {
                $id = [Guid]::NewGuid().ToString()
                $results += "<UnitTestResult testId=`"$id`" testName=`"$($r.Name)`" outcome=`"$($r.Outcome)`" />"
                $storageAttr = if ($r.Storage) { " storage=`"$($r.Storage)`"" } else { '' }
                $defs += "<UnitTest id=`"$id`" name=`"$($r.Name)`"$storageAttr />"
            }
            Set-Content -LiteralPath (Join-Path $dir 'run.trx') -Encoding UTF8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>$results</Results>
  <TestDefinitions>$defs</TestDefinitions>
</TestRun>
"@
            return $dir
        }

        $script:Runtime = '/home/vsts/work/1/linuxtests/dev2.runtime.tests.dll'
        $script:Queue   = '/home/vsts/work/1/linuxtests/queueprocessor/warewolf.execution.queueprocessor.tests.dll'
        $script:Audit   = '/home/vsts/work/1/linuxtests/warewolf.auditing.tests.dll'
        $script:AllAsm  = @('Dev2.Runtime.Tests.dll', 'QueueProcessor/Warewolf.Execution.QueueProcessor.Tests.dll',
                            'Warewolf.Auditing.Tests.dll', 'Dev2.Common.Tests.dll')
    }
    AfterEach { if ($script:dir) { Remove-Item -LiteralPath $script:dir -Recurse -Force -ErrorAction SilentlyContinue } }

    It 'retries Failed, Error and Timeout results but not Passed, NotExecuted or Inconclusive ones' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'PassedTest';       Outcome = 'Passed';       Storage = $script:Runtime }
            @{ Name = 'FailedTest';       Outcome = 'Failed';       Storage = $script:Runtime }
            @{ Name = 'SkippedTest';      Outcome = 'NotExecuted';  Storage = $script:Audit }
            @{ Name = 'InconclusiveTest'; Outcome = 'Inconclusive'; Storage = $script:Audit }
            @{ Name = 'ErrorTest';        Outcome = 'Error';        Storage = $script:Queue }
            @{ Name = 'TimeoutTest';      Outcome = 'Timeout';      Storage = $script:Runtime }
        )
        Get-FailedTestNames -TestResultsPath $script:dir | Should -Be @('FailedTest', 'ErrorTest', 'TimeoutTest')
    }

    It 'returns nothing to retry when only skipped and inconclusive results are left' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'PassedTest';       Outcome = 'Passed';       Storage = $script:Runtime }
            @{ Name = 'SkippedTest';      Outcome = 'NotExecuted';  Storage = $script:Audit }
            @{ Name = 'InconclusiveTest'; Outcome = 'Inconclusive'; Storage = $script:Audit }
        )
        @(Get-FailedTestNames -TestResultsPath $script:dir).Count | Should -Be 0
        @(Get-FailedTestAssemblies -TestResultsPath $script:dir -AssemblyPaths $script:AllAsm).Count | Should -Be 0
    }

    It 'narrows the retry to the assemblies holding a failure, keeping their relative paths' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'FailedTest';  Outcome = 'Failed';      Storage = $script:Runtime }
            @{ Name = 'FailedTwice'; Outcome = 'Failed';      Storage = $script:Runtime }
            @{ Name = 'ErrorTest';   Outcome = 'Error';       Storage = $script:Queue }
            @{ Name = 'SkippedTest'; Outcome = 'NotExecuted'; Storage = $script:Audit }
        )
        Get-FailedTestAssemblies -TestResultsPath $script:dir -AssemblyPaths $script:AllAsm |
            Should -Be @('Dev2.Runtime.Tests.dll', 'QueueProcessor/Warewolf.Execution.QueueProcessor.Tests.dll')
    }

    It 'matches a Windows TRX storage path against a backslash-relative assembly path' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'ErrorTest'; Outcome = 'Error'; Storage = 'd:\a\1\s\bin\servertests\queueprocessor\warewolf.execution.queueprocessor.tests.dll' }
        )
        Get-FailedTestAssemblies -TestResultsPath $script:dir -AssemblyPaths @('Dev2.Runtime.Tests.dll', 'QueueProcessor\Warewolf.Execution.QueueProcessor.Tests.dll') |
            Should -Be @('QueueProcessor\Warewolf.Execution.QueueProcessor.Tests.dll')
    }

    It 'falls back to every assembly when a failed test has no storage in the TRX' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'FailedTest';    Outcome = 'Failed' }
            @{ Name = 'FailedLocated'; Outcome = 'Failed'; Storage = $script:Runtime }
        )
        Get-FailedTestAssemblies -TestResultsPath $script:dir -AssemblyPaths $script:AllAsm | Should -Be $script:AllAsm
    }

    It 'falls back to every assembly when no failed storage matches the assembly list' {
        $script:dir = script:New-TrxFixture @(
            @{ Name = 'FailedTest'; Outcome = 'Failed'; Storage = '/elsewhere/Some.Other.Tests.dll' }
        )
        Get-FailedTestAssemblies -TestResultsPath $script:dir -AssemblyPaths $script:AllAsm | Should -Be $script:AllAsm
    }

    It 'stops the retry loop instead of re-running the whole filter when nothing is retryable' {
        $loopText = $script:TestRunAst.Extent.Text
        $loopText | Should -Match 'if \(-not \$TestsToRun\) \{ break \}'
        $loopText | Should -Match 'if \(\$retryAssemblies\) \{ \$asmList = \$retryAssemblies \}'
    }
}

Describe 'MSSQL container reuse across retries' {
    BeforeAll {
        script:Import-TestRunFunction 'Test-MssqlFixtureProbe', 'Test-ReusableMssqlContainer'
        $script:MssqlFullFixtureProbe = script:Get-TestRunScriptVariable 'MssqlFullFixtureProbe'
        # Offline stand-in for the docker CLI: `ps` lists $script:running, `exec` prints $script:probeOut.
        function script:docker {
            $script:dockerCalls += , ($args -join ' ')
            switch ($args[0]) {
                'ps'   { $script:running }
                'exec' { $script:probeOut }
            }
        }
    }
    BeforeEach { $script:dockerCalls = @() }

    It 'reads the fixture probe through sqlcmd padding: <Output> -> <Expected>' -TestCases @(
        @{ Output = @('', '1', ''); Expected = $true }
        @{ Output = @('1');         Expected = $true }
        @{ Output = @('', '0');     Expected = $false }
        @{ Output = @();            Expected = $false }
    ) {
        param($Output, $Expected)
        Test-MssqlFixtureProbe $Output | Should -Be $Expected
    }

    It 'reuses a running container that already holds the full fixture' {
        $script:running = 'sqlserver'; $script:probeOut = @('', '1')
        Test-ReusableMssqlContainer 'pwd' | Should -BeTrue
        ($script:dockerCalls | Where-Object { $_ -like 'exec sqlserver*' }) | Should -Match ([regex]::Escape('-P pwd'))
    }

    It 'does not reuse when no sqlserver container is running' {
        $script:running = $null; $script:probeOut = @('1')
        Test-ReusableMssqlContainer 'pwd' | Should -BeFalse
        ($script:dockerCalls | Where-Object { $_ -like 'exec*' }) | Should -BeNullOrEmpty
    }

    It 'does not reuse a running container without the fixture (or with a stale SA password)' {
        $script:running = 'sqlserver'; $script:probeOut = $null
        Test-ReusableMssqlContainer 'pwd' | Should -BeFalse
    }

    It 'only skips re-creating the container on retries' {
        $fn = $script:TestRunAst.Find({ param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Start-HostMSSQLServer' }, $true)
        $fn.Extent.Text | Should -Match 'if \(\$ReuseRunning -and \(Test-ReusableMssqlContainer \$saPwd\)\)'
        $script:TestRunAst.Extent.Text | Should -Match 'Start-HostMSSQLServer \$StartMSSQLServer -ReuseRunning:\(\$loop -gt 0\)'
    }
}
