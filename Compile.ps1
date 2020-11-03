Param(
  [string]$MSBuildPath="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
  [string]$Target="",
  [string]$CustomVersion="",
  [string]$NuGet="",
  [string]$Config="Debug",
  [switch]$AutoVersion,
  [switch]$SolutionWideOutputs,
  [switch]$AcceptanceTesting,
  [switch]$UITesting,
  [switch]$Server,
  [switch]$Studio,
  [switch]$Release,
  [switch]$Web,
  [switch]$RegenerateSpecFlowFeatureFiles
)
$KnownSolutionFiles = "$PSScriptRoot\Dev\AcceptanceTesting.sln",
                      "$PSScriptRoot\Dev\UITesting.sln",
                      "$PSScriptRoot\Dev\Server.sln",
                      "$PSScriptRoot\Dev\Studio.sln",
                      "$PSScriptRoot\Dev\Release.sln",
                      "$PSScriptRoot\Dev\Web.sln"
$NoSolutionParametersPresent = !($AcceptanceTesting.IsPresent) -and !($UITesting.IsPresent) -and !($Server.IsPresent) -and !($Studio.IsPresent) -and !($Release.IsPresent) -and !($Web.IsPresent) -and !($RegenerateSpecFlowFeatureFiles.IsPresent)
if ($Target -ne "") {
	$Target = " /t:" + $Target
}

#find script
if ("$PSScriptRoot" -eq "" -or $PSScriptRoot -eq $null) {
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

#Find Compiler
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
    $GetMSBuildCommand = Get-Command MSBuild -ErrorAction SilentlyContinue
    if ($GetMSBuildCommand) {
        $MSBuildPath = $GetMSBuildCommand.Path
    }
}
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
    $GetvswhereCommand = Get-Command vswhere -ErrorAction SilentlyContinue
    if ($GetvswhereCommand) {
        $VswherePath = $GetvswhereCommand.Path
    } else {
        if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe") {
            $VswherePath = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
        } else {
            wget "https://github.com/Microsoft/vswhere/releases/download/2.5.2/vswhere.exe" -OutFile "$env:windir\vswhere.exe"
            $VswherePath = "$env:windir\vswhere.exe"
        }
    }
	[xml]$GetMSBuildPath = &$VswherePath -latest -requires Microsoft.Component.MSBuild -version 15.0 -format xml    
    if ($GetMSBuildPath -ne $null) {
        $MSBuildPath = $GetMSBuildPath.instances.instance.installationPath + "\MSBuild\15.0\Bin\MSBuild.exe"
    }
}
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
    if (Test-Path $MSBuildPath.Replace("Enterprise", "Professional")) {
        $MSBuildPath = $MSBuildPath.Replace("Enterprise", "Professional")
    }
    if (Test-Path $MSBuildPath.Replace("Enterprise", "Community")) {
        $MSBuildPath = $MSBuildPath.Replace("Enterprise", "Community")
    }
    if (Test-Path $MSBuildPath.Replace("Enterprise", "BuildTools")) {
        $MSBuildPath = $MSBuildPath.Replace("Enterprise", "BuildTools")
    }
	if ("$env:MSBuildPath" -ne "" -and (Test-Path "$env:MSBuildPath")) {
		$MSBuildPath = $env:MSBuildPath
	}
}
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
	$env:MSBuildPath = Read-Host 'Please enter the path to MSBuild.exe. For example: C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe. Or change the value of the MSBuildPath environment variable to be the path to MSBuild.exe'
	if ("$env:MSBuildPath" -ne "" -and (Test-Path "$env:MSBuildPath")) {
		$MSBuildPath = $env:MSBuildPath
        [System.Environment]::SetEnvironmentVariable("MSBuildPath", $MSBuildPath, "Machine")
	} else {
		Write-Host MSBuild not found. Download from: https://aka.ms/vs/15/release/vs_buildtools.exe
		sleep 10
		exit 1
	}
}

