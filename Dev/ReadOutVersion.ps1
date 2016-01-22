Write-Host Reading all Warewolf assembly versions in `"$PSScriptRoot`"
$FullVersionString = git tag --points-at HEAD
if (-Not [string]::IsNullOrEmpty($FullVersionString)) {
    Write-Host This is version $FullVersionString
} else {
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
	    Write-Host ERROR! Read $countReadVersions different versions! All Warewolf assembly versions in $PSScriptRoot must conform for build to pass.
	    exit 1
    } else {
	    if ($countReadVersions -eq 1) {
		    git tag $AllReadVersionsConcatinated
            Write-Host This is version $AllReadVersionsConcatinated
	    }
    }
}