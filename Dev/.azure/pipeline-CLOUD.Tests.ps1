#Requires -Version 7.0
<#
    Pester 5 suite: pipeline-CLOUD.yml only versions, tags and publishes a release the way it was
    meant to - one unique version per release-branch build, never a feature branch.

        Invoke-Pester -Path ./pipeline-CLOUD.Tests.ps1

    Build 31073 (2026-10-29) is the regression this guards: a manual run of a feature branch minted
    3.0.3.16 from the highest tag in ITS OWN history (3.0.3.15) although 3.0.3.16-.20 were already
    released, ignored "fatal: tag '3.0.3.16' already exists", moved the rolling 3/3.0/3.0.3 tags
    onto the feature branch, overwrote the published 3.0.3.16 zips, and made itself the latest
    version on the release page - the Release stage had no condition at all.

    A pipeline cannot be run locally, so this suite checks what CAN be proven offline:

      * the YAML parses and every embedded PowerShell script is syntactically valid;
      * the Release stage and the tag step run on release branches only (develop, 7580* - the
        branches the trigger includes), never on Build.Reason alone;
      * no release zip is uploaded with --overwrite true;
      * 'Apply Version from Git Tags' - EXECUTED in a throwaway git repo - mints the highest tag in
        the WHOLE repo + 1, and reuses a tag already at HEAD;
      * 'Tag Source and Push Version Tags to GitLab' - EXECUTED in a throwaway git repo with
        'git push' stubbed - tags and force-moves the rolling tags, refuses a version tagged on
        another commit, re-tags nothing on an already-tagged HEAD, and fails on a failed push
        without echoing the GitLab credential.

    Requires the powershell-yaml module (the whole suite is skipped, not failed, without it) and
    git on PATH.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:YamlPath = Join-Path $PSScriptRoot 'pipeline-CLOUD.yml'
    $script:Yaml     = Get-Content -LiteralPath $script:YamlPath -Raw

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Doc = ConvertFrom-Yaml -Ordered $script:Yaml
    }

    function script:Get-StepScript {
        param($Step)
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        if ($Step.Contains('inputs') -and $Step['inputs'].Contains('inlineScript')) { return [string]$Step['inputs']['inlineScript'] }
        return $null
    }

    function script:Get-Stage {
        param([string] $Name)
        @($script:Doc['stages']) | Where-Object { $_['stage'] -eq $Name } | Select-Object -First 1
    }

    function script:Find-Step {
        param([string] $DisplayName)
        foreach ($stage in @($script:Doc['stages'])) {
            foreach ($job in @($stage['jobs'])) {
                foreach ($step in @($job['steps'])) { if ($step['displayName'] -eq $DisplayName) { return $step } }
            }
        }
    }

    # Replaces every Azure Pipelines $(Macro) with its value. A PowerShell subexpression such as
    # $($x -join ',') is untouched: its first character after '(' is not a macro-name character.
    function script:Expand-Macros {
        param([string] $Script, [hashtable] $Values)
        [regex]::Replace($Script, '\$\(([A-Za-z][A-Za-z0-9_.]*)\)', {
            param($m)
            $name = $m.Groups[1].Value
            if (-not $Values.ContainsKey($name)) { throw "Test supplied no value for pipeline macro `$($name)." }
            [string]$Values[$name]
        })
    }

    # Runs git in -Repo, throwing on failure. Commits carry a fixed identity so no global config is needed.
    function script:Invoke-RepoGit {
        param([string] $Repo, [Parameter(ValueFromRemainingArguments)] [string[]] $GitArgs)
        $out = git -C $Repo -c user.name=Test -c user.email=test@example.invalid -c commit.gpgsign=false -c tag.gpgsign=false @GitArgs 2>&1
        if ($LASTEXITCODE -ne 0) { throw "git $($GitArgs -join ' ') failed: $out" }
        $out
    }

    # A repo shaped like build 31073's: main has 3.0.3.15 then an untagged HEAD; a side branch
    # forked at 3.0.3.15 carries 3.0.3.16 and 3.0.3.20, none of them reachable from HEAD.
    function script:New-VersionRepo {
        $repo = Join-Path ([IO.Path]::GetTempPath()) ("wwcloud-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $repo 'Dev') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $repo 'Dev/AssemblyCommonInfo.cs') -Value '[assembly: AssemblyVersion("0.0.0.0")]' -NoNewline
        Set-Content -LiteralPath (Join-Path $repo 'Dev/AssemblyCommonInfo.fs') -Value '[<assembly: AssemblyVersion("0.0.0.0")>]' -NoNewline
        Invoke-RepoGit $repo init -q -b main | Out-Null
        Invoke-RepoGit $repo add -A | Out-Null
        Invoke-RepoGit $repo commit -q -m base | Out-Null
        Invoke-RepoGit $repo tag 3.0.3.15 | Out-Null
        Invoke-RepoGit $repo checkout -q -b side | Out-Null
        Invoke-RepoGit $repo commit -q --allow-empty -m side1 | Out-Null
        Invoke-RepoGit $repo tag 3.0.3.16 | Out-Null
        Invoke-RepoGit $repo commit -q --allow-empty -m side2 | Out-Null
        Invoke-RepoGit $repo tag 3.0.3.20 | Out-Null
        foreach ($t in '3', '3.0', '3.0.3') { Invoke-RepoGit $repo tag $t | Out-Null }
        Invoke-RepoGit $repo checkout -q main | Out-Null
        Invoke-RepoGit $repo commit -q --allow-empty -m head | Out-Null
        $repo
    }

    function script:Get-TagSha {
        param([string] $Repo, [string] $Tag)
        (git -C $Repo rev-parse -q --verify "refs/tags/$Tag^{commit}")
    }

    # Runs -Body in a child pwsh (both steps end in 'exit', which would end this Pester run if
    # dot-sourced). -Prelude is prepended - the tag step's tests use it to stub 'git push'.
    function script:Invoke-StepProcess {
        param([string] $Body, [string] $Prelude = '', [hashtable] $Environment = @{})
        $file = Join-Path ([IO.Path]::GetTempPath()) ("wwcloud-step-" + [guid]::NewGuid().ToString('N') + '.ps1')
        Set-Content -LiteralPath $file -Value ($Prelude + "`n" + $Body)
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

    $script:ReleaseBranchCondition = "and(succeeded(), or(eq(variables['Build.SourceBranch'], 'refs/heads/develop'), startsWith(variables['Build.SourceBranch'], 'refs/heads/7580')))"
}

Describe 'pipeline-CLOUD - structure' -Skip:(-not $script:HaveYaml) {

    It 'parses, and every embedded PowerShell script is syntactically valid' {
        $bad = foreach ($stage in @($script:Doc['stages'])) {
            foreach ($job in @($stage['jobs'])) {
                foreach ($step in @($job['steps'])) {
                    $body = Get-StepScript $step
                    if (-not $body) { continue }
                    $errors = $null
                    [System.Management.Automation.Language.Parser]::ParseInput($body, [ref]$null, [ref]$errors) | Out-Null
                    if ($errors) { "$($job['job']) / $($step['displayName']): $($errors[0].Message)" }
                }
            }
        }
        $bad | Should -BeNullOrEmpty
    }

    It 'triggers CI on exactly the release branches the conditions allow' {
        @($script:Doc['trigger']['branches']['include']) | Should -Be @('develop', '7580*')
    }
}

Describe 'pipeline-CLOUD - publishing is limited to release branches' -Skip:(-not $script:HaveYaml) {

    It 'gates the Release stage on the release branches, not on Build.Reason' {
        $condition = (Get-Stage 'Release')['condition']
        $condition | Should -Be $script:ReleaseBranchCondition
        $condition | Should -Not -Match 'Build\.Reason'
    }

    It 'gates the GitLab tag step on the same release branches' {
        (Find-Step 'Tag Source and Push Version Tags to GitLab')['condition'] | Should -Be $script:ReleaseBranchCondition
    }

    It 'never overwrites a published release zip' {
        $upload = Get-StepScript (Find-Step 'Upload zips to Azure Storage')
        $uploads = [regex]::Matches($upload, '(?s)az storage blob upload\b.*?--overwrite (\w+)')
        $uploads.Count | Should -Be 4
        @($uploads | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique) | Should -Be @('false')
    }
}

Describe 'pipeline-CLOUD - Apply Version from Git Tags' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:VersionStep = Get-StepScript (Find-Step 'Apply Version from Git Tags to AssemblyCommonInfo.cs')
        function script:Invoke-VersionStep {
            param([string] $Repo)
            Invoke-StepProcess (Expand-Macros $script:VersionStep @{ 'Build.SourcesDirectory' = $Repo })
        }
    }

    BeforeEach { $script:Repo = New-VersionRepo }
    AfterEach  { Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue }

    It 'mints the highest tag in the whole repo + 1, even when HEAD''s history stops lower' {
        $r = Invoke-VersionStep $script:Repo
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match 'Generated next version: 3\.0\.3\.21'
        Get-Content -LiteralPath (Join-Path $script:Repo 'Dev/AssemblyCommonInfo.cs') -Raw | Should -Be '[assembly: AssemblyVersion("3.0.3.21")]'
        Get-Content -LiteralPath (Join-Path $script:Repo 'Dev/AssemblyCommonInfo.fs') -Raw | Should -Be '[<assembly: AssemblyVersion("3.0.3.21")>]'
    }

    It 'reuses the version tag already at HEAD' {
        Invoke-RepoGit $script:Repo tag 3.0.3.21 | Out-Null
        $r = Invoke-VersionStep $script:Repo
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match 'Commit is already tagged: 3\.0\.3\.21'
        Get-Content -LiteralPath (Join-Path $script:Repo 'Dev/AssemblyCommonInfo.cs') -Raw | Should -Be '[assembly: AssemblyVersion("3.0.3.21")]'
    }
}

Describe 'pipeline-CLOUD - Tag Source and Push Version Tags to GitLab' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:TagStep = Get-StepScript (Find-Step 'Tag Source and Push Version Tags to GitLab')
        # 'git push' never leaves the machine: it is recorded to $env:PUSH_LOG (refspecs only) and
        # exits with $env:PUSH_EXIT. Every other git call runs for real against the throwaway repo.
        $script:PushStub = @'
function git {
    if ($args -contains 'push') {
        Add-Content -LiteralPath $env:PUSH_LOG -Value (($args | Where-Object { $_ -like 'refs/tags/*' -or $_ -eq '--force' }) -join ' ')
        $global:LASTEXITCODE = [int]$env:PUSH_EXIT
        return
    }
    & (Get-Command git -CommandType Application | Select-Object -First 1) @args
}
'@
        function script:Invoke-TagStep {
            param([string] $Repo, [string] $Version, [int] $PushExit = 0)
            $body = Expand-Macros $script:TagStep @{ 'Build.SourcesDirectory' = $Repo; HighestReadVersion = $Version }
            Invoke-StepProcess $body -Prelude $script:PushStub -Environment @{
                PUSH_LOG = $script:PushLog; PUSH_EXIT = [string]$PushExit; GitLabTagsAccount = 'S3cretGitLabToken'
            }
        }
        function script:Get-Pushes { if (Test-Path -LiteralPath $script:PushLog) { @(Get-Content -LiteralPath $script:PushLog) } else { @() } }
    }

    BeforeEach {
        $script:Repo    = New-VersionRepo
        $script:Head    = (git -C $script:Repo rev-parse HEAD)
        $script:PushLog = Join-Path ([IO.Path]::GetTempPath()) ("wwcloud-push-" + [guid]::NewGuid().ToString('N') + '.log')
    }
    AfterEach {
        Remove-Item -LiteralPath $script:Repo -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $script:PushLog -Force -ErrorAction SilentlyContinue
    }

    It 'tags HEAD, pushes the version, and force-moves the rolling 3-, 2- and 1-part tags' {
        $r = Invoke-TagStep $script:Repo '3.0.3.21'
        $r.ExitCode | Should -Be 0 -Because $r.Output
        foreach ($t in '3.0.3.21', '3.0.3', '3.0', '3') { Get-TagSha $script:Repo $t | Should -Be $script:Head -Because "tag $t" }
        Get-Pushes | Should -Be @('refs/tags/3.0.3.21', '--force refs/tags/3.0.3', '--force refs/tags/3.0', '--force refs/tags/3')
    }

    It 'refuses a version already tagged on another commit, before tagging or pushing anything' {
        $sideSha = Get-TagSha $script:Repo '3.0.3.16'
        $r = Invoke-TagStep $script:Repo '3.0.3.16'
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match "Version tag '3\.0\.3\.16' already exists on $sideSha"
        Get-Pushes | Should -BeNullOrEmpty
        Get-TagSha $script:Repo '3.0.3' | Should -Not -Be $script:Head
    }

    It 'neither re-tags nor moves the rolling tags when HEAD already carries the version' {
        Invoke-RepoGit $script:Repo tag 3.0.3.21 | Out-Null
        $rollingBefore = Get-TagSha $script:Repo '3.0.3'
        $r = Invoke-TagStep $script:Repo '3.0.3.21'
        $r.ExitCode | Should -Be 0 -Because $r.Output
        $r.Output | Should -Match 'already tagged 3\.0\.3\.21'
        Get-Pushes | Should -BeNullOrEmpty
        Get-TagSha $script:Repo '3.0.3' | Should -Be $rollingBefore
    }

    It 'fails on a failed push, before moving any rolling tag, without echoing the GitLab credential' {
        $r = Invoke-TagStep $script:Repo '3.0.3.21' -PushExit 1
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match 'Pushing tag 3\.0\.3\.21 failed'
        $r.Output | Should -Not -Match 'S3cretGitLabToken'
        Get-Pushes | Should -Be @('refs/tags/3.0.3.21')
        Get-TagSha $script:Repo '3.0.3' | Should -Not -Be $script:Head
    }

    It 'fails when HighestReadVersion was never set' {
        $r = Invoke-TagStep $script:Repo '0.0.0.0'
        $r.ExitCode | Should -Not -Be 0
        $r.Output | Should -Match 'HighestReadVersion was not set'
    }
}
