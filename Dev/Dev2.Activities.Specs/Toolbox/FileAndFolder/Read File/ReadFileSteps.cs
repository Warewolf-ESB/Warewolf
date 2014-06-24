using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System;
using System.IO;
using System.DirectoryServices.ActiveDirectory;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Read_File
{
    [Binding]
    public class ReadFileSteps : FileToolsBase
    {
        [When(@"the read file tool is executed")]
        public void WhenTheReadFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();


            var fileRead = new DsfFileRead
            {
                InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder).ResolveDomain(),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder)
            };

            TestStartNode = new FlowStep
            {
                Action = fileRead
            };

            ScenarioContext.Current.Add("activity", fileRead);
        }
    }
}
