/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
    [TestCategory("Runtime WebServer")]
    public class DataObjectExtentionsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IResource resource)
        public void SetResourceNameAndId_GivenResourceNameIsBad_ShouldFixAndLoadResource()
        {
            //---------------Set up test pack-------------------
            var mockResource = new Mock<IResource>();
            var expectedResourceId = Guid.NewGuid();
            mockResource.SetupGet(o => o.ResourceID).Returns(expectedResourceId);
            mockResource.SetupGet(o => o.ResourceName).Returns("Hello World");
            mockResource.Setup(o => o.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            var resourceCatalog = new Mock<IResourceCatalog>();
            const string ResourceId = "acb75027-ddeb-47d7-814e-a54c37247ec1";
            var objSourceResourceID = ResourceId.ToGuid();
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), objSourceResourceID)).Returns(mockResource.Object);
            
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ResourceID);
            dataObject.SetupProperty(o => o.TestsResourceIds);
            const string ResourceName = "acb75027-ddeb-47d7-814e-a54c37247ec1.xml";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            dataObject.Object.SetResourceNameAndId(resourceCatalog.Object, ResourceName, out var outResource);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(It.IsAny<Guid>(), objSourceResourceID));
            dataObject.VerifySet(o => o.ResourceID = expectedResourceId, Times.Exactly(1));
            dataObject.VerifySet(o => o.ServiceName = "Hello World", Times.Exactly(1));
            dataObject.VerifySet(o => o.SourceResourceID = expectedResourceId, Times.Exactly(1));
        }
    }
}
