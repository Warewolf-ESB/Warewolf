
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.Webs;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugOutputViewModelTest
    {
        static Mock<IResourceRepository> _resourceRepo = new Mock<IResourceRepository>();
        private static Mock<IEnvironmentModel> _environmentModel;
        private static IEnvironmentRepository _environmentRepo;
        private static Mock<IWorkspaceItemRepository> _mockWorkspaceRepo;
        private static Mock<IContextualResourceModel> _firstResource;
        private Mock<IContextualResourceModel> _secondResource;
        private static Mock<IEventAggregator> _eventAggregator;
        private static Mock<IResourceDependencyService> _resourceDependencyService;
        private static MainViewModel _mainViewModel;
        private static string _resourceName = "TestResource";
        private static string _displayName = "test2";
        private static string _serviceDefinition = "<x/>";
        private static ImportServiceContext _importServiceContext;
        private static Guid _serverID = Guid.NewGuid();
        private static Guid _workspaceID = Guid.NewGuid();
        private static Guid _firstResourceID = Guid.NewGuid();
        private Guid _secondResourceID = Guid.NewGuid();
        public static Mock<IPopupController> _popupController;
        private static Mock<IFeedbackInvoker> _feedbackInvoker;
        private static Mock<IWebController> _webController;
        private static Mock<IWindowManager> _windowManager;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            CreateFullExportsAndVm();
        }

        [TestMethod]
        public void OpenNullLineItemDoesntStartProcess()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel();
            vm.OpenMoreLink(null);
            Assert.IsNull(vm.ProcessController);
        }

        [TestMethod]
        public void OpenEmptyMoreLinkDoesntStartProcess()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel();

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("");

            vm.OpenMoreLink(lineItem.Object);
            Assert.IsNull(vm.ProcessController);
        }

        [TestMethod]
        public void DebugOutputViewModelCanOpenNonNullOrEmptyMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("More");

            var vm = new DebugOutputViewModel();

            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(true));
        }

        [TestMethod]
        public void DebugOutputViewModelCantOpenEmptyMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel();

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("");
            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(false));
        }

        [TestMethod]
        public void DebugOutputViewModelCantOpenNullMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel();

            Assert.IsTrue(vm.CanOpenMoreLink(null).Equals(false));
        }


        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void DebugOutputViewModelAppendErrorExpectErrorMessageAppende()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);
            mock2.SetupGet(m => m.StateType).Returns(StateType.Append);
            mock2.SetupGet(m => m.HasError).Returns(true);
            mock2.SetupGet(m => m.ErrorMessage).Returns("Error Test");

            mock1.SetupSet(s => s.ErrorMessage).Callback(s => Assert.IsTrue(s.Equals("Error Test")));
            mock1.SetupSet(s => s.HasError).Callback(s => Assert.IsTrue(s.Equals(true)));

            var vm = new DebugOutputViewModel();

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);

            Assert.IsTrue(vm.RootItems.Count == 1);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsTrue(root.HasError.Equals(true));
        }

        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void DebugOutputViewModelAppendNestedDebugstatesExpectNestedInRootItems()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel();
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.IsTrue(vm.RootItems.Count == 1);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;

            Assert.IsTrue(root.Content.Equals(mock1.Object));

            var firstChild = root.Children.First() as DebugStateTreeViewItemViewModel;
            Assert.IsTrue(firstChild.Content.ParentID.Equals(_firstResourceID));
        }

        [TestMethod]
        public void DebugOutputDisplayedExpectsDisplayedToCorrectViewModel()
        {
            ImportService.CurrentContext = _importServiceContext;

            AddAdditionalContext();
            var msg = new DebugWriterWriteMessage
                (DebugStateFactory.Create(_firstResource.Object.ServerID, _firstResource.Object.ID, StateType.Message,
                                          "Test1"));
            var msg2 = new DebugWriterWriteMessage
                (DebugStateFactory.Create(_secondResource.Object.ServerID, _secondResource.Object.ID, StateType.Message,
                                          "Test2"));
            _mainViewModel.Handle(msg);
            _mainViewModel.Handle(msg2);

            var firstctx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var secondctx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);

            var firstDebug = firstctx.DebugOutputViewModel;
            var firstItem = firstDebug.RootItems.First() as DebugStringTreeViewItemViewModel;
            Assert.IsTrue(firstDebug.RootItems.Count == 1 && firstItem.Content == "Test1");

            var secondDebug = secondctx.DebugOutputViewModel;
            var secondItem = secondDebug.RootItems.First() as DebugStringTreeViewItemViewModel;
            Assert.IsTrue(secondDebug.RootItems.Count == 1 && secondItem.Content == "Test2");
        }

        #region PendingQueue

        // BUG 9735 - 2013.06.22 - TWR : added
        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void DebugOutputViewModelPendingQueueExpectedQueuesMessagesAndFlushesWhenFinishedProcessing()
        {
            ImportService.CurrentContext = _importServiceContext;

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.IsLoaded).Returns(true);

            var vm = new DebugOutputViewModel(envRepo.Object) { DebugStatus = DebugStatus.Executing };
            for(var i = 0; i < 10; i++)
            {
                var state = new Mock<IDebugState>();
                var stateType = i % 2 == 0 ? StateType.Message : StateType.After;
                state.Setup(s => s.StateType).Returns(stateType);
                vm.Append(state.Object);
            }

            Assert.AreEqual(5, vm.PendingItemCount);
            Assert.AreEqual(5, vm.ContentItemCount);

            vm.DebugStatus = DebugStatus.Finished;

            Assert.AreEqual(0, vm.PendingItemCount);
            Assert.AreEqual(10, vm.ContentItemCount);
        }

        #endregion



        private static void CreateFullExportsAndVm()
        {
            CreateEnvironmentModel();
            var securityContext = GetMockSecurityContext();
            var environmentRepo = GetEnvironmentRepository();
            var workspaceRepo = GetworkspaceItemRespository();
            _eventAggregator = new Mock<IEventAggregator>();
            _popupController = new Mock<IPopupController>();
            _feedbackInvoker = new Mock<IFeedbackInvoker>();
            _resourceDependencyService = new Mock<IResourceDependencyService>();
            _webController = new Mock<IWebController>();
            _windowManager = new Mock<IWindowManager>();
            _importServiceContext =
                CompositionInitializer.InitializeMockedMainViewModel(securityContext: securityContext,
                                                                     environmentRepo: environmentRepo,
                                                                     workspaceItemRepository: workspaceRepo,
                                                                     aggregator: _eventAggregator,
                                                                     popupController: _popupController,
                                                                     resourceDepService: _resourceDependencyService,
                                                                     feedbackInvoker: _feedbackInvoker,
                                                                     webController: _webController,
                                                                     windowManager: _windowManager);

            ImportService.CurrentContext = _importServiceContext;
            try
            {
                _mainViewModel = new MainViewModel(environmentRepo, false);
            }
            catch(Exception e)
            {

            }
        }


        public static Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.ServiceDefinition).Returns(_serviceDefinition);
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_firstResourceID);

            return result;
        }

        public static Mock<IFrameworkSecurityContext> GetMockSecurityContext()
        {
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.Name).Returns("Test User");
            var mockContext = new Mock<IFrameworkSecurityContext>();
            mockContext.Setup(m => m.UserIdentity).Returns(mockIdentity.Object);
            return mockContext;
        }

        public static IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.IsLoaded).Returns(true);
            mock.Setup(repository => repository.Source).Returns(_environmentModel.Object);
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        private static void CreateEnvironmentModel()
        {
            _environmentModel = CreateMockEnvironment();

            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);

            var channel = new Mock<IStudioClientContext>();
            channel.SetupGet(c => c.WorkspaceID).Returns(_workspaceID);
            channel.SetupGet(c => c.ServerID).Returns(_serverID);

            _environmentModel.SetupGet(s => s.DsfChannel).Returns(channel.Object);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);
            var wizardEngine = new Mock<IWizardEngine>();

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.WizardEngine).Returns(wizardEngine.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));

            return env;
        }

        public static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);

            var eventAggregator = new Mock<IEventAggregator>();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(new Guid());
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            connection.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));

            return connection;
        }

        public static Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            _mockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(_workspaceID);
            item.SetupGet(i => i.ServerID).Returns(_serverID);
            item.SetupGet(i => i.ServiceName).Returns(_resourceName);
            list.Add(item.Object);
            _mockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            _mockWorkspaceRepo.Setup(c => c.Remove(_firstResource.Object)).Verifiable();
            return _mockWorkspaceRepo;
        }

        private void AddAdditionalContext()
        {
            _secondResource = new Mock<IContextualResourceModel>();

            _secondResource.Setup(c => c.ResourceName).Returns("WhoCares");
            _secondResource.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            _secondResource.Setup(c => c.ServiceDefinition).Returns("");
            _secondResource.Setup(c => c.Category).Returns("Testing2");
            _secondResource.Setup(c => c.Environment).Returns(_environmentModel.Object);
            _secondResource.Setup(c => c.ServerID).Returns(_serverID);
            _secondResource.Setup(c => c.ID).Returns(_secondResourceID);

            var msg = new AddWorkSurfaceMessage(_secondResource.Object);
            _mainViewModel.Handle(msg);
        }
    }
}
