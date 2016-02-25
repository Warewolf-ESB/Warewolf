git -C "%~dp0" reset --hard
git -C "%~dp0" clean -xdf
git -C "%~dp0" pull
"%~dp0..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\AcceptanceTesting.sln" /Build Debug
if not %errorlevel%==0 pause