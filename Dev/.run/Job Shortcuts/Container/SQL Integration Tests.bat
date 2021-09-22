mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.Integration.Tests,Warewolf.Tools.Specs -Category SqlDatabaseBroker -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -InContainer