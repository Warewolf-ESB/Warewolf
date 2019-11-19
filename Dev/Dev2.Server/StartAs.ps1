Param(
  [switch]$NoExit
)
NET localgroup "Warewolf Administrators" /ADD
NET user "$env:SERVER_USERNAME" "$env:SERVER_PASSWORD" /ADD /Y
NET localgroup "Administrators" "$env:SERVER_USERNAME" /ADD
NET localgroup "Warewolf Administrators" "$env:SERVER_USERNAME" /ADD
Import-Module C:\Server\UserRights.psm1
Grant-UserRight -Account "$env:SERVER_USERNAME" -Right SeServiceLogonRight
sc.exe create "Warewolf Server" start= auto binPath= "C:\Server\Warewolf Server.exe" obj= ".\$env:SERVER_USERNAME" password= $env:SERVER_PASSWORD
sc.exe start "Warewolf Server"
if ($NoExit.IsPresent) {
    ping -t localhost
}