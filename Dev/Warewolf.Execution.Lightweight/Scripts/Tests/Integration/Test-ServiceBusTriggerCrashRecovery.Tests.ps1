#Requires -Version 7.0

<#
    Pester 5 test suite for Test-ServiceBusTriggerCrashRecovery.ps1's own pure helpers,
    dot-sourced via -LoadFunctionsOnly so this runs with NO docker, SQL Edge, Service Bus
    emulator, func CLI or dotnet SDK dependency (same convention as
    Test-ShovelBridgeE2E.Tests.ps1 / Deploy-WwExecutionEngine.Tests.ps1).

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Integration/Test-ServiceBusTriggerCrashRecovery.Tests.ps1

    Scope: the two helpers that failed CI, not the harness itself. The crash/recovery phases
    are live-infra behaviour proven by actually running the script, exactly as before.

    Both suites are regressions for build 30657 (2026-09-13), where the Linux CI job died with
    nothing in the log but "dotnet build failed for the Execution Engine (exit 1)":
      * Get-WwEngineBuildOutputPath — the staged deploy was read from bin/output, which
        `dotnet build` never produces; it existed only on a dev box holding a stale publish.
      * Invoke-DotnetBuild — the build was piped to Out-Null, so the MSB3030 that actually
        broke the build (a case-mismatched csproj path, harmless on Windows, fatal on the
        case-sensitive agent) never reached the log and cost a full pipeline round-trip.

    `dotnet` is shadowed with a tracking *function* (functions win over applications in command
    resolution), so the suite runs identically with or without the .NET SDK installed and never
    shells out to a real build.
#>

BeforeAll {
    $script:TargetScript = Join-Path $PSScriptRoot 'Test-ServiceBusTriggerCrashRecovery.ps1'
    . $script:TargetScript -LoadFunctionsOnly
}

Describe 'Test-ServiceBusTriggerCrashRecovery — Get-WwEngineBuildOutputPath (pure, no build)' {

    # NB: no angle brackets in test names — Pester 5 treats <Name> as a data placeholder and
    # fails the test looking for a variable of that name.
    It 'resolves the engine build output to bin/Configuration/TargetFramework under the engine directory' {
        # Separators are normalised before comparing: Join-Path emits the platform's own, so a
        # literal expectation would assert the host OS rather than the path structure.
        $path = Get-WwEngineBuildOutputPath -EngineDir '/engine'
        ($path -replace '\\', '/') | Should -Be '/engine/bin/Debug/net10.0'
    }

    It 'never resolves to the legacy bin/output path' {
        # Regression for build 30657's follow-on failure: bin/output is not a dotnet build
        # artefact at all, so the staging step threw "Engine build output not found".
        $path = Get-WwEngineBuildOutputPath -EngineDir '/engine'
        Split-Path -Path $path -Leaf   | Should -Be 'net10.0'
        Split-Path -Path $path -Parent | Should -Not -BeLike '*output'
    }

    It 'produces a path the script can actually Test-Path on this platform' {
        # The real assertion behind the separator handling: build the layout dotnet would
        # leave behind, then prove the helper points the staging step at it.
        $engineDir = Join-Path $TestDrive 'Warewolf.Execution.Lightweight'
        $expected  = Join-Path $engineDir 'bin' | Join-Path -ChildPath 'Debug' | Join-Path -ChildPath 'net10.0'
        New-Item -ItemType Directory -Path $expected -Force | Out-Null

        $path = Get-WwEngineBuildOutputPath -EngineDir $engineDir
        Test-Path -LiteralPath $path | Should -BeTrue
    }

    It 'honours a non-default Configuration and TargetFramework' {
        $path = Get-WwEngineBuildOutputPath -EngineDir '/engine' -Configuration 'Release' -TargetFramework 'net11.0'
        ($path -replace '\\', '/') | Should -Be '/engine/bin/Release/net11.0'
    }

    It 'requires an engine directory' {
        # Asserted against the parameter metadata, not by calling it bare: a missing mandatory
        # parameter PROMPTS on an interactive host, which would print a prompt banner into the
        # CI log (and only throws because the agent's pwsh is non-interactive).
        $parameter = (Get-Command Get-WwEngineBuildOutputPath).Parameters['EngineDir']
        $mandatory = $parameter.Attributes |
            Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } |
            ForEach-Object { $_.Mandatory }
        $mandatory | Should -Contain $true
    }
}

