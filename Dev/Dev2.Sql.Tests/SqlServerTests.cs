using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Sql.Tests
{
    [TestClass]    
    public class SqlServerTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_Connect_ConnectionStringIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect(null, CommandType.Text, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        [ExpectedException(typeof(ArgumentException))]
        public void SqlServer_Connect_ConnectionStringIsInvalid_ThrowsArgumentException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect("xxx", CommandType.Text, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_Connect_CommandTextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect("Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=SSPI;", CommandType.Text, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataSet")]
        [ExpectedException(typeof(Exception))]
        public void SqlServer_FetchDataSet_ConnectionNotInitialized_ThrowsConnectFirstException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataSet();

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataSet")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_FetchDataSet_CommandIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataSet((SqlCommand)null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataTable")]
        [ExpectedException(typeof(Exception))]
        public void SqlServer_FetchDataTable_ConnectionNotInitialized_ThrowsConnectFirstException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataTable();

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataTable")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_FetchDataTable_CommandIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataTable((IDbCommand)null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDatabases")]
        [ExpectedException(typeof(Exception))]
        public void SqlServer_FetchDatabases_ConnectionNotInitialized_ThrowsConnectFirstException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDatabases();

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchStoredProcedures")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_FetchStoredProcedures_ProcedureProcessorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchStoredProcedures(null, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchStoredProcedures")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_FetchStoredProcedures_FunctionProcessorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, bool> procProcessor = (command, list, arg3) => false;

                sqlServer.FetchStoredProcedures(procProcessor, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_CreateCommand")]
        [ExpectedException(typeof(Exception))]
        public void SqlServer_CreateCommand_ConnectionNotInitialized_ThrowsConnectFirstException()
        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.CreateCommand();

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }
    }
}
