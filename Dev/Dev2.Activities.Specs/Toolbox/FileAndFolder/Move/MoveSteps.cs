
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
