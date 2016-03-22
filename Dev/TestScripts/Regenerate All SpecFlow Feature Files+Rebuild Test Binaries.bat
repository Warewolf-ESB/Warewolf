cd /d "%~dp0.."
.nuget\nuget.exe restore AcceptanceTesting.sln
if not %errorlevel%==0 pause & exit 1
for /d %%d in (*.Specs) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Warewolf.AcceptanceTesting.*) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Dev2.Studio.UISpecs\Dev2.Studio.UI.Specs.csproj /force /verbose
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Dev2.Installer.Specs\Warewolf.AcceptanceTesting.Installer.csproj /force /verbose
powershell -file BakeInVersion.ps1
if not %errorlevel%==0 pause & exit 1
"%vs120comntools%..\IDE\devenv.com" AcceptanceTesting.sln /Build Debug
if not %errorlevel%==0 git checkout AssemblyCommonInfo.cs & git checkout AssemblyCommonInfo.fs & pause & exit 1
powershell -file ReadOutVersion.ps1
if not %errorlevel%==0 pause & exit 1
git checkout AssemblyCommonInfo.cs
if not %errorlevel%==0 pause & exit 1
git checkout AssemblyCommonInfo.fs
if not %errorlevel%==0 pause & exit 1