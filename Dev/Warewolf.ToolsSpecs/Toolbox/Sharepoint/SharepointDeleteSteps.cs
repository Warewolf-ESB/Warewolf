using System;
using System.Linq.Expressions;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Moq;
using TechTalk.SpecFlow;
using Dev2.Activities.Designers2.SharepointListDelete;

namespace Warewolf.ToolsSpecs.Toolbox.Sharepoint
{
    [Binding]
    public sealed class SharepointDeleteSteps
    {
        [Given(@"I drag Sharepoint Delete Tool onto the design surface")]
        public void GivenIDragSharepointDeleteToolOntoTheDesignSurface()
        {
            var sharepointDeleteActivity = new SharepointDeleteListItemActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sharepointDeleteActivity);
            var mockSharePointSourceViewModel = new Mock<IManageSharePointSourceViewModel>();
            var mockSharepointSourceModel = new Mock<ISharePointSourceModel>();
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

            var sqlServerDesignerViewModel = new SharepointListDeleteDesignerViewModel(modelItem);

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockSharePointSourceViewModel);
            ScenarioContext.Current.Add("mockSharepointSourceModel", mockSharepointSourceModel);
        }

        [When(@"Sharepoint Delete Input variables are")]
        public void WhenSharepointDeleteInputVariablesAre()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"the Sharepoint Delete Tool is refreshed")]
        public void ThenTheSharepointDeleteToolIsRefreshed()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
