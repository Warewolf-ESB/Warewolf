@echo off
@echo Assembly Name eg: Dev2.Activities.Specs.dll:
set /P assembly=
cd %CD%\..\..\..\..\..
copy /Y "%CD%\TestSettings\Nightly\Win8\Domain\SpecFlow.testsettings" "%CD%\SpecFlow.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%assembly%" /testSettings:"SpecFlow.testsettings"
pause