@echo off
cd %CD%\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Test
copy /Y "%CD%\TestSettings\Local\SpecFlow.testsettings" "%CD%\SpecFlow.testsettings"
IF NOT %3 == "" GOTO RunTestsWith3Args
IF NOT %2 == "" GOTO RunTestsWith2Args
IF NOT %1 == "" GOTO RunTestsWith1Arg

@echo Assembly Name eg: Dev2.Activities.Specs.dll:
set /P assembly=
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%assembly%" /testSettings:"SpecFlow.testsettings"
GOTO exit

:RunTestsWith1Arg
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%1" /testSettings:"SpecFlow.testsettings"
GOTO exit
:RunTestsWith2Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%1" /testcontainer:"%CD%\TestBinaries\%2" /testSettings:"SpecFlow.testsettings"
GOTO exit
:RunTestsWith3Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%1" /testcontainer:"%CD%\TestBinaries\%2" /testcontainer:"%CD%\TestBinaries\%3" /testSettings:"SpecFlow.testsettings"

:exit
DEL /F /Q "%CD%\SpecFlow.testsettings"