powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0..\..\..\..\Compile.ps1" -AcceptanceTesting -InDockerContainer
mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -ExcludeCategories ExampleWorkflowExecution,WorkflowExecution,AssignWorkflowExecution,StudioTestFrameworkWithSubworkflow,RemoteServer,WorkflowMerging,StudioTestFrameworkWithDropboxTools,StudioTestFrameworkWithDeletedResources,MSSql,Scheduler,CompositionLoadTests,NestedForEachExecution,StudioTestFrameworkWithSharepointTools,StudioTestFrameworkWithUtilityTools,SubworkflowExecution,StudioTestFrameworkWithHTTPWebTools,StudioTestFrameworkWithFileAndFolderTools,StudioTestFramework,StudioTestFrameworkWithDataTools,StudioTestFrameworkWithDatabaseTools,StudioTestFrameworkWithHelloWorldWorkflow,ExecuteInBrowser,StudioTestFrameworkWithScriptingTools -PreTestRunScript "StartAs.ps1 -Cleanup -Anonymous -ResourcesPath ServerTests" -InContainer