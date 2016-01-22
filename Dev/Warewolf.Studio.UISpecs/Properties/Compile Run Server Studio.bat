"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\Server.sln" /Build Debug
if not %errorlevel%==0 pause
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\Studio.sln" /Build Debug
if not %errorlevel%==0 pause
"%~dp0..\..\TestScripts\Studio\Startup.bat"
if not %errorlevel%==0 pause