#Requires -Version 7.0

<#
    Pester 5 test suite for Rollback-WwExecutionEngine.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Rollback-WwExecutionEngine.Tests.ps1

    Design notes (same harness conventions as the deploy suite)
    -----------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers (incl. the PURE
      Get-OwnershipDecision) without running any phase.
    * `az` is shadowed by a tracking *function* so tests run with or without the
      Azure CLI installed.
    * End-to-end behaviour is exercised in -DryRun: every delete is echoed, not
      performed; ownership is driven by a fixture summary.json `created` map.
#>

BeforeAll {
    $script:RollbackScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Rollback-WwExecutionEngine.ps1'
}

Describe 'Rollback-WwExecutionEngine — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:RollbackScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:RollbackScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Get-OwnershipDecision -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-Az             -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }
}

Describe 'Rollback-WwExecutionEngine — helper functions' {

    BeforeAll { . $script:RollbackScript -LoadFunctionsOnly }

    Context 'Get-OwnershipDecision (pure)' {
        It 'created=$true  -> Owned'            { Get-OwnershipDecision -Created $true  -TagRunId $null     -ExpectedRunId 'wwx-1' | Should -Be 'Owned' }
        It 'created=$false -> Preserved'        { Get-OwnershipDecision -Created $false -TagRunId 'wwx-1'   -ExpectedRunId 'wwx-1' | Should -Be 'Preserved' }
        It 'no created, matching tag -> Owned'  { Get-OwnershipDecision -Created $null  -TagRunId 'wwx-1'   -ExpectedRunId 'wwx-1' | Should -Be 'Owned' }
        It 'no created, other tag -> Preserved' { Get-OwnershipDecision -Created $null  -TagRunId 'wwx-9'   -ExpectedRunId 'wwx-1' | Should -Be 'Preserved' }
        It 'no created, no tag -> Preserved'    { Get-OwnershipDecision -Created $null  -TagRunId $null     -ExpectedRunId 'wwx-1' | Should -Be 'Preserved' }
    }

    Context 'Invoke-Az' {
        BeforeEach {
            $script:azExit = 0; $script:azOut = '{}'; $script:azCalls = 0
            function az { $script:azCalls++; $global:LASTEXITCODE = $script:azExit; $script:azOut }
        }
        It 'skips a mutating call under -DryRun and returns null' {
            $DryRun = $true
            Invoke-Az -Args @('keyvault', 'delete') -Mutating | Should -BeNullOrEmpty
            $script:azCalls | Should -Be 0
        }
        It 'returns null on failure when -AllowFail is set' {
            $DryRun = $false; $script:azExit = 1
            Invoke-Az -Args @('functionapp', 'show') -AllowFail | Should -BeNullOrEmpty
        }
    }

    Context 'Confirm-Action (per-action cleanup gate)' {
        It 'auto-proceeds under -DryRun without prompting' {
            $DryRun = $true; $Force = $false; $NonInteractive = $false
            Mock Read-Host { throw 'should not prompt under DryRun' }
            Confirm-Action 'delete X' | Should -BeTrue
        }
        It 'auto-proceeds under -Force'          { $DryRun=$false; $Force=$true;  $NonInteractive=$false; Confirm-Action 'delete X' | Should -BeTrue }
        It 'auto-proceeds under -NonInteractive' { $DryRun=$false; $Force=$false; $NonInteractive=$true;  Confirm-Action 'delete X' | Should -BeTrue }
        It 'prompts and honours NO interactively' {
            $DryRun=$false; $Force=$false; $NonInteractive=$false
            Mock Read-Host { 'n' }
            Confirm-Action 'delete X' | Should -BeFalse
        }
        It 'prompts and honours YES interactively' {
            $DryRun=$false; $Force=$false; $NonInteractive=$false
            Mock Read-Host { 'y' }
            Confirm-Action 'delete X' | Should -BeTrue
        }
    }
}

