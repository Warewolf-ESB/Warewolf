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
using Dev2.Activities.Designers2.ReadFolder;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Read_Folder
{
    [Binding]
    public class ReadFolderSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public ReadFolderSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"Read is ""(.*)""")]
        public void GivenReadIs(string readType)
        {
            scenarioContext.Add("readType", readType);
        }
        
        [When(@"the read folder file tool is executed")]
        public void WhenTheReadFolderFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var readtype = scenarioContext.Get<string>("readType");
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

            var x = scenarioContext.Get<string>(CommonSteps.SourceHolder);

            var folderRead = new DsfFolderRead
                {
                    InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                    Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                    Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                    Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                    IsFilesAndFoldersSelected = isFileOrFolder,
                    IsFoldersSelected = isFolderSelected,
                    IsFilesSelected = isFileSelected,
                    PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile)
                };

            TestStartNode = new FlowStep
            {
                Action = folderRead
            };

            scenarioContext.Add("activity", folderRead);
            var viewModel = new ReadFolderDesignerViewModel(ModelItemUtils.CreateModelItem(folderRead));
            if (!scenarioContext.ContainsKey("viewModel"))
                scenarioContext.Add("viewModel", viewModel);
        }
    }
}
