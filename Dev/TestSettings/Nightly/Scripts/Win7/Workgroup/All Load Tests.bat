@echo off
cd %CD%\..\..\..\..\..
"C:\Program Files (Workgroup)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
copy /Y "%CD%\TestSettings\Nightly\Win7\Workgroup\Load.testsettings" "%CD%\Load.testsettings"
"C:\Program Files (Workgroup)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Load.Tests\bin\Debug\Dev2.LoadTest.dll" /testSettings:"Load.testsettings"
DEL /F /Q "%CD%\Load.testsettings"
pause
FOR /D %%p IN ("%CD%\TestResults\*.*") DO rmdir "%%p" /s /q