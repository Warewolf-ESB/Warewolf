"%~dp0..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
cd /d "%~dp0.."
for /d %%d in (*.Specs) do (
	"%~dp0..\packages\SpecFlow.1.9.0\tools\specflow.exe" generateAll "%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Warewolf.AcceptanceTesting.*) do (
	"%~dp0..\packages\SpecFlow.1.9.0\tools\specflow.exe" generateAll "%%d\%%d.csproj" /force /verbose
)
"%~dp0..\packages\SpecFlow.1.9.0\tools\specflow.exe" generateAll "%~dp0..\Dev2.Studio.UISpecs\Dev2.Studio.UI.Specs.csproj" /force /verbose
"%~dp0..\packages\SpecFlow.1.9.0\tools\specflow.exe" generateAll "%~dp0..\Dev2.Installer.Specs\Warewolf.AcceptanceTesting.Installer.csproj" /force /verbose
pause