Describe 'Rollback-WwExecutionEngine — end-to-end (DryRun, no side effects)' {

    BeforeEach {
        # az shim: existence probes return JSON (with our run tag); everything else
        # is echoed by -DryRun and never actually invoked here.
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account' -and $a -contains 'show') { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a -contains 'show') { return '{"name":"x","tags":{"wwx-test-run":"wwx-TESTRUN"}}' }
            return '{}'
        }

        # Fixture summary.json — created map drives ownership.
        $global:rbDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwrb-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $global:rbDir -Force | Out-Null
        $global:summaryObj = [ordered]@{
            runId               = 'wwx-TESTRUN'
            subscriptionId      = 'sub-123'
            resourceGroup       = 'DEV2'
            appName             = 'wwenginetest'
            storageAccount      = 'stwwenginetest'
            appInsightsName     = 'wwenginetest-ai'
            entraAppDisplayName = 'wwenginetest-auth'
            keyVault            = @{ name = 'kv-test'; secret = 'dp' }
            created             = [ordered]@{ resourceGroup = $true; storageAccount = $true; functionApp = $true; appInsights = $true; keyVault = $true; entraApp = $true }
        }
        $global:summaryPath = Join-Path $global:rbDir 'deploy.summary.json'
        $global:summaryObj | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $global:summaryPath -Encoding UTF8

        $script:commonArgs = @{ SummaryPath = $global:summaryPath; NonInteractive = $true; DryRun = $true }
    }

    AfterEach {
        if ($global:rbDir -and (Test-Path -LiteralPath $global:rbDir)) { Remove-Item -LiteralPath $global:rbDir -Recurse -Force }
    }

    AfterAll {
        Remove-Variable -Name rbDir, summaryObj, summaryPath -Scope Global -ErrorAction SilentlyContinue
    }

    It 'runs every phase to completion' {
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Phase 0  Pre-flight'
        $out | Should -Match 'Phase 1  Discovery'
        $out | Should -Match 'Phase 2  Rollback plan'
        $out | Should -Match 'Phase 3  Teardown'
        $out | Should -Match 'Phase 4  Leak check'
        $out | Should -Match 'Rollback complete'
    }

    It 'discovers App Insights via ARM fallback when the app-insights extension fails' {
        # Override the shim: the 'az monitor app-insights' extension path fails (as
        # with the WinError 5 cache lock); ARM 'az resource show' still resolves it.
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account' -and $a -contains 'show') { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'monitor') { $global:LASTEXITCODE = 1; return $null }            # broken extension
            if ($a -contains 'show') { return '{"name":"x","tags":{"wwx-test-run":"wwx-TESTRUN"}}' }
            return '{}'
        }
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'appInsights\s*: exists=True\s*-> Owned'   # found despite extension failure
        $out | Should -Match 'App Insights component'                   # appears in the delete plan
    }

    It 'plans deletion of owned resources and delegates auth to the cleanup script' {
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match "\[DRYRUN\] & 'Cleanup-WwExecutionAuth\.ps1'"
        $out | Should -Match '\[DRYRUN\] az keyvault delete'
        $out | Should -Match '\[DRYRUN\] az monitor app-insights component delete'
        $out | Should -Match '\[DRYRUN\] az functionapp delete'
        $out | Should -Match '\[DRYRUN\] az storage account delete'
    }

    It 'deletes the Key Vault THEN purges it (soft-delete order)' {
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $del   = $out.IndexOf('az keyvault delete')
        $purge = $out.IndexOf('az keyvault purge')
        $del | Should -BeGreaterThan 0
        $purge | Should -BeGreaterThan $del
    }

    It 'tears down in dependency order (auth -> key vault -> function app -> storage)' {
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $auth = $out.IndexOf("Cleanup-WwExecutionAuth.ps1'")
        $kv   = $out.IndexOf('az keyvault delete')
        $fa   = $out.IndexOf('az functionapp delete')
        $st   = $out.IndexOf('az storage account delete')
        $auth | Should -BeLessThan $kv
        $kv   | Should -BeLessThan $fa
        $fa   | Should -BeLessThan $st
    }

    It 'PRESERVES a resource the run did not create (created=false)' {
        $global:summaryObj.created.storageAccount = $false
        $global:summaryObj | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $global:summaryPath -Encoding UTF8
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az storage account delete'
        $out | Should -Match 'PRESERVE'
    }

    It 'flags a PARTIAL run (failed deploy summary) and still cleans what it created' {
        # The crash-safe deploy writes a summary with status=failed + lastPhase when
        # it dies mid-run (e.g. Phase 4). Rollback must accept it, note the partial
        # run, and tear down the recorded created-map.
        $global:summaryObj.status    = 'failed'
        $global:summaryObj.lastPhase = 'Phase 4  Deploy package'
        $global:summaryObj.error     = 'func publish failed (1).'
        $global:summaryObj | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $global:summaryPath -Encoding UTF8
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match "PARTIAL run \(status='failed', lastPhase='Phase 4  Deploy package'\)"
        $out | Should -Match '\[DRYRUN\] az functionapp delete'   # still cleans what it created
    }

    It 'skips a created=true resource that no longer exists (never actually created)' {
        # Intent was recorded (created=$true) but the resource is absent — the az
        # existence probe returns nothing. Rollback must NOT attempt to delete it.
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account' -and $a -contains 'show') { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }   # absent
            if ($a -contains 'show') { return '{"name":"x","tags":{"wwx-test-run":"wwx-TESTRUN"}}' }
            return '{}'
        }
        $global:summaryObj.status = 'failed'
        $global:summaryObj | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $global:summaryPath -Encoding UTF8
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az functionapp delete'   # absent -> skipped
    }

    It 'never deletes the resource group by default (existing-RG strategy)' {
        $out = (& $script:RollbackScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az group delete'
        $out | Should -Match "Resource group '.*' preserved"
    }

    It 'deletes the resource group only with -DeleteResourceGroup when the run created it' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.DeleteResourceGroup = $true
        $out = (& $script:RollbackScript @callArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az group delete'
    }

    It 'refuses RG deletion even with -DeleteResourceGroup when the RG pre-existed' {
        $global:summaryObj.created.resourceGroup = $false
        $global:summaryObj | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $global:summaryPath -Encoding UTF8
        $callArgs = $script:commonArgs.Clone(); $callArgs.DeleteResourceGroup = $true
        $out = (& $script:RollbackScript @callArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az group delete'
    }

    It 'throws when neither -SummaryPath nor -ResourceGroup resolves a target' {
        { & $script:RollbackScript -NonInteractive -DryRun } | Should -Throw '*ResourceGroup*'
    }

    It 'does NOT auto-delete the Entra app in the no-summary fallback' {
        $out = (& $script:RollbackScript -ResourceGroup DEV2 -EntraAppDisplayName 'x-auth' -RunId 'wwx-1' -NonInteractive -DryRun) 6>&1 | Out-String
        $out | Should -Not -Match "\[DRYRUN\] & 'Cleanup-WwExecutionAuth\.ps1'"
        $out | Should -Match 'pass -IncludeEntraApp'
    }

    It 'includes the Entra app in the no-summary fallback only with -IncludeEntraApp' {
        $out = (& $script:RollbackScript -ResourceGroup DEV2 -EntraAppDisplayName 'x-auth' -RunId 'wwx-1' -IncludeEntraApp -NonInteractive -DryRun) 6>&1 | Out-String
        $out | Should -Match "\[DRYRUN\] & 'Cleanup-WwExecutionAuth\.ps1'"
    }
}
