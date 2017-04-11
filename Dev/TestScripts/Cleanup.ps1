#Requires -RunAsAdministrator
Param(
    [int]$SleepDuration = 5
)

taskkill /im "Warewolf Studio.exe"
Wait-Process "Warewolf Studio" -Timeout $SleepDuration
taskkill /im "Warewolf Studio.exe" /f

$ServiceOutput = ""
sc.exe stop "Warewolf Server" 2>&1 | %{$ServiceOutput += "`n" + $_}
if ($ServiceOutput -ne "`n[SC] ControlService FAILED 1062:`n`nThe service has not been started.`n") {
    Write-Host $ServiceOutput
    sleep $SleepDuration
}

taskkill /im "Warewolf Server.exe"
Wait-Process "Warewolf Server" -Timeout $SleepDuration
taskkill /im "Warewolf Server.exe" /f

$ToClean = `
"$env:LOCALAPPDATA\Warewolf\DebugData\PersistSettings.dat",
"$env:LOCALAPPDATA\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml",
"$env:PROGRAMDATA\Warewolf\Resources",
"$env:PROGRAMDATA\Warewolf\Tests",
"$env:PROGRAMDATA\Warewolf\Workspaces",
"$env:PROGRAMDATA\Warewolf\Server Settings"

[int]$ExitCode = 0
foreach ($FileOrFolder in $ToClean) {
	Remove-Item $FileOrFolder -Recurse -ErrorAction SilentlyContinue
	if (Test-Path $FileOrFolder) {
		Write-Host Cannot delete $FileOrFolder
		$ExitCode = 1
	}	
}
if ($ExitCode -eq 1) {
    exit 1
}