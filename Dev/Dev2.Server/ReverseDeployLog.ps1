sc.exe stop "Warewolf Server"
Wait-Process -Name "Warewolf Server" -ErrorAction SilentlyContinue -Timeout 180
$warewolfServer = Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
if ($warewolfServer) {
	$warewolfServer | Stop-Process -Force
}
Remove-Variable warewolfServer
Move-Item "C:\programdata\warewolf\Server Log\warewolf-server.log" "$PSScriptRoot\TestResults\warewolf-server.log"
