Param(
	[string]$FromPath,
	[string]$SourcePath)
	
Set-Location -Path $FromPath
$Path = Get-Location;
Set-Location -Path $SourcePath
$SPath = Get-Location;
cmd /c mklink /d C:\ProgramData\Warewolf\Resources\ProfilerService $Path

cmd /c mklink /d C:\ProgramData\Warewolf\Resources\ProfilerSources $SPath
