using System;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Models;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
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

            var environmentVm = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, StudioResourceRepository.Instance);

            var child = new Mock<IExplorerItemModel>().Object;

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

            var environmentVm = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, StudioResourceRepository.Instance);

            var child = new Mock<IExplorerItemModel>().Object;
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

            var environmentVm = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, StudioResourceRepository.Instance);

            var child = new Mock<IExplorerItemModel>().Object;

            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);
            environmentVm.Children.Add(child);

            mockEnvironmentModel.Setup(e => e.IsAuthorized).Returns(true);
            connection.Raise(c => c.PermissionsChanged += null, EventArgs.Empty);

            Assert.AreEqual(3, environmentVm.Children.Count);
        }


        static Mock<IEnvironmentConnection> CreateConnection()
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            return conn;
        }
    }
}