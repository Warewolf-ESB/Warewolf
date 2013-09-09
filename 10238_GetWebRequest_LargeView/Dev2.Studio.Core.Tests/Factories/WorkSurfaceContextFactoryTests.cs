using System;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Factories
{
    [TestClass]
    public class WorkSurfaceContextFactoryTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkSurfaceContextFactory_CreateDeployViewModel")]
        public void WorkSurfaceContextFactory_CreateDeployViewModel_ResourceModel_DeployViewModelInitialized()
        {
            CompositionInitializer.DefaultInitialize();
            var mock = NavigationViewModelTest.GetMockEnvironment();
            var resourceModel = new ResourceModel(mock.Object);
            resourceModel.ResourceName = "Expected Resource";

            //------------Execute Test---------------------------
            var actual = WorkSurfaceContextFactory.CreateDeployViewModel(resourceModel);

            // Assert DeployViewModelInitialized
            Assert.IsNotNull(actual, "Cannot create DeployWorkSurface with resource model");
        }
    }
}
