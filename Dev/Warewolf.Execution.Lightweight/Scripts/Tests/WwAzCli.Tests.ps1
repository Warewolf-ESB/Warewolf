#Requires -Version 7.0
#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
<#
    Pester 5 suite for WwAzCli.psm1 — the shared az transient-retry policy used by
    Deploy-WwExecutionEngine.ps1 (Invoke-Az), Setup-ApplicationInsights.ps1 and the three
    auth scripts.

    Offline: no az is invoked. -Invoke is a script block that plays back a scripted sequence
    of (exit code, output) pairs, and Start-Sleep is mocked inside the module so the backoff
    can be asserted without waiting for it.
#>

BeforeDiscovery {
    # The exact signature az printed in build 30801. Needed at discovery time for -ForEach data.
    $script:ArmOutage503 = "ERROR: Service Unavailable(<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN'>" +
        "<html><head><title>AzureResourceManager</title></head><body><h2>Our services aren't available right now</h2></body></html>)"
}

BeforeAll {
    Import-Module (Join-Path (Split-Path $PSScriptRoot -Parent) 'WwAzCli.psm1') -Force

    $script:ArmOutage503 = "ERROR: Service Unavailable(<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN'>" +
        "<html><head><title>AzureResourceManager</title></head><body><h2>Our services aren't available right now</h2></body></html>)"

    # Plays back one scripted step per call: @{ Exit = <int>; Out = <string> }.
    function New-ScriptedInvoke([object[]] $Steps) {
        $global:wwStepIndex = 0
        $global:wwSteps     = $Steps
        return {
            $step = $global:wwSteps[[Math]::Min($global:wwStepIndex, $global:wwSteps.Count - 1)]
            $global:wwStepIndex++
            $global:LASTEXITCODE = $step.Exit
            $step.Out
        }
    }
}

AfterAll {
    Remove-Variable -Name wwStepIndex, wwSteps -Scope Global -ErrorAction SilentlyContinue
    Remove-Module WwAzCli -ErrorAction SilentlyContinue
}

Describe 'WwAzCli — Test-WwAzTransientError' {

    It 'classifies <Name> as transient' -ForEach @(
        @{ Name = 'the build-30801 ARM outage page'; Text = $script:ArmOutage503 }
        @{ Name = 'an HTTP 503 status line';          Text = 'Operation returned an invalid status code 503 Service Unavailable' }
        @{ Name = 'ServiceUnavailable (ARM code)';     Text = '{"error":{"code":"ServiceUnavailable"}}' }
        @{ Name = 'a 502 Bad Gateway';                 Text = 'ERROR: 502 Bad Gateway' }
        @{ Name = 'a 504 GatewayTimeout';              Text = '(GatewayTimeout) The gateway did not receive a response' }
        @{ Name = 'a 500 Internal Server Error';       Text = 'ERROR: 500 Internal Server Error' }
        @{ Name = 'throttling 429';                    Text = '(TooManyRequests) Rate limit exceeded' }
        @{ Name = 'a connection reset (10054)';        Text = "ConnectionResetError(10054, 'An existing connection was forcibly closed')" }
        @{ Name = 'a read timeout';                    Text = 'ReadTimeoutError: Read timed out. (read timeout=300)' }
        @{ Name = 'a DNS hiccup';                      Text = 'getaddrinfo failed' }
    ) {
        Test-WwAzTransientError -Output $Text | Should -BeTrue
    }

    It 'does NOT classify <Name> as transient' -ForEach @(
        @{ Name = 'ResourceNotFound';    Text = "ERROR: (ResourceNotFound) The Resource 'Microsoft.Web/sites/x' was not found." }
        @{ Name = 'AuthorizationFailed'; Text = "ERROR: (AuthorizationFailed) The client does not have authorization." }
        @{ Name = 'a Conflict';          Text = 'ERROR: (Conflict) The role assignment already exists.' }
        @{ Name = 'a validation error';  Text = "ERROR: unrecognized arguments: --functions-scale-limit 5" }
        @{ Name = 'BadRequest';          Text = 'ERROR: (BadRequest) The parameter runtime version is invalid.' }
    ) {
        Test-WwAzTransientError -Output $Text | Should -BeFalse
    }

    It 'treats null / empty output as not transient' {
        Test-WwAzTransientError -Output $null | Should -BeFalse
        Test-WwAzTransientError -Output ''    | Should -BeFalse
    }
}

