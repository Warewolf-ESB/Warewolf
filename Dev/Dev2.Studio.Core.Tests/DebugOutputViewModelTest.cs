using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Enums;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Core.Workspaces;
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
        private Mock<IEventAggregator> _eventAggregator;
        private Mock<IResourceDependencyService> _resourceDependencyService;
        MainViewModel _mainViewModel;
        private string _resourceName = "TestResource";
        private string _displayName = "test2";
        private string _serviceDefinition = "<x/>";
        private static ImportServiceContext _importServiceContext;
        private Guid _serverID = Guid.NewGuid();
        private Guid _workspaceID = Guid.NewGuid();
        private Guid _firstResourceID = Guid.NewGuid();
        private Guid _secondResourceID = Guid.NewGuid();
        public Mock<IPopupController> _popupController;
        private Mock<IFeedbackInvoker> _feedbackInvoker;
        private Mock<IWebController> _webController;
        private Mock<IWindowManager> _windowManager;

        [TestMethod]
        public void DebugOutputDisplayedExpectsDisplayedToCorrectViewModel()
        {
            CreateFullExportsAndVm();
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

        private void CreateFullExportsAndVm()
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
            _mainViewModel = new MainViewModel(environmentRepo, false);
        }


        public Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
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

        public Mock<IFrameworkSecurityContext> GetMockSecurityContext()
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
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        private void CreateEnvironmentModel()
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
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            connection.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));

            return connection;
        }

        public Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
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
