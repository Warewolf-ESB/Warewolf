/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Providers.Events;
using Dev2.Security;
using Dev2.Services.Security;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;

namespace Dev2.Core.Tests.Security
{
    [TestClass]
    public class ClientSecurityServiceTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(ClientSecurityService))]
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
        [TestCategory(nameof(ClientSecurityService))]
        public void ClientSecurityService_EnvironmentConnection_NetworkStateChangedToOnline_DoesNotInvokeRead()
        {
            Verify_EnvironmentConnection_NetworkStateChanged(NetworkState.Offline, NetworkState.Online);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(ClientSecurityService))]
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
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID))
                .Callback((StringBuilder xmlRequest, Guid wsID) => { actualRequest = xmlRequest; })
                .Returns(requestResult)
                .Verifiable();

            new ClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            connection.Raise(c => c.NetworkStateChanged += null, new NetworkStateEventArgs(fromState, toState));

            // wait for ReadAsync to finish
            Thread.Sleep(1000);

            //------------Assert Results-------------------------
            if (toState == NetworkState.Online)
            {
                connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID), Times.Never());
                Assert.IsNull(actualRequest);
            }
            else
            {
                connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID), Times.Never());
                Assert.IsNull(actualRequest);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(ClientSecurityService))]
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
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), workspaceID))
                .Returns(requestResult);

            var clientSecurityService = new TestClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            clientSecurityService.Read();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, clientSecurityService.ReadAsyncHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(ClientSecurityService))]
        public void ClientSecurityService_WritePermissions_DoesNothing()
        {
            //------------Setup for test--------------------------
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Verifiable();

            var clientSecurityService = new TestClientSecurityService(connection.Object);

            //------------Execute Test---------------------------
            clientSecurityService.TestWritePermissions();

            //------------Assert Results-------------------------
            connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(ClientSecurityService))]
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
        [TestCategory(nameof(ClientSecurityService))]
        public void ClientSecurityService_PermissionsAndAuthenticationModified_NewPermissionsAndAuthenticationReceived_ShouldReplaceCurrentPermissionsAndAuthentication()
        {
            //------------Setup for test--------------------------
            var connection = new Mock<IEnvironmentConnection>();
            IEventPublisher eventPublisher = new EventPublisher();
            connection.Setup(c => c.ServerEvents).Returns(eventPublisher);
            var clientSecurityService = new TestClientSecurityService(connection.Object);
            var currentPermissions = new List<WindowsGroupPermission>();

            var resourceID = Guid.NewGuid();

            var resourcePermission = new WindowsGroupPermission();
            resourcePermission.ResourceID = resourceID;
            resourcePermission.Permissions = Permissions.View & Permissions.Execute;

            var serverPermission = new WindowsGroupPermission();
            serverPermission.ResourceID = Guid.Empty;
            serverPermission.Permissions = Permissions.DeployFrom & Permissions.DeployTo;
            serverPermission.IsServer = true;

            currentPermissions.Add(serverPermission);
            currentPermissions.Add(resourcePermission);

            var currentOverrideResource = new NamedGuid
            {
                Name = "authLogin",
                Value = Guid.NewGuid()
            };
            var currenthmac = new HMACSHA256();
            var currentSecretKey = Convert.ToBase64String(currenthmac.Key);
            var currentSettings = new SecuritySettingsTO(currentPermissions, currentOverrideResource, currentSecretKey);

            clientSecurityService.SetCurrentPermissions(currentPermissions);
            clientSecurityService.SetCurrentSettings(currentSettings);
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

            var changedOverrideResource = new NamedGuid
            {
                Name = "authLoginNew",
                Value = Guid.NewGuid()
            };
            var hmac = new HMACSHA256();
            var changedSecretKey = Convert.ToBase64String(hmac.Key);
            var changedAuthentication = new SecuritySettingsTO(changedPermissions, changedOverrideResource, changedSecretKey);

            var authenticationModifiedMemo = new AuthenticationModifiedMemo();
            authenticationModifiedMemo.ModifiedAuthentication = changedAuthentication;

            var permissionsModifiedMemo = new PermissionsModifiedMemo();
            permissionsModifiedMemo.ModifiedPermissions = changedPermissions;
            //------------Execute Test---------------------------
            connection.Raise(environmentConnection => environmentConnection.PermissionsModified += null, null, changedPermissions);
            eventPublisher.Publish(permissionsModifiedMemo);

            connection.Raise(environmentConnection => environmentConnection.AuthenticationModified += null, null, changedAuthentication);
            eventPublisher.Publish(authenticationModifiedMemo);

            //------------Assert Results-------------------------
            var updateResourcePermission = clientSecurityService.Permissions.FirstOrDefault(permission => permission.ResourceID == resourceID);
            var updateServerPermission = clientSecurityService.Permissions.FirstOrDefault(permission => permission.ResourceID == Guid.Empty);
            Assert.IsNotNull(updateResourcePermission);
            Assert.IsNotNull(updateServerPermission);
            Assert.AreEqual(Permissions.Contribute, updateResourcePermission.Permissions);
            Assert.AreEqual(Permissions.Administrator, updateServerPermission.Permissions);

            Assert.AreEqual(changedOverrideResource, clientSecurityService.OverrideResource);
            Assert.AreEqual(changedSecretKey, clientSecurityService.SecretKey);
        }
    }

    public class TestClientSecurityService : ClientSecurityService
    {
        List<WindowsGroupPermission> _currentPermissions;
        SecuritySettingsTO _currentSettings;

        public TestClientSecurityService(IEnvironmentConnection environmentConnection)
            : base(environmentConnection)
        {
        }

        public void TestWritePermissions()
        {
            WritePermissions(null, null, "");
        }

        public int ReadAsyncHitCount { get; private set; }

        public override Task ReadAsync()
        {
            ReadAsyncHitCount++;
            return base.ReadAsync();
        }

        protected override SecuritySettingsTO ReadSecuritySettings()
        {
            var currentSettings = new SecuritySettingsTO();
            if (_currentSettings != null)
            {
                currentSettings = _currentSettings;
            }

            return currentSettings;
        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            var currentPermissions = new List<WindowsGroupPermission>();
            if (_currentPermissions != null)
            {
                currentPermissions = _currentPermissions;
            }

            return currentPermissions;
        }

        public void SetCurrentPermissions(List<WindowsGroupPermission> windowsGroupPermissions)
        {
            _currentPermissions = windowsGroupPermissions;
        }

        public void SetCurrentSettings(SecuritySettingsTO settings)
        {
            _currentSettings = settings;
        }
    }
}