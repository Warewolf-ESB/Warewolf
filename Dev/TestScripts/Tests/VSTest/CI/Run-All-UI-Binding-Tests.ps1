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
            }
        }
    }
}
if ($TestList.StartsWith(",")) {
	$TestList = $TestList -replace "^.", " /Tests:"
}

# Create assemblies list.
if (Test-Path "$PSScriptRoot\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\.."
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.UIBindingTests\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\..\Warewolf.UIBindingTests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.UIBindingTests.*.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($TestAssembliesPath -eq "") {
	Write-Host Cannot find Warewolf.UIBindingTests.*.dll at $PSScriptRoot or $PSScriptRoot\Warewolf.UIBindingTests
	exit 1
}
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $TestAssembliesPath -Include Warewolf.UIBindingTests.*.dll -Recurse | Sort-Object -Property Name -Unique ) {
    $TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx " + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList