@echo off
cd %CD%\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
copy /Y "%CD%\TestSettings\Local\Integration.testsettings" "%CD%\Integration.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\Dev2.IntegrationTest.dll" /testSettings:"Integration.testsettings"
DEL /F /Q "%CD%\Integration.testsettings"
pause