Get-ChildItem "$PSScriptRoot\*.feature.cs" |
    Foreach-Object {
        (Get-Content $_) | 
            Foreach-Object {
                $_.Replace("[Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute(""Warewolf_Studio.exe"")]", "[Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute(""Warewolf Studio.exe"")]")
		} | Set-Content $_
	}