using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Move
{
    [Binding]
    public class MoveSteps : FileToolsBase
    {

        [When(@"the Move file tool is executed")]
        public void WhenTheMoveFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var move = new DsfPathMove();
            move.InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder);
            move.Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder);
            move.Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder);
            move.OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder);
            move.DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder);
            move.DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder);
            move.Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder);
            move.Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder);

            TestStartNode = new FlowStep
            {
                Action = move
            };
            ScenarioContext.Current.Add("activity", move);
        }
    }
}
