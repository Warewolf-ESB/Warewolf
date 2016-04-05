# Read playlists.
$TestList = ""
Get-ChildItem "$PSScriptRoot" -Filter *.playlist | `
Foreach-Object{
	[xml]$playlistContent = Get-Content $_.FullName
	if ($playlistContent.Playlist.Add.count -le 0) {
		Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
		Continue
	}
	foreach( $TestName in $playlistContent.Playlist.Add) {
		$TestList += "," + $TestName.Test
	}
}
if ($TestList.length -gt 0) {
	$TestList = $TestList -replace "^.", " "
}


# Create assemblies list.
$SolutionDir = (get-item $PSScriptRoot ).parent.parent.FullName
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir | ? {$_.PSIsContainer -and (($_.Name.StartsWith("Dev2.") -or $_.Name.StartsWith("Warewolf.")) -and $_.Name.EndsWith(".Tests"))} ) {
    $TestAssembliesList = "`"$SolutionDir\" + $file.Name + "\bin\Debug\" + $file.Name + ".dll`" " + $TestAssembliesList 
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + $TestList

# Display full command including full argument string.
Write-Host $SolutionDir> `"$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`" $FullArgsList

Start-Process -FilePath "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList $FullArgsList -verb RunAs -WorkingDirectory $SolutionDir -Wait

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