using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Copy
{
    [Binding]
    public class CopySteps : FileToolsBase
    {
        [When(@"the copy file tool is executed")]
        public void WhenTheCopyFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();


            var create = new DsfPathCopy();
            create.InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder);
            create.Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder);
            create.Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder);
            create.OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder);
            create.DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder);
            create.DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder);
            create.Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder);
            create.Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder);

            TestStartNode = new FlowStep
            {
                Action = create
            };

            ScenarioContext.Current.Add("activity", create);
        }
    }
}
