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
	echo MSBuild not found. Download from: https://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe
)

if not %errorlevel%==0 pause & exit 1