#Requires -RunAsAdministrator
$Output = ""
taskkill /im "Warewolf Studio.exe" /fi "STATUS eq RUNNING" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq RUNNING" 1>$null

sleep 5

taskkill /im "Warewolf Studio.exe" /fi "STATUS eq UNKNOWN" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq UNKNOWN" 1>$null

sleep 5

taskkill /im "Warewolf Studio.exe" /fi "STATUS eq NOT RESPONDING" 1>$null
taskkill /im "Warewolf Studio.vshost.exe" /fi "STATUS eq NOT RESPONDING" 1>$null

$ServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
if ($ServerService -ne $null -and $ServerService.Status -eq "Running") {
    [int32]$Result = 1
    $RetryCount = 0
    while ($Result -ne 0 -and $RetryCount++ -lt 5) {
        [int32]$Result = $ServerService.Stop()
        if ($Result -ne 0) {
            sleep 10
            Stop-Process -Name "Warewolf Server"
        }
    }
}
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