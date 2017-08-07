using System;
using System.Collections.Generic;
using System.Net;
using System.Network;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Communication;
using Dev2.Core.Tests.Environments;
using Dev2.Explorer;
using Dev2.Network;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServerTests
    {
        private Mock<IServer> _env;
        private Mock<IEnvironmentConnection> _envConnection;
        private Mock<IExplorerRepository> _proxyLayer;
        private Guid _serverId;
        private Mock<IAuthorizationService> _authorizationService;

        [TestInitialize]
        public void Initialize()
        {
            var envId = new Guid();
            _serverId = new Guid();
            _env = new Mock<IServer>();
            _authorizationService = new Mock<IAuthorizationService>();
            _envConnection = new Mock<IEnvironmentConnection>();
            _proxyLayer = new Mock<IExplorerRepository>();
            _envConnection.Setup(connection => connection.ServerID).Returns(_serverId);
            _envConnection.Setup(connection => connection.DisplayName).Returns("EnvName");
            _env.Setup(model => model.Connection).Returns(_envConnection.Object);
            _env.Setup(model => model.EnvironmentID).Returns(envId);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Server_GivenNewInstance_IsNotNull()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.DisplayName).Returns("TestConnection");
            var server = new Server(Guid.Empty,mockConnection.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(server);
            Assert.IsNotNull(server.Connection);
            Assert.IsNotNull(server.EnvironmentID);
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeployTo_GivenIsAuthorizedDeployToIsTrue_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _authorizationService.Setup(model => model.IsAuthorized(AuthorizationContext.DeployTo,null)).Returns(true);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, new Mock<IEnvironmentConnection>().Object);
            server.AuthorizationService = _authorizationService.Object;
            //------------Assert Results-------------------------
            Assert.IsTrue(server.CanDeployTo);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeployFrom_GivenIsAuthorizedDeployFromIsTrue_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _authorizationService.Setup(model => model.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, new Mock<IEnvironmentConnection>().Object);
            server.AuthorizationService = _authorizationService.Object;
            //------------Assert Results-------------------------
            Assert.IsTrue(server.CanDeployFrom);            
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ServerID_GivenEnvironmentConnectionServerID_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, mockConnection.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.ServerID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void EnvironmentModel_GivenConnection_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, mockConnection.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(server.Connection);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UpdateRepository_GivenNewServerInstane_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, mockConnection.Object);
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
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, mockConnection.Object);
            server.AuthorizationService = auth.Object;
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
            server.Permissions = windowsGroupPermissions;
            //------------Assert Precondition-------------------------
            Assert.IsNotNull(server.Permissions);
            Assert.AreEqual(1, server.Permissions.Count);
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = proxyLayer.Object;
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            var adminManager = new Mock<IAdminManager>();
            adminManager.Setup(manager => manager.GetServerVersion()).Returns("2.0.0.0");
            _proxyLayer.SetupGet(repository => repository.AdminManagerProxy).Returns(adminManager.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            var server = new Server(Guid.Empty,_envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            var server = new Server(Guid.Empty,_envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------            
            var server = new Server(Guid.Empty, _envConnection.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(server.IsConnected);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void AllowEdit_GivenServerIsNotLocalHost_ShouldReturnTrue()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, _envConnection.Object);
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
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, _envConnection.Object);
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
            var server = new Server(Guid.Empty,_envConnection.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var toString = server.DisplayName;
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
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, _envConnection.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));
            Assert.AreEqual(serverName, server.DisplayName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_GivenNoEnvirnmentConnection_ShouldHaveValueOfNewRemoteServer()
        {
            const string serverName = "Default Name";
            //------------Setup for test--------------------------
            var server = new Server(Guid.Empty,_envConnection.Object);
            //------------Assert Precondition--------------------
            Assert.IsFalse(string.IsNullOrEmpty(server.DisplayName));            
            //------------Execute Test---------------------------
            server.Connection = null;
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

            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            var server = new Server(Guid.Empty, _envConnection.Object);
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
            var server = new Server(Guid.Empty, _envConnection.Object);
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
            _envConnection.Setup(model => model.IsConnected).Returns(true);
            _envConnection.Setup(model => model.Disconnect());
            var server = new Server(Guid.Empty, _envConnection.Object);
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            server.Disconnect();
            //------------Assert Results-------------------------
            _envConnection.Verify(connection => connection.Disconnect(), Times.AtLeastOnce);
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
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
            var server = new Server(Guid.Empty, _envConnection.Object);
            server.ProxyLayer = _proxyLayer.Object;
            //------------Assert Precondition--------------------
            //------------Execute Test---------------------------
            var explorerDuplicates = server.LoadExplorerDuplicates();
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerDuplicates);
            Assert.IsTrue(server.HasLoaded);
            _proxyLayer.Verify(repository => repository.LoadExplorerDuplicates(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void GetServerInformation_Given_ServerInformation_IsNotNUll_Returns_ServerInformation()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var server = new Server(Guid.Empty, _envConnection.Object);
            PrivateObject privateObj = new PrivateObject(server);
            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("information", "value for inforamtion");
            privateObj.SetField("_serverInformation", info);
            var information = server.GetServerInformation();
            Assert.IsNotNull(information);
            Assert.AreEqual(info, information);
        }

        [TestMethod]
        public void GetServerVersion_Given_ServerVersion_IsNotNUll_Returns_ServerVersion()
        {
            //------------Setup for test--------------------------
            _envConnection.Setup(model => model.IsLocalHost).Returns(false);
            var server = new Server(Guid.Empty, _envConnection.Object);
            PrivateObject privateObj = new PrivateObject(server);
            privateObj.SetField("_version", "0.0.0.1");
            var version = server.GetServerVersion();
            Assert.IsNotNull(version);
            Assert.AreEqual("0.0.0.1", version);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModel_Constructor_NullConnection_ThrowsArgumentNullException()
        {
            //var wizard = new Mock<IWizardEngine>();
            // ReSharper disable ObjectCreationAsStatement
            new Server(Guid.NewGuid(), null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void EnvironmentModel_Constructor_ConnectionAndWizardEngine_InitializesConnectionAndResourceRepository()
        {

            var connection = CreateConnection();
            //, wizard.Object
            var env = new Server(Guid.NewGuid(), connection.Object);
            Assert.IsNotNull(env.Connection);
            Assert.IsNotNull(env.ResourceRepository);
            Assert.AreSame(connection.Object, env.Connection);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModel_Constructor_ConnectionAndNullResourceRepository_ThrowsArgumentNullException()
        {
            var connection = CreateConnection();
            // ReSharper disable ObjectCreationAsStatement
            new Server(Guid.NewGuid(), connection.Object, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void EnvironmentModel_Constructor_ConnectionAndResourceRepository_InitializesConnectionAndResourceRepository()
        {
            var connection = CreateConnection();
            var repo = new Mock<IResourceRepository>();
            var env = new Server(Guid.NewGuid(), connection.Object, repo.Object);

            Assert.IsNotNull(env.Connection);
            Assert.IsNotNull(env.ResourceRepository);
            Assert.AreSame(connection.Object, env.Connection);
            Assert.AreSame(repo.Object, env.ResourceRepository);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentModel_DisplayName")]
        public void EnvironmentModel_DisplayName_WithConnection_ContainsConnectionAddress()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            var repo = new Mock<IResourceRepository>();
            var env = new Server(Guid.NewGuid(), connection.Object, repo.Object);
            const string expectedDisplayName = "localhost";
            //------------Execute Test---------------------------
            string displayName = env.DisplayName;
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedDisplayName, displayName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        [ExpectedException(typeof(ArgumentException))]
        public void EnvironmentModel_Connect_IsNotConnectedAndNameIsEmpty_ThrowsArgumentException()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(false);
            connection.Setup(c => c.DisplayName).Returns("");

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_Connect_IsNotConnectedAndNameIsNotEmpty_DoesInvokeConnection()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(false);
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();

            connection.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_Connect_IsConnected_DoesNotInvokeConnection()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();

            connection.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_Connect")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModel_ConnectOther_Null_ThrowsArgumentNullException()
        {
            var connection = CreateConnection();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect(null);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_ConnectOther_NonNullAndConnected_DoesNotInvokeOthersConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c2.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_ConnectOther_NonNullAndConnected_InvokesThisConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c1.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_ConnectOther_NonNullAndNotConnected_InvokesOtherConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.IsConnected).Returns(false);
            c2.Setup(c => c.Connect(It.IsAny<Guid>())).Callback(() => c2.Setup(c => c.IsConnected).Returns(true)).Verifiable();

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c2.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Once());
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnvironmentModel_ConnectOther_NonNullAndNotConnectedFails_ThrowsInvalidOperationException()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.IsConnected).Returns(false);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_NetworkStateChanged")]
        public void EnvironmentModel_NetworkStateChanged_Offline_DoesPublishEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents(NetworkState.Offline);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_NetworkStateChanged")]
        public void EnvironmentModel_NetworkStateChanged_Online_DoesPublishEnvironmentConnectedMessage()
        {
            TestConnectionEvents(NetworkState.Online);
        }

        static void TestConnectionEvents(NetworkState toState)
        {

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(connection => connection.IsConnected).Returns(true);
            environmentConnection.Setup(connection => connection.ServerEvents).Returns(EventPublishers.Studio);

            var repo = new Mock<IResourceRepository>();
            var envModel = new Server(Guid.NewGuid(), environmentConnection.Object, repo.Object);

            envModel.IsConnectedChanged += (sender, args) => Assert.AreEqual(toState == NetworkState.Online, args.IsConnected);

            environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, toState));
        }

        [TestMethod]
        public void EnvironmentModel_ForceLoadResources_InvokesForceLoadOnResourceRepository()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.ForceLoad()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object);

            Assert.IsTrue(env.CanStudioExecute);

            env.ForceLoadResources();

            resourceRepo.Verify(r => r.ForceLoad(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentModel_LoadResources_ShouldLoadTrue_InvokesLoadOnResourceRepository()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object);

            Assert.IsTrue(env.CanStudioExecute);

            env.LoadResources();

            resourceRepo.Verify(r => r.UpdateWorkspace(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentModel_LoadResources_ShouldLoadFalse_NotInvokeLoadOnResourceRepository()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object) { CanStudioExecute = false };

            env.LoadResources();

            resourceRepo.Verify(r => r.Load(), Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EnvironmentModel_Load")]
        public void EnvironmentModel_Load_Loads_SetsLoaded()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object) { CanStudioExecute = true };

            env.LoadResources();
            Assert.IsTrue(env.HasLoadedResources);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EnvironmentModel_Load")]
        public void EnvironmentModel_Load_DoesNotLoads_SetsLoaded()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object) { CanStudioExecute = false };

            env.LoadResources();
            Assert.IsFalse(env.HasLoadedResources);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EnvironmentModel_Load")]
        public void EnvironmentModel_Load_CallsLoadedEvent()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new Server(Guid.NewGuid(), connection.Object, resourceRepo.Object);
            env.ResourcesLoaded += (sender, args) => Assert.AreEqual(args.Model, env);
            env.CanStudioExecute = false;

            env.LoadResources();
            Assert.IsFalse(env.HasLoadedResources);

        }


        [TestMethod]
        [TestCategory("EnvironmentModel_IsLocalHost")]
        public void EnvironmentModel_IsLocalHost_IsLocalHost_True()
        {
            var conn = CreateConnection();
            conn.SetupProperty(c => c.DisplayName, "localhost");
            conn.Setup(connection => connection.IsLocalHost).Returns(conn.Object.DisplayName == "localhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost;
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_IsLocalHost")]
        public void EnvironmentModel_IsLocalHost_IsNotLocalHost_False()
        {
            var conn = CreateConnection();
            conn.Setup(c => c.DisplayName).Returns("notlocalhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost;
            Assert.IsFalse(isLocalHost);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_AuthorizationService")]
        public void EnvironmentModel_AuthorizationService_Constructor_PropertyInitialized()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();

            //------------Execute Test---------------------------
            var env = new Server(Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object);
            connection.Raise(environmentConnection => environmentConnection.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
            //------------Assert Results-------------------------
            Assert.IsNotNull(env.AuthorizationService);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connection")]
        public void EnvironmentModel_Connection_PermissionsChanged_IsAuthorizedChanged()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(false);

            var envModel = new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);
            Assert.IsFalse(envModel.IsAuthorized);

            //------------Execute Test---------------------------
            connection.Setup(c => c.IsAuthorized).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(envModel.IsAuthorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connection")]
        public void EnvironmentModel_Connection_PermissionsChanged_IsDeployFromChanged()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);

            var envModel = new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);
            connection.Raise(environmentConnection => environmentConnection.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
            Assert.IsFalse(envModel.IsAuthorizedDeployFrom);

            //------------Execute Test---------------------------
            envModel.AuthorizationServiceMock.Setup(service => service.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(envModel.IsAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connection")]
        public void EnvironmentModel_Connection_PermissionsChanged_IsDeployToChanged()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);

            var envModel = new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);
            connection.Raise(environmentConnection => environmentConnection.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
            Assert.IsFalse(envModel.IsAuthorizedDeployTo);

            //------------Execute Test---------------------------
            envModel.AuthorizationServiceMock.Setup(service => service.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(envModel.IsAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_AuthorizationService")]
        public void EnvironmentModel_AuthorizationService_PermissionsChanged_IsAuthorizedDeployToAndIsAuthorizedDeployFromChanged()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();

            var envModel = new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);
            connection.Raise(environmentConnection => environmentConnection.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(false).Verifiable();
            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(false).Verifiable();

            Assert.IsFalse(envModel.IsAuthorizedDeployFrom);
            Assert.IsFalse(envModel.IsAuthorizedDeployTo);

            //------------Execute Test---------------------------
            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true).Verifiable();
            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true).Verifiable();
            envModel.AuthorizationServiceMock.Raise(a => a.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            envModel.AuthorizationServiceMock.Verify(a => a.IsAuthorized(AuthorizationContext.DeployFrom, null));
            envModel.AuthorizationServiceMock.Verify(a => a.IsAuthorized(AuthorizationContext.DeployTo, null));
            Assert.IsTrue(envModel.IsAuthorizedDeployFrom);
            Assert.IsTrue(envModel.IsAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherIsNull_False()
        {
            //------------Setup for test--------------------------
            var environment = CreateEqualityEnvironmentModel(Guid.NewGuid(), "Test", new Guid(), "https://myotherserver1:3143");

            //------------Execute Test---------------------------
            var actual = environment.Equals(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherIsSame_True()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment1 = CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);
            var environment2 = CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = environment1.Equals(environment2);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherHasDifferentID_False()
        {
            //------------Setup for test--------------------------
            const string Name = "test";
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";

            var environment1 = CreateEqualityEnvironmentModel(Guid.NewGuid(), Name, serverID, ServerUri);
            var environment2 = CreateEqualityEnvironmentModel(Guid.NewGuid(), Name, serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = environment1.Equals(environment2);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherHasDifferentName_True()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment1 = CreateEqualityEnvironmentModel(resourceID, Name + "1", serverID, ServerUri);
            var environment2 = CreateEqualityEnvironmentModel(resourceID, Name + "1", serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = environment1.Equals(environment2);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Disconnect")]
        public void EnvironmentModel_Disconnect_IsConnected_DoesInvokeDisconnectOnConnection()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.Disconnect()).Verifiable();

            var environment = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            //------------Execute Test---------------------------
            environment.Disconnect();

            //------------Assert Results-------------------------
            connection.Verify(c => c.Disconnect());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Disconnect")]
        public void EnvironmentModel_Disconnect_IsNotConnected_DoesNotInvokeDisconnectOnConnection()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(false);
            connection.Setup(c => c.Disconnect()).Verifiable();

            var environment = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            //------------Execute Test---------------------------
            environment.Disconnect();

            //------------Assert Results-------------------------
            connection.Verify(c => c.Disconnect(), Times.Never());
        }


        [TestMethod]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        [Owner("Leon Rajindrasomething")]
        public void EnvironmentTreeViewModel_PermissionsChanged_MemoIDEqualsEnvironmentServerId_UserPermissionChanges()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            //var connectionServerId = Guid.NewGuid();
            var memoServerID = Guid.NewGuid();

            var pubMemo = new PermissionsModifiedMemo { ServerID = memoServerID };

            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);
            connection.SetupGet(c => c.ServerID).Returns(memoServerID);

            connection.Setup(a => a.DisplayName).Returns("localhost");
            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }
        [TestMethod]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        [Owner("Leon Rajindrasomething")]
        public void EnvironmentTreeViewModel_PermissionsChanged_MemoIDEqualsEnvironmentServerId_UserPermissionChangesNonLocalHost()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var memoServerID = Guid.NewGuid();

            var pubMemo = new PermissionsModifiedMemo { ServerID = memoServerID };

            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);
            connection.SetupGet(c => c.ServerID).Returns(memoServerID);

            connection.Raise(environmentConnection => environmentConnection.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
            connection.Setup(a => a.DisplayName).Returns("bob");
            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }

        static Mock<IEnvironmentConnection> CreateConnection()
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            conn.Setup(connection => connection.DisplayName).Returns("localhost");
            return conn;
        }

        static Server CreateEnvironmentModel(Guid id, IEnvironmentConnection connection)
        {
            var repo = new Mock<IResourceRepository>();

            return new Server(id, connection, repo.Object);
        }

        public static IServer CreateEqualityEnvironmentModel(Guid resourceID, string resourceName, Guid serverID, string serverUri)
        {
            var proxy = new TestEqualityConnection(serverID, serverUri);
            return new Server(resourceID, proxy) { Name = resourceName };
        }
    }

    public class TestEqualityConnection : ServerProxy
    {
        public TestEqualityConnection(Guid serverID, string serverUri)
            : base(serverUri, CredentialCache.DefaultCredentials, new SynchronousAsyncWorker())
        {
            ServerID = serverID;
        }
    }

}
