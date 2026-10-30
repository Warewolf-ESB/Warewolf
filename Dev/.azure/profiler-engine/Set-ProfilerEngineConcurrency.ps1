#Requires -Version 7.0
<#
.SYNOPSIS
    Applies the profiler's Service Bus trigger concurrency to a staged engine package.

.DESCRIPTION
    host.json                              extensions.serviceBus.maxConcurrentCalls = -MaxConcurrentCalls, and
                                           concurrency.dynamicConcurrencyEnabled = false with
                                           -DisableDynamicConcurrency (dynamic concurrency ignores every static cap).
    Settings/executionengine.settings.json serviceBusTrigger.maxConcurrentExecutions = -MaxConcurrentExecutions.

    The same three settings Deploy-WwExecutionEngine.ps1 writes for -ServiceBusMaxConcurrentCalls,
    -DisableDynamicConcurrency and -ServiceBusTriggerMaxConcurrentExecutions, applied at build time instead.
    Port of Unlimited.ProfilerServices' SolutionItems/Scripts/Set-ProfilerEngineConcurrency.ps1, which reads the
    values from its profilerinfrastructure.json; DEV2 has no such file, so they are parameters here.
#>
[CmdletBinding()]
param(
    # The staged engine package: host.json at its root, Settings/executionengine.settings.json under it.
    [Parameter(Mandatory)][string] $PackageRoot,
    [Parameter(Mandatory)][ValidateRange(1, 1000)][int] $MaxConcurrentCalls,
    [Parameter(Mandatory)][ValidateRange(1, 1000)][int] $MaxConcurrentExecutions,
    [switch] $DisableDynamicConcurrency
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Set-JsonValue($Object, [string] $Name, $Value) {
    if ($Object.PSObject.Properties[$Name]) { $Object.$Name = $Value } else { $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value }
}

$hostJsonPath = Join-Path $PackageRoot 'host.json'
$hostJson = Get-Content -LiteralPath $hostJsonPath -Raw | ConvertFrom-Json
if (-not ($hostJson.PSObject.Properties['extensions'] -and $hostJson.extensions.PSObject.Properties['serviceBus'])) {
    throw "$hostJsonPath has no extensions.serviceBus section, so maxConcurrentCalls cannot be applied."
}
Set-JsonValue $hostJson.extensions.serviceBus 'maxConcurrentCalls' $MaxConcurrentCalls
if ($DisableDynamicConcurrency) {
    if (-not $hostJson.PSObject.Properties['concurrency']) { Set-JsonValue $hostJson 'concurrency' ([pscustomobject]@{}) }
    Set-JsonValue $hostJson.concurrency 'dynamicConcurrencyEnabled' $false
}
$hostJson | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $hostJsonPath -Encoding utf8

$settingsPath = Join-Path $PackageRoot 'Settings/executionengine.settings.json'
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
if (-not $settings.PSObject.Properties['serviceBusTrigger']) { Set-JsonValue $settings 'serviceBusTrigger' ([pscustomobject]@{}) }
Set-JsonValue $settings.serviceBusTrigger 'maxConcurrentExecutions' $MaxConcurrentExecutions
$settings | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $settingsPath -Encoding utf8

Write-Host "maxConcurrentCalls=$MaxConcurrentCalls, dynamic concurrency $(if ($DisableDynamicConcurrency) { 'off' } else { 'unchanged' }), maxConcurrentExecutions=$MaxConcurrentExecutions."
