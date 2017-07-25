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
using System.IO;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Copy
{
    [Binding]
    public class CopySteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public CopySteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [When(@"the copy file tool is executed")]
        public void WhenTheCopyFileToolIsExecuted()
        {
            if(!Directory.Exists("c:\\copydir"))
            {
                Directory.CreateDirectory("c:\\copydir");
                Directory.CreateDirectory("c:\\copydir\\1");
                Directory.CreateDirectory("c:\\copydir\\6");
                Directory.CreateDirectory("c:\\copydir\\33");
                // ReSharper disable LocalizableElement
                File.WriteAllText("c:\\copydir\\bob.txt", "bob");

                File.WriteAllText("c:\\copydir\\1\\bob.txt", "dora");
                File.WriteAllText("c:\\copydir\\6\\bob.txt", "moon");
                File.WriteAllText("c:\\copydir\\33\\bob.txt", "dave");
                // ReSharper restore LocalizableElement
            }
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var copy = new DsfPathCopy
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder), 
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder), 
                OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder), 
                DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder), 
                DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder), 
                Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder), 
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                DestinationPrivateKeyFile = scenarioContext.Get<string>(CommonSteps.DestinationPrivateKeyFile)
            };
           
            TestStartNode = new FlowStep
            {
                Action = copy
            };

            scenarioContext.Add("activity", copy);
        }
    }
}
