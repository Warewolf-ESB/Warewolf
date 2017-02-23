using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Explorer;
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
        private Guid _serverId;

        [TestInitialize]
        public void Initialize()
        {
            var envId = new Guid();
            _serverId = new Guid();
            _env = new Mock<IEnvironmentModel>();
            _envConnection = new Mock<IEnvironmentConnection>();
            _proxyLayer = new Mock<IExplorerRepository>();
            _envConnection.Setup(connection => connection.ServerID).Returns(_serverId);
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
        public void EnvironmentModel_GivenEnvironment_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.EnvironmentModel);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UpdateRepository_GivenNewServerInstane_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var server = new Server(_env.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.UpdateRepository);
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
        public void Permissions_GivenServerIsConnected_ShouldReturn1()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(true);
            var query = new Mock<IQueryManager>();
            var windowsGroupPermissions = new List<IWindowsGroupPermission>
            {
                new Mock<IWindowsGroupPermission>().Object
            };
            query.Setup(manager => manager.FetchPermissions()).Returns(windowsGroupPermissions);
            _proxyLayer.Setup(repository => repository.QueryManagerProxy).Returns(query.Object);
            var server = new Server(_proxyLayer.Object, _env.Object);
            Assert.AreEqual(1, server.Permissions.Count);
            //------------Assert Precondition-------------------------
            Assert.IsNotNull(server.Permissions);
            //------------Execute Test---------------------------
            server.Permissions = new List<IWindowsGroupPermission>();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, server.Permissions.Count);
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
            var adminManager = new Mock<IAdminManager>();
            adminManager.Setup(manager => manager.GetMinSupportedServerVersion()).Returns("1.5.0.9");
            _proxyLayer.SetupGet(repository => repository.AdminManagerProxy).Returns(adminManager.Object);
            var server = new Server(_proxyLayer.Object, _env.Object);
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
            var query = new Mock<IAdminManager>();
            query.Setup(manager => manager.GetMinSupportedServerVersion()).Returns("2.0.0.0");
            _proxyLayer.Setup(repository => repository.AdminManagerProxy).Returns(query.Object);
            var server = new Server(_proxyLayer.Object, _env.Object);
            //------------Assert Precondition-------------------------
            //------------Execute Test---------------------------
            var minSupportedVersion = server.GetMinSupportedVersion();
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(minSupportedVersion));
            Assert.AreEqual("2.0.0.0", minSupportedVersion);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void IsConnected_GivenEnvironmentConnectionIsConnected_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(connection => connection.IsConnected).Returns(true);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------            
            //------------Assert Results-------------------------
            Assert.IsTrue(server.IsConnected);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void AllowEdit_GivenServerIsNotLocalHost_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(server.AllowEdit);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_GivenEnvirnmentConnectionIsConnected_ShouldHaveValue()
        {
            const string serverName = "Localhost";
            const string serverNameConnected = "Localhost (Connected)";
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(true);
            _envConnection.Setup(model => model.IsConnected).Returns(true);
            _envConnection.Setup(model => model.DisplayName).Returns(serverName);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));
            Assert.AreEqual(serverNameConnected, server.DisplayName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ToString_GivenDisplyaName_ShouldDisplayName()
        {
            const string serverName = "Localhost";
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(true);
            _envConnection.Setup(model => model.DisplayName).Returns(serverName);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var toString = server.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual(serverName, toString);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_GivenEnvirnmentConnectionIsNotConnected_ShouldHaveValue()
        {
            const string serverName = "Localhost";
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(true);
            _envConnection.Setup(model => model.IsConnected).Returns(false);
            _envConnection.Setup(model => model.DisplayName).Returns(serverName);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));
            Assert.AreEqual(serverName, server.DisplayName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_GivenNoEnvirnmentConnection_ShouldHaveValueOfNewRemoteServer()
        {
            const string serverName = "New Remote Server...";
            //------------Setup for test--------------------------
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));            
            //------------Execute Test---------------------------
            server.EnvironmentConnection = null;
            //------------Assert Results-------------------------            
            Assert.AreEqual(serverName, server.DisplayName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExplorerRepository_GivenProxyLayer_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _proxyLayer.Setup(repository => repository.QueryManagerProxy).Returns(new Mock<IQueryManager>().Object);
            var server = new Server(_proxyLayer.Object, _env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.ExplorerRepository);
            Assert.IsNotNull(server.QueryProxy);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Connect_GivenEnvironmentConnectionIsNoConnected_ShouldConnect()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _envConnection.Setup(model => model.IsConnected).Returns(false);
            _envConnection.Setup(model => model.Connect(It.IsAny<Guid>()));
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            server.Connect();
            //------------Assert Results-------------------------
            Assert.IsNotNull(server);
            _envConnection.Verify(connection => connection.Connect(It.IsAny<Guid>()), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ConnectAsync_GivenEnvironmentConnectionIsNoConnected_ShouldConnect()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _envConnection.Setup(model => model.IsConnected).Returns(false);
            _envConnection.Setup(model => model.ConnectAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var connectAsync = server.ConnectAsync();
            //------------Assert Results-------------------------
            Assert.IsTrue(connectAsync.Result);
            _envConnection.Verify(connection => connection.ConnectAsync(It.IsAny<Guid>()), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Disconnect_GivenEnvironmentConnectionIsNoConnected_ShouldConnect()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _envConnection.Setup(model => model.IsConnected).Returns(false);
            _envConnection.Setup(model => model.Disconnect());
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            server.Disconnect();
            //------------Assert Results-------------------------
            _envConnection.Verify(connection => connection.Disconnect(), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void FetchServer_GivenServerId_ShouldReturnServer()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var fetchServer = server.FetchServer(_serverId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(fetchServer);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void FetchServer_GivenNonExistingServerId_ShouldReturnServer()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var fetchServer = server.FetchServer(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNull(fetchServer);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAllServerConnections_Given1ServerIsConnected_ShouldReturn1Server()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _envConnection.Setup(model => model.IsConnected).Returns(true);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var allServerConnections = server.GetAllServerConnections();
            //------------Assert Results-------------------------
            Assert.IsNotNull(allServerConnections);
            Assert.IsTrue(allServerConnections.Count > 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetServerConnections_Given1ServerIsConnected_ShouldReturn1Server()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _envConnection.Setup(model => model.IsConnected).Returns(true);
            var server = new Server(_env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var allServerConnections = server.GetServerConnections();
            //------------Assert Results-------------------------
            Assert.IsNotNull(allServerConnections);
            Assert.IsTrue(allServerConnections.Count > 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void LoadTools_ShouldReturnTools()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var query = new Mock<IQueryManager>();
            var toolDescriptors = new List<IToolDescriptor>
            {
                new Mock<IToolDescriptor>().Object
            };
            query.Setup(manager => manager.FetchTools()).Returns(toolDescriptors);
            _proxyLayer.Setup(repository => repository.QueryManagerProxy).Returns(query.Object);
            var server = new Server(_proxyLayer.Object, _env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var tools = server.LoadTools();
            //------------Assert Results-------------------------
            Assert.IsNotNull(tools);
            Assert.AreEqual(1 , tools.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void LoadExplorer_ShouldReturnTools()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            _proxyLayer.Setup(repository => repository.LoadExplorer(false)).ReturnsAsync(new ServerExplorerItem());
            var server = new Server(_proxyLayer.Object, _env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var loadExplorer = server.LoadExplorer();
            //------------Assert Results-------------------------
            Assert.IsNotNull(loadExplorer);
            Assert.IsTrue(server.HasLoaded);
            _proxyLayer.Verify(repository => repository.LoadExplorer(false), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void LoadExplorerDuplicates_ShouldReturnTools()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var duplicates = new List<string>()
            {
                "SomeDuplicateResource"
            };
            _proxyLayer.Setup(repository => repository.LoadExplorerDuplicates()).ReturnsAsync(duplicates);
            var server = new Server(_proxyLayer.Object, _env.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var explorerDuplicates = server.LoadExplorerDuplicates();
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerDuplicates);
            Assert.IsTrue(server.HasLoaded);
            _proxyLayer.Verify(repository => repository.LoadExplorerDuplicates(), Times.AtLeastOnce);
        }
    }
}