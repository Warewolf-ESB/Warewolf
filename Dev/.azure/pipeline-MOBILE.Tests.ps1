#Requires -Version 7.0
<#
    Pester 5 suite: the Mobile Studio APK is built, signed and published only the way it was meant to be.
    That means pipeline.yml builds it in Compile_Release (main branches only), and pipeline-MOBILE.yml
    uploads it to the public release container without ever replacing a published version.

        Invoke-Pester -Path ./pipeline-MOBILE.Tests.ps1

    A pipeline cannot be run locally, so this suite checks what CAN be proven offline:

      * both YAML files parse and every embedded PowerShell script is syntactically valid;
      * pipeline.yml runs the app tests in the Test stage and fails on a red or missing result;
      * pipeline.yml compiles the iOS host (device and simulator frameworks, shared tests) on macOS,
        and builds the XcodeGen host app (iosApp/project.yml) for a device, unsigned;
      * pipeline.yml builds the APK in Compile_Release with the stamped version, the secure-file
        keystore and the signing variables, and publishes the artifact pipeline-MOBILE.yml downloads;
        the job is emitted on main branches only, because Azure DevOps validates a secure file at
        compile time even for a skipped stage (run 31115 was invalid on a feature branch);
      * pipeline-MOBILE.yml is triggered by, and its Release stage gated on, exactly the main branches
        pipeline.yml's IsMainBranch covers;
      * 'Read APK version' - EXECUTED against a fake artifact - accepts a valid one and rejects a
        missing or 0.0.x version;
      * 'Upload APK to Azure Storage' - EXECUTED with az stubbed - never overwrites a versioned APK and
        only ever moves WarewolfStudio-latest.apk forward.

    Requires the powershell-yaml module (the whole suite is skipped, not failed, without it).
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:MobileYaml = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'pipeline-MOBILE.yml') -Raw
    $script:CiYaml     = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'pipeline.yml') -Raw

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Mobile = ConvertFrom-Yaml -Ordered $script:MobileYaml
        $script:Ci     = ConvertFrom-Yaml -Ordered $script:CiYaml
    }

    function script:Get-StepScript {
        param($Step)
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        if ($Step.Contains('inputs') -and $Step['inputs'].Contains('inlineScript')) { return [string]$Step['inputs']['inlineScript'] }
        return $null
    }

    function script:Get-Stage {
        param($Doc, [string] $Name)
        @($Doc['stages']) | Where-Object { $_['stage'] -eq $Name } | Select-Object -First 1
    }

    # A stage's jobs, including those inside a compile-time '${{ if ... }}:' insertion.
    function script:Get-Jobs {
        param($Doc, [string] $Stage)
        foreach ($entry in @((Get-Stage $Doc $Stage)['jobs'])) {
            if ($entry.Contains('job')) { $entry; continue }
            foreach ($key in @($entry.Keys)) { if ($key -like '${{*') { @($entry[$key]) } }
        }
    }

    function script:Get-Job {
        param($Doc, [string] $Stage, [string] $Name)
        @(Get-Jobs $Doc $Stage) | Where-Object { $_['job'] -eq $Name } | Select-Object -First 1
    }

    function script:Find-Step {
        param($Job, [string] $DisplayName)
        @($Job['steps']) | Where-Object { $_['displayName'] -eq $DisplayName } | Select-Object -First 1
    }

    # Replaces every Azure Pipelines $(Macro) with its value. A PowerShell subexpression such as
    # $(if (...) {...}) is untouched: no ')' directly follows a macro-like name.
    function script:Expand-Macros {
        param([string] $Script, [hashtable] $Values)
        [regex]::Replace($Script, '\$\(([A-Za-z][A-Za-z0-9_.]*)\)', {
            param($m)
            $name = $m.Groups[1].Value
            if (-not $Values.ContainsKey($name)) { throw "Test supplied no value for pipeline macro `$($name)." }
            [string]$Values[$name]
        })
    }

    # Runs -Body in a child pwsh, so a 'throw' or 'exit' cannot end this Pester run.
    function script:Invoke-StepProcess {
        param([string] $Body, [hashtable] $Environment = @{})
        $file = Join-Path ([IO.Path]::GetTempPath()) ("wwmobile-step-" + [guid]::NewGuid().ToString('N') + '.ps1')
        Set-Content -LiteralPath $file -Value $Body
        $saved = @{}
        foreach ($k in $Environment.Keys) { $saved[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $Environment[$k]) }
        try {
            $output = & (Get-Process -Id $PID).Path -NoProfile -NonInteractive -File $file 2>&1 | ForEach-Object { [string]$_ }
            [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
        } finally {
            foreach ($k in $saved.Keys) { [Environment]::SetEnvironmentVariable($k, $saved[$k]) }
            Remove-Item -LiteralPath $file -Force -ErrorAction SilentlyContinue
        }
    }

    $script:MainBranches = @('develop', '7580*', '7232*')
    $script:MainBranchCondition = "and(succeeded(), or(eq(variables['resources.pipeline.ci.sourceBranch'], 'refs/heads/develop'), startsWith(variables['resources.pipeline.ci.sourceBranch'], 'refs/heads/7580'), startsWith(variables['resources.pipeline.ci.sourceBranch'], 'refs/heads/7232')))"
}

