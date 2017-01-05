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
) else (
set errorlevel=1
echo MSBuild not found. Download from: https://download.microsoft.com/download/9/B/B/9BB1309E-1A8F-4A47-A6C5-ECF76672A3B3/BuildTools_Full.exe
)

if not %errorlevel%==0 pause & exit 1