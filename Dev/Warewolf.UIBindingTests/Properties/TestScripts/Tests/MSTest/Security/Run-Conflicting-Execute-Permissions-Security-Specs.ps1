if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$SolutionDir = (Get-Item $PSScriptRoot).parent.parent.parent.parent.FullName
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
	            Continue
            }
        }
    }
}
if ($TestList.StartsWith(",")) {
	$TestList = $TestList -replace "^.", " /Tests:"
}

# Create test settings.
$TestSettingsFile = "$PSScriptRoot\LocalSecuritySpecs.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding="UTF-8"?>
<TestSettings
  id=`"3264dd0f-6fc1-4cb9-b44f-c649fef29609`"
  name="ConflictingPermissionsSecuritySpecs"
  enableDefaultDataCollectors="false"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run conflicting permissions security specs.</Description>
  <Deployment enabled="false" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)

if ($TestList -eq "") {
	# Create full VSTest argument string.
	$FullArgsList = "/testcontainer:`"" + $SolutionDir + "\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll`" /resultsfile:TestResults\ConflictingExecutePermissionsSecuritySpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + " /category:`"ConflictingExecutePermissionsSecurity`""
} else {
	# Create full VSTest argument string.
	$FullArgsList = "/testcontainer:`"" + $SolutionDir + "\Warewolf.SecuritySpecs\bin\Debug\Warewolf.SecuritySpecs.dll`" /resultsfile:TestResults\ConflictingExecutePermissionsSecuritySpecsResults.trx /testsettings:`"" + $TestSettingsFile + "`"" + $TestList
}
# Start server under test
cmd.exe /c $SolutionDir\TestScripts\Server\Service\Startup.bat

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList