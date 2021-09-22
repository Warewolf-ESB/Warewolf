mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -Category StudioTestFrameworkWithRabbitMQTools -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -Coverage