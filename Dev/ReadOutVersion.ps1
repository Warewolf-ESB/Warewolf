$WarewolfGitRepoDirectory = "$PSScriptRoot"
Write-Host Reading Warewolf assembly versions...

# Foreach Warewolf assembly.
$AllReadVersions = @()
foreach ($file in Get-ChildItem -recurse $WarewolfGitRepoDirectory) {
	if (($file.Name.EndsWith(".dll") -or ($file.Name.EndsWith(".exe") -and -Not $file.Name.EndsWith(".vshost.exe"))) -and ($file.Name.StartsWith("Dev2.") -or $file.Name.StartsWith("Warewolf.") -or $file.Name.StartsWith("WareWolf"))) {
		# Get version.
		$ReadVersion = [system.diagnostics.fileversioninfo]::GetVersionInfo($file.FullName).FileVersion

        # Check for invalid.
        if ($ReadVersion.StartsWith("0.0.") -or $ReadVersion.EndsWith(".0")) {
	        Write-Host ERROR! Invalid version! $file`t$ReadVersion
	        throw "ERROR! `"$ReadVersion`" is an invalid version! All Warewolf assembly versions in `"$WarewolfGitRepoDirectory`" must conform to strict version regulations for build to pass."
        }
	}
}
"FullVersionString=$AllReadVersionsConcatinated" | Out-File -LiteralPath FullVersionString -Encoding utf8
Write-Host Compile has completed successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/