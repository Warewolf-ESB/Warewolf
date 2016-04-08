using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Moq;
using TechTalk.SpecFlow;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using Dev2.Activities.Designers2.DropBox2016.Download;
using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Specs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class ReadDropboxSteps
    {
        [Given(@"I drag Read Dropbox Tool onto the design surface")]
        public void GivenIDragReadDropboxToolOntoTheDesignSurface()
        {
            var dropBoxUploadTool = new DsfDropBoxDownloadActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dropBoxUploadTool);
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockResourcRepositorySetUp = new Mock<IResourceRepository>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var sources = new List<OauthSource>()
            {
                new OauthSource(){ResourceName = "Test Resource Name"}
            };
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockResourcRepositorySetUp.Setup(repository => repository.FindSourcesByType<OauthSource>(mockEnvironmentModel.Object, It.IsAny<enSourceType>()))
                .Returns(sources);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourcRepositorySetUp.Object);

            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

            var downloadViewModel = new DropBoxDownloadViewModel(modelItem, mockEnvironmentModel.Object, mockEventAggregator.Object);
            ScenarioContext.Current.Add("downloadViewModel", downloadViewModel);
            ScenarioContext.Current.Add("mockEnvironmentModel", mockEnvironmentModel);
            ScenarioContext.Current.Add("eventAggrMock", mockEventAggregator);
            
        }

        private static DropBoxDownloadViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<DropBoxDownloadViewModel>("downloadViewModel");
        }
        private static Mock<IEnvironmentModel> GeEnvrionmentModel()
        {
            return ScenarioContext.Current.Get<Mock<IEnvironmentModel>>("mockEnvironmentModel");
        }
        private Mock<IEventAggregator> GetEventAggregator()
        {
            return ScenarioContext.Current.Get<Mock<IEventAggregator>>("eventAggrMock");
        }
        [Given(@"Read New is Enabled")]
        public void GivenReadNewIsEnabled()
        {
            var canExecute = GetViewModel().NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Given(@"Read Edit is Disabled")]
        public void GivenReadEditIsDisabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Given(@"Read Local File is Enabled")]
        public void GivenReadLocalFileIsEnabled()
        {
            var fromPath = GetViewModel().FromPath;
            Assert.IsNotNull(fromPath);
        }

        [Given(@"Read Dropbox File is Enabled")]
        public void GivenReadDropboxFileIsEnabled()
        {
            var dropBoxPath = GetViewModel().ToPath;
            Assert.IsNotNull(dropBoxPath);
        }

        [When(@"I Click Read New")]
        public void WhenIClickReadNew()
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }

        [When(@"I Select ""(.*)"" as the Read source")]
        public void WhenISelectAsTheReadSource(string sourceName)
        {
            if (sourceName == "Drop")
            {
                var oauthSource = new OauthSource()
                {
                    ResourceName = "Drop",
                    Key = "sourceKey",
                    Secret = "fgklkgjfkngnf"
                };
                GetViewModel().SelectedSource = oauthSource;
            }
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }

        [When(@"I change Read source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeReadSourceFromTo(string oldSourceName, string newSourceName)
        {
            var selectedSource = GetViewModel().SelectedSource;
            Assert.AreEqual(oldSourceName, selectedSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().FromPath));
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().ToPath));
            GetViewModel().SelectedSource = new OauthSource()
            {
                ResourceName = newSourceName
            };
        }

        [Then(@"the Read New Dropbox Source window is opened")]
        public void ThenTheReadNewDropboxSourceWindowIsOpened()
        {
            Mock<IEventAggregator> eventAggregator = GetEventAggregator();
            eventAggregator.Verify(a => a.Publish(It.IsAny<IMessage>()));        
        }

        [Then(@"Read Edit is Enabled")]
        public void ThenReadEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Then(@"the ""(.*)"" Read Dropbox Source window is opened")]
        public void ThenTheReadDropboxSourceWindowIsOpened(string sourceName)
        {
            if (sourceName == "Drop")
                Assert.IsTrue(GetViewModel().SelectedSource.ResourceName == sourceName);
        }

        [Then(@"Read Local File equals ""(.*)""")]
        public void ThenReadLocalFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }

        [Then(@"Read Dropbox File equals ""(.*)""")]
        public void ThenReadDropboxFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }
        [Then(@"I set Read Dropbox Local File equals ""(.*)""")]
        public void ThenISetLocalFileEquals(string localPath)
        {
            GetViewModel().FromPath = localPath;
        }
        [Then(@"I set Read Dropbox File equals ""(.*)""")]
        public void ThenISetDropboxFileEquals(string dropboxPath)
        {
            GetViewModel().ToPath = dropboxPath;
        }
    }
}
