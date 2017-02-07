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
using System.Activities.Statements;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Zip
{
    [Binding]
    public class ZipSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public ZipSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"Archive Password as ""(.*)""")]
        public void GivenArchivePasswordAs(string archivePassword)
        {
            scenarioContext.Add("archivePassword", archivePassword);
        }

        [Given(@"the Compression as ""(.*)""")]
        public void GivenTheCompressionAs(string compressio)
        {
            scenarioContext.Add("compressio", compressio);
        }

        [When(@"the Zip file tool is executed")]
        public void WhenTheZipFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected override void BuildDataList()
        {
            try
            {
                BuildShapeAndTestData();

                var zip = new DsfZip
                {
                    InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                    Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                    Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                    OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder),
                    DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                    DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                    Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder),
                    Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                    ArchivePassword = scenarioContext.Get<string>("archivePassword"),
                    CompressionRatio = scenarioContext.Get<string>("compressio"),
                    PrivateKeyFile = scenarioContext.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                    DestinationPrivateKeyFile = scenarioContext.Get<string>(CommonSteps.DestinationPrivateKeyFile)
                };

                TestStartNode = new FlowStep
                {
                    Action = zip
                };
                // CI
                scenarioContext.Add("activity", zip);
            }
            catch(Exception)
            {
                //
            }
        }
    }
}
