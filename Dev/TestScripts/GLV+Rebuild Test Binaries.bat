git -C "%~dp0..\.." reset --hard
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0..\.." clean -xdf
if not %errorlevel%==0 pause & exit 1
git -C "%~dp0..\.." pull
if not %errorlevel%==0 pause & exit 1
"%~dp0..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug
if not %errorlevel%==0 pause & exit 1