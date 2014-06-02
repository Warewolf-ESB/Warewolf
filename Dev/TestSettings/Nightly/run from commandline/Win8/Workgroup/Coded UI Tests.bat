@echo off
cd %CD%\..\..\..\..\..
copy /Y "%CD%\TestSettings\Nightly\Win8\Workgroup\UI.testsettings" "%CD%\UI.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\Dev2.Studio.UITests.dll" /testSettings:"UI.testsettings"
pause