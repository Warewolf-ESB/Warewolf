mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.Integration.Tests -Category "Load Tests" -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath LoadTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage