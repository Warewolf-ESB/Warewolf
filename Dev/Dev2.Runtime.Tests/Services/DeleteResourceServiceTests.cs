using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteResourceServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var service = new DeleteResource();

            //------------Execute Test---------------------------
            var resId = service.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var service = new DeleteResource();

            //------------Execute Test---------------------------
            var resId = service.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void HandlesType_GivenServiceIsCreated_ShouldHandleCorrectly()
        {
            //---------------Set up test pack-------------------
            DeleteResource resourceService = new DeleteResource();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.HandlesType();
            //---------------Test Result -----------------------
            Assert.AreEqual("DeleteResourceService", handlesType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateServiceEntry_GivenServiceIsCreated_ShouldCreateCorrectDynamicService()
        {
            //---------------Set up test pack-------------------
            DeleteResource resourceService = new DeleteResource();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.CreateServiceEntry();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, handlesType.Actions.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenGlobalWorkspace_ShouldDeleteTests()
        {
            //---------------Set up test pack-------------------
            var testCatalog = new Mock<ITestCatalog>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var workScpace = new Mock<IWorkspace>();
            workScpace.Setup(workspace => workspace.ID).Returns(GlobalConstants.ServerWorkspaceID);
            resourceCatalog.Setup(catalog => catalog.DeleteResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new ResourceCatalogResult()
            {
                Message = "Hi", Status = ExecStatus.Success
            });
            const string guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            DeleteResource resourceService = new DeleteResource(resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid) },
                {"ResourceType", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            testCatalog.Verify(catalog => catalog.DeleteAllTests(guid.ToGuid()), Times.Once);
            var serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsFalse(executeMessage.HasError);
            Assert.IsFalse(string.IsNullOrEmpty(executeMessage.Message.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenWorkspace_ShouldNotDeleteTests()
        {
            //---------------Set up test pack-------------------
            var testCatalog = new Mock<ITestCatalog>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var workScpace = new Mock<IWorkspace>();
            workScpace.Setup(workspace => workspace.ID).Returns(Guid.NewGuid);
            resourceCatalog.Setup(catalog => catalog.DeleteResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new ResourceCatalogResult()
            {
                Message = "Hi",
                Status = ExecStatus.Success
            });
            const string guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            DeleteResource resourceService = new DeleteResource(resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid) },
                {"ResourceType", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            testCatalog.Verify(catalog => catalog.DeleteAllTests(guid.ToGuid()), Times.Exactly(0));
            var serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsFalse(executeMessage.HasError);
            Assert.IsFalse(string.IsNullOrEmpty(executeMessage.Message.ToString()));
        }
    }
}