/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        #region ToString

      
        #endregion

        #region NewRecord Tests

        [TestMethod]
        public void NewRecordExpectedAddANewRecordToRecordset()
        {
            var rs = new Recordset { Name = "MyRec" };
            rs.Records.Add(rs.NewRecord());
            Assert.AreEqual(1, rs.Records.Count);
            Assert.AreEqual("MyRec(1)", rs.Records[0].Label);
        }

        #endregion

        #region SetValue Tests

        [TestMethod]
        public void SetValueExpectedAddANewRecordToRecordset()
        {
            var rs = new Recordset { Name = "MyRec" };
            rs.Records.Add(rs.NewRecord());
            rs.Fields.Add(new RecordsetField { Name = "MyField", Alias = "MyField" });
            rs.SetValue(0, 0, "MyTestData");
            Assert.AreEqual("MyTestData", rs.Records[0][0].Value);
        }

        [TestMethod]
        public void RecordSet_AddColumn_ShouldAddColumnOnTheRecSet()
        {
            var recordset = new RecordSet { Name = "MyRec" };
            Assert.IsNotNull(recordset);
        }

        [TestMethod]
        public void SetValueWithRecordNotExistiongExpectedAddANewRecordToRecordset()
        {
            var rs = new Recordset { Name = "MyRec" };
            rs.Fields.Add(new RecordsetField { Name = "MyField", Alias = "MyField" });
            rs.SetValue(0, 0, "MyTestData");
            Assert.AreEqual("MyTestData", rs.Records[0][0].Value);
        }

        [TestMethod]
        public void SetValueSecondMethodExpectedAddANewRecordToRecordset()
        {
            var rs = new Recordset { Name = "MyRec" };
            var rsr = rs.NewRecord();
            rs.Records.Add(rsr);
            rs.Fields.Add(new RecordsetField { Name = "MyField", Alias = "MyField" });
            rs.Fields.Add(new RecordsetField { Name = "MyField2", Alias = "MyField2" });
            rs.SetValue(ref rsr, 0, "MyTestData");
            Assert.AreEqual("MyTestData", rs.Records[0][0].Value);
            rs.SetValue(ref rsr, 1, "MyTestData1");
            rs.SetValue(ref rsr, 0, "MyTestData3");
            Assert.AreEqual("MyTestData1", rs.Records[0][1].Value);
            Assert.AreEqual("MyTestData3", rs.Records[0][0].Value);
        }

        #endregion
       
    }
}
