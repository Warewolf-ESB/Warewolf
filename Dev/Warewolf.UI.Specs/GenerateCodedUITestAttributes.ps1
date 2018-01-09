Get-ChildItem "$PSScriptRoot\*.feature.cs" |
    Foreach-Object {
        (Get-Content $_) | 
            Foreach-Object {
                $_.Replace("UnitTesting.TestClassAttribute", "UITesting.CodedUITestAttribute")
		} | Set-Content $_
	}