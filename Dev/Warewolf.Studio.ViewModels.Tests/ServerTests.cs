using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;

// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServerTests
    {
        private Mock<IEnvironmentModel> _env;
        private Mock<IEnvironmentConnection> _envConnection;
        private Mock<IExplorerRepository> _proxyLayer;

        [TestInitialize]
        public void Initialize()
        {
            var envId = new Guid();
            var serverId = new Guid();
            _env = new Mock<IEnvironmentModel>();
            _envConnection = new Mock<IEnvironmentConnection>();
            _proxyLayer = new Mock<IExplorerRepository>();
            _envConnection.Setup(connection => connection.ServerID).Returns(serverId);
            _envConnection.Setup(connection => connection.DisplayName).Returns("EnvName");
            _env.Setup(model => model.Connection).Returns(_envConnection.Object);
            _env.Setup(model => model.ID).Returns(envId);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DefaultCtor_GivenNewInstance_IsNotNull()
        {
            //------------Setup for test--------------------------
            var server = new Server();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(server);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Server_GivenNewInstance_IsNotNull()
        {
            //------------Setup for test--------------------------
            var server = new Server(_env.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(server);
            Assert.IsNotNull(server.EnvironmentConnection);
            Assert.IsNotNull(server.EnvironmentConnection.ServerID);
            Assert.IsNotNull(server.EnvironmentID);
            Assert.IsNotNull(server.ResourceID);
            Assert.IsFalse(string.IsNullOrEmpty(server.ResourceName));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeployTo_GivenIsAuthorizedDeployToIsTrue_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _env.Setup(model => model.IsAuthorizedDeployTo).Returns(true);
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(server.CanDeployTo);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeployFrom_GivenIsAuthorizedDeployFromIsTrue_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _env.Setup(model => model.IsAuthorizedDeployFrom).Returns(true);
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(server.CanDeployFrom);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ServerID_GivenEnvironmentConnectionServerID_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.ServerID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Permissions_GivenIsAuthorizedDeployFromIsTrue_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(service => service.GetResourcePermissions(It.IsAny<Guid>()))
                .Returns(Permissions.Administrator);
            _env.Setup(model => model.AuthorizationService).Returns(auth.Object);
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.GetPermissions(It.IsAny<Guid>()));
            Assert.AreEqual(Permissions.Administrator, server.GetPermissions(It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void HasLoaded_GivenServerIsConnected_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(true);
            var server = new Server(_env.Object);
            //------------Assert Precondition-------------------------
            Assert.IsTrue(server.IsConnected);
            //------------Execute Test---------------------------
            PrivateObject privateObject = new PrivateObject(server);
            typeof(Server).GetProperty("IsConnected").SetValue(privateObject.Target, true);
            //------------Assert Results-------------------------
            Assert.IsTrue(server.HasLoaded);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetServerInformation_GivenServerIsNotConnected_ShouldReturnTrue()
        {
            var valueFunction = new Dictionary<string, string> { { "some key", "some value" } };
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(false);
            var proxyLayer = new Mock<IExplorerRepository>();

            var adminManager = new Mock<IAdminManager>();
            adminManager.Setup(manager => manager.GetServerInformation()).Returns(valueFunction);
            proxyLayer.SetupGet(repository => repository.AdminManagerProxy).Returns(adminManager.Object);
            var server = new Server(proxyLayer.Object, _env.Object);
            //------------Assert Precondition-------------------------
            //------------Execute Test---------------------------
            var serverInformation = server.GetServerInformation();
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverInformation);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetServerVersion_GivenServerIsNotConnected_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(false);
            var server = new Server(_env.Object);
            var adminManager = new Mock<IAdminManager>();
            adminManager.Setup(manager => manager.GetServerVersion()).Returns("2.0.0.0");
            _proxyLayer.SetupGet(repository => repository.AdminManagerProxy).Returns(adminManager.Object);
            //------------Assert Precondition-------------------------
            //------------Execute Test---------------------------
            var serverVersion = server.GetServerVersion();
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(serverVersion));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetMinSupportedVersion_GivenServerIsNotConnected_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(false);
            var server = new Server(_env.Object);
            var adminManager = new Mock<IAdminManager>();
            adminManager.Setup(manager => manager.GetMinSupportedServerVersion()).Returns("1.5.0.9");
            _proxyLayer.SetupGet(repository => repository.AdminManagerProxy).Returns(adminManager.Object);
            //------------Assert Precondition-------------------------
            //------------Execute Test---------------------------
            var minSupportedVersion = server.GetMinSupportedVersion();
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(minSupportedVersion));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Connect_GivenServerIsNotConnected_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(false);
            var server = new Server(_env.Object);
            //------------Assert Precondition-------------------------
            //------------Execute Test---------------------------
            var minSupportedVersion = server.GetMinSupportedVersion();
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(minSupportedVersion));
        }
    }
}