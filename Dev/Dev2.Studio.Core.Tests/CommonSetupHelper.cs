using Caliburn.Micro;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Core.Tests.Environments;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Core.Tests
{
    public static class CommonSetupHelper
    {
        public static Mock<IEnvironmentConnection> CreateConnection(bool isConnected)
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            conn.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            conn.Setup(c => c.IsConnected).Returns(isConnected);
            conn.Setup(connection => connection.DisplayName).Returns("localhost");
            return conn;
        }

        public static Mock<IEnvironmentConnection> CreateConnection(bool isConnected, Uri uri, AuthenticationType auth = AuthenticationType.Windows)
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(uri);
            conn.Setup(connection => connection.AppServerUri).Returns(uri);
            conn.Setup(c => c.IsConnected).Returns(isConnected);
            conn.Setup(connection => connection.DisplayName).Returns("localhost");
            conn.Setup(a => a.AuthenticationType).Returns(auth);
            return conn;
        }

        public static void RegisterServerRepository()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IServerRepository>();
            var environments = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false, new Uri("http://azureprivatecloud/machine1:3142")).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false, new Uri("http://azureprivatecloud/machine2:3142")).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            environmentRepository.Setup(env => env.ActiveServer).Returns(environments[0]);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);

            CustomContainer.Register(environmentRepository.Object);
        }
    }
}
