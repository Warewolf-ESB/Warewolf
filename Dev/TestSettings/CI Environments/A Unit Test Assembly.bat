@echo off
@echo Assembly Name eg: Dev2.Infrustructer.Tests.dll:
set /P assembly=
cd %CD%\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Test
copy /Y "%CD%\TestSettings\CI Environments\UnitTestsWithCoverage.testsettings" "%CD%\UnitTestsWithCoverage.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%assembly%" /testSettings:"UnitTestsWithCoverage.testsettings"
DEL /F /Q "%CD%\UnitTestsWithCoverage.testsettings"
pause