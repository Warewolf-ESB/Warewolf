---
name: warewolf-build
description: How to build Warewolf (Compile.ps1) — server, studio, tests, lightweight engine, release, SpecFlow regen. Invoke before compiling, when a build fails, or when choosing a build switch/solution/output path. Defers full detail to Compile.md.
---

# Building Warewolf

All builds run through `Compile.ps1` at the repo root. It wraps `dotnet restore` + `dotnet publish` and handles post-processing (native DLL pinning, resource copying, test-host bootstrapping).

**Prerequisites:** .NET 8 SDK, Visual Studio 2022/2026 (MSBuild), `nuget.exe` on PATH or at `C:\Windows\nuget.exe`.

```powershell
# Build test binaries (most common — required before running tests)
.\Compile.ps1 -ServerTests

# Build the server only
.\Compile.ps1 -Server

# Build the Studio desktop application
.\Compile.ps1 -Studio

# Build everything (slow — use before a release or after cross-cutting changes)
.\Compile.ps1

# Release build with auto-versioning from git tags
.\Compile.ps1 -Release -Config Release -AutoVersion -Target Rebuild

# Regenerate SpecFlow .feature.cs code-behind files after editing .feature files
.\Compile.ps1 -RegenerateSpecFlowFeatureFiles
```

Solution–switch–output mapping:

| Switch | Solution | Output |
|---|---|---|
| `-ServerTests` | `Dev\ServerTests.sln` | `Bin\ServerTests\` |
| `-Server` | `Dev\Server.sln` | `Bin\Server\` |
| `-Studio` | `Dev\Studio.sln` | `Bin\Studio\` |
| `-Release` | `Dev\Release.sln` | `Bin\Release\` |
| `-UITesting` | `Dev\UITesting.sln` | `Bin\UITesting\` |
| `-Web` | `Dev\Web.sln` | `Bin\Web\` |

The Lightweight engine is included in `-ServerTests`; the Server is built with `-Server`.

**Full parameter reference and troubleshooting:** [Compile.md](../../../Compile.md).
