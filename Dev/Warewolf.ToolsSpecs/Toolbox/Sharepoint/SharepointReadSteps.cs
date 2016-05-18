using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Sharepoint;
using TechTalk.SpecFlow;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.SharepointListRead;

namespace Warewolf.ToolsSpecs.Toolbox.Sharepoint
{
    [Binding]
    public sealed class SharepointReadSteps
    {
        [Given(@"I drag Sharepoint Read Tool onto the design surface")]
        public void GivenIDragSharepointReadToolOntoTheDesignSurface()
        {
            var sharepointReadActivity = new SharepointReadListActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sharepointReadActivity);
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

            var sqlServerDesignerViewModel = new SharepointListReadDesignerViewModel(modelItem);

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockSharePointSourceViewModel);
            ScenarioContext.Current.Add("mockSharepointSourceModel", mockSharepointSourceModel);
        }

        [Then(@"the Sharepoint Read Tool is refreshed")]
        public void ThenTheSharepointReadToolIsRefreshed()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"Sharepoint Read variables are")]
        public void WhenSharepointReadVariablesAre(Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
