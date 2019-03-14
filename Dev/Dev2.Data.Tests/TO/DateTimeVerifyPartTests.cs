using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    [TestClass]
    public class DateTimeVerifyPartTests
    {
        [ClassInitialize]
        public static void PreConditions(TestContext testContext)
        {
            var regionName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            var regionNameUI = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            Assert.AreEqual("en-ZA", regionName);
            Assert.AreEqual("en-ZA", regionNameUI);
        }

        [TestMethod]
        public void DateTimeVerifyPart_ShouldCreateDateTiimePart()
        {
            var dataListVerifyPart = IntellisenseFactory.CreateDateTimePart("2008", "Year");
            Assert.IsNotNull(dataListVerifyPart);
        }
    }
}
