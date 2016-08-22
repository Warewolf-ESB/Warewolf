using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DuplicateResourceServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void HandlesType_GivenServiceIsCreated_ShouldHandleCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
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
            var resourceCatalog = new Mock<IResourceCatalog>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
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
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var workScpace = new Mock<IWorkspace>();
            var xElement = XML.XmlResource.Fetch("PluginService");
            var pluginSource = new PluginSource(xElement);
            var guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, Guid.Parse(guid))).Returns(pluginSource);
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid) },
                {"NewResourceName", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, Guid.Parse(guid)));
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
