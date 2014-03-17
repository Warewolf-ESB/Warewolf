using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Delete
{
    [Binding]
    public class DeleteSteps : FileToolsBase
    {
        [When(@"the delete file tool is executed")]
        public void WhenTheDeleteFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var delete = new DsfPathDelete
            {
                InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder)
            };

            TestStartNode = new FlowStep
            {
                Action = delete
            };

            ScenarioContext.Current.Add("activity", delete);
        }
    }
}
