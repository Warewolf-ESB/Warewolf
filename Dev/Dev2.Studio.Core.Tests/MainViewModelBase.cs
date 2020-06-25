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

        protected readonly Guid _firstResourceId = Guid.NewGuid();
        protected readonly Guid _secondResourceId = Guid.NewGuid();
        protected readonly Guid _serverId = Guid.NewGuid();
        protected readonly Guid _workspaceId = Guid.NewGuid();
        protected const string DisplayName = "test2";
        protected Mock<IEnvironmentConnection> _environmentConnection;
        protected Mock<IServer> _environmentModel;
        protected IServerRepository _serverRepo;
        protected Mock<IEventAggregator> _eventAggregator;
        protected Mock<IContextualResourceModel> _firstResource;
        protected ShellViewModel _shellViewModel;
        protected Mock<IWorkspaceItemRepository> _mockWorkspaceRepo;
        public Mock<IPopupController> _popupController;
        protected const string ResourceName = "TestResource";
        protected Mock<IResourceRepository> _resourceRepo;
        protected Mock<IContextualResourceModel> _secondResource;
        protected const string ServiceDefinition = "<x/>";

        protected Mock<IWindowManager> _windowManager;
        protected Mock<IAuthorizationService> _authorizationService;
        protected Mock<IServer> _activeEnvironment;

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
            var mockEnvironmentConnection = SetupMockConnection();
            mockEnvironmentModel.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            mockEnv.Setup(repository => repository.Source).Returns(mockEnvironmentModel.Object);

            var environmentRepo = mockEnv.Object;
            EmptyEnvRepo = mockEnv;
            _eventAggregator = new Mock<IEventAggregator>();
            _popupController = new Mock<IPopupController>();
            _windowManager = new Mock<IWindowManager>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockWorkspaceItemRepository = GetworkspaceItemRespository();

            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>()))
                .Returns(viewMock.Object);
            _shellViewModel = new ShellViewModel(_eventAggregator.Object, asyncWorker.Object, environmentRepo,new Mock<IVersionChecker>().Object, vieFactory.Object, false, null, _popupController.Object);


        }

        private static Mock<IEnvironmentConnection> SetupMockConnection()
        {
            var uri = new Uri("http://bravo.com/");
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(a => a.AppServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.AuthenticationType).Returns(Dev2.Runtime.ServiceModel.Data.AuthenticationType.Public);
            mockEnvironmentConnection.Setup(a => a.WebServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.ID).Returns(Guid.Empty);
            return mockEnvironmentConnection;
        }

        protected void CreateFullExportsAndVm(IExplorerViewModel viewModel)
        {
            CreateFullExportsAndVm();
            _shellViewModel.ExplorerViewModel = viewModel;
        }

        protected void CreateFullExportsAndVm()
        {
            CreateResourceRepo();
            var environmentRepo = GetEnvironmentRepository();
            _eventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = _eventAggregator.Object;
            _popupController = new Mock<IPopupController>();
            _windowManager = new Mock<IWindowManager>();
            CustomContainer.Register(_windowManager.Object);
            _browserPopupController = new Mock<IBrowserPopupController>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockWorkspaceItemRepository = GetworkspaceItemRespository();

            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            
            var explorerViewModel = new Mock<IExplorerViewModel>();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>()))
                .Returns(viewMock.Object);
            _shellViewModel = new ShellViewModel(_eventAggregator.Object, asyncWorker.Object, environmentRepo, new Mock<IVersionChecker>().Object, vieFactory.Object, false, _browserPopupController.Object, _popupController.Object, explorerViewModel.Object, null);
            var activeEnvironment = new Mock<IServer>();
            activeEnvironment.Setup(server => server.DisplayName).Returns("localhost");
            _activeEnvironment = activeEnvironment;
            _authorizationService = new Mock<IAuthorizationService>();
            _activeEnvironment.Setup(e => e.AuthorizationService).Returns(_authorizationService.Object);

            _shellViewModel.ActiveServer = _activeEnvironment.Object;
        }

        protected Mock<IBrowserPopupController> _browserPopupController;

        protected Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();
            result.Setup(c => c.ResourceName).Returns(ResourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(DisplayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(ServiceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverId);
            result.Setup(c => c.ID).Returns(_firstResourceId);
            result.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);

            return result;
        }

        protected IServerRepository GetEnvironmentRepository()
        {
            var models = new List<IServer> { _environmentModel.Object };
            var mock = new Mock<IServerRepository>();
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(mock.Object);
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.Source).Returns(_environmentModel.Object);
            mock.Setup(repo => repo.Get(It.IsAny<Guid>())).Returns(_environmentModel.Object);
            mock.Setup(repo => repo.ActiveServer).Returns(_environmentModel.Object);
            _serverRepo = mock.Object;
            return _serverRepo;
        }

        protected void CreateResourceRepo()
        {
            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("");
            _environmentModel = CreateMockEnvironment();
            _resourceRepo = new Mock<IResourceRepository>();

            _resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            _resourceRepo.Setup(r => r.GetDependenciesXml(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>())).Returns(msg);
            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);

            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

        }

        protected Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.SetupGet(c => c.WorkspaceID).Returns(_workspaceId);
            connection.SetupGet(c => c.ServerID).Returns(_serverId);
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
            env.Setup(e => e.EnvironmentID).Returns(_serverId);
            env.Setup(e => e.Name).Returns($"Server_{rand.Next(1, 100)}");

            return env;
        }

        protected Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            _mockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(_workspaceId);
            item.SetupGet(i => i.ServerID).Returns(_serverId);
            item.SetupGet(i => i.EnvironmentID).Returns(_serverId);
            item.SetupGet(i => i.ServiceName).Returns(ResourceName);
            item.SetupGet(i => i.ID).Returns(_firstResourceId);
            list.Add(item.Object);
            _mockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            _mockWorkspaceRepo.Setup(c => c.Remove(_firstResource.Object)).Verifiable();
            return _mockWorkspaceRepo;
        }

        protected void AddAdditionalContext()
        {
            _secondResource = new Mock<IContextualResourceModel>();
            _secondResource.Setup(c => c.ResourceName).Returns("WhoCares");
            _secondResource.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            _secondResource.Setup(c => c.WorkflowXaml).Returns(new StringBuilder());
            _secondResource.Setup(c => c.Category).Returns("Testing2");
            _secondResource.Setup(c => c.Environment).Returns(_environmentModel.Object);
            _secondResource.Setup(c => c.ServerID).Returns(_serverId);
            _secondResource.Setup(c => c.ID).Returns(_secondResourceId);
            _secondResource.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);
            var msg = new AddWorkSurfaceMessage(_secondResource.Object);
            _shellViewModel.Handle(msg);
        }

        protected void SetupForDelete()
        {
            _popupController.Setup(c => c.Show()).Verifiable();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            _resourceRepo.Setup(c => c.HasDependencies(_firstResource.Object)).Returns(false).Verifiable();
            var succesResponse = new ExecuteMessage();

            succesResponse.SetMessage(@"<DataList>Success</DataList>");
            _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(succesResponse);
        }

        #endregion Methods used by tests
    }
}