#Find NuGet
if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
    $NuGetCommand = Get-Command NuGet -ErrorAction SilentlyContinue
    if ($NuGetCommand) {
        $NuGet = $NuGetCommand.Path
    }
}
if (("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) -and (Test-Path "$env:windir")) {
    wget "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "$env:windir\nuget.exe"
    $NuGet = "$env:windir\nuget.exe"
}
if ("$NuGet" -eq "" -or !(Test-Path "$NuGet" -ErrorAction SilentlyContinue)) {
	Write-Host NuGet not found. Download from: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to: c:\windows\nuget.exe. If you do not have permission to create c:\windows\nuget.exe use the -NuGet switch.
    sleep 10
    exit 1
}

#Version
$GitCommitID = git -C "$PSScriptRoot" rev-parse HEAD
if ($AutoVersion.IsPresent -or $CustomVersion -ne "") {
    Write-Host Writing C# and F# versioning files...

    # Get all the latest version tags from server repo.
    git -C "$PSScriptRoot" fetch --all --tags -f

    # Generate informational version.
    # (from git commit id and time)
    $GitCommitTimeObject = git -C "$PSScriptRoot" show -s --format="%ct" $GitCommitID
    if (($GitCommitTimeObject[0]).ToString().length -gt 1){
        $GitCommitTimeString = $GitCommitTimeObject[0]
    } else {
        $GitCommitTimeString = $GitCommitTimeObject
    }
    if ([string]::IsNullOrEmpty($GitCommitTimeString)) {
	    Write-Host Cannot resolve time of commit `"$GitCommitID`".
    } else {
        write-host Resolved time of commit `"$GitCommitID`" as `"$GitCommitTimeString`".
        $GitCommitTimeDouble = [Double]$GitCommitTimeString
        $origin = New-Object -Type DateTime -ArgumentList 1970, 1, 1, 0, 0, 0, 0
        $GitCommitTime = $origin.AddSeconds($GitCommitTimeDouble)
    }
    $GitBranchName = git -C "$PSScriptRoot" rev-parse --abbrev-ref HEAD
    if ($CustomVersion -ne "") {
	    $FullVersionString = "$CustomVersion"
    } else {
	    # Check if this version already tagged.
	    $FullVersionString = git -C "$PSScriptRoot" tag --points-at HEAD
	    if (-not [string]::IsNullOrEmpty($FullVersionString))  {
		    $FullVersionString = $FullVersionString.Trim()
            $MultiTags = $FullVersionString.Split("\n")
            if ($MultiTags.Count -gt 1) {
			    # This commit has more than one tag, using first tag
			    Write-Host This commit has more than one tags as `"$FullVersionString`".
                $FullVersionString = $MultiTags[-1]
			    Write-Host Using last tag as `"$FullVersionString`".
            }
		    # This version is already tagged.
		    Write-Host You are up to date with version `"$FullVersionString`".
	    } else {
		    # This version is not already tagged.
		    Write-Host This version is not tagged, generating new tag...
		    # Get last known version
            $AllTags = git -C "$PSScriptRoot" tag -l --sort=-version:refname
		    $FullVersionString = $AllTags[0]
		    if ([string]::IsNullOrEmpty($FullVersionString)) {
			    Write-Host No local tags found in git history. 
			    exit 1
		    } else {
			    $FullVersionString = $FullVersionString.Trim()
			    # Make new version from last known version.
			    Write-Host Last version was `"$FullVersionString`". Generating next version...
			    do {
				    # Increment build number.
				    [int]$NewBuildNumber = $FullVersionString.Split(".")[3]
                    if ($NewBuildNumber -eq $null) 
                    {
                        $NewBuildNumber = 0
                    }
                    else
                    {
                        $NewBuildNumber++
                    }
				    $FullVersionString = $FullVersionString.Split(".")[0] + "." + $FullVersionString.Split(".")[1] + "." + $FullVersionString.Split(".")[2] + "." + $NewBuildNumber
				    Write-Host Next version would be `"$FullVersionString`". Checking against Origin repo...
				    # Check tag against origin
				    $originTag = git -C "$PSScriptRoot" ls-remote --tags origin $FullVersionString
				    if ($originTag.length -ne 0) {
					    Write-Host Origin has tag `"$originTag`".
				    } else {
					    Write-Host Double checking with hard coded integration manager repo...
					    # Check tag against hard coded integration manager repo
					    $originTag = git -C "$PSScriptRoot" ls-remote --tags "https://gitlab.com/warewolf/warewolf" $FullVersionString
					    if ($originTag.length -ne 0) {
						    Write-Host Hard coded integration manager repo has tag `"$originTag`".
					    } else {
						    Write-Host Double checking with hard coded blessed repo...
						    # Check tag against hard coded blessed repo
						    $originTag = git -C "$PSScriptRoot" ls-remote --tags "https://github.com/Warewolf-ESB/Warewolf" $FullVersionString
						    if ($originTag.length -ne 0) {
							    Write-Host Hard coded blessed repo has tag `"$originTag`".
						    }
					    }
				    }
			    } while ($originTag.length -ne 0)
		    }
		    # New (unique) version has been generated.
		    Write-Host Origin has confirmed version `"$FullVersionString`" is OK.
	    }
    }
    # Write version files
    $CSharpVersionFile = "$PSScriptRoot\Dev\AssemblyCommonInfo.cs"
    Write-Host Writing C Sharp version file to `"$CSharpVersionFile`" as...
    $CSharpVersionFileContents = @"
