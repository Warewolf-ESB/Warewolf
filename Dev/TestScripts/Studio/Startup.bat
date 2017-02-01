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

REM ** Init paths to Warewolf server under test **
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET ServerBinDirectory=%ServerBinDirectory%\Server
IF EXIST "%~dp0Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Dev2.Server\bin\Debug
IF EXIST "%~dp0DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0DebugServer
IF EXIST "%~dp0Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Server
IF EXIST "%~dp0Warewolf Server.exe" SET ServerBinDirectory=%~dp0
IF EXIST "%~dp0..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\DebugServer
IF EXIST "%~dp0..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Server
IF EXIST "%~dp0..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..
IF EXIST "%~dp0..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\DebugServer
IF EXIST "%~dp0..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Server
IF EXIST "%~dp0..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..
IF EXIST "%~dp0..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Server
IF EXIST "%~dp0..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..
IF EXIST "%~dp0..\..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Server
IF EXIST "%~dp0..\..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..
IF NOT EXIST "%ServerBinDirectory%\Warewolf Server.exe" echo Cannot find Warewolf Server.exe & waitfor ErrorMessage /t 10 2>NUL & exit 1
IF EXIST "%ServerBinDirectory%\ServerStarted" DEL "%ServerBinDirectory%\ServerStarted"

sc interrogate "Warewolf Server"
if %ERRORLEVEL% EQU 1060 GOTO NotInstalled
if %ERRORLEVEL% EQU 1061 GOTO NotReady
if %ERRORLEVEL% EQU 1062 GOTO NotStarted
if %ERRORLEVEL% EQU 0 GOTO Running

:NotInstalled
@echo on
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate sc create "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand
	) else (
		nircmd elevate sc create "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand
	)
) else (
	IF NOT "%1"=="" (
		sc create "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand
	) else (
		sc create "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand
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
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING")
waitfor ServiceReady /t 10 2>NUL
sc interrogate "Warewolf Server"
goto WaitForServiceReadyLoopBody

:Running
IF EXIST %windir%\nircmd.exe (nircmd elevate sc stop "Warewolf Server") else (sc stop "Warewolf Server")
IF NOT "%1"=="" (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand) else (sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand)
) else (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand)
)
GOTO StartService

:NotStarted
IF NOT "%1"=="" (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand) else (sc config "Warewolf Server" binPath= "%DotCoverExePath% cover /TargetExecutable=\"%ServerBinDirectory%\Warewolf Server.exe\" /LogFile=\"%ProgramData%\Warewolf\Server Log\dotCover.log\" /Output=\"%ProgramData%\Warewolf\Server Log\dotCover.dcvr\"" start= demand)
) else (
	IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%ServerBinDirectory%\Warewolf Server.exe" start= demand)
)
GOTO StartService

:StartService
REM ** Try Refresh Warewolf Server Bin Resources and Tests **
IF EXIST "%~dp0..\..\..\Resources - ServerTests" echo d | xcopy /S /Y "%~dp0..\..\..\Resourses - ServerTests" "%ServerBinDirectory%\Resources - ServerTests"
IF EXIST "%~dp0..\..\..\Resourses - Release" echo d | xcopy /S /Y "%~dp0..\..\..\Resources - Release" "%ServerBinDirectory%"

REM ** Try Refresh Warewolf ProgramData Resources and Tests **
IF EXIST "%ServerBinDirectory%\Resources - ServerTests" echo d | xcopy /S /Y "%ServerBinDirectory%\Resources - ServerTests" "%ProgramData%\Warewolf"
IF EXIST "%ServerBinDirectory%\Resources - Release" echo d | xcopy /S /Y "%ServerBinDirectory%\Resources - Release" "%ProgramData%\Warewolf"

REM ** Start the server service
IF EXIST %windir%\nircmd.exe (nircmd elevate sc start "Warewolf Server") else (sc start "Warewolf Server")

:WaitForServerStart
set /a LoopCounter=0
:WaitForServerStartLoopBody
IF EXIST "%ServerBinDirectory%\ServerStarted" GOTO StartStudio
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for "%ServerBinDirectory%\ServerStarted" file to appear...
waitfor ServerStart /t 10 2>NUL
goto WaitForServerStartLoopBody

:StartStudio
REM Init paths to Warewolf studio under test
IF EXIST "%~dp0Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\Studio
IF EXIST "%ServerBinDirectory%\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\DebugStudio

IF EXIST "%~dp0..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\Studio
IF EXIST "%ServerBinDirectory%\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\DebugStudio

IF EXIST "%~dp0..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\Studio
IF EXIST "%ServerBinDirectory%\..\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\DebugStudio

IF EXIST "%~dp0..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\..\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\Studio
IF EXIST "%ServerBinDirectory%\..\..\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\DebugStudio

IF EXIST "%~dp0..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\..\..\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\Studio
IF EXIST "%ServerBinDirectory%\..\..\..\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\DebugStudio

IF EXIST "%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\..\..\..\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\..\Studio
IF EXIST "%ServerBinDirectory%\..\..\..\..\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\..\DebugStudio

IF EXIST "%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\..\..\..\..\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\..\..\Studio
IF EXIST "%ServerBinDirectory%\..\..\..\..\..\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\..\..\..\..\..\DebugStudio

REM ** Start Studio **
@echo on
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate "%DotCoverExePath:\"=%" cover /TargetExecutable="%StudioBinDirectory%\Warewolf Studio.exe" /LogFile="%LocalAppData%\Warewolf\Studio Logs\dotCover.log" /Output="%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr"
	) else (
		nircmd elevate "%StudioBinDirectory%\Warewolf Studio.exe"
	)
) else (
	IF NOT "%1"=="" (
		"%DotCoverExePath:\"=%" cover /TargetExecutable="%StudioBinDirectory%\Warewolf Studio.exe" /LogFile="%LocalAppData%\Warewolf\Studio Logs\dotCover.log" /Output="%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr"
	) else (
		START "%StudioBinDirectory%\Warewolf Studio.exe" /D "%StudioBinDirectory%" "Warewolf Studio.exe"
	)
)
IF NOT "%1"=="" (
	waitfor StudioStart /t 600 2>NUL
) else (
	waitfor StudioStart /t 60 2>NUL
)
@echo Started "%StudioBinDirectory%\Warewolf Studio.exe".
exit 0
