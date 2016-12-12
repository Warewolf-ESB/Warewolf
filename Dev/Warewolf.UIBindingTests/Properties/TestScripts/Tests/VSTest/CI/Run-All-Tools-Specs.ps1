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

# Create test settings.
$TestSettingsFile = "$PSScriptRoot\LocalUITesting.testsettings"
[system.io.file]::WriteAllText($TestSettingsFile,  @"
<?xml version=`"1.0`" encoding=`"UTF-8`"?>
<TestSettings
  id=`"
"@ + [guid]::NewGuid() + @"
`"
  name="Tools Specs"
  enableDefaultDataCollectors="false"
  xmlns=`"http://microsoft.com/schemas/VisualStudio/TeamTest/2010`">
  <Description>Run Tool Specs.</Description>
  <Deployment enabled="false" />
  <Execution>
    <Timeouts testTimeout=`"180000`" />
  </Execution>
</TestSettings>
"@)

# Create full VSTest argument string.
$FullArgsList = " `"" + $SolutionDir + "\Warewolf.ToolsSpecs\bin\Debug\Warewolf.ToolsSpecs.dll`" /logger:trx /Settings:`"" + $TestSettingsFile + "`"" + $TestList

# Display full command including full argument string.
Write-Host $SolutionDir> `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`" $FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList