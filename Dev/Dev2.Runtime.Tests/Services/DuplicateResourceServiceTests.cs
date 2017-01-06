using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
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
    public class DuplicateResourceServiceTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var service = new DuplicateResourceService();

            //------------Execute Test---------------------------
            var resId = service.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var service = new DuplicateResourceService();

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
            DuplicateResourceService resourceService = new DuplicateResourceService();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.HandlesType();
            //---------------Test Result -----------------------
            Assert.AreEqual("DuplicateResourceService", handlesType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateServiceEntry_GivenServiceIsCreated_ShouldCreateCorrectDynamicService()
        {
            //---------------Set up test pack-------------------
            DuplicateResourceService resourceService = new DuplicateResourceService();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.CreateServiceEntry();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, handlesType.Actions.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourcePayLoad_ShouldExctactPayLoad()
        {
            const string guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IExplorerItem>();
            resourceCatalog.Setup(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResourceCatalogDuplicateResult
                {
                    DuplicatedItems = new List<IExplorerItem> { resource.Object }
                });
            var workScpace = new Mock<IWorkspace>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid) },
                {"NewResourceName", new StringBuilder("NewName") },
                {"destinationPath", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsNotNull(stringBuilder);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenMissingDestinationPath_ShouldReturnFailure()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IExplorerItem>();
            resourceCatalog.Setup(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResourceCatalogDuplicateResult
                {
                    DuplicatedItems = new List<IExplorerItem> { resource.Object }
                });
            var workScpace = new Mock<IWorkspace>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
                            {
                                  {"ResourceID", new StringBuilder(Guid.NewGuid().ToString()) },
                                {"NewResourceName", new StringBuilder("NewName") },
                            }, workScpace.Object);
            }
            catch (Exception ex)
            {
                resourceCatalog.Verify(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
                var serializer = new Dev2JsonSerializer();
                var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
                Assert.IsTrue(executeMessage.HasError);
                Assert.AreEqual("Destination Paths not specified", executeMessage.Message.ToString());
                Assert.AreEqual("Destination Paths not specified", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenMissingResourceId_ShouldReturnFailure()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IExplorerItem>();
            resourceCatalog.Setup(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResourceCatalogDuplicateResult
                {
                    DuplicatedItems = new List<IExplorerItem> { resource.Object }
                });
            var workScpace = new Mock<IWorkspace>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"NewResourceName", new StringBuilder("NewName") },
                {"destinatioPath", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            var serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsTrue(executeMessage.HasError);
            Assert.AreEqual("ResourceId is required", executeMessage.Message.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenCatalogThrowsException_ShouldReturnFailureAndCatalogException()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Catalog Error"));
            var workScpace = new Mock<IWorkspace>();
            const string guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
                                                        {
                                                            {"ResourceID", new StringBuilder(guid) },
                                                            {"NewResourceName", new StringBuilder("NewName") },
                                                            {"destinatioPath", new StringBuilder("NewName") },
                                                        }, workScpace.Object);
            }
            catch (Exception ex)
            {
                resourceCatalog.Verify(catalog => catalog.DuplicateResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));
                var serializer = new Dev2JsonSerializer();
                var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
                Assert.IsTrue(executeMessage.HasError);
                Assert.AreEqual("ResourceId is required", executeMessage.Message);
                Assert.AreEqual("Catalog Error", ex.Message);
            }


        }
    }
}
