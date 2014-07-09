using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Washing;

namespace Tu.Server.Tests
{
    [TestClass]
    public class WashingOutputColumnsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingOutputColumns_Constructor")]
        public void WashingOutputColumns_Constructor_AddsColumns()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var washingOutputColumns = new WashingOutputColumns();

            //------------Assert Results-------------------------
            Assert.AreEqual(167, washingOutputColumns.Count);
        }
    }
}
