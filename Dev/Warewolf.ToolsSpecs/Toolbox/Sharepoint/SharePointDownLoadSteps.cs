using System;
using System.Activities.Statements;
using ActivityUnitTests;
using Dev2.Activities.Sharepoint;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Sharepoint
{
    [Binding]
    public class SharePointDownLoadSteps : BaseActivityUnitTest
    {
        public SharepointSource GetSource()
        {
            var source = new SharepointSource()
            {
                UserName = ScenarioContext.Current.Get<string>("UserName"),
                Password = ScenarioContext.Current.Get<string>("Password"),
                ResourceID = Guid.NewGuid(),
                Server = ScenarioContext.Current.Get<string>("ServerUrl"),
                IsSharepointOnline = true,
                AuthenticationType = AuthenticationType.User
            };

            ScenarioContext.Current.Add("SharepointSource", source);

            return source;
        }

        [Given(@"I have a Sharepoint Server download path with value '(.*)'")]
        public void GivenIHaveASharepointServerDownloadPathWithValue(string path)
        {
            ScenarioContext.Current.Add("ServerInputPath", path);
        }


        [When(@"the Sharepoint download file tool is executed")]
        public void WhenTheSharepointDownloadFileToolIsExecuted()
        {
            var fileUpload = new SharepointFileDownLoadActivity()
            {
                ServerInputPath = ScenarioContext.Current.Get<string>("ServerInputPath"),
                LocalInputPath = ScenarioContext.Current.Get<string>("LocalInputPath"),
                Result = "[[Result]]",
                SharepointSource = GetSource()
            };

            TestStartNode = new FlowStep
            {
                Action = fileUpload
            };

            ScenarioContext.Current.Add("activity", fileUpload);

            IDSFDataObject result = ExecuteProcess(isDebug: true);
            ScenarioContext.Current.Add("result", result);
        }
    }
}
