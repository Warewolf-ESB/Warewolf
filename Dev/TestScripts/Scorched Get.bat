git --version
if "%errorlevel%"=="0" GOTO GitCommandline
IF NOT EXIST "%programfiles(x86)%\Git\bin\git.exe" echo GIT not found. &pause &exit 1
"%programfiles(x86)%\Git\bin\git.exe" -C "%~dp0..\.." reset --hard
if not %errorlevel%==0 pause & exit 1
"%programfiles(x86)%\Git\bin\git.exe" -C "%~dp0..\.." clean -xdf
if not %errorlevel%==0 pause & exit 1
echo Repo scorched. Get version? Leave blank to get latest. Branch and tag names are also allowed.
set /p Version=
if "%Version%"=="" ("%programfiles(x86)%\Git\bin\git.exe" -C "%~dp0..\.." pull) else ("%programfiles(x86)%\Git\bin\git.exe" -C "%~dp0..\.." fetch --all & "%programfiles(x86)%\Git\bin\git.exe" -C "%~dp0..\.." checkout %Version%)
if not %errorlevel%==0 pause & exit 1
GOTO exit

:GitCommandline
git -C "%~dp0..\.." reset --hard
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0..\.." clean -xdf
@echo off
if not %errorlevel%==0 pause & exit 1
echo Repo scorched. Get version? Leave blank to get latest. Branch and tag names are also allowed.
set /p Version=
if "%Version%"=="" (git -C "%~dp0..\.." pull) else (git -C "%~dp0..\.." fetch --all & git -C "%~dp0..\.." checkout %Version%)
@echo on
if not %errorlevel%==0 pause & exit 1
:exit