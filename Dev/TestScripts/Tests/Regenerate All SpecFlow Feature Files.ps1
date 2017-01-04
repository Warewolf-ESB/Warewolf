if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$RepoDir = (Get-Item $PSScriptRoot ).parent.parent.parent.FullName

Set-Location "$RepoDir"
Dev\TestScripts\Tests\nuget.exe restore Dev\AcceptanceTesting.sln
if (-not $env:errorlevel -eq 0) {
    pause
    exit 1
}
foreach ($ProjectDir in get-ChildItem "Dev\*.Specs") {
    $FullPath = $ProjectDir.FullName
    $ProjectName = $ProjectDir.Name
	Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "$FullPath\$ProjectName.csproj" /force /verbose
}
foreach ($ProjectDir in get-ChildItem "Dev\Warewolf.UIBindingTests.*") {
    $FullPath = $ProjectDir.FullName
    $ProjectName = $ProjectDir.Name
	Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "$FullPath\$ProjectName.csproj" /force /verbose
}
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.ToolsSpecs\Warewolf.ToolsSpecs.csproj /force /verbose
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.SecuritySpecs\Warewolf.SecuritySpecs.csproj /force /verbose
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.UISpecs\Warewolf.UISpecs.csproj /force /verbose