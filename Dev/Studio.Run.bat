"%~dp0TestScripts\Tests\NuGet.exe" restore "%~dp0Dev2.Server\Dev2.Server.csproj" -SolutionDirectory "%~dp0"
"%~dp0TestScripts\Tests\NuGet.exe" restore "%~dp0Dev2.Studio\Dev2.Studio.csproj" -SolutionDirectory "%~dp0"
if not %errorlevel%==0 pause

IF EXIST MSBuild (
	MSBuild "%~dp0Dev2.Server\Dev2.Server.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	MSBuild "%~dp0Dev2.Studio\Dev2.Studio.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" (
	"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0Dev2.Server\Dev2.Server.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0Dev2.Studio\Dev2.Studio.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
	"%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "%~dp0Dev2.Server\Dev2.Server.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "%~dp0Dev2.Studio\Dev2.Studio.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" (
	"%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%~dp0Dev2.Server\Dev2.Server.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%~dp0Dev2.Studio\Dev2.Studio.csproj" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
)
if not %errorlevel%==0 pause

START "%~dp0Dev2.Server\bin\Debug\Warewolf Server.exe" /D "%~dp0Dev2.Server\bin\Debug" "Warewolf Server.exe"
if not %errorlevel%==0 pause

@echo off
:WaitForServerStart
IF EXIST "%~dp0Dev2.Server\bin\Debug\ServerStarted" goto StartStudio 
@echo Waiting 5 more seconds for server start...
ping -n 5 -w 1000 192.0.2.2 > nul
goto WaitForServerStart

:StartStudio
@echo on
START "%~dp0Dev2.Studio\bin\Debug\Warewolf Studio.exe" /D "%~dp0Dev2.Studio\bin\Debug" "Warewolf Studio.exe"
if not %errorlevel%==0 pause