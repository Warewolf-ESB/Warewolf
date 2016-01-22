Write-Host Getting script path.
$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$CurrentDirectory = Split-Path $Invocation.MyCommand.Path
Write-Host Got script path as "$CurrentDirectory".

Write-Host Starting compile using compiler at "$env:vs120comntools..\IDE\devenv.com".
[System.Diagnostics.Process]::Start("""" + $env:vs120comntools + "..\IDE\devenv.com""", """" + $CurrentDirectory + "\..\Server.sln"" /Build ""Debug""")
Write-Host Compile finished.

Write-Host Loading assembly at "$CurrentDirectory\..\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll".
Add-Type -Path "$CurrentDirectory\..\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll"
Write-Host Assembly loaded. 

Write-Host Loading type.
$ResourceHandler = New-Object Dev2.Runtime.ESB.Management.Services.FetchResourceDefintition
Write-Host Type loaded.

Write-Host Recursing through resources.
get-childitem "$CurrentDirectory\bin\Debug\Resources" -recurse | where {$_.extension -eq ".xml"} | % {

	Write-Host Resource found at $_.FullName.
	$sb = New-Object System.Text.StringBuilder
	[void]$sb.Append([string](Get-Content $_.FullName))
	Write-Host Resource read.
	
	Write-Host Decrypting resource.
	$sb = $ResourceHandler.DecryptAllPasswords($sb)
	Write-Host Resource decrypted.

	Write-Host Writing decrypted resource.
	$sb.ToString() | Out-File -LiteralPath $_.FullName -Encoding utf8 -Force
	Write-Host Written back to $_.FullName.
}