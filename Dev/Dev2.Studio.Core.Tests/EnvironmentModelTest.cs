using System;
using System.Network;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    ///This is a result class for EnvironmentModelTest and is intended
    ///to contain all EnvironmentModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnvironmentModelTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Connect Tests

        //5559 Check test
        //[TestMethod]
        //public void TestConnectDefault_Expected_DefaultEnvironmentConnectCalled() {

        //    Mock<IEnvironmentModel> mockEnvironment = new Mock<IEnvironmentModel>();
        //    mockEnvironment.Setup(connect => connect.Connect()).Verifiable();

        //    var env = new EnvironmentModel();
        //    env.DsfAddress = new Uri("http://localhost:77/dsf");
        //    env.Name = "result";
        //    env.WebServerPort = 1234;


        //    Mock<IEnvironmentConnection> envConnection = CreateFakeEnvironmentConnection();
        //    env.EnvironmentConnection = envConnection.Object;
        //    env.Connect();

        //    envConnection.Verify(c => c.Connect(), Times.Once());
        //}

        #endregion Connect Tests

        #region ToSourceDefinition

        [TestMethod]
        public void ToSourceDefinitionExpectedCategoryIsNotServers()
        {
            // BUG: 8786 - TWR - 2013.02.20
            var eventAggregator = new Mock<IEventAggregator>();
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.DisplayName).Returns(() => "TestEnv");
            environmentConnection.Setup(c => c.WebServerUri).Returns(() => new Uri("http://localhost:1234"));
            environmentConnection.Setup(c => c.AppServerUri).Returns(() => new Uri("http://localhost:77/dsf"));

            var envModel = new EnvironmentModel(environmentConnection.Object)
            {
                ID = Guid.NewGuid()
            };
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

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            environmentConnection.Setup(c => c.IsAuxiliary).Returns(false);

            var envModel = new EnvironmentModel(environmentConnection.Object, false);

            environmentConnection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(NetworkState.Connecting, NetworkState.Online));

            eventAggregator.Verify(e => e.Publish(It.IsAny<AbstractEnvironmentMessage>()), Times.Never());
        }


        #endregion

        #region TestConnectionEvents

        static void TestConnectionEvents<TExpectedMessage>(ConnectionEventType eventType, ConnectionEventState eventState, bool isAuxiliary)
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<TExpectedMessage>())).Verifiable();

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            environmentConnection.Setup(c => c.IsAuxiliary).Returns(isAuxiliary);

            var envModel = new EnvironmentModel(environmentConnection.Object, false);

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

    }

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
}
