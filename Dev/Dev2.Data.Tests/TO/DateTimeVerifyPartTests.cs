using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    [TestClass]
    public class DateTimeVerifyPartTests
    {
        [TestMethod]
        public void DateTimeVerifyPart_ShouldCreateDateTiimePart()
        {
            var dataListVerifyPart = IntellisenseFactory.CreateDateTimePart("2008", "Year");
            Assert.IsNotNull(dataListVerifyPart);
        }
    }
}
