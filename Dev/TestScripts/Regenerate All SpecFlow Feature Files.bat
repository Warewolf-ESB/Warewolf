"%~dp0..\.nuget\nuget.exe" restore "%~dp0..\AcceptanceTesting.sln"
if not %errorlevel%==0 pause & exit 1
"%~dp0..\packages\SpecFlow.1.9.0\tools\specflow.exe" generateAll "%~dp0..\Dev2.Explorer.Specs\Dev2.Explorer.Specs.csproj" /force /verbose
pause