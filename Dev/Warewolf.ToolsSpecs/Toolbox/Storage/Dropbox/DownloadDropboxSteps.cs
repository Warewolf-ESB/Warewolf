using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.DropBox2016.Download;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Storage;

namespace Warewolf.ToolsSpecs.Toolbox.Storage.Dropbox
{
    [Binding]
    public class DownloadDropboxSteps
    {
        [Given(@"I drag Download Dropbox Tool onto the design surface")]
        public void GivenIDragDownloadDropboxToolOntoTheDesignSurface()
        {
            var dropBoxDownloadTool = new DsfDropBoxDownloadActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dropBoxDownloadTool);
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
        }
    }
}
