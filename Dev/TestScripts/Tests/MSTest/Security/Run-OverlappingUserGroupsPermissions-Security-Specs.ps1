# Read playlists and args.
$TestList = ""
if ($Args.Count -gt 0) {
    $TestList = $Args.ForEach({ "," + $_ })
} else {
    Get-ChildItem "$PSScriptRoot" -Filter *.playlist | `
    Foreach-Object{
	    [xml]$playlistContent = Get-Content $_.FullName
	    if ($playlistContent.Playlist.Add.count -gt 0) {
	        foreach( $TestName in $playlistContent.Playlist.Add) {
		        $TestList += " /test:" + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	        }
	    } else {        
            if ($playlistContent.Playlist.Add.Test -ne $null) {
                $TestList = " /test:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
            } else {
	            Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
            }
        }
    }
}
if ($TestList.StartsWith(",")) {
	$TestList = $TestList -replace "^.", " /test:"
}

# Create test settings.
$TestSettingsFile = "$PSScriptRoot\OverlappingUserGroupsPermissionsSecurity.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`"
  name="OverlappingUserGroupsPermissionsSecurity"
  enableDefaultDataCollectors="false"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Overlapping User Groups Permissions Security Specs.</Description>
  <Deployment enabled="false" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)

# Find test assembly
$TestAssemblyPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.SecuritySpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.SecuritySpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.SecuritySpecs.dll"
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Warewolf.SecuritySpecs.dll at $PSScriptRoot\Warewolf.SecuritySpecs\bin\Debug or $PSScriptRoot
	exit 1
}
if (!(Test-Path $PSScriptRoot\TestResults)) {
	New-Item -ItemType Directory $PSScriptRoot\TestResults
}

# Create full VSTest argument string.
if ($TestList -eq "") {
	# Create full VSTest argument string.
	$FullArgsList = " /testcontainer:`"" + $TestAssemblyPath + "`" /resultsfile:" + $PSScriptRoot + "\TestResults\ResourcePermissionsSecuritySpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + " /category:`"OverlappingUserGroupsPermissionsSecurity`""
} else {
	# Create full VSTest argument string.
	$FullArgsList = " /testcontainer:`"" + $TestAssemblyPath + "`" /resultsfile:" + $PSScriptRoot + "\TestResults\ResourcePermissionsSecuritySpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + $TestList
}

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList