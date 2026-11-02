<#
.SYNOPSIS
    CD smoke test: proves the timer Function App (AzureFunctions.ProfilerServices, tuprofiler<env>-function) can execute
    a workflow (by default GrantDates) on the Warewolf Lightweight Execution Engine.

.DESCRIPTION
    Run after both the engine package and the timer app are deployed. It needs no az and no key: it sends a GET to the
    timer app's anonymous SmokeTestGrantDates function (<app>/api/smoke/grantdates[?requestUrl=<path>]). The function
    calls the engine with the app's managed identity token, as the timer does, and returns the engine's answer. Nothing
    calls the engine with the pipeline's identity, so a pass covers the app's settings, its token, its
    Warewolf_ClientApps role on the route and the workflow itself.

    GrantDates sends the missing grant dates email when dates are missing, exactly as the Monday run does.

    Retried only while the answer says nothing about the call: no response, the function not yet loaded after the
    deploy (404/503), or the engine unavailable (503, e.g. a cold start). Any other answer is final.

.EXAMPLE
    ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl https://tuprofilerdev-function.azurewebsites.net

    The timer's own call: GET <app>/api/smoke/grantdates, which executes
    secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json on the engine.

.EXAMPLE
    ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl https://tuprofilerdev-function.azurewebsites.net `
        -RequestUrl 'secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json'

    A workflow on the engine's secure/ route, named explicitly. Pass the engine route as is: the function adds the VM
    server's 'Profiler/' prefix itself when the app still points at the VM, so 'Profiler/secure/...' is refused (400).

.EXAMPLE
    ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl https://tuprofilerdev-function.azurewebsites.net/api `
        -RequestUrl 'public/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json'

    A workflow on the engine's public/ route. A trailing /api on the app URL is accepted.

.EXAMPLE
    ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl https://tuprofileruat-function.azurewebsites.net -Attempts 3 -TimeoutSec 120

    Fewer retries (waits of 15 s, then 30 s between attempts) and a shorter per-request timeout.

.EXAMPLE
    ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl http://localhost:7001 -Attempts 1

    Against the app run locally (func start / F5; launchSettings.json uses port 7001). http is accepted only for localhost.

.EXAMPLE
    $result = ./Test-ProfilerEngineCaller.ps1 -FunctionAppUrl https://tuprofilerdev-function.azurewebsites.net
    $result.EngineStatus   # 200

    The returned object, for a caller that wants the details. A failure throws, so a pipeline step fails without it.

.EXAMPLE
    # Azure Pipelines (CD), after the engine and timer app deploys. No az and no key needed.
    - task: PowerShell@2
      displayName: 'Smoke test: timer app executes GrantDates on the engine'
      inputs:
        pwsh: true
        targetType: filePath
        filePath: '$(Pipeline.Workspace)\ci\drop\Unlimited.ProfilerServices\SolutionItems\Scripts\Test-ProfilerEngineCaller.ps1'
        arguments: >-
          -FunctionAppUrl "https://$(ProfilerTimerAppName).azurewebsites.net"
          -RequestUrl "secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json"

.OUTPUTS
    [pscustomobject] FunctionAppUrl, RequestUrl, EngineStatus, Attempts.
#>
# Below the help block: Get-Help ignores a help block that follows #Requires.
#Requires -Version 7.0
[CmdletBinding()]
param(
    # The timer app's base URL, e.g. https://tuprofilerdev-function.azurewebsites.net (a trailing /api is accepted).
    [Parameter(Mandatory)][string] $FunctionAppUrl,
    # The workflow path on the engine, starting with secure/ or public/ (e.g.
    # secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json). Empty: the timer's own GrantDates call.
    [string] $RequestUrl,
    # How many times to call before giving up, waiting 15 s, 30 s, 60 s, ... between calls (retried only while not ready).
    [ValidateRange(1, 10)][int] $Attempts = 5,
    # Seconds to wait for each call. Azure closes an HTTP request after 230 seconds.
    [ValidateRange(10, 230)][int] $TimeoutSec = 230
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$parsed = $null
if (-not [uri]::TryCreate($FunctionAppUrl, [UriKind]::Absolute, [ref]$parsed) -or -not ($parsed.Scheme -eq 'https' -or $parsed.IsLoopback)) {
    throw "FunctionAppUrl must be an absolute https URL (http only for localhost), not '$FunctionAppUrl'."
}
$uri = ($FunctionAppUrl.TrimEnd('/') -replace '/api$', '') + '/api/smoke/grantdates'
if ($RequestUrl) { $uri += '?requestUrl=' + [uri]::EscapeDataString($RequestUrl) }

$hints = @{
    0   = 'No engine token: check the app''s system-assigned identity and GenericEngineResourceAppId (api://<engine appId> must exist).'
    401 = 'The engine rejected the token: check GenericEngineResourceAppId is the engine''s Entra appId (the token audience).'
    403 = 'The engine refused the route: the app''s identity needs the Warewolf_ClientApps role on it in secure.config.'
    404 = 'The engine has no workflow at this route: check the engine package, GenericBaseURL and the request URL.'
}

foreach ($attempt in 1..$Attempts) {
    $retry = $null
    try {
        $r = Invoke-WebRequest -Method Get -Uri $uri -TimeoutSec $TimeoutSec -SkipHttpErrorCheck
    } catch {
        $r = $null
        $retry = "no response: $($_.Exception.Message)"
    }

    if ($r) {
        $result = $null
        try { $result = $r.Content | ConvertFrom-Json } catch { }
        $engineStatus = if ($result -and $result.PSObject.Properties['engineStatus']) { [int]$result.engineStatus } else { $null }
        Write-Host "Attempt $attempt/$Attempts`: $uri answered $($r.StatusCode); engine status $engineStatus."

        if ($null -eq $engineStatus) {
            if ($r.StatusCode -in 404, 503) { $retry = "the smoke test function is not answering yet ($($r.StatusCode))" }
            else { throw "$uri answered $($r.StatusCode) without an engine status: $($r.Content)" }
        } elseif ($r.StatusCode -eq 200 -and $engineStatus -ge 200 -and $engineStatus -lt 300) {
            Write-Host "The timer app executed $($result.requestUrl) on the engine ($engineStatus)."
            return [pscustomobject]@{ FunctionAppUrl = $FunctionAppUrl; RequestUrl = $result.requestUrl; EngineStatus = $engineStatus; Attempts = $attempt }
        } elseif ($engineStatus -eq 503) {
            $retry = 'the engine is unavailable (503)'
        } else {
            $detail = if ($result.PSObject.Properties['error']) { $result.error } else { $result.engineBody }
            $hint = if ($hints.ContainsKey($engineStatus)) { " $($hints[$engineStatus])" } else { '' }
            throw "The timer app could not execute $($result.requestUrl) on the engine (engine status $engineStatus): $detail.$hint"
        }
    }

    if ($attempt -eq $Attempts) { throw "Smoke test gave up after $Attempts attempts: $retry." }
    $delay = 15 * [math]::Pow(2, $attempt - 1)
    Write-Host "##[warning]Attempt $attempt/$Attempts`: $retry. Retrying in $delay s."
    Start-Sleep -Seconds $delay
}
