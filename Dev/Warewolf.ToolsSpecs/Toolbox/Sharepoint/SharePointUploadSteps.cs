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
    public class SharePointUploadSteps : BaseActivityUnitTest
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

        [Given(@"I have a Sharepoint Server path with value '(.*)'")]
        public void GivenIHaveASharepointServerPathWithValue(string path)
        {
            ScenarioContext.Current.Add("ServerInputPath", path);
        }

        [Given(@"I have a local file url with value '(.*)'")]
        public void GivenIHaveALocalFileUrlWithValue(string localPath)
        {
            ScenarioContext.Current.Add("LocalInputPath", localPath);
        }


        [Given(@"source Sharepoint server credentials as '(.*)' and '(.*)'")]
        public void GivenSourceSharepointServerCredentialsAsAnd(string username, string password)
        {
            ScenarioContext.Current.Add("UserName", username);
            ScenarioContext.Current.Add("Password", password);
        }

        [Given(@"source Sharepoint sharepoint Server url is '(.*)'")]
        public void GivenSourceSharepointSharepointServerUrlIs(string url)
        {
            ScenarioContext.Current.Add("ServerUrl", url);
        }

        [When(@"the Sharepoint upload file tool is executed")]
        public void WhenTheSharepointUploadFileToolIsExecuted()
        {
            var fileUpload = new SharepointFileUploadActivity()
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
