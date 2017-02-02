REM ** Check for admin **
@echo off
echo Administrative permissions required. Detecting permissions...
REM using the "net session" command to detect admin, it requires elevation in the most operating systems - Ashley
IF EXIST %windir%\nircmd.exe (nircmd elevate net session >nul 2>&1) else (net session >nul 2>&1)
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
) else (
	echo Failure: Current permissions inadequate.
	exit 1
)

REM ** Cleanup **
IF EXIST "%~dp0Cleanup.bat" (
	call "%~dp0Cleanup.bat"
) else (
	IF EXIST "%~dp0qtcleanup.bat" (
		call "%~dp0qtcleanup.bat"
	)
)

REM ** Init path to DotCover Exe **
IF NOT "%1"=="" (
	IF EXIST "%LocalAppData%\JetBrains\Installations\dotCover07\dotCover.exe" (
		set DotCoverExePath=\"%LocalAppData%\JetBrains\Installations\dotCover07\dotCover.exe\"
	) else (
		set DotCoverExePath=%1
	)
)

REM Init paths to Warewolf server under test
IF EXIST "%DeploymentDirectory%\DebugServer.zip" powershell.exe -nologo -noprofile -command "& { Expand-Archive '%DeploymentDirectory%\DebugServer.zip' '%DeploymentDirectory%\Server' -Force }"
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Warewolf Server.exe" SET DeploymentDirectory=%~dp0
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0Server
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\Server
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\Server
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..\Server
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..\Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..\..
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET DeploymentDirectory=%DeploymentDirectory%\Server
IF NOT EXIST "%DeploymentDirectory%\Warewolf Server.exe" echo Cannot find Warewolf Server.exe & waitfor ErrorMessage /t 10 2>NUL & exit 1
IF EXIST "%DeploymentDirectory%\ServerStarted" DEL "%DeploymentDirectory%\ServerStarted"

sc interrogate "Warewolf Server"
if %ERRORLEVEL% EQU 1060 GOTO NotInstalled
if %ERRORLEVEL% EQU 1061 GOTO NotReady
if %ERRORLEVEL% EQU 1062 GOTO NotStarted
if %ERRORLEVEL% EQU 0 GOTO Running

:NotInstalled
@echo on
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate sc create "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand
	) else (
		nircmd elevate sc create "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand
	)
) else (
	IF NOT "%1"=="" (
		sc create "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand
	) else (
		sc create "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand
	)
)
GOTO StartService

:NotReady
set /a LoopCounter=0
:WaitForServiceReadyLoopBody
IF NOT %ERRORLEVEL% EQU 1061 GOTO Running
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for server service to be ready...
waitfor ServiceReady /t 10 2>NUL
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING")
sc interrogate "Warewolf Server"
goto WaitForServiceReadyLoopBody

:Running
IF EXIST %windir%\nircmd.exe (nircmd elevate sc stop "Warewolf Server") else (sc stop "Warewolf Server")
IF NOT "%1"=="" (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand) else (sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand)
) else (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand)
)
GOTO StartService

:NotStarted
IF NOT "%1"=="" (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand) else (sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%DeploymentDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand)
) else (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand)
)
GOTO StartService

:StartService
REM ** Try Refresh Warewolf Server Bin Resources and Tests **
IF EXIST "%~dp0..\..\..\Resources - ServerTests" echo d | xcopy /S /Y "%~dp0..\..\..\Resourses - ServerTests" "%DeploymentDirectory%\Resources - ServerTests"
IF EXIST "%~dp0..\..\..\Resourses - Release" echo d | xcopy /S /Y "%~dp0..\..\..\Resources - Release" "%DeploymentDirectory%"

REM ** Try Refresh Warewolf ProgramData Resources and Tests **
IF EXIST "%DeploymentDirectory%\Resources - ServerTests" echo d | xcopy /S /Y "%DeploymentDirectory%\Resources - ServerTests" "%ProgramData%\Warewolf"
IF EXIST "%DeploymentDirectory%\Resources - Release" echo d | xcopy /S /Y "%DeploymentDirectory%\Resources - Release" "%ProgramData%\Warewolf"

REM ** Start the server service
IF EXIST %windir%\nircmd.exe (nircmd elevate sc start "Warewolf Server") else (sc start "Warewolf Server")

:WaitForServerStart
set /a LoopCounter=0
:WaitForServerStartLoopBody
IF EXIST "%DeploymentDirectory%\ServerStarted" GOTO ServerStarted
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for "%DeploymentDirectory%\ServerStarted" file to appear...
waitfor ServerStart /t 10 2>NUL
goto WaitForServerStartLoopBody

:ServerStarted
IF EXIST "%programdata%\Warewolf\Server Log\wareWolf-Server.log" TYPE "%programdata%\Warewolf\Server Log\wareWolf-Server.log"
exit 0
