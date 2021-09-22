mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryCount 6 -RetryRebuild -Projects Dev2.Integration.Tests -Category "Load Tests" -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath LoadTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage -InContainer