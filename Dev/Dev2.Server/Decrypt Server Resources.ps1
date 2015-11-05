Write-Host Getting script path.
$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$CurrentDirectory = Split-Path $Invocation.MyCommand.Path
Write-Host Got script path as "$CurrentDirectory".

Write-Host Starting compile using compiler at "$env:vs120comntools..\IDE\devenv.com".
[System.Diagnostics.Process]::Start("""" + $env:vs120comntools + "..\IDE\devenv.com""", """" + $CurrentDirectory + "\..\Dev2.Runtime.Services\Dev2.Runtime.Services.csproj"" /Build ""Debug|Any CPU""")
Write-Host Compile finished.

Write-Host Loading assembly at "$CurrentDirectory\..\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll".
Add-Type -Path "$CurrentDirectory\..\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll"
Write-Host Assembly loaded. 

Write-Host Loading type.
$ResourceHandler = New-Object -TypeName "Dev2.Runtime.ESB.Management.Services.FetchResourceDefintition"
Write-Host Type loaded.

Write-Host Recursing through resources.
foreach ($resource in Get-ChildItem -Path "$CurrentDirectory\bin\Debug\Resources" -Recurse)
{
	Write-Host Resource found at "$resource".
	$sb = New-Object -TypeName "System.Text.StringBuilder"
	[void]$sb.Append($resource)
	Write-Host Resource read.
	
	Write-Host Decrypting resource.
	$sb = $ResourceHandler.DecryptAllPasswords($sb)
	Write-Host Resource decrypted.

	Write-Host Writing decrypted resource.
	$sb.ToString() | Out-File -LiteralPath $resource -Encoding utf8 -Force
	Write-Host Written back to "$resource".
}