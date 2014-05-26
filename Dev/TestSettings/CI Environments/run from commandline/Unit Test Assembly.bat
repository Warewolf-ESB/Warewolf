@echo off
@echo Assembly Name eg: Dev2.Infrustructer.Tests.dll:
set /P assembly=
cd %CD%\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"..\..\TestBinaries\%assembly%" /testSettings:"UnitTestsWithCoverage.testsettings"
pause