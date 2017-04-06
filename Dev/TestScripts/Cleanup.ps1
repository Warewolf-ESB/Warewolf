#Requires -RunAsAdministrator
$ServiceOutput = ""

taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
    taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
	}
}
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
	taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
	}
}
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
	taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
	}
}

sc.exe stop "Warewolf Server" 2>&1 | %{$ServiceOutput += "`n" + $_}
if ($ServiceOutput -ne "`n[SC] ControlService FAILED 1062:`n`nThe service has not been started.`n") {
    sleep 10
    Write-Host $ServiceOutput
}

taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
	}
}
taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
	}
}
taskkill /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING" 2>&1 | %{$Output = $_}
if (!($Output.ToString().StartsWith("INFO: "))) {
    sleep 10
	Write-Host $Output.ToString()
	taskkill /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING" /f 2>&1 | %{$Output = $_}
	if (!($Output.ToString().StartsWith("INFO: "))) {
		Write-Host $Output.ToString()
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