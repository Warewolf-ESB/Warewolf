Param(
  [switch]$NoExit,
  [switch]$Coverage=$false,
  [string]$Username=$env:SERVER_USERNAME,
  [string]$Password=$env:SERVER_PASSWORD,
  [string]$ResourcesPath,
  [string]$ServerPath
)
if ($ResourcesPath -and (Test-Path "$ResourcesPath\Resources")) {
	Copy-Item -Path "$ResourcesPath\*" -Destination C:\programdata\Warewolf -Recurse
} else {
	if ($ResourcesPath) {
		Write-Error -Message "Resources path not found at $ResourcesPath\Resources"
	}
}
$WarewolfServerProcess = Get-Process "Warewolf Server" -ErrorAction SilentlyContinue
$WarewolfServerService = Get-Service "Warewolf Server" -ErrorAction SilentlyContinue
if ($WarewolfServerProcess) {
	Sleep 30
	$pair = "$($Username):$($Password)"
	$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
	$basicAuthValue = "Basic $encodedCreds"
	$Headers = @{
		Authorization = $basicAuthValue
	}
	Invoke-WebRequest -Uri http://localhost:3142/Secure/FetchExplorerItemsService.json?ReloadResourceCatalogue=true -Headers $Headers -UseBasicParsing
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
		$ServerBinFolderPath = Split-Path -Path "$BinPath" -Parent
		if (!(Test-Path "$ServerBinFolderPath\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe")) {
			.\NuGet.exe install JetBrains.dotCover.CommandLineTools -ExcludeVersion -NonInteractive -OutputDirectory "$ServerBinFolderPath."
		}
		if (!(Test-Path "$PSScriptRoot\TestResults")) {
			New-Item -ItemType Directory "$PSScriptRoot\TestResults"
		}
		$CoverageConfigPath = "$PSScriptRoot\DotCover Runner.xml"
		@"
<AnalyseParams>
    <TargetExecutable>$BinPath</TargetExecutable>
    <Output>$PSScriptRoot\DotCover.dcvr</Output>
    <Scope>
        <ScopeEntry>$ServerBinFolderPath\Warewolf*.dll</ScopeEntry>
        <ScopeEntry>$ServerBinFolderPath\Warewolf*.exe</ScopeEntry>
        <ScopeEntry>$ServerBinFolderPath\Dev2.*.dll</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*tests</ModuleMask>
                <ModuleMask>*specs</ModuleMask>
                <ModuleMask>*Tests</ModuleMask>
                <ModuleMask>*Specs</ModuleMask>
                <ModuleMask>Warewolf.UIBindingTests*</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
"@ | Out-File -FilePath $CoverageConfigPath
		$BinPath = "\`"$ServerBinFolderPath\JetBrains.dotCover.CommandLineTools\tools\dotCover.exe\`" cover \`"$CoverageConfigPath\`" /LogFile=\`"$ServerBinFolderPath\DotCover.log\`" --DisableNGen";
	}
	if ($Username) {
		Write-Host Starting Warewolf server as $Username
		Write-Host 1. Create Warewolf Administrators group.
		NET localgroup "Warewolf Administrators" /ADD
		if ($Password -eq 'W@rEw0lf@dm1n' -or $Password -eq '') {
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
		Write-Host 2. Create Warewolf Administrator.
		NET user "$Username" "$Password" /ADD /Y
		if ($Error[1].Exception.Message -eq "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements."){
			exit 1
		}
		NET user "$Username" "$Password" /Y
		if ($Error[1].Exception.Message -eq "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements."){
			exit 1
		}
		Write-Host 3. Add Warewolf Administrator to Administrators group.
		NET localgroup "Administrators" "$Username" /ADD
		Write-Host 4. Add Warewolf Administrator to Warewolf Administrators group.
		NET localgroup "Warewolf Administrators" "$Username" /ADD
		Write-Host 5. Grant Warewolf Administrator logon as a batch job rights.
		Import-Module $PSScriptRoot\UserRights.psm1;Grant-UserRight -Account "$Username" -Right SeServiceLogonRight
		if ($WarewolfServerService) {
			Write-Host Configuring service to $BinPath
			sc.exe config "Warewolf Server" start= auto binPath= "$BinPath" obj= ".\$Username" password= $Password
		} else {
			Write-Host Creating service for $BinPath
			sc.exe create "Warewolf Server" start= auto binPath= "$BinPath" obj= ".\$Username" password= $Password
		}
		sc.exe start "Warewolf Server"
	} else {
		if ($NoExit.IsPresent) {
			&"$BinPath"
		} else {
			if ($WarewolfServerService) {
				Write-Host Configuring service to $BinPath
				sc.exe config "Warewolf Server" start= auto binPath= "$BinPath"
			} else {
				Write-Host Creating service for $BinPath
				sc.exe create "Warewolf Server" start= auto binPath= "$BinPath"
			}
			sc.exe start "Warewolf Server"
		}
	}
}
if ($NoExit.IsPresent) {
	if (Test-Path "C:\Windows\System32\pauseloop.exe") {
		Write-Host Warewolf Server started successfully.
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
	while (!(Test-Path "$PSScriptRoot\serverstarted" -ErrorAction SilentlyContinue) -and $LoopCounter++ -lt 12) {
		Write-Host Still waiting for server to start...
		Start-Sleep 5
	}
}