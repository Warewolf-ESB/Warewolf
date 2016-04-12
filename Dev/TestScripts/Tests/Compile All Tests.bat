"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\UITesting.sln"
if not %errorlevel%==0 pause & exit 1

IF EXIST "%vs120comntools%..\IDE\devenv.com" ( 
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\UITesting.sln" /Build Debug
if not %errorlevel%==0 pause & exit 1
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\AcceptanceTesting.sln" /Build Debug
) else (
"%vs140comntools%..\IDE\devenv.com" "%~dp0..\..\UITesting.sln" /Build Debug
if not %errorlevel%==0 pause & exit 1
"%vs140comntools%..\IDE\devenv.com" "%~dp0..\..\AcceptanceTesting.sln" /Build Debug )

if not %errorlevel%==0 pause & exit 1