#Requires -RunAsAdministrator
$DefaultDotCoverPath = "$env:LOCALAPPDATA\JetBrains\Installations\dotCover07\dotCover.exe"
if ((Test-Path "$PSScriptRoot\Startup.ps1") -and (Test-Path $DefaultDotCoverPath)) {
    &"$PSScriptRoot\Startup.ps1" -DotCoverPath $DefaultDotCoverPath
} else {
	Write-Host This script depends on "Startup.ps1" and $DefaultDotCoverPath
}