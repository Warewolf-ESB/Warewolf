using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.PathOperations;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(CommonSteps.ActualSourceHolder),
                            ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                            ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                            true);
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            const string resourceName = "Dev2.Activities.Specs.Toolbox.FileAndFolder.Unzip.Test.zip";
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
