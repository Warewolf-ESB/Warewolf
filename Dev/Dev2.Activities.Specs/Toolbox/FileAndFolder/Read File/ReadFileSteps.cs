
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
