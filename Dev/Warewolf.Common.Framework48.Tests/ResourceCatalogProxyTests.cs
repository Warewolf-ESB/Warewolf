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
using System.Security.Principal;
using System.Text;
using Dev2.Communication;

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
            environmentConnection.Setup(o => o.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder("{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}"));

            var proxy = new ResourceCatalogProxy(environmentConnection.Object);
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var resource = proxy.GetResourceById<RabbitMQSource>(Guid.Empty, resourceId);

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }
		
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceCatalogProxy))]
        public void ResourceCatalogProxy_ResumeWorkflowExecution_Executes()
        {
            var executeMessage = new ExecuteMessage
            {
                Message = new StringBuilder("success"),
            };
            var serialize = new Dev2JsonSerializer().Serialize(executeMessage);

            var environmentConnection = GetConnection();
            environmentConnection.Setup(o => o.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(serialize));

            var proxy = new ResourceCatalogProxy(environmentConnection.Object);
            const string resourceId = "acb75027-ddeb-47d7-814e-a54c37247ec1";
            const string startActivity = "bd557ca7-113b-4197-afc3-de5d086dfc69";
            const string version = "0";
            var resource = proxy.ResumeWorkflowExecution(resourceId,"{}",startActivity,version,new Mock<IPrincipal>().Object.ToString());

            Assert.AreEqual("success", resource.Message.ToString());
        }
        private static ResourceCatalogProxy GetResourceCatalog()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection. Object);
            return proxy;
        }

        private static Mock<IEnvironmentConnection> GetConnection()
        {
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(o => o.IsConnected).Returns(true);

            return mockEnvironmentConnection;
        }
    }
}
