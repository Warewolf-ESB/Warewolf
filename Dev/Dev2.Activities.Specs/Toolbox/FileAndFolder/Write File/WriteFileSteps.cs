
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using Dev2.PathOperations;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File
// ReSharper restore CheckNamespace
{
    [Binding]
    public class WriteFileSteps : FileToolsBase
    {
        [Given(@"Method is '(.*)'")]
        public void GivenMethodIs(string method)
        {
            ScenarioContext.Current.Add("method", method);
        }

        [Given(@"input contents as '(.*)'")]
        public void GivenInputContentsAs(string content)
        {
            ScenarioContext.Current.Add("content", content);
        }

        [When(@"the write file tool is executed")]
        public void WhenTheWriteFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"the input contents from a file '(.*)'")]
        public void GivenTheInputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                                fileName);
            var content = ReadFile(resourceName);
            ScenarioContext.Current.Add("content", content);
        }

        [Then(@"the output contents from a file '(.*)'")]
        public void ThenTheOutputContentsFromAFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File.testfiles.{0}",
                                              fileName);
            var expectedContents = ReadFile(resourceName);

            var broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(CommonSteps.ActualSourceHolder),
                            ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                            ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                            true);

            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);

            var fileContents = broker.Get(sourceEndPoint);

            bool does = fileContents.Contains(expectedContents);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(does);
        }


        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var readtype = ScenarioContext.Current.Get<string>("method");
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
                Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder),
                OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                Overwrite = overwrite,
                AppendTop = appendTop,
                AppendBottom = appendBottom,
                FileContents = ScenarioContext.Current.Get<string>("content")
            };

            TestStartNode = new FlowStep
            {
                Action = fileWrite
            };

            ScenarioContext.Current.Add("activity", fileWrite);
        }
    }
}
