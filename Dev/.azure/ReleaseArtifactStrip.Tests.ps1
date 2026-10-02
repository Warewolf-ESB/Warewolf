#Requires -Version 7.0
<#
    Pester 5 suite: the Release stage's "Strip Deploy-Time Resources and Config from Release
    Artifacts" step (pipeline-CLOUD.yml) strips deploy-time content but keeps engine metadata.

        Invoke-Pester -Path ./ReleaseArtifactStrip.Tests.ps1

    Release 3.0.3.1 shipped both engine zips without Resources/fidelity-allowlist.json, because the
    step emptied the whole Resources folder. FidelityAllowList fails closed without that file, so
    every engine deployed from the release (e.g. via the Web Studio Engines screen) refused to open
    any workflow body: "...contains activity type 'DsfFlowDecisionActivity', which has not yet
    passed round-trip fidelity testing".

    The step's own script is lifted out of the YAML and run against a fake publish folder, with
    $(Agent.BuildDirectory) pointed at TestDrive. Requires powershell-yaml; skipped (not failed)
    without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:StepName = 'Strip Deploy-Time Resources and Config from Release Artifacts'
    $script:Body = $null

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $doc = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'pipeline-CLOUD.yml') -Raw)
        foreach ($job in @($doc['stages'] | ForEach-Object { $_['jobs'] })) {
            foreach ($step in @($job['steps'])) {
                if ($step -is [System.Collections.IDictionary] -and $step['displayName'] -eq $script:StepName) {
                    $script:Body = [string]$step['powershell']
                }
            }
        }
    }

    # Lays out both publish folders the step walks, then runs the step's script against them.
    # The YAML joins paths with '\'; on Linux that is not a separator, so normalise it.
    function script:Invoke-StripStep([string] $BuildDir, [switch] $OmitAllowList) {
        foreach ($name in 'AzureFunctionsPublish', 'AzureFunctionsPublishWindows') {
            $publish = Join-Path $BuildDir $name
            $resources = Join-Path $publish 'Resources'
            New-Item -ItemType Directory -Force -Path (Join-Path $resources 'tools') | Out-Null
            New-Item -ItemType Directory -Force -Path (Join-Path $publish 'Prompts') | Out-Null
            Set-Content -LiteralPath (Join-Path $resources 'Hello World.bite') -Value '<Service />'
            Set-Content -LiteralPath (Join-Path $resources 'tools/Fidelity_Assign.bite') -Value '<Service />'
            Set-Content -LiteralPath (Join-Path $publish 'Prompts/system.md') -Value 'prompt'
            Set-Content -LiteralPath (Join-Path $publish 'secure.config') -Value 'secret'
            Set-Content -LiteralPath (Join-Path $publish 'Warewolf.Execution.Lightweight.dll') -Value 'engine'
            if (-not $OmitAllowList) {
                Set-Content -LiteralPath (Join-Path $resources 'fidelity-allowlist.json') -Value '{"results":[]}'
            }
        }

        $sep = [System.IO.Path]::DirectorySeparatorChar
        $code = $script:Body.Replace('$(Agent.BuildDirectory)', $BuildDir).Replace('\', $sep)
        & ([scriptblock]::Create($code)) 6>$null
    }
}

Describe 'Release artifact strip step (pipeline-CLOUD.yml)' -Skip:(-not $script:HaveYaml) {

    It 'finds the strip step (guards against a vacuous pass)' {
        $script:Body | Should -Not -BeNullOrEmpty
        $script:Body | Should -Match 'AzureFunctionsPublishWindows'
    }

    Context 'both engine publish folders (linux-x64 and win-x64)' -ForEach @(
        @{ Folder = 'AzureFunctionsPublish' }, @{ Folder = 'AzureFunctionsPublishWindows' }
    ) {
        BeforeAll {
            $script:BuildDir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            Invoke-StripStep -BuildDir $script:BuildDir
            $script:Publish = Join-Path $script:BuildDir $Folder
        }

        It 'keeps Resources/fidelity-allowlist.json in <Folder>' {
            Join-Path $script:Publish 'Resources/fidelity-allowlist.json' | Should -Exist
        }

        It 'removes every deploy-time workflow from Resources in <Folder>' {
            $left = @(Get-ChildItem -LiteralPath (Join-Path $script:Publish 'Resources') -Force -Recurse |
                      Select-Object -ExpandProperty Name)
            $left | Should -Be @('fidelity-allowlist.json')
        }

        It 'still strips secure.config and Prompts from <Folder>' {
            Join-Path $script:Publish 'secure.config' | Should -Not -Exist
            Join-Path $script:Publish 'Prompts' | Should -Not -Exist
        }

        It 'leaves the engine binaries in <Folder>' {
            Join-Path $script:Publish 'Warewolf.Execution.Lightweight.dll' | Should -Exist
        }
    }

    It 'fails the release when the publish output has no fidelity-allowlist.json' {
        $buildDir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        { Invoke-StripStep -BuildDir $buildDir -OmitAllowList } | Should -Throw '*fidelity-allowlist.json*'
    }
}
