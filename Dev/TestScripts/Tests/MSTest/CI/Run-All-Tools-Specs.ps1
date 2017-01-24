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
	$TestList = $TestList -replace "^.", " /Tests:"
}

# Create test settings.
$TestSettingsFile = "$PSScriptRoot\LocalUITesting.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding=`"UTF-8`"?>
<TestSettings name=`"Tools Specs`" id=`"
"@ + [guid]::NewGuid() + @"
`" xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>These are default test settings for a local test run.</Description>
  <NamingScheme baseName=`"ToolsSpecs`" appendTimeStamp=`"false`" useDefault=`"false`" />
  <Execution>
    <Hosts skipUnhostableTests=`"false`" />
    <TestTypeSpecific>
      <UnitTestRunConfig testTypeId=`"13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b`">
        <AssemblyResolution>
          <TestDirectory useLoadContext=`"true`" />
        </AssemblyResolution>
      </UnitTestRunConfig>
    </TestTypeSpecific>
    <AgentRule name=`"LocalMachineDefaultRole`">
    </AgentRule>
  </Execution>
</TestSettings>
"@)

# Find test assembly
$TestAssemblyPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.ToolsSpecs.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.ToolsSpecs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.ToolsSpecs.dll"
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Warewolf.ToolsSpecs.dll at $PSScriptRoot\Warewolf.ToolsSpecs\bin\Debug or $PSScriptRoot
	exit 1
} else {
	New-Item -ItemType Directory $PSScriptRoot\TestResults
}

# Create full MSTest argument string.
$FullArgsList = " /testcontainer:`"" + $TestAssemblyPath + "`" /resultsfile:`"" + $PSScriptRoot + "\TestResults\ToolsSpecsResults.trx`"" + $TestList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList