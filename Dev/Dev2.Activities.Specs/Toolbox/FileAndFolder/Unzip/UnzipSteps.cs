using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Unzip
{
    [Binding]
    public class UnzipSteps : FileToolsBase
    {
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
    }
}
