"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\Server.sln"
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\Server.sln" /Build Debug
if not %errorlevel%==0 pause
"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\Studio.sln"
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\Studio.sln" /Build Debug
if not %errorlevel%==0 pause