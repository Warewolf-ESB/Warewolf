#Requires -RunAsAdministrator
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq RUNNING" 1>$null

sleep 5

taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq UNKNOWN" 1>$null

sleep 5

taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq NOT RESPONDING" 1>$null

sc.exe stop "Warewolf Server"

sleep 5

Get-Process *Warewolf* | %{if (!($_.HasExited)) {$_.Kill()}}

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