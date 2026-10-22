# Warewolf - Once bitten, there's no going back
# Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
# Licensed under GNU Affero General Public License 3.0 or later.
<#
.SYNOPSIS
    Builds the engine's npm packages: one self-contained engine per platform, and @warewolf/engine.

.DESCRIPTION
    The esbuild pattern. @warewolf/engine-linux-x64, -linux-arm64 and -win32-x64 each hold a
    self-contained publish (no .NET runtime needed) restricted to their os/cpu, so npm installs only
    the one that matches. @warewolf/engine lists them as optionalDependencies and carries the
    warewolf-server CLI (cli.mjs), which runs the engine self-hosted on 127.0.0.1:3142 and installs it
    as the "Warewolf Server" service.

    Azure-only files are left out: the deployment scripts, and the Key Vault-encrypted Elasticsearch
    logging source (the self-hosted engine logs to the console).

    The packages are "private" until publishing is decided, so `npm publish` refuses them.

.EXAMPLE
    .\Build-NpmPackages.ps1 -Version 3.0.2-79
    .\Build-NpmPackages.ps1 -Version 3.0.2-79 -Runtimes linux-x64 -Pack
#>
param(
    # npm needs semver: a 3.0.2.79 engine build is 3.0.2-79.
    [string]   $Version  = "0.0.0-dev",
    [string[]] $Runtimes = @("linux-x64", "linux-arm64", "win-x64"),
    [string]   $Output   = (Join-Path $PSScriptRoot "out"),
    [switch]   $Pack
)

$ErrorActionPreference = "Stop"
$Project = Join-Path $PSScriptRoot "..\..\Warewolf.Execution.Lightweight.csproj"

# .NET runtime id -> npm platform suffix, os and cpu.
$Platforms = @{
    "linux-x64"   = @{ Suffix = "linux-x64";   Os = "linux"; Cpu = "x64" }
    "linux-arm64" = @{ Suffix = "linux-arm64"; Os = "linux"; Cpu = "arm64" }
    "win-x64"     = @{ Suffix = "win32-x64";   Os = "win32"; Cpu = "x64" }
}

function Write-Json($Path, $Value) {
    $Value | ConvertTo-Json -Depth 5 | Set-Content -Path $Path -Encoding utf8NoBOM
}

if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
New-Item -ItemType Directory -Path $Output | Out-Null

$optional = [ordered]@{}
foreach ($rid in $Runtimes) {
    $platform = $Platforms[$rid]
    if (-not $platform) { throw "Unknown runtime $rid. Known: $($Platforms.Keys -join ', ')" }
    $name = "@warewolf/engine-$($platform.Suffix)"
    $dir  = Join-Path $Output "engine-$($platform.Suffix)"
    $bin  = Join-Path $dir "bin"

    Write-Host "Publishing $rid -> $name"
    # The csproj turns the app host and self-contained output off for Azure; a package needs both.
    dotnet publish $Project -c Release -r $rid -p:SelfContained=true -p:UseAppHost=true -o $bin -nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid ($LASTEXITCODE)" }
    Get-ChildItem $bin -Filter *.ps1 -Recurse -File | Remove-Item -Force
    Remove-Item (Join-Path $bin "Settings" "ElasticsearchLoggingSource.bite") -Force -ErrorAction SilentlyContinue

    Write-Json (Join-Path $dir "package.json") ([ordered]@{
        name        = $name
        version     = $Version
        description = "The self-contained Warewolf execution engine for $($platform.Os) $($platform.Cpu). Installed by @warewolf/engine."
        private     = $true
        license     = "AGPL-3.0-or-later"
        os          = @($platform.Os)
        cpu         = @($platform.Cpu)
        files       = @("bin")
    })
    $optional[$name] = $Version
}

$meta = Join-Path $Output "engine"
New-Item -ItemType Directory -Path $meta | Out-Null
Copy-Item (Join-Path $PSScriptRoot "cli.mjs") $meta
Copy-Item (Join-Path $PSScriptRoot "README.md") $meta
Write-Json (Join-Path $meta "package.json") ([ordered]@{
    name                 = "@warewolf/engine"
    version              = $Version
    description          = "The Warewolf execution engine, self-hosted on 127.0.0.1:3142, with a warewolf-server CLI to run it as a service."
    private              = $true
    license              = "AGPL-3.0-or-later"
    type                 = "module"
    bin                  = [ordered]@{ "warewolf-server" = "cli.mjs" }
    engines              = [ordered]@{ node = ">=18" }
    files                = @("cli.mjs", "README.md")
    optionalDependencies = $optional
})

if ($Pack) {
    foreach ($dir in Get-ChildItem $Output -Directory) {
        Push-Location $dir.FullName
        try { npm pack --pack-destination $Output | Out-Host } finally { Pop-Location }
    }
}

Write-Host ""
Write-Host "Built in $Output"
Get-ChildItem $Output -Directory | ForEach-Object {
    $size = (Get-ChildItem $_.FullName -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
    Write-Host ("  {0,-24} {1,8:N0} MB" -f $_.Name, $size)
}
