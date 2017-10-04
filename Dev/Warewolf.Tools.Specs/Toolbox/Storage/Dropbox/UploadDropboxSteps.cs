using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Upload;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Studio.Core.Messages;
using Dev2.Common.Interfaces.Data;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class UploadDropboxSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public UploadDropboxSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        [Given(@"I drag Upload Dropbox Tool onto the design surface")]
        public void GivenIDragWriteDropboxToolOntoTheDesignSurface()
        {
            var dropBoxUploadTool = new DsfDropBoxUploadActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dropBoxUploadTool);
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockResourcRepositorySetUp = new Mock<IResourceRepository>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var dropBoxSourceManager = new Mock<IDropboxSourceManager>();
            var sources = new List<OauthSource>()
            {
                new DropBoxSource(){ResourceName = "Test Resource Name"}
            };
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockResourcRepositorySetUp.Setup(repository => repository.FindSourcesByType<OauthSource>(mockEnvironmentModel.Object,It.IsAny<enSourceType>()))
                .Returns(sources);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourcRepositorySetUp.Object);

            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);
            var mock = new Mock<IResourceCatalog>();
            mock.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(new List<IResource>());
            var uploadViewModel = new DropBoxUploadViewModel(modelItem, dropBoxSourceManager.Object);
            scenarioContext.Add("uploadViewModel", uploadViewModel);
            scenarioContext.Add("mockEnvironmentModel", mockEnvironmentModel);
            scenarioContext.Add("mockEventAggregator", mockEventAggregator);
        }

        DropBoxUploadViewModel GetViewModel()
        {
            return scenarioContext.Get<DropBoxUploadViewModel>("uploadViewModel");
        }

        Mock<IServer> GeEnvrionmentModel()
        {
            return scenarioContext.Get<Mock<IServer>>("mockEnvironmentModel");
        }

        [Given(@"New is Enabled")]
        [When(@"New is Enabled")]
        [Then(@"New is Enabled")]
        public void GivenNewIsEnabled()
        {
            var canExecute = GetViewModel().NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [When(@"the Dropbox tool is executed")]
        public void WhenTheDropboxToolIsExecuted()
        {
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"Edit is Enabled")]
        public void GivenEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }
        [Given(@"Edit is Disabled")]
        public void GivenEditIsDisabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Given(@"Local File is Enabled")]
        public void GivenLocalFileIsEnabled()
        {
            var fromPath = GetViewModel().FromPath;
        }

        [Then(@"I Click Edit")]
        public void ThenIClickEdit()
        {
            GetViewModel().EditDropboxSourceCommand.CanExecute(null);
        }


        [Given(@"Dropbox File is Enabled")]
        public void GivenDropboxFileIsEnabled()
        {
            var dropBoxPath = GetViewModel().ToPath;
        }

        [When(@"I Click New")]
        public void WhenIClickNew()
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }

        [When(@"I Select ""(.*)"" as the source")]
        public void WhenISelectAsTheSource(string sourceName)
        {
            if (sourceName == "Drop")
            {
                var oauthSource = new DropBoxSource()
                {
                    ResourceName = "Drop",
                    AppKey = "sourceKey",
                    AccessToken = "fgklkgjfkngnf"
                };
                GetViewModel().SelectedSource = oauthSource;
            }
        }

        [Then(@"I set Local File equals ""(.*)""")]
        public void ThenISetLocalFileEquals(string localPath)
        {
            GetViewModel().FromPath = localPath;
        }
        [Then(@"I set Dropbox File equals ""(.*)""")]
        public void ThenISetDropboxFileEquals(string dropboxPath)
        {
            GetViewModel().ToPath = dropboxPath;
        }

        [When(@"I change source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeSourceFromTo(string oldSourceName, string newSourceName)
        {
            Assert.AreEqual<string>(oldSourceName,GetViewModel().SelectedSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().FromPath));
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().ToPath));
            GetViewModel().SelectedSource = new DropBoxSource()
            {
                ResourceName = newSourceName
            };
        }

        [Then(@"the New Dropbox Source window is opened")]
        public void ThenTheNewDropboxSourceWindowIsOpened()
        {
            var mock = scenarioContext.Get<Mock<IEventAggregator>>("mockEventAggregator");
            mock.Verify(aggregator => aggregator.Publish(It.IsAny<IMessage>()));
        }

        [Then(@"the ""(.*)"" Dropbox Source window is opened")]
        public void ThenTheDropboxSourceWindowIsOpened(string sourceName)
        {
            if(sourceName == "Drop")
            {
                Assert.IsTrue(GetViewModel().SelectedSource.ResourceName == sourceName);
            }
        }
        [Then(@"Edit is Enabled")]
        public void ThenEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Then(@"Local File equals ""(.*)""")]
        public void ThenLocalFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }

        [Then(@"Dropbox File equals ""(.*)""")]
        public void ThenDropboxFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }

        protected override void BuildDataList()
        {
            throw new NotImplementedException();
        }
    }
}
