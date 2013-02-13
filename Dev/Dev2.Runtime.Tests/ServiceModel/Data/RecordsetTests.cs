using System;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // PBI: 801
    // BUG: 8477

    /// <author>trevor.williams-ros</author>
    /// <date>2013/02/13</date>
    [TestClass]
    public class RecordsetTests
    {
        #region CTOR

        [TestMethod]
        public void ConstructorWithNoParametersExpectedInitializesListProperties()
        {
            var rs = new Recordset();
            Assert.IsNotNull(rs.Fields);
            Assert.IsNotNull(rs.Records);
        }

        #endregion

        #region AddRecord

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddRecordWithNullExpectedThrowsArgumentNullException()
        {
            var rs = new Recordset();
            rs.AddRecord(null);
        }

        [TestMethod]
        public void AddRecordWithValuesAndNoFieldsExpectedAddsRecordWithoutValues()
        {
            var values = new[]
            {
                "value 1",
                "value 2"
            };
            var rs = new Recordset();
            var record = rs.AddRecord(fieldIndex => values[fieldIndex]);
            Assert.AreEqual(0, record.Count);
        }

        [TestMethod]
        public void AddRecordWithValuesAndFieldsExpectedAddsRecordWithValues()
        {
            var values = new[]
            {
                "value 1",
                "value 2"
            };
            var rs = new Recordset();
            rs.Fields.AddRange(new[] { new RecordsetField(), new RecordsetField() });
            var record = rs.AddRecord(fieldIndex => values[fieldIndex]);
            Assert.AreEqual(2, record.Count);
            Assert.AreEqual(record[0].Value, values[0]);
            Assert.AreEqual(record[1].Value, values[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void AddRecordWithDifferentValuesAndFieldCountsExpectedThrowsIndexOutOfRangeException()
        {
            var values = new[]
            {
                "value 1",
                "value 2"
            };
            var rs = new Recordset();
            rs.Fields.AddRange(new[] { new RecordsetField(), new RecordsetField(), new RecordsetField() });
            rs.AddRecord(fieldIndex => values[fieldIndex]);
        }
        #endregion

        #region ToString

        [TestMethod]
        public void ToStringExpectedReturnsJson()
        {
            var rs = new Recordset { Name = "MyRec" };
            var jsonStr = rs.ToString();
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
            Assert.AreEqual(rs.Name, jsonObj.Name.Value);
        }

        #endregion

    }
}
