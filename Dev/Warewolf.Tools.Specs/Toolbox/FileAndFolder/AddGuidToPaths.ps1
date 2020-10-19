#Source and Destination
"$PSScriptRoot\Copy\Copy.feature.cs", "$PSScriptRoot\Move\Move.feature.cs", "$PSScriptRoot\Zip\Zip.feature.cs", "$PSScriptRoot\Rename\Rename.feature.cs", "$PSScriptRoot\Unzip\Unzip.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                if ($_.StartsWith("            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo") -and $_.EndsWith(" `", @__tags);") -and $PreviousLine -ne "            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);") 
                {
					"            var getGuid = Dev2.Activities.Specs.BaseTypes.CommonSteps.GetGuid();"
					"            sourceLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(sourceLocation, getGuid);"
					"            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $_
}

#Destination Only
"$PSScriptRoot\Create\Create.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                if ($_.StartsWith("            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo") -and $_.EndsWith(" `", @__tags);") -and $PreviousLine -ne "            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);") 
                {
					"            var getGuid = Dev2.Activities.Specs.BaseTypes.CommonSteps.GetGuid();"
					"            destinationLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(destinationLocation, getGuid);"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $_
}

#Source Only
"$PSScriptRoot\Write File\Write File.feature.cs", "$PSScriptRoot\Delete\Delete.feature.cs", "$PSScriptRoot\Read File\Read File.feature.cs", "$PSScriptRoot\Read Folder\Read Folder.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                if ($_.StartsWith("            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo") -and $_.EndsWith(" `", @__tags);") -and $PreviousLine -ne "            sourceLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(sourceLocation, getGuid);") 
                {
					"            var getGuid = Dev2.Activities.Specs.BaseTypes.CommonSteps.GetGuid();"
					"            sourceLocation = Dev2.Activities.Specs.BaseTypes.CommonSteps.AddGuidToPath(sourceLocation, getGuid);"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $_
}