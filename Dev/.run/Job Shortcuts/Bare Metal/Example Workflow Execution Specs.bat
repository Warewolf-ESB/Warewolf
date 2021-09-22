mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\TestRun.ps1" -RetryCount 6 -RetryRebuild -Projects Dev2.Activities.Specs -Categories ExampleWorkflowExecution,DateTimeExampleWorkflowExecution -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath Release" -PostTestRunScript ReverseDeployLog.ps1