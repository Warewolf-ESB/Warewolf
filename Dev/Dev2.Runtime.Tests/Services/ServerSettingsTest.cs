using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ServerSettingsTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SaveServerSettings_Execute")]
        public void SaveServerSettings_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveServerSettings = new SaveServerSettings();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveServerSettings.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("GetServerSettings_Execute")]
        public void GetServerSettings_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveServerSettings = new SaveServerSettings();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveServerSettings.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }
    }
}
