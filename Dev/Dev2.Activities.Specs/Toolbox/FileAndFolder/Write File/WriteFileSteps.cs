using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Write_File
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
