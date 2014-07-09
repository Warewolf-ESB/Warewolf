using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Extensions;

namespace Tu.Core.Tests.Extensions
{
    [TestClass]
    public class DataTableUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataTableUtils_ToCsv")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DataTableUtils_ToCsv_NullDataTable_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            DataTableUtils.ToCsv(null, null, null, null);

            //------------Assert Results-------------------------
        }

    }
}
