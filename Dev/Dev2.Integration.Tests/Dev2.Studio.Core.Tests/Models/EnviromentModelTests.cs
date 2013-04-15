using System;
using System.Security.Principal;
using Caliburn.Micro;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests.Models
{
    [TestClass]
    public class EnviromentModelTests
    {
        #region Connect Tests

        [TestMethod]
        public void EnvironmentModelConnectToAvailableServersAuxiliaryChannelOnLocalhostExpectedConnectionSuccesful()
        {
            TestAuxilliaryConnections(ServerSettings.DsfAddress);
        }

        [TestMethod]
        public void EnvironmentModelConnectToAvailableServersAuxiliaryChannelOnPCNameExpectedConnectionSuccesful()
        {
            TestAuxilliaryConnections(string.Format(ServerSettings.DsfAddressFormat, Environment.MachineName));
        }

        #endregion Connect Tests

        #region TestAuxilliaryConnections

        static void TestAuxilliaryConnections(string appServerUri)
        {
            var connection = CreateConnection(appServerUri, true);
            var environment = new EnvironmentModel(connection) { Name = "conn" };

            var auxConnection = CreateConnection(appServerUri, true);
            var auxEnvironment = new EnvironmentModel(auxConnection) { Name = "auxconn" };

            environment.Connect();
            Assert.IsTrue(environment.IsConnected);

            auxEnvironment.Connect(environment);
            Assert.IsTrue(auxEnvironment.IsConnected);

            auxEnvironment.Disconnect();
            environment.Disconnect();
        }

        #endregion

        #region CreateConnection

        static TcpConnection CreateConnection(string appServerUri, bool isAuxiliary = false)
        {
            var securityContetxt = new Mock<IFrameworkSecurityContext>();
            securityContetxt.Setup(c => c.UserIdentity).Returns(WindowsIdentity.GetCurrent());

            var eventAggregator = new Mock<IEventAggregator>();
            return new TcpConnection(securityContetxt.Object, new Uri(appServerUri), Int32.Parse(ServerSettings.WebserverPort), eventAggregator.Object, isAuxiliary);
        }

        #endregion

    }
}