Describe 'pipeline-MOBILE - structure' -Skip:(-not $script:HaveYaml) {

    It 'parses, and every embedded PowerShell script in it and in the APK jobs is syntactically valid' {
        $jobs = @(@((Get-Stage $script:Mobile 'Release')['jobs'])) + (Get-Job $script:Ci 'Compile_Release' 'build_release')
        $bad = foreach ($job in $jobs) {
            foreach ($step in @($job['steps'])) {
                $body = Get-StepScript $step
                if (-not $body) { continue }
                $errors = $null
                [System.Management.Automation.Language.Parser]::ParseInput($body, [ref]$null, [ref]$errors) | Out-Null
                if ($errors) { "$($job['job']) / $($step['displayName']): $($errors[0].Message)" }
            }
        }
        $bad | Should -BeNullOrEmpty
    }

    It 'never runs on a commit or PR of its own, only on a Warewolf Compile_Release on a main branch' {
        $script:Mobile['trigger'] | Should -Be 'none'
        $script:Mobile['pr'] | Should -Be 'none'
        $ci = @($script:Mobile['resources']['pipelines']) | Where-Object { $_['pipeline'] -eq 'ci' }
        $ci['source'] | Should -Be 'Warewolf'
        @($ci['trigger']['branches']['include']) | Should -Be $script:MainBranches
        @($ci['trigger']['stages']) | Should -Be @('Compile_Release')
    }

    It 'triggers on exactly the branches pipeline.yml treats as main branches (IsMainBranch)' {
        $isMain = [regex]::Match($script:CiYaml, "(?m)^\s*\$\{\{ if (.+) \}\}:\s*\r?\n\s*IsMainBranch: 'true'").Groups[1].Value
        $isMain | Should -Not -BeNullOrEmpty
        $exact  = @([regex]::Matches($isMain, "eq\(variables\['Build\.SourceBranch'\], 'refs/heads/([^']+)'\)") | ForEach-Object { $_.Groups[1].Value })
        $prefix = @([regex]::Matches($isMain, "startsWith\(variables\['Build\.SourceBranch'\], 'refs/heads/([^']+)'\)") | ForEach-Object { "$($_.Groups[1].Value)*" })
        ($exact + $prefix) | Should -Be $script:MainBranches
    }

    It 'gates the Release stage on the same main branches of the triggering run' {
        (Get-Stage $script:Mobile 'Release')['condition'] | Should -Be $script:MainBranchCondition
    }

    It 'names the storage service connection as a literal (authorized at compile time)' {
        $upload = Find-Step (Get-Job $script:Mobile 'Release' 'Publish_Apk') 'Upload APK to Azure Storage'
        $upload['inputs']['azureSubscription'] | Should -Be 'AzureStorage-WarewolfReleases'
    }

    It 'overwrites only WarewolfStudio-latest.apk, never a versioned APK' {
        $upload = Get-StepScript (Find-Step (Get-Job $script:Mobile 'Release' 'Publish_Apk') 'Upload APK to Azure Storage')
        $uploads = [regex]::Matches($upload, '(?s)az storage blob upload\b.*?--name (\$\w+).*?--overwrite (\w+)')
        $uploads.Count | Should -Be 2
        @($uploads | ForEach-Object { "$($_.Groups[1].Value)=$($_.Groups[2].Value)" }) | Should -Be @('$versionedName=false', '$latestName=true')
    }
}

