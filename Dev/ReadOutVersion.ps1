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
		if ([convert]::ToInt32($SeperateVersionNumbers[0], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[0], 10)`
		-or [convert]::ToInt32($SeperateVersionNumbers[1], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[1], 10)`
		-or [convert]::ToInt32($SeperateVersionNumbers[2], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[2], 10)`
		-or [convert]::ToInt32($SeperateVersionNumbers[3], 10) -gt [convert]::ToInt32($SeperateVersionNumbersHighest[3], 10)){
			$HighestReadVersion = $ReadVersion
		}

        # Check for invalid.
        if ($ReadVersion.StartsWith("0.0.") -or $ReadVersion.EndsWith(".0")) {
			$getFullPath = $file.FullName
	        Write-Host ERROR! Invalid version! $getFullPath $ReadVersion
	        throw "ERROR! `"$getFullPath $ReadVersion`" is an invalid version! All Warewolf assembly versions in `"$WarewolfGitRepoDirectory`" must conform to strict version regulations for build to pass."
        }
	}
}
Out-File -LiteralPath FullVersionString -InputObject "FullVersionString=$HighestReadVersion" -Encoding default
Write-Host Compile has completed successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/