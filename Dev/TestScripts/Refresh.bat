git reset --hard
git clean -xdf
git pull
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug
if not %errorlevel%==0 pause