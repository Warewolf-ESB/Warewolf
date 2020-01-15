Param(
  [switch]$NoExit
)
$WarewolfServerProcess =  Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
if ($WarewolfServerProcess) {
	Invoke-WebRequest -Uri http://localhost:3142/Public/FetchExplorerItemsService.json?ReloadResourceCatalogue=true
	Write-Host Warewolf server is now running.
} else {
	Remove-Item "C:\ProgramData\Warewolf\Server Log\warewolf-server.log" -ErrorAction SilentlyContinue
	Write-Host Create Warewolf Administrators group.
	NET localgroup "Warewolf Administrators" /ADD
	Write-Host Create Warewolf Administrator.
	NET user "$env:SERVER_USERNAME" "$env:SERVER_PASSWORD" /ADD /Y
	Write-Host Add Warewolf Administrator to Administrators group.
	NET localgroup "Administrators" "$env:SERVER_USERNAME" /ADD
	Write-Host Add Warewolf Administrator to Warewolf Administrators group.
	NET localgroup "Warewolf Administrators" "$env:SERVER_USERNAME" /ADD
	Write-Host Grant Warewolf Administrator logon as a batch job rights.
	Import-Module C:\Server\UserRights.psm1
	Grant-UserRight -Account "$env:SERVER_USERNAME" -Right SeServiceLogonRight
	if (Test-Path "C:\Server\Warewolf Server.exe") {
		sc.exe create "Warewolf Server" start= auto binPath= "C:\Server\Warewolf Server.exe" obj= ".\$env:SERVER_USERNAME" password= $env:SERVER_PASSWORD
	} else {
		if (Test-Path "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe") {
			sc.exe create "Warewolf Server" start= auto binPath= "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe" obj= ".\$env:SERVER_USERNAME" password= $env:SERVER_PASSWORD
		} else {
			Write-Error -Message "This script expects a Warewolf Server at either C:\Server or C:\Program Files (x86)\Warewolf\Server"
			exit 1
		}
	}
	sc.exe start "Warewolf Server"
}
if ($NoExit.IsPresent) {
	ping -t localhost
}