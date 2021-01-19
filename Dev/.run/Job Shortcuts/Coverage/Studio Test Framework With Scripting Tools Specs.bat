mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -Category StudioTestFrameworkWithScriptingTools -PreTestRunScript "StartAs.ps1 -Cleanup -Anonymous -ResourcesPath ServerTests" -Coverage