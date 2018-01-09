/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Workspaces;
using Moq;
using Newtonsoft.Json;
using Dev2.Studio.Interfaces.Enums;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Core.Tests
{
    public abstract class MainViewModelBase
    {
        #region Variables

        protected readonly Guid FirstResourceId = Guid.NewGuid();
        protected readonly Guid SecondResourceId = Guid.NewGuid();
        protected readonly Guid ServerId = Guid.NewGuid();
        protected readonly Guid WorkspaceId = Guid.NewGuid();
        protected const string DisplayName = "test2";
        protected Mock<IEnvironmentConnection> EnvironmentConnection;
        protected Mock<IServer> EnvironmentModel;
        protected IServerRepository ServerRepo;
        protected Mock<IEventAggregator> EventAggregator;
        protected Mock<IContextualResourceModel> FirstResource;
        protected ShellViewModel ShellViewModel;
        protected Mock<IWorkspaceItemRepository> MockWorkspaceRepo;
        public Mock<IPopupController> PopupController;
        protected const string ResourceName = "TestResource";
        protected Mock<IResourceRepository> ResourceRepo;
        protected Mock<IContextualResourceModel> SecondResource;
        protected const string ServiceDefinition = "<x/>";

        protected Mock<IWindowManager> WindowManager;
        protected Mock<IAuthorizationService> AuthorizationService;
        protected Mock<IServer> ActiveEnvironment;

        #endregion Variables

        #region Methods used by tests
        protected Mock<IServerRepository> EmptyEnvRepo { get; set; }
        protected void CreateFullExportsAndVmWithEmptyRepo()
        {
            CreateResourceRepo();
            var mockEnv = new Mock<IServerRepository>();
            mockEnv.SetupProperty(g => g.ActiveServer); // Start tracking changes
            mockEnv.Setup(g => g.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockEnv.Object);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            mockEnv.Setup(repository => repository.Source).Returns(mockEnvironmentModel.Object);

            var environmentRepo = mockEnv.Object;
            EmptyEnvRepo = mockEnv;
            EventAggregator = new Mock<IEventAggregator>();
            PopupController = new Mock<IPopupController>();
            WindowManager = new Mock<IWindowManager>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockWorkspaceItemRepository = GetworkspaceItemRespository();

            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>()))
                .Returns(viewMock.Object);
            ShellViewModel = new ShellViewModel(EventAggregator.Object, asyncWorker.Object, environmentRepo,new Mock<IVersionChecker>().Object, vieFactory.Object, false, null, PopupController.Object);
        }

        protected void CreateFullExportsAndVm(IExplorerViewModel viewModel)
        {
            CreateFullExportsAndVm();
            ShellViewModel.ExplorerViewModel = viewModel;
        }

        protected void CreateFullExportsAndVm()
        {
            CreateResourceRepo();
            var environmentRepo = GetEnvironmentRepository();
            EventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = EventAggregator.Object;
            PopupController = new Mock<IPopupController>();
            WindowManager = new Mock<IWindowManager>();
            CustomContainer.Register(WindowManager.Object);
            CustomContainer.Register(PopupController.Object);
            BrowserPopupController = new Mock<IBrowserPopupController>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockWorkspaceItemRepository = GetworkspaceItemRespository();

            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            
            var explorerViewModel = new Mock<IExplorerViewModel>();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>()))
                .Returns(viewMock.Object);
            ShellViewModel = new ShellViewModel(EventAggregator.Object, asyncWorker.Object, environmentRepo, new Mock<IVersionChecker>().Object, vieFactory.Object, false, BrowserPopupController.Object, PopupController.Object, explorerViewModel.Object);
            var activeEnvironment = new Mock<IServer>();
            activeEnvironment.Setup(server => server.DisplayName).Returns("localhost");
            ActiveEnvironment = activeEnvironment;
            AuthorizationService = new Mock<IAuthorizationService>();
            ActiveEnvironment.Setup(e => e.AuthorizationService).Returns(AuthorizationService.Object);

            ShellViewModel.ActiveServer = ActiveEnvironment.Object;
        }

        protected Mock<IBrowserPopupController> BrowserPopupController;

        protected Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();
            result.Setup(c => c.ResourceName).Returns(ResourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(DisplayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(ServiceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(EnvironmentModel.Object);
            result.Setup(c => c.ServerID).Returns(ServerId);
            result.Setup(c => c.ID).Returns(FirstResourceId);
            result.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);

            return result;
        }

        protected IServerRepository GetEnvironmentRepository()
        {
            var models = new List<IServer> { EnvironmentModel.Object };
            var mock = new Mock<IServerRepository>();
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(mock.Object);
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.Source).Returns(EnvironmentModel.Object);
            mock.Setup(repo => repo.Get(It.IsAny<Guid>())).Returns(EnvironmentModel.Object);
            mock.Setup(repo => repo.ActiveServer).Returns(EnvironmentModel.Object);
            ServerRepo = mock.Object;
            return ServerRepo;
        }

        protected void CreateResourceRepo()
        {
            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("");
            EnvironmentModel = CreateMockEnvironment();
            ResourceRepo = new Mock<IResourceRepository>();

            ResourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
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
            connection.SetupGet(c => c.WorkspaceID).Returns(WorkspaceId);
            connection.SetupGet(c => c.ServerID).Returns(ServerId);
            connection.Setup(c => c.AppServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            connection.Setup(c => c.WebServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            connection.Setup(c => c.IsConnected).Returns(true);
            var cnt = 0;
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(
                    () =>
                    {
                        if (cnt == 0)
                        {
                            cnt++;
                            return new StringBuilder($"<XmlData>{string.Join("\n", sources)}</XmlData>");
                        }

                        return new StringBuilder(JsonConvert.SerializeObject(new ExecuteMessage()));
                    }
                );
            connection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            return connection;
        }

        protected Mock<IServer> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);
            var env = new Mock<IServer>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.EnvironmentID).Returns(ServerId);
            env.Setup(e => e.Name).Returns($"Server_{rand.Next(1, 100)}");

            return env;
        }

        protected Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            MockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(WorkspaceId);
            item.SetupGet(i => i.ServerID).Returns(ServerId);
            item.SetupGet(i => i.EnvironmentID).Returns(ServerId);
            item.SetupGet(i => i.ServiceName).Returns(ResourceName);
            item.SetupGet(i => i.ID).Returns(FirstResourceId);
            list.Add(item.Object);
            MockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
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
            SecondResource.Setup(c => c.ServerID).Returns(ServerId);
            SecondResource.Setup(c => c.ID).Returns(SecondResourceId);
            SecondResource.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);
            var msg = new AddWorkSurfaceMessage(SecondResource.Object);
            ShellViewModel.Handle(msg);
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

        #endregion Methods used by tests
    }
}
