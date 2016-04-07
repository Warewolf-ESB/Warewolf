git -C "%~dp0..\.." reset --hard
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0..\.." clean -xdf
if not %errorlevel%==0 pause & exit 1
if "%1"=="" (git -C "%~dp0..\.." pull) else (git -C "%~dp0..\.." checkout %1)
if not %errorlevel%==0 pause & exit 1