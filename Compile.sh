#!/usr/bin/env bash
# Bash version of Compile.ps1
# Usage: see parameter parsing below

set -e

# Default values
declare MSBuildPath="/usr/bin/msbuild"
declare Target=""
declare CustomVersion=""
declare NuGet=""
declare Config="Debug"
declare AutoVersion=0
declare ProjectSpecificOutputs=0
declare Release=0
declare Web=0
declare ServerTests=0
declare RegenerateSpecFlowFeatureFiles=0
declare InContainer=0
declare GitCredential=""
declare FrameworkTarget=""
declare Disablemaxcpucount=0

# Parse arguments (simple approach, can be improved)
while [[ $# -gt 0 ]]; do
  case $1 in
    --MSBuildPath) MSBuildPath="$2"; shift;;
    --Target) Target="$2"; shift;;
    --CustomVersion) CustomVersion="$2"; shift;;
    --NuGet) NuGet="$2"; shift;;
    --Config) Config="$2"; shift;;
    --AutoVersion) AutoVersion=1;;
    --ProjectSpecificOutputs) ProjectSpecificOutputs=1;;
    --Release) Release=1;;
    --Web) Web=1;;
    --ServerTests) ServerTests=1;;
    --RegenerateSpecFlowFeatureFiles) RegenerateSpecFlowFeatureFiles=1;;
    --InContainer) InContainer=1;;
    --GitCredential) GitCredential="$2"; shift;;
    --FrameworkTarget) FrameworkTarget="$2"; shift;;
    --Disablemaxcpucount) Disablemaxcpucount=1;;
    *) echo "Unknown option $1"; exit 1;;
  esac
  shift
done

if [[ -n "$Target" ]]; then
  Target="-t:$Target"
fi

PSScriptRoot="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# FrameworkTarget update for csproj/fsproj
if [[ -n "$FrameworkTarget" && "$FrameworkTarget" != "net6.0-windows" ]]; then
  find "$PSScriptRoot/Dev" \( -name '*.csproj' -o -name '*.fsproj' \) | while read -r file; do
    # Replace <TargetFramework> or <TargetFrameworks> with new value
    sed -i.bak -E "s|<TargetFramework(s)?>[^<]*</TargetFramework(s)?>|<TargetFramework>$FrameworkTarget</TargetFramework>|g" "$file"
  done
fi

# NuGet and MSBuild path finding (Linux/Mono/dotnet)
if [[ $InContainer -eq 0 ]]; then
  if [[ -z "$NuGet" ]]; then
    if command -v nuget >/dev/null 2>&1; then
      NuGet="$(command -v nuget)"
    else
      echo "NuGet not found. Please install NuGet CLI and set --NuGet path."
      exit 1
    fi
  fi
  if [[ ! -x "$MSBuildPath" ]]; then
    if command -v msbuild >/dev/null 2>&1; then
      MSBuildPath="$(command -v msbuild)"
    elif command -v dotnet >/dev/null 2>&1; then
      MSBuildPath="dotnet msbuild"
    else
      echo "MSBuild not found. Please install msbuild or dotnet SDK."
      exit 1
    fi
  fi
fi

