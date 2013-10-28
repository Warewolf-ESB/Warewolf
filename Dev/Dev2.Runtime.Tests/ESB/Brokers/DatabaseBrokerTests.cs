using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Tests.Runtime.ESB.Brokers
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DatabaseBrokerTests
    {
        #region MsSql

        [TestMethod]
        [TestCategory("MsSqlBrokerRegressionTest")]
        [Description("Test for MsSqlBroker's GetDatabases method: An unsorted datatable is passed to GetDatabases and a sorted list of strings is expected to be returned")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void MsSqlBroker_MsSqlBrokerRegressionTest_GetDatabases_ListIsSorted()
        // ReSharper restore InconsistentNaming
        {
            //Init
            var table = new DataTable();
            table.Columns.Add("database_name", typeof(string));
            table.Rows.Add("bbb");
            table.Rows.Add("ccc");
            table.Rows.Add("aaa");
            var expected = new List<string> { "aaa", "bbb", "ccc" };
            //Exe
            var actual = new MsSqlBroker().GetDatabases(table);
            //Assert
            CollectionAssert.AreEqual(expected, actual, "GetDatabases returned an unsorted list");
        }
        
        #endregion
    }
}
