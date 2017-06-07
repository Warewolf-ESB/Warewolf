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
$SolutionFolderPath = ''
if ((Test-Path "$PSScriptRoot\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot"
} elseif ((Test-Path "$PSScriptRoot\..\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\..\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\..\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\..\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\..\..\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot\..\.."
}  elseif ((Test-Path "$PSScriptRoot\..\..\..\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\..\..\..\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\..\..\..\..\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\..\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\..\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.*.Tests.dll") -or (Test-Path "$PSScriptRoot\..\..\..\..\..\Dev2.*.Tests") -or (Test-Path "$PSScriptRoot\..\..\..\..\..\Dev2.*.Tests.dll")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($SolutionFolderPath -eq "") {
	Write-Host Cannot find Warewolf.*.Tests or Dev2.*.Tests projects at $PSScriptRoot or parent directories up to 5 levels deep.
	exit 1
}
$TestAssembliesList = ""
foreach ($file in Get-ChildItem $SolutionFolderPath -Filter Warewolf.*.Tests.dll ) {
	if ($file.Name -ne "Warewolf.Studio.ViewModels.Tests.dll" -and $file.Name -ne "Warewolf.COMIPC.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
	}
}
foreach ($file in Get-ChildItem $SolutionFolderPath -Filter Dev2.*.Tests.dll ) {
	if ($file.Name -ne "Dev2.Activities.Designers.Tests.dll" -and $file.Name -ne "Dev2.Activities.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
	}
}

if ($TestAssembliesList -eq "") {
    foreach ($folder in Get-ChildItem $SolutionFolderPath -Filter Warewolf.*.Tests ) {
	    if ($folder.Name -ne "Warewolf.Studio.ViewModels.Tests" -and $folder.Name -ne "Warewolf.COMIPC.Tests") {
		    $TestAssembliesList = $TestAssembliesList + " `"" + $folder.FullName + "\bin\Debug\" + $folder.Name + ".dll`""
	    }
    }
    foreach ($folder in Get-ChildItem $SolutionFolderPath -Filter Dev2.*.Tests ) {
	    if ($folder.Name -ne "Dev2.Activities.Designers.Tests" -and $folder.Name -ne "Dev2.Activities.Tests") {
		    $TestAssembliesList = $TestAssembliesList + " `"" + $folder.FullName + "\bin\Debug\" + $folder.Name + ".dll`""
	    }
    }
}
if ($TestAssembliesList -eq "") {
	Write-Host Cannot find any Warewolf.*.Tests or Dev2.*.Tests project folders at $SolutionFolderPath.
	exit 1
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx " + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList
