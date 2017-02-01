set /a LoopCounter=0
:RetryClean
IF NOT EXIST "%PROGRAMDATA%\Warewolf\Resources" IF NOT EXIST "%PROGRAMDATA%\Warewolf\Tests" IF NOT EXIST "%PROGRAMDATA%\Warewolf\Workspaces" IF NOT EXIST "%PROGRAMDATA%\Warewolf\Server Settings" GOTO StopRetrying

:InterrogateService
sc interrogate "Warewolf Server"
if %ERRORLEVEL% EQU 1060 GOTO NotInstalled
if %ERRORLEVEL% EQU 1061 GOTO NotReady
if %ERRORLEVEL% EQU 1062 GOTO NotStarted
if %ERRORLEVEL% EQU 0 GOTO Running

:NotInstalled
GOTO StopProcess

:NotStarted
GOTO StopProcess

:Running
GOTO StopService

:NotReady
set /a LoopCounter=0
:WaitForServiceReadyLoopBody
IF NOT %ERRORLEVEL% EQU 1061 GOTO Running
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %ComputerName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for server service to be ready...
waitfor ServiceReady /t 10 2>NUL
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING")

IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN")

IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq NOT RESPONDING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq NOT RESPONDING")
sc interrogate "Warewolf Server"
goto WaitForServiceReadyLoopBody

:StopService
REM Stop Server Service:
IF EXIST %windir%\nircmd.exe (nircmd elevate sc stop "Warewolf Server") else (sc stop "Warewolf Server")
:StopProcess
IF "%1"=="softkill" (
	waitfor ServiceStop /t 10 2>NUL
	REM ** Soft Kill **
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING") else (taskkill /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING") else (taskkill /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING")
	
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN") else (taskkill /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN") else (taskkill /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN")
) else (	
	REM ** Forced TaskKill **
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq RUNNING")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq RUNNING")

	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq UNKNOWN")
	IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq UNKNOWN")
)
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq NOT RESPONDING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im "Warewolf Server.vshost.exe" /fi "STATUS eq NOT RESPONDING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq NOT RESPONDING") else (taskkill /f /im WarewolfCOMIPC.exe /fi "STATUS eq NOT RESPONDING")
	
REM ** Delete the Warewolf ProgramData folder
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Resources") else (rd /S /Q "%PROGRAMDATA%\Warewolf\Resources")
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Tests") else (rd /S /Q "%PROGRAMDATA%\Warewolf\Tests")
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Workspaces") else (rd /S /Q "%PROGRAMDATA%\Warewolf\Workspaces")
IF EXIST %windir%\nircmd.exe (nircmd elevate cmd /c rd /S /Q "%PROGRAMDATA%\Warewolf\Server Settings\") else (rd /S /Q "%PROGRAMDATA%\Warewolf\Server Settings")
@echo off
IF EXIST "%PROGRAMDATA%\Warewolf\Resources" echo ERROR CANNOT DELETE %PROGRAMDATA%\Warewolf\Resources
IF EXIST "%PROGRAMDATA%\Warewolf\Tests" echo ERROR CANNOT DELETE %PROGRAMDATA%\Warewolf\Tests
IF EXIST "%PROGRAMDATA%\Warewolf\Workspaces" echo ERROR CANNOT DELETE %PROGRAMDATA%\Warewolf\Workspaces
IF EXIST "%PROGRAMDATA%\Warewolf\Server Settings" echo ERROR CANNOT DELETE %PROGRAMDATA%\Warewolf\Server Settings
@echo on

set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 24 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 24: Waiting 10 more seconds for "%PROGRAMDATA%\Warewolf" folder cleanup...
waitfor ServerWorkspaceClean /t 10 2>NUL
set errorlevel=0
goto RetryClean

:StopRetrying
IF NOT "%1"=="" exit 0