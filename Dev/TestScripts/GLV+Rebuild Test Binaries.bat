git -C "%~dp0..\.." reset --hard
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0..\.." clean -xdf
if not %errorlevel%==0 pause & exit 1
if "%1"=="" (git -C "%~dp0..\.." pull) else (git -C "%~dp0..\.." checkout %1)
if not %errorlevel%==0 pause & exit 1
"%~dp0..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
powershell -ExecutionPolicy ByPass -File "%~dp0..\BakeInVersion.ps1"
if not %errorlevel%==0 pause & exit 1
IF EXIST "%vs120comntools%..\IDE\devenv.com" ( "%vs120comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug
) else (
IF EXIST "%vs140comntools%..\IDE\devenv.com" "%vs140comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug )
if not %errorlevel%==0 git -C "%~dp0.." checkout "AssemblyCommonInfo.cs" & git -C "%~dp0.." checkout "AssemblyCommonInfo.fs" & pause & exit 1
powershell -ExecutionPolicy ByPass -File "%~dp0..\ReadOutVersion.ps1"
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0.." checkout "AssemblyCommonInfo.cs"
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0.." checkout "AssemblyCommonInfo.fs"
if not %errorlevel%==0 pause & exit 1