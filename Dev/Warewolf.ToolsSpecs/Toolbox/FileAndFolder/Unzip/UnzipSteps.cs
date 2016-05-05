
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
                ArchivePassword = ScenarioContext.Current.Get<string>("archivePassword"),
                PrivateKeyFile = ScenarioContext.Current.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                DestinationPrivateKeyFile = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPrivateKeyFile)
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

        static void RunCopy()
        {
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(CommonSteps.ActualSourceHolder),
                                                                            ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                                                                            ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                                                                            true);
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            
            string resourceName = "Warewolf.ToolsSpecs.Toolbox.FileAndFolder.Unzip.Test.zip";
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
