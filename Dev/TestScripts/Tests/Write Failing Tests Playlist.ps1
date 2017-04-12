Param(
  [string]$PlaylistFileName = "TestFailures",
  [string]$TestResultsFolder = ""
)

# Find test results
if ($TestResultsFolder -eq "") {
	if (Test-Path "$PSScriptRoot\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\TestResults"
	} elseif (Test-Path "$PSScriptRoot\*.trx") {
		$TestResultsFolder = "$PSScriptRoot"
	} elseif (Test-Path "$PSScriptRoot\..\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\TestResults"
	} elseif (Test-Path "$PSScriptRoot\..\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\.."
	} elseif (Test-Path "$PSScriptRoot\..\..\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\TestResults"
	} elseif (Test-Path "$PSScriptRoot\..\..\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\.."
	} elseif (Test-Path "$PSScriptRoot\..\..\..\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\..\TestResults"
	} elseif (Test-Path "$PSScriptRoot\..\..\..\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\.."
	} elseif (Test-Path "$PSScriptRoot\..\..\..\..\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\..\..\TestResults"
	} elseif (Test-Path "$PSScriptRoot\..\..\..\..\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\..\.."
	} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\TestResults\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\..\..\..\TestResults"
	} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\*.trx") {
		$TestResultsFolder = "$PSScriptRoot\..\..\..\..\.."
	}
	if ($TestResultsFolder -eq "") {
		Write-Host Cannot find test results at $PSScriptRoot or $PSScriptRoot\TestResults
		exit 1
	}
}

# Write failing tests playlist.
Write-Host Writing all test failures in `"$TestResultsFolder`" to a playlist file

Get-ChildItem "$TestResultsFolder" -Filter *.trx | Rename-Item -NewName {$_.name -replace ' ','_' }

$PlayList = "<Playlist Version=`"1.0`">"
Get-ChildItem "$TestResultsFolder" -Filter *.trx | `
Foreach-Object{
	[xml]$trxContent = Get-Content $_.FullName
	if ($trxContent.TestRun.Results.UnitTestResult.count -gt 0) {
	    foreach($TestResult in $trxContent.TestRun.Results.UnitTestResult) {
		    if ($TestResult.outcome -eq "Failed") {
		        if ($trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.count -gt 0) {
		            foreach($TestDefinition in $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod) {
			            if ($TestDefinition.name -eq $TestResult.testName) {
				            $PlayList += "<Add Test=`"" + $TestDefinition.className + "." + $TestDefinition.name + "`" />"
			            }
		            }
                } else {
			        Write-Host Error parsing TestRun.TestDefinitions.UnitTest.TestMethod from trx file at $_.FullName
			        Continue
		        }
		    }
	    }
	} elseif ($trxContent.TestRun.Results.UnitTestResult.outcome -eq "Failed") {
        $PlayList += "<Add Test=`"" + $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.className + "." + $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.name + "`" />"
    } else {
		Write-Host Error parsing TestRun.Results.UnitTestResult from trx file at $_.FullName
    }
}
$PlayList += "</Playlist>"
$OutPlaylistPath = $TestResultsFolder + "\" + $PlaylistFileName + ".playlist"
$PlayList | Out-File -LiteralPath $OutPlaylistPath -Encoding utf8 -Force
Write-Host Playlist file written to `"$OutPlaylistPath`".