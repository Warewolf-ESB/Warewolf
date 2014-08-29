@echo off
cd %CD%\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Test
copy /Y "%CD%\TestSettings\CI Environments\Integration.testsettings" "%CD%\Integration.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\Dev2.IntegrationTest.dll" /testSettings:"Integration.testsettings"
DEL /F /Q "%CD%\Integration.testsettings"
pause