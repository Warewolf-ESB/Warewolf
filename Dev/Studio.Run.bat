REM ** Check for admin **
@echo off
echo Administrative permissions required. Detecting permissions...
REM using the "net session" command to detect admin, it requires elevation in the most operating systems - Ashley
net session >nul 2>&1
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
) else (
	echo Failure: Current permissions inadequate. &pause &exit 1
)
@echo on

"%~dp0TestScripts\Tests\NuGet.exe" restore "%~dp0Server.sln" -SolutionDirectory "%~dp0."
"%~dp0TestScripts\Tests\NuGet.exe" restore "%~dp0Studio.sln" -SolutionDirectory "%~dp0."
if not %errorlevel%==0 pause

IF EXIST MSBuild (
	MSBuild "%~dp0Server.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	MSBuild "%~dp0Studio.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" (
	"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0Server.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0Studio.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
	"%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "%~dp0Server.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "%~dp0Studio.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" (
	"%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%~dp0Server.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
	if not %errorlevel%==0 pause & exit 1
	"%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%~dp0Studio.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
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