using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Dev2.Activities.Designers2.DropBox2016.Delete;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Core.DynamicServices;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Activities.Designers2.Core;
using Dev2.Runtime.Interfaces;
using Dev2.Studio.Interfaces;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class DeleteDropboxSteps
    {
        private readonly ScenarioContext scenarioContext;

        public DeleteDropboxSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        DropBoxDeleteViewModel GetViewModel()
        {
            return scenarioContext.Get<DropBoxDeleteViewModel>("deleteViewModel");
        }

        [Given(@"I drag Delete Dropbox Tool onto the design surface")]
        public void GivenIDragDeleteDropboxToolOntoTheDesignSurface()
        {
            var dropBoxDeleteTool = new DsfDropBoxDeleteActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dropBoxDeleteTool);
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
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
            var mock = new Mock<IResourceCatalog>();
            mock.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(new List<IResource>());
            var deleteViewModel = new DropBoxDeleteViewModel(modelItem, dropBoxSourceManager.Object);
            scenarioContext.Add("deleteViewModel", deleteViewModel);
            scenarioContext.Add("mockEnvironmentModel", mockEnvironmentModel);
            scenarioContext.Add("mockEventAggregator", mockEventAggregator.Object);
        }

        [Given(@"Dropbox Delete New is Enabled")]
        public void GivenDropboxDeleteNewIsEnabled()
        {
            var canExecute = GetViewModel().NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [Given(@"Dropbox Delete Edit is Disabled")]
        public void GivenDropboxDeleteEditIsDisabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Then(@"Dropbox Delete Edit is Enabled")]
        public void ThenDropboxDeleteEditIsEnabled()
        {
            var canExecute = GetViewModel().EditDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [When(@"I click Dropbox Delete Edit")]
        public void WhenIClickDropboxDeleteEdit()
        {
            GetViewModel().EditDropboxSourceCommand.CanExecute(null);
        }

        [Given(@"Delete Dropbox File is Enabled")]
        public void GivenDeleteDropboxFileIsEnabled()
        {
            var dropBoxDeletePath = GetViewModel().DeletePath;
            //No asserts neccessary
        }
        
        [When(@"I Click Delete New")]
        public void WhenIClickDeleteNew()
        {
            GetViewModel().NewSourceCommand.Execute(null);
        }
        
        [When(@"I Select ""(.*)"" as the Delete source")]
        public void WhenISelectAsTheDeleteSource(string sourceName)
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
        
        [Then(@"I set Delete Dropbox File equals ""(.*)""")]
        public void ThenISetDeleteDropboxFileEquals(string deletePath)
        {
            GetViewModel().DeletePath = deletePath;
        }

        [Then(@"the Delete ""(.*)"" Dropbox Source window is opened")]
        public void ThenTheDeleteDropboxSourceWindowIsOpened(string sourceName)
        {
            if (sourceName == "Drop")
                Assert.IsTrue(GetViewModel().SelectedSource.ResourceName == sourceName);
        }

        [When(@"I change Delete source from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeDeleteSourceFromTo(string oldSourceName, string newSourceName)
        {
            Assert.AreEqual(oldSourceName, GetViewModel().SelectedSource.ResourceName);
            Assert.IsFalse(string.IsNullOrEmpty(GetViewModel().DeletePath));
            GetViewModel().SelectedSource = new DropBoxSource()
            {
                ResourceName = newSourceName
            };
        }

        [When(@"Delete Dropbox File equals ""(.*)""")]
        public void WhenDeleteDropboxFileEquals(string deletePath)
        {
            Assert.IsTrue(string.IsNullOrEmpty(deletePath));
        }
    }
}
