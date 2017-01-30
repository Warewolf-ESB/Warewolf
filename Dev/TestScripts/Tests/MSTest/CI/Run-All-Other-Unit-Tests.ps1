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
if (Test-Path "$PSScriptRoot\Warewolf.Tests\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\Warewolf.Tests"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.Tests\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\Warewolf.Tests"
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.Tests\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\Warewolf.Tests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.Tests\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\Warewolf.Tests"
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.Tests\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\Warewolf.Tests"
} elseif (Test-Path "$PSScriptRoot\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot"
} elseif (Test-Path "$PSScriptRoot\..\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\.."
} elseif (Test-Path "$PSScriptRoot\..\..\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\.."
} elseif (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.*.Tests.dll") {
	$TestAssembliesPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($TestAssemblyPath -eq "") {
	Write-Host Cannot find Warewolf.*.Tests.dll at $PSScriptRoot or $PSScriptRoot\Warewolf.Tests
	exit 1
}
if (!(Test-Path $PSScriptRoot\TestResults)) {
	New-Item -ItemType Directory $PSScriptRoot\TestResults
}

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $TestAssemblyPath -Filter Warewolf.*.Tests ) {
	if ($file.Name -ne "Warewolf.*.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
	}
}
foreach ($file in Get-ChildItem $TestAssemblyPath -Filter Dev2.*.Tests ) {
	if ($file.Name -ne "Dev2.Activities.Designers.Tests.dll" -and $file.Name -ne "Dev2.Activities.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
	}
}

# Create full MSTest argument string.
$FullArgsList = $TestAssembliesList + " /resultsfile:TestResults\UnitTestResults.trx " + $TestList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList