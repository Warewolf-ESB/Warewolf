
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
using System.Linq;
using System.Network;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Providers.Events;
using Dev2.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Security
{
    [TestClass]
    public class ClientSecurityServiceTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClientSecurityService_Constructor_EnvironmentConnectionIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var clientSecurityService = new ClientSecurityService(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_EnvironmentConnection")]
        public void ClientSecurityService_EnvironmentConnection_NetworkStateChangedToOnline_DoesNotInvokeRead()
        {
            Verify_EnvironmentConnection_NetworkStateChanged(NetworkState.Offline, NetworkState.Online);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_EnvironmentConnection")]
        public void ClientSecurityService_EnvironmentConnection_NetworkStateChangedToOffline_DoesNotInvokeRead()
        {
            Verify_EnvironmentConnection_NetworkStateChanged(NetworkState.Online, NetworkState.Offline);
        }

        void Verify_EnvironmentConnection_NetworkStateChanged(NetworkState fromState, NetworkState toState)
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var dataListID = Guid.Empty;

            var serializer = new Dev2JsonSerializer();
            var requestResult = serializer.SerializeToBuilder(new SecuritySettingsTO());

            StringBuilder actualRequest = null;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            connection.Setup(c => c.WorkspaceID).Returns(workspaceID);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID))
                .Callback((StringBuilder xmlRequest, Guid wsID, Guid dlID) => { actualRequest = xmlRequest; })
                .Returns(requestResult)
                .Verifiable();

            var clientSecurityService = new ClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            connection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(fromState, toState));

            // wait for ReadAsync to finish
            Thread.Sleep(1000);

            //------------Assert Results-------------------------
            if(toState == NetworkState.Online)
            {
                connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID), Times.Never());
                Assert.IsNull(actualRequest);
            }
            else
            {
                connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID), Times.Never());
                Assert.IsNull(actualRequest);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_Read")]
        public void ClientSecurityService_Read_DoesInvokeReadAsync()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var dataListID = Guid.Empty;

            var serializer = new Dev2JsonSerializer();
            var requestResult = serializer.SerializeToBuilder(new SecuritySettingsTO());

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            connection.Setup(c => c.WorkspaceID).Returns(workspaceID);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID))
                .Returns(requestResult);

            var clientSecurityService = new TestClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            clientSecurityService.Read();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, clientSecurityService.ReadAsyncHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_Read")]
        public void ClientSecurityService_ReadAsync_DoesInvokeReadPermissions()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var dataListID = Guid.Empty;

            var serializer = new Dev2JsonSerializer();
            var requestResult = serializer.SerializeToBuilder(new SecuritySettingsTO());

            StringBuilder actualRequest = null;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            connection.Setup(c => c.WorkspaceID).Returns(workspaceID);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID))
                .Callback((StringBuilder xmlRequest, Guid wsID, Guid dlID) => { actualRequest = xmlRequest; })
                .Returns(requestResult)
                .Verifiable();

            var clientSecurityService = new ClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            var readTask = clientSecurityService.ReadAsync();
            readTask.Wait();

            //------------Assert Results-------------------------
            connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID, dataListID),Times.Never());
            Assert.IsNull(actualRequest);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_WritePermissions")]
        public void ClientSecurityService_WritePermissions_DoesNothing()
        {
            //------------Setup for test--------------------------
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();

            var clientSecurityService = new TestClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            clientSecurityService.TestWritePermissions();

            //------------Assert Results-------------------------
            connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ClientSecurityService_OnDispose")]
        public void ClientSecurityService_OnDispose_DoesNothing()
        {
            //------------Setup for test--------------------------
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var clientSecurityService = new ClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            clientSecurityService.Dispose();

            //------------Assert Results-------------------------
            Assert.IsTrue(true);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientSecurityService_PermissionsModified")]
        public void ClientSecurityService_PermissionsModified_NewPermissionsReceived_ShouldReplaceCurrentPermissions()
        {
            //------------Setup for test--------------------------
            var connection = new Mock<IEnvironmentConnection>();
            IEventPublisher eventPublisher = new EventPublisher();
            connection.Setup(c => c.ServerEvents).Returns(eventPublisher);
            var clientSecurityService = new TestClientSecurityService(connection.Object);
            var currentPermissions = new List<WindowsGroupPermission>();
            Guid resourceID = Guid.NewGuid();

            var resourcePermission = new WindowsGroupPermission();
            resourcePermission.ResourceID = resourceID;
            resourcePermission.Permissions = Permissions.View & Permissions.Execute;

            var serverPermission = new WindowsGroupPermission();
            serverPermission.ResourceID = Guid.Empty;
            serverPermission.Permissions = Permissions.DeployFrom & Permissions.DeployTo;
            serverPermission.IsServer = true;

            currentPermissions.Add(serverPermission);
            currentPermissions.Add(resourcePermission);
            clientSecurityService.SetCurrentPermissions(currentPermissions);
            clientSecurityService.ReadAsync().Wait();

            var changedPermissions = new List<WindowsGroupPermission>();

            var changedResourcePermission = new WindowsGroupPermission();
            changedResourcePermission.ResourceID = resourceID;
            changedResourcePermission.Permissions = Permissions.Contribute;

            var changedServerPermission = new WindowsGroupPermission();
            changedServerPermission.ResourceID = Guid.Empty;
            changedServerPermission.Permissions = Permissions.Administrator;
            changedServerPermission.IsServer = true;

            changedPermissions.Add(changedServerPermission);
            changedPermissions.Add(changedResourcePermission);

            var permissionsModifiedMemo = new PermissionsModifiedMemo();
            permissionsModifiedMemo.ModifiedPermissions = changedPermissions;
            //------------Execute Test---------------------------
            connection.Raise(environmentConnection => environmentConnection.PermissionsModified += null, null,changedPermissions);
            eventPublisher.Publish(permissionsModifiedMemo);
            //------------Assert Results-------------------------
            var updateResourcePermission = clientSecurityService.Permissions.FirstOrDefault(permission => permission.ResourceID == resourceID);
            var updateServerPermission = clientSecurityService.Permissions.FirstOrDefault(permission => permission.ResourceID == Guid.Empty);
            Assert.IsNotNull(updateResourcePermission);
            Assert.IsNotNull(updateServerPermission);
            Assert.AreEqual(Permissions.Contribute, updateResourcePermission.Permissions);
            Assert.AreEqual(Permissions.Administrator, updateServerPermission.Permissions);
        }


    }

    public class TestClientSecurityService : ClientSecurityService
    {
        List<WindowsGroupPermission> _currentPermissions;

        public TestClientSecurityService(IEnvironmentConnection environmentConnection)
            : base(environmentConnection)
        {
        }

        public void TestWritePermissions()
        {
            base.WritePermissions(null);
        }

        public int ReadAsyncHitCount { get; private set; }
        public override Task ReadAsync()
        {
            ReadAsyncHitCount++;
            return base.ReadAsync();
        }

        #region Overrides of ClientSecurityService

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            var currentPermissions = new List<WindowsGroupPermission>();
            if(_currentPermissions != null)
            {
                currentPermissions = _currentPermissions;
            }
            return currentPermissions;
        }

        #endregion

        public void SetCurrentPermissions(List<WindowsGroupPermission> windowsGroupPermissions)
        {
            _currentPermissions = windowsGroupPermissions;
        }
    }
}
