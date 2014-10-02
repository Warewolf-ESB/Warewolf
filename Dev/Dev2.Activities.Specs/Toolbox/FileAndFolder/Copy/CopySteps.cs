
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Statements;
using System.IO;
using Dev2.Activities.Specs.BaseTypes;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Copy
{
    [Binding]
    public class CopySteps : FileToolsBase
    {
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
                File.WriteAllText("c:\\copydir\bob.txt", "bob");

                File.WriteAllText("c:\\copydir\\1\\bob.txt", "dora");
                File.WriteAllText("c:\\copydir\\6\\bob.txt", "moon");
                File.WriteAllText("c:\\copydir\\33\\bob.txt", "dave");
                // ReSharper restore LocalizableElement
            }
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();


            var create = new DsfPathCopy();
            create.InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder);
            create.Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder);
            create.Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder);
            create.OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder);
            create.DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder);
            create.DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder);
            create.Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder);
            create.Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder);

            TestStartNode = new FlowStep
            {
                Action = create
            };

            ScenarioContext.Current.Add("activity", create);
        }
    }
}
