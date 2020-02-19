using System.Collections.Generic;
using System.Text;
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
            var n = new GetClusterSettingsService();
            var ws = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder> { };
            var result = n.Execute(values, ws.Object);
            Assert.IsInstanceOfType(result, typeof(StringBuilder));
            var expected = "{\"$id\":\"1\",\"$type\":\"Dev2.Common.ClusterSettings, Dev2.Common\",\"Key\":null}";
            Assert.AreEqual(expected, result.ToString());
        }
    }
}
