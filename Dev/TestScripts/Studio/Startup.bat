REM ********************************************************************************************************************
REM * Hi-Jack the Auto Build Variables by QtAgent since this is injected after it has REM * setup
REM * Open the autogenerated qtREM * setup in the test run location of
REM * C:\Users\IntegrationTester\AppData\Local\VSEQT\QTAgent\...
REM * For example:
REM * set DeploymentDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set TestRunDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set TestRunResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set TotalAgents=5
REM * set AgentWeighting=100
REM * set AgentLoadDistributor=Microsoft.VisualStudio.TestTools.Execution.AgentLoadDistributor
REM * set AgentId=1
REM * set TestDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set ResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results
REM * set DataCollectionEnvironmentContext=Microsoft.VisualStudio.TestTools.Execution.DataCollectionEnvironmentContext
REM * set TestLogsDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set ControllerName=rsaklfsvrtfsbld:6901
REM * set TestDeploymentDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set AgentName=RSAKLFTST7X64-3
REM ********************************************************************************************************************

REM ** Check for admin **
echo Administrative permissions required. Detecting permissions...
REM using the "net session" command to detect admin, it requires elevation in the most operating systems - Ashley
IF EXIST %windir%\nircmd.exe (nircmd elevate net session >nul 2>&1) else (net session >nul 2>&1)
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
) else (
	echo Failure: Current permissions inadequate.
	exit 1
)

REM ** Kill The Warewolf ;) **
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Studio.exe" /T /F) else (taskkill /im "Warewolf Studio.exe" /T /F)
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Server.exe" /T /F) else (taskkill /im "Warewolf Server.exe" /T /F)

REM  Wait 5 seconds ;)
ping -n 5 -w 1000 192.0.2.2 > nul

REM ** Delete the Warewolf ProgramData folder
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Resources") else (elevate rd /S /Q "%PROGRAMDATA%\Warewolf\Resources")
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Workspaces") else (elevate rd /S /Q "%PROGRAMDATA%\Warewolf\Workspaces")
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Server Settings") else (elevate rd /S /Q "%PROGRAMDATA%\Warewolf\Server Settings")
IF EXIST "%PROGRAMDATA%\Warewolf\Resources" powershell.exe -nologo -noprofile -command "& { Remove-Item '%PROGRAMDATA%\Warewolf\Resources' -Recurse -Force }"
IF EXIST "%PROGRAMDATA%\Warewolf\Workspaces" powershell.exe -nologo -noprofile -command "& { Remove-Item '%PROGRAMDATA%\Warewolf\Workspaces' -Recurse -Force }"
IF EXIST "%PROGRAMDATA%\Warewolf\Server Settings" powershell.exe -nologo -noprofile -command "& { Remove-Item '%PROGRAMDATA%\Warewolf\Server Settings' -Recurse -Force }"
IF EXIST "%PROGRAMDATA%\Warewolf\Resources" exit 1
IF EXIST "%PROGRAMDATA%\Warewolf\Workspaces" exit 1
IF EXIST "%PROGRAMDATA%\Warewolf\Server Settings" exit 1

REM Init paths to Warewolf server under test
IF EXIST "%DeploymentDirectory%\DebugServer.zip" powershell.exe -nologo -noprofile -command "& { Expand-Archive '%DeploymentDirectory%\DebugServer.zip' '%DeploymentDirectory%\Server' -Force }"
IF EXIST "%DeploymentDirectory%\DebugStudio.zip" powershell.exe -nologo -noprofile -command "& { Expand-Archive '%DeploymentDirectory%\DebugStudio.zip' '%DeploymentDirectory%\Studio' -Force }"
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0Server\
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET DeploymentDirectory=%DeploymentDirectory%\Server

REM ** Start Warewolf server from deployed binaries **
IF EXIST "%DeploymentDirectory%\ServerStarted" DEL "%DeploymentDirectory%\ServerStarted"
IF EXIST %windir%\nircmd.exe (nircmd elevate "%DeploymentDirectory%\Warewolf Server.exe") else (START "%DeploymentDirectory%\Warewolf Server.exe" /D "%DeploymentDirectory%" "Warewolf Server.exe")
@echo Started "%DeploymentDirectory%\Warewolf Server.exe".

rem using the "ping" command as make-shift wait or sleep command, wait for server started file to appear
:WaitForServerStart
IF EXIST "%DeploymentDirectory%\ServerStarted" goto StartStudio 
rem wait for 5 seconds before trying again
@echo Waiting 5 seconds...
ping -n 5 -w 1000 192.0.2.2 > nul
goto WaitForServerStart 

:StartStudio
REM Try use Default Workspace Layout
IF EXIST "%DeploymentDirectory%\..\DefaultWorkspaceLayout.xml" COPY /Y "%DeploymentDirectory%\..\DefaultWorkspaceLayout.xml" "%LocalAppData%\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml"
REM Init paths to Warewolf studio under test
IF NOT EXIST "%DeploymentDirectory%\..\Studio\Warewolf Studio.exe" IF EXIST "%~dp0..\..\Dev2.Studio\bin\Debug\Warewolf Studio.exe" SET DeploymentDirectory=%~dp0..\..\Dev2.Studio\bin\Debug
IF EXIST "%DeploymentDirectory%\..\Studio\Warewolf Studio.exe" SET DeploymentDirectory=%DeploymentDirectory%\..\Studio
REM ** Start Warewolf studio from deployed binaries **
IF EXIST %windir%\nircmd.exe (nircmd elevate "%DeploymentDirectory%\Warewolf Studio.exe") else (START "%DeploymentDirectory%\Warewolf Studio.exe" /D "%DeploymentDirectory%" "Warewolf Studio.exe")

exit 0
