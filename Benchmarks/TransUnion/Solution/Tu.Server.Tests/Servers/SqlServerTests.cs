using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Servers;

namespace Tu.Server.Tests.Servers
{
    [TestClass]
    public class SqlServerTests
    {
        public const string DemoDbConnectionString = "Data Source=RSAKLFSVRGENDEV;Initial Catalog=DemoDB;Integrated Security=SSPI;";

        [TestMethod]
        [TestCategory("SqlServer_Constructor")]
        [Description("SqlServer Constructor with null connection string throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_UnitTest_ConstructorWithNullConnectionString_ThrowsArgumentNullException()
        {
            var server = new SqlServer(null);
        }

        [TestMethod]
        [TestCategory("SqlServer_Constructor")]
        [Description("SqlServer Constructor with invalid connection string throws ArgumentException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentException))]
        public void SqlServer_UnitTest_ConstructorWithInvalidConnectionString_ThrowsArgumentException()
        {
            var server = new SqlServer("xxxxxx");
        }

        [TestMethod]
        [TestCategory("SqlServer_Constructor")]
        [Description("SqlServer Constructor with valid connection string creates closed connection.")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_ConstructorWithValidConnectionString_CreatesClosedConnection()
        {
            var server = new SqlServer(DemoDbConnectionString);
            Assert.IsNotNull(server.Connection, "Constructor di not create connection.");
            Assert.AreEqual(ConnectionState.Closed, server.Connection.State, "Constructor created open connection.");
        }

        [TestMethod]
        [TestCategory("SqlServer_FetchDataTable")]
        [Description("SqlServer FetchDataTable with null command text throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_UnitTest_FetchDataTableWithNullCommandText_ThrowsArgumentNullException()
        {
            var server = new SqlServer(DemoDbConnectionString);
            var dt = server.FetchDataTable(null, CommandType.StoredProcedure);
        }

        [TestMethod]
        [TestCategory("SqlServer_FetchDataTable")]
        [Description("SqlServer FetchDataTable with invalid command text throws SqlException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(SqlException))]
        public void SqlServer_UnitTest_FetchDataTableWithInvalidCommandText_ThrowsSqlException()
        {
            var server = new SqlServer(DemoDbConnectionString);
            var dt = server.FetchDataTable("xxxxx", CommandType.StoredProcedure);
        }

        [TestMethod]
        [TestCategory("SqlServer_FetchDataTable")]
        [Description("SqlServer FetchDataTable with valid command text returns DataTable.")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_FetchDataTableWithValidCommandText_ReturnsDataTable()
        {
            using(var server = new SqlServer(DemoDbConnectionString))
            {
                var dt = server.FetchDataTable("FetchClient", CommandType.StoredProcedure, new SqlParameter("@clientName", "demo"));
                Assert.IsNotNull(dt, "FetchDataTable did not return DataTable.");
                Assert.AreEqual(2, dt.Rows.Count, "FetchDataTable did not return DataTable.");
            }
        }

        [TestMethod]
        [TestCategory("SqlServer_FetchDataTable")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_FetchDataTableWithValidCommandText_ClosesConnection()
        {
            using(var server = new SqlServer(DemoDbConnectionString))
            {
                var dt = server.FetchDataTable("FetchClient", CommandType.StoredProcedure, new SqlParameter("@clientName", "demo"));
                Assert.AreEqual(ConnectionState.Closed, server.Connection.State);
            }
        }


        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuery")]
        [Description("SqlServer ExecuteNonQuery with no parameters does not add parameters.")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_FetchDataTableWithZeroLengthParameters_DoesNotAddParameters()
        {
            var server = new SqlServer(DemoDbConnectionString);
            var result = server.FetchDataTable("SELECT clientID, clientName FROM clientDetails WHERE clientName = 'demo'", CommandType.Text, new SqlParameter[0]);
            Assert.IsNotNull(result, "FetchDataTable did not return DataTable.");
            Assert.AreEqual(2, result.Rows.Count, "FetchDataTable did not return DataTable.");
        }

        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuery")]
        [Description("SqlServer ExecuteNonQuery with no parameters does not add parameters.")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_FetchDataTableWithNullParameters_DoesNotAddParameters()
        {
            var server = new SqlServer(DemoDbConnectionString);
            var result = server.FetchDataTable("SELECT clientID, clientName FROM clientDetails WHERE clientName = 'demo'", CommandType.Text);
            Assert.IsNotNull(result, "FetchDataTable did not return DataTable.");
            Assert.AreEqual(2, result.Rows.Count, "FetchDataTable did not return DataTable.");
        }


        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuery")]
        [Description("SqlServer ExecuteNonQuery with null command text throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_UnitTest_ExecuteNonQueryWithNullCommandText_ThrowsArgumentNullException()
        {
            var server = new SqlServer(DemoDbConnectionString);
            server.ExecuteNonQuery(null, CommandType.StoredProcedure);
        }

        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuery")]
        [Description("SqlServer ExecuteNonQuery with invalid command text throws SqlException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(SqlException))]
        public void SqlServer_UnitTest_ExecuteNonQueryWithInvalidCommandText_ThrowsSqlException()
        {
            var server = new SqlServer(DemoDbConnectionString);
            server.ExecuteNonQuery("xxxxx", CommandType.StoredProcedure);
        }

        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuery")]
        [Description("SqlServer ExecuteNonQuery with valid command text returns DataTable.")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_ExecuteNonQueryWithValidCommandText_ReturnsDataTable()
        {
            using(var server = new SqlServer(DemoDbConnectionString))
            {
                server.ExecuteNonQuery("FetchClient", CommandType.StoredProcedure, new SqlParameter("@clientName", "demo"));
            }
        }


        [TestMethod]
        [TestCategory("SqlServer_ExecuteNonQuer")]
        [Owner("Trevor Williams-Ros")]
        public void SqlServer_UnitTest_ExecuteNonQuerWithValidCommandText_ClosesConnection()
        {
            using(var server = new SqlServer(DemoDbConnectionString))
            {
                server.ExecuteNonQuery("FetchClient", CommandType.StoredProcedure, new SqlParameter("@clientName", "demo"));
                Assert.AreEqual(ConnectionState.Closed, server.Connection.State);
            }
        }

    }
}
