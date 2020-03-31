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
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;

namespace Warewolf.Tests
{
    [TestClass]
    public class ProxyTests
    {
        [TestMethod]
        public void IHubProxy_GetResource_ReturnsResource()
        {
            var expected =
                "{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}";

            // setup mock hub proxy
            var mockProxy = new Mock<IHubProxy>();
            mockProxy.Setup(o => o.Invoke<Receipt>(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns(Task<Receipt>.Factory.StartNew(() => new Receipt()));
            mockProxy.Setup(o => o.Invoke<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns(Task<string>.Factory.StartNew(() => expected));
            var proxy = mockProxy.Object;

            // run test
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var request = new ResourceRequest<RabbitMQSource>(Guid.Empty, resourceId);
            var task = proxy.ExecReq2<RabbitMQSource>(request);
            var resource = task.Result;

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }
    }
}
