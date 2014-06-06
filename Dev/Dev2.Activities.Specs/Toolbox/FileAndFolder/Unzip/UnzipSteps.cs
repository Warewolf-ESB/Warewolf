using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.PathOperations;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Threading.Tasks;
namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Unzip
{
    [Binding]
    public class UnzipSteps : FileToolsBase
    {
        
        [Given(@"zip credentials as '(.*)' and '(.*)'")]
        public void GivenZipCredentialsAsAnd(string userName, string password)
        {
            ScenarioContext.Current.Add(CommonSteps.SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add(CommonSteps.SourcePasswordHolder, password.Replace('"', ' ').Trim());
        }

        //
        [When(@"the Unzip file tool is executed")]
        public void WhenTheUnzipFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [When(@"the Unzip file tool is executed with a single file")]
        public void WhenTheUnzipFileToolIsExecutedWithASingleFile()
        {
            ScenarioContext.Current.Add("singleFile",true );
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            CopyZipFileToSourceLocation();
            var unzip = new DsfUnZip
            {
                InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder),
                DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder),
                ArchivePassword = ScenarioContext.Current.Get<string>("archivePassword")
            };

            TestStartNode = new FlowStep
            {
                Action = unzip
            };

            ScenarioContext.Current.Add("activity", unzip);
        }

        void CopyZipFileToSourceLocation()
        {
            RunwithRetry(1);
        }

        static void RunwithRetry(int retrycount)
        {
            if (retrycount == 0)
                return;
            const int TimeOut = 15000;

            var cancel = new CancellationTokenSource();
            Task a = new Task(RunCopy, cancel.Token);
            a.Start();
            a.Wait(TimeOut);

            if(a.Status == TaskStatus.Running)
            {
                cancel.Cancel();
                a.Dispose();
                RunwithRetry(retrycount - 1);
            }
            else if(a.Status == TaskStatus.Faulted)
            {
                if(a.Exception != null)
                {
                    throw a.Exception;
                }
            }
        }

        static void RunCopy()
        {
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(CommonSteps.ActualSourceHolder),
                                                                            ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                                                                            ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                                                                            true);
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            
             string resourceName = "Dev2.Activities.Specs.Toolbox.FileAndFolder.Unzip.Test.zip";
             if (ScenarioContext.Current.ContainsKey("WhenTheUnzipFileToolIsExecutedWithASingleFile"))
                 resourceName = "TestFile.zip";
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> filesToCleanup = new List<string>();
            using(Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream != null)
                {
                    sourceEndPoint.Put(stream, sourceEndPoint.IOPath, new Dev2CRUDOperationTO(true), null, filesToCleanup);
                }
            }
            filesToCleanup.ForEach(File.Delete);
        }
    }
}
