cd /d "%~dp0..\..\.."
Dev\TestScripts\Tests\nuget.exe restore Dev\AcceptanceTesting.sln
if not %errorlevel%==0 pause & exit 1
for /d %%d in (Dev\*.Specs) do (
	Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "Dev\%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Dev\Warewolf.UIBindingTests.*) do (
	Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "Dev\%%d\%%d.csproj" /force /verbose
)
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.ToolsSpecs\Warewolf.ToolsSpecs.csproj /force /verbose
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.SecuritySpecs\Warewolf.SecuritySpecs.csproj /force /verbose
Dev\packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Dev\Warewolf.UISpecs\Warewolf.UISpecs.csproj /force /verbose