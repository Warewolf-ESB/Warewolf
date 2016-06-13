
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dev2.PathOperations;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Activities.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Unzip
{
    [Binding]
    public class UnzipSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public UnzipSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"zip credentials as ""(.*)"" and ""(.*)""")]
        public void GivenZipCredentialsAsAnd(string userName, string password)
        {
            scenarioContext.Add(CommonSteps.SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            scenarioContext.Add(CommonSteps.SourcePasswordHolder, password.Replace('"', ' ').Trim());
        }
        
        [When(@"the Unzip file tool is executed")]
        public void WhenTheUnzipFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [When(@"the Unzip file tool is executed with a single file")]
        public void WhenTheUnzipFileToolIsExecutedWithASingleFile()
        {
            scenarioContext.Add("singleFile",true );
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            CopyZipFileToSourceLocation();
            var unzip = new DsfUnZip
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder),
                DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                ArchivePassword = scenarioContext.Get<string>("archivePassword"),
                PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                DestinationPrivateKeyFile = scenarioContext.Get<string>(CommonSteps.DestinationPrivateKeyFile)
            };

            TestStartNode = new FlowStep
            {
                Action = unzip
            };

            scenarioContext.Add("activity", unzip);
        }

        void CopyZipFileToSourceLocation()
        {
            RunwithRetry(1);
        }

        void RunwithRetry(int retrycount)
        {
            if (retrycount == 0)
                return;
            const int TimeOut = 300000;

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

        void RunCopy()
        {
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(scenarioContext.Get<string>(CommonSteps.ActualSourceHolder),
                                                                            scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                                                                            scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                                                                            true);
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            
            string resourceName = "Warewolf.ToolsSpecs.Toolbox.FileAndFolder.Unzip.Test.zip";
             if (scenarioContext.ContainsKey("WhenTheUnzipFileToolIsExecutedWithASingleFile"))
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
