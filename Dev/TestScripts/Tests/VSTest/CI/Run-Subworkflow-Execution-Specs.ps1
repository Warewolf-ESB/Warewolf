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
            if ($playlistContent.Playlist.Add.Test -ne $NULL) {
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
$TestSettingsFile = "$PSScriptRoot\SubworkflowExecutionSpecs.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"
"@ + [guid]::NewGuid() + @"
`"
  name="SubworkflowExecutionSpecs"
  enableDefaultDataCollectors="false"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Subworkflow Execution Specs.</Description>
  <Deployment enabled="false" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)

# Create assemblies list.
$TestAssembliesPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\.."
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Dev2.*.Specs.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($TestAssembliesPath -eq "") {
	Write-Host Cannot find Warewolf.*.Specs.dll at $PSScriptRoot or $PSScriptRoot\Warewolf.Specs
	exit 1
}
$TestAssembliesList = ""
foreach ($file in Get-ChildItem $TestAssembliesPath -Filter Warewolf.*.Specs.dll ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}
foreach ($file in Get-ChildItem $TestAssembliesPath -Filter Dev2.*.Specs.dll ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}

if ($TestList -eq "") {
    # Create full VSTest argument string.
    $FullArgsList = $TestAssembliesList + " /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + " /TestCaseFilter:`"TestCategory=SubworkflowExecution`""
} else {
    # Create full VSTest argument string.
    $FullArgsList = $TestAssembliesList + " /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + $TestList
}

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList
