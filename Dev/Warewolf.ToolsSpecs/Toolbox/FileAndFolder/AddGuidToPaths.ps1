if ([string]::IsNullOrEmpty($PSScriptRoot) -and -not [string]::IsNullOrEmpty($MyInvocation.MyCommand.Path)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$FileAndFolderSpecsDir = (Get-Item $PSScriptRoot ).FullName

"$FileAndFolderSpecsDir\Copy\*.feature.cs", "$FileAndFolderSpecsDir\Move\*.feature.cs", "$FileAndFolderSpecsDir\Unzip\*.feature.cs", "$FileAndFolderSpecsDir\Zip\*.feature.cs", "$FileAndFolderSpecsDir\Rename\*.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                if ($_ -match "TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo" -and $PreviousLine -ne "            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation);") 
                {
                    "            sourceLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(sourceLocation);"
                    "            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation);"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $_
}