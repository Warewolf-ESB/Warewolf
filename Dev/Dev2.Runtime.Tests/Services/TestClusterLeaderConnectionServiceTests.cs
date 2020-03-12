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
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
using Warewolf.Service;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestClusterLeaderConnectionServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterLeaderConnectionService))]
        public void TestClusterLeaderConnectionService_Expect_Success()
        {
            var clusterSettings = Common.Config.Cluster.Get();
            var clusterSettingsData = new ClusterSettingsData  { LeaderServerKey = clusterSettings.Key };

            var leaderConnectionService = new TestClusterLeaderConnectionService();
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "LeaderServerKey", clusterSettingsData.LeaderServerKey.ToStringBuilder() }};
            var result = leaderConnectionService.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":false,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":16,\"m_StringValue\":\"\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterConnection))]
        public void TestClusterConnection_CanConnectToServer_False()
        {
            var clusterSettingsData = new ClusterSettingsData  { LeaderServerKey = "" };

            var leaderConnectionService = new TestClusterLeaderConnectionService();
            var mockWorkspace = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> {{ "LeaderServerKey", clusterSettingsData.LeaderServerKey.ToStringBuilder() }};
            var result = leaderConnectionService.Execute(values, mockWorkspace.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":true,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":39,\"m_StringValue\":\"the cluster key provided does not match\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TestClusterLeaderConnectionService))]
        public void TestClusterLeaderConnectionService_Default_For_Coverage()
        {
            var leaderConnectionService = new TestClusterLeaderConnectionService();

            Assert.AreEqual(Guid.Empty, leaderConnectionService.GetResourceID(null));
            Assert.AreEqual(AuthorizationContext.Contribute, leaderConnectionService.GetAuthorizationContextForService());
            var handlesType = leaderConnectionService.CreateServiceEntry();
            Assert.AreEqual(1, handlesType.Actions.Count);
            Assert.AreEqual(Cluster.TestClusterLeaderConnection, leaderConnectionService.HandlesType());
        }
    }
}
