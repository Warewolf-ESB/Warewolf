using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.DropBox2016.DropboxFile;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Storage;

namespace Dev2.Activities.Specs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class ReadListDropboxSteps
    {
        [Given(@"I drag Readlist Dropbox Tool onto the design surface")]
        public void GivenIDragReadlistDropboxToolOntoTheDesignSurface()
        {
            var dropBoxUploadTool = new DsfDropboxFileListActivity();
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

            var dropBoxFileListDesignerViewModel = new DropBoxFileListDesignerViewModel(modelItem, mockEnvironmentModel.Object, mockEventAggregator.Object);
            ScenarioContext.Current.Add("dropBoxFileListDesignerViewModel", dropBoxFileListDesignerViewModel);
            ScenarioContext.Current.Add("mockEnvironmentModel", mockEnvironmentModel);
            ScenarioContext.Current.Add("eventAggrMock", mockEventAggregator);
        }
        private static DropBoxFileListDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<DropBoxFileListDesignerViewModel>("dropBoxFileListDesignerViewModel");
        }
        private static Mock<IEnvironmentModel> GeEnvrionmentModel()
        {
            return ScenarioContext.Current.Get<Mock<IEnvironmentModel>>("mockEnvironmentModel");
        }
        private Mock<IEventAggregator> GetEventAggregator()
        {
            return ScenarioContext.Current.Get<Mock<IEventAggregator>>("eventAggrMock");
        }
        [Given(@"Readlist New is Enabled")]
        public void GivenReadlistNewIsEnabled()
        {
            var canExecute = GetViewModel().NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }
        
        [Given(@"Readlist Edit is Disabled")]
        public void GivenReadlistEditIsDisabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }
        
        [Given(@"Readlist Dropbox File is Enabled")]
        public void GivenReadlistDropboxFileIsEnabled()
        {
            
        }
        [Then(@"the ""(.*)"" Readlist Dropbox Source window is opened")]
        public void ThenTheReadlistDropboxSourceWindowIsOpened(string sourceName)
        {
            if (sourceName == "Drop")
                Assert.IsTrue(GetViewModel().SelectedSource.ResourceName == sourceName);
        }

        
        [When(@"I Click Readlist New")]
        public void WhenIClickReadlistNew()
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }
        
        [When(@"I Select ""(.*)"" as the Readlist source")]
        public void WhenISelectAsTheReadlistSource(string sourceName)
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
        
        [When(@"I Readlist click ""(.*)""")]
        public void WhenIReadlistClick(string p0)
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }
        
        [When(@"I change Readlist source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeReadlistSourceFromTo(string oldSourceName, string newSourceName)
        {
            var selectedSource = GetViewModel().SelectedSource;
            Assert.AreEqual(oldSourceName, selectedSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().ToPath));
            GetViewModel().SelectedSource = new OauthSource()
            {
                ResourceName = newSourceName
            };
        }
        
        [When(@"Readlist Dropbox File equals ""(.*)""")]
        public void WhenReadlistDropboxFileEquals(string emptyString)
        {
            Assert.IsTrue(string.IsNullOrEmpty(emptyString));
        }
        
        [Then(@"the New Readlist Dropbox Source window is opened")]
        public void ThenTheNewReadlistDropboxSourceWindowIsOpened()
        {
            Mock<IEventAggregator> eventAggregator = GetEventAggregator();
            eventAggregator.Verify(a => a.Publish(It.IsAny<IMessage>()));     
        }
        
        [Then(@"Readlist Edit is Enabled")]
        public void ThenReadlistEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }
       
        
        [Then(@"I set Readlist Dropbox File equals ""(.*)""")]
        public void ThenISetReadlistDropboxFileEquals(string dropboxPath)
        {
            GetViewModel().ToPath = dropboxPath;
        }
    }
}
