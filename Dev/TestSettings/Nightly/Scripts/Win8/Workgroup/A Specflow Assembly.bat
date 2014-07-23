@echo off
@echo Assembly Name eg: Dev2.Activities.Specs (no .dll extension):
set /P assembly=
cd %CD%\..\..\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
copy /Y "%CD%\TestSettings\Nightly\Win8\Workgroup\SpecFlow.testsettings" "%CD%\SpecFlow.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\%assembly%\bin\Debug\%assembly%.dll" /testSettings:"SpecFlow.testsettings"
DEL /F /Q "%CD%\SpecFlow.testsettings"
pause
FOR /D %%p IN ("%CD%\TestResults\*.*") DO rmdir "%%p" /s /q