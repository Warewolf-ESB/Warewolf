"%~dp0nuget.exe" restore "%~dp0..\..\AcceptanceTesting.sln"
"%~dp0nuget.exe" restore "%~dp0..\..\UITesting.sln"

IF EXIST MSBuild (
MSBuild "%~dp0..\..\AcceptanceTesting.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
if not %errorlevel%==0 pause & exit 1
MSBuild "%~dp0..\..\UITesting.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
) else IF EXIST "%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" (
"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0..\..\AcceptanceTesting.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
if not %errorlevel%==0 pause & exit 1
"%programfiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" "%~dp0..\..\UITesting.sln" /p:Platform="Any CPU";Configuration="Debug" /maxcpucount
)

if not %errorlevel%==0 pause & exit 1