Describe 'WwAzCli — Invoke-WwAzWithRetry' {

    BeforeEach {
        Mock Start-Sleep -ModuleName WwAzCli { }
        $script:retries = [System.Collections.Generic.List[object]]::new()
        $script:onRetry = { param($attempt, $max, $wait) $script:retries.Add(@($attempt, $max, $wait)) }
    }

    It 'returns the first success without retrying' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 0; Out = 'ok' })
        $r = Invoke-WwAzWithRetry -Invoke $invoke -OnRetry $script:onRetry
        $r.ExitCode | Should -Be 0
        $r.Output   | Should -Be 'ok'
        $r.Attempts | Should -Be 1
        $script:retries.Count | Should -Be 0
        Should -Invoke Start-Sleep -ModuleName WwAzCli -Times 0 -Exactly
    }

    It 'retries a transient failure and returns the eventual success' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 1; Out = $script:ArmOutage503 }, @{ Exit = 0; Out = 'ok' })
        $r = Invoke-WwAzWithRetry -Invoke $invoke -OnRetry $script:onRetry
        $r.ExitCode | Should -Be 0
        $r.Output   | Should -Be 'ok'
        $r.Attempts | Should -Be 2
        $script:retries.Count | Should -Be 1
        $global:LASTEXITCODE  | Should -Be 0
    }

    It 'gives up after -MaxAttempts on a persistent transient failure, with the default 3/7/15/30 backoff' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 1; Out = $script:ArmOutage503 })
        $r = Invoke-WwAzWithRetry -Invoke $invoke -OnRetry $script:onRetry
        $r.ExitCode  | Should -Be 1
        $r.Attempts  | Should -Be 5
        $r.Transient | Should -BeTrue
        $r.Output    | Should -Match "aren't available right now"
        $global:wwStepIndex | Should -Be 5
        ($script:retries | ForEach-Object { $_[2] }) -join ',' | Should -Be '3,7,15,30'
        ($script:retries | ForEach-Object { "$($_[0])/$($_[1])" }) -join ',' | Should -Be '1/5,2/5,3/5,4/5'
        Should -Invoke Start-Sleep -ModuleName WwAzCli -Times 4 -Exactly
        $global:LASTEXITCODE | Should -Be 1
    }

    It 'does NOT retry a real error' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 3; Out = 'ERROR: (ResourceNotFound) not found' }, @{ Exit = 0; Out = 'never' })
        $r = Invoke-WwAzWithRetry -Invoke $invoke -OnRetry $script:onRetry
        $r.ExitCode  | Should -Be 3
        $r.Attempts  | Should -Be 1
        $r.Transient | Should -BeFalse
        $global:wwStepIndex   | Should -Be 1
        $script:retries.Count | Should -Be 0
        $global:LASTEXITCODE  | Should -Be 3
    }

    It 'reuses the last delay once -DelaySeconds runs out, and skips Start-Sleep for a zero wait' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 1; Out = $script:ArmOutage503 })
        $r = Invoke-WwAzWithRetry -Invoke $invoke -MaxAttempts 4 -DelaySeconds @(0, 2) -OnRetry $script:onRetry
        $r.Attempts | Should -Be 4
        ($script:retries | ForEach-Object { $_[2] }) -join ',' | Should -Be '0,2,2'
        Should -Invoke Start-Sleep -ModuleName WwAzCli -Times 2 -Exactly
    }

    It 'never throws for an az failure (the caller owns throw / -AllowFail)' {
        $invoke = New-ScriptedInvoke @(@{ Exit = 1; Out = 'ERROR: (BadRequest) nope' })
        { Invoke-WwAzWithRetry -Invoke $invoke } | Should -Not -Throw
    }

    It 'resolves `az` in the CALLER''s scope, so a test shim is used instead of the real CLI' {
        # The reason -Invoke exists: a module function calling `& az` would skip this shim.
        function az { $global:LASTEXITCODE = 0; 'from-the-shim' }
        $r = Invoke-WwAzWithRetry -Invoke { & az account show }
        $r.Output | Should -Be 'from-the-shim'
    }
}

