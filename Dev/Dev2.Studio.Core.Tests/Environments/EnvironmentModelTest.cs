using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Network;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore]//Ashley: 15-11-2013 background nullref exception in navigationviewmodel load resources async during unit test run in environment (round 1)
    public class EnvironmentModelTest
    {
        bool _wasCalled;

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModelConstructorWithNullConnectionExpectedThrowsArgumentNullException()
        {
            //var wizard = new Mock<IWizardEngine>();
            var env = new EnvironmentModel(Guid.NewGuid(), null);
        }

        [TestMethod]
        public void EnvironmentModelConstructorWithConnectionAndWizardEngineExpectedInitializesConnectionAndResourceRepository()
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
        public void EnvironmentModelConstructorWithConnectionAndNullResourceRepositoryExpectedThrowsArgumentNullException()
        {
            var connection = CreateConnection();
            var env = new EnvironmentModel(Guid.NewGuid(), connection.Object, (IResourceRepository)null);
        }

        [TestMethod]
        public void EnvironmentModelConstructorWithConnectionAndResourceRepositoryExpectedInitializesConnectionAndResourceRepository()
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
	    [TestCategory("EnvironmentModel_Constructor")]
        [Description("EnvironmentModel ResourceRepository initializes with a wizard engine")]
	    [Owner("Ashley Lewis")]
	    // ReSharper disable InconsistentNaming
        public void EnvironmentModel_UnitTest_Constructor_ResourceRepositoryInitializedCorrectly()
	    // ReSharper restore InconsistentNaming
	    {
            //Isolate EnvironmentModel ResourceRepository as a functional unit
            var env = new EnvironmentModel(Guid.NewGuid(), CreateConnection().Object);
		
		    //Assert Expected
		    Assert.IsNotNull(env.ResourceRepository.WizardEngine);
	    }

        #endregion

        #region Connect

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnvironmentModelConnectWithNoNameExpectedThrowsArgumentException()
        {
            var connection = CreateConnection();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();
        }

        [TestMethod]
        public void EnvironmentModelConnectWithNameExpectedInvokesEnvironmentConnection()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.Connect(It.IsAny<bool>())).Verifiable();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect();

            connection.Verify(c => c.Connect(It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentModelConnectOtherWithNullExpectedThrowsArgumentNullException()
        {
            var connection = CreateConnection();

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            env.Connect(null);
        }

        [TestMethod]
        public void EnvironmentModelConnectOtherWithNonNullAndConnectedExpectedDoesNotInvokeOthersConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<bool>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect(It.IsAny<bool>())).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c2.Verify(c => c.Connect(It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        public void EnvironmentModelConnectOtherWithNonNullAndConnectedExpectedInvokesThisConnect()
        {
            var c1 = CreateConnection();
            c1.Setup(c => c.DisplayName).Returns("Test");
            c1.Setup(c => c.Connect(It.IsAny<bool>())).Verifiable();

            var c2 = CreateConnection();
            c2.Setup(c => c.DisplayName).Returns("Other");
            c2.Setup(c => c.Connect(It.IsAny<bool>())).Verifiable();
            c2.Setup(c => c.IsConnected).Returns(true);

            var e1 = CreateEnvironmentModel(Guid.NewGuid(), c1.Object);
            var e2 = CreateEnvironmentModel(Guid.NewGuid(), c2.Object);

            e1.Connect(e2);

            c1.Verify(c => c.Connect(It.IsAny<bool>()), Times.Once());
        }

        #endregion

        #region ToSourceDefinition

        [TestMethod]
        public void EnvironmentModelToSourceDefinitionExpectedCategoryIsNotServers()
        {
            // BUG: 8786 - TWR - 2013.02.20
            var eventAggregator = new Mock<IEventAggregator>();
            var environmentConnection = CreateConnection();
            environmentConnection.Setup(c => c.DisplayName).Returns(() => "TestEnv");
            environmentConnection.Setup(c => c.WebServerUri).Returns(() => new Uri("http://localhost:1234"));
            environmentConnection.Setup(c => c.AppServerUri).Returns(() => new Uri("http://localhost:77/dsf"));

            var envModel = CreateEnvironmentModel(Guid.NewGuid(), environmentConnection.Object);
            var sourceDef = envModel.ToSourceDefinition();
            var sourceXml = XElement.Parse(sourceDef);
            var category = sourceXml.ElementSafe("Category").ToUpper();
            Assert.AreNotEqual("SERVERS", category);
        }

        #endregion

        #region ServerStateChanged

        // PBI 9228: TWR - 2013.04.17

        [TestMethod]
        public void ServerStateChangedEventWhenOfflineExpectedPublishesEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents<EnvironmentDisconnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Offline, false);
        }

        [TestMethod]
        public void ServerStateChangedEventWhenOnlineExpectedPublishesEnvironmentConnectedMessage()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Online, false);
        }

        [TestMethod]
        public void ServerStateChangedEventWhenAuxiliaryConnectionExpectedDoesNothing()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Online, true);
        }

        [TestMethod]
        public void LoginStateChangedEventWhenOfflineExpectedPublishesEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents<EnvironmentDisconnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Offline, false);
        }

        [TestMethod]
        public void LoginStateChangedEventWhenOnlineExpectedPublishesEnvironmentConnectedMessage()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Online, false);
        }

        [TestMethod]
        public void LoginStateChangedEventWhenAuxiliaryConnectionExpectedDoesNothing()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Online, true);
        }

        [TestMethod]
        public void NetworkStateChangedEventExpectedDoesNotPublishEnvironmentMessages()
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<AbstractEnvironmentMessage>())).Verifiable();
            EventPublishers.Aggregator = eventAggregator.Object;
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.IsAuxiliary).Returns(false);
            environmentConnection.Setup(connection => connection.ServerEvents).Returns(EventPublishers.Studio);

            var envModel = CreateEnvironmentModel(EventPublishers.Aggregator,Guid.NewGuid(), environmentConnection.Object);

            environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, NetworkState.Online));

            eventAggregator.Verify(e => e.Publish(It.IsAny<AbstractEnvironmentMessage>()), Times.Never());
        }

        EnvironmentModel CreateEnvironmentModel(IEventAggregator aggregator, Guid id, IEnvironmentConnection environmentConnection)
        {
            var wizard = new Mock<IWizardEngine>();

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.WizardEngine).Returns(wizard.Object);

            return new EnvironmentModel(aggregator,id, environmentConnection, repo.Object, false);
        }

        #endregion

        #region ServerStateChanged

        // PBI 9228: TWR - 2013.04.17

        [TestMethod]
        public void EnvironmentModelServerStateChangedEventWhenOfflineExpectedPublishesEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents<EnvironmentDisconnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Offline, false);
        }

        [TestMethod]
        public void EnvironmentModelServerStateChangedEventWhenOnlineExpectedPublishesEnvironmentConnectedMessage()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Online, false);
        }

        [TestMethod]
        public void EnvironmentModelServerStateChangedEventWhenAuxiliaryConnectionExpectedDoesNothing()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.ServerState, ConnectionEventState.Online, true);
        }

        [TestMethod]
        public void EnvironmentModelLoginStateChangedEventWhenOfflineExpectedPublishesEnvironmentDisconnectedMessage()
        {
            TestConnectionEvents<EnvironmentDisconnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Offline, false);
        }

        [TestMethod]
        public void EnvironmentModelLoginStateChangedEventWhenOnlineExpectedPublishesEnvironmentConnectedMessage()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Online, false);
        }

        [TestMethod]
        public void EnvironmentModelLoginStateChangedEventWhenAuxiliaryConnectionExpectedDoesNothing()
        {
            TestConnectionEvents<EnvironmentConnectedMessage>(ConnectionEventType.LoginState, ConnectionEventState.Online, true);
        }

        #endregion

        #region TestConnectionEvents

        static void TestConnectionEvents<TExpectedMessage>(ConnectionEventType eventType, ConnectionEventState eventState, bool isAuxiliary)
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<TExpectedMessage>())).Verifiable();
            EventPublishers.Aggregator = eventAggregator.Object;
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.IsAuxiliary).Returns(isAuxiliary);
            environmentConnection.Setup(connection => connection.ServerEvents).Returns(EventPublishers.Studio);
            var repo = new Mock<IResourceRepository>();
            var envModel = new EnvironmentModel(EventPublishers.Aggregator,Guid.NewGuid(), environmentConnection.Object, repo.Object, false);

            envModel.IsConnectedChanged += (sender, args) =>
            {
                Assert.AreEqual(eventState == ConnectionEventState.Online, args.IsConnected);
            };

            switch(eventType)
            {
                case ConnectionEventType.ServerState:
                    environmentConnection.Raise(c => c.ServerStateChanged += null, new ServerStateEventArgs(eventState == ConnectionEventState.Online ? ServerState.Online : ServerState.Offline));
                    break;
                case ConnectionEventType.NetworkState:
                    environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, eventState == ConnectionEventState.Online ? NetworkState.Online : NetworkState.Offline));
                    break;
                case ConnectionEventType.LoginState:
                    environmentConnection.Raise(c => c.LoginStateChanged += null, new LoginStateEventArgs(eventState == ConnectionEventState.Online ? AuthenticationResponse.Success : AuthenticationResponse.Logout, false));
                    break;
            }

            if(isAuxiliary)
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentConnectedMessage>()), Times.Never());
                eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDisconnectedMessage>()), Times.Never());
            }
            else
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<TExpectedMessage>()), Times.Once());
            }
        }

        #endregion

        #region LoadResources

        [TestMethod]
        public void EnvironmentModelForceLoadResourcesExpectedInvokesForceLoadOnResourceRepository()
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
        public void EnvironmentModelLoadResourcesWithShouldLoadTrueExpectedInvokesLoadOnResourceRepository()
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
        public void EnvironmentModelLoadResourcesWithShouldLoadFalseExpectedNotInvokeLoadOnResourceRepository()
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

        #endregion

        #region LoginStateChanged

        [TestMethod]
        public void EnvironmentModelLoginStateChangedWithLoginExpectedPublishesEnvironmentConnectedMessage()
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.Is<EnvironmentConnectedMessage>(m => m != null))).Verifiable();

            EventPublishers.Aggregator = eventAggregator.Object;

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            connection.Raise(c => c.LoginStateChanged += null, new LoginStateEventArgs(AuthenticationResponse.Success, true, false, string.Empty));

            eventAggregator.Verify(e => e.Publish(It.Is<EnvironmentConnectedMessage>(m => m != null)));

        }

        [TestMethod]
        public void EnvironmentModelLoginStateChangedWithLogoutExpectedPublishesEnvironmentDisconnectedMessage()
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.Is<EnvironmentDisconnectedMessage>(m => m != null))).Verifiable();

            EventPublishers.Aggregator = eventAggregator.Object;

            var connection = CreateConnection();
            connection.Setup(c => c.DisplayName).Returns("Test");
            connection.Setup(c => c.IsConnected).Returns(true);

            var env = CreateEnvironmentModel(Guid.NewGuid(), connection.Object);

            connection.Raise(c => c.LoginStateChanged += null, new LoginStateEventArgs(AuthenticationResponse.InvalidCredentials, false, false, string.Empty));

            eventAggregator.Verify(e => e.Publish(It.Is<EnvironmentDisconnectedMessage>(m => m != null)));

        }

        #endregion

        #region NetworkStateChanged

        [TestMethod]
        public void EnvironmentModelNetworkStateChangedEventExpectedDoesNotPublishEnvironmentMessages()
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<IMessage>())).Verifiable();
            EventPublishers.Aggregator = eventAggregator.Object;
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.IsAuxiliary).Returns(false);
            environmentConnection.Setup(connection => connection.ServerEvents).Returns(EventPublishers.Studio);

            var repo = new Mock<IResourceRepository>();
            var envModel = new EnvironmentModel(EventPublishers.Aggregator,Guid.NewGuid(), environmentConnection.Object, repo.Object, false);

            environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, NetworkState.Online));

            eventAggregator.Verify(e => e.Publish(It.IsAny<AbstractEnvironmentMessage>()), Times.Never());
        }

        #endregion

        #region CreateConnection

        static Mock<IEnvironmentConnection> CreateConnection()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            return conn;
        }

        #endregion

        #region CreateEnvironmentModel

        static EnvironmentModel CreateEnvironmentModel(Guid id, IEnvironmentConnection connection)
        {
            var wizard = new Mock<IWizardEngine>();

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.WizardEngine).Returns(wizard.Object);

            return new EnvironmentModel(id, connection, repo.Object, false);
        }

        #endregion

        #region IsLocalHost

        [TestMethod]
        public void IsLocalHost()
        {
            var conn = CreateConnection();
            conn.Setup(c => c.DisplayName).Returns("localhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost();
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod]
        public void IsNotLocalHost()
        {
            var conn = CreateConnection();
            conn.Setup(c => c.DisplayName).Returns("notlocalhost");
            var env = CreateEnvironmentModel(Guid.NewGuid(), conn.Object);
            var isLocalHost = env.IsLocalHost();
            Assert.IsFalse(isLocalHost);
        }
        #endregion

        #region Enums

        internal enum ConnectionEventType
        {
            ServerState,
            NetworkState,
            LoginState
        }

        internal enum ConnectionEventState
        {
            Online,
            Offline
        }

        #endregion

    }
}
