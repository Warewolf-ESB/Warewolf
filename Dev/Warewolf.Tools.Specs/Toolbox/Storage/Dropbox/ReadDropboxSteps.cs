using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.DropboxFile;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class ReadDropboxSteps
    {
        private readonly ScenarioContext scenarioContext;

        public ReadDropboxSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        [Given(@"I drag Readlist Dropbox Tool onto the design surface")]
        public void GivenIDragReadDropboxToolOntoTheDesignSurface()
        {
            var mock = new Mock<IServer>();

            var mockShellVm = new Mock<IShellViewModel>();
            mockShellVm.SetupGet(model => model.ActiveServer).Returns(mock.Object);
            CustomContainer.Register(mockShellVm.Object);
            var dropboxFileListActivity = new DsfDropboxFileListActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dropboxFileListActivity);
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
            mockResourcRepositorySetUp.Setup(repository => repository.FindSourcesByType<OauthSource>(mockEnvironmentModel.Object, It.IsAny<enSourceType>()))
                .Returns(sources);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourcRepositorySetUp.Object);

            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);
            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(new List<IResource>());
            var viewModel = new DropBoxFileListDesignerViewModel(modelItem, dropBoxSourceManager.Object);
            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockEnvironmentModel", mockEnvironmentModel);
            scenarioContext.Add("eventAggrMock", mockEventAggregator);
        }

        DropBoxFileListDesignerViewModel GetViewModel()
        {
            return scenarioContext.Get<DropBoxFileListDesignerViewModel>("viewModel");
        }

        Mock<IServer> GeEnvrionmentModel()
        {
            return scenarioContext.Get<Mock<IServer>>("mockEnvironmentModel");
        }

        Mock<IEventAggregator> GetEventAggregator()
        {
            return scenarioContext.Get<Mock<IEventAggregator>>("eventAggrMock");
        }

        [Given(@"Readlist New is Enabled")]
        public void GivenReadNewIsEnabled()
        {
            var canExecute = GetViewModel().NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Given(@"Readlist Edit is Disabled")]
        public void GivenReadEditIsDisabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Given(@"Readlist Dropbox File is Enabled")]
        public void GivenReadDropboxFileIsEnabled()
        {
            var propertyInfo = GetViewModel().GetType().GetProperty("ToPath");
            Assert.IsNotNull(propertyInfo);
        }

        [When(@"I Click Readlist New")]
        public void WhenIClickReadNew()
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }

        [When(@"I Readlist click Edit")]
        public void WhenIReadlistClickEdit()
        {
            var editDropboxSourceCommand = GetViewModel().EditDropboxSourceCommand;
            editDropboxSourceCommand.Execute(null);
        }

        [When(@"I Select ""(.*)"" as the Readlist source")]
        public void WhenISelectAsTheReadSource(string sourceName)
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

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }

        [When(@"I change Readlist source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeReadSourceFromTo(string oldSourceName, string newSourceName)
        {
            var selectedSource = GetViewModel().SelectedSource;
            Assert.AreEqual<string>(oldSourceName, selectedSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().ToPath));
            GetViewModel().SelectedSource = new DropBoxSource()
            {
                ResourceName = newSourceName
            };
        }

        [Then(@"the New Readlist Dropbox Source window is opened")]
        public void ThenTheReadNewDropboxSourceWindowIsOpened()
        {
            Mock<IEventAggregator> eventAggregator = GetEventAggregator();
            eventAggregator.Verify(a => a.Publish(It.IsAny<IMessage>()));
        }

        [Then(@"Readlist Edit is Enabled")]
        public void ThenReadEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Then(@"the ""(.*)"" Readlist Dropbox Source window is opened")]
        public void ThenTheReadDropboxSourceWindowIsOpened(string sourceName)
        {
            if (sourceName == "Drop")
            {
                Assert.IsTrue(GetViewModel().SelectedSource.ResourceName == sourceName);
            }
        }

        [Then(@"Readlist Local File equals ""(.*)""")]
        public void ThenReadLocalFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }

        [Then(@"Readlist Dropbox File equals ""(.*)""")]
        [When(@"Readlist Dropbox File equals ""(.*)""")]
        public void ThenReadDropboxFileEquals(string dropBoxFile)
        {
            Assert.AreEqual(dropBoxFile, GetViewModel().ToPath);
        }

        [Then(@"I set Readlist Dropbox File equals ""(.*)""")]
        public void ThenISetDropboxFileEquals(string dropboxPath)
        {
            GetViewModel().ToPath = dropboxPath;
        }
    }
}