Describe 'WwAzCli — Invoke-WwAz (the inline pipeline steps'' `az` alias)' {

    BeforeEach {
        Mock Start-Sleep -ModuleName WwAzCli { }
        $script:hostLines = [System.Collections.Generic.List[string]]::new()
        Mock Write-Host -ModuleName WwAzCli { $script:hostLines.Add([string]$Object) }

        # Stands in for the real az executable. Plays back one step per call:
        # @{ Exit = <int>; Out = <stdout lines>; Err = <stderr lines> }, and records the args it got.
        $global:wwAzCalls = [System.Collections.Generic.List[object]]::new()
        function script:Use-FakeAz([object[]] $Steps) {
            $global:wwAzSteps = $Steps
            $global:wwAzCalls.Clear()
            Mock Get-WwAzExecutable -ModuleName WwAzCli {
                {
                    $global:wwAzCalls.Add(@($args))
                    $step = $global:wwAzSteps[[Math]::Min($global:wwAzCalls.Count - 1, $global:wwAzSteps.Count - 1)]
                    foreach ($l in @($step.Err)) { if ($l) { Write-Error -Message $l -ErrorAction Continue } }
                    foreach ($l in @($step.Out)) { if ($l) { $l } }
                    $global:LASTEXITCODE = $step.Exit
                }
            }
        }
    }

    AfterEach {
        Remove-Item Alias:\az -ErrorAction SilentlyContinue
        Remove-Variable -Name wwAzCalls, wwAzSteps -Scope Global -ErrorAction SilentlyContinue
    }

    It 'retries the build-30844 ARM outage page and returns the eventual success' {
        Use-FakeAz @(@{ Exit = 1; Err = $script:ArmOutage503 }, @{ Exit = 0; Out = 'true' })
        $out = Invoke-WwAz functionapp config appsettings list --name app -o tsv
        $out                  | Should -Be 'true'
        $global:LASTEXITCODE  | Should -Be 0
        $global:wwAzCalls.Count | Should -Be 2
    }

    It 'does NOT retry a real error, and leaves its exit code for the step''s own check' {
        Use-FakeAz @(@{ Exit = 3; Err = 'ERROR: (ResourceNotFound) not found' }, @{ Exit = 0; Out = 'never' })
        $out = Invoke-WwAz functionapp show --name app
        $out                    | Should -BeNullOrEmpty
        $global:LASTEXITCODE    | Should -Be 3
        $global:wwAzCalls.Count | Should -Be 1
    }

    It 'gives up after 5 attempts on a persistent outage with the final exit code' {
        Use-FakeAz @(@{ Exit = 1; Err = $script:ArmOutage503 })
        Invoke-WwAz functionapp stop --name app
        $global:LASTEXITCODE    | Should -Be 1
        $global:wwAzCalls.Count | Should -Be 5
    }

    It 'returns stdout only - stderr (warnings included) goes to the host, never into the captured value' {
        Use-FakeAz @(@{ Exit = 0; Out = @('line1', 'line2'); Err = 'WARNING: this command is in preview' })
        $out = @(Invoke-WwAz servicebus queue show --name q)
        $out -join '|'   | Should -Be 'line1|line2'
        $script:hostLines | Should -Contain 'WARNING: this command is in preview'
    }

    It 'writes only the FINAL attempt''s stderr, and names the command without any option value on retry' {
        Use-FakeAz @(@{ Exit = 1; Err = "$script:ArmOutage503 p=S3cr3t" }, @{ Exit = 0; Out = '{}' })
        Invoke-WwAz login --service-principal -u appid -p S3cr3t --tenant t | Out-Null
        ($script:hostLines -join "`n") | Should -Not -Match 'S3cr3t'
        $script:hostLines | Should -Contain '##[warning]az login failed transiently (attempt 1/5); retrying in 3s.'
    }

    It 'passes every argument through to az unchanged' {
        Use-FakeAz @(@{ Exit = 0; Out = 'x' })
        Invoke-WwAz functionapp config appsettings set --settings '@C:\t\s.json' --query "[?name=='A'].value | [0]" -o none | Out-Null
        $global:wwAzCalls[0] -join ' ' | Should -Be "functionapp config appsettings set --settings @C:\t\s.json --query [?name=='A'].value | [0] -o none"
        $global:wwAzCalls[0].Count     | Should -Be 10
    }

    It 'works through the `az` alias exactly as the pipeline steps use it' {
        Use-FakeAz @(@{ Exit = 1; Err = $script:ArmOutage503 }, @{ Exit = 0; Out = 'false' })
        Set-Alias -Name az -Value Invoke-WwAz -Scope Script
        $stored = az functionapp config appsettings list --name app --query "[?name=='X'].value | [0]" -o tsv
        if ($LASTEXITCODE -ne 0) { throw 'unexpected' }
        $stored | Should -Be 'false'
    }

    It 'resolves the real az executable, never the `az` alias that routes to itself' {
        Mock Get-Command -ModuleName WwAzCli { [pscustomobject]@{ Source = 'C:\az\az.cmd' } }
        Set-Alias -Name az -Value Invoke-WwAz -Scope Global
        InModuleScope WwAzCli { Get-WwAzExecutable } | Should -Be 'C:\az\az.cmd'
        Should -Invoke Get-Command -ModuleName WwAzCli -Times 1 -Exactly -ParameterFilter { $CommandType -eq 'Application' -and $Name -eq 'az' }
        Remove-Item Alias:\az -Force -ErrorAction SilentlyContinue
    }
}
