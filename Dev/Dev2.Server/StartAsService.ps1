Param(
  [switch]$NoExit,
  [switch]$Coverage=$false,
  [string]$Username=$env:SERVER_USERNAME,
  [string]$Password=$env:SERVER_PASSWORD,
  [string]$ResourcesPath,
  [string]$ServerPath,
  [switch]$Cleanup,
  [switch]$Anonymous
)
if ($Username -eq $null -or $Username -eq "" -or $Anonymous.IsPresent) {
    $IsAnonymous = $true
}
$WarewolfServerProcess = Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
$WarewolfServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
if ($Cleanup.IsPresent) {
	if ($WarewolfServerProcess) {
		taskkill /IM "Warewolf Server.exe" /T /F
		Wait-Process -Name "Warewolf Server"
		$WarewolfServerProcess = $null
	}
	$ToClean = @(
		"%LOCALAPPDATA%\Warewolf\DebugData\PersistSettings.dat",
		"%LOCALAPPDATA%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml",
		"%PROGRAMDATA%\Warewolf\Workspaces",
		"%PROGRAMDATA%\Warewolf\Server Settings",
		"%PROGRAMDATA%\Warewolf\VersionControl",
		"%PROGRAMDATA%\Warewolf\Audits\auditDB.db"
    )
	$ToPublish = @(
		"%PROGRAMDATA%\Warewolf\Resources",
		"%PROGRAMDATA%\Warewolf\Tests",
		"%PROGRAMDATA%\Warewolf\VersionControl",
		"%PROGRAMDATA%\Warewolf\DetailedLogs",
		"%PROGRAMDATA%\Warewolf\Server Log\wareWolf-Server.log"
	)
}
if ($ResourcesPath -and (Test-Path "$ResourcesPath\Resources")) {
	if (!(Test-Path C:\programdata\Warewolf)) {
		New-Item -ItemType Directory -path C:\programdata\Warewolf
	}
	Copy-Item -Path "$ResourcesPath\*" -Destination C:\programdata\Warewolf -Recurse -Force
} else {
    if ($ResourcesPath -and (Test-Path "$PSScriptRoot\Resources - $ResourcesPath\Resources")) {
		if (!(Test-Path C:\programdata\Warewolf)) {
			New-Item -ItemType Directory -path C:\programdata\Warewolf
        }
	    Copy-Item -Path "$PSScriptRoot\Resources - $ResourcesPath\*" -Destination C:\programdata\Warewolf -Recurse -Force
    } else {
	    if ($ResourcesPath) {
		    Write-Error -Message "Resources path not found at `"$ResourcesPath\Resources`" or `"$PSScriptRoot\Resources - `$ResourcesPath\Resources`""
	    }
    }
}
if ($IsAnonymous -and (Test-Path "C:\ProgramData\Warewolf\Server Settings - Copy")) {
	Copy-Item -Path "C:\ProgramData\Warewolf\Server Settings - Copy\*" -Destination "C:\ProgramData\Warewolf\Server Settings" -Force -Recurse
}
if ($WarewolfServerProcess) {
	if ($IsAnonymous) {
		Invoke-WebRequest -Uri http://localhost:3142/Public/FetchExplorerItemsService.json?ReloadResourceCatalogue=true -UseBasicParsing
	} else {
		Sleep 30
		$pair = "$($Username):$($Password)"
		$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
		$basicAuthValue = "Basic $encodedCreds"
		$Headers = @{
			Authorization = $basicAuthValue
		}
		Invoke-WebRequest -Uri http://localhost:3142/Secure/FetchExplorerItemsService.json?ReloadResourceCatalogue=true -Headers $Headers -UseBasicParsing
	}
} else {
	if (Test-Path "$PSScriptRoot\serverstarted") {
		Remove-Item "$PSScriptRoot\serverstarted"
	}
	if (Test-Path "$PSScriptRoot\Warewolf Server.exe") {
		$BinPath = "$PSScriptRoot\Warewolf Server.exe"
	} else {
		if (Test-Path "$PSScriptRoot\bin\Release\net48\win\Warewolf Server.exe") {
			$BinPath = "$PSScriptRoot\bin\Release\net48\win\Warewolf Server.exe"
		} else {
			if (Test-Path "$PSScriptRoot\bin\Debug\net48\win\Warewolf Server.exe") {
				$BinPath = "$PSScriptRoot\bin\Debug\net48\win\Warewolf Server.exe"
			} else {
				if (Test-Path "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe") {
					$BinPath = "C:\Program Files (x86)\Warewolf\Server\Warewolf Server.exe"
				} else {
					if ($ServerPath -ne $null -and $ServerPath -ne "" -and (Test-Path "$ServerPath")) {
						$BinPath = $ServerPath
					} else {
						Write-Error -Message "Run this script from the Warewolf Server.exe file directory or use -ServerPath argument."
						exit 1
					}
				}
			}
		}
	}
	if ($Coverage) {
		if (!(Test-Path "$PSScriptRoot\TestResults")) {
			New-Item -ItemType Directory "$PSScriptRoot\TestResults"
		}
		if (Test-Path "$PSScriptRoot\TestResults\Snapshot.coverage") {
			Remove-Item "$PSScriptRoot\TestResults\Snapshot.coverage"
		}
		&"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe" instrument "$BinPath" --session-id 73c34ce5-501c-4369-a4cb-04d31427d1a4
		&"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe" collect "$BinPath" --session-id 73c34ce5-501c-4369-a4cb-04d31427d1a4  --output "$PSScriptRoot\TestResults\Snapshot.coverage" --server-mode";
	}
	if (!($IsAnonymous)) {
		Write-Host Starting Warewolf server as $Username
		Write-Host 1. Create Warewolf Administrators group.
		NET localgroup "Warewolf Administrators" /ADD
		if ($Password -eq '') {
			$Password = [String]'wW'[(ForEach-Object { Get-Random -Maximum 2 })]
			$Password += [String]'aA@'[(ForEach-Object { Get-Random -Maximum 3 })]
			$Password += [String]'rR'[(ForEach-Object { Get-Random -Maximum 2 })]
			$Password += [String]'eE'[(ForEach-Object { Get-Random -Maximum 2 })]
			$Password += [String]'wW'[(ForEach-Object { Get-Random -Maximum 2 })]
			$Password += [String]'oO0'[(ForEach-Object { Get-Random -Maximum 3 })]
			$Password += [String]'lL1'[(ForEach-Object { Get-Random -Maximum 3 })]
			$Password += [String]'fF'[(ForEach-Object { Get-Random -Maximum 2 })]
			$Password += '@dm1n'
			[System.Environment]::SetEnvironmentVariable('SERVER_PASSWORD', $Password, [System.EnvironmentVariableTarget]::Machine)
		}
		Write-Host 2. Create new Warewolf Administrator.
		NET user "$Username" "$Password" /ADD /Y
		if ($Error[1].Exception.Message -eq "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements."){
			exit 1
		}
		NET user "$Username" "$Password" /Y
		if ($Error[1].Exception.Message -eq "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements."){
			exit 1
		}
		Write-Host 3. Add new Warewolf Administrator to Administrators group.
		NET localgroup "Administrators" "$Username" /ADD
		Write-Host 4. Add new Warewolf Administrator to Warewolf Administrators group.
		NET localgroup "Warewolf Administrators" "$Username" /ADD
	}
}
if ($WarewolfServerService) {
	Write-Host Configuring service to $BinPath
	sc.exe config "Warewolf Server" start= auto binPath= "$BinPath"
} else {
	Write-Host Creating service for $BinPath
	sc.exe create "Warewolf Server" start= auto binPath= "$BinPath"
}
sc.exe start "Warewolf Server"
if ($NoExit.IsPresent) {
	if (Test-Path "C:\Windows\System32\pauseloop.exe") {	
		Write-Host Warewolf Server started successfully.

		Write-Host 5. Read Server Logs started.
		Get-Content -Path "C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log" -Tail 5 -Wait
		Write-Host 6. Read Server Logs ended.
		
		&"C:\Windows\System32\pauseloop.exe"
	} else {
		if (Test-Path "$PSScriptRoot\pauseloop.exe") {
			&"$PSScriptRoot\pauseloop.exe"
		} else {
			ping -t localhost
		}
	}
} else {
	$LoopCounter = 0
	$LoopCounterMax = 30
	if ($Coverage) {
		$LoopCounterMax = 60
	}
	while (!(Test-Path "$PSScriptRoot\serverstarted" -ErrorAction SilentlyContinue) -and $LoopCounter++ -lt $LoopCounterMax) {
		Write-Host Still waiting for server to start...
		Start-Sleep 6
	}
}
