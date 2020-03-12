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
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Service;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestClusterConnectionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_Expect_Success()
        {
            var leaderServerResource = new NamedGuid {Name = "testServer", Value = Guid.NewGuid()};

            var clusterSettingsData = new ClusterSettingsData
            {
                Key = "asdf", LeaderServerKey = "asdf", LeaderServerResource = leaderServerResource
            };

            var serializeObject = JsonConvert.SerializeObject(clusterSettingsData);
            
            var mockConnections = new Mock<IConnections>();
            var validationResult = new ValidationResult {IsValid = true};
            mockConnections.Setup(o => o.CanConnectToServer(It.IsAny<Connection>())).Returns(validationResult);
            var mockHubProxy = new Mock<IHubProxy>();
            var testClusterResult = new TestClusterResult { HasError = false, Success = true };
            mockHubProxy.Setup(o => o.Invoke<TestClusterResult>("ExecuteCommand", It.IsAny<Envelope>(),
                    It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(testClusterResult));
            
            mockConnections.Setup(o => o.CreateHubProxy(It.IsAny<Connection>())).Returns(mockHubProxy.Object);
            
            var mockResource = new Mock<IResource>();
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(mockResource.Object);

            var testClusterConnection = new TestClusterConnection(mockConnections.Object, mockResourceCatalog.Object);
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "ClusterSettingsData", new StringBuilder(serializeObject) }};
            var result = testClusterConnection.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":false,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":16,\"m_StringValue\":\"\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_CanConnectToServer_False()
        {
            var leaderServerResource = new NamedGuid {Name = "testServer", Value = Guid.NewGuid()};

            var clusterSettingsData = new ClusterSettingsData
            {
                Key = "asdf", LeaderServerKey = "asdf", LeaderServerResource = leaderServerResource
            };

            var serializeObject = JsonConvert.SerializeObject(clusterSettingsData);
            
            var mockConnections = new Mock<IConnections>();
            var validationResult = new ValidationResult {IsValid = false, ErrorMessage = "error" };
            mockConnections.Setup(o => o.CanConnectToServer(It.IsAny<Connection>())).Returns(validationResult);
            var mockHubProxy = new Mock<IHubProxy>();
            var testClusterResult = new TestClusterResult { HasError = false, Success = true };
            mockHubProxy.Setup(o => o.Invoke<TestClusterResult>("ExecuteCommand", It.IsAny<Envelope>(),
                    It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(testClusterResult));
            
            mockConnections.Setup(o => o.CreateHubProxy(It.IsAny<Connection>())).Returns(mockHubProxy.Object);
            
            var mockResource = new Mock<IResource>();
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(mockResource.Object);

            var testClusterConnection = new TestClusterConnection(mockConnections.Object, mockResourceCatalog.Object);
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "ClusterSettingsData", new StringBuilder(serializeObject) }};
            var result = testClusterConnection.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":true,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":16,\"m_StringValue\":\"error\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_No_LeaderServerKey_Expect_Error()
        {
            var leaderServerResource = new NamedGuid {Name = "testServer", Value = Guid.NewGuid()};

            var clusterSettingsData = new ClusterSettingsData
            {
                Key = "asdf", LeaderServerKey = "", LeaderServerResource = leaderServerResource
            };

            var serializeObject = JsonConvert.SerializeObject(clusterSettingsData);
            
            var mockConnections = new Mock<IConnections>();
            var validationResult = new ValidationResult {IsValid = true};
            mockConnections.Setup(o => o.CanConnectToServer(It.IsAny<Connection>())).Returns(validationResult);
            var mockHubProxy = new Mock<IHubProxy>();
            var testClusterResult = new TestClusterResult { HasError = false, Success = true };
            mockHubProxy.Setup(o => o.Invoke<TestClusterResult>("ExecuteCommand", It.IsAny<Envelope>(),
                    It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(testClusterResult));
            
            mockConnections.Setup(o => o.CreateHubProxy(It.IsAny<Connection>())).Returns(mockHubProxy.Object);
            
            var mockResource = new Mock<IResource>();
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(mockResource.Object);

            var testClusterConnection = new TestClusterConnection(mockConnections.Object, mockResourceCatalog.Object);
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "ClusterSettingsData", new StringBuilder(serializeObject) }};
            var result = testClusterConnection.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":true,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":35,\"m_StringValue\":\"no cluster key for leader specified\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_Invoke_TestClusterResult_Expect_Failure()
        {
            var leaderServerResource = new NamedGuid {Name = "testServer", Value = Guid.NewGuid()};

            var clusterSettingsData = new ClusterSettingsData
            {
                Key = "asdf", LeaderServerKey = "fdsa", LeaderServerResource = leaderServerResource
            };

            var serializeObject = JsonConvert.SerializeObject(clusterSettingsData);
            
            var mockConnections = new Mock<IConnections>();
            var validationResult = new ValidationResult {IsValid = true};
            mockConnections.Setup(o => o.CanConnectToServer(It.IsAny<Connection>())).Returns(validationResult);
            var mockHubProxy = new Mock<IHubProxy>();
            var testClusterResult = new TestClusterResult { HasError = false, Success = false };
            mockHubProxy.Setup(o => o.Invoke<TestClusterResult>("ExecuteCommand", It.IsAny<Envelope>(),
                    It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(testClusterResult));
            
            mockConnections.Setup(o => o.CreateHubProxy(It.IsAny<Connection>())).Returns(mockHubProxy.Object);
            
            var mockResource = new Mock<IResource>();
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(mockResource.Object);

            var testClusterConnection = new TestClusterConnection(mockConnections.Object, mockResourceCatalog.Object);
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "ClusterSettingsData", new StringBuilder(serializeObject) }};
            var result = testClusterConnection.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":true,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":40,\"m_StringValue\":\"failed verifying cluster key with leader\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_Default_For_Coverage()
        {
            var mockConnections = new Mock<IConnections>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var testClusterConnection = new TestClusterConnection(mockConnections.Object, mockResourceCatalog.Object);

            Assert.AreEqual(Guid.Empty, testClusterConnection.GetResourceID(null));
            Assert.AreEqual(AuthorizationContext.Contribute, testClusterConnection.GetAuthorizationContextForService());
            var handlesType = testClusterConnection.CreateServiceEntry();
            Assert.AreEqual(1, handlesType.Actions.Count);
            Assert.AreEqual(Cluster.TestClusterConnection, testClusterConnection.HandlesType());
        }
    }
}
