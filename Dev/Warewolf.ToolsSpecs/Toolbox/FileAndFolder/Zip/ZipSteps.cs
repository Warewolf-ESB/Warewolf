
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
using Dev2.Common;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Zip
{
    [Binding]
    public class ZipSteps : FileToolsBase
    {
        [Given(@"Archive Password as '(.*)'")]
        public void GivenArchivePasswordAs(string archivePassword)
        {
            ScenarioContext.Current.Add("archivePassword", archivePassword);
        }

        [Given(@"the Compression as '(.*)'")]
        public void GivenTheCompressionAs(string compressio)
        {
            ScenarioContext.Current.Add("compressio", compressio);
        }

        [When(@"the Zip file tool is executed")]
        public void WhenTheZipFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        protected override void BuildDataList()
        {
            try
            {
                BuildShapeAndTestData();

                var zip = new DsfZip
                {
                    InputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                    Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                    Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                    OutputPath = ScenarioContext.Current.Get<string>(CommonSteps.DestinationHolder),
                    DestinationUsername = ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                    DestinationPassword = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                    Overwrite = ScenarioContext.Current.Get<bool>(CommonSteps.OverwriteHolder),
                    Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder),
                    ArchivePassword = ScenarioContext.Current.Get<string>("archivePassword"),
                    CompressionRatio = ScenarioContext.Current.Get<string>("compressio"),
                    PrivateKeyFile = ScenarioContext.Current.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                    DestinationPrivateKeyFile = ScenarioContext.Current.Get<string>(CommonSteps.DestinationPrivateKeyFile)
                };

                TestStartNode = new FlowStep
                {
                    Action = zip
                };
                // CI
                ScenarioContext.Current.Add("activity", zip);
            }
            catch(Exception e)
            {
                Dev2Logger.Debug("Error Setting Up Zip",e);
            }
        }
    }
}
