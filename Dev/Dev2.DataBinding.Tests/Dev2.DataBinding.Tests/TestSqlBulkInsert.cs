using Dev2.Activities.Designers2.SqlBulkInsert;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.DataBinding.Tests
{
    [TestClass]
    public class TestSqlBulkInsert : UiBindingTestBase
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SqlBulkInsert_Bindings")]
        public void SqlBulkInsert_Bindings()
        {
            //------------Setup for test--------------------------
            var sqlBulkInsertDesigner = new SqlBulkInsertDesigner();
            //------------Execute Test---------------------------
            var bindingList = GetBindings(sqlBulkInsertDesigner);
            //------------Assert Results-------------------------
            Assert.IsTrue(bindingList.Count > 0);
            Assert.IsTrue(bindingList.ContainsBinding("ModelItem.DisplayName"));
            Assert.IsTrue(bindingList.ContainsBinding("IsSelectedDatabaseFocused"));
        }
    }
}