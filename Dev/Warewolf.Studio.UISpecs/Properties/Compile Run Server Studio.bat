"%vs120comntools%..\IDE\devenv.exe" "%~dp0..\..\Server.sln" /Build Debug
"%vs120comntools%..\IDE\devenv.exe" "%~dp0..\..\Studio.sln" /Build Debug
"%~dp0..\..\TestScripts\Studio\Startup.bat"
if not %errorlevel%==0 pause