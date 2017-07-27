if ([string]::IsNullOrEmpty($PSScriptRoot) -and -not [string]::IsNullOrEmpty($MyInvocation.MyCommand.Path)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$FileAndFolderSpecsDir = (Get-Item $PSScriptRoot ).FullName

"$FileAndFolderSpecsDir\Copy\Copy.feature.cs", "$FileAndFolderSpecsDir\Move\Move.feature.cs", "$FileAndFolderSpecsDir\Zip\Zip.feature.cs", "$FileAndFolderSpecsDir\Rename\Rename.feature.cs", "$FileAndFolderSpecsDir\Unzip\Unzip.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                if ($_.StartsWith("            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo") -and $_.EndsWith(" file at location`", exampleTags);") -and $PreviousLine -ne "            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);") 
                {
					"            var getGuid = Dev2.Activities.Specs.BaseTypes.CommonSteps.GetGuid();"
					"            sourceLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(sourceLocation, getGuid);"
					"            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $_
}