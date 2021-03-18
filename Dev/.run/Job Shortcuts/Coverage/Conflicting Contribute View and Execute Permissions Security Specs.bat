mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.*.Specs,Warewolf.*.Specs -ExcludeProjects Dev2.Activities.Specs,Warewolf.Tools.Specs,Warewolf.UI.Load.Specs,Warewolf.UI.Specs -Category ConflictingContributeViewExecutePermissionsSecurity -PreTestRunScript "StartAsService.ps1 -Cleanup -Anonymous -ResourcesPath ServerTests" -PostTestRunScript "ReverseDeployLog.ps1" -Coverage