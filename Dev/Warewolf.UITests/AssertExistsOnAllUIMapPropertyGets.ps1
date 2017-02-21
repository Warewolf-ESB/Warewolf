$PreviousLine = ""
(Get-ChildItem "$PSScriptRoot\*UIMap.Designer.cs" -Recurse) |
    ForEach-Object {
        $fileName = $_
        (Get-Content $fileName) | 
            Foreach-Object {
                if ($_.ToString().Contains("if ((this.") -and $_.ToString().Contains("== null))") -and !($_.ToString().Contains("Params == null))")))
                {
                    $ObjectName = $_.ToString().Substring($_.ToString().IndexOf("(this.")+"(this.".Length,$_.ToString().IndexOf(" == null)")-$_.ToString().IndexOf("(this.")-"(this.".Length)
                    #Add Or Not Exists
                    $_.ToString().TrimEnd(")") + " || !" + $ObjectName + ".Exists))"
                } else { $_ }
            } | Set-Content $fileName
    }