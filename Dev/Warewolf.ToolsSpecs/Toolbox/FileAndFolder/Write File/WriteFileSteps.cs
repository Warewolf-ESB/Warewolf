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
using System.Activities.Statements;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Activities.Designers2.WriteFile;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;

// ReSharper disable CheckNamespace
namespace Warewolf.ToolsSpecs.Toolbox.FileAndFolder.Write_File
// ReSharper restore CheckNamespace
{
    [Binding]
    public class WriteFileSteps : FileToolsBase
    {
        private readonly ScenarioContext _scenarioContext;

        public WriteFileSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            this._scenarioContext = scenarioContext;
        }

        [Given(@"Method is ""(.*)""")]
        public void GivenMethodIs(string method)
        {
            _scenarioContext.Add("method", method);
        }

        [Given(@"input contents as ""(.*)""")]
        public void GivenInputContentsAs(string content)
        {
            _scenarioContext.Add("content", content);
        }

        [When(@"the write file tool is executed")]
        public void WhenTheWriteFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            _scenarioContext.Add("result", result);
        }

        [Given(@"the input contents from a file ""(.*)""")]
        public void GivenTheInputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                                fileName);
            var content = ReadFile(resourceName);
            _scenarioContext.Add("content", content);
        }

        [Then(@"the output contents from a file ""(.*)""")]
        public void ThenTheOutputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                              fileName);
            var expectedContents = ReadFile(resourceName);

            var broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(_scenarioContext.Get<string>(CommonSteps.ActualSourceHolder),
                            _scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                            _scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                            true);

            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            var fileContents = broker.Get(sourceEndPoint);

            bool does = fileContents.Contains(expectedContents.Replace("\n","\r\n"));
            Assert.IsTrue(does);
        }


        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var readtype = _scenarioContext.Get<string>("method");
            var overwrite = false;
            var appendTop = false;
            var appendBottom = false;

            switch(readtype)
            {
                case "Overwrite":
                    overwrite = true;
                    break;
                case "Append Top":
                    appendTop = true;
                    break;
                case "Append Bottom":
                    appendBottom = true;
                    break;
            }

            var fileWrite = new DsfFileWrite
            {                                
                Username = _scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = _scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = _scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                OutputPath = _scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Overwrite = overwrite,
                AppendTop = appendTop,
                AppendBottom = appendBottom,
                FileContents = _scenarioContext.Get<string>("content"),
                PrivateKeyFile = _scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile)
            };

            TestStartNode = new FlowStep
            {
                Action = fileWrite
            };

            _scenarioContext.Add("activity", fileWrite);

            var viewModel = new WriteFileDesignerViewModel(ModelItemUtils.CreateModelItem(fileWrite));
            if (!_scenarioContext.ContainsKey("viewModel"))
                _scenarioContext.Add("viewModel", viewModel);
        }
    }
}
