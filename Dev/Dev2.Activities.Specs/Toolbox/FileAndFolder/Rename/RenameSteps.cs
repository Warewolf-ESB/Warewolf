using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Rename
{
    [Binding]
    public class RenameSteps : FileToolsBase
    {
        [When(@"the rename file tool is executed")]
        public void WhenTheRenameFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }


        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var rename = new DsfPathRename
                {
                    InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                    Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                    Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                    OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder),
                    DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                    DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                    Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder),
                    Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder)
                };

            TestStartNode = new FlowStep
            {
                Action = rename
            };
            ScenarioContext.Current.Add("activity", rename);
        }
    }
}
