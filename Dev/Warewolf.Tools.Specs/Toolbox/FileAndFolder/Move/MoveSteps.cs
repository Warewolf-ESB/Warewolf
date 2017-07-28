/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Move
{
    [Binding]
    public class MoveSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public MoveSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [When(@"the Move file tool is executed")]
        public void WhenTheMoveFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile);
            var DestinationPrivateKeyFile = scenarioContext.Get<string>(CommonSteps.DestinationPrivateKeyFile);

            var move = new DsfPathMove
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder),
                DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                DestinationPrivateKeyFile = scenarioContext.Get<string>(CommonSteps.DestinationPrivateKeyFile)
            };

            TestStartNode = new FlowStep
            {
                Action = move
            };
            if (!scenarioContext.ContainsKey("activity")) {
                scenarioContext.Add("activity", move);
            }
        }
    }
}
