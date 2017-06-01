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
            if ($playlistContent.Playlist.Add.Test -ne $NULL) {
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
$TestSettingsFile = "$PSScriptRoot\LocalAcceptanceTesting.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`"
  name=`"ExampleWorkflowExecutionSpecs`"
  enableDefaultDataCollectors=`"false`"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run example workflow execution specs.</Description>
  <Deployment enabled=`"false`" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)

# Find test assemblies
$TestAssemblyPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\.."
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.Specs\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.Specs"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Dev2.*.Specs.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Warewolf.*.Specs.dll at $PSScriptRoot or $PSScriptRoot\Warewolf.Specs
	exit 1
}
if (!(Test-Path $PSScriptRoot\TestResults)) {
	New-Item -ItemType Directory $PSScriptRoot\TestResults
}

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $TestAssemblyPath -Include Dev2.*.Specs.dll, Warewolf.*.Specs.dll -Recurse | Where-Object {-not $_.FullName.Contains("\obj\")} | Sort-Object -Property Name -Unique ) {
    $TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "`""
}

if ($TestList -eq "") {
	# Create full MSTest argument string.
	$FullArgsList = $TestAssembliesList + " /resultsfile:TestResults\OtherSpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + " /category:`"!ExampleWorkflowExecution&!WorkflowExecution&!SubworkflowExecution`""
} else {
	# Create full MSTest argument string.
	$FullArgsList = $TestAssembliesList + " /resultsfile:TestResults\OtherSpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + $TestList
}

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList