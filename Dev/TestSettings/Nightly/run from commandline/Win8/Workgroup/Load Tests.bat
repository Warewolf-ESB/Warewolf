@echo off
cd %CD%\..\..\..\..\..
copy /Y "%CD%\TestSettings\Nightly\Win8\Workgroup\Load.testsettings" "%CD%\Load.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\Dev2.LoadTest.dll" /testSettings:"Load.testsettings"
pause