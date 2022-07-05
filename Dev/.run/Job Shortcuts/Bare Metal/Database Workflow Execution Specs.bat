mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting\net48"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting\net48"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.Activities.Specs -Category DatabaseWorkflowExecution -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1