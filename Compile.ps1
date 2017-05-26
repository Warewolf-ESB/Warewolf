Param(
  [string]$MSBuildPath,
  [string]$Solution,
  [String]$Config="Debug"
)

if ($Solution -ne "" -and $Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    Write-Host "-Solution must either be left blank to compile all solutions or AcceptanceTesting, UITesting, Server, Studio or Release to compile just one solution."
    sleep 10
    exit 1
}

#Find Compiler
if ("$MSBuildPath" -eq "") {
    $GetMSBuildCommand = Get-Command MSBuild -ErrorAction SilentlyContinue
    if ($GetMSBuildCommand) {
        $MSBuildPath = $GetMSBuildCommand.Path
    } else {
        $MSBuildPath = &"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe"
    }
}
if (!(Test-Path "$MSBuildPath" -ErrorAction SilentlyContinue)) {
	Write-Host MSBuild not found. Download from: https://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe
    sleep 10
    exit 1
}

#Start Compiling All 4 Solutions
$LASTEXITCODE = 0
if ($Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    &"$MSBuildPath" "$PSScriptRoot\Dev\AcceptanceTesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/property:OutDir=$PSScriptRoot\Bin\AcceptanceTesting"
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "Server" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    &"$MSBuildPath" "$PSScriptRoot\Dev\UITesting.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/property:OutDir=$PSScriptRoot\Bin\UITesting"
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Studio" -and $Solution -ne "Release") {
    &"$MSBuildPath" "$PSScriptRoot\Dev\Server.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/property:OutDir=$PSScriptRoot\Bin\Server"
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Release") {
    &"$MSBuildPath" "$PSScriptRoot\Dev\Studio.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/property:OutDir=$PSScriptRoot\Bin\Studio"
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}

if ($Solution -ne "AcceptanceTesting" -and $Solution -ne "UITesting" -and $Solution -ne "Server" -and $Solution -ne "Studio") {
    &"$MSBuildPath" "$PSScriptRoot\Dev\Release.sln" "/p:Platform=`"Any CPU`";Configuration=`"$Config`"" "/maxcpucount" "/property:OutDir=$PSScriptRoot\Bin\Release"
}
if ($LASTEXITCODE -ne 0) {
    sleep 10
    exit 1
}
exit 0