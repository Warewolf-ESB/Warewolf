@echo off
cd %CD%\..\..\..\Win8\Domain
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"..\..\..\..\..\TestBinaries\Dev2.LoadTest.dll" /testSettings:"Load.testsettings"
pause