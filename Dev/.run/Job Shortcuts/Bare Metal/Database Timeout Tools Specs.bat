mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Warewolf.Tools.Specs -Category DatabaseTimeout -PreTestRunScript "StartAs.ps1 -Cleanup -Anonymous -ResourcesPath ServerTests" -PostTestRunScript 'ReverseDeployLog.ps1'