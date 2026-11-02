#Requires -Version 7.0
<#
.SYNOPSIS
    Applies the profiler's trigger concurrency (ProfilerEngineConcurrency.json 'trigger') to a staged engine package.

.DESCRIPTION
    host.json                              extensions.serviceBus.maxConcurrentCalls = trigger.maxConcurrentCalls, and
                                           concurrency.dynamicConcurrencyEnabled = false when
                                           trigger.disableDynamicConcurrency (dynamic concurrency ignores every static cap).
    Settings/executionengine.settings.json serviceBusTrigger.maxConcurrentExecutions = trigger.maxConcurrentExecutions.
#>
[CmdletBinding()]
param(
    # The staged engine package: host.json at its root, Settings/executionengine.settings.json under it.
    [Parameter(Mandatory)][string] $PackageRoot,
    # The token-replaced Engine/ProfilerEngineConcurrency.json.
    [Parameter(Mandatory)][string] $ConfigPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$trigger = (Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json).trigger

function Set-JsonValue($Object, [string] $Name, $Value) {
    if ($Object.PSObject.Properties[$Name]) { $Object.$Name = $Value } else { $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value }
}

$hostJsonPath = Join-Path $PackageRoot 'host.json'
$hostJson = Get-Content -LiteralPath $hostJsonPath -Raw | ConvertFrom-Json
if (-not ($hostJson.PSObject.Properties['extensions'] -and $hostJson.extensions.PSObject.Properties['serviceBus'])) {
    throw "$hostJsonPath has no extensions.serviceBus section, so maxConcurrentCalls cannot be applied."
}
Set-JsonValue $hostJson.extensions.serviceBus 'maxConcurrentCalls' $trigger.maxConcurrentCalls
if ($trigger.disableDynamicConcurrency) {
    if (-not $hostJson.PSObject.Properties['concurrency']) { Set-JsonValue $hostJson 'concurrency' ([pscustomobject]@{}) }
    Set-JsonValue $hostJson.concurrency 'dynamicConcurrencyEnabled' $false
}
$hostJson | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $hostJsonPath -Encoding utf8

$settingsPath = Join-Path $PackageRoot 'Settings/executionengine.settings.json'
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
if (-not $settings.PSObject.Properties['serviceBusTrigger']) { Set-JsonValue $settings 'serviceBusTrigger' ([pscustomobject]@{}) }
Set-JsonValue $settings.serviceBusTrigger 'maxConcurrentExecutions' $trigger.maxConcurrentExecutions
$settings | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $settingsPath -Encoding utf8

Write-Host "maxConcurrentCalls=$($trigger.maxConcurrentCalls), dynamic concurrency $(if ($trigger.disableDynamicConcurrency) { 'off' } else { 'unchanged' }), maxConcurrentExecutions=$($trigger.maxConcurrentExecutions)."
