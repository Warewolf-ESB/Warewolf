powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\..\..\..\Compile.ps1" -AcceptanceTesting
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -Projects Dev2.Activities.Specs -Category ExampleWorkflowExecution -PreTestRunScript "StartAs.ps1 -Cleanup -Anonymous -ResourcesPath Release" -RunInDocker