Write-Host Getting script path.
$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$SolutionDirectory = (get-item (Split-Path $Invocation.MyCommand.Path)).parent.FullName
Write-Host Got solution path as `"$SolutionDirectory`".

Write-Host Loading assembly at `"$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll`".
if (Test-Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll") {
    Add-Type -Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll"
} else {
    Write-Host Cannot find assembly at "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\Dev2.Runtime.Services.dll", please compile that before running this tool.
    pause
    exit 1
}
Write-Host Assembly loaded.

Write-Host Loading assembly at `"$SolutionDirectory\Dev2.Warewolf.Security\bin\Debug\Warewolf.Security.dll`".
if (Test-Path "$SolutionDirectory\Dev2.Warewolf.Security\bin\Debug\Warewolf.Security.dll") {
    Add-Type -Path "$SolutionDirectory\Dev2.Warewolf.Security\bin\Debug\Warewolf.Security.dll"
} else {
    Write-Host Cannot find assembly at "$SolutionDirectory\Dev2.Warewolf.Security\bin\Debug\Warewolf.Security.dll", please compile that before running this tool.
    pause
    exit 1
}
Write-Host Assembly loaded.

Write-Host Loading type.
$ResourceHandler = New-Object Dev2.Runtime.ESB.Management.Services.FetchResourceDefinition
Write-Host Type loaded.

Write-Host Recursing through resources.
get-childitem "$SolutionDirectory\Resources - Debug\Resources","$env:ProgramData\Warewolf\Resources","$SolutionDirectory\Dev2.Server\bin\Debug\Resources" -recurse | where {$_.extension -eq ".xml"} | % {

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