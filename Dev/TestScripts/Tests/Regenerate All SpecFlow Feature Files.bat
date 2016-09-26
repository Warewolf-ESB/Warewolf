cd /d "%~dp0..\.."
.nuget\nuget.exe restore AcceptanceTesting.sln
if not %errorlevel%==0 pause & exit 1
if exist packages\SpecFlow.1.9.0\tools\specflow.exe.config del packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo ^<?xml version="1.0" encoding="utf-8" ?^>  >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo ^<configuration^>  >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo     ^<startup^>  >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo         ^<supportedRuntime version="v4.0.30319" /^>  >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo     ^</startup^>  >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
echo ^</configuration^> >>packages\SpecFlow.1.9.0\tools\specflow.exe.config
if not %errorlevel%==0 pause & exit 1
for /d %%d in (*.Specs) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Warewolf.UIBindingTests.*) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Dev2.Installer.Specs\Warewolf.UIBindingTests.Installer.csproj /force /verbose
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Warewolf.ToolsSpecs\Warewolf.ToolsSpecs.csproj /force /verbose