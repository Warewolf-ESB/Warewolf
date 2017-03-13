param([Parameter(Mandatory = $false)][AllowEmptyString()][string]$CustomVersionString = "")
$WarewolfGitRepoDirectory = "$PSScriptRoot"
Write-Host Writing C# and F# versioning files...

# Get all the latest version tags from server repo.
git -C "$WarewolfGitRepoDirectory" fetch --tags

# Generate informational version.
# (from git commit id and time)
$GitCommitID = git -C "$WarewolfGitRepoDirectory" rev-parse HEAD
$GitCommitTimeObject = git -C "$WarewolfGitRepoDirectory" show -s --format="%ct" $GitCommitID
if (($GitCommitTimeObject[0]).ToString().length -gt 1){
    $GitCommitTimeString = $GitCommitTimeObject[0]
} else {
    $GitCommitTimeString = $GitCommitTimeObject
}
if ([string]::IsNullOrEmpty($GitCommitTimeString)) {
	Write-Host Cannot resolve time of commit `"$GitCommitID`".
} else {
    write-host Resolved time of commit `"$GitCommitID`" as `"$GitCommitTimeString`".
    $GitCommitTimeDouble = [Double]$GitCommitTimeString
    $origin = New-Object -Type DateTime -ArgumentList 1970, 1, 1, 0, 0, 0, 0
    $GitCommitTime = $origin.AddSeconds($GitCommitTimeDouble)
}
$GitBranchName = git rev-parse --abbrev-ref HEAD
if (-not "$CustomVersionString" -eq "") {
	$FullVersionString = "$CustomVersionString"
} else {
	# Check if this version already tagged.
	$FullVersionString = git -C "$WarewolfGitRepoDirectory" tag --points-at HEAD
	if (-not [string]::IsNullOrEmpty($FullVersionString))  {
		$FullVersionString = $FullVersionString.Trim()
		if ($FullVersionString -Match " ") {
			# This commit has more than on tag, using first tag
			Write-Host This commit has more than one tags as `"$FullVersionString`".
			$FullVersionString = $FullVersionString.Split(" ")[0]
			Write-Host Using last tag as `"$FullVersionString`".
		}
		# This version is already tagged.
		Write-Host You are up to date with version `"$FullVersionString`".
	} else {
		# This version is not already tagged.
		Write-Host This version is not tagged, generating new tag...
		# Get last known version
		$FullVersionString = git -C "$WarewolfGitRepoDirectory" describe --abbrev=0 --tags
		if ([string]::IsNullOrEmpty($FullVersionString)) {
			Write-Host No local tags found in git history. 
			exit 1
		} else {
			$FullVersionString = $FullVersionString.Trim()
			# Make new version from last known version.
			Write-Host Last version was `"$FullVersionString`". Generating next version...
			do {
				# Increment build number.
				[int]$NewBuildNumber = $FullVersionString.Split(".")[3]
				$NewBuildNumber++
				$FullVersionString = $FullVersionString.Split(".")[0] + "." + $FullVersionString.Split(".")[1] + "." + $FullVersionString.Split(".")[2] + "." + $NewBuildNumber
				Write-Host Next version would be `"$FullVersionString`". Checking against Origin repo...
				# Check tag against origin
				$originTag = git -C "$WarewolfGitRepoDirectory" ls-remote --tags origin $FullVersionString
				if ($originTag.length -ne 0) {
					Write-Host Origin has tag `"$originTag`".
				} else {
					Write-Host Double checking with hard coded integration manager repo...
					# Check tag against hard coded integration manager repo
					$originTag = git -C "$WarewolfGitRepoDirectory" ls-remote --tags "file:////rsaklfsvrgendev/Git-Repositories/Warewolf-ESB" $FullVersionString
					if ($originTag.length -ne 0) {
						Write-Host Hard coded integration manager repo has tag `"$originTag`".
					} else {
						Write-Host Double checking with hard coded blessed repo...
						# Check tag against hard coded blessed repo
						$originTag = git -C "$WarewolfGitRepoDirectory" ls-remote --tags "https://github.com/Warewolf-ESB/Warewolf" $FullVersionString
						if ($originTag.length -ne 0) {
							Write-Host Hard coded blessed repo has tag `"$originTag`".
						}
					}
				}
			} while ($originTag.length -ne 0)
		}
		# New (unique) version has been generated.
		Write-Host Origin has confirmed version `"$FullVersionString`" is OK.
	}
}
# Write version files
$CSharpVersionFile = "$WarewolfGitRepoDirectory\AssemblyCommonInfo.cs"
Write-Host Writing C Sharp version file to `"$CSharpVersionFile`" as...
$Line1 = "using System.Reflection;"
$Line2 = "[assembly: AssemblyCompany(""Warewolf"")]"
$Line3 = "[assembly: AssemblyProduct(""Warewolf"")]"
$Line4 = "[assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)]"
$Line5 = "[assembly: AssemblyVersion(""" + $FullVersionString + """)]"
$Line6 = "[assembly: AssemblyInformationalVersion(""" + $GitCommitTime + " " + $GitCommitID + " " + $GitBranchName + """)]"
$Line7 = "[assembly: InternalsVisibleTo(""Dev2.Activities.Designers.Tests"")]"
$Line8 = "[assembly: InternalsVisibleTo(""Warewolf.Studio.ViewModels.Tests"")]"
Write-Host $Line1
$Line1 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Force
Write-Host $Line2
$Line2 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
Write-Host $Line3
$Line3 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
Write-Host $Line4
$Line4 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
Write-Host $Line5
$Line5 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
Write-Host $Line6
$Line6 | Out-File -LiteralPath $CSharpVersionFile -Encoding utf8 -Append
Write-Host C Sharp version file written to `"$CSharpVersionFile`".

$FSharpVersionFile = "$WarewolfGitRepoDirectory\AssemblyCommonInfo.fs"
Write-Host Writing F Sharp version file to `"$FSharpVersionFile`" as...
$Line1 = "namespace Warewolf.FSharp"
$Line2 = "open System.Reflection;"
$Line3 = "[<assembly: AssemblyCompany(""Warewolf"")>]"
$Line4 = "[<assembly: AssemblyProduct(""Warewolf"")>]"
$Line5 = "[<assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)>]"
$Line6 = "[<assembly: AssemblyVersion(""" + $FullVersionString + """)>]"
# Ashley: F# Compile thinks this is invalid for some reason
#$Line7 = "[<assembly: AssemblyInformationalVersion(""" + $GitCommitTime + " " + $GitCommitID + """)>]"
$Line7 = "do()"
Write-Host $Line1
$Line1 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Force
Write-Host $Line2
$Line2 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host $Line3
$Line3 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host $Line4
$Line4 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host $Line5
$Line5 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host $Line6
$Line6 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host $Line7
$Line7 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
Write-Host F Sharp version file written to `"$FSharpVersionFile`".

Write-Host Version written successfully! For more info about this script see: http://warewolf.io/ESB-blog/artefact-sharing-efficient-ci/
