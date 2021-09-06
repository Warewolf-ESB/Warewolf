mkdir "%~dp0..\..\..\..\bin\AcceptanceTesting"
cd /d "%~dp0..\..\..\..\bin\AcceptanceTesting"
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -NoExit -File "%~dp0..\TestRun.ps1" -RetryRebuild -Projects Dev2.Activities.Specs -ExcludeCategories ExampleWorkflowExecution,DateTimeExampleWorkflowExecution,RemoteServer,StudioTestFrameworkWithScriptingTools,StudioTestFrameworkWithHelloWorldWorkflow,StudioTestFrameworkWithHTTPWebPostTools,StudioTestFrameworkWithHTTPWebGetTools,StudioTestFrameworkWithHTTPWebDeleteTools,MSSql,DatabaseWorkflowExecution,ExecuteInBrowser,StudioTestFrameworkWithUtilityTools,StudioTestFrameworkWithSharepointTools,SubworkflowOracleExecution,RabbitMQWorkflowExecution,WorkflowMerging,StudioTestFrameworkWithFileAndFolderTools,DatabaseSubworkflowExecution,StudioTestFrameworkWithDatabaseTools,StudioTestFrameworkWithSubworkflow,Scheduler,StudioTestFrameworkWithHTTPWebPutTools,StudioTestFrameworkWithDropboxTools,SubworkflowExecution,StudioTestFrameworkWithDataTools,StudioTestFrameworkWithDeletedResources,StudioTestFrameworkWithRabbitMQTools,WorkflowExecution,StudioTestFramework,NestedForEachExecution,WarewolfSearch,AssignWorkflowExecution,WorkflowExecutionLogging,StudioTestFrameworkWithHTTPWebTools,CompositionLoadTests -PreTestRunScript "StartAsService.ps1 -Cleanup -ResourcesPath ServerTests" -PostTestRunScript ReverseDeployLog.ps1 -InContainer