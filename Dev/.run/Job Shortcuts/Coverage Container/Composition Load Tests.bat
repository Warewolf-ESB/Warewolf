mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -Category CompositionLoadTests -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath LoadTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage -InContainer