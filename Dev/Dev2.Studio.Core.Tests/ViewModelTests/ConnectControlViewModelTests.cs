using Caliburn.Micro;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.CustomControls.Connections;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    // ReSharper disable ObjectCreationAsStatement
    public class ConnectControlViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_CallbackFunctionIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            //------------Execution-------------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, null, connectControlSingleton.Object, "TEST : ", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_ConnectoControlSingletonIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //------------Execution-------------------------------
            new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, null, "TEST : ", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_LabelTextIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //------------Execution-------------------------------
            new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, null, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_EnvironmentRepositoryIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            //------------Execution-------------------------------
            new ConnectControlViewModel(mainViewModel.Object, null, e => { }, connectControlSingleton.Object, "TEST : ", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_VariablesAreInitialized()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(e => e.All()).Returns((ICollection<IEnvironmentModel>)null);
            //------------Execution-------------------------------
            const string labelText = "TEST : ";
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, labelText, false);
            //------------Assert----------------------------------
            Assert.AreEqual(0, viewModel.SelectedServerIndex);
            Assert.AreEqual(null, viewModel.SelectedServer);
            Assert.AreEqual(false, viewModel.IsConnectButtonSpinnerVisible);
            Assert.AreEqual(true, viewModel.IsDropDownEnabled);
            Assert.AreEqual(true, viewModel.IsEnabled);
            Assert.AreEqual(0, viewModel.Servers.Count);
            Assert.AreEqual(labelText, viewModel.LabelText);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_AddNewServer")]
        public void ConnectControlViewModel_AddNewServer_ResourceRepositoryReturnsNull_False()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            connectControlSingleton.Setup(c => c.ToggleConnection(It.IsAny<int>()))
                                 .Verifiable();
            environmentRepository.Setup(e => e.All()).Returns((ICollection<IEnvironmentModel>)null);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            //------------Execution-------------------------------
            int serverIndex;
            var didAddNew = viewModel.AddNewServer(out serverIndex, i => { });
            //------------Assert----------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(didAddNew);
            Assert.AreEqual(-1, serverIndex);
            Assert.AreEqual(0, viewModel.Servers.Count);
            connectControlSingleton.Verify(c => c.ToggleConnection(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_AddNewServer")]
        public void ConnectControlViewModel_AddNewServer_ResourceRepositoryReturnsAServer_True()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            connectControlSingleton.Setup(c => c.ToggleConnection(It.IsAny<int>()))
                                   .Verifiable();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {
                    new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false, false).Object, new Mock<IResourceRepository>().Object, false)
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            //------------Execution-------------------------------
            int serverIndex;
            var didAddNew = viewModel.AddNewServer(out serverIndex, i => { });
            //------------Assert----------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(didAddNew);
            Assert.AreEqual(0, serverIndex);
            Assert.AreEqual(1, viewModel.Servers.Count);
            connectControlSingleton.Verify(c => c.ToggleConnection(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_AddNewServer")]
        public void ConnectControlViewModel_AddNewServer_ResourceRepositoryReturnExistingServers_False()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(true);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {
                    env1
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            //------------Execution-------------------------------
            int serverIndex;
            var didAddNew = viewModel.AddNewServer(out serverIndex, i => { });
            //------------Assert----------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(didAddNew);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SetTargetEnvironment")]
        public void ConnectControlViewModel_SetTargetEnvironment_HasOneConnectedServerOtherThanLocalhost_SelectedIndexHasChanged()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(true);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            var indexBefore = viewModel.SelectedServerIndex;
            //------------Execution-------------------------------
            viewModel.SetTargetEnvironment();
            //------------Assert----------------------------------
            Assert.AreEqual(0, indexBefore);
            Assert.AreEqual(1, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SetTargetEnvironment")]
        public void ConnectControlViewModel_SetTargetEnvironment_NoConnectedServerOtherThanLocalhost_SelectedIndexNotChanged()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            var indexBefore = viewModel.SelectedServerIndex;
            //------------Execution-------------------------------
            viewModel.SetTargetEnvironment();
            //------------Assert----------------------------------
            Assert.AreEqual(indexBefore, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_UpdateActiveEnvironment")]
        public void ConnectControlViewModel_UpdateActiveEnvironment_ActiveServerHasChangedToAServerOnTheList_SelectedIndexIsChanged()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            Mock<IEnvironmentModel> selectedEnv = new Mock<IEnvironmentModel>();
            selectedEnv.Setup(e => e.ID).Returns(env2Id);
            var indexBefore = viewModel.SelectedServerIndex;
            //------------Execution-------------------------------
            viewModel.UpdateActiveEnvironment(selectedEnv.Object, false);
            //------------Assert----------------------------------
            Assert.AreEqual(1, indexBefore);
            Assert.AreEqual(0, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_UpdateActiveEnvironment")]
        public void ConnectControlViewModel_UpdateActiveEnvironment_ActiveServerHasChangedToAServerNotOnTheList_SelectedIndexNotChanged()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            Mock<IEnvironmentModel> selectedEnv = new Mock<IEnvironmentModel>();
            selectedEnv.Setup(e => e.ID).Returns(Guid.NewGuid);
            var indexBefore = viewModel.SelectedServerIndex;
            //------------Execution-------------------------------
            viewModel.UpdateActiveEnvironment(selectedEnv.Object, false);
            //------------Assert----------------------------------
            Assert.AreEqual(indexBefore, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectedStatusChangedHandler")]
        public void ConnectControlViewModel_ConnectedStatusChangedHandler_SetBusyStateAndEnvironmentIdNotFound_ConnectButtonSpinnerIsNotVisible()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            //------------Execution-------------------------------
            viewModel.ConnectedStatusChanged(null, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, Guid.NewGuid(), true));
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.IsConnectButtonSpinnerVisible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectedStatusChangedHandler")]
        public void ConnectControlViewModel_ConnectedStatusChangedHandler_SetBusyStateAndEnvironmentIdWasFound_ConnectButtonSpinnerIsVisible()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            //------------Execution-------------------------------
            viewModel.ConnectedStatusChanged(null, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, env1Id, true));
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.IsConnectButtonSpinnerVisible);
            Assert.IsFalse(viewModel.IsDropDownEnabled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectedStatusChangedHandler")]
        public void ConnectControlViewModel_ConnectedStatusChangedHandler_SetBusyStateAndEnvironmentIdIsForTheSelectedServer_CallbackHandlerIstCalled()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var wasCallbackHandlerCalled = false;
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { wasCallbackHandlerCalled = true; }, connectControlSingleton.Object, "TEST : ", true);
            viewModel.SelectedServer = viewModel.Servers[0];
            //------------Execution-------------------------------
            viewModel.ConnectedStatusChanged(null, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, env1Id, true));
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(wasCallbackHandlerCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectedStatusChangedHandler")]
        public void ConnectControlViewModel_ConnectedStatusChangedHandler_SetConnectedStateAndEnvironmentIdWasFound_ConnectButtonSpinnerIsNotVisible()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            //------------Execution-------------------------------
            viewModel.ConnectedStatusChanged(null, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Connected, env1Id, false));
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.IsConnectButtonSpinnerVisible);
            Assert.IsTrue(viewModel.IsDropDownEnabled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectedServerChangedHandler")]
        public void ConnectControlViewModel_ConnectedServerChangedHandler_ServerIdFound_SelectedIndexChanged()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            var selectedIndexBefore = viewModel.SelectedServerIndex;
            //------------Execution-------------------------------
            viewModel.ConnectedServerChanged(null, new ConnectedServerChangedEvent(env2Id));
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, selectedIndexBefore);
            Assert.AreEqual(0, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_ConnectCommand")]
        public void ConnectControlViewModel_ConnectCommand_Execute_CallsToggleConnection()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            connectControlSingleton.Setup(c => c.ToggleConnection(It.IsAny<int>())).Verifiable();
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            //------------Execution-------------------------------
            viewModel.ConnectCommand.Execute(null);
            //------------Execution-------------------------------
            Assert.IsNotNull(viewModel);
            connectControlSingleton.Verify(c => c.ToggleConnection(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_EditCommand")]
        public void ConnectControlViewModel_EditCommand_Execute_CallsEditConnection()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                    env1    
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            connectControlSingleton.Setup(c => c.EditConnection(It.IsAny<int>(), It.IsAny<Action<int>>())).Verifiable();
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true);
            //------------Execution-------------------------------
            viewModel.EditCommand.Execute(null);
            //------------Assert------------------------------
            Assert.IsNotNull(viewModel);
            connectControlSingleton.Verify(c => c.EditConnection(It.IsAny<int>(), It.IsAny<Action<int>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SelectedServerIndex")]
        public void ConnectControlViewModel_SelectedServerIndex_SetSelectedIndexToNegativeNumber_DidNotChange()
        {
            TestSelectedIndex(-1, 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SelectedIndex")]
        public void ConnectControlViewModel_SelectedIndex_SetToNewServerAndNewServerWasAdded_SelectedIndexSetToNewServerIndex()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(false, false, ConnectControlSingleton.NewServerText).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = ConnectControlSingleton.NewServerText
            };
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = "AzureOnFire"
            };
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {   
                     new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid() , CreateConnection(false, false).Object, new Mock<IResourceRepository>().Object, false)
                        {
                            Name = "New One"
                        }
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            //------------Execution-------------------------------
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true, (model, type, arg3, arg4, arg5, arg6, arg7) => { }) { SelectedServerIndex = 0 };
            //------------Assert------------------------------
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(2, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_OpenConnectionWizard")]
        public void ConnectControlViewModel_OpenConnectionWizard_IndexIsNegative_OpensWizardForNewConnection()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(false, false, ConnectControlSingleton.NewServerText).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = ConnectControlSingleton.NewServerText
            };
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = "AzureOnFire"
            };
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>();
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var actualEnvironmentId = Guid.Empty.ToString();
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true, (model, type, arg3, arg4, environmentId, arg6, arg7) =>
                {
                    actualEnvironmentId = environmentId;
                });
            //------------Execution-------------------------------
            viewModel.OpenConnectionWizard(-1);
            //------------Assert------------------------------
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(null, actualEnvironmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_OpenConnectionWizard")]
        public void ConnectControlViewModel_OpenConnectionWizard_IndexPositive_OpensWizardForEditConnection()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(false, false, ConnectControlSingleton.NewServerText).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = ConnectControlSingleton.NewServerText
            };
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false)
            {
                Name = "AzureOnFire"
            };
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>();
            environmentRepository.Setup(e => e.All()).Returns(environments);
            var actualEnvironmentId = Guid.Empty.ToString();
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true, (model, type, arg3, arg4, environmentId, arg6, arg7) =>
            {
                actualEnvironmentId = environmentId;
            });
            //------------Execution-------------------------------
            viewModel.OpenConnectionWizard(1);
            //------------Assert------------------------------
            Assert.IsNotNull(viewModel);
            Assert.AreNotEqual(Guid.Empty.ToString(), actualEnvironmentId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_WhenServersCollectionHasLocalHost_SelectedIndexDefaultsToLocalHost()
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, true).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(true);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {
                    env1
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            //------------Execution-------------------------------
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", false);
            //------------Assert----------------------------------
            Assert.AreEqual(1, viewModel.SelectedServerIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SelectedServerIndex")]
        public void ConnectControlViewModel_SelectedServerIndex_SetSelectedIndexToANumberLargerThanServersCollection_DidNotChange()
        {
            TestSelectedIndex(2, 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ConnectControlViewModel_SelectedServerIndex")]
        public void ConnectControlViewModel_SelectedServerIndex_SetSelectedIndexToAValidIndex_DidChangeToThatIndex()
        {
            TestSelectedIndex(1, 1);
        }

        static void TestSelectedIndex(int selectedServerIndex, int expectedVal)
        {
            //------------Setup for test--------------------------
            var mainViewModel = new Mock<IMainViewModel>();
            var activeEnvironment = new Mock<IEnvironmentModel>();
            var env1Id = Guid.NewGuid();
            var env2Id = Guid.NewGuid();
            activeEnvironment.Setup(a => a.ID).Returns(env1Id);
            mainViewModel.Setup(m => m.ActiveEnvironment).Returns(activeEnvironment.Object);
            var env1 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env1Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var env2 = new TestEnvironmentModel(new Mock<IEventAggregator>().Object, env2Id, CreateConnection(true, false).Object, new Mock<IResourceRepository>().Object, false);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var connectControlEnvironments = new ObservableCollection<IConnectControlEnvironment>();
            var controEnv1 = new Mock<IConnectControlEnvironment>();
            var controEnv2 = new Mock<IConnectControlEnvironment>();
            controEnv1.Setup(c => c.EnvironmentModel).Returns(env1);
            controEnv2.Setup(c => c.EnvironmentModel).Returns(env2);
            controEnv1.Setup(c => c.IsConnected).Returns(false);
            connectControlEnvironments.Add(controEnv2.Object);
            connectControlEnvironments.Add(controEnv1.Object);
            connectControlSingleton.Setup(c => c.Servers).Returns(connectControlEnvironments);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> environments = new Collection<IEnvironmentModel>
                {
                    env1
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            //------------Execution-------------------------------
            var viewModel = new ConnectControlViewModel(mainViewModel.Object, environmentRepository.Object, e => { }, connectControlSingleton.Object, "TEST : ", true)
                {
                    SelectedServerIndex = selectedServerIndex
                };
            //------------Assert-------------------------------
            Assert.AreEqual(expectedVal, viewModel.SelectedServerIndex);
        }

        static Mock<IEnvironmentConnection> CreateConnection(bool isConnected, bool isLocalhost, string name = "localhost")
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            conn.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            conn.Setup(c => c.IsConnected).Returns(isConnected);
            conn.Setup(c => c.IsLocalHost).Returns(isLocalhost);
            conn.Setup(connection => connection.DisplayName).Returns(name);
            return conn;
        }
       
    }
}
