using System;
using Caliburn.Micro;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    public class EnvironmentTreeViewModelTests
    {

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_DeployFromFalse_ChildrenCleared()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(authorizationService.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object, navigationViewModelType: NavigationViewModelType.DeployFrom);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(false);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, environmentVm.Children.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_DeployFromTrue_ChildrenNotClearedCleared()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(authorizationService.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object, navigationViewModelType: NavigationViewModelType.DeployFrom);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(3, environmentVm.Children.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_DeployToTrue_ChildrenIsNotCleared()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(authorizationService.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object, navigationViewModelType: NavigationViewModelType.DeployTo);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(3, environmentVm.Children.Count);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_IsAuthorizedFalse_ChildrenCleared()
        {

            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();

            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, asyncWorker.Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(false);
            environmentVm.DoUpdateBasedOnPermissions();
            Assert.AreEqual(0, environmentVm.Children.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_IsAuthorizedTrue_ChildrenNotCleared()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(3, environmentVm.Children.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EnvironmentTreeViewModel_PermissionsChanged")]
        public void EnvironmentTreeViewModel_PermissionsChanged_DeployToFalse_ChildrenCleared()
        {
            var connection = CreateConnection();
            connection.Setup(c => c.IsAuthorized).Returns(true);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(authorizationService.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(connection.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object, navigationViewModelType: NavigationViewModelType.DeployTo);
            var environmentVm = new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            var child = new Mock<ITreeNode>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            authorizationService.Setup(e => e.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(false);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, environmentVm.Children.Count);
        }

        static Mock<IEnvironmentConnection> CreateConnection()
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            return conn;
        }
    }
}