mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -Projects Dev2.*.Specs,Warewolf.*.Specs -ExcludeProjects Dev2.Activities.Specs,Warewolf.Tools.Specs,Warewolf.UI.Specs,Warewolf.UI.Load.Specs -Category ResourcePermissionsSecurity -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -InContainer