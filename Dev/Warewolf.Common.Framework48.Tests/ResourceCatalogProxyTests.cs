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
