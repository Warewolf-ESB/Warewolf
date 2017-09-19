/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        [ExpectedException(typeof(ArgumentNullException))]

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
        [TestCategory("SqlServer_FetchDataTable")]
        [ExpectedException(typeof(Exception))]

        public void SqlServer_FetchDataTable_ConnectionNotInitialized_ThrowsConnectFirstException()

        {
            //------------Setup for test--------------------------
            var connBuilder = new Mock<IConnectionBuilder>();
            var sqlServer = new SqlServer(connBuilder.Object);
            try
            {
                //------------Execute Test---------------------------
                sqlServer.FetchDataTable("");

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
            var conBuilder = new Mock<IConnectionBuilder>();
            var sqlServer = new SqlServer(conBuilder.Object);
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

        public void SqlServer_FetchStoredProcedures_FunctionProcessorIsNull_ThrowsArgumentNullException()

        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) => false;

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
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_EmptyReturnsNothing()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            var somethingAdded = false;
           
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            mockCommand.SetupSequence(command => command.ExecuteReader())
                .Returns(dt.CreateDataReader())
                .Returns(new DataTable().CreateDataReader());
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) => { somethingAdded = true; return true; };

                sqlServer.FetchStoredProcedures(procProcessor, procProcessor);
                Assert.IsFalse(somethingAdded);
                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_SPReturnsSPs()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();

            mockCommand.Setup(a => a.ExecuteReader()).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            //factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            //factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.StoredProcedure, "Dave.Bob")).Returns(mockCommand.Object);
            //factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, "sp_helptext 'Dave.Bob'")).Returns(helpTextCommand.Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_STORED_PROCEDURE", "Dave");

            mockCommand.SetupSequence(command => command.ExecuteReader())
                .Returns(dt.CreateDataReader());
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) =>
                    {
                        somethingAdded = true; return true;
                    };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> funcProcessor = (command, list, arg3, a) =>
                {
                    funcAdded = true; return true;
                };

                sqlServer.FetchStoredProcedures(procProcessor, funcProcessor);
                Assert.IsTrue(somethingAdded);
                Assert.IsFalse(funcAdded);



                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_FuncReturnsSPs()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();

            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_SCALAR_FUNCTION", "Dave");

            mockCommand.SetupSequence(command => command.ExecuteReader())
                .Returns(dt.CreateDataReader())
                .Returns(new DataTable().CreateDataReader());

            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) =>
                {
                    somethingAdded = true; return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> funcProcessor = (command, list, arg3, a) =>
                {
                    funcAdded = true; return true;
                };

                sqlServer.FetchStoredProcedures(procProcessor, funcProcessor);
                Assert.IsTrue(funcAdded);
                Assert.IsFalse(somethingAdded);



                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_TableValuesProcFunc_NotReturned()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.SetupAllProperties();
            var somethingAdded = false;
            var funcAdded = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_TABLE_VALUED_FUNCTION", "Dave");

            mockCommand.SetupSequence(command => command.ExecuteReader())
                .Returns(dt.CreateDataReader())
                .Returns(new DataTable().CreateDataReader());
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection("")).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) =>
                {
                    somethingAdded = true; return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> funcProcessor = (command, list, arg3, a) =>
                {
                    Assert.AreEqual("select * from Dave.Bob()", a);
                    funcAdded = true;
                    return true;
                };

                sqlServer.FetchStoredProcedures(procProcessor, funcProcessor);
                Assert.IsTrue(funcAdded);
                Assert.IsFalse(somethingAdded);



                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_Connect")]

        public void SqlServer_FetchStoredProcedures_Connect_VerifyUnderlyingConnectionIsCalled()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Closed);
            factory.Setup(a => a.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("a");
                factory.Verify(a => a.BuildConnection(It.IsAny<string>()));
                conn.Verify(a => a.Open());



                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_BeginTransaction")]
        public void SqlServer_FetchStoredProcedures_BeginTransaction()
        {
            //------------Setup for test--------------------------
            var conBuilder = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var dbTran = new Mock<IDbTransaction>();
            conn.Setup(a => a.BeginTransaction()).Returns(dbTran.Object);
            conBuilder.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(conBuilder.Object);

            sqlServer.Connect("a");
            sqlServer.BeginTransaction();
            conn.Verify(a => a.Open(), Times.Never);//Connection is open

            //------------Assert Results-------------------------
            conn.Verify(a => a.BeginTransaction());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_RollbackTransaction")]

        public void SqlServer_FetchStoredProcedures_RollbackTransaction()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var dbTran = new Mock<IDbTransaction>();
            conn.Setup(a => a.BeginTransaction()).Returns(dbTran.Object);
            factory.Setup(a => a.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                
                sqlServer.Connect("a");
                sqlServer.BeginTransaction();
                sqlServer.RollbackTransaction();
                factory.Verify(a => a.BuildConnection(It.IsAny<string>()));
                conn.Verify(a => a.Open(), Times.Never);

                dbTran.Verify(a => a.Rollback());
                dbTran.Verify(a => a.Dispose());
                conn.Verify(a => a.BeginTransaction());

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_TableValuesProcWithParamsFunc_NotReturned()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            DataTable dt = new DataTable();

            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_TABLE_VALUED_FUNCTION", "Dave");
            var queue = new Queue<DataTable>();
            queue.Enqueue(dt);

            var dtParams = new DataTable();
            dtParams.Columns.Add("PARAMETER_NAME");
            dtParams.Columns.Add("DATA_TYPE");
            dtParams.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            dtParams.Rows.Add("@moo", SqlDbType.VarChar, 25);
            queue.Enqueue(dtParams); // no params
            
            var param = new Mock<IDataParameterCollection>();
            mockCommand.SetupSequence(command => command.ExecuteReader())
                .Returns(dt.CreateDataReader())
                .Returns(dtParams.CreateDataReader());
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            mockCommand.Setup(a => a.Parameters).Returns(param.Object);
            
            var somethingAdded = false;
            var funcAdded = false;
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection("")).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) =>
                {
                    somethingAdded = true; return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> funcProcessor = (command, list, arg3, a) =>
                {
                    Assert.AreEqual("select * from Dave.Bob(@moo)", a);
                    funcAdded = true;
                    return true;
                };

                sqlServer.FetchStoredProcedures(procProcessor, funcProcessor);
                Assert.IsTrue(funcAdded);
                Assert.IsFalse(somethingAdded);
                param.Verify(a => a.Add(It.IsAny<object>()), Times.Once);
                mockCommand.VerifySet(command => command.CommandType = CommandType.Text, Times.Exactly(2));
                mockCommand.VerifySet(command => command.CommandText = GlobalConstants.SchemaQuery);
                mockCommand.VerifySet(command => command.CommandText = "Dave.Bob");
                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchDatabases_CallsSchemaFunctionWithCorrectyParams()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");
            mockCommand.Setup(a => a.ExecuteReader()).Returns(dt.CreateDataReader);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.GetSchema( "Databases")).Returns(dt);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------


                var output = sqlServer.FetchDatabases();
                Assert.AreEqual("Bob", output[0]);
                Assert.AreEqual("Dave", output[1]);
                conn.Verify(a => a.GetSchema("Databases"), Times.Once());


                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchDatabases_OnException()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Throws(new DbEx("There is no text for object "));
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");
            mockCommand.Setup(a => a.ExecuteReader()).Returns(dt.CreateDataReader);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.GetSchema("Databases")).Returns(dt);
            conn.SetupSequence(a => a.CreateCommand())
                .Returns(mockCommand.Object)
                .Returns(helpTextCommand.Object);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                sqlServer.Connect("");
                //------------Execute Test---------------------------


                var output = sqlServer.FetchDatabases();
                Assert.AreEqual("Bob", output[0]);
                Assert.AreEqual("Dave", output[1]);
                conn.Verify(a => a.GetSchema("Databases"), Times.Once());


                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable")]
        public void SqlServer_FetchDataTable_OnException()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader())
                .Throws(new DbEx("There is no text for object "));
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>()))
                .Throws(new DbEx("There is no text for object "));
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");

            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.GetSchema("Databases")).Returns(dt);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            conn.Setup(connection => connection.CreateCommand()).Returns(mockCommand.Object);
            var sqlServer = new SqlServer(factory.Object);

            //------------Execute Test---------------------------
            try
            {
                sqlServer.Connect("");
              sqlServer.FetchDataTable(mockCommand.Object);

                //------------Assert Results-------------------------
               
            }
            finally
            {
                mockCommand.Verify(a => a.ExecuteReader());
                sqlServer.Dispose();
            }
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_FetchDataTable_AddParams_VerifyAllAdded()

        {
            ////------------Setup for test--------------------------
            //var factory = new Mock<IDbFactory>();
            //var mockCommand = new Mock<IDbCommand>();



            //mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            //var added = new SqlCommand().Parameters;
            //mockCommand.Setup(a => a.Parameters).Returns(added);
            //var helpTextCommand = new Mock<IDbCommand>();
            //helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //DataTable dt = new DataTable();
            //dt.Columns.Add("database_name");
            //dt.Rows.Add(new object[] { "Bob" });
            //dt.Rows.Add(new object[] { "Dave" });
            //factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            //var conn = new Mock<IDbConnection>();
            //conn.Setup(a => a.State).Returns(ConnectionState.Open);
            //var sqlServer = new SqlServer();
            //try
            //{
            //    PrivateObject pvt = new PrivateObject(sqlServer);
            //    pvt.SetField("_connection", conn.Object);
            //    pvt.SetField("_command",mockCommand.Object);
            //    //------------Execute Test---------------------------
            //    IDbDataParameter[] param = new IDbDataParameter[] { new SqlParameter("a", "a"), new SqlParameter("b", "b") };

            //    SqlServer.AddParameters(mockCommand.Object,param);
            //    Assert.AreEqual(2,added.Count);


            //    //------------Assert Results-------------------------
            //}
            //finally
            //{
            //    sqlServer.Dispose();
            //}
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_FetchDataTable_ConnectionsString()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");

            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.GetSchema("Databases")).Returns(dt);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(builder => builder.BuildConnection("bob")).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect("bob");
                factory.Verify(builder => builder.BuildConnection("bob"));

            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_FetchDataTable_ConnectionsStringNull()

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
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            var sqlServer = new SqlServer();
            try
            {
                Assert.IsNull(sqlServer.ConnectionString);

            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_Connect_SetsCommandTypeAndCommandType()

        {
            //------------Setup for test--------------------------
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var mock = new Mock<IConnectionBuilder>();
            mock.Setup(builder => builder.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(mock.Object);
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect("bob", CommandType.StoredProcedure, "select * from ");
                PrivateObject privateObject = new PrivateObject(sqlServer);
                var commandText = (string)privateObject.GetField("_commantText");
                var commandType = (CommandType)privateObject.GetField("_commandType");
                Assert.AreEqual(commandType, CommandType.Text);
                Assert.AreEqual("select * from ", commandText);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_CreateCommand_CreateCommand()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IConnectionBuilder>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(a => a.Parameters).Returns(added);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add("Bob");
            dt.Rows.Add("Dave");
            var conn = new Mock<ISqlConnection>();
            conn.Setup(a => a.GetSchema("Databases")).Returns(dt);
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            factory.Setup(a => a.BuildConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer(factory.Object);
            try
            {

                //------------Execute Test---------------------------
                sqlServer.Connect("homeString");
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
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_FetchDataSet_CallsNestedFactory()

        {
            ////------------Setup for test--------------------------
            //var factory = new Mock<IDbFactory>();
            //var mockCommand = new Mock<IDbCommand>();
            //mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            //var added = new SqlCommand().Parameters;
            //mockCommand.Setup(a => a.Parameters).Returns(added);
            //var helpTextCommand = new Mock<IDbCommand>();
            //helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //DataTable dt = new DataTable();
            //dt.Columns.Add("database_name");
            //dt.Rows.Add(new object[] { "Bob" });
            //dt.Rows.Add(new object[] { "Dave" });
            //factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            //var conn = new Mock<IDbConnection>();
            //conn.Setup(a => a.State).Returns(ConnectionState.Open);
            //conn.Setup(a => a.ConnectionString).Returns("bob");
            //conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            //factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            //factory.Setup(a => a.FetchDataSet(It.IsAny<DbCommand>())).Returns(new DataSet());
            //var sqlServer = new SqlServer();
            //try
            //{

            //    PrivateObject pvt = new PrivateObject(sqlServer);
            //    pvt.SetField("_connection", conn.Object);
            //    //------------Execute Test---------------------------
            //    sqlServer.FetchDataSet(mockCommand.Object, new SqlParameter[] { });
            //    factory.Verify(a=>a.FetchDataSet(It.IsAny<IDbCommand>()));
            //}
            //finally
            //{
            //    sqlServer.Dispose();
            //}
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchDataTable_addParams")]

        public void SqlServer_FetchDataSet_CallsNestedFactory_ParamsOnly_UsesNestedCommand()

        {
            ////------------Setup for test--------------------------
            //var factory = new Mock<IDbFactory>();
            //var mockCommand = new Mock<IDbCommand>();
            //mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            //var added = new SqlCommand().Parameters;
            //mockCommand.Setup(a => a.Parameters).Returns(added);
            //var helpTextCommand = new Mock<IDbCommand>();
            //helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            //DataTable dt = new DataTable();
            //dt.Columns.Add("database_name");
            //dt.Rows.Add(new object[] { "Bob" });
            //dt.Rows.Add(new object[] { "Dave" });
            //factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            //var conn = new Mock<IDbConnection>();
            //conn.Setup(a => a.State).Returns(ConnectionState.Open);
            //conn.Setup(a => a.ConnectionString).Returns("bob");
            //conn.Setup(a => a.CreateCommand()).Returns(mockCommand.Object);
            //factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            //factory.Setup(a => a.FetchDataSet(It.IsAny<DbCommand>())).Returns(new DataSet());
            //var sqlServer = new SqlServer();
            //try
            //{

            //    PrivateObject pvt = new PrivateObject(sqlServer);
            //    pvt.SetField("_connection", conn.Object);
            //    pvt.SetField("_command", mockCommand.Object);
            //    //------------Execute Test---------------------------
            //    sqlServer.FetchDataSet( new SqlParameter[] { });
            //    factory.Verify(a => a.FetchDataSet(It.IsAny<IDbCommand>()));
            //}
            //finally
            //{
            //    sqlServer.Dispose();
            //}
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_IsTableValueFunction")]

        public void SqlServer_IsTableValueFunction_InvalidRow()

        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            Assert.IsFalse(SqlServer.IsTableValueFunction(null, null));

            //------------Assert Results-------------------------

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_IsFunction")]


        public void SqlServer_IsFunction_InvalidRow()

        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            Assert.IsFalse(SqlServer.IsFunction(null, null));

            //------------Assert Results-------------------------

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_IsSp")]

        public void SqlServer_IsSP_InvalidRow()

        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            Assert.IsFalse(SqlServer.IsStoredProcedure(null, null));

            //------------Assert Results-------------------------

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
    class DbEx : DbException
    {
        public DbEx(string message) : base(message) { }
    }
}
