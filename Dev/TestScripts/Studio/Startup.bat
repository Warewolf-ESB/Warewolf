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
@echo on

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
		set DotCoverExePath="%LocalAppData%\JetBrains\Installations\dotCover07\dotCover.exe"
	) else (
		set DotCoverExePath=%1
	)
)

REM ** Init paths to Warewolf server under test **
IF EXIST "%~dp0..\..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..
IF EXIST "%~dp0..\..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Server
IF EXIST "%~dp0..\..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..
IF EXIST "%~dp0..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Server
IF EXIST "%~dp0..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..
IF EXIST "%~dp0..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Server
IF EXIST "%~dp0..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\DebugServer
IF EXIST "%~dp0..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..
IF EXIST "%~dp0..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Server
IF EXIST "%~dp0..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\DebugServer
IF EXIST "%~dp0..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Dev2.Server\bin\Debug
IF EXIST "%~dp0Warewolf Server.exe" SET ServerBinDirectory=%~dp0
IF EXIST "%~dp0Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Server
IF EXIST "%~dp0DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0DebugServer
IF EXIST "%~dp0Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Dev2.Server\bin\Debug
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET ServerBinDirectory=%DeploymentDirectory%\Server
IF EXIST "%ServerBinDirectory%\Warewolf Server.exe" (
	echo Success: Warewolf server found at %ServerBinDirectory%\Warewolf Server.exe.
) else (
	echo Failure: Cannot find Warewolf Server.exe.
	exit 1
)

REM ** Try Refresh Warewolf Server Bin Resources and Tests
IF EXIST "%~dp0..\..\Resources - UITests" echo d | xcopy /S /Y "%~dp0..\..\Resources - UITests" "%ServerBinDirectory%\Resources - UITests"

REM ** Try Refresh Warewolf ProgramData Resources and Tests
IF EXIST "%ServerBinDirectory%\Resources - UITests" echo d | xcopy /S /Y "%ServerBinDirectory%\Resources - UITests" "%ProgramData%\Warewolf"

REM ** Start Warewolf server from deployed binaries **
IF EXIST "%ServerBinDirectory%\ServerStarted" DEL "%ServerBinDirectory%\ServerStarted"
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate %DotCoverExePath% cover /TargetExecutable="%ServerBinDirectory%\Warewolf Server.exe" /LogFile="%ProgramData%\Warewolf\Server Log\dotCover.log" /Output="%ProgramData%\Warewolf\Server Log\dotCover.dcvr"
	) else (
		nircmd elevate "%ServerBinDirectory%\Warewolf Server.exe"
	)
) else (
	IF NOT "%1"=="" (
		%DotCoverExePath% cover /TargetExecutable="%ServerBinDirectory%\Warewolf Server.exe" /LogFile="%ProgramData%\Warewolf\Server Log\dotCover.log" /Output="%ProgramData%\Warewolf\Server Log\dotCover.dcvr"
	) else (
		START "%ServerBinDirectory%\Warewolf Server.exe" /D "%ServerBinDirectory%" "Warewolf Server.exe"
	)
)
@echo Started "%ServerBinDirectory%\Warewolf Server.exe".

REM ** Wait for server start
@echo off
:WaitForServerStart
set /a LoopCounter=0
:MainLoopBody
IF EXIST "%ServerBinDirectory%\ServerStarted" goto StartStudio
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 300 echo Timed out waiting for the Warewolf server to start. &exit /b
@echo Waiting 10 more seconds for %ServerBinDirectory%\ServerStarted file to appear...
waitfor ServerStart /t 10 2>NUL
goto MainLoopBody

:StartStudio
REM Try Delete Workspace Layout
IF EXIST "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml" DEL "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml"
REM Init paths to Warewolf studio under test
IF EXIST "%~dp0..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET StudioBinDirectory=%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\Studio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\Studio
IF EXIST "%ServerBinDirectory%\..\DebugStudio\Warewolf Studio.exe" SET StudioBinDirectory=%ServerBinDirectory%\..\DebugStudio

REM ** Start Warewolf studio from deployed binaries **
@echo on
IF EXIST %windir%\nircmd.exe (
	IF NOT "%1"=="" (
		nircmd elevate %DotCoverExePath% cover /TargetExecutable="%StudioBinDirectory%\Warewolf Studio.exe" /LogFile="%LocalAppData%\Warewolf\Studio Logs\dotCover.log" /Output="%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr"
	) else (
		nircmd elevate "%StudioBinDirectory%\Warewolf Studio.exe"
	)
) else (
	IF NOT "%1"=="" (
		%DotCoverExePath% cover /TargetExecutable="%StudioBinDirectory%\Warewolf Studio.exe" /LogFile="%LocalAppData%\Warewolf\Studio Logs\dotCover.log" /Output="%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr"
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
