using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Zip
{
    [Binding]
    public class ZipSteps : FileToolsBase
    {
        [Given(@"Archive Password as '(.*)'")]
        public void GivenArchivePasswordAs(string archivePassword)
        {
            ScenarioContext.Current.Add("archivePassword", archivePassword);
        }

        [Given(@"the Compression as '(.*)'")]
        public void GivenTheCompressionAs(string compressio)
        {
            ScenarioContext.Current.Add("compressio", compressio);
        }

        [When(@"the Zip file tool is executed")]
        public void WhenTheZipFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var zip = new DsfZip
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
               CompressionRatio = ScenarioContext.Current.Get<string>("compressio"),
            };

            TestStartNode = new FlowStep
            {
                Action = zip
            };
            // CI
            ScenarioContext.Current.Add("activity", zip);
        }
    }
}
