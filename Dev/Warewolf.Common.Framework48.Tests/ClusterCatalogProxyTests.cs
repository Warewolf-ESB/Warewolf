/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.SignalR.Wrappers;
using Newtonsoft.Json.Linq;
using Warewolf.Client;
using Warewolf.Data;
using Warewolf.Esb;
using Warewolf.EsbClient;

namespace Warewolf.Tests
{
    [TestClass]
    public class ClusterLeaderProxyTests
    {
        [TestMethod]
        public void ClusterLeaderProxy_Construct()
        {
            var proxy = GetClusterLeaderProxy();
            Assert.IsNotNull(proxy);
        }

        [TestMethod]
        public void ClusterLeaderProxy_()
        {
            var notified = false;

            var mockSubscription = new Mock<ISubscriptionWrapper>();
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();
            mockClusterDispatcher.Setup(o => o.Write(It.IsAny<Object>())).Callback<Object>((arg) =>
            {
                mockSubscription.Raise(o => o.Received += null, new List<JToken> {JObject.FromObject(arg)});
            });

            var mockHub = new Mock<IHubProxyWrapper>();
            mockHub.Setup(o => o.Subscribe(nameof(ChangeNotification))).Returns(mockSubscription.Object);
            var hub = mockHub.Object;
            var watcher = hub.Watch<ChangeNotification>();
            watcher.Received += notification =>
            {
                Assert.IsNotNull(notification, "cluster notifications should not be null");
                notified = true;
            };
            
            InjectNotificationIntoMock(mockClusterDispatcher);
            
            Assert.IsTrue(notified);
        }

        private static void InjectNotificationIntoMock(Mock<IClusterDispatcher> mockClusterDispatcher)
        {
            var mockFile = new Mock<IFile>();
            var mockDir = new Mock<IDirectory>();
            var settings = new LegacySettings("some path", mockFile.Object, mockDir.Object, mockClusterDispatcher.Object);
            settings.AuditFilePath = "some new file path";
        }

        #region Setup

        private static ClusterLeaderProxy GetClusterLeaderProxy()
        {
            var environmentConnection = GetConnection();
            var proxy = new ClusterLeaderProxy(environmentConnection);
            return proxy;
        }

        private static IEnvironmentConnection GetConnection()
        {
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(o => o.IsConnected).Returns(true);
            var returnValue =
                new StringBuilder("{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}");
            mockEnvironmentConnection.Setup(o => o.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(returnValue);
            mockEnvironmentConnection.Setup(o => o.ExecuteCommandAsync(It.IsAny<ICatalogRequest>(), It.IsAny<Guid>()))
                .ReturnsAsync(returnValue);

            var environmentConnection = mockEnvironmentConnection.Object;
            return environmentConnection;
        }
        
        //mockProxy.Setup(o => o.Subscribe(nameof(LeaderNotification))).Returns();
        //var proxy = mockProxy.Object;

        /*  // run test
          var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
          var request = new EventRequest<LeaderNotification>(Guid.Empty);
          var watcher = proxy.Watch<LeaderNotification>(request);
          watcher.OnChange += (LeaderNotification notification) => { notified = true; };

          Assert.IsTrue(notified);*/
        #endregion
    }
}
