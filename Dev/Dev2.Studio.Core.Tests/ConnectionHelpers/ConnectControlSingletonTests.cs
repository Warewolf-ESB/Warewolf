
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ConnectionHelpers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    // ReSharper disable ObjectCreationAsStatement
    public class ConnectControlSingletonTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_StudioResourceRepositoryIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(null, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_AsyncworkerIsNull_ThrowsException()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(studioResourceRepository.Object, null, serverProvider.Object, environmentRepository.Object);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_ServerProviderIsNull_ThrowsException()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, null, environmentRepository.Object);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlSingleton_Constructor_EnvironmentRepositoryIsNull_ThrowsException()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            //------------Execute Test---------------------------
            new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, null);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        public void ConnectControlSingleton_Constructor_ServerProviderReturnsNoEnvironment_WillHaveAtLeastOneEnvironmentLoaded()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>();
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            //------------Execute Test---------------------------
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.IsNotNull(connectControlSingleton.Servers);
            Assert.AreEqual(1, connectControlSingleton.Servers.Count);
            Assert.AreEqual(ConnectControlSingleton.NewServerText, connectControlSingleton.Servers[0].DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Constructor")]
        public void ConnectControlSingleton_Constructor_ServerProviderReturnsOneEnvironment_WillHaveTwoEnvironmentsLoaded()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            //------------Execute Test---------------------------
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
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
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;

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
            Assert.AreEqual(selectedServer.EnvironmentModel.ID, environmentId);
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnection_SelectedServerIndexIsOutofRange_StudioRepositoryLoadIsNotCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
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
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnection_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;

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
            Assert.AreEqual(selectedServer.EnvironmentModel.ID, environmentId);
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnectionOverload_SelectedServerIsDisconnected_StudioRepositoryLoadIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
            var selectedId = selectedServer.EnvironmentModel.ID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Busy, actualConnectedState);
            Assert.IsFalse(actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ToggleConnection")]
        public void ConnectControlSingleton_ToggleConnectionOverload_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
            var selectedId = selectedServer.EnvironmentModel.ID;
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
                actualDoCallback = arg.DoCallback;
                environmentId = arg.EnvironmentId;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ToggleConnection(selectedId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Disconnected, actualConnectedState);
            Assert.AreEqual(true, actualDoCallback);
            Assert.AreEqual(selectedId, environmentId);
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsNotChangedOnTheDialog_StudioResourceRepositoryLoadIsNotCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(e => e.All()).Returns(environmentModels);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
                {
                    selectedIndex = index;
                });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(1, selectedIndex);
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsChangedOnTheDialog_StudioResourceRepositoryLoadIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environments = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false, new Uri("http://azureprivatecloud/machine1:3142")).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false, new Uri("http://azureprivatecloud/machine2:3142")).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
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
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_AuthChangedOnTheDialog_StudioResourceRepositoryLoadIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environments = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Anonymous).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
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
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_AuthNotChangedOnTheDialog_StudioResourceRepositoryLoadIsNotCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false,new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false,new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environments = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false, new Uri("http://localhost:3142/dsf"),AuthenticationType.Public).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            { };
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            { });
            //------------Assert Results-------------------------
        
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Never());
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_ServerUriIsChangedWhenItsConnected_StudioResourceRepositoryDisconnectIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>())).Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var server1 = Guid.NewGuid();
            var server2 = Guid.NewGuid();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environments = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server1, CreateConnection(false, new Uri("http://azureprivatecloud/machine1:3142")).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, server2, CreateConnection(false, new Uri("http://azureprivatecloud/machine2:3142")).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedIndex = -1;
            //------------Execute Test---------------------------
            connectControlSingleton.EditConnection(1, index =>
            {
                selectedIndex = index;
            });
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectControlSingleton);
            Assert.AreEqual(1, selectedIndex);
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_EditConnection")]
        public void ConnectControlSingleton_EditConnection_SelectedServerIndexIsOutOfRange_StudioResourceRepositoryDisconnectIsNotCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>())).Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
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
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Refresh")]
        public void ConnectControlSingleton_Refresh_SelectedServerIsDisconnected_StudioRepositoryLoadIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
            var selectedId = selectedServer.EnvironmentModel.ID;
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
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Refresh")]
        public void ConnectControlSingleton_Refresh_SelectedServerIsConnected_StudioRepositoryLoadIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Connected;
            bool actualDoCallback = false;
            Guid environmentId = Guid.Empty;
            var selectedId = selectedServer.EnvironmentModel.ID;
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
            studioResourceRepository.Verify(s => s.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>(), It.IsAny<Action<Guid>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Remove")]
        public void ConnectControlSingleton_Remove_SelectedServerIsDisconnected_StudioRepositoryRemoveEnvironmentIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>()))
                                      .Verifiable();
            studioResourceRepository.Setup(s => s.RemoveEnvironment(It.IsAny<Guid>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = false;

            Guid environmentId = Guid.Empty;
            var selectedId = selectedServer.EnvironmentModel.ID;
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
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Never());
            studioResourceRepository.Verify(s => s.RemoveEnvironment(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_Remove")]
        public void ConnectControlSingleton_Remove_SelectedServerIsConnected_StudioRepositoryDisconnectIsCalled()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.Disconnect(It.IsAny<Guid>()))
                                      .Verifiable();
            studioResourceRepository.Setup(s => s.RemoveEnvironment(It.IsAny<Guid>()))
                                    .Verifiable();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
            var selectedServer = connectControlSingleton.Servers[1];
            selectedServer.IsConnected = true;

            Guid environmentId = Guid.NewGuid();
            var eventRaised = false;
            var selectedId = selectedServer.EnvironmentModel.ID;
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
            studioResourceRepository.Verify(s => s.Disconnect(It.IsAny<Guid>()), Times.Once());
            studioResourceRepository.Verify(s => s.RemoveEnvironment(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_SetConnectionState")]
        public void ConnectControlSingleton_SetConnectionState_WhenThereIsASubscriber_RaisesAnEvent()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var expectedServerId = Guid.NewGuid();
            var actualId = Guid.Empty;
            const ConnectionEnumerations.ConnectedState expectedConnectedState = ConnectionEnumerations.ConnectedState.Busy;
            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Disconnected;
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);
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

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ResourcesLoadedHandler")]
        public void ConnectControlSingleton_ResourcesLoadedHandler_ServerIdFoundAndIsConnected_RaisesConnectedEventWithConnectedState()
        {
             var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var serverId = Guid.NewGuid();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, serverId, CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            var connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Busy;
         
            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ResourcesLoadedHandler(serverId);
            //------------Assert Results-------------------------
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Connected, actualConnectedState);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ResourcesLoadedHandler")]
        public void ConnectControlSingleton_ResourcesLoadedHandler_ServerIdFoundAndIsNotConnected_RaisesConnectedEventWithDisconnectedState()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var serverId = Guid.NewGuid();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, serverId, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            var connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Busy;

            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ResourcesLoadedHandler(serverId);
            //------------Assert Results-------------------------
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Disconnected, actualConnectedState);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlSingleton_ResourcesLoadedHandler")]
        public void ConnectControlSingleton_ResourcesLoadedHandler_ServerIdIsNotFound_RaisesConnectedEventWithDisconnectedState()
        {
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var serverId = Guid.NewGuid();
            List<IEnvironmentModel> environmentModels = new List<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, serverId, CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
                };
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            var connectControlSingleton = new ConnectControlSingleton(studioResourceRepository.Object, asyncWorker.Object, serverProvider.Object, environmentRepository.Object);

            ConnectionEnumerations.ConnectedState actualConnectedState = ConnectionEnumerations.ConnectedState.Busy;

            connectControlSingleton.ConnectedStatusChanged += (sender, arg) =>
            {
                actualConnectedState = arg.ConnectedStatus;
            };
            //------------Execute Test---------------------------
            connectControlSingleton.ResourcesLoadedHandler(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(ConnectionEnumerations.ConnectedState.Disconnected, actualConnectedState);
        }

        static Mock<IEnvironmentConnection> CreateConnection(bool isConnected)
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            conn.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            conn.Setup(c => c.IsConnected).Returns(isConnected);
            conn.Setup(connection => connection.DisplayName).Returns("localhost");
            return conn;
        }

        static Mock<IEnvironmentConnection> CreateConnection(bool isConnected, Uri uri,AuthenticationType auth = AuthenticationType.Windows)
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
    }
}
