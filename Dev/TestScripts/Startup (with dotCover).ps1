#Requires -RunAsAdministrator
$DotCoverInstallation = Get-ChildItem "$env:LOCALAPPDATA\JetBrains\Installations\dotCover*"
if ((Test-Path "$PSScriptRoot\Startup.ps1") -and (Test-Path "$DotCoverInstallation\dotCover.exe")) {
    &"$PSScriptRoot\Startup.ps1" -DotCoverPath "$DotCoverInstallation\dotCover.exe"
} else {
	Write-Host This script depends on "Startup.ps1" and dotCover must be installed to "$env:LOCALAPPDATA\JetBrains\Installations"
}