@echo off
@echo Assembly Name eg: Dev2.Infrastructure.Tests.dll:
set /P assembly=
cd %CD%\..\..\..\Win8\Workgroup
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"..\..\..\..\..\TestBinaries\%assembly%" /testSettings:"UnitTestWithCoverage.testsettings"
pause