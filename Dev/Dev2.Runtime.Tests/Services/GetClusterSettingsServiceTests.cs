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
using Dev2.Common.Serializers;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetClusterSettingsServiceTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetClusterSettingsService))]
        public void GetClusterSettings_()
        {
            var serializer = new Dev2JsonSerializer();
            var clusterSettings = Common.Config.Cluster.Get();
            var settings = serializer.SerializeToBuilder(clusterSettings);
            
            var n = new GetClusterSettingsService();
            var ws = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> { };
            var result = n.Execute(values, ws.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            Assert.AreEqual(settings.ToString(), result.ToString());
        }
    }
}
