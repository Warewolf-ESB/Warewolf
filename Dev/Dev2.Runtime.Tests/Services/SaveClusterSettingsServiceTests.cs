/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Collections.Generic;
using System.Text;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveClusterSettingsServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveClusterSettingsService))]
        public void SaveClusterSettingsService_Expect_NoError()
        {
            var n = new SaveClusterSettingsService();
            var ws = new Mock<IWorkspace>();

            var clusterSettingsData = new ClusterSettingsData();

            var serializer = new Communication.Dev2JsonSerializer();
            var obj = serializer.SerializeToBuilder(clusterSettingsData);
            var values = new Dictionary<string, StringBuilder>
            {
                {"ClusterSettingsData", obj},
            };
            var result = n.Execute(values, ws.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":false,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":16,\"m_StringValue\":\"\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveClusterSettingsService))]
        public void SaveClusterSettingsService_Expect_Error()
        {
            var n = new SaveClusterSettingsService();
            var ws = new Mock<IWorkspace>();

            var clusterSettingsData = new ClusterSettingsData();

            var values = new Dictionary<string, StringBuilder>
            {
                {"ClusterSettingsData", new StringBuilder(clusterSettingsData.ToString())},
            };
            var result = n.Execute(values, ws.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Communication.ExecuteMessage, Dev2.Infrastructure\",\"HasError\":true,\"Message\":{\"$id\":\"2\",\"$type\":\"System.Text.StringBuilder, mscorlib\",\"m_MaxCapacity\":2147483647,\"Capacity\":53,\"m_StringValue\":\"Object reference not set to an instance of an object.\",\"m_currentThread\":0}}";
            Assert.AreEqual(expected, result.ToString());
        }
    }
}
