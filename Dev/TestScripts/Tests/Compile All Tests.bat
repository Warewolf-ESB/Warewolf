"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
"%~dp0..\..\.nuget\nuget.exe" restore "%~dp0..\..\UITesting.sln"
if not %errorlevel%==0 pause & exit 1

IF EXIST "%vs120comntools%..\IDE\devenv.com" ( 
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\UITesting.sln" /Build Debug
if not %errorlevel%==0 pause & exit 1
"%vs120comntools%..\IDE\devenv.com" "%~dp0..\..\AcceptanceTesting.sln" /Build Debug
) else IF EXIST "%vs140comntools%..\IDE\devenv.com" (
"%vs140comntools%..\IDE\devenv.com" "%~dp0..\..\UITesting.sln" /Build Debug
if not %errorlevel%==0 pause & exit 1
"%vs140comntools%..\IDE\devenv.com" "%~dp0..\..\AcceptanceTesting.sln" /Build Debug
) else IF EXIST MSBuild (
MSBuild "%~dp0..\..\UITesting.sln" /p:Platform="Any CPU";Configuration="Debug"
if not %errorlevel%==0 pause & exit 1
MSBuild "%~dp0..\..\AcceptanceTesting.sln" /p:Platform="Any CPU";Configuration="Debug"
) else IF EXIST "C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe" (
"C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe" "%~dp0..\..\UITesting.sln" /p:Platform="Any CPU";Configuration="Debug"
if not %errorlevel%==0 pause & exit 1
"C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe" "%~dp0..\..\AcceptanceTesting.sln" /p:Platform="Any CPU";Configuration="Debug"
)

if not %errorlevel%==0 pause & exit 1