mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -Category ExampleWorkflowExecution -PreTestRunScript 'StartAs.ps1 -Cleanup -Anonymous -ResourcesPath Release' -PostTestRunScript 'ReverseDeployLog.ps1'