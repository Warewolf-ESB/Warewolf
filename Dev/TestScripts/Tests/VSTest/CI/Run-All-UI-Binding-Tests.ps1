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

# Find test assemblies
$SolutionFolderPath = ""
if ((Test-Path "$PSScriptRoot\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot"
} elseif ((Test-Path "$PSScriptRoot\..\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot\..\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\..\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\..\..\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\..\.."
} elseif ((Test-Path "$PSScriptRoot\..\..\..\..\..\Warewolf.UIBindingTests.*")) {
	$SolutionFolderPath = "$PSScriptRoot\..\..\..\..\.."
}
if ($SolutionFolderPath -eq "") {
	Write-Host Cannot find Warewolf.UIBindingTests.* projects at $PSScriptRoot or parent directories up to 5 levels deep.
	exit 1
}
$TestAssembliesList = ""
foreach ($file in Get-ChildItem $SolutionFolderPath -Filter Warewolf.UIBindingTests.*.dll ) {
	$TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "`""
}

if ($TestAssembliesList -eq "") {
    foreach ($folder in Get-ChildItem $SolutionFolderPath -Filter Warewolf.UIBindingTests.* ) {
	    $TestAssembliesList = $TestAssembliesList + " `"" + $folder.FullName + "\bin\Debug\" + $folder.Name + ".dll`""
    }
}
if ($TestAssembliesList -eq "") {
	Write-Host Cannot find any Warewolf.UIBindingTests.* project folders at $SolutionFolderPath.
	exit 1
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx /Parallel" + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Append -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList