# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
#Requires -Version 5.1
<#
.SYNOPSIS
    Generates a workflow index file for the Warewolf Azure Function Lightweight host.

.DESCRIPTION
    Enumerates all .bite and .xml workflow files under ResourcesDir and produces
    workflow-index.json mapping lowercase relative-path keys (without extension) to
    their original relative paths (with extension, forward-slash separated).

    The index is read at startup by WorkflowIndex.cs, enabling O(1) lookups for
    every HTTP request without any per-request disk I/O.

    Key format   : "tools/hello world"          (lowercase, forward slashes, no extension)
    Value format : "tools/Hello World.bite"     (original casing, forward slashes, with extension)

    .bite files take priority over .xml files when both exist for the same workflow name.

.PARAMETER ResourcesDir
    Path to the Resources directory that contains workflow files.

.PARAMETER OutputPath
    Full path where workflow-index.json will be written.

.EXAMPLE
    .\Generate-WorkflowIndex.ps1 `
        -ResourcesDir "D:\Dev\Resources - Release\Resources" `
        -OutputPath   "D:\Dev\Resources - Release\Resources\workflow-index.json"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$ResourcesDir,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$ResourcesDir = [System.IO.Path]::GetFullPath($ResourcesDir)

if (-not (Test-Path -LiteralPath $ResourcesDir -PathType Container)) {
    Write-Warning "Resources directory not found: $ResourcesDir - skipping index generation."
    exit 0
}

# Key   = lowercase relative path without extension (forward slashes)
# Value = relative path with extension, original casing (forward slashes)
$index = [System.Collections.Generic.Dictionary[string, string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase
)

# Process .bite first (preferred format), then .xml (legacy fallback).
# TryAdd ensures .bite wins when both exist for the same relative name.
$basePath = $ResourcesDir
if (-not $basePath.EndsWith('\')) { $basePath = "$basePath\" }

foreach ($pattern in @('*.bite', '*.xml')) {
    Get-ChildItem -LiteralPath $ResourcesDir -Filter $pattern -Recurse -File |
        ForEach-Object {
            # Compute relative path without requiring .NET Core APIs
            $relPath = $_.FullName.Substring($basePath.Length)
            $ext     = $_.Extension
            $noExt   = $relPath.Substring(0, $relPath.Length - $ext.Length)
            $key     = $noExt.Replace('\', '/').TrimStart('/').ToLowerInvariant()
            $value   = $relPath.Replace('\', '/')

            # First writer wins — .bite entries are never overwritten by .xml
            if (-not $index.ContainsKey($key)) {
                $index.Add($key, $value)
            }
        }
}

# Build a sorted ordered dictionary for deterministic, diff-friendly output.
$sorted = [ordered]@{}
foreach ($key in ($index.Keys | Sort-Object)) {
    $sorted[$key] = $index[$key]
}

$json = $sorted | ConvertTo-Json -Depth 1

# Ensure output directory exists (handles first-time generation).
$outputDir = [System.IO.Path]::GetDirectoryName($OutputPath)
if ($outputDir -and -not (Test-Path -LiteralPath $outputDir -PathType Container)) {
    [System.IO.Directory]::CreateDirectory($outputDir) | Out-Null
}

[System.IO.File]::WriteAllText($OutputPath, $json, [System.Text.Encoding]::UTF8)

Write-Host "workflow-index.json: $($index.Count) entries written to: $OutputPath"
