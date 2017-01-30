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
            }
        }
    }
}

# Find test assemblies
$TestAssemblyPath = ""
if (Test-Path "$PSScriptRoot\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.Tests\Dev2.Activities.Tests.dll"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Dev2.Activities.Tests.dll") {
	$TestAssemblyPath = "$PSScriptRoot\..\..\..\..\..\Dev2.Activities.Tests.dll"
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Dev2.Activities.Tests.dll at $PSScriptRoot or $PSScriptRoot\Warewolf.Tests
	exit 1
}
if (!(Test-Path $PSScriptRoot\TestResults)) {
	New-Item -ItemType Directory $PSScriptRoot\TestResults
}

# Create full MSTest argument string.
$FullArgsList = " /testcontainer:`"" + $TestAssemblyPath + "`" /resultsfile:`"" + $PSScriptRoot + "\TestResults\ActivityUnitTestResults.trx`"" + $TestList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList