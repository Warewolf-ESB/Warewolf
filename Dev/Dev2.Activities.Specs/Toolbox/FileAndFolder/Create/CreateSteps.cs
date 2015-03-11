
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Create
{
    [Binding]
    public class CreateSteps : FileToolsBase
    {
        [When(@"the create file tool is executed")]
        public void WhenTheCreateFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var create = new DsfPathCreate
            {
                OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder),
                Username = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder).ResolveDomain(),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder)
            };

            TestStartNode = new FlowStep
            {
                Action = create
            };

            ScenarioContext.Current.Add("activity", create);
        }

        #endregion

        [BeforeScenario("fileFeature")]
        public static void SetupForTesting()
        {
            StartSftpServer();
        }

        [AfterScenario("fileFeature")]
        public void CleanUpFiles()
        {
            ShutdownSftpServer();
            RemovedFilesCreatedForTesting();
          
        }
    }
}
