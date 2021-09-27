mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryCount 6 -RetryRebuild -Projects Warewolf.UI.Tests -ExcludeCategories Scheduler,"Merge Foreach",MSSql,"Plugin Sources","Recordset Search",Search,"Shortcut Keys","Workflow Testing","Recordset Tools","Debug Input","Merge Simple Tools Conflicts","Scheduler Delete Task","Title Search","Service Search","Default Layout","Email Tools","File Tools","DotNet Connector Tool","Sharepoint Tools","Save Dialog","Source Wizards","DotNet Connector Mocking Tests","Merge Assign Conflicts","Assign Tool","Data Tools","Database Tools",Settings,"Test Name Search","Deploy from Remote","Input Search","Scalar Search","Deploy from Explorer","Control Flow Tools","Merge All Tools Conflicts","Server Sources",Variables,"Utility Tools","Deploy Filtering",Tools,"Merge Decision Conflicts","Deploy Hello World","Deploy Select Dependencies","Tabs and Panes","Object Search","Dependency Graph","Dropbox Tools","Database Sources","Merge Switch Conflicts","Hello World Mocking Tests","Scheduler Delete Is Disabled",Deploy,"Workflow Mocking Tests","Merge Variable Conflicts","Web Sources","Resource Tools","Output Search","HTTP Tools","Studio Shutdown","Merge Sequence Conflicts" -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath UITests" -PostTestRunScript ReverseDeployLog.ps1 -InContainer