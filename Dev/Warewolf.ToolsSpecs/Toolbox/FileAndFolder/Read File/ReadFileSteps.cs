
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Read_File
{
    [Binding]
    public class ReadFileSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public ReadFileSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [When(@"the read file tool is executed")]
        public void WhenTheReadFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            string privateKeyFile;
            scenarioContext.TryGetValue(CommonSteps.SourcePrivatePublicKeyFile,out privateKeyFile);
            var fileRead = new DsfFileRead
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder).ResolveDomain(),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                PrivateKeyFile = privateKeyFile
            };

            TestStartNode = new FlowStep
            {
                Action = fileRead
            };

            scenarioContext.Add("activity", fileRead);
        }
    }
}
