using System;
using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Read_Folder
{
    [Binding]
    public class ReadFolderSteps : FileToolsBase
    {
        [Given(@"Read is '(.*)'")]
        public void GivenReadIs(string readType)
        {
            ScenarioContext.Current.Add("readType", readType);
        }
        
        [When(@"the read folder file tool is executed")]
        public void WhenTheReadFolderFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var readtype = ScenarioContext.Current.Get<string>("readType");
            var isFileSelected = false;
            var isFolderSelected = false;
            var isFileOrFolder = false;

            switch(readtype)
            {
                case "Files":
                    isFileSelected = true;
                    break;
                case "Folders":
                    isFolderSelected = true;
                    break;
                default:
                    isFileOrFolder = true;
                    break;
            }

            var folderRead = new DsfFolderRead
                {
                    InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                    Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                    Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                    Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder),
                    IsFilesAndFoldersSelected = isFileOrFolder,
                    IsFoldersSelected = isFolderSelected,
                    IsFilesSelected = isFileSelected
                };

            TestStartNode = new FlowStep
            {
                Action = folderRead
            };

            ScenarioContext.Current.Add("activity", folderRead);
        }
    }
}
