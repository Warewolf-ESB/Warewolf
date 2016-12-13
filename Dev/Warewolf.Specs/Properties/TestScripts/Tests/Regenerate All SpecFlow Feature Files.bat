cd /d "%~dp0..\.."
TestScripts\Tests\nuget.exe restore AcceptanceTesting.sln
if not %errorlevel%==0 pause & exit 1
if exist packages\SpecFlow.2.1.0\tools\specflow.exe.config del packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo ^<?xml version="1.0" encoding="utf-8" ?^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo ^<configuration^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo     ^<startup^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^<supportedRuntime version="v4.0.30319" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo     ^</startup^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo     ^<runtime^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo       ^<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^<dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<assemblyIdentity name="TechTalk.SpecFlow.Generator" publicKeyToken="0778194805d6db41" culture="neutral" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<bindingRedirect oldVersion="2.0.0.0-2.1.0.0" newVersion="2.1.0.0" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^</dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^<dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<assemblyIdentity name="TechTalk.SpecFlow.Parser" publicKeyToken="0778194805d6db41" culture="neutral" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<bindingRedirect oldVersion="2.0.0.0-2.1.0.0" newVersion="2.1.0.0" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^</dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^<dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<assemblyIdentity name="TechTalk.SpecFlow.Utils" publicKeyToken="0778194805d6db41" culture="neutral" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<bindingRedirect oldVersion="2.0.0.0-2.1.0.0" newVersion="2.1.0.0" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^</dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^<dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<assemblyIdentity name="TechTalk.SpecFlow" publicKeyToken="0778194805d6db41" culture="neutral" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo           ^<bindingRedirect oldVersion="2.0.0.0-2.1.0.0" newVersion="2.1.0.0" /^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo         ^</dependentAssembly^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo       ^</assemblyBinding^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo     ^</runtime^>  >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
echo ^</configuration^> >>packages\SpecFlow.2.1.0\tools\specflow.exe.config
if not %errorlevel%==0 pause & exit 1
for /d %%d in (*.Specs) do (
	packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Warewolf.UIBindingTests.*) do (
	packages\SpecFlow.2.1.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Warewolf.ToolsSpecs\Warewolf.ToolsSpecs.csproj /force /verbose
packages\SpecFlow.2.1.0\tools\specflow.exe generateAll Warewolf.SecuritySpecs\Warewolf.SecuritySpecs.csproj /force /verbose