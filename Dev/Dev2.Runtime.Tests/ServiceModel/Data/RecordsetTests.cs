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
