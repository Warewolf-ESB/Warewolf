
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Deploy;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Threading;
using Dev2.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployViewModelTest : DeployViewModelTestBase
    {
        Mock<IAuthorizationService> _authService;

        [TestInitialize]
        public void Init()
        {
            _authService = new Mock<IAuthorizationService>();
        }

        #region Connect

        [TestMethod]
        public void DeployViewModelConnectWithServerExpectedDoesNotDisconnectOtherServers()
        {
            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = e1.Object;

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = e2.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(repository => repository.Filter(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(new ObservableCollection<IExplorerItemModel>());

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, studioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object);



            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);

            deployViewModel.SelectedSourceServer = s1;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            deployViewModel.SelectedDestinationServer = s2;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DeployViewModel_SelectDestinationServer")]
        public void DeployViewModel_SelectDestinationServer_SelectDestinationServer_CalculateStatsHitOnce()
        {
            //------------Setup for test--------------------------
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>())).Returns(true);
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>())).Returns(true);
            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            e1.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = e1.Object;

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            e2.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = e2.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            Mock<IDeployStatsCalculator> mockDeployStatsCalculator = new Mock<IDeployStatsCalculator>();
            int calcStats;
            mockDeployStatsCalculator.Setup(c => c.CalculateStats(It.IsAny<IEnumerable<IExplorerItemModel>>(), It.IsAny<Dictionary<string, Func<IExplorerItemModel, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out calcStats)).Verifiable();

            Mock<IStudioResourceRepository> mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(new ExplorerItemModel(mockStudioResourceRepository.Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object));
            // ReSharper disable ObjectCreationAsStatement
            new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, mockStudioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object, mockDeployStatsCalculator.Object) { SelectedSourceServer = s1, SelectedDestinationServer = s2 };
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockDeployStatsCalculator.Verify(c => c.CalculateStats(It.IsAny<IEnumerable<IExplorerItemModel>>(), It.IsAny<Dictionary<string, Func<IExplorerItemModel, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out calcStats), Times.Exactly(4));
        }

        #endregion

        #region Deploy

        [TestMethod]
        public void DeployViewModelDeployWithServerExpectedDoesNotDisconnectOtherServers()
        {
            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s1 = e1.Object;
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo1 = new ResourceRepository(e1.Object);
            e1.Setup(e => e.ResourceRepository).Returns(resourceRepo1);
            var r1 = new Mock<IContextualResourceModel>();
            r1.Setup(r => r.Category).Returns("test");
            r1.Setup(r => r.ResourceName).Returns("testResource");
            r1.Setup(r => r.Environment).Returns(e1.Object);
            resourceRepo1.Add(r1.Object);

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s2 = e2.Object;
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo2 = new ResourceRepository(e2.Object);
            e2.Setup(e => e.ResourceRepository).Returns(resourceRepo2);

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var statsCalc = new Mock<IDeployStatsCalculator>();
            IAsyncWorker asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker().Object;
            statsCalc.Setup(s => s.SelectForDeployPredicate(It.IsAny<ExplorerItemModel>())).Returns(true);
            Mock<IStudioResourceRepository> mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(new ExplorerItemModel(mockStudioResourceRepository.Object, asyncWorker, new Mock<IConnectControlSingleton>().Object));
            IStudioResourceRepository studioResourceRepository = mockStudioResourceRepository.Object;
            var deployViewModel = new DeployViewModel(asyncWorker, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, studioResourceRepository, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object, statsCalc.Object)
            {
                SelectedSourceServer = s1,
                SelectedDestinationServer = s2
            };

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);

            deployViewModel.DeployCommand.Execute(null);

            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_AssertSelectedEnvIsDestination()
        {
            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();

            server.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>())).Returns(true);
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>())).Returns(true);
            var secondServer = new Mock<IEnvironmentModel>();
            secondServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            var provider = new Mock<IEnvironmentModelProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();
            var resRepo = new Mock<IResourceRepository>();
            var resRepo2 = new Mock<IResourceRepository>();
            var id = Guid.NewGuid();

            const string expectedResourceName = "Test Resource";
            resourceNode.Setup(res => res.ResourceName).Returns(expectedResourceName);
            resourceNode.Setup(res => res.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceNode.Setup(res => res.ID).Returns(id);

            //Setup Servers
            resRepo.Setup(c => c.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Verifiable();
            resRepo.Setup(c => c.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(resourceNode.Object);
            resRepo.Setup(c => c.DeployResources(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>(),
                                       It.IsAny<IDeployDto>(), It.IsAny<IEventAggregator>())).Verifiable();

            resRepo.Setup(c => c.All()).Returns(new List<IResourceModel>());
            resRepo2.Setup(c => c.All()).Returns(new List<IResourceModel>());

            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            server.Setup(svr => svr.ResourceRepository).Returns(resRepo.Object);

            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            secondServer.Setup(svr => svr.ResourceRepository).Returns(resRepo2.Object);

            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IEnvironmentModel>())).Returns(server.Object);

            provider.Setup(prov => prov.Load()).Returns(new List<IEnvironmentModel> { server.Object, secondServer.Object });


            var initialResource = new Mock<IContextualResourceModel>();
            initialResource.Setup(res => res.Environment).Returns(server.Object);
            initialResource.Setup(res => res.ResourceName).Returns(expectedResourceName);

            //Setup Navigation Tree

            

            //Setup Server Resources


            var mockStudioResourceRepository = GetMockStudioResourceRepository();
            var resourceTreeNode = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object,mockStudioResourceRepository.Object);
            mockStudioResourceRepository.Setup(repository => repository.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(resourceTreeNode);
            var sourceDeployNavigationViewModel = new DeployNavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, mockedServerRepo.Object, mockStudioResourceRepository.Object, true) { Environment = server.Object, ExplorerItemModels = new ObservableCollection<IExplorerItemModel>() };

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object, mockStudioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object)
            {
                Source = sourceDeployNavigationViewModel,
                SelectedSourceServer = server.Object
            };
            int active = 0;
            int getActive = 0;
            resourceTreeNode.IsChecked = true;
            PrivateObject pvt = new PrivateObject(deployViewModel);
            pvt.SetField("_setActive",new Action<IEnvironmentModel>( a=>active++));
            pvt.SetField("_getActive", new Func<IEnvironmentModel> (()=> { getActive++; return  new Mock<IEnvironmentModel>().Object;}));
            //------------Execute Test--------------------------- 
            deployViewModel.DeployCommand.Execute(null);

            Assert.AreEqual(active,2);
            Assert.AreEqual(getActive, 1);

        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasFound_DialogIsShown()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
                {
                    var viewModel = (DeployDialogViewModel)o;
                    viewModel.DialogResult = ViewModelDialogResults.Cancel;
                    isOverwriteMessageDisplayed = true;
                };

            SetupResources(deployStatsCalculator, true);
            deployViewModel.DeployCommand.Execute(null);

            Assert.IsTrue(isOverwriteMessageDisplayed);
            Assert.IsFalse(deployViewModel.DeploySuccessfull);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasFoundAndUserOptsToOverwrite_DeploysSuccessfully()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
            {
                var viewModel = (DeployDialogViewModel)o;
                viewModel.DialogResult = ViewModelDialogResults.Okay;
                isOverwriteMessageDisplayed = true;
            };

            deployViewModel.HasNoResourcesToDeploy = (o, i) => false;

            SetupResources(deployStatsCalculator, true);

            var mockEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            var resourceRepository = new Mock<IResourceRepository>();
            mockEnv.Setup(m => m.ResourceRepository).Returns(resourceRepository.Object);
            mockEnv.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            resourceRepository.Setup(m => m.DeleteResource(It.IsAny<IResourceModel>()));
            deployViewModel.Target.Environment = mockEnv.Object;

            deployViewModel.DeployCommand.Execute(null);

            Assert.IsTrue(isOverwriteMessageDisplayed);
            Assert.IsTrue(deployViewModel.DeploySuccessfull);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasNotFound_DialogIsNotShown()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
            {
                var viewModel = (DeployDialogViewModel)o;
                viewModel.DialogResult = ViewModelDialogResults.Cancel;
                isOverwriteMessageDisplayed = true;
            };

            SetupResources(deployStatsCalculator, false);
            deployViewModel.DeployCommand.Execute(null);

            Assert.IsFalse(isOverwriteMessageDisplayed);
            Assert.IsFalse(deployViewModel.DeploySuccessfull);
        }
        #endregion

        #region AddServerToDeployMessage
        [TestMethod]
        public void DeployViewModel_IsInstanceOfIHandleEnvironmentDeleted()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            SetupVmForMessages(out server, out vm);
            Assert.IsInstanceOfType(vm, typeof(IHandle<EnvironmentDeletedMessage>));
        }

        [TestMethod]
        public void DeployViewModel_EnvironmentDeletedCallsREmoveEnvironmentFromBothSourceAndDestinationNavigationViewModels()
        {
            //Setup
            IEnvironmentModel server;
            DeployViewModel vm;
            SetupVmForMessages(out server, out vm);
            var mockEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            vm.Target.Environment = mockEnv.Object;
            vm.Source.Environment = mockEnv.Object;
            Assert.IsNotNull(vm.Target.Environment);
            Assert.IsNotNull(vm.Source.Environment);

            //Test
            var msg = new EnvironmentDeletedMessage(mockEnv.Object);
            vm.Handle(msg);

            //Assert
            Assert.IsNull(vm.Target.Environment);
            Assert.IsNull(vm.Source.Environment);

        }
        #endregion

        #region SelectItemInDeployMessage

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DeployViewModel_SelectItemInDeploy")]
        public void DeployViewModel_SelectItemInDeploy_TwoServers_ItemAndServerSelected()
        {

            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();
            var secondServer = new Mock<IEnvironmentModel>();
            var provider = new Mock<IEnvironmentModelProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();

            //Setup Servers
            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            server.Setup(a => a.AuthorizationService).Returns(_authService.Object);

            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IEnvironmentModel>())).Returns(server.Object);
            mockedServerRepo.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(server.Object);
            provider.Setup(prov => prov.Load()).Returns(new List<IEnvironmentModel> { server.Object, secondServer.Object });

            //Setup Navigation Tree
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var treeParent = new ExplorerItemModel
            {
                DisplayName = "Test Category",
                ResourceType = Common.Interfaces.Data.ResourceType.Folder,
                IsDeploySourceExpanded = false
            };
            const string expectedResourceName = "Test Resource";
            var resourceID = Guid.NewGuid();
            resourceNode.Setup(res => res.ResourceName).Returns(expectedResourceName);
            resourceNode.Setup(model => model.ID).Returns(resourceID);
            resourceNode.Setup(res => res.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var resourceTreeNode = new ExplorerItemModel
            {
                Parent = treeParent,
                DisplayName = resourceNode.Object.ResourceName,
                ResourceId = resourceID,
                EnvironmentId = server.Object.ID,
                IsChecked = false
            };

            //Setup Server Resources
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(resourceTreeNode);
            mockStudioResourceRepository.Setup(repository => repository.Filter(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(new ObservableCollection<IExplorerItemModel>());

            var sourceDeployNavigationViewModel = new DeployNavigationViewModel(eventAggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, mockedServerRepo.Object, mockStudioResourceRepository.Object, true) { ExplorerItemModels = new ObservableCollection<IExplorerItemModel>() };
            server.Setup(svr => svr.LoadResources()).Callback(() => sourceDeployNavigationViewModel.ExplorerItemModels.Add(treeParent));
            sourceDeployNavigationViewModel.Environment = server.Object;

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object, mockStudioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object)
            {
                Source = sourceDeployNavigationViewModel
            };

            var initialResource = new Mock<IContextualResourceModel>();
            initialResource.Setup(res => res.Environment).Returns(server.Object);
            initialResource.Setup(res => res.ID).Returns(resourceID);

            //------------Execute Test--------------------------- 
            deployViewModel.Handle(new SelectItemInDeployMessage(initialResource.Object.ID, initialResource.Object.Environment.ID));

            // Assert item visible and selected
            Assert.IsTrue(resourceTreeNode.IsChecked.GetValueOrDefault(false), "Deployed item not selected in deploy");
            Assert.IsTrue(treeParent.IsDeploySourceExpanded, "Item not visible in deploy view");

            mockStudioResourceRepository.Verify(r => r.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>()));
        }

        #endregion

        [TestMethod]
        [Description("DeployViewModel CanDeploy must be false if server is disconnected.")]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_UnitTest_CanDeployToDisconnectedServer_ReturnsFalse()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;

            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsTrue(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is false when server is connected.");

            destEnv.Setup(e => e.IsConnected).Returns(false);
            //Changed to true because it was showing the error when not connected which isnt right
            Assert.IsFalse(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is false when server is disconnected.");
        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_SourceServerDisconnected_SourceServerHasDroppedTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            Assert.IsFalse(deployViewModel.SourceServerHasDropped);

            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            mockSourceServer.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.SourceServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Source server has disconnected.", deployViewModel.ServerDisconnectedMessage);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnectedMustClearConflicts")]
        [Owner("Leon Rajindrapersadh")]
        public void DeployViewModel_SourceServerDisconnected_ClearsConflictingNodes()
        {

            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;


            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedDestinationServer = destServer.Object;

            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var target = new PartialNaviationViewModel(deployViewModel.Target);
            deployViewModel.Target = target;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            Assert.IsFalse(deployViewModel.SourceServerHasDropped);

            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            deployViewModel.SourceEnvironmentConnectedChanged(this, connectedEventArgs);

            Assert.IsTrue(target.ClearCalled);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_DestinationServerDisconnected_DestinationServerHasDroppedTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);
            destEnv.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Assert.IsFalse(deployViewModel.DestinationServerHasDropped);
            destEnv.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            destEnv.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.DestinationServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Destination server has disconnected.", deployViewModel.ServerDisconnectedMessage);

        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_SourceAndDestinationServerDisconnected_BothPropertiesTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);
            destEnv.Setup(server => server.IsConnected).Returns(true);
            Assert.IsFalse(deployViewModel.DestinationServerHasDropped);

            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            destEnv.Setup(server => server.IsConnected).Returns(false);
            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            destEnv.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);
            mockSourceServer.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.DestinationServerHasDropped);
            Assert.IsTrue(deployViewModel.SourceServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Source and Destination servers have disconnected.", deployViewModel.ServerDisconnectedMessage);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Hagashen Naidu")]
        public void DeployViewModel_CanDeployWithSameSourceAndDestinationServer_ReturnsFalse()
        {
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;

            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            destServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            destServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = destServer.Object;

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsFalse(deployViewModel.CanDeploy);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_CanDeploy_IsAuthorizedToDeployToFrom_Correct()
        {
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: false, isAuthorizedDeployTo: false);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: false, isAuthorizedDeployTo: true);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: true, isAuthorizedDeployTo: false);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: true, isAuthorizedDeployFrom: true, isAuthorizedDeployTo: true);
        }




        [TestMethod]
        [TestCategory("DeployViewModel_CanSelectAllDependencies")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_CanSelectAllDependencies_IsAuthorizedToDeployFrom_Correct()
        {
            Verify_CanSelectAllDependencies_IsAuthorized(expectedCanSelect: false, isAuthorizedDeployFrom: false);
            Verify_CanSelectAllDependencies_IsAuthorized(expectedCanSelect: true, isAuthorizedDeployFrom: true);
        }

        void Verify_CanSelectAllDependencies_IsAuthorized(bool expectedCanSelect, bool isAuthorizedDeployFrom)
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(isAuthorizedDeployFrom);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            mockDestinationServer.Setup(server => server.IsConnected).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockDestinationServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;

            Assert.AreEqual(expectedCanSelect, deployViewModel.CanSelectAllDependencies);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceServerIsNull_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = null;

            destServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = destServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_DestinationServerIsNull_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.DisplayName).Returns("localhost");
            mockEnvironmentModel.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockEnvironmentModel.Object;
            deployViewModel.SelectedDestinationServer = null;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_DestinationServerNullAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://different"));
            mockDestinationServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceServerNullAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            mockDestinationServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceDestinationServerNotSameAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();

            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            mockDestinationServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceDestinationServerSameAppAddress_False()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockDestinationServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsFalse(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_Constructor")]
        public void DeployViewModel_Constructor_Default()
        {
            //------------Setup for test--------------------------
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);

            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            var deployViewModel = new DeployViewModel();
            //------------Assert Results-------------------------
            Assert.IsNotNull(deployViewModel);
            Assert.IsNotNull(deployViewModel.EnvironmentRepository);
            Assert.IsNotNull(deployViewModel.EventPublisher);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_Constructor")]
        public void DeployViewModel_Constructor_GuidsPassed_SetsValues()
        {
            //------------Setup for test--------------------------
            var a = Dev2MockFactory.SetupEnvironmentModel();
            a.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            IEnvironmentModel environmentModel = a.Object;

            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            // ReSharper restore ObjectCreationAsStatement


            //------------Execute Test---------------------------
            var deployViewModel = new DeployViewModel(Guid.NewGuid(), Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deployViewModel);
            Assert.IsNotNull(deployViewModel.EnvironmentRepository);
            Assert.IsNotNull(deployViewModel.EventPublisher);
            Assert.IsFalse(deployViewModel.DestinationServerHasDropped);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_SourceContext")]
        public void DeployViewModel_SourceContext_NullValue_ExpectNewguid()
        {
            //------------Setup for test--------------------------
            var a = Dev2MockFactory.SetupEnvironmentModel();
            a.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            IEnvironmentModel environmentModel = a.Object;

            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            // ReSharper restore ObjectCreationAsStatement


            //------------Execute Test---------------------------
            var deployViewModel = new DeployViewModel(Guid.NewGuid(), Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deployViewModel.SourceContext);
            Assert.AreNotEqual(deployViewModel.SourceContext, Guid.Empty);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_DestinationContext")]
        public void DeployViewModel_DestinationContext_NullValue_ExpectNewguid()
        {
            //------------Setup for test--------------------------
            var a = Dev2MockFactory.SetupEnvironmentModel();
            a.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            IEnvironmentModel environmentModel = a.Object;

            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            // ReSharper restore ObjectCreationAsStatement


            //------------Execute Test---------------------------
            var deployViewModel = new DeployViewModel(Guid.NewGuid(), Guid.Empty);


            //------------Assert Results-------------------------
            Assert.IsNotNull(deployViewModel.DestinationContext);
            Assert.AreNotEqual(deployViewModel.DestinationContext, Guid.Empty);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Dispose")]
        public void DeployViewModel_Dispose_ExpectUnsubscribeFromEventPublishers()
        {
            //------------Setup for test--------------------------

            var agg = new Mock<IEventAggregator>();

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = e1.Object;

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = e2.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(repository => repository.Filter(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(new ObservableCollection<IExplorerItemModel>());

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, agg.Object, studioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object);



            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);

            deployViewModel.Dispose();
            //------------Assert Results-------------------------
            agg.Verify(ax => ax.Unsubscribe(It.IsAny<object>()));
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectDependencies")]
        public void DeployViewModel_SelectDependencies_NullExplorerItemModel_DoesNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            //------------Execute Test---------------------------
            deployViewModel.SelectDependencies(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectDependencies")]
        public void DeployViewModel_SelectDependencies_ResourceNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(new Mock<IResourceRepository>().Object);
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            ExplorerItemModel explorerItemModel = new ExplorerItemModel { ResourceId = Guid.NewGuid() };
            //------------Execute Test---------------------------
            deployViewModel.SelectDependencies(new List<IExplorerItemModel> { explorerItemModel });
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectDependencies")]
        public void DeployViewModel_SelectDependencies_NoDependencies_NothingSelected()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            destEnv.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            destServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResource = new Mock<IContextualResourceModel>();
            mockResource.Setup(model => model.ResourceName).Returns("resource");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResource.Object);
            mockResourceRepository.Setup(repository => repository.GetDependanciesOnList(It.IsAny<List<IContextualResourceModel>>(), It.IsAny<IEnvironmentModel>(), false)).Returns(new List<string>());
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            mockSourceServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            IExplorerItemModel explorerItemModel;
            IEnvironmentModel environmentModel;
            deployViewModel.Source.ExplorerItemModels = CreateModels(false, out environmentModel, out explorerItemModel).ExplorerItemModels;
            //------------Execute Test---------------------------
            deployViewModel.SelectDependencies(new List<IExplorerItemModel> { explorerItemModel });
            //------------Assert Results-------------------------
            Assert.IsFalse(deployViewModel.Source.ExplorerItemModels[0].IsChecked.GetValueOrDefault(false));
            Assert.IsFalse(deployViewModel.Source.ExplorerItemModels[0].Children[0].IsChecked.GetValueOrDefault(false));
            Assert.IsFalse(deployViewModel.Source.ExplorerItemModels[0].Children[0].Children[0].IsChecked.GetValueOrDefault(false));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectDependencies")]
        public void DeployViewModel_SelectDependencies_DependencyFound_Selected()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel explorerItemModel;
            IEnvironmentModel environmentModel;
            StudioResourceRepository studioResourceRepository = CreateModels(false, out environmentModel, out explorerItemModel);
            explorerItemModel.IsChecked = true;
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResource = new Mock<IContextualResourceModel>();
            mockResource.Setup(model => model.ResourceName).Returns("resource");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResource.Object);
            mockResourceRepository.Setup(repository => repository.GetDependanciesOnList(It.IsAny<List<IContextualResourceModel>>(), It.IsAny<IEnvironmentModel>(), false)).Returns(new List<string> { "TestResource" });
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.Source.ExplorerItemModels = studioResourceRepository.ExplorerItemModels;
            //------------Execute Test---------------------------
            deployViewModel.SelectDependencies(new List<IExplorerItemModel> { explorerItemModel });
            //------------Assert Results-------------------------
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].IsChecked.GetValueOrDefault(false));
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].Children[0].IsChecked.GetValueOrDefault(false));
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].Children[0].Children[0].IsChecked.GetValueOrDefault(false));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectAllDependencies")]
        public void DeployViewModel_SelectAllDependenciesCommand_SourceNull_NothingHappens()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel explorerItemModel;
            IEnvironmentModel environmentModel;
            StudioResourceRepository studioResourceRepository = CreateModels(false, out environmentModel, out explorerItemModel);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResource = new Mock<IContextualResourceModel>();
            mockResource.Setup(model => model.ResourceName).Returns("resource");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResource.Object);
            mockResourceRepository.Setup(repository => repository.GetDependanciesOnList(It.IsAny<List<IContextualResourceModel>>(), It.IsAny<IEnvironmentModel>(), false)).Returns(new List<string> { "TestResource" });
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.Source.ExplorerItemModels = studioResourceRepository.ExplorerItemModels;
            deployViewModel.Source = null;
            //------------Execute Test---------------------------
            deployViewModel.SelectAllDependanciesCommand.Execute(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectAllDependencies")]
        public void DeployViewModel_SelectAllDependenciesCommand_SourceExplorerItemModelsNull_NothingHappens()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel explorerItemModel;
            IEnvironmentModel environmentModel;
            CreateModels(false, out environmentModel, out explorerItemModel);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResource = new Mock<IContextualResourceModel>();
            mockResource.Setup(model => model.ResourceName).Returns("resource");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResource.Object);
            mockResourceRepository.Setup(repository => repository.GetDependanciesOnList(It.IsAny<List<IContextualResourceModel>>(), It.IsAny<IEnvironmentModel>(), false)).Returns(new List<string> { "TestResource" });
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            //------------Execute Test---------------------------
            deployViewModel.SelectAllDependanciesCommand.Execute(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_SelectAllDependencies")]
        public void DeployViewModel_SelectAllDependenciesCommand_DependencyCheckedItem_Selected()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel explorerItemModel;
            IEnvironmentModel environmentModel;
            StudioResourceRepository studioResourceRepository = CreateModels(false, out environmentModel, out explorerItemModel);
            ExplorerItemModel secondResourceToCheck = new ExplorerItemModel();
            explorerItemModel.IsChecked = true;
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            destEnv.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            destServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResource = new Mock<IContextualResourceModel>();
            mockResource.Setup(model => model.ResourceName).Returns("resource");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResource.Object);
            mockResourceRepository.Setup(repository => repository.GetDependanciesOnList(It.IsAny<List<IContextualResourceModel>>(), It.IsAny<IEnvironmentModel>(), false)).Returns(new List<string> { "TestResource" });
            mockSourceServer.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            studioResourceRepository.ExplorerItemModels[0].Children.Add(secondResourceToCheck);
            deployViewModel.Source.ExplorerItemModels = studioResourceRepository.ExplorerItemModels;
            //------------Execute Test---------------------------
            deployViewModel.SelectAllDependanciesCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].IsChecked.GetValueOrDefault(false));
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].Children[0].IsChecked.GetValueOrDefault(false));
            Assert.IsTrue(deployViewModel.Source.ExplorerItemModels[0].Children[0].Children[0].IsChecked.GetValueOrDefault(false));
            Assert.IsFalse(deployViewModel.Source.ExplorerItemModels[0].Children[1].IsChecked.GetValueOrDefault(false));
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_WhenDeployingResource_ResourceRepositoryDeployCalled()
        {
            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();

            server.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>())).Returns(true);
            _authService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>())).Returns(true);
            var secondServer = new Mock<IEnvironmentModel>();
            secondServer.Setup(x => x.AuthorizationService).Returns(_authService.Object);
            var provider = new Mock<IEnvironmentModelProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();
            var resRepo = new Mock<IResourceRepository>();
            var resRepo2 = new Mock<IResourceRepository>();
            var id = Guid.NewGuid();

            const string expectedResourceName = "Test Resource";
            resourceNode.Setup(res => res.ResourceName).Returns(expectedResourceName);
            resourceNode.Setup(res => res.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceNode.Setup(res => res.ID).Returns(id);

            //Setup Servers
            resRepo.Setup(c => c.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Verifiable();
            resRepo.Setup(c => c.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(resourceNode.Object);
            resRepo.Setup(c => c.DeployResources(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>(),
                                       It.IsAny<IDeployDto>(), It.IsAny<IEventAggregator>())).Verifiable();

            resRepo.Setup(c => c.All()).Returns(new List<IResourceModel>());
            resRepo2.Setup(c => c.All()).Returns(new List<IResourceModel>());

            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            server.Setup(svr => svr.ResourceRepository).Returns(resRepo.Object);

            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            secondServer.Setup(svr => svr.ResourceRepository).Returns(resRepo2.Object);

            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IEnvironmentModel>())).Returns(server.Object);

            provider.Setup(prov => prov.Load()).Returns(new List<IEnvironmentModel> { server.Object, secondServer.Object });


            var initialResource = new Mock<IContextualResourceModel>();
            initialResource.Setup(res => res.Environment).Returns(server.Object);
            initialResource.Setup(res => res.ResourceName).Returns(expectedResourceName);

            //Setup Navigation Tree

            var resourceTreeNode = new ExplorerItemModel();

            //Setup Server Resources


            var mockStudioResourceRepository = GetMockStudioResourceRepository();
            mockStudioResourceRepository.Setup(repository => repository.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(resourceTreeNode);
            var sourceDeployNavigationViewModel = new DeployNavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, mockedServerRepo.Object, mockStudioResourceRepository.Object, true) { Environment = server.Object, ExplorerItemModels = new ObservableCollection<IExplorerItemModel>() };

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object, mockStudioResourceRepository.Object, new Mock<IConnectControlViewModel>().Object, new Mock<IConnectControlViewModel>().Object)
            {
                Source = sourceDeployNavigationViewModel,
                SelectedSourceServer = server.Object
            };
            resourceTreeNode.IsChecked = true;
            //------------Execute Test--------------------------- 
            deployViewModel.DeployCommand.Execute(null);

            resRepo.Verify(
                sender =>
                sender.DeployResources(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>(),
                                       It.IsAny<IDeployDto>(), It.IsAny<IEventAggregator>()));
        }

        Mock<IStudioResourceRepository> GetMockStudioResourceRepository()
        {
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            return mockStudioResourceRepository;
        }

        void Verify_CanDeploy_IsAuthorized(bool expectedCanDeploy, bool isAuthorizedDeployFrom, bool isAuthorizedDeployTo)
        {
            var sourceConnection = new Mock<IEnvironmentConnection>();
            sourceConnection.Setup(c => c.AppServerUri).Returns(new Uri("http://localhost"));

            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(e => e.Connection).Returns(sourceConnection.Object);
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(isAuthorizedDeployFrom);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);
            mockSourceServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            var remoteConnection = new Mock<IEnvironmentConnection>();
            remoteConnection.Setup(c => c.AppServerUri).Returns(new Uri("http://remote"));

            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(e => e.Connection).Returns(remoteConnection.Object);
            mockDestinationServer.Setup(server => server.IsConnected).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployTo).Returns(isAuthorizedDeployTo);
            mockDestinationServer.Setup(a => a.AuthorizationService).Returns(_authService.Object);
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;

            Assert.AreEqual(expectedCanDeploy, deployViewModel.CanDeploy);
        }
    }

    public class PartialNaviationViewModel : DeployNavigationViewModel
    {
        public bool ClearCalled { get; set; }

        public PartialNaviationViewModel(DeployNavigationViewModel model)
            : base(model.EventAggregator, model.AsyncWorker, model.EnvironmentRepository, model.StudioResourceRepository, true)
        {

        }

        public override void ClearConflictingNodesNodes()
        {
            ClearCalled = true;
        }
    }
}
