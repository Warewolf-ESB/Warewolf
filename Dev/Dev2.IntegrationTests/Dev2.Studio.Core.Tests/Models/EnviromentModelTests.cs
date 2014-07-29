using System;
using Dev2.AppResources.Repositories;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
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
            var repo = new Mock<IResourceRepository>();
            var sRepo = new Mock<IStudioResourceRepository>();
            var connection = CreateConnection(appServerUri);
            var environment = new EnvironmentModel(Guid.NewGuid(), connection, repo.Object,sRepo.Object) { Name = "conn" };

            var auxRepo = new Mock<IResourceRepository>();
            var auxConnection = CreateConnection(appServerUri);
            var auxEnvironment = new EnvironmentModel(Guid.NewGuid(), auxConnection, auxRepo.Object,sRepo.Object) { Name = "auxconn" };

            environment.Connect();
            Assert.IsTrue(environment.IsConnected);

            auxEnvironment.Connect(environment);
            Assert.IsTrue(auxEnvironment.IsConnected);

            auxEnvironment.Disconnect();
            environment.Disconnect();
        }

        #endregion

        #region CreateConnection

        static IEnvironmentConnection CreateConnection(string appServerUri)
        {

            return new ServerProxy(new Uri(appServerUri));
        }

        #endregion

    }
}
