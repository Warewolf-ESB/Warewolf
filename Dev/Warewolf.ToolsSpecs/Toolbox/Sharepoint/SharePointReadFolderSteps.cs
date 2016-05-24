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
    public class SharePointReadFolderSteps : FileToolsBase
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var readtype = ScenarioContext.Current.Get<string>("readType");
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

            var folderRead = new SharepointReadFolderItemActivity()
            {
                ServerInputPath = ScenarioContext.Current.Get<string>(CommonSteps.SourceHolder),
                Username = ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                Result = ScenarioContext.Current.Get<string>(CommonSteps.ResultVariableHolder),
                IsFilesAndFoldersSelected = isFileOrFolder,
                IsFoldersSelected = isFolderSelected,
                IsFilesSelected = isFileSelected,
                PrivateKeyFile = ScenarioContext.Current.Get<string>(CommonSteps.SourcePrivatePublicKeyFile),
                SharepointSource = new SharepointSource()
                {
                    UserName = "ttrtr",
                    Password = "wdasdasd",
                    AuthenticationType = AuthenticationType.User,
                    ResourceID = Guid.NewGuid(),
                }
            };

            TestStartNode = new FlowStep
            {
                Action = folderRead
            };

            ScenarioContext.Current.Add("activity", folderRead);
        }

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

        [When(@"the sharepoint read folder file tool is executed")]
        public void WhenTheSharepointReadFolderFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I have a sharepoint path with value '(.*)'")]
        public void GivenIHaveASharepointPathWithValue(string path)
        {
            ScenarioContext.Current.Add("InputPath", path);
        }

        [Given(@"source sharepoint credentials as '(.*)' and '(.*)'")]
        public void GivenSourceSharepointCredentialsAsAnd(string username, string password)
        {
            ScenarioContext.Current.Add("UserName", username);
            ScenarioContext.Current.Add("Password", password);
        }

        [Given(@"Read Type is '(.*)'")]
        public void GivenReadTypeIs(string readType)
        {
            ScenarioContext.Current.Add("ReadType", readType);
        }

        [Given(@"source sharepoint sharepoint Server is '(.*)'")]
        public void GivenSourceSharepointSharepointServerIs(string server)
        {
            ScenarioContext.Current.Add("Server", server);
        }

        [When(@"the sharepoint read file tool is executed")]
        public void WhenTheSharepointReadFileToolIsExecuted()
        {
            var readtype = ScenarioContext.Current.Get<string>("ReadType");

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

            var fileRead = new SharepointReadFolderItemActivity()
            {
                ServerInputPath = ScenarioContext.Current.Get<string>("InputPath"),
                Result = "[[Result]]",
                IsFilesAndFoldersSelected = isFileOrFolder,
                IsFoldersSelected = isFolderSelected,
                IsFilesSelected = isFileSelected,
                SharepointSource = GetSource()
            };

            TestStartNode = new FlowStep
            {
                Action = fileRead
            };

            ScenarioContext.Current.Add("activity", fileRead);

            IDSFDataObject result = ExecuteProcess(isDebug: true);
            ScenarioContext.Current.Add("result", result);

            Assert.IsNotNull(fileRead);
        }
    }
}