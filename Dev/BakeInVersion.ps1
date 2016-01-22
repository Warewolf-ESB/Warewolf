Write-Host Writing C# and F# versioning files...
$FullVersionString = git tag --points-at HEAD
$GitCommitID = git rev-parse HEAD
if ([string]::IsNullOrEmpty($FullVersionString)) {

    Write-Host This version is not tagged, generating new tag...
    $FullVersionString = git describe --abbrev=0 --tags
    $FullVersionString = $FullVersionString.Trim()
    if ([string]::IsNullOrEmpty($FullVersionString)) {
        Write-Host No local tags found in git history. Setting version to 0.0.0.0
        $FullVersionString = 0.0.0.0
    }

    Write-Host Last version was $FullVersionString. Generating next version...
    do {
    	[int]$NewBuildNumber = $FullVersionString.Split(".")[3]
    	$NewBuildNumber++
    	$FullVersionString = $FullVersionString.Split(".")[0] + "." + $FullVersionString.Split(".")[1] + "." + $FullVersionString.Split(".")[2] + "." + $NewBuildNumber
        Write-Host Next local tag would be $FullVersionString. Checking against origin...
        $originTag = git ls-remote --tags origin $FullVersionString
        if ($originTag.length -ne 0) {
            Write-Host Origin has tag $originTag
        }
        
    } while ($originTag.length -ne 0)
    Write-Host Origin does not have tag. Setting version to $FullVersionString.
} else {
    Write-Host This version is already tagged as: $FullVersionString
}
$SeperateVersions = $FullVersionString -split " "
$FullVersionString = $SeperateVersions[-1]
if ([string]::IsNullOrEmpty($FullVersionString)) {
	Write-Host Cannot resolve version string from repo from commit $GitCommitID
} else {
	$CSharpVersionFile = "$PSScriptRoot\AssemblyCommonInfo.cs"
	Write-Host Writing C Sharp version file to `"$CSharpVersionFile`" as...
	$Line1 = "using System.Reflection;"
	$Line2 = "[assembly: AssemblyCompany(""Warewolf"")]"
	$Line3 = "[assembly: AssemblyProduct(""Warewolf ESB"")]"
	$Line4 = "[assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)]"
	$Line5 = "[assembly: AssemblyVersion(""" + $FullVersionString + """)]"
	$Line6 = "[assembly: AssemblyInformationalVersion(""" + $GitCommitID + """)]"
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
	Write-Host C Sharp version file written to $CSharpVersionFile
	
	$FSharpVersionFile = "$PSScriptRoot\AssemblyCommonInfo.fs"
	Write-Host Writing F Sharp version file to `"$FSharpVersionFile`" as...
	$Line1 = "namespace Warewolf.FSharp"
	$Line2 = "open System.Reflection;"
	$Line3 = "[<assembly: AssemblyCompany(""Warewolf"")>]"
	$Line4 = "[<assembly: AssemblyProduct(""Warewolf ESB"")>]"
	$Line5 = "[<assembly: AssemblyCopyright(""Copyright Warewolf " + (Get-Date).year + """)>]"
	$Line6 = "[<assembly: AssemblyVersion(""" + $FullVersionString + """)>]"
	$Line7 = "[<assembly: AssemblyInformationalVersion(""" + $GitCommitID + """)>]"
	$Line8 = "do()"
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
	Write-Host $Line8
	$Line8 | Out-File -LiteralPath $FSharpVersionFile -Encoding utf8 -Append
	Write-Host F Sharp version file written to $FSharpVersionFile
}