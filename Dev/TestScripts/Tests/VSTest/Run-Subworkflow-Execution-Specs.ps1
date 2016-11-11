if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$SolutionDir = (Get-Item $PSScriptRoot ).parent.parent.parent.FullName
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
	            Continue
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
  id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`"
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
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir -Filter Warewolf.*.Specs ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
}
foreach ($file in Get-ChildItem $SolutionDir -Filter Dev2.*.Specs ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
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

# Start server under test
cmd.exe /c $SolutionDir\TestScripts\Server\Service\Startup.bat

# Run VSTest with full argument string.
Start-Process -FilePath "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList @($FullArgsList) -verb RunAs -WorkingDirectory $SolutionDir -Wait

# Stop server under test
cmd.exe /c $SolutionDir\TestScripts\Server\Service\Cleanup.bat

# Write failing tests playlist.
[string]$testResultsFolder = $SolutionDir + "\TestResults"
Write-Host Writing all test failures in `"$testResultsFolder`" to a playlist file

Get-ChildItem "$testResultsFolder" -Filter *.trx | Rename-Item -NewName {$_.name -replace ' ','_' }

$PlayList = "<Playlist Version=`"1.0`">"
Get-ChildItem "$testResultsFolder" -Filter *.trx | `
Foreach-Object{
	[xml]$trxContent = Get-Content $_.FullName
	if ($trxContent.TestRun.Results.UnitTestResult.count -le 0) {
		Write-Host Error parsing TestRun.Results.UnitTestResult from trx file at $_.FullName
		Continue
	}
	foreach( $TestResult in $trxContent.TestRun.Results.UnitTestResult) {
		if ($TestResult.outcome -eq "Passed") {
			Continue
		}
		if ($trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.count -le 0) {
			Write-Host Error parsing TestRun.TestDefinitions.UnitTest.TestMethod from trx file at $_.FullName
			Continue
		}
		foreach( $TestDefinition in $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod) {
			if ($TestDefinition.name -eq $TestResult.testName) {
				$PlayList += "<Add Test=`"" + $TestDefinition.className + "." + $TestDefinition.name + "`" />"
			}
		}
	}
}
$PlayList += "</Playlist>"
$OutPlaylistPath = $testResultsFolder + "\TestFailures.playlist"
$PlayList | Out-File -LiteralPath $OutPlaylistPath -Encoding utf8 -Force
Write-Host Playlist file written to `"$OutPlaylistPath`".