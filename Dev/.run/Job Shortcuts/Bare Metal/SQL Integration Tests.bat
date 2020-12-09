powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\..\..\..\Compile.ps1" -AcceptanceTesting
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -Projects Dev2.Integration.Tests,Warewolf.Tools.Specs -Category SqlDatabaseBroker -PreTestRunScript "StartAs.ps1 -Cleanup -Anonymous -ResourcesPath ServerTests"