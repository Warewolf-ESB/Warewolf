if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$SolutionDir = (Get-Item $PSScriptRoot ).parent.parent.FullName
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
	            Continue
            }
        }
    }
}
if ($TestList.StartsWith(",")) {
	$TestList = $TestList -replace "^.", " /Tests:"
}

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir -Include Warewolf.AcceptanceTesting.*.dll -Recurse | Where-Object {-not $_.FullName.Contains("\obj\")} | Sort-Object -Property Name -Unique ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx " + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Run VSTest with full argument string.
Start-Process -FilePath "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList @($FullArgsList) -verb RunAs -WorkingDirectory $SolutionDir -Wait

# Write failing tests playlist.
[string]$testResultsFolder = $SolutionDir + "\TestResults"
if (Test-Path $testResultsFolder\*.trx) {
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
}