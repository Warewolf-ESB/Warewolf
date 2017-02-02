using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class Dev2ColumnTests
    {
        [TestMethod]
        public void GivenUnEqualColumns_Dev2Column_ShoudEquals_ShouldReturnFalse()
        {
            var dev2Column = DataListFactory.CreateDev2Column("Column1", "Column1Description", true,
                enDev2ColumnArgumentDirection.None);
            var other = DataListFactory.CreateDev2Column("OtherColumn", "OtherColumnDescription", true,
                enDev2ColumnArgumentDirection.None);
            Assert.IsNotNull(dev2Column);
            Assert.IsFalse(dev2Column.Equals(other));
            var dev2Column2 = dev2Column;
            Assert.IsTrue(dev2Column == dev2Column2);
        }
    }
}
