@echo off
cd %CD%\..\..\Win2k8
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"..\..\..\..\TestBinaries\Dev2.IntegrationTests.dll" /testSettings:"Integration.testsettings"
pause