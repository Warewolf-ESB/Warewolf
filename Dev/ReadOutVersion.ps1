$WarewolfGitRepoDirectory = "$PSScriptRoot"
Write-Host Reading Warewolf assembly files...

# Foreach Warewolf assembly.
$HighestReadVersion = "0.0.0.0"
foreach ($file in Get-ChildItem -recurse $WarewolfGitRepoDirectory) {
	if (($file.Name.EndsWith(".dll") -or ($file.Name.EndsWith(".exe") -and -Not $file.Name.EndsWith(".vshost.exe"))) -and ($file.Name.StartsWith("Dev2.") -or $file.Name.StartsWith("Warewolf.") -or $file.Name.StartsWith("WareWolf"))) {
		# Get version.
		$ReadVersion = [system.diagnostics.fileversioninfo]::GetVersionInfo($file.FullName).FileVersion
		
		# Find highest version
		$SeperateVersionNumbers = $ReadVersion.split(".")
		$SeperateVersionNumbersHighest = $HighestReadVersion.split(".")
		if ($SeperateVersionNumbers[0] -gt $SeperateVersionNumbersHighest[0]`
		-or $SeperateVersionNumbers[1] -gt $SeperateVersionNumbersHighest[1]`
		-or $SeperateVersionNumbers[2] -gt $SeperateVersionNumbersHighest[2]`
		-or $SeperateVersionNumbers[3] -gt $SeperateVersionNumbersHighest[3]){
			$HighestReadVersion = $ReadVersion
		}

        # Check for invalid.
        if ($ReadVersion.StartsWith("0.0.") -or $ReadVersion.EndsWith(".0")) {
	        Write-Host ERROR! Invalid version! $file`t$ReadVersion
	        throw "ERROR! `"$file $ReadVersion`" is an invalid version! All Warewolf assembly versions in `"$WarewolfGitRepoDirectory`" must conform to strict version regulations for build to pass."
        }
	}
}
"FullVersionString=$HighestReadVersion" | Out-File -LiteralPath FullVersionString -Encoding utf8
Write-Host Compile has completed successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/