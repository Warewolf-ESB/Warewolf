$SolutionDir = (get-item $PSScriptRoot ).parent.parent.FullName
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir | ? {$_.PSIsContainer -and (($_.Name.StartsWith("Dev2.") -or $_.Name.StartsWith("Warewolf.")) -and $_.Name.EndsWith(".Tests"))} ) {
    $TestAssembliesList = "`"$SolutionDir\" + $file.Name + "\bin\Debug\" + $file.Name + ".dll`" " + $TestAssembliesList 
}
$TestAssembliesList = $TestAssembliesList + "/logger:trx"
Start-Process -FilePath "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList $TestAssembliesList -verb RunAs -WorkingDirectory $SolutionDir -Wait

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