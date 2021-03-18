mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Warewolf.UI.Tests -Category Scalar Search -PreTestRunScript "StartAsService.ps1 -Cleanup -Anonymous -ResourcesPath UITests" -PostTestRunScript "ReverseDeployLog.ps1"