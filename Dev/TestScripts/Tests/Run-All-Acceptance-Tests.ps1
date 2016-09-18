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
$TestSettingsFile = "$PSScriptRoot\LocalAcceptanceTesting.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding=`"UTF-8`"?>
<TestSettings name=`"Local Acceptance Run`" id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`" xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>These are default test settings for a local acceptance test run.</Description>
    <Deployment>
		  <DeploymentItem filename=`"Dev2.Server\bin\Debug\`" outputDirectory=`"Server`" />
		  <DeploymentItem filename=`"packages\FSharp.Core.3.0.0.2\lib\net40\FSharp.Core.dll`" />
		  <DeploymentItem filename=`"packages\FSharp.Core.3.0.0.2\lib\net40\policy.2.3.FSharp.Core.dll`" />
    </Deployment>
  <NamingScheme baseName=`"AcceptanceTesting`" appendTimeStamp=`"false`" useDefault=`"false`" />
    <Scripts setupScript=`"TestScripts\Server\Startup.bat`" cleanupScript=`"TestScripts\Server\Cleanup.bat`" />
  <RemoteController name=`"rsaklfsvrtfsbld`" />
  <Execution hostProcessPlatform=`"MSIL`">
    <Timeouts testTimeout=`"180000`" />
    <TestTypeSpecific>
      <UnitTestRunConfig testTypeId=`"13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b`">
        <AssemblyResolution>
          <TestDirectory useLoadContext=`"true`" />
        </AssemblyResolution>
      </UnitTestRunConfig>
      <WebTestRunConfiguration testTypeId=`"4e7599fa-5ecb-43e9-a887-cd63cf72d207`">
        <Browser name=`"Internet Explorer 9.0`" MaxConnections=`"6`">
          <Headers>
            <Header name=`"User-Agent`" value=`"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)`" />
            <Header name=`"Accept`" value=`"*/*`" />
            <Header name=`"Accept-Language`" value=`"{{`$IEAcceptLanguage}}`" />
            <Header name=`"Accept-Encoding`" value=`"GZIP`" />
          </Headers>
        </Browser>
      </WebTestRunConfiguration>
    </TestTypeSpecific>
    <AgentRule name=`"LocalMachineDefaultRole`">
    </AgentRule>
  </Execution>
</TestSettings>
"@)

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir -Include Dev2.*.Specs.dll, Warewolf.*.Specs.dll, Warewolf.UIBindingTests.*.dll -Exclude Dev2.Installer.Specs.dll -Recurse | Where-Object {-not $_.FullName.Contains("\obj\")} | Sort-Object -Property Name -Unique ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Run VSTest with full argument string.
Start-Process -FilePath "$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList @($FullArgsList) -verb RunAs -WorkingDirectory $SolutionDir -Wait

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