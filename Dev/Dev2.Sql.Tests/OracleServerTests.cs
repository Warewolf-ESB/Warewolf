using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Sql.Tests
{
    [TestClass]
    public class OracleServerTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OracleServer_Connect")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DeploymentItem("Oracle.ManagedDataAccess.dll")]
        
        public void OracleServer_Connect_ConnectionStringIsNull_ThrowsArgumentNullException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
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
        [TestCategory("OracleServer_Connect")]
        [ExpectedException(typeof(ArgumentException))]
        [DeploymentItem("Oracle.ManagedDataAccess.dll")]
        
        public void OracleServer_Connect_ConnectionStringIsInvalid_ThrowsArgumentException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
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
        [TestCategory("OracleServer_FetchDataTable")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DeploymentItem("Oracle.ManagedDataAccess.dll")]
        
        public void OracleServer_FetchDataTable_CommandIsNull_ThrowsArgumentNullException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataTable(null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OracleServer_FetchDatabases")]
        [ExpectedException(typeof(Exception))]
        [DeploymentItem("Oracle.ManagedDataAccess.dll")]
        
        public void OracleServer_FetchDatabases_ConnectionNotInitialized_ThrowsConnectFirstException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
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
        [TestCategory("OracleServer_FetchStoredProcedures")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DeploymentItem("Oracle.ManagedDataAccess.dll")]
        
        public void OracleServer_FetchStoredProcedures_FunctionProcessorIsNull_ThrowsArgumentNullException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
            try
            {
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procProcessor = (command, list, arg3,a) => false;

                sqlServer.FetchStoredProcedures(procProcessor, null);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        
        

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_FetchDataTable_addParams")]
        
        public void OracleServer_FetchDataTable_AddParams_VerifyAllAdded()
    
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();



            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new OracleServer(factory.Object);
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                pvt.SetField("_command",mockCommand.Object);
                //------------Execute Test---------------------------
                IDbDataParameter[] param = new IDbDataParameter[] { new SqlParameter("a", "a"), new SqlParameter("b", "b") };

                //SqlServer.AddParameters(mockCommand.Object,param);
                Assert.AreEqual(2,added.Count);


                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_FetchDataTable_addParams")]
        
        public void OracleServer_FetchDataTable_ConnectionsString()
    
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            var sqlServer = new OracleServer(factory.Object);
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                pvt.SetField("_command", mockCommand.Object);
                //------------Execute Test---------------------------

                Assert.AreEqual("bob", sqlServer.ConnectionString);

            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_FetchDataTable_addParams")]
        
        public void OracleServer_FetchDataTable_ConnectionsStringNull()
    
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            var sqlServer = new OracleServer(factory.Object);
            try
            {
                Assert.IsNull( sqlServer.ConnectionString);

            }
            finally
            {
                sqlServer.Dispose();
            }
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_FetchDataTable_addParams")]
        
        public void OracleServer_CreateCommand_CreateCommand()
    
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new OracleServer(factory.Object);
            try
            {

                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                //------------Execute Test---------------------------
                sqlServer.CreateCommand();
                conn.Verify(a => a.CreateCommand());
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_IsTableValueFunction")]
        
        public void OracleServer_IsTableValueFunction_InvalidRow()
    
        {
            //------------Setup for test--------------------------

        
            //------------Execute Test---------------------------
            Assert.IsFalse(  SqlServer.IsTableValueFunction(null,null));

            //------------Assert Results-------------------------
       
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_IsFunction")]

        
        public void OracleServer_IsFunction_InvalidRow()
    
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            Assert.IsFalse(SqlServer.IsFunction(null, null));

            //------------Assert Results-------------------------

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OracleServer_IsSp")]
        
        public void OracleServer_IsSP_InvalidRow()
    
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            Assert.IsFalse(SqlServer.IsStoredProcedure(null, null));

            //------------Assert Results-------------------------

        }




        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("OracleServer_CreateCommand")]
        [ExpectedException(typeof(Exception))]
        
        public void OracleServer_CreateCommand_ConnectionNotInitialized_ThrowsConnectFirstException()
    
        {
            //------------Setup for test--------------------------
            var sqlServer = new OracleServer();
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