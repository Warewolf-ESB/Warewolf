Param(
  [string]$MSBuildPath="%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe",
  [string]$Config="Debug",
  [string]$Target="",
  [switch]$ProjectSpecificOutputs,
  [switch]$AcceptanceTesting,
  [switch]$UITesting,
  [switch]$Server,
  [switch]$Studio,
  [switch]$Release
)
$NoSolutionParametersPresent = !($AcceptanceTesting.IsPresent) -and !($UITesting.IsPresent) -and !($Server.IsPresent) -and !($Studio.IsPresent) -and !($Release.IsPresent)
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
	Write-Host MSBuild not found. Download from: https://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe
    sleep 10
    exit 1
}

#Compile Solutions
$LASTEXITCODE = 0
if ($AcceptanceTesting.IsPresent -or $NoSolutionParametersPresent) {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\AcceptanceTesting"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\AcceptanceTesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($UITesting.IsPresent -or $NoSolutionParametersPresent) {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\UITesting"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\UITesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Server.IsPresent -or $NoSolutionParametersPresent) {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Server"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Server.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Studio.IsPresent -or $NoSolutionParametersPresent) {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Studio"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Studio.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Release.IsPresent -or $NoSolutionParametersPresent) {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Release"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Release.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty $Target
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}
exit 0