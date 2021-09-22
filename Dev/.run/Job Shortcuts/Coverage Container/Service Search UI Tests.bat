mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Warewolf.UI.Tests -Category "Service Search" -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath UITests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage -InContainer