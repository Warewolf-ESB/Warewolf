if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$WorkingDir = (Get-Item $PSScriptRoot).FullName
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
		        $TestList += "," + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	        }
	    } else {        
            if ($playlistContent.Playlist.Add.Test -ne $null) {
                $TestList = " /Tests:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
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
$TestSettingsFile = "$PSScriptRoot\ShortcutKeysUITests.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"
"@ + [guid]::NewGuid() + @"
`"
  name=`"ShortcutKeysUITests`"
  enableDefaultDataCollectors=`"false`"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Shortcut Keys UI Tests.</Description>
  <Deployment enabled=`"false`" />
  <NamingScheme baseName=`"UI`" appendTimeStamp=`"false`" useDefault=`"false`" />
  <Execution>
    <Timeouts testTimeout=`"600000`" />
    <AgentRule name=`"LocalMachineDefaultRole`">
      <DataCollectors>
        <DataCollector uri=`"datacollector://microsoft/VideoRecorder/1.0`" assemblyQualifiedName=`"Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`" friendlyName=`"Screen and Voice Recorder`">
          <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase=`"false`" xmlns="" />
          </Configuration>
        </DataCollector>
      </DataCollectors>
    </AgentRule>
  </Execution>
</TestSettings>
"@)

# Find test assembly
$TestAssemblyPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.UITests\bin\Debug\Warewolf.UITests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.UITests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.UITests.dll"
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Warewolf.UITests.dll at $WorkingDir\Warewolf.UITests\bin\Debug or $WorkingDir
	exit 1
}

# Create full VSTest argument string.
if ($TestList -eq "") {
	$FullArgsList = " `"" + $TestAssemblyPath + "`" /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + " /TestCaseFilter:`"TestCategory=Shortcut Keys`""
} else {
	$FullArgsList = " `"" + $TestAssemblyPath + "`" /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + $TestList
}

# Display full command including full argument string.
Write-Host $WorkingDir> `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList