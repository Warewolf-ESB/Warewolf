@echo off
cd %CD%\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
IF NOT %9 == "" GOTO RunTestsWith9Args
IF NOT %8 == "" GOTO RunTestsWith8Args
IF NOT %7 == "" GOTO RunTestsWith7Arg
IF NOT %6 == "" GOTO RunTestsWith6Args
IF NOT %5 == "" GOTO RunTestsWith5Args
IF NOT %4 == "" GOTO RunTestsWith4Arg
IF NOT %3 == "" GOTO RunTestsWith3Args
IF NOT %2 == "" GOTO RunTestsWith2Args
IF NOT %1 == "" GOTO RunTestsWith1Arg

@echo Assembly Name eg: Dev2.Infrustructer.Tests.dll:
set /P assembly=
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%assembly%" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit

:RunTestsWith1Arg
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith2Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith3Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith4Arg
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith5Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%5" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith6Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%5" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%6" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith7Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%5" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%6" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%7" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith8Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%5" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%6" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%7" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%8" /testSettings:"UnitTestsWithCoverage.testsettings"
GOTO exit
:RunTestsWith9Args
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%1" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%2" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%3" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%4" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%5" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%6" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%7" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%8" /testcontainer:"%CD%\Dev2.Server\bin\Debug\%9" /testSettings:"UnitTestsWithCoverage.testsettings"

:exit
DEL /F /Q "%CD%\UnitTestsWithCoverage.testsettings"
pause