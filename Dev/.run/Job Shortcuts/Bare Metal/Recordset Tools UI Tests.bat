mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryCount 6 -RetryRebuild -Projects Warewolf.UI.Tests -Category "Recordset Tools" -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath UITests" -PostTestRunScript ReverseDeployLog.ps1