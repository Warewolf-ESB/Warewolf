
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
using System.Activities.Statements;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2;
using Dev2.Activities.Specs.BaseTypes;

// ReSharper disable CheckNamespace
namespace Warewolf.ToolsSpecs.Toolbox.FileAndFolder.Write_File
// ReSharper restore CheckNamespace
{
    [Binding]
    public class WriteFileSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public WriteFileSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"Method is ""(.*)""")]
        public void GivenMethodIs(string method)
        {
            scenarioContext.Add("method", method);
        }

        [Given(@"input contents as ""(.*)""")]
        public void GivenInputContentsAs(string content)
        {
            scenarioContext.Add("content", content);
        }

        [When(@"the write file tool is executed")]
        public void WhenTheWriteFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"the input contents from a file ""(.*)""")]
        public void GivenTheInputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                                fileName);
            var content = ReadFile(resourceName);
            scenarioContext.Add("content", content);
        }

        [Then(@"the output contents from a file ""(.*)""")]
        public void ThenTheOutputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                              fileName);
            var expectedContents = ReadFile(resourceName);

            var broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(scenarioContext.Get<string>(CommonSteps.ActualSourceHolder),
                            scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                            scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                            true);

            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            var fileContents = broker.Get(sourceEndPoint);

            bool does = fileContents.Contains(expectedContents.Replace("\n","\r\n"));
            Assert.IsTrue(does);
        }


        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var readtype = scenarioContext.Get<string>("method");
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
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                OutputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Overwrite = overwrite,
                AppendTop = appendTop,
                AppendBottom = appendBottom,
                FileContents = scenarioContext.Get<string>("content"),
                PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile)
            };

            TestStartNode = new FlowStep
            {
                Action = fileWrite
            };

            scenarioContext.Add("activity", fileWrite);
        }
    }
}
