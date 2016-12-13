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
call "%~dp0Cleanup.bat"

:StopRetryingClean

REM Init paths to Warewolf server under test
IF EXIST "%~dp0Warewolf Server.exe" SET ServerBinDirectory=%~dp0
IF EXIST "%~dp0Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Server
IF EXIST "%~dp0DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0DebugServer
IF EXIST "%~dp0Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0Dev2.Server\bin\Debug
IF EXIST "%~dp0..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..
IF EXIST "%~dp0..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Server
IF EXIST "%~dp0..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\DebugServer
IF EXIST "%~dp0..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..
IF EXIST "%~dp0..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Server
IF EXIST "%~dp0..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\DebugServer
IF EXIST "%~dp0..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..
IF EXIST "%~dp0..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Server
IF EXIST "%~dp0..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\Dev2.Server\bin\Debug
IF EXIST "%~dp0..\..\..\..\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..
IF EXIST "%~dp0..\..\..\..\Server\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Server
IF EXIST "%~dp0..\..\..\..\DebugServer\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\DebugServer
IF EXIST "%~dp0..\..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Dev2.Server\bin\Debug
IF EXIST "%ServerBinDirectory%\Warewolf Server.exe" (
	echo Success: Warewolf server found at %ServerBinDirectory%\Warewolf Server.exe.
) else (
	echo Failure: Cannot find Warewolf Server.exe.
	exit 1
)

REM ** Try Refresh Warewolf Server Bin Resources and Tests
IF EXIST "%~dp0..\..\Resources - Debug\Resources" echo d | xcopy /S /Y "%~dp0..\..\Resources - Debug\Resources" "%ServerBinDirectory%\Resources"
IF EXIST "%~dp0..\..\Resources - Debug\Tests" echo d | xcopy /S /Y "%~dp0..\..\Resources - Debug\Tests" "%ServerBinDirectory%\Tests"

REM ** Try Refresh Warewolf ProgramData Resources and Tests
IF NOT EXIST "%ProgramData%\Warewolf\Resources" IF EXIST "%ServerBinDirectory%\Resources" echo d | xcopy /S /Y "%ServerBinDirectory%\Resources" "%ProgramData%\Warewolf\Resources"
IF NOT EXIST "%ProgramData%\Warewolf\Tests" IF EXIST "%ServerBinDirectory%\Tests" echo d | xcopy /S /Y "%ServerBinDirectory%\Tests" "%ProgramData%\Warewolf\Tests"

REM ** Start Warewolf server from deployed binaries **
IF EXIST "%ServerBinDirectory%\ServerStarted" DEL "%ServerBinDirectory%\ServerStarted"
IF EXIST %windir%\nircmd.exe (nircmd elevate "%ServerBinDirectory%\Warewolf Server.exe") else (START "%ServerBinDirectory%\Warewolf Server.exe" /D "%ServerBinDirectory%" "Warewolf Server.exe")
@echo Started "%ServerBinDirectory%\Warewolf Server.exe".

REM ** Wait for server start
@echo off
:WaitForServerStart
set /a LoopCounter=0
:MainLoopBody
IF EXIST "%ServerBinDirectory%\ServerStarted" goto StartStudio
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 30 echo Timed out waiting for the Warewolf server to start. &exit /b
@echo Waiting 10 more seconds for %ServerBinDirectory%\ServerStarted file to appear...
waitfor ServerStart /t 10 2>NUL
goto MainLoopBody

:StartStudio
REM Try use Default Workspace Layout
IF EXIST "%ServerBinDirectory%\DefaultWorkspaceLayout.xml" COPY /Y "%ServerBinDirectory%\DefaultWorkspaceLayout.xml" "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml"
IF EXIST "%ServerBinDirectory%\..\DefaultWorkspaceLayout.xml" COPY /Y "%ServerBinDirectory%\..\DefaultWorkspaceLayout.xml" "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml"
IF EXIST "%~dp0..\..\Warewolf.UITests\Properties\DefaultWorkspaceLayout.xml" COPY /Y "%~dp0..\..\Warewolf.UITests\Properties\DefaultWorkspaceLayout.xml" "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml"
REM Init paths to Warewolf studio under test
IF EXIST "%~dp0..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET ServerBinDirectory=%~dp0..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET ServerBinDirectory=%~dp0..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET ServerBinDirectory=%~dp0..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET ServerBinDirectory=%~dp0..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET ServerBinDirectory=%~dp0..\..\..\..\..\..\Dev2.Studio\bin\Debug
IF EXIST "%ServerBinDirectory%\..\Studio\Warewolf Studio.exe" SET ServerBinDirectory=%ServerBinDirectory%\..\Studio
IF EXIST "%ServerBinDirectory%\..\DebugStudio\Warewolf Studio.exe" SET ServerBinDirectory=%ServerBinDirectory%\..\DebugStudio

REM ** Start Warewolf studio from deployed binaries **
@echo on
IF EXIST %windir%\nircmd.exe (nircmd elevate "%ServerBinDirectory%\Warewolf Studio.exe") else (START "%ServerBinDirectory%\Warewolf Studio.exe" /D "%ServerBinDirectory%" "Warewolf Studio.exe")
@echo Started "%ServerBinDirectory%\Warewolf Studio.exe".

exit 0
