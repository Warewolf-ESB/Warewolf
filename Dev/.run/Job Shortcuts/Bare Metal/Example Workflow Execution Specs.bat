mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -Category ExampleWorkflowExecution -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath Release" -PostTestRunScript ReverseDeployLog.ps1