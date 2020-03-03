Param(
  [switch]$NoExit,
  [string]$Username=$env:SERVER_USERNAME,
  [string]$Password=$env:SERVER_PASSWORD,
  [string]$ResourcesPath
)
if (Test-Path "$ResourcesPath\Resources") {
    Copy-Item -Path "$ResourcesPath\*" -Destination C:\programdata\Warewolf -Recurse
} else {
    if ($ResourcesPath) {
        Write-Error -Message Resources path not found at $ResourcesPath\Resources
    }
}
$WarewolfServerProcess = Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
$WarewolfServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
if ($WarewolfServerProcess) {
	Sleep 30
	Invoke-WebRequest -Uri http://localhost:3142/Public/FetchExplorerItemsService.json?ReloadResourceCatalogue=true -UseDefaultCredentials -UseBasicParsing
} else {
    if ($Username) {
	    Write-Host Starting Warewolf server as $Username
	    Write-Host 1. Create Warewolf Administrators group.
	    NET localgroup "Warewolf Administrators" /ADD
	    Write-Host 2. Create Warewolf Administrator.
	    NET user "$Username" "$Password" /ADD /Y
        if ($Error[1].Exception.Message -eq "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements."){
            exit 1
        }
	    Write-Host 3. Add Warewolf Administrator to Administrators group.
	    NET localgroup "Administrators" "$Username" /ADD
	    Write-Host 4. Add Warewolf Administrator to Warewolf Administrators group.
	    NET localgroup "Warewolf Administrators" "$Username" /ADD
	    Write-Host 5. Grant Warewolf Administrator logon as a batch job rights.
	    Import-Module $PSScriptRoot\UserRights.psm1;Grant-UserRight -Account "$Username" -Right SeServiceLogonRight
    }
	if (Test-Path "$PSScriptRoot\Warewolf Server.exe") {
		$BinPath = "$PSScriptRoot\Warewolf Server.exe"
	} else {
		if (Test-Path "$PSScriptRoot\bin\Release\Warewolf Server.exe") {
			$BinPath = "$PSScriptRoot\bin\Release\Warewolf Server.exe"
		} else {
			if (Test-Path "$PSScriptRoot\bin\Debug\Warewolf Server.exe") {
			    $BinPath = "$PSScriptRoot\bin\Debug\Warewolf Server.exe"
		    } else {
			    if (Test-Path "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe") {
			        $BinPath = "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe"
		        } else {
			        Write-Error -Message "This script expects a Warewolf Server at either $PSScriptRoot, $PSScriptRoot\bin\Release, $PSScriptRoot\bin\Debug or C:\Program Files (x86)\Warewolf\Server"
			        exit 1
		        }
		    }
		}
    }
    if ($WarewolfServerService) {
        sc.exe config "Warewolf Server" start= auto binPath= "$BinPath" obj= ".\$Username" password= $Password
    } else {
        sc.exe create "Warewolf Server" start= auto binPath= "$BinPath" obj= ".\$Username" password= $Password
    }
	sc.exe start "Warewolf Server"
}
if ($NoExit.IsPresent) {
	ping -t localhost
}