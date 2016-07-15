using Dev2.Data.Binary_Objects;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.EqualityComparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class ExtensionClassesTests
    {
        [TestMethod]
        public void DataListConstants_ShouldHave_AllConstants()
        {            
            Assert.IsNotNull(DataListConstants.DefaultCase);
            Assert.IsNotNull(DataListConstants.DefaultDecision);
            Assert.IsNotNull(DataListConstants.DefaultStack);
            Assert.IsNotNull(DataListConstants.DefaultSwitch);
            Assert.IsNotNull(DataListConstants.EmptyRowStartIdx);
            Assert.IsNotNull(DataListConstants.MinRowSize);
            Assert.IsNotNull(DataListConstants.RowGrowthFactor);
        }

        [TestMethod]
        public void Dev2ColumnComparer_ShouldHave()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
        }
        [TestMethod]
        public void Dev2ColumnComparer_GetHashCode_ShouldHaveHashCode()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
            var dev2Column = DataListFactory.CreateDev2Column("", "", false, enDev2ColumnArgumentDirection.None);
            var hashCode = comparer.GetHashCode(dev2Column);
            Assert.IsNotNull(hashCode);
        }
        [TestMethod]
        public void Dev2ColumnComparer_Equals_Should()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
            var dev2Column = DataListFactory.CreateDev2Column("", "Value", false, enDev2ColumnArgumentDirection.None);
            var dev2Column2 = DataListFactory.CreateDev2Column("", "SomeValue", false, enDev2ColumnArgumentDirection.Input);
            var equals = comparer.Equals(dev2Column, dev2Column2);
            Assert.IsFalse(equals);
        }
    }
}
