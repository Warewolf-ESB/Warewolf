$WarewolfGitRepoDirectory = "$PSScriptRoot"
Write-Host Reading Warewolf assembly files...

# Foreach Warewolf assembly.
$AllReadVersions = @()
foreach ($file in Get-ChildItem -recurse $WarewolfGitRepoDirectory) {
	if (($file.Name.EndsWith(".dll") -or ($file.Name.EndsWith(".exe") -and -Not $file.Name.EndsWith(".vshost.exe"))) -and ($file.Name.StartsWith("Dev2.") -or $file.Name.StartsWith("Warewolf.") -or $file.Name.StartsWith("WareWolf"))) {
		# Get version.
		$AllReadVersions += [system.diagnostics.fileversioninfo]::GetVersionInfo($file.FullName).FileVersion
	}
}
# Count the number of unique versions.
$AllReadVersions = $AllReadVersions | select -uniq
$AllReadVersionsConcatinated = ""
$countReadVersions = 0

# Format versions and display them to the user.
Write-Host Read versions:
foreach ($readVersion in $AllReadVersions) {
    Write-Host $readVersion
    $AllReadVersionsConcatinated += $readVersion
    $countReadVersions++
}

# If there is more than one unique version.
if ($countReadVersions -gt 1) {
	# Display each Warewolf assembly's version for debugging.
	Write-Host ERROR! Not all assembly versions are equal!
	foreach ($file in Get-ChildItem -recurse $WarewolfGitRepoDirectory) {
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
	throw "ERROR! Read $countReadVersions different versions as $AllReadVersionsConcatinated! All Warewolf assembly versions in $WarewolfGitRepoDirectory must conform for build to pass."
}
# If there is less than one unique version.
if ($countReadVersions -lt 1) {
	throw "ERROR! No versions read from assemblies!`nAssembly definition:`n((FileName.EndsWith(`".dll`") -or (FileName.EndsWith(`".exe`") -and -Not FileName.EndsWith(`".vshost.exe`")) -and (FileName.StartsWith(`"Dev2`") -or FileName.StartsWith(`"Warewolf`") -or FileName.StartsWith(`"WareWolf`")))
} else {
	"FullVersionString=$AllReadVersionsConcatinated" | Out-File -LiteralPath FullVersionString -Encoding utf8
}
Write-Host Compile has completed successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/