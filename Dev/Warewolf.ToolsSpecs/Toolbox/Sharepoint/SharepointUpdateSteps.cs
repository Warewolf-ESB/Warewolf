using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dev2.Activities;
using Dev2.Activities.Designers2.SharepointListUpdate;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Moq;
using TechTalk.SpecFlow;
using Dev2.Activities.Sharepoint;

namespace Warewolf.ToolsSpecs.Toolbox.Sharepoint
{
    [Binding]
    public sealed class SharepointUpdateSteps
    {
        [Given(@"I drag Sharepoint Update Tool onto the design surface")]
        public void GivenIDragSharepointUpdateToolOntoTheDesignSurface()
        {
            var sharepointUpdateActivity = new SharepointUpdateListItemActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sharepointUpdateActivity);
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

            var sqlServerDesignerViewModel = new SharepointListUpdateDesignerViewModel(modelItem);

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockSharePointSourceViewModel);
            ScenarioContext.Current.Add("mockSharepointSourceModel", mockSharepointSourceModel);
        }

        [When(@"Sharepoint Update variables are")]
        public void WhenSharepointUpdateVariablesAre()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"Sharepoint Update Input variables are")]
        public void WhenSharepointUpdateInputVariablesAre()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
