Write-Host Reading all Warewolf assembly versions in `"$PSScriptRoot`"

$AllReadVersions = @()
foreach ($file in Get-ChildItem -recurse $PSScriptRoot) {
	if (($file.Name.EndsWith(".dll") -or $file.Name.EndsWith(".exe")) -and ($file.Name.StartsWith("Dev2.") -or $file.Name.StartsWith("Warewolf."))) {
		# go get the fileversion
		$AllReadVersions += [system.diagnostics.fileversioninfo]::GetVersionInfo($file.FullName).FileVersion
	}
}
$AllReadVersions = $AllReadVersions | select -uniq
$AllReadVersionsConcatinated = ""
$countReadVersions = 0
Write-Host Read versions:
foreach ($readVersion in $AllReadVersions) {
    Write-Host $readVersion
    $AllReadVersionsConcatinated += $readVersion
    $countReadVersions++
}
if ($countReadVersions -gt 1) {
	Write-Host ERROR! Not all assembly versions are equal!
	foreach ($file in Get-ChildItem -recurse $PSScriptRoot) {
		if (($file.Name.EndsWith(".dll") -or ($file.Name.EndsWith(".exe") -and -Not $file.Name.EndsWith(".vshost.exe"))) -and ($file.Name.StartsWith("Dev2") -or $file.Name.StartsWith("Warewolf") -or $file.Name.StartsWith("WareWolf"))) {
			$FullAssemblyPath = $file.FullName
			# go get the fileversion
			$ReadVersion = [system.diagnostics.fileversioninfo]::GetVersionInfo($FullAssemblyPath).FileVersion
			$lengthofver = $ReadVersion.length
			if ($ReadVersion.length -gt 8) {
				Write-Host $ReadVersion`t$FullAssemblyPath
			} else {
				Write-Host $ReadVersion`t`t$FullAssemblyPath
			}
		}
	}
	throw "ERROR! Read $countReadVersions different versions as $AllReadVersionsConcatinated! All Warewolf assembly versions in $PSScriptRoot\.. must conform for build to pass."
}
if ($countReadVersions -lt 1) {
	throw "ERROR! No versions read from assemblies!`nAssembly definition:`nif (($file.Name.EndsWith(`".dll`") -or $file.Name.EndsWith(`".exe`")) -and ($file.Name.StartsWith(`"Dev2.`") -or $file.Name.StartsWith(`"Warewolf.`"))) {"
} else {
	"FullVersionString=$AllReadVersionsConcatinated" | Out-File -LiteralPath FullVersionString -Encoding utf8
}
Write-Host Compile has completed successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/