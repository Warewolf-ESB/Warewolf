Write-Host Getting script path.
$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$SolutionDirectory = (get-item (Split-Path $Invocation.MyCommand.Path)).parent.FullName
Write-Host Got solution path as `"$SolutionDirectory`".

Write-Host Loading assembly at `"$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\Dev2.Runtime.Services.dll`".
if (Test-Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\Dev2.Runtime.Services.dll") {
    Add-Type -Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\Dev2.Runtime.Services.dll"
} else {
    Write-Host Cannot find assembly at "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\Dev2.Runtime.Services.dll", please compile that before running this tool.
    pause
    exit 1
}
Write-Host Dev2.Runtime.Services Assembly loaded.

Write-Host Loading assembly at `"$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\System.Security.Cryptography.ProtectedData.dll`".
if (Test-Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\System.Security.Cryptography.ProtectedData.dll") {
    Add-Type -Path "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\System.Security.Cryptography.ProtectedData.dll"
} else {
    Write-Host Cannot find assembly at "$SolutionDirectory\Dev2.Runtime.Services\bin\Debug\net48\win\System.Security.Cryptography.ProtectedData.dll", please compile that before running this tool.
    pause
    exit 1
}
Write-Host System.Security.Cryptography.ProtectedData Assembly loaded.

Write-Host Loading assembly at `"$SolutionDirectory\Dev2.Warewolf.Security\bin\Debug\net48\win\netstandard2.0\Warewolf.Security.dll`".
if (Test-Path "$SolutionDirectory\Warewolf.Security\bin\Debug\net48\win\netstandard2.0\Warewolf.Security.dll") {
    Add-Type -Path "$SolutionDirectory\Warewolf.Security\bin\Debug\net48\win\netstandard2.0\Warewolf.Security.dll"
} else {
    Write-Host Cannot find assembly at "$SolutionDirectory\Warewolf.Security\bin\Debug\net48\win\netstandard2.0\Warewolf.Security.dll", please compile that before running this tool.
    pause
    exit 1
}
Write-Host Assembly loaded.

Write-Host Loading type.
$ResourceHandler = New-Object Dev2.Runtime.ESB.Management.Services.FetchResourceDefinition
Write-Host Type loaded.

Write-Host Recursing through resources.
get-childitem "$SolutionDirectory\Resources - Release\Resources",
"$SolutionDirectory\Resources - ServerTests\Resources",
"$SolutionDirectory\Resources - UITests\Resources",
"$env:ProgramData\Warewolf\Resources",
"$SolutionDirectory\Dev2.Server\bin\Debug\Resources" -recurse | where {$_.extension -eq ".xml" -or $_.extension -eq ".bite"} | % {

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