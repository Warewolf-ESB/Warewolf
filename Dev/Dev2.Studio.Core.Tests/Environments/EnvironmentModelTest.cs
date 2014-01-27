using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Network;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common.Common;
using Dev2.Core.Tests.Network;
using Dev2.Network;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EnvironmentModelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModel_Constructor_NullConnection_ThrowsArgumentNullException()
        {
            //var wizard = new Mock<IWizardEngine>();
            var env = new EnvironmentModel(Guid.NewGuid(), null);
        }

        [TestMethod]
        public void EnvironmentModel_Constructor_ConnectionAndWizardEngine_InitializesConnectionAndResourceRepository()
        {
            //// Needed for ResourceRepository!
            //var wizardEngine = new Mock<IWizardEngine>();
            //var importServiceContext = new ImportServiceContext();
            //ImportService.CurrentContext = importServiceContext;
            //ImportService.Initialize(new List<ComposablePartCatalog>());
            //ImportService.AddExportedValueToContainer(wizardEngine.Object);

            //var wizard = new Mock<IWizardEngine>();

            var connection = CreateConnection();
            //, wizard.Object
            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object);
            Assert.IsNotNull(env.Connection);
            Assert.IsNotNull(env.ResourceRepository);
            Assert.AreSame(connection.Object, env.Connection);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModel_Constructor_ConnectionAndNullResourceRepository_ThrowsArgumentNullException()
        {
            var connection = CreateConnection();
            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, null);
        }

        [TestMethod]
        public void EnvironmentModel_Constructor_ConnectionAndResourceRepository_InitializesConnectionAndResourceRepository()
        {
            var connection = CreateConnection();
            var repo = new Mock<IResourceRepository>();
            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, repo.Object);

            Assert.IsNotNull(env.Connection);
            Assert.IsNotNull(env.ResourceRepository);
            Assert.AreSame(connection.Object, env.Connection);
            Assert.AreSame(repo.Object, env.ResourceRepository);
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
            connection.Setup(c => c.Connect()).Verifiable();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();

            connection.Verify(c => c.Connect(), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_Connect_IsConnected_DoesNotInvokeConnection()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.Connect()).Verifiable();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();

            connection.Verify(c => c.Connect(), Times.Never());
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
            c1.Setup(c => c.Connect()).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect()).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c2.Verify(c => c.Connect(), Times.Never());
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_ConnectOther_NonNullAndConnected_InvokesThisConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect()).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect()).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c1.Verify(c => c.Connect(), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        public void EnvironmentModel_ConnectOther_NonNullAndNotConnected_InvokesOtherConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect()).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.IsConnected).Returns(false);
            c2.Setup(c => c.Connect()).Callback(() => c2.Setup(c => c.IsConnected).Returns(true)).Verifiable();

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c2.Verify(c => c.Connect(), Times.Once());
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Connect")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnvironmentModel_ConnectOther_NonNullAndNotConnectedFails_ThrowsInvalidOperationException()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect()).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.IsConnected).Returns(false);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);
        }

        [TestMethod]
        public void EnvironmentModel_ToSourceDefinition_CategoryIsNotServers()
        {
            // BUG: 8786 - TWR - 2013.02.20
            var eventAggregator = new Mock<IEventAggregator>();
            var environmentConnection = CreateConnection();
            environmentConnection.Setup(c => c.DisplayName).Returns(() => "TestEnv");
            environmentConnection.Setup(c => c.WebServerUri).Returns(() => new Uri("http://localhost:1234"));
            environmentConnection.Setup(c => c.AppServerUri).Returns(() => new Uri("http://localhost:77/dsf"));

            var envModel = CreateEnvironmentModel(Guid.NewGuid(), environmentConnection.Object);
            var sourceDef = envModel.ToSourceDefinition().ToString();
            var sourceXml = XElement.Parse(sourceDef);
            var category = sourceXml.ElementSafe("Category").ToUpper();
            Assert.AreNotEqual("SERVERS", category);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_NetworkStateChanged")]
        public void EnvironmentModel_NetworkStateChanged_Offline_DoesPublishEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents<EnvironmentDisconnectedMessage>(NetworkState.Offline);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_NetworkStateChanged")]
        public void EnvironmentModel_NetworkStateChanged_Online_DoesPublishEnvironmentConnectedMessage()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(NetworkState.Online);
        }

        static void TestConnectionEvents<TMessage>(NetworkState toState)
        {
            var eventAggregator = new Mock<IEventAggregator>();
            if(toState == NetworkState.Online)
            {
                eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentConnectedMessage>())).Verifiable();
            }
            else
            {
                eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDisconnectedMessage>())).Verifiable();
            }

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(connection => connection.IsConnected).Returns(true);
            environmentConnection.Setup(connection => connection.ServerEvents).Returns(EventPublishers.Studio);

            var repo = new Mock<IResourceRepository>();
            var envModel = new EnvironmentModel(eventAggregator.Object, Guid.NewGuid(), environmentConnection.Object, repo.Object, false);

            envModel.IsConnectedChanged += (sender, args) => Assert.AreEqual(toState == NetworkState.Online, args.IsConnected);

            environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, toState));

            if(toState == NetworkState.Online)
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentConnectedMessage>()));
            }
            else
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDisconnectedMessage>()));
            }
        }

        [TestMethod]
        public void EnvironmentModel_ForceLoadResources_InvokesForceLoadOnResourceRepository()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.ForceLoad()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, resourceRepo.Object);

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

            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, resourceRepo.Object);

            Assert.IsTrue(env.CanStudioExecute);

            env.LoadResources();

            resourceRepo.Verify(r => r.UpdateWorkspace(It.IsAny<List<IWorkspaceItem>>()), Times.Once());
        }

        [TestMethod]
        public void EnvironmentModel_LoadResources_ShouldLoadFalse_NotInvokeLoadOnResourceRepository()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Load()).Verifiable();

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, resourceRepo.Object);

            env.CanStudioExecute = false;

            env.LoadResources();

            resourceRepo.Verify(r => r.Load(), Times.Never());
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_IsLocalHost")]
        public void EnvironmentModel_IsLocalHost_IsLocalHost_True()
        {
            var conn = CreateConnection();
            conn.SetupProperty(c => c.DisplayName, "localhost");
            conn.Setup(connection => connection.IsLocalHost).Returns(conn.Object.DisplayName == "localhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost();
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod]
        [TestCategory("EnvironmentModel_IsLocalHost")]
        public void EnvironmentModel_IsLocalHost_IsNotLocalHost_False()
        {
            var conn = CreateConnection();
            conn.Setup(c => c.DisplayName).Returns("notlocalhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost();
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
            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object);

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

            var envModel = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);
            Assert.IsFalse(envModel.IsAuthorized);

            //------------Execute Test---------------------------
            connection.Setup(c => c.IsAuthorized).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(envModel.IsAuthorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_AuthorizationService")]
        public void EnvironmentModel_AuthorizationService_PermissionsChanged_IsAuthorizedDeployToAndIsAuthorizedDeployFromChanged()
        {
            //------------Setup for test--------------------------
            var connection = CreateConnection();

            var envModel = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), connection.Object, new Mock<IResourceRepository>().Object, false);

            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true).Verifiable();
            envModel.AuthorizationServiceMock.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true).Verifiable();

            Assert.IsFalse(envModel.IsAuthorizedDeployFrom);
            Assert.IsFalse(envModel.IsAuthorizedDeployTo);

            //------------Execute Test---------------------------
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
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherHasDifferentServerID_False()
        {
            //------------Setup for test--------------------------
            const string Name = "test";
            var resourceID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";

            var environment1 = CreateEqualityEnvironmentModel(resourceID, Name, Guid.NewGuid(), ServerUri);
            var environment2 = CreateEqualityEnvironmentModel(resourceID, Name, Guid.NewGuid(), ServerUri);

            //------------Execute Test---------------------------
            var actual = environment1.Equals(environment2);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModel_Equals")]
        public void EnvironmentModel_Equals_OtherHasDifferentServerUri_False()
        {
            //------------Setup for test--------------------------
            const string Name = "test";
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            var environment1 = CreateEqualityEnvironmentModel(resourceID, Name, serverID, "https://myotherserver1:3143");
            var environment2 = CreateEqualityEnvironmentModel(resourceID, Name, serverID, "https://myotherserver2:3143");

            //------------Execute Test---------------------------
            var actual = environment1.Equals(environment2);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
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

        static Mock<IEnvironmentConnection> CreateConnection()
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            return conn;
        }

        static EnvironmentModel CreateEnvironmentModel(Guid id, IEnvironmentConnection connection)
        {
            var repo = new Mock<IResourceRepository>();

            return new EnvironmentModel(id, connection, repo.Object, false);
        }

        //[TestMethod]
        //[Owner("Trevor Williams-Ros")]
        //[TestCategory("ConnectControlViewModel_Servers")]
        //public void EnvironmentModel_Equality_Diction_ItemWithSameKeyAdded_NotAddedAndNoExceptionThrown()
        //{
        //    //------------Setup for test--------------------------
        //    var serverID = Guid.NewGuid();
        //    const string ServerUri = "https://myotherserver:3143";

        //    var servers = new List<IEnvironmentModel>
        //    {
        //        CreateConnectControlServer(Guid.Empty, "Server1", serverID, ServerUri),
        //        CreateConnectControlServer(Guid.Empty, "Server2", serverID, ServerUri),
        //    };

        //    var testDictionary = new Dictionary<IEnvironmentModel, IEnvironmentModel>();
        //    testDictionary.Add(servers[0], servers[0]);


        //    //------------Execute Test---------------------------
        //    testDictionary.Add(servers[1], servers[1]);

        //    //------------Assert Results-------------------------
        //}

        public static IEnvironmentModel CreateEqualityEnvironmentModel(Guid resourceID, string resourceName, Guid serverID, string serverUri)
        {
            // See .. EnvironmentRepository.CreateEnvironmentModel()
            var proxy = new TestEqualityConnection(serverID, serverUri);
            return new EnvironmentModel(resourceID, proxy) { Name = resourceName };
        }
    }

    public class TestEqualityConnection : ServerProxy
    {
        public TestEqualityConnection(Guid serverID, string serverUri)
            : base(serverUri)
        {
            ServerID = serverID;
        }
    }
}
