if ([string]::IsNullOrEmpty($PSScriptRoot) -and -not [string]::IsNullOrEmpty($MyInvocation.MyCommand.Path)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$ProjectDir = (Get-Item $PSScriptRoot ).FullName
$fileName = "$ProjectDir\UIMap.Designer.cs"

$PreviousLine = ""
(Get-Content $fileName) | 
    Foreach-Object {
        if ($_ -match "using System;" -and $PreviousLine -ne "    using TechTalk.SpecFlow;") 
        {
            #Add SpecFlow reference
            "    using TechTalk.SpecFlow;"
        }
        if ($_ -match "public partial class UIMap" -and $PreviousLine -ne "    [Binding]") 
        {
            #Add SpecFlow class attribute
            "    [Binding]"
        }
        if ($_ -match "        public void *" -and -not $PreviousLine.StartsWith("        [When(@`""))
        {
            #Add SpecFlow step attribute
            "        [When(@`"I " + $_.Substring("        public void ".length, $_.length - "        public void ".length - "()".length).replace("_", " ") + "`")]"
        }
        $_ # send the current line to output
        $PreviousLine = $_
    } | Set-Content $fileName