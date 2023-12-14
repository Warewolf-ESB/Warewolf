sc.exe stop "Warewolf Server"
Wait-Process -Name "Warewolf Server" -ErrorAction SilentlyContinue -Timeout 180
$warewolfServer = Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
if ($warewolfServer) {
	$warewolfServer | Stop-Process -Force
}
Remove-Variable warewolfServer
Move-Item "C:\programdata\warewolf\Server Log\warewolf-server.log" "$PSScriptRoot\TestResults\warewolf-server.log"
if ($Coverage) {
	&"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe" shutdown 73c34ce5-501c-4369-a4cb-04d31427d1a4
}
