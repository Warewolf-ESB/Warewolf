using System;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Data;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class DataObjectExtentionsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IResource resource)
        public void SetResourceNameAndId_GivenResourceNameIsBad_ShouldFixAndLoadResource()
        {
            //---------------Set up test pack-------------------
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(resource1 => resource1.ResourceName).Returns("Hello World");
            resource.Setup(resource1 => resource1.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            var resourceCatalog = new Mock<IResourceCatalog>();
            const string ResouId = "acb75027-ddeb-47d7-814e-a54c37247ec1";
            var objSourceResourceID = ResouId.ToGuid();
            resourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), objSourceResourceID))
                .Returns(resource.Object);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ResourceID);
            dataObject.SetupProperty(o => o.TestsResourceIds);
            const string ResourceName = "acb75027-ddeb-47d7-814e-a54c37247ec1.xml";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            dataObject.Object.SetResourceNameAndId(resourceCatalog.Object, ResourceName, out IResource outResource);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(It.IsAny<Guid>(), objSourceResourceID));
            dataObject.VerifySet(o => o.ResourceID = resourceId, Times.Exactly(1));
            dataObject.VerifySet(o => o.ServiceName = "Hello World", Times.Exactly(1));
            dataObject.VerifySet(o => o.SourceResourceID = resourceId, Times.Exactly(1));
        }
    }
}
