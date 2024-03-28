Param(
  [string]$MSBuildPath="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
  [string]$Target="",
  [string]$CustomVersion="",
  [string]$NuGet="",
  [string]$Config="Debug",
  [switch]$AutoVersion,
  [switch]$ProjectSpecificOutputs,
  [switch]$AcceptanceTesting,
  [switch]$UITesting,
  [switch]$Server,
  [switch]$Studio,
  [switch]$StudioProject,
  [switch]$COMIPCProject,
  [switch]$Release,
  [switch]$Web,
  [switch]$NewServerNet6,
  [switch]$ServerTests,
  [switch]$RegenerateSpecFlowFeatureFiles,
  [switch]$InContainer,
  [string]$GitCredential,
  [switch]$ForceMultitargetting
)
$KnownSolutionFiles = "Dev\AcceptanceTesting.sln",
					  "Dev\UITesting.sln",
					  "Dev\Server.sln",
					  "Dev\Studio.sln",
					  "Dev\Release.sln",
					  "Dev\Web.sln",
					  "Dev\NewServerNet6.sln",
					  "Dev\ServerTests.sln",
					  "Dev\Dev2.Studio\Dev2.Studio.csproj",
					  "Dev\Warewolf.COMIPC\Warewolf.COMIPC.csproj"
$NoSolutionParametersPresent = !($AcceptanceTesting.IsPresent) -and !($UITesting.IsPresent) -and !($Server.IsPresent) -and !($Studio.IsPresent) -and !($Release.IsPresent) -and !($Web.IsPresent) -and !($RegenerateSpecFlowFeatureFiles.IsPresent) -and !($NewServerNet6.IsPresent) -and !($ServerTests.IsPresent) -and !($StudioProject.IsPresent) -and !($COMIPCProject.IsPresent)
if ($Target -ne "") {
	$Target = "/t:" + $Target
}

#find script
if ("$PSScriptRoot" -eq "" -or $PSScriptRoot -eq $null) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

if ($ForceMultitargetting.IsPresent) {
	$path = "$PSScriptRoot\Dev\"
	$files = Get-ChildItem -Path $path -Include *.csproj,*.fsproj -Recurse

	foreach ($file in $files) {
		$xml = [xml](Get-Content $file.FullName)

		# Replace target framework nodes
		$nodes = $xml.SelectNodes("//TargetFramework[.='net6.0-windows'] | //TargetFrameworks[.='net6.0-windows']")
		foreach ($node in $nodes) {
			$node.'#text' = 'net6.0-windows;net48'
            $newNode = $xml.CreateElement("TargetFrameworks")
            $newNode.InnerText = 'net6.0-windows;net48'
            $node.ParentNode.ReplaceChild($newNode, $node)
		}

		# Special handling for Dev2.Data.csproj
		if ($file.Name -eq 'Dev2.Data.csproj') {
			$refNodes = $xml.SelectNodes("//Reference[@Include='Infragistics.Calculations'] | //PackageReference[@Include='System.Configuration.ConfigurationManager' and @Version='6.0.1'] | //FrameworkReference[@Include='Microsoft.AspNetCore.App']")
			$itemGroupNode = $xml.CreateElement("ItemGroup")
			$conditionAttr = $xml.CreateAttribute("Condition")
			$conditionAttr.Value = "'$(TargetFrameworkIdentifier)' != '.NETFramework'"
			$itemGroupNode.Attributes.Append($conditionAttr)

			foreach ($refNode in $refNodes) {
				$refNode.ParentNode.RemoveChild($refNode)
				$itemGroupNode.AppendChild($refNode)
			}

			$xml.Project.AppendChild($itemGroupNode)
		}

		$xml.Save($file.FullName)
	}
}

