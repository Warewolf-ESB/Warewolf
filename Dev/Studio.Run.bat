"%~dp0TestScripts\Tests\Compile All Tests.bat"
if not %errorlevel%==0 pause
"%~dp0TestScripts\Studio\Startup.bat"
if not %errorlevel%==0 pause