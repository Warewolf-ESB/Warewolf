using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Washing;

namespace Tu.Server.Tests
{
    [TestClass]
    public class WashingOutputColumnMappingTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingOutputColumnMapping_Constructor")]
        public void WashingOutputColumnMapping_Constructor_AddsMappings()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var washingOutputColumnMapping = new WashingOutputColumnMapping();

            //------------Assert Results-------------------------
            Assert.AreEqual(84, washingOutputColumnMapping.Count);
        }
    }
}