if (!($InContainer.IsPresent)) {
	#Find Local NuGet
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
	
	#Find Local Compiler
	if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
		$GetMSBuildCommand = Get-Command MSBuild -ErrorAction SilentlyContinue
		if ($GetMSBuildCommand) {
			$MSBuildPath = $GetMSBuildCommand.Path
		}
	}
	if ($MSBuildPath -ne $null -and !(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
		$GetvswhereCommand = Get-Command vswhere -ErrorAction SilentlyContinue
		if ($GetvswhereCommand) {
			$VswherePath = $GetvswhereCommand.Path
		} else {
			if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe") {
				$VswherePath = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
			} else {
				&"$NuGet" install vswhere -ExcludeVersion -NonInteractive -OutputDirectory "$env:windir"
				$VswherePath = "$env:windir\vswhere\tools\vswhere.exe"
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
		$GetMSBuildCommand = reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0" /v MSBuildToolsPath
		$GetMSBuildCommand = $GetMSBuildCommand[2].Substring(34, $GetMSBuildCommand[2].Length-34) + "msbuild.exe"
	}
	if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
		$env:MSBuildPath = Read-Host 'Please enter the path to MSBuild.exe. For example: C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe. Or change the value of the MSBuildPath environment variable to be the path to MSBuild.exe'
		if ("$env:MSBuildPath" -ne "" -and (Test-Path "$env:MSBuildPath")) {
			$MSBuildPath = $env:MSBuildPath
			[System.Environment]::SetEnvironmentVariable("MSBuildPath", $MSBuildPath, "Machine")
		} else {
			Write-Host MSBuild not found. Download from: https://aka.ms/vs/15/release/vs_buildtools.exe
			sleep 10
			exit 1
		}
	}
}

