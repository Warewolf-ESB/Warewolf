/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Text;

namespace Warewolf.Common.Framework48.Tests
{
    [TestClass]
    public class ResourceCatalogProxyTests
    {
        [TestMethod]
        public void ResourceCatalogProxy_Construct()
        {
            var proxy = GetResourceCatalog();
            Assert.IsNotNull(proxy);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ResourceCatalogProxy))]
        public void ResourceCatalogProxy_GetResourceById_ReturnsResource()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection);
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var resource = proxy.GetResourceById<RabbitMQSource>(Guid.Empty, resourceId);

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ResourceCatalogProxy))]
        public void ResourceCatalogProxy_ResumeWorkflowExecution_Executes()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection);
            var resourceId = "acb75027-ddeb-47d7-814e-a54c37247ec1";
            var startActivity = "bd557ca7-113b-4197-afc3-de5d086dfc69";
            var version = "0";
            var resource = proxy.ResumeWorkflowExecution(resourceId,"{}",startActivity,version);

            Assert.AreEqual("success", resource);
        }
        private static ResourceCatalogProxy GetResourceCatalog()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection);
            return proxy;
        }

        private static IEnvironmentConnection GetConnection()
        {
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(o => o.IsConnected).Returns(true);
            mockEnvironmentConnection.Setup(o => o.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder("{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}"));

            var environmentConnection = mockEnvironmentConnection.Object;
            return environmentConnection;
        }
    }
}
