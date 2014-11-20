using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Serialization
{
    [TestClass]
    public class ResourceJsonConvertorTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceTypeConvertor_ConvertToJson")]
        public void ResourceTypeConvertor_ConvertToJson_EnumAsString_ExpectConvert()
        {
            //------------Setup for test--------------------------
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var data = serializer.Serialize(new ServerExplorerItem("a", Guid.Empty, Dev2.Common.Interfaces.Data.ResourceType.DbService, null, Permissions.Administrator, "bob"));
            
            //------------Execute Test---------------------------
            Assert.IsTrue(data.Contains("DbService"));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceTypeConvertor_ConvertToJson")]
        public void ResourceTypeConvertor_ConvertToJson_EnumAsString_Deserialise_ExpectConvert()
        {
            //------------Setup for test--------------------------
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var data = serializer.Serialize(new ServerExplorerItem("a", Guid.Empty, Dev2.Common.Interfaces.Data.ResourceType.DbService, null, Permissions.Administrator, "bob"));
            
            //------------Execute Test---------------------------
            Assert.IsTrue(data.Contains("DbService"));
            data = data.Replace(@"""ResourceType"": ""DbService""", @"""ResourceType"": ""4""");
            var item = (ServerExplorerItem)serializer.Deserialize(data, typeof(ServerExplorerItem));
            Assert.AreEqual (ResourceType.PluginService, item.ResourceType);
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceTypeConvertor_ConvertToJson")]
        public void ResourceTypeConvertor_ConvertToJson_EnumAsString_Deserialise_ExpectConvert_OldServer_IsServer()
        {
            //------------Setup for test--------------------------
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var data = serializer.Serialize(new ServerExplorerItem("a", Guid.Empty, Dev2.Common.Interfaces.Data.ResourceType.DbService, null, Permissions.Administrator, "bob"));

            //------------Execute Test---------------------------
            Assert.IsTrue(data.Contains("DbService"));
            data = data.Replace(@"""ResourceType"": ""DbService""", @"""ResourceType"": ""1024""");
            var item = (ServerExplorerItem)serializer.Deserialize(data, typeof(ServerExplorerItem));
            Assert.AreEqual(ResourceType.Server, item.ResourceType);
            //------------Assert Results-------------------------
        }

          
    }
}