Describe 'pipeline.yml - Mobile Studio jobs' -Skip:(-not $script:HaveYaml) {

    It 'runs the shared app tests in the Test stage and fails on a red or missing result' {
        $job = Get-Job $script:Ci 'Test' 'Mobile_Studio_Tests'
        $job | Should -Not -BeNullOrEmpty
        $job['pool']['vmImage'] | Should -Be 'ubuntu-latest'
        [string]$job['steps'][1]['bash'] | Should -Match './gradlew :shared:app:desktopTest'
        $publish = Find-Step $job 'Publish Mobile Studio test results'
        $publish['inputs']['testResultsFiles'] | Should -Match 'shared/app/build/test-results/desktopTest/\*\.xml$'
        $publish['inputs']['failTaskOnFailedTests'] | Should -BeTrue
        $publish['inputs']['failTaskOnMissingResultsFile'] | Should -BeTrue
    }

    It 'compiles the iOS host on a macOS agent, for device and simulator, with the shared tests' {
        $job = Get-Job $script:Ci 'Test' 'Mobile_Studio_iOS_Build'
        $job | Should -Not -BeNullOrEmpty
        $job['pool']['vmImage'] | Should -Match '^macOS-'
        $build = Find-Step $job 'Compile Mobile Studio iOS frameworks and tests'
        [string]$build['bash'] | Should -Match 'set -euo pipefail'
        [string]$build['bash'] | Should -Match ':shared:app:linkDebugFrameworkIosArm64'
        [string]$build['bash'] | Should -Match ':shared:app:linkDebugFrameworkIosSimulatorArm64'
        [string]$build['bash'] | Should -Match ':shared:app:compileTestKotlinIosSimulatorArm64'
        $build['workingDirectory'] | Should -Match 'Mobile/Warewolf.Studio$'
    }

    It 'generates the Xcode host from iosApp/project.yml and builds it for a device, unsigned' {
        $job = Get-Job $script:Ci 'Test' 'Mobile_Studio_iOS_Build'
        $app = Find-Step $job 'Build Mobile Studio iOS app (unsigned)'
        $app | Should -Not -BeNullOrEmpty
        $app['workingDirectory'] | Should -Match 'Mobile/Warewolf.Studio/iosApp$'
        [string]$app['bash'] | Should -Match 'set -euo pipefail'
        [string]$app['bash'] | Should -Match 'JAVA_HOME='
        [string]$app['bash'] | Should -Match 'xcodegen generate'
        [string]$app['bash'] | Should -Match '-sdk iphoneos'
        [string]$app['bash'] | Should -Match 'CODE_SIGNING_ALLOWED=NO'
        Test-Path -LiteralPath (Join-Path $PSScriptRoot '../../Mobile/Warewolf.Studio/iosApp/project.yml') | Should -BeTrue
    }

    It 'never commits the generated Xcode project' {
        $ignore = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../Mobile/Warewolf.Studio/.gitignore') -Raw
        $ignore | Should -Match '(?m)^iosApp/\*\.xcodeproj/$'
    }

    It 'emits the APK job only on main branches, so its secure file is never validated on a feature branch (run 31115)' {
        $guarded = @(@((Get-Stage $script:Ci 'Compile_Release')['jobs']) | Where-Object { -not $_.Contains('job') })
        $guarded.Count | Should -Be 1
        $key = @($guarded[0].Keys)[0]
        $key | Should -Be "`${{ if eq(variables['IsMainBranch'], 'true') }}"
        @($guarded[0][$key] | ForEach-Object { $_['job'] }) | Should -Be @('build_release_apk')
    }

    It 'builds the APK after build_release, with its stamped version as an output variable' {
        $job = Get-Job $script:Ci 'Compile_Release' 'build_release_apk'
        $job | Should -Not -BeNullOrEmpty
        $job['dependsOn'] | Should -Be 'build_release'
        $job['variables']['ReleaseVersion'] | Should -Be "`$[ dependencies.build_release.outputs['releaseVersion.Value'] ]"
        $share = @((Get-Job $script:Ci 'Compile_Release' 'build_release')['steps']) | Where-Object { $_['name'] -eq 'releaseVersion' }
        Get-StepScript $share | Should -Match 'task\.setvariable variable=Value;isOutput=true'
    }

    It 'signs with the secure-file keystore and the signing variables, and refuses an unsigned APK' {
        $job = Get-Job $script:Ci 'Compile_Release' 'build_release_apk'
        $keystore = @($job['steps']) | Where-Object { $_['task'] -eq 'DownloadSecureFile@1' }
        $keystore['inputs']['secureFile'] | Should -Be 'WarewolfStudio-upload.jks'
        $build = Find-Step $job 'Build signed release APK'
        [string]$build['bash'] | Should -Match ':androidApp:assembleRelease'
        [string]$build['bash'] | Should -Match "-PwwVersionName='\`$\(ReleaseVersion\)' -PwwVersionCode='\`$\(Build\.BuildId\)'"
        $build['env']['WW_ANDROID_KEYSTORE'] | Should -Be "`$($($keystore['name']).secureFilePath)"
        foreach ($k in 'WW_ANDROID_KEYSTORE_PASSWORD', 'WW_ANDROID_KEY_ALIAS', 'WW_ANDROID_KEY_PASSWORD') { $build['env'][$k] | Should -Match '^\$\(\w+\)$' -Because $k }
        [string](Find-Step $job 'Verify APK signature and stage artifact')['bash'] | Should -Match 'androidApp-release\.apk'
    }

    It 'publishes the artifact (APK + FullVersionString) that pipeline-MOBILE.yml downloads' {
        $job = Get-Job $script:Ci 'Compile_Release' 'build_release_apk'
        $published = (Find-Step $job 'Publish MobileStudioApk')['inputs']['artifactName']
        $download = @((Get-Job $script:Mobile 'Release' 'Publish_Apk')['steps']) | Where-Object { $_.Contains('download') }
        $download['download'] | Should -Be 'ci'
        $download['artifact'] | Should -Be $published
        $stage = [string](Find-Step $job 'Verify APK signature and stage artifact')['bash']
        $stage | Should -Match 'WarewolfStudio-\$\(ReleaseVersion\)\.apk'
        $stage | Should -Match '/FullVersionString'
    }
}

