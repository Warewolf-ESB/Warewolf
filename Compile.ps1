Param(
  [string]$MSBuildPath="%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe",
  [string]$Solution="All",
  [string]$Config="Debug",
  [switch]$ProjectSpecificOutputs
)

if ($Solution -ne "All" -and $Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    Write-Host "-Solution must either be left blank to compile all solutions or AcceptanceTesting, UITesting, Server, Studio or Release to compile just one solution."
    sleep 10
    exit 1
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
if ($Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\AcceptanceTesting"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\AcceptanceTesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\UITesting"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\UITesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Server"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Server.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Release") {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Studio"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Studio.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio") {
    if ($ProjectSpecificOutputs.IsPresent) {
        $OutputProperty = ""
    } else {
        $OutputProperty = "/property:OutDir=$PSScriptRoot\Bin\Release"
    }
    &"$MSBuildPath" "$PSScriptRoot\Dev\Release.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" $OutputProperty
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}
exit 0