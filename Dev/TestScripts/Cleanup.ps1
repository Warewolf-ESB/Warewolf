#Requires -RunAsAdministrator
Param(
    [int]$SleepDuration = 5
)

$KillRunningStudioOutput = ""
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" 2>&1 | %{$KillRunningStudioOutput = $_}
if (!($KillRunningStudioOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillRunningStudioOutput.ToString()
    sleep $SleepDuration
    taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" /f 2>&1 | %{$KillRunningStudioOutput = $_}
	if (!($KillRunningStudioOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillRunningStudioOutput.ToString()
	}
}
$KillUnknownStudioOutput = ""
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" 2>&1 | %{$KillUnknownStudioOutput = $_}
if (!($KillUnknownStudioOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillUnknownStudioOutput.ToString()
    sleep $SleepDuration
	taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" /f 2>&1 | %{$KillUnknownStudioOutput = $_}
	if (!($KillUnknownStudioOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillUnknownStudioOutput.ToString()
	}
}
$KillUnresponsiveStudioOutput = ""
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" 2>&1 | %{$KillUnresponsiveStudioOutput = $_}
if (!($KillUnresponsiveStudioOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillUnresponsiveStudioOutput.ToString()
    sleep $SleepDuration
	taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" /f 2>&1 | %{$KillUnresponsiveStudioOutput = $_}
	if (!($KillUnresponsiveStudioOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillUnresponsiveStudioOutput.ToString()
	}
}

$ServiceOutput = ""
sc.exe stop "Warewolf Server" 2>&1 | %{$ServiceOutput += "`n" + $_}
if ($ServiceOutput -ne "`n[SC] ControlService FAILED 1062:`n`nThe service has not been started.`n") {
    Write-Host $ServiceOutput
    sleep $SleepDuration
}

$KillRunningServerOutput = ""
taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING" 2>&1 | %{$KillRunningServerOutput = $_}
if (!($KillRunningServerOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillRunningServerOutput.ToString()
    sleep $SleepDuration
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING" /f 2>&1 | %{$KillRunningServerOutput = $_}
	if (!($KillRunningServerOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillRunningServerOutput.ToString()
	}
}
$KillUnknownServerOutput = ""
taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN" 2>&1 | %{$KillUnknownServerOutput = $_}
if (!($KillUnknownServerOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillUnknownServerOutput.ToString()
    sleep $SleepDuration
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN" /f 2>&1 | %{$KillUnknownServerOutput = $_}
	if (!($KillUnknownServerOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillUnknownServerOutput.ToString()
	}
}
$KillUnresponsiveServerOutput = ""
taskkill /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING" 2>&1 | %{$KillUnresponsiveServerOutput = $_}
if (!($KillUnresponsiveServerOutput.ToString().StartsWith("INFO: "))) {
	Write-Host $KillUnresponsiveServerOutput.ToString()
    sleep $SleepDuration
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING" /f 2>&1 | %{$KillUnresponsiveServerOutput = $_}
	if (!($KillUnresponsiveServerOutput.ToString().StartsWith("INFO: "))) {
		Write-Host $KillUnresponsiveServerOutput.ToString()
	}
}

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