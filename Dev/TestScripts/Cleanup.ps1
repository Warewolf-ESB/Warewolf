#Requires -RunAsAdministrator
$ServerService = Get-Service "Warewolf Server"
if ($ServerService.Status -eq "Running") {
    [int32]$Result = 1
    $RetryCount = 0
    while ($Result -ne 0 -and $RetryCount++ -lt 5) {
        [int32]$Result = $ServerService.Stop()
        if ($Result -ne 0) {
            Stop-Process -Name "Warewolf Server"
        }
        sleep 10
    }
}
Get-Process *Warewolf* | %{$_.Kill()}

$ToClean = `
"$env:LOCALAPPDATA\Warewolf\DebugData\PersistSettings.dat",
"$env:LOCALAPPDATA\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml",
"$env:PROGRAMDATA\Warewolf\Resources",
"$env:PROGRAMDATA\Warewolf\Tests",
"$env:PROGRAMDATA\Warewolf\Workspaces",
"$env:PROGRAMDATA\Warewolf\Server Settings"

foreach ($FileOrFolder in $ToClean) {
	Remove-Item $FileOrFolder -Recurse -ErrorAction SilentlyContinue
	if (Test-Path $FileOrFolder) {
		Write-Host Cannot delete $FileOrFolder
		sleep 10
		exit 1
	}	
}
exit 0