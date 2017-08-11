Param(
  [string]$MSBuildPath="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe",
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
                      "$PSScriptRoot\Dev\Web2.sln"
$NoSolutionParametersPresent = !($AcceptanceTesting.IsPresent) -and !($UITesting.IsPresent) -and !($Server.IsPresent) -and !($Studio.IsPresent) -and !($Release.IsPresent) -and !($Web.IsPresent)
if ($Target -ne "") {
	$Target = "/t:" + $Target
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
        Write-Host vswhere.exe not found. Download from: https://github.com/Microsoft/vswhere/releases
    }
	$MSBuildPath = &$VswherePath -latest -requires Microsoft.Component.MSBuild -version 15.0
    $StartKey = "installationPath: "
    $EndKey = " installationVersion: "
    $SubstringStart = $MSBuildPath.IndexOf($StartKey) + $StartKey.Length
    $MSBuildPath = $MSBuildPath.Substring($SubStringStart, $MSBuildPath.IndexOf($EndKey) - $SubStringStart) + "\MSBuild\15.0\Bin\MSBuild.exe"
}
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
	Write-Host MSBuild not found. Download from: https://aka.ms/vs/15/release/vs_buildtools.exe
    sleep 10
    exit 1
}

#Find NuGet
if ("$NuGetPath" -eq "" -or !(Test-Path "$NuGetPath" -ErrorAction SilentlyContinue)) {
    $NuGetCommand = Get-Command NuGet -ErrorAction SilentlyContinue
    if ($NuGetCommand) {
        $NuGetPath = $NuGetCommand.Path
    }
}
if ("$NuGetPath" -eq "" -or !(Test-Path "$NuGetPath" -ErrorAction SilentlyContinue)) {
	Write-Host NuGet not found. Download from: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    sleep 10
    exit 1
}

#Version
if ($AutoVersion.IsPresent -or $CustomVersion -ne "") {
    Write-Host Writing C# and F# versioning files...

    # Get all the latest version tags from server repo.
    git -C "$PSScriptRoot" fetch --tags

    # Generate informational version.
    # (from git commit id and time)
    $GitCommitID = git -C "$PSScriptRoot" rev-parse HEAD
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
		    if ($FullVersionString -Match " ") {
			    # This commit has more than one tag, using first tag
			    Write-Host This commit has more than one tags as `"$FullVersionString`".
			    $FullVersionString = $FullVersionString.Split(" ")[0]
			    Write-Host Using last tag as `"$FullVersionString`".
		    }
		    # This version is already tagged.
		    Write-Host You are up to date with version `"$FullVersionString`".
	    } else {
		    # This version is not already tagged.
		    Write-Host This version is not tagged, generating new tag...
		    # Get last known version
		    $FullVersionString = git -C "$PSScriptRoot" describe --abbrev=0 --tags
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
				    $NewBuildNumber++
				    $FullVersionString = $FullVersionString.Split(".")[0] + "." + $FullVersionString.Split(".")[1] + "." + $FullVersionString.Split(".")[2] + "." + $NewBuildNumber
				    Write-Host Next version would be `"$FullVersionString`". Checking against Origin repo...
				    # Check tag against origin
				    $originTag = git -C "$PSScriptRoot" ls-remote --tags origin $FullVersionString
				    if ($originTag.length -ne 0) {
					    Write-Host Origin has tag `"$originTag`".
				    } else {
					    Write-Host Double checking with hard coded integration manager repo...
					    # Check tag against hard coded integration manager repo
					    $originTag = git -C "$PSScriptRoot" ls-remote --tags "file:////rsaklfsvrdev/Git-Repositories/Warewolf" $FullVersionString
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
    $Line1 = "using System.Reflection;"
    $Line2 = "[assembly: AssemblyCompany(""Warewolf"")]"
    $Line3 = "[assembly: AssemblyProduct(""Warewolf"")]"
    $Line4 = "[assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)]"
    $Line5 = "[assembly: AssemblyVersion(""" + $FullVersionString + """)]"
    $Line6 = "[assembly: AssemblyInformationalVersion(""" + $GitCommitTime + " " + $GitCommitID + " " + $GitBranchName + """)]"
    $Line7 = "[assembly: InternalsVisibleTo(""Dev2.Activities.Designers.Tests"")]"
    $Line8 = "[assembly: InternalsVisibleTo(""Warewolf.Studio.ViewModels.Tests"")]"
    Write-Host $Line1
    $Line1 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Force
    Write-Host $Line2
    $Line2 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line3
    $Line3 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line4
    $Line4 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line5
    $Line5 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line6
    $Line6 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
    Write-Host C Sharp version file written to `"$CSharpVersionFile`".

    $FSharpVersionFile = "$PSScriptRoot\Dev\AssemblyCommonInfo.fs"
    Write-Host Writing F Sharp version file to `"$FSharpVersionFile`" as...
    $Line1 = "namespace Warewolf.FSharp"
    $Line2 = "open System.Reflection;"
    $Line3 = "[<assembly: AssemblyCompany(""Warewolf"")>]"
    $Line4 = "[<assembly: AssemblyProduct(""Warewolf"")>]"
    $Line5 = "[<assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)>]"
    $Line6 = "[<assembly: AssemblyVersion(""" + $FullVersionString + """)>]"
    # Ashley: F# Compiler thinks this is invalid for some reason
    #$Line7 = "[<assembly: AssemblyInformationalVersion(""" + $GitCommitTime + " " + $GitCommitID + """)>]"
    $Line7 = "do()"
    Write-Host $Line1
    $Line1 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Force
    Write-Host $Line2
    $Line2 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line3
    $Line3 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line4
    $Line4 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line5
    $Line5 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line6
    $Line6 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
    Write-Host $Line7
    $Line7 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
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
			&"$PSScriptRoot\Dev\packages\SpecFlow.2.1.0\tools\specflow.exe" "generateAll" "$FullPath\$ProjectName.csproj" "/force" "/verbose"
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
        if ((Get-Variable "$OutputFolderName*" -ValueOnly).IsPresent -or $NoSolutionParametersPresent) {
            if ($SolutionWideOutputs.IsPresent) {
                $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\$OutputFolderName"
            } else {
                $OutputProperty = ""
            }
            &"$NuGetPath" "restore" "$SolutionFile"
            &"$MSBuildPath" "$SolutionFile" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
            if ($LASTEXITCODE -ne 0) {
				Write-Host Build failed. Check your pending changes. If you do not have any pending changes then you can try running 'dev\scorched get.bat' before retrying. Compiling Warewolf requires at at least MSBuild 15.0, download from: https://aka.ms/vs/15/release/vs_buildtools.exe and FSharp 4.0, download from http://download.microsoft.com/download/9/1/2/9122D406-F1E3-4880-A66D-D6C65E8B1545/FSharp_Bundle.exe
                sleep 30
                exit 1
            }
        }
    }
}
exit 0