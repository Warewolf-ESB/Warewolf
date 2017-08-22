Get-ChildItem "$PSScriptRoot\*.feature.cs" |
    Foreach-Object {
        $PreviousLine = ""
        (Get-Content $_) | 
            Foreach-Object {
                $_.Replace("UnitTesting.TestClassAttribute", "UITesting.CodedUITestAttribute")
                $PreviousLine = $_
    } | Set-Content $_
}