#Version
$GitCommitID = git -C "$PSScriptRoot" rev-parse HEAD
if ($AutoVersion.IsPresent -or $CustomVersion -ne "") {
	Write-Host Writing C# and F# versioning files...

	if ($GitCredential -ne "") {
		git -C "$PSScriptRoot" remote set-url origin https://$GitCredential@gitlab.com/warewolf/warewolf
	}
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
			$AllTags = git -C "$PSScriptRoot" tag -l --sort=-creatordate --merged
			foreach ($element in $AllTags) {
				$dotCount = $element.Split('.').Count - 1
				if ($dotCount -eq 3) {
					$FullVersionString = $element
					break
				}
			}
			if ([string]::IsNullOrEmpty($FullVersionString) -or $dotCount -ne 3) {
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
	if (Test-Path "$PSScriptRoot\$SolutionFile") {
		$GetSolutionFileInfo = Get-Item "$PSScriptRoot\$SolutionFile"
		$SolutionFileName = $GetSolutionFileInfo.Name
		$SolutionFileExtension = $GetSolutionFileInfo.Extension
		$OutputFolderName = $SolutionFileName.TrimEnd($SolutionFileExtension).TrimStart("Dev2.").TrimEnd("2")
		if ($OutputFolderName -eq "Studi") {
			$OutputFolderName = "StudioProject"
		}
		if ($OutputFolderName -eq "ServerTest") {
			$OutputFolderName = "ServerTests"
		}
		if ($OutputFolderName -eq "Warewolf.COMIPC") {
			$OutputFolderName = "COMIPCProject"
		}
		if ((Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent.Length -gt 1) {
			$SolutionParameterIsPresent = (Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent[0]
		} else {
			$SolutionParameterIsPresent = (Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent
		}
		if ($SolutionParameterIsPresent -or $NoSolutionParametersPresent) {
			if ($OutputFolderName -eq "Webs") {
				npm install --add-python-to-path='true' --global --production windows-build-tools
			}
			if (($OutputFolderName -eq "AcceptanceTesting" -or $OutputFolderName -eq "ServerTests") -and !($ProjectSpecificOutputs.IsPresent)) {
				&"$NuGet" install Microsoft.TestPlatform -ExcludeVersion -NonInteractive -OutputDirectory "$PSScriptRoot\Bin\$OutputFolderName" -Version "17.2.0"
			}
			if ($ProjectSpecificOutputs.IsPresent) {
				$OutputProperty = ""
			} else {
				$OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\$OutputFolderName"
			}
			if (!($InContainer.IsPresent)) {
				Write-Host "$MSBuildPath" "$PSScriptRoot\$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/nodeReuse:false" "/restore" $OutputProperty $Target
				&"$MSBuildPath" "$PSScriptRoot\$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/nodeReuse:false" "/restore" $OutputProperty $Target
			} else {
				docker run -t -m 4g -v "$PSScriptRoot":"C:\Build" registry.gitlab.com/warewolf/msbuild "C:\Build\$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"$NugetPackVersion" "/maxcpucount" "/nodeReuse:false" "/restore" $OutputProperty $Target
			}
			if ($LASTEXITCODE -ne 0) {
				Write-Host Build failed. Check your pending changes. If you do not have any pending changes then you can try running 'dev\scorch.bat' to thoroughly clean your workspace. Compiling Warewolf requires at at least MSBuild 15.0, download from: https://aka.ms/vs/15/release/vs_buildtools.exe and FSharp 4.0, download from http://download.microsoft.com/download/9/1/2/9122D406-F1E3-4880-A66D-D6C65E8B1545/FSharp_Bundle.exe
				exit 1
			}
			if ($OutputFolderName -ne "COMIPCProject" -and $OutputFolderName -ne "StudioProject") {
				if (!($ProjectSpecificOutputs.IsPresent)) {
					if ($Target -eq "/t:Debug" -or $Target -eq "") {
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
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - Release\Resources" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - Release\Tests" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - Release" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - ServerTests" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - UITests" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse
						Copy-Item -Path "$PSScriptRoot\Dev\Resources - Load" -Destination "$PSScriptRoot\Bin\$OutputFolderName" -Force -Recurse

						if (!(Test-Path "$PSScriptRoot\Bin\$OutputFolderName\_PublishedWebsites\Dev2.Web")) {
							Copy-Item -Path "$PSScriptRoot\Dev\Dev2.Web2" "$PSScriptRoot\Bin\$OutputFolderName\_PublishedWebsites\Dev2.Web" -Force -Recurse
						}
						Copy-Item -Path "$PSScriptRoot\TestRun.ps1" "$PSScriptRoot\Bin\$OutputFolderName\TestRun.ps1" -Force
					}
					Copy-Item -Path "$PSScriptRoot\Bin\$OutputFolderName\runtimes\win-x64\native\SQLite.Interop.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\SQLite.Interop.dll" -Force
					Copy-Item -Path "$PSScriptRoot\Dev\Server Tests Setup\sni.dll" -Destination "$PSScriptRoot\Bin\$OutputFolderName\sni.dll" -Force
					if (!(Test-Path "$PSScriptRoot\Bin\$OutputFolderName\testhost.dll.config")) {
						@"
<?xml version="1.0" encoding="utf-8"?>

<configuration>
	<configSections>
		<section name="secureSettings" type="System.Configuration.NameValueSectionHandler" />
		<section name="entityFramework"
				 type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
				 requirePermission="false" />
	</configSections>
	<runtime>
		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
			<dependentAssembly>
				<assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
				<bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
			</dependentAssembly>
		</assemblyBinding>
	</runtime>
	<secureSettings>
		<add key="ServerID" value="d53bbcc5-4794-4dfa-b096-3aa815692e66" />
		<add key="ServerKey"
			 value="BwIAAACkAABSU0EyAAQAAAEAAQBBgKRIdzPGpaPt3hJ7Kxm8iVrKpfu4wfsJJf/3gBG5qhiS0rs5j5HqkLazdO5az9oPWnSTmNnww03WvCJhz8nhaJjXHoEK6xtcWL++IY+R3E27xaHaPQJSDvGg3j1Jvm0QKUGmzZX75tGDC4s17kQSCpsVW3vEuZ5gBdMLMi3UqaVW9EO7qOcEvVO9Cym7lxViqUhvq6c0nLzp6C6zrtZGjLtFqo9KDj7PMkq10Xc0JkzE1ptRz/YytMRacIDn8tptbHbxM8AtObeeiZ7V6Tznmi82jcAm2Jugr0D97Da2MXZuqEKLL5uPagL4RUHite3kT/puSNbTtqZLdqMtV5HGqVmn2a64JU3b8TIW8rKd5rKucG4KwoXRNQihJzX1it8vcqt6tjDnJZdJkuyDjdd6BKCYHWeX9mqDwKJ3EY+TRZmsl9RILyV/XviyrpTYBdDDmdQ9YLSLt0LtdpPpcRzciwMsBEfMH5NPFOtqSF/151Sl/DBdEJxOINXDl1qdO5MtgL7rXkfiGwu66n4hokRdVlj6TTcXTCn6YrUbzOts6IZJnQ9cwK693u9yMJ3Di0hp49L6LWnoWmW334ys+iOfob0i4eM+M3XNw7wGN/jd6t2KYemVZEnTcl5Lon5BpdoFlxa7Y1n+kXbaeSAwTJIe9HM6uoXIH61VCIk0ac69oZcG2/FhSeBO/DcGIQQqdFvuFqJY0g2qbt7+hmEZDZBehr3KpoRTgB5xPW/ThVhuaoZxlpEb4hFmKoj900knnQk=" />
		<add key="SystemKey"
			 value="BgIAAACkAABSU0ExAAQAAAEAAQBzb9y6JXoJj70+TVeUgRc7hPjb6tTJR7B/ZHZKFQsTLkhQLHo+93x/f30Lj/FToE2xXqnuZPk9IV94L4ekt+5jgEFcf1ReuJT/G1dVb1POiEC0upGdagwW10T3PcBK+UzfSXz5kD0SiGhXamPnT/zuHiTtVjv87W+5WuvU1vsrsQ==" />
	</secureSettings>
	<appSettings>
		<add key="webServerPort" value="1234" />
		<add key="webServerSslPort" value="1236" />
		<add key="webServerEnabled" value="true" />
		<add key="SupportedFileExtensions" value=".js,.css,.jpg,.jpeg,.bmp,.bm,.gif,.ico,.tiff,.png" />
		<add key="Hello World" value="acb75027-ddeb-47d7-814e-a54c37247ec1" />
		<add key="ForEachWithHelloWorldTest" value="c5381f15-c5e5-41a9-b322-ab7b2a891aa5" />
		<add key="Control Flow - Sequence" value="0bdc3207-ff6b-4c01-a5eb-c7060222f75d" />
		<add key="Loop Constructs - For Each" value="8ba79b49-226e-4c67-a732-4657fd0edb6b" />
		<add key="Loop Constructs - Select and Apply" value="ea916fa6-76ca-4243-841c-74fa18dd8c14" />
		<add key="Select and Apply" value="b65bd0c2-4b17-426d-a515-3891ab8c4a93" />
		<add key="DllSourceForExecutionSpecs" value="D9017469-ADAD-40F9-AAD2-F1F0A0D2614B" />
		<add key="testRabbitMQSource" value="78899836-7c8f-4a8e-8c0a-b45eba3a522e" />
		<add key="SharePoint Test Server" value="94d4b4ca-31e1-494d-886b-cd94224c9a8b" />
		<add key="SecuritySpecsUser" value="SecuritySpecsUser" />
		<add key="SecuritySpecsPassword" value="ASfas123@!fda" />
		<add key="userGroup" value="Administrators" />

	</appSettings>
	<connectionStrings>
		<add name="Persistence"
			 connectionString="Server=RSAKLFSVRDEV;Database=WFPersistenceStore;Integrated Security=true" />
	</connectionStrings>
	

</configuration>
"@ | Out-File -LiteralPath "$PSScriptRoot\Bin\$OutputFolderName\testhost.dll.config" -Encoding utf8 -Force
					}
				}
			}
		}
	}
}
exit 0
