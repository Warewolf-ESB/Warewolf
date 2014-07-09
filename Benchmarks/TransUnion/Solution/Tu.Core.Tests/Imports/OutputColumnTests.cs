using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Imports;
using Tu.Rules;

namespace Tu.Core.Tests.Imports
{
    [TestClass]
    public class OutputColumnTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OutputColumn_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OutputColumn_Constructor_NullColumnName_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var outputColumn = new OutputColumn(null, null);

            //------------Assert Results-------------------------
        }   

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OutputColumn_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OutputColumn_Constructor_NullColumnType_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var outputColumn = new OutputColumn("xx", null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OutputColumn_IsValid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OutputColumn_IsValid_NullDataRow_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var outputColumn = new OutputColumn("xx", typeof(string));

            //------------Execute Test---------------------------
            outputColumn.IsValid(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OutputColumn_IsValid")]
        public void OutputColumn_IsValid_DBNullValue_ConvertedToNull()
        {
            //------------Setup for test--------------------------
            var dt = new DataTable();
            dt.Columns.Add("xx");
            var dr = dt.Rows.Add(Convert.DBNull);

            var actualFieldValue = new object();
            var isValid = new IsValidFunc((ruleName, fieldName, fieldValue) =>
            {
                actualFieldValue = fieldValue;
                return new ValidationResult();
            });


            var outputColumn = new OutputColumn("xx", typeof(string), "theRule", isValid);
            Assert.IsNotNull(actualFieldValue);

            //------------Execute Test---------------------------
            outputColumn.IsValid(dr);

            //------------Assert Results-------------------------
            Assert.AreNotEqual(Convert.DBNull, actualFieldValue);
            Assert.IsNull(actualFieldValue);
        }
    }
}
