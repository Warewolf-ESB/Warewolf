using System;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ResourceModelFactoryTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_UserPermissions_Contribute()
        {
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.WorkflowService, "iconPath", "displayName"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "displayName"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "resourceName", "displayName"));
        }

        static void Verify_CreateResourceModel_UserPermissions(Func<IEnvironmentModel, IContextualResourceModel> createResourceModel)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();

            //------------Execute Test---------------------------
            var resourceModel = createResourceModel(environmentModel.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(Permissions.Contribute, resourceModel.UserPermissions);
        }
    }
}