Describe 'Test-ServiceBusTriggerCrashRecovery — Invoke-DotnetBuild (shimmed dotnet, no SDK needed)' {

    BeforeAll {
        # Tracking shim: functions win over applications, so nothing here ever runs a real build.
        function script:dotnet {
            $script:DotnetArgs = @($args)
            if ($null -ne $script:DotnetOutput) { $script:DotnetOutput }
            $global:LASTEXITCODE = $script:DotnetExitCode
        }

        function script:Invoke-BuildCapturingHostOutput {
            <#
            Runs Invoke-DotnetBuild with the information stream redirected to a file (a throwing
            pipeline loses in-memory capture, a file redirect flushes as it goes) and returns
            both what was echoed and what was thrown.
            #>
            param([string] $Description = 'the Execution Engine', [int] $TailLines = 40)
            $logPath = Join-Path $TestDrive "build-$([Guid]::NewGuid().ToString('N')).log"
            $thrown  = $null
            try {
                Invoke-DotnetBuild -ProjectPath 'Engine.csproj' -Description $Description -TailLines $TailLines 6> $logPath
            } catch {
                $thrown = $_
            }
            $echoed = if (Test-Path -LiteralPath $logPath) { @(Get-Content -LiteralPath $logPath) } else { @() }
            [pscustomobject]@{ Echoed = $echoed; Thrown = $thrown }
        }
    }

    BeforeEach {
        $script:DotnetArgs     = @()
        $script:DotnetOutput   = $null
        $script:DotnetExitCode = 0
    }

    AfterAll {
        $global:LASTEXITCODE = 0
    }

    It 'passes the project through to a quiet Debug build' {
        Invoke-DotnetBuild -ProjectPath 'Engine.csproj' -Description 'the Execution Engine'
        $script:DotnetArgs | Should -Be @('build', 'Engine.csproj', '-c', 'Debug', '--nologo', '-v', 'quiet')
    }

    It 'returns quietly and echoes nothing when the build succeeds' {
        $script:DotnetOutput   = @('warning CS8618: Non-nullable property', 'Build succeeded.')
        $script:DotnetExitCode = 0

        $run = Invoke-BuildCapturingHostOutput
        $run.Thrown | Should -BeNullOrEmpty
        $run.Echoed | Should -BeNullOrEmpty
    }

    It 'throws naming the project and the build exit code when the build fails' {
        $script:DotnetOutput   = @('Engine.csproj(1,1): error MSB3030: Could not copy the file "ssl generation/TrustCertificate.bat" because it was not found.')
        $script:DotnetExitCode = 1

        $run = Invoke-BuildCapturingHostOutput -Description 'the Execution Engine'
        $run.Thrown                    | Should -Not -BeNullOrEmpty
        $run.Thrown.Exception.Message  | Should -BeLike '*the Execution Engine*'
        $run.Thrown.Exception.Message  | Should -BeLike '*exit 1*'
    }

    It 'never fails with an empty log — the build output is surfaced, not discarded' {
        # The whole point of the fix: build 30657's log carried "exit 1" and nothing else.
        $script:DotnetOutput   = @('Engine.csproj(1,1): error MSB3030: Could not copy the file.')
        $script:DotnetExitCode = 1

        $run = Invoke-BuildCapturingHostOutput
        @($run.Echoed).Count | Should -BeGreaterThan 0
        ($run.Echoed -join [Environment]::NewLine) | Should -BeLike '*MSB3030*'
    }

    It 'puts the first error in the exception message, where the CI step summary shows it' {
        $script:DotnetOutput   = @(
            'Engine.csproj(1,1): error MSB3030: Could not copy the file.',
            'Other.csproj(2,2): error CS1002: ; expected'
        )
        $script:DotnetExitCode = 1

        $run = Invoke-BuildCapturingHostOutput
        $run.Thrown.Exception.Message | Should -BeLike '*MSB3030*'
    }

    It 'surfaces the error line even when hundreds of warnings would have crowded it out of the tail' {
        # This solution emits ~850 warnings at -v quiet, so a plain tail of 40 lines would show
        # nothing but warnings — the failure that actually mattered scrolls past.
        $script:DotnetOutput   = @(1..850 | ForEach-Object { "Engine.csproj($_,1): warning CS8618: Non-nullable property" }) +
                                 @('Engine.csproj(999,1): error MSB3030: Could not copy the file.') +
                                 @(1..60 | ForEach-Object { "Engine.csproj($_,2): warning CS8600: Converting null literal" })
        $script:DotnetExitCode = 1

        $run    = Invoke-BuildCapturingHostOutput
        $echoed = $run.Echoed -join [Environment]::NewLine
        $echoed                       | Should -BeLike '*MSB3030*'
        $echoed                       | Should -Not -BeLike '*warning CS8600*'
        $run.Thrown.Exception.Message | Should -BeLike '*MSB3030*'
    }

    It 'falls back to the output tail when the failure produced no recognisable error line' {
        $script:DotnetOutput   = @('MSBuild version 17.0', 'Killed', 'The build stopped unexpectedly.')
        $script:DotnetExitCode = 137

        $run = Invoke-BuildCapturingHostOutput
        ($run.Echoed -join [Environment]::NewLine) | Should -BeLike '*The build stopped unexpectedly.*'
        $run.Thrown.Exception.Message              | Should -BeLike '*exit 137*'
        $run.Thrown.Exception.Message              | Should -Not -BeLike '*First error:*'
    }

    It 'caps the echoed output at TailLines' {
        $script:DotnetOutput   = @(1..200 | ForEach-Object { "line $_" })
        $script:DotnetExitCode = 1

        $run = Invoke-BuildCapturingHostOutput -TailLines 10
        @($run.Echoed).Count | Should -Be 10
    }
}
