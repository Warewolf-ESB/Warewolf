mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -Projects Warewolf.Auditing.Tests -InContainer
echo Exiting with exit code %ERRORLEVEL%
exit %ERRORLEVEL%