using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable CC0021 // Use nameof
[assembly: AssemblyCompany(@"Warewolf")]
[assembly: AssemblyProduct(@"Warewolf")]
#pragma warning restore CC0021 // Use nameof
[assembly: AssemblyCopyright(@"Copyright Warewolf 
"@ + (Get-Date).year + @"
")]
[assembly: AssemblyVersion(@"
"@ + $FullVersionString + @"
")]
[assembly: AssemblyInformationalVersion(@"
"@ + $GitCommitTime + " " + $GitCommitID + " " + $GitBranchName + @"
")]
[assembly: InternalsVisibleTo("Dev2.Runtime.Tests")]
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
"@
    Write-Host $CSharpVersionFileContents
    $CSharpVersionFileContents | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Force
    Write-Host C Sharp version file written to `"$CSharpVersionFile`".

    $FSharpVersionFile = "$PSScriptRoot\Dev\AssemblyCommonInfo.fs"
    Write-Host Writing F Sharp version file to `"$FSharpVersionFile`" as...
    $FSharpVersionFileContents = @"
namespace Warewolf.FSharp
namespace Warewolf.FSharp
open System.Reflection;
#nowarn
[<assembly: AssemblyCompany(@"Warewolf")>]
[<assembly: AssemblyProduct(@"Warewolf")>]

[<assembly: AssemblyCopyright(@"Copyright Warewolf 
"@ + (Get-Date).year + @"
")>]
[<assembly: AssemblyVersion(@"
"@ + $FullVersionString + @"
")>]
do()
"@
    Write-Host $FSharpVersionFileContents
    $FSharpVersionFileContents | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Force
    Write-Host F Sharp version file written to `"$FSharpVersionFile`".
    Write-Host Warewolf version written successfully! For more info about Warewolf versioning see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/
}

if ($RegenerateSpecFlowFeatureFiles.IsPresent) {
	&"nuget.exe" "restore" "$PSScriptRoot\Dev\AcceptanceTesting.sln"
	if ($LASTEXITCODE -ne 0) {
        sleep 30
		exit 1
	}
	foreach ($ProjectDir in ((Get-ChildItem "$PSScriptRoot\Dev\*Specs") + (Get-ChildItem "$PSScriptRoot\Dev\Warewolf.UIBindingTests.*"))) {
		$FullPath = $ProjectDir.FullName
		$ProjectName = $ProjectDir.Name
		if (Test-Path "$FullPath\$ProjectName.csproj") {
			&"$env:userprofile\.nuget\packages\specflow\2.3.2\tools\specflow.exe" "generateAll" "$FullPath\$ProjectName.csproj" "/force" "/verbose"
		} else {
            Write-Warning -Message "Project file not found in folder $FullPath`nExpected it to be $FullPath\$ProjectName.csproj"
        }
	}
	foreach ($FeatureFile in (Get-ChildItem "$PSScriptRoot\Dev\**\*.feature.cs")) {
        (Get-Content $FeatureFile).replace('TestCategoryAttribute("MSTest:DeploymentItem:', 'DeploymentItem("').replace('DeploymentItem("Warewolf_Studio.exe")', 'DeploymentItem("Warewolf Studio.exe")') | Set-Content $FeatureFile
	}
}

#Compile Solutions
foreach ($SolutionFile in $KnownSolutionFiles) {
    if (Test-Path $SolutionFile) {
        $GetSolutionFileInfo = Get-Item $SolutionFile
        $SolutionFileName = $GetSolutionFileInfo.Name
        $SolutionFileExtension = $GetSolutionFileInfo.Extension
        $OutputFolderName = $SolutionFileName.TrimEnd("." + $SolutionFileExtension).TrimEnd("2")
        if ((Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent.Length -gt 1) {
            $SolutionParameterIsPresent = (Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent[0]
        } else {
            $SolutionParameterIsPresent = (Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent
        }
        if ($SolutionParameterIsPresent -or $NoSolutionParametersPresent) {
			if ($OutputFolderName -eq "Webs") {
				npm install --add-python-to-path='true' --global --production windows-build-tools
			}
            if ($SolutionWideOutputs.IsPresent) {
                $OutputProperty = " /property:OutDir=$PSScriptRoot\Bin\$OutputFolderName"
            } else {
                $OutputProperty = ""
            }
			if ($FullVersionString -ne $null -and $FullVersionString -ne "") {
				$NugetPackVersion = ";PackageVersion=$FullVersionString"
			} else {
				$NugetPackVersion = ""
			}
            &"$NuGet" "restore" "$SolutionFile"
            Write-Host `nDotNet Restore:
            dotnet restore "$SolutionFile"
			Write-Host Building like this: "$MSBuildPath" "$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"$NugetPackVersion" "/maxcpucount" "/nodeReuse:false"$OutputProperty$Target
            &"$MSBuildPath" "$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"$NugetPackVersion" "/maxcpucount" "/nodeReuse:false"$OutputProperty$Target
            if ($LASTEXITCODE -ne 0) {
				Write-Host Build failed. Check your pending changes. If you do not have any pending changes then you can try running 'dev\scorch.bat' to thoroughly clean your workspace. Compiling Warewolf requires at at least MSBuild 15.0, download from: https://aka.ms/vs/15/release/vs_buildtools.exe and FSharp 4.0, download from http://download.microsoft.com/download/9/1/2/9122D406-F1E3-4880-A66D-D6C65E8B1545/FSharp_Bundle.exe
                exit 1
            }
			if ($SolutionWideOutputs.IsPresent -and ($Target -eq "/t:Debug" -or $Target -eq "")) {
				if (Test-Path "$PSScriptRoot\Bin\$OutputFolderName\SQLite.Interop.dll") {
					Remove-Item -Path "$PSScriptRoot\Bin\$OutputFolderName\SQLite.Interop.dll" -Force
				}
				if (Test-Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll") {
					Copy-Item -Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll" -Force
				}
				if (Test-Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.dll") {
					Copy-Item -Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.dll" -Force
				}
				if (Test-Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.dll") {
					Copy-Item -Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.dll" -Force
				}
				if (Test-Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.TestFramework.dll") {
					Copy-Item -Path "$env:userprofile\.nuget\packages\mstest.testadapter\2.1.2\build\_common\Microsoft.VisualStudio.TestPlatform.TestFramework.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\Microsoft.VisualStudio.TestPlatform.TestFramework.dll" -Force
				}
				Copy-Item -Path "$PSScriptRoot\Dev\Resources - Release" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
				Copy-Item -Path "$PSScriptRoot\Dev\Resources - ServerTests" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
				Copy-Item -Path "$PSScriptRoot\Dev\Resources - UITests" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
				Copy-Item -Path "$PSScriptRoot\Dev\Resources - Load" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
				if (!(Test-Path "$PSScriptRoot\Bin\$OutputFolderName\_PublishedWebsites\Dev2.Web")) {
					Copy-Item -Path "$PSScriptRoot\Dev\Dev2.Web2" "$PSScriptRoot\Bin\$OutputFolderName\_PublishedWebsites\Dev2.Web" -Force -Recurse
				}
            }
        }
    }
}
exit 0