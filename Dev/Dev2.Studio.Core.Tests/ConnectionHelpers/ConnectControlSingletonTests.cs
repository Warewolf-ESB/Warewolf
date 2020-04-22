/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ConnectionHelpers
{
    [TestClass]
    public class ConnectControlSingletonTests
    {

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_ServerProviderIsNull_ThrowsException()
        {
            var environmentRepository = new Mock<IServerRepository>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(null, environmentRepository.Object);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_EnvironmentRepositoryIsNull_ThrowsException()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(serverProvider.Object, null);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        public void ConnectControlSingleton_Constructor_ServerProviderReturnsNoEnvironment_WillHaveAtLeastOneEnvironmentLoaded()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>();
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            //------------Execute Test---------------------------
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.IsNotNull(connectControlSingleton.Servers);
            Assert.AreEqual(1, connectControlSingleton.Servers.Count);
            Assert.AreEqual("localhost", connectControlSingleton.Servers[0].DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        public void ConnectControlSingleton_Constructor_ServerProviderReturnsOneEnvironment_WillHaveTwoEnvironmentsLoaded()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            //------------Execute Test---------------------------
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.IsNotNull(connectControlSingleton.Servers);
            Assert.AreEqual(2, connectControlSingleton.Servers.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnection_SelectedServerIsDisconnected_StudioRepositoryLoadIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;

            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
                {
                    actualConnectedState = arg.ConnectedStatus;
                    actualDoCallback = arg.DoCallback;
                    environmentId = arg.EnvironmentId;
                };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(selectedServer.Server.EnvironmentID, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnection_SelectedServerIndexIsOutofRange_StudioRepositoryLoadIsNotCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var eventWasRaised = false;

            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                eventWasRaised = true;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(99);
            connectControlSingleton.ToggleConnection(-1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);

            Assert.IsFalse(eventWasRaised);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnection_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;

            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Disconnected, actualConnectedState);
            Assert.AreEqual(true, actualDoCallback);
            Assert.AreEqual(selectedServer.Server.EnvironmentID, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnectionOverload_SelectedServerIsDisconnected_StudioRepositoryLoadIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            var selectedId = selectedServer.Server.EnvironmentID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnectionOverload_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            var selectedId = selectedServer.Server.EnvironmentID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Disconnected, actualConnectedState);
            Assert.AreEqual(true, actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsNotChangedOnTheDialog_StudioResourceRepositoryLoadIsNotCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IServerRepository>();
            environmentRepository.Setup(e => e.All()).Returns(environmentModels);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
                {
                    selectedIndex = index;
                });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(1, selectedIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsChangedOnTheDialog_StudioResourceRepositoryLoadIsCalled()
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
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            {
                selectedIndex = index;
            });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(server1, environmentId);
            Assert.AreEqual(1, selectedIndex);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_AuthChangedOnTheDialog_StudioResourceRepositoryLoadIsCalled()
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
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Anonymous).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            {
                selectedIndex = index;
            });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(server1, environmentId);
            Assert.AreEqual(1, selectedIndex);
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_AuthNotChangedOnTheDialog_StudioResourceRepositoryLoadIsNotCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false,new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false,new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IServerRepository>();
            var environments = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            { };
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            { });
            //------------Assert Results-------------------------
        
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsChangedWhenItsConnected_StudioResourceRepositoryDisconnectIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IServerRepository>();
            var environments = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, server1, CommonSetupHelper.CreateConnection(false, new Uri("http://azureprivatecloud/machine1:3142")).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, server2, CommonSetupHelper.CreateConnection(false, new Uri("http://azureprivatecloud/machine2:3142")).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            {
                selectedIndex = index;
            });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(1, selectedIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_SelectedServerIndexIsOutOfRange_StudioResourceRepositoryDisconnectIsNotCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var wasCallbackInvoked = false;
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(99, index =>
            {
                wasCallbackInvoked = true;
            });
            connectControlSingleton.EditConnection(-1, index =>
            {
                wasCallbackInvoked = true;
            });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.IsFalse(wasCallbackInvoked);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Refresh")]
        public void ConnectControlSingleton_Refresh_SelectedServerIsDisconnected_StudioRepositoryLoadIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            var selectedId = selectedServer.Server.EnvironmentID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.Refresh(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Refresh")]
        public void ConnectControlSingleton_Refresh_SelectedServerIsConnected_StudioRepositoryLoadIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            var actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            var actualDoCallback = false;
            var environmentId = Guid.Empty;
            var selectedId = selectedServer.Server.EnvironmentID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.Refresh(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Remove")]
        public void ConnectControlSingleton_Remove_SelectedServerIsDisconnected_StudioRepositoryRemoveEnvironmentIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            var environmentId = Guid.Empty;
            var selectedId = selectedServer.Server.EnvironmentID;
            var eventRaised = false;
            connectControlSingleton.ConnectedServerChanged += (sender, arg) =>
            {
                environmentId = arg.EnvironmentId;
                eventRaised = true;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.Remove(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(Guid.Empty, environmentId);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Remove")]
        public void ConnectControlSingleton_Remove_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IServerRepository>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            var environmentId = Guid.NewGuid();
            var eventRaised = false;
            var selectedId = selectedServer.Server.EnvironmentID;
            connectControlSingleton.ConnectedServerChanged += (sender, arg) =>
            {
                environmentId = arg.EnvironmentId;
                eventRaised = true;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.Remove(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(Guid.Empty, environmentId);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_SetConnectionState")]
        public void ConnectControlSingleton_SetConnectionState_WhenThereIsASubscriber_RaisesAnEvent()
        {
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentModels = new List<IServer>
                {
                    new TestServer(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CommonSetupHelper.CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            var environmentRepository = new Mock<IServerRepository>();
            var expectedServerId = Guid.NewGuid();
            var actualId = Guid.Empty;
            const ConnectionEnumerations.ConnectedState expectedConnectedState = ConnectionEnumerations.ConnectedState.Busy;
            var actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            connectControlSingleton.ConnectedStatusChanged += (s, a) =>
            {
                actualId = a.EnvironmentId;
                actualConnectedState = a.ConnectedStatus;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.SetConnectionState(expectedServerId, expectedConnectedState);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedServerId, actualId);
            Assert.AreEqual(expectedConnectedState, actualConnectedState);
        }
    }
}
