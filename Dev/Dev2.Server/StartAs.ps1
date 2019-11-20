Param(
  [switch]$NoExit
)
$WarewolfServerProcess =  Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
if ($WarewolfServerProcess) {
	Write-Warning -Message "Warewolf server has started."
} else {
	NET localgroup "Warewolf Administrators" /ADD
	NET user "$env:SERVER_USERNAME" "$env:SERVER_PASSWORD" /ADD /Y
	NET localgroup "Administrators" "$env:SERVER_USERNAME" /ADD
	NET localgroup "Warewolf Administrators" "$env:SERVER_USERNAME" /ADD
	Import-Module C:\Server\UserRights.psm1
	Grant-UserRight -Account "$env:SERVER_USERNAME" -Right SeServiceLogonRight
	if (Test-Path "C:\Server\Warewolf Server.exe") {
		sc.exe create "Warewolf Server" start= auto binPath= "C:\Server\Warewolf Server.exe" obj= ".\$env:SERVER_USERNAME" password= $env:SERVER_PASSWORD
	} else if (Test-Path "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe") {
		sc.exe create "Warewolf Server" start= auto binPath= "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe" obj= ".\$env:SERVER_USERNAME" password= $env:SERVER_PASSWORD
	} else {
		Write-Error -Message "This script expect a Warewolf Server at either C:\Server or C:\Program Files (x86)\Warewolf\Server"
		exit 1
	}
	sc.exe start "Warewolf Server"
	if ($NoExit.IsPresent) {
		ping -t localhost
	} else {
		$Counter = 0
		$CounterMax = 300
		while (!(Test-Path "C:\Server\serverstarted") -and $Counter -lt $CounterMax) {
			Start-Sleep 1000
			$Counter = $Counter + 1
		}
		if ($Counter -eq $CounterMax) {
			Write-Error -Message "Timed out waiting for server to start."
			exit 1
		}
	}
}