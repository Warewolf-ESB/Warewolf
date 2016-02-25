git reset --hard
git clean -xdf
git pull
"..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug
if not %errorlevel%==0 pause