#Requires -RunAsAdministrator

Param(
  [string]$DotCoverPath,
  [string]$ServerPath,
  [string]$StudioPath,
  [string]$ResourcesType,
  [string]$ServerUsername,
  [string]$ServerPassword
)

if ($ServerPath -ne "" -and !(Test-Path $ServerPath)) {
    Write-Host Server path not found: $ServerPath
    sleep 30
    exit 1
}
if ($StudioPath -ne "" -and !(Test-Path $StudioPath)) {
    Write-Host Studio path not found: $StudioPath
    sleep 30
    exit 1
}
if ($DotCoverPath -ne "" -and !(Test-Path $DotCoverPath)) {
    Write-Host DotCover path not found: $DotCoverPath
    sleep 30
    exit 1
}
#If only a server path is specified, then skip studio startup entirely
if (($ServerPath -ne "") -and ($StudioPath -eq "")) {
	[bool]$SkipStudioStartup = $true
} else {
	[bool]$SkipStudioStartup = $false
}

if ($ResourcesType -eq "") {
	$title = "Server Resources"
	$message = "What type of resources would you like the server to start with?"

	$UITest = New-Object System.Management.Automation.Host.ChoiceDescription "&UITest", `
		"Uses these resources for running UI Tests."

	$ServerTest = New-Object System.Management.Automation.Host.ChoiceDescription "&ServerTest", `
		"Uses these resources for running everything except unit tests and Coded UI tests."

	$Release = New-Object System.Management.Automation.Host.ChoiceDescription "&Release", `
		"Uses these resources for Warewolf releases."

	$options = [System.Management.Automation.Host.ChoiceDescription[]]($UITest, $ServerTest, $Release)

	$result = $host.ui.PromptForChoice($title, $message, $options, 0) 

	switch ($result)
		{
			0 {$ResourcesType = "UITests"}
			1 {$ResourcesType = "ServerTests"}
			2 {$ResourcesType = "Release"}
		}
}

if (Test-Path "$PSScriptRoot\Cleanup.ps1") {
    &"$PSScriptRoot\Cleanup.ps1"
}

if ($ServerPath -eq "") {
    $CurrentDirectory = $PSScriptRoot
    $NumberOfParentsSearched = 0
    while ($ServerPath -eq "" -and $NumberOfParentsSearched++ -lt 7) {
        if (Test-Path "$CurrentDirectory\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\Server\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\Server\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\DebugServer\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\DebugServer\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\ReleaseServer\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\ReleaseServer\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\Dev2.Server\bin\Debug\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\Dev2.Server\bin\Debug\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\Dev2.Server\bin\Release\Warewolf Server.exe") {
            $ServerPath = "$CurrentDirectory\Dev2.Server\bin\Release\Warewolf Server.exe"
        } elseif (Test-Path "$CurrentDirectory\*Server.zip") {
			Expand-Archive "$CurrentDirectory\*Server.zip" "$CurrentDirectory\Server" -Force
			$ServerPath = "$CurrentDirectory\Server\Warewolf Server.exe"
		}
        if ($ServerPath -eq "") {
            $CurrentDirectory = (Get-Item $CurrentDirectory).Parent.FullName
        }
    }
    if ($ServerPath -eq "") {
        Write-Host Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath
        sleep 30
        exit 1
    }
}

if ((Get-Service "Warewolf Server" -ErrorAction SilentlyContinue) -eq $null) {
    if ($DotCoverPath -eq "") {
        New-Service -Name "Warewolf Server" -BinaryPathName "$ServerPath" -StartupType Manual
    } else {
        $BinPathWithDotCover = "\`"" + $DotCoverPath + "\`" cover /TargetExecutable=\`"" + $ServerPath + "\`" /LogFile=\`"%ProgramData%\Warewolf\Server Log\dotCover.log\`" /Output=\`"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\`""
        New-Service -Name "Warewolf Server" -BinaryPathName "$BinPathWithDotCover" -StartupType Manual
    }
} else {
	if ($DotCoverPath -eq "") {
		Write-Host Configuring service to $ServerPath
		sc.exe config "Warewolf Server" binPath= "$ServerPath"
	} else {
		$BinPathWithDotCover = "\`"" + $DotCoverPath + "\`" cover /TargetExecutable=\`"" + $ServerPath + "\`" /LogFile=\`"%ProgramData%\Warewolf\Server Log\dotCover.log\`" /Output=\`"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\`""
		Write-Host Configuring service to $BinPathWithDotCover
		sc.exe config "Warewolf Server" binPath= "$BinPathWithDotCover"
	}
}
if ($ServerUsername -ne "" -and $ServerPassword -ne "") {
    sc.exe config "Warewolf Server" obj= "$ServerUsername" password= "$ServerPassword"
}

$CurrentDirectory = $PSScriptRoot
$NumberOfParentsSearched = 0
$ResourcesPath = ""
while ($ResourcesPath -eq "" -and $NumberOfParentsSearched++ -lt 6) {
    if (Test-Path "$CurrentDirectory\Resources - $ResourcesType") {
        $ResourcesPath = "$CurrentDirectory\Resources - $ResourcesType"
    } else {
        $CurrentDirectory = (Get-Item $CurrentDirectory).Parent.FullName
    }
}
if ($ResourcesPath -ne "" -and $CurrentDirectory -ne (Get-Item $ServerPath).Directory.FullName ) {
    Copy-Item -Path "$CurrentDirectory\Resources - $ResourcesType" -Destination (Get-Item $ServerPath).Directory.FullName -Recurse -Force
}

Write-Host Copying resources from ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*") into Warewolf ProgramData.
Copy-Item -Path ((Get-Item $ServerPath).Directory.FullName + "\Resources - $ResourcesType\*") -Destination "$env:ProgramData\Warewolf" -Recurse -Force


# ****************************** Server Start ******************************
Start-Service "Warewolf Server"
Write-Host Server has started.

if (!($SkipStudioStartup)) {
	if ($StudioPath -eq "") {
		$CurrentDirectory = $PSScriptRoot
		$NumberOfParentsSearched = 0
		while ($StudioPath -eq "" -and $NumberOfParentsSearched++ -lt 7) {
			if (Test-Path "$CurrentDirectory\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\Studio\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\Studio\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\DebugStudio\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\DebugStudio\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\ReleaseStudio\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\ReleaseStudio\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\Dev2.Studio\bin\Debug\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\Dev2.Studio\bin\Debug\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\Dev2.Studio\bin\Release\Warewolf Studio.exe") {
				$StudioPath = "$CurrentDirectory\Dev2.Studio\bin\Release\Warewolf Studio.exe"
			} elseif (Test-Path "$CurrentDirectory\*Studio.zip") {
				Expand-Archive "$CurrentDirectory\*Studio.zip" "$CurrentDirectory\Studio" -Force
				$StudioPath = "$CurrentDirectory\Studio\Warewolf Studio.exe"
			}
			if ($StudioPath -eq "") {
				$CurrentDirectory = (Get-Item $CurrentDirectory).Parent.FullName
			}
		}
		if ($StudioPath -eq "") {
			Write-Host Cannot find Warewolf Studio.exe. To run the studio provide a path to that file as a commandline parameter like this: -StudioPath
			exit 0
		}
	}

	if ($StudioPath -ne "") {
		if ($DotCoverPath -eq "") {
			Start-Process "$StudioPath"
			sleep 60
		} else {
			Start-Process $DotCoverPath "cover /TargetExecutable=`"$StudioPath`" /LogFile=`"$env:LocalAppData\Warewolf\Studio Logs\dotCover.log`" /Output=`"$env:LocalAppData\Warewolf\Studio Logs\dotCover.dcvr`""
			sleep 900
		}
		Write-Host Studio has started.
	}
}