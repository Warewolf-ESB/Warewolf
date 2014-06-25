using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels;
using Dev2.Threading;
using Dev2.Webs;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Core.Tests
{
    public abstract class MainViewModelBase
    {
        #region Variables

        protected readonly Guid FirstResourceID = Guid.NewGuid();
        protected readonly Guid SecondResourceID = Guid.NewGuid();
        protected readonly Guid ServerID = Guid.NewGuid();
        protected readonly Guid WorkspaceID = Guid.NewGuid();
        protected const string DisplayName = "test2";
        protected Mock<IEnvironmentConnection> EnvironmentConnection;
        protected Mock<IEnvironmentModel> EnvironmentModel;
        protected IEnvironmentRepository EnvironmentRepo;
        protected Mock<IEventAggregator> EventAggregator;
        protected Mock<IFeedbackInvoker> FeedbackInvoker;
        protected Mock<IContextualResourceModel> FirstResource;
        protected MainViewModel MainViewModel;
        protected Mock<IWorkspaceItemRepository> MockWorkspaceRepo;
        public Mock<IPopupController> PopupController;
        protected const string ResourceName = "TestResource";
        protected Mock<IResourceRepository> ResourceRepo;
        protected Mock<IContextualResourceModel> SecondResource;
        protected const string ServiceDefinition = "<x/>";
        protected Mock<IWebController> WebController;
        protected Mock<IWindowManager> WindowManager;
        protected Mock<IAuthorizationService> AuthorizationService;
        protected Mock<IEnvironmentModel> ActiveEnvironment;
        protected Mock<IStudioResourceRepository> MockStudioResourceRepository;

        #endregion Variables

        #region Methods used by tests

        protected void CreateFullExportsAndVmWithEmptyRepo()
        {
            CreateResourceRepo();
            var mockEnv = new Mock<IEnvironmentRepository>();
            mockEnv.SetupProperty(g => g.ActiveEnvironment); // Start tracking changes
            mockEnv.Setup(g => g.All()).Returns(new List<IEnvironmentModel>());
            mockEnv.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            mockEnv.Setup(repository => repository.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var environmentRepo = mockEnv.Object;

            EventAggregator = new Mock<IEventAggregator>();
            PopupController = new Mock<IPopupController>();
            FeedbackInvoker = new Mock<IFeedbackInvoker>();
            WebController = new Mock<IWebController>();
            WindowManager = new Mock<IWindowManager>();
            MockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            SetupDefaultMef(FeedbackInvoker);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            Mock<IWorkspaceItemRepository> mockWorkspaceItemRepository = GetworkspaceItemRespository();
            // ReSharper disable ObjectCreationAsStatement
            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            // ReSharper restore ObjectCreationAsStatement
            MainViewModel = new MainViewModel(EventAggregator.Object, asyncWorker.Object, environmentRepo,
                new Mock<IVersionChecker>().Object, false, null, PopupController.Object,
                WindowManager.Object, WebController.Object, FeedbackInvoker.Object, MockStudioResourceRepository.Object);
        }

        protected void CreateFullExportsAndVm()
        {
            CreateResourceRepo();
            var environmentRepo = GetEnvironmentRepository();
            EventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = EventAggregator.Object;
            PopupController = new Mock<IPopupController>();
            FeedbackInvoker = new Mock<IFeedbackInvoker>();
            WebController = new Mock<IWebController>();
            WindowManager = new Mock<IWindowManager>();
            MockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            SetupDefaultMef(FeedbackInvoker);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            Mock<IWorkspaceItemRepository> mockWorkspaceItemRepository = GetworkspaceItemRespository();
            // ReSharper disable ObjectCreationAsStatement
            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            // ReSharper restore ObjectCreationAsStatement
            FindCefSharpWpfDll();//Ashley: Load Xaml references manually...
            MainViewModel = new MainViewModel(EventAggregator.Object, asyncWorker.Object, environmentRepo,
                new Mock<IVersionChecker>().Object, false, null, PopupController.Object
                , WindowManager.Object, WebController.Object, FeedbackInvoker.Object, MockStudioResourceRepository.Object);

            ActiveEnvironment = new Mock<IEnvironmentModel>();
            AuthorizationService = new Mock<IAuthorizationService>();
            ActiveEnvironment.Setup(e => e.AuthorizationService).Returns(AuthorizationService.Object);

            MainViewModel.ActiveEnvironment = ActiveEnvironment.Object;
        }

        static void FindCefSharpWpfDll()
        {
            var assemblyPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Binaries\CefSharp\CefSharp.Wpf.dll");
            if(File.Exists(assemblyPath))
            {
                Assembly.LoadFile(assemblyPath);
            }
        }

        protected Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();
            result.Setup(c => c.ResourceName).Returns(ResourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(DisplayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(ServiceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(EnvironmentModel.Object);
            result.Setup(c => c.ServerID).Returns(ServerID);
            result.Setup(c => c.ID).Returns(FirstResourceID);
            result.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);

            return result;
        }

        protected IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { EnvironmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(c => c.ReadSession()).Returns(new[] { EnvironmentModel.Object.ID });
            mock.Setup(s => s.Source).Returns(EnvironmentModel.Object);
            EnvironmentRepo = mock.Object;
            return EnvironmentRepo;
        }

        protected void CreateResourceRepo()
        {
            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("");
            EnvironmentModel = CreateMockEnvironment();
            ResourceRepo = new Mock<IResourceRepository>();

            ResourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            ResourceRepo.Setup(r => r.GetDependenciesXml(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>())).Returns(msg);
            FirstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { FirstResource.Object };
            ResourceRepo.Setup(c => c.All()).Returns(coll);
            EnvironmentModel.Setup(m => m.ResourceRepository).Returns(ResourceRepo.Object);
        }

        protected Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.SetupGet(c => c.WorkspaceID).Returns(WorkspaceID);
            connection.SetupGet(c => c.ServerID).Returns(ServerID);
            connection.Setup(c => c.AppServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            int cnt = 0;
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    () =>
                    {
                        if(cnt == 0)
                        {
                            cnt++;
                            return new StringBuilder(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));
                        }

                        return new StringBuilder(JsonConvert.SerializeObject(new ExecuteMessage()));
                    }
                );
            connection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            return connection;
        }

        protected Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());
            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));
            return env;
        }

        protected Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            MockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(WorkspaceID);
            item.SetupGet(i => i.ServerID).Returns(ServerID);
            item.SetupGet(i => i.ServiceName).Returns(ResourceName);
            item.SetupGet(i => i.ID).Returns(FirstResourceID);
            list.Add(item.Object);
            MockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            MockWorkspaceRepo.Setup(c => c.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            MockWorkspaceRepo.Setup(c => c.Remove(FirstResource.Object)).Verifiable();
            return MockWorkspaceRepo;
        }

        protected void AddAdditionalContext()
        {
            SecondResource = new Mock<IContextualResourceModel>();
            SecondResource.Setup(c => c.ResourceName).Returns("WhoCares");
            SecondResource.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            SecondResource.Setup(c => c.WorkflowXaml).Returns(new StringBuilder());
            SecondResource.Setup(c => c.Category).Returns("Testing2");
            SecondResource.Setup(c => c.Environment).Returns(EnvironmentModel.Object);
            SecondResource.Setup(c => c.ServerID).Returns(ServerID);
            SecondResource.Setup(c => c.ID).Returns(SecondResourceID);
            SecondResource.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);
            var msg = new AddWorkSurfaceMessage(SecondResource.Object);
            MainViewModel.Handle(msg);
        }

        protected void SetupForDelete()
        {
            PopupController.Setup(c => c.Show()).Verifiable();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            ResourceRepo.Setup(c => c.HasDependencies(FirstResource.Object)).Returns(false).Verifiable();
            var succesResponse = new ExecuteMessage();

            succesResponse.SetMessage(@"<DataList>Success</DataList>");
            ResourceRepo.Setup(s => s.DeleteResource(FirstResource.Object)).Returns(succesResponse);
        }

        protected Mock<IEnvironmentRepository> SetupForDeleteServer()
        {
            CreateResourceRepo();
            var models = new List<IEnvironmentModel> { EnvironmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.Get(It.IsAny<Guid>())).Returns(EnvironmentModel.Object);
            mock.Setup(s => s.Source).Returns(EnvironmentModel.Object);
            mock.Setup(s => s.ReadSession()).Returns(new[] { EnvironmentModel.Object.ID });
            mock.Setup(s => s.Remove(It.IsAny<IEnvironmentModel>()))
                .Callback<IEnvironmentModel>(s =>
                    Assert.AreEqual(EnvironmentModel.Object, s))
                .Verifiable();
            PopupController = new Mock<IPopupController>();
            EventAggregator = new Mock<IEventAggregator>();
            EventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()))
                .Callback<object>(m =>
                {
                    var removeMsg = (EnvironmentDeletedMessage)m;
                    Assert.AreEqual(EnvironmentModel.Object, removeMsg.EnvironmentModel);
                })
                .Verifiable();
            SetupDefaultMef();
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            MainViewModel = new MainViewModel(EventAggregator.Object, asyncWorker.Object, mock.Object, new Mock<IVersionChecker>().Object, false);
            SetupForDelete();
            FirstResource.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            FirstResource.Setup(r => r.ServerResourceType).Returns("Server");
            FirstResource.Setup(r => r.ConnectionString)
                .Returns(TestResourceStringsTest.ResourceToHydrateConnectionString1);
            EnvironmentConnection = new Mock<IEnvironmentConnection>();
            EnvironmentConnection.Setup(c => c.AppServerUri)
                .Returns(new Uri(TestResourceStringsTest.ResourceToHydrateActualAppUri));
            EnvironmentModel.Setup(r => r.Connection).Returns(EnvironmentConnection.Object);
            return mock;
        }

        protected void SetupDefaultMef()
        {
            SetupDefaultMef(new Mock<IFeedbackInvoker>());
        }

        protected static void SetupDefaultMef(Mock<IFeedbackInvoker> feedbackInvoker)
        {
            ImportService.CurrentContext = new ImportServiceContext();
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(new Mock<IPopupController>().Object);
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);
            ImportService.AddExportedValueToContainer<IFeedBackRecorder>(new FeedbackRecorder());
            ImportService.AddExportedValueToContainer(new Mock<IWindowManager>().Object);
        }

        #endregion Methods used by tests
    }
}
