using System;
using System.Data;
using Dev2.Activities.SqlBulkInsert;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SqlBulkCopyWrapperTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkCopyWrapper_WriteToServer")]
        [ExpectedException(typeof(ArgumentException))]
        // ReSharper disable InconsistentNaming
        public void SqlBulkCopyWrapper_WriteToServer_WhenNullBulkCopyObject_ExpectException()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var sqlBulkCopyWrapper = new SqlBulkCopyWrapper(null);
            DataTable dataTable = new DataTable("myTable");

            //------------Execute Test---------------------------
            sqlBulkCopyWrapper.WriteToServer(dataTable);

        }
    }
}