Describe 'pipeline-MOBILE - Read APK version' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:ReadStep = Get-StepScript (Find-Step (Get-Job $script:Mobile 'Release' 'Publish_Apk') 'Read APK version')
        function script:Invoke-ReadStep {
            param([string] $Workspace)
            Invoke-StepProcess (Expand-Macros $script:ReadStep @{
                'Pipeline.Workspace' = $Workspace
                'resources.pipeline.ci.sourceBranch' = 'refs/heads/develop'
                'resources.pipeline.ci.runName' = '31100 (3.0.3.21)'
            })
        }
        function script:New-Artifact {
            param([string] $Version, [string] $ApkVersion = $Version)
            $ws = Join-Path ([IO.Path]::GetTempPath()) ("wwmobile-" + [guid]::NewGuid().ToString('N'))
            $dir = New-Item -ItemType Directory -Path (Join-Path $ws 'ci/MobileStudioApk') -Force
            Set-Content -LiteralPath (Join-Path $dir 'FullVersionString') -Value $Version -NoNewline
            Set-Content -LiteralPath (Join-Path $dir "WarewolfStudio-$ApkVersion.apk") -Value 'apk'
            $ws
        }
    }

    AfterEach { if ($script:Ws) { Remove-Item -LiteralPath $script:Ws -Recurse -Force -ErrorAction SilentlyContinue } }

    It 'sets ApkVersion and ApkPath from a valid artifact' {
        $script:Ws = New-Artifact '3.0.3.21'
        $r = Invoke-ReadStep $script:Ws
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match '##vso\[task\.setvariable variable=ApkVersion\]3\.0\.3\.21'
        $r.Output | Should -Match '##vso\[task\.setvariable variable=ApkPath\].*WarewolfStudio-3\.0\.3\.21\.apk'
    }

    It 'fails when the APK for the recorded version is missing' {
        $script:Ws = New-Artifact '3.0.3.21' -ApkVersion '3.0.3.20'
        $r = Invoke-ReadStep $script:Ws
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match 'WarewolfStudio-3\.0\.3\.21\.apk not found'
    }

    It 'fails on a 0.0.x version' {
        $script:Ws = New-Artifact '0.0.0.0'
        $r = Invoke-ReadStep $script:Ws
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match 'no valid version'
    }
}

Describe 'pipeline-MOBILE - Upload APK to Azure Storage' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:UploadStep = Get-StepScript (Find-Step (Get-Job $script:Mobile 'Release' 'Publish_Apk') 'Upload APK to Azure Storage')
        # The step imports Dev/Warewolf.Execution.Lightweight/Scripts/WwAzCli.psm1 from the sources
        # directory; a fake one there records every az call instead of reaching Azure. Blobs already
        # in the container are listed in $env:AZ_EXISTING; latest's metadata version is $env:AZ_LATEST_VERSION.
        $script:AzStub = @'
function Invoke-WwAz {
    $line = $args -join ' '
    Add-Content -LiteralPath $env:AZ_LOG -Value $line
    $name = $args[[array]::IndexOf($args, '--name') + 1]
    $existing = @($env:AZ_EXISTING -split ',' | Where-Object { $_ })
    $global:LASTEXITCODE = 0
    if ($line -like 'storage blob exists*') { if ($existing -contains $name) { 'true' } else { 'false' }; return }
    if ($line -like 'storage blob metadata show*') { $env:AZ_LATEST_VERSION; return }
    if ($line -like 'storage blob upload*' -and $line -like '*--overwrite false*' -and $existing -contains $name) {
        $global:LASTEXITCODE = 1; return
    }
}
Export-ModuleMember -Function Invoke-WwAz
'@
        function script:Invoke-UploadStep {
            param([string[]] $Existing = @(), [string] $LatestVersion = '', [string] $Version = '3.0.3.21')
            Invoke-StepProcess (Expand-Macros $script:UploadStep @{
                'Build.SourcesDirectory' = $script:Src
                AzureStorageAccountName = 'stwwreleases'
                AzureStorageContainerName = 'releases'
                ApkVersion = $Version
                ApkPath = '/tmp/WarewolfStudio.apk'
            }) -Environment @{ AZ_LOG = $script:AzLog; AZ_EXISTING = ($Existing -join ','); AZ_LATEST_VERSION = $LatestVersion }
        }
        function script:Get-Uploads {
            if (-not (Test-Path -LiteralPath $script:AzLog)) { return @() }
            @(Get-Content -LiteralPath $script:AzLog | Where-Object { $_ -like 'storage blob upload*' } | ForEach-Object {
                $a = $_ -split ' '
                "$($a[[array]::IndexOf($a, '--name') + 1]) overwrite=$($a[[array]::IndexOf($a, '--overwrite') + 1])"
            })
        }
    }

    BeforeEach {
        $script:Src = Join-Path ([IO.Path]::GetTempPath()) ("wwmobile-src-" + [guid]::NewGuid().ToString('N'))
        $scripts = New-Item -ItemType Directory -Path (Join-Path $script:Src 'Dev/Warewolf.Execution.Lightweight/Scripts') -Force
        Set-Content -LiteralPath (Join-Path $scripts 'WwAzCli.psm1') -Value $script:AzStub
        $script:AzLog = Join-Path $script:Src 'az.log'
    }
    AfterEach { Remove-Item -LiteralPath $script:Src -Recurse -Force -ErrorAction SilentlyContinue }

    It 'publishes the version and creates latest when there is no latest yet' {
        $r = Invoke-UploadStep
        $r.ExitCode | Should -Be 0 -Because $r.Output
        Get-Uploads | Should -Be @('WarewolfStudio-3.0.3.21.apk overwrite=false', 'WarewolfStudio-latest.apk overwrite=true')
    }

    It 'moves latest forward from an older version' {
        $r = Invoke-UploadStep -Existing 'WarewolfStudio-latest.apk' -LatestVersion '3.0.3.20'
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match 'now points at 3\.0\.3\.21 \(was: 3\.0\.3\.20\)'
        Get-Uploads | Should -Be @('WarewolfStudio-3.0.3.21.apk overwrite=false', 'WarewolfStudio-latest.apk overwrite=true')
    }

    It 'leaves latest alone when it is already a higher version (an older build finished late)' {
        $r = Invoke-UploadStep -Existing 'WarewolfStudio-latest.apk' -LatestVersion '3.0.3.100'
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match 'already 3\.0\.3\.100'
        Get-Uploads | Should -Be @('WarewolfStudio-3.0.3.21.apk overwrite=false')
    }

    It 'fails, and leaves latest alone, when the version is already published' {
        $r = Invoke-UploadStep -Existing 'WarewolfStudio-3.0.3.21.apk', 'WarewolfStudio-latest.apk' -LatestVersion '3.0.3.20'
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match 'APK upload failed \(WarewolfStudio-3\.0\.3\.21\.apk\)'
        Get-Uploads | Should -Be @('WarewolfStudio-3.0.3.21.apk overwrite=false')
    }
}
