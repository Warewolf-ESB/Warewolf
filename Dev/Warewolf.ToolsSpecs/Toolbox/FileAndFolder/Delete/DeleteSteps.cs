/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Delete;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Activities.Statements;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Delete
{
    [Binding]
    public class DeleteSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public DeleteSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [When(@"the delete file tool is executed")]
        public void WhenTheDeleteFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            if (!scenarioContext.ContainsKey("activity"))
            {
                BuildShapeAndTestData();

                string privateKeyFile;
                scenarioContext.TryGetValue(CommonSteps.SourcePrivatePublicKeyFile, out privateKeyFile);
                var delete = new DsfPathDelete
                {
                    InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                    Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder).ResolveDomain(),
                    Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                    Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                    PrivateKeyFile = privateKeyFile
                };

                TestStartNode = new FlowStep
                {
                    Action = delete
                };

                scenarioContext.Add("activity", delete);
            }
        }

        [When(@"validating the delete tool")]
        public void WhenValidatingTheDeleteTool()
        {
            BuildDataList();
            var delete = scenarioContext.Get<DsfPathDelete>("activity");
            delete.PerformValidation();

            var viewModel = new DeleteDesignerViewModel(ModelItemUtils.CreateModelItem(delete));
            if (!scenarioContext.ContainsKey("viewModel"))
                scenarioContext.Add("viewModel", viewModel);
            viewModel.Validate();
        }
    }
}