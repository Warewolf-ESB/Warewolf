using Dev2.Activities.Sharepoint;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Sharepoint
{
    [Binding]
    public class SharePointCopyFileSteps : FileToolsBase
    {
        public SharepointSource GetSource()
        {
            var source = new SharepointSource()
            {
                UserName = ScenarioContext.Current.Get<string>("UserName"),
                Password = ScenarioContext.Current.Get<string>("Password"),
                ResourceID = Guid.NewGuid(),
                Server = ScenarioContext.Current.Get<string>("Server"),
                IsSharepointOnline = true,
                AuthenticationType = AuthenticationType.User
            };

            ScenarioContext.Current.Add("SharepointSource", source);

            return source;
        }

        [Given(@"I have a Server file path '(.*)'")]
        public void GivenIHaveAServerFilePath(string path)
        {
            ScenarioContext.Current.Add("PathTo", path);
        }

        [When(@"the sharepoint copy file tool is executed")]
        public void WhenTheSharepointCopyFileToolIsExecuted()
        {
            var fileCopy = new SharepointCopyFileActivity()
            {
                ServerInputPathFrom = ScenarioContext.Current.Get<string>("InputPath"),
                ServerInputPathTo = ScenarioContext.Current.Get<string>("PathTo"),
                Overwrite = true,
                Result = "[[Result]]",
                SharepointSource = GetSource()
            };

            TestStartNode = new FlowStep
            {
                Action = fileCopy
            };

            ScenarioContext.Current.Add("activity", fileCopy);

            IDSFDataObject result = ExecuteProcess(isDebug: true);
            ScenarioContext.Current.Add("result", result);

            Assert.IsNotNull(fileCopy);
        }

        [Then(@"the result will be '(.*)'")]
        public void ThenTheResultWillBe(string p0)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            Assert.IsTrue(result.Environment.AllErrors.Count > 0);
        }
    }
}