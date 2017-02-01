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
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET ServerBinDirectory=%DeploymentDirectory%\Server
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
IF NOT EXIST "%DeploymentDirectory%\Warewolf Server.exe" echo Cannot find Warewolf Server.exe & waitfor ErrorMessage /t 10 2>NUL & exit 1
IF EXIST "%DeploymentDirectory%\ServerStarted" DEL "%DeploymentDirectory%\ServerStarted"

REM ** Try Refresh Warewolf Server Bin Resources and Tests
IF EXIST "%~dp0..\..\Resourses - ServerTests" echo d | xcopy /S /Y "%~dp0..\..\Resources - ServerTests" "%DeploymentDirectory%"
IF EXIST "%~dp0..\..\Resourses - Release" echo d | xcopy /S /Y "%~dp0..\..\Resources - Release" "%DeploymentDirectory%"

REM ** Try Refresh Warewolf ProgramData Resources and Tests
IF EXIST "%DeploymentDirectory%\Resources - ServerTests" echo d | xcopy /S /Y "%DeploymentDirectory%\Resources - ServerTests" "%ProgramData%\Warewolf"
IF EXIST "%DeploymentDirectory%\Resources - Release" echo d | xcopy /S /Y "%DeploymentDirectory%\Resources - Release" "%ProgramData%\Warewolf"

REM ** Start Warewolf server from deployed binaries **
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate %DotCoverExePath% cover /TargetExecutable="%DeploymentDirectory%\Warewolf Server.exe" /LogFile="%ProgramData%\Warewolf\Server Log\dotCover.log" /Output="%ProgramData%\Warewolf\Server Log\dotCover.dcvr"
	) else (
		nircmd elevate "%DeploymentDirectory%\Warewolf Server.exe"
	)
) else (
	IF NOT "%1"=="" (
		%DotCoverExePath% cover /TargetExecutable="%DeploymentDirectory%\Warewolf Server.exe" /LogFile="%ProgramData%\Warewolf\Server Log\dotCover.log" /Output="%ProgramData%\Warewolf\Server Logs\dotCover.dcvr"
	) else (
		START "%DeploymentDirectory%\Warewolf Server.exe" /D "%DeploymentDirectory%" "Warewolf Server.exe"
	)
)
@echo Started "%DeploymentDirectory%\Warewolf Server.exe".

:WaitForServerStart
set /a LoopCounter=0
:MainLoopBody
IF EXIST "%DeploymentDirectory%\ServerStarted" exit 0
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 30 exit 1
rem wait for 5 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 30: Waiting 5 more seconds for "%DeploymentDirectory%\ServerStarted" file to appear...
waitfor ServerStart /t 5 2>NUL
goto MainLoopBody