# Versioning
GitCommitID=$(git -C "$PSScriptRoot" rev-parse HEAD)
if [[ $AutoVersion -eq 1 || -n "$CustomVersion" ]]; then
  echo "Writing C# and F# versioning files..."
  if [[ -n "$GitCredential" ]]; then
    git -C "$PSScriptRoot" remote set-url origin "https://$GitCredential@gitlab.com/warewolf/warewolf"
  fi
  git -C "$PSScriptRoot" fetch --all --tags -f
  GitCommitTimeString=$(git -C "$PSScriptRoot" show -s --format="%ct" "$GitCommitID")
  if [[ -z "$GitCommitTimeString" ]]; then
    echo "Cannot resolve time of commit $GitCommitID."
  else
    GitCommitTime=$(date -d "@$GitCommitTimeString" '+%Y-%m-%dT%H:%M:%S')
  fi
  GitBranchName=$(git -C "$PSScriptRoot" rev-parse --abbrev-ref HEAD)
  if [[ -n "$CustomVersion" ]]; then
    FullVersionString="$CustomVersion"
  else
    FullVersionString=$(git -C "$PSScriptRoot" tag --points-at HEAD | head -n1)
    if [[ -z "$FullVersionString" ]]; then
      AllTags=$(git -C "$PSScriptRoot" tag -l --sort=-creatordate --merged)
      for element in $AllTags; do
        dotCount=$(echo "$element" | awk -F. '{print NF-1}')
        if [[ $dotCount -eq 3 ]]; then
          FullVersionString="$element"
          break
        fi
      done
      if [[ -z "$FullVersionString" ]]; then
        echo "No local tags found in git history."
        exit 1
      else
        # Increment build number
        IFS='.' read -r v1 v2 v3 v4 <<< "$FullVersionString"
        NewBuildNumber=$((v4+1))
        FullVersionString="$v1.$v2.$v3.$NewBuildNumber"
      fi
    fi
  fi
  # Write version files
  CSharpVersionFile="$PSScriptRoot/Dev/AssemblyCommonInfo.cs"
  echo "Writing C Sharp version file to $CSharpVersionFile..."
  cat > "$CSharpVersionFile" <<EOF
using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable CC0021 // Use nameof
[assembly: AssemblyCompany("Warewolf")]
[assembly: AssemblyProduct("Warewolf")]
#pragma warning restore CC0021 // Use nameof
[assembly: AssemblyCopyright("Copyright Warewolf $(date +%Y)")]
[assembly: AssemblyVersion("$FullVersionString")]
[assembly: AssemblyInformationalVersion("$GitCommitTime $GitCommitID $GitBranchName")]
[assembly: InternalsVisibleTo("Dev2.Runtime.Tests")]
[assembly: InternalsVisibleTo("Dev2.Runtime.WebServer.Tests")]
[assembly: InternalsVisibleTo("Dev2.Studio.Core.Tests")]
[assembly: InternalsVisibleTo("Dev2.TaskScheduler.Wrappers")]
[assembly: InternalsVisibleTo("Dev2.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("Warewolf.Studio.ViewModels.Tests")]
[assembly: InternalsVisibleTo("Warewolf.QueueWorker.Tests")]
[assembly: InternalsVisibleTo("Dev2.Data.Tests")]
[assembly: InternalsVisibleTo("Warewolf.Tools.Specs")]
[assembly: InternalsVisibleTo("Warewolf.Common.Framework48.Tests")]
[assembly: InternalsVisibleTo("Warewolf.Storage.Tests")]
[assembly: InternalsVisibleTo("Warewolf.UIBindingTests.ComDll")]
[assembly: InternalsVisibleTo("Warewolf.UIBindingTests.PluginSource")]
[assembly: InternalsVisibleTo("Dev2.Utils.Tests")]
[assembly: InternalsVisibleTo("Warewolf.Core.Tests")]
[assembly: InternalsVisibleTo("Dev2.Common.Tests")]
[assembly: InternalsVisibleTo("Dev2.Activities.Tests")]
[assembly: InternalsVisibleTo("Dev2.Activities.Designers.Tests")]
[assembly: InternalsVisibleTo("Dev2.CustomControls.Tests")]
[assembly: InternalsVisibleTo("Dev2.Activities.Specs")]
[assembly: InternalsVisibleTo("Dev2.Integration.Tests")]
[assembly: InternalsVisibleTo("Warewolf.HangfireServer.Tests")]
EOF
  echo "C Sharp version file written to $CSharpVersionFile."
  FSharpVersionFile="$PSScriptRoot/Dev/AssemblyCommonInfo.fs"
  echo "Writing F Sharp version file to $FSharpVersionFile..."
  cat > "$FSharpVersionFile" <<EOF
namespace Warewolf.FSharp
open System.Reflection
#nowarn
[<assembly: AssemblyCompany("Warewolf")>]
[<assembly: AssemblyProduct("Warewolf")>]
[<assembly: AssemblyCopyright("Copyright Warewolf $(date +%Y)")>]
[<assembly: AssemblyVersion("$FullVersionString")>]
do()
EOF
  echo "F Sharp version file written to $FSharpVersionFile."
  echo "Warewolf version written successfully! For more info about Warewolf versioning see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/"
fi

# Compile
if [[ -f "$PSScriptRoot/Dev/ServerTests.sln" ]]; then
  SolutionFileName=$(basename "Dev/ServerTests.sln")
  SolutionFileExtension=".${SolutionFileName##*.}"
  OutputFolderName="${SolutionFileName%Dev/ServerTests.slnExtension}"
  if [[ $ProjectSpecificOutputs -eq 1 ]]; then
    OutputProperty=""
  else
    if [[ -n "$FrameworkTarget" ]]; then
      OutputFolderName+="/$FrameworkTarget"
    fi
    OutputProperty="\"-property:OutDir=$PSScriptRoot/Bin/$OutputFolderName\""
  fi
  if [[ $InContainer -eq 0 ]]; then
    # Build the command as an array for proper argument handling
    MSBUILD_CMD=()
    # Split MSBuildPath if it contains spaces (e.g., "dotnet msbuild")
    read -ra MSBUILD_PATH_ARR <<< "$MSBuildPath"
    MSBUILD_CMD+=("${MSBUILD_PATH_ARR[@]}")
    MSBUILD_CMD+=("$PSScriptRoot/Dev/ServerTests.sln" -t:Restore)
    # Run restore
    "${MSBUILD_CMD[@]}"

    # Build command for build
    MSBUILD_CMD=()
    MSBUILD_CMD+=("${MSBUILD_PATH_ARR[@]}")
    MSBUILD_CMD+=("$PSScriptRoot/Dev/ServerTests.sln")
    if [[ $Disablemaxcpucount -eq 0 ]]; then
      MSBUILD_CMD+=("-maxcpucount")
    fi
    MSBUILD_CMD+=("-p:Platform=Any CPU" "-p:Configuration=$Config")
    [[ -n "$FrameworkTarget" ]] && MSBUILD_CMD+=("-p:TargetFramework=$FrameworkTarget")
    [[ -n "$OutputProperty" ]] && MSBUILD_CMD+=("$OutputProperty")
    [[ -n "$Target" ]] && MSBUILD_CMD+=("$Target")
    # Run build
    "${MSBUILD_CMD[@]}"
  else
    docker run -t -m 4g -v "$PSScriptRoot:/Build" registry.gitlab.com/warewolf/msbuild "/Build/Dev/ServerTests.sln" "-p:Platform=Any CPU;Configuration=$Config$FrameworkTarget" $OutputProperty $Target
  fi
  if [[ $? -ne 0 ]]; then
    echo "Build failed. Check your pending changes. Compiling Warewolf requires at least MSBuild 15.0 and FSharp 4.0."
    exit 1
  fi
  # Dockerfile for net6.0
  if [[ "$FrameworkTarget" == "net6.0" ]]; then
    DockerfileContent="FROM mcr.microsoft.com/dotnet/sdk:6.0\n\nEXPOSE 3142\nEXPOSE 3143\n\nADD . Server\nENV SERVER_PATH \"Server/Warewolf Server.exe\"\nENV SERVER_WORKINGDIR \"/programdata/Warewolf\"\nENV SERVER_LOG \"/programdata/Warewolf/Server Log/warewolf-server.log\"\nENV SERVER_USERNAME \"WarewolfAdmin\"\nENV SERVER_PASSWORD \"W@rEw0lf@dm1n\"\n\n# Run the application\nCMD [\"dotnet\", \"./Server/Warewolf Server.dll\"]\n"
    OutputFile="$OutputFolderName/Dockerfile"
    mkdir -p "$OutputFolderName"
    echo -e "$DockerfileContent" > "$OutputFile"
  fi
fi
exit 0
