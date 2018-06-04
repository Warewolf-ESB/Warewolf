/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities.Specs.BaseTypes;
using System.Activities.Statements;
using Dev2.Activities.Designers2.ReadFolderNew;
using Dev2.Studio.Core.Activities.Utils;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.ReadFolderNew
{
    [Binding]
    public class ReadFolderNewSteps : FileToolsBase
    {
        readonly ScenarioContext _scenarioContext;

        public ReadFolderNewSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this._scenarioContext = scenarioContext;
        }

        [When(@"the new read folder file tool is executed")]
        public void WhenTheNewReadFolderFileToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            _scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var readtype = _scenarioContext.Get<string>("readType");
            var isFileSelected = false;
            var isFolderSelected = false;
            var isFileOrFolder = false;

            switch (readtype)
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

            var x = _scenarioContext.Get<string>(CommonSteps.SourceHolder);

            var folderRead = new DsfFolderRead
            {
                InputPath = _scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = _scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = _scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = _scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                IsFilesAndFoldersSelected = isFileOrFolder,
                IsFoldersSelected = isFolderSelected,
                IsFilesSelected = isFileSelected,
                PrivateKeyFile = _scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile)
            };

            TestStartNode = new FlowStep
            {
                Action = folderRead
            };

            _scenarioContext.Add("activity", folderRead);
            var viewModel = new ReadFolderNewDesignerViewModel(ModelItemUtils.CreateModelItem(folderRead));
            if (!_scenarioContext.ContainsKey("viewModel"))
            {
                _scenarioContext.Add("viewModel", viewModel);
            }
        }
    }
}
