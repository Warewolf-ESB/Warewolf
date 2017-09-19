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
        [TestCategory("SqlServer_FetchDataTable")]
        [ExpectedException(typeof(Exception))]

        public void SqlServer_FetchDataTable_ConnectionNotInitialized_ThrowsConnectFirstException()

        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                //sqlServer.FetchDataTable();

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

        public void SqlServer_FetchStoredProcedures_FunctionProcessorIsNull_ThrowsArgumentNullException()

        {
            //------------Setup for test--------------------------
            var sqlServer = new SqlServer();
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
        [TestCategory("SqlServer_FetchStoredProcedures")]

        public void SqlServer_FetchStoredProcedures_EmptyReturnsNothing()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataAdapter>();
            var somethingAdded = false;
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            factory.Setup(a => a.CreateTable(It.IsAny<IDataAdapter>(), LoadOption.OverwriteChanges)).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection",conn.Object);
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) => { somethingAdded = true; return true; };

                 sqlServer.FetchStoredProcedures(procProcessor,procProcessor);
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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();
            var queue = new Queue<DataTable>();
            
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.StoredProcedure, "Dave.Bob")).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, "sp_helptext 'Dave.Bob'")).Returns(helpTextCommand.Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add(new object[] { "Bob", "SQL_STORED_PROCEDURE", "Dave" });
            queue.Enqueue(dt);

            queue.Enqueue(new DataTable()); // no params

            factory.Setup(a => a.CreateTable(It.IsAny<IDataAdapter>(), LoadOption.OverwriteChanges)).Returns(queue.Dequeue);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var queue = new Queue<DataTable>();

            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.StoredProcedure, "Dave.Bob")).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, "sp_helptext 'Dave.Bob'")).Returns(helpTextCommand.Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add(new object[] { "Bob", "SQL_SCALAR_FUNCTION", "Dave" });
            queue.Enqueue(dt);

            queue.Enqueue(new DataTable()); // no params

            factory.Setup(a => a.CreateTable(It.IsAny<IDataAdapter>(), LoadOption.OverwriteChanges)).Returns(queue.Dequeue);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();
            var queue = new Queue<DataTable>();

            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.StoredProcedure, "Dave.Bob")).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, "sp_helptext 'Dave.Bob'")).Returns(helpTextCommand.Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_TABLE_VALUED_FUNCTION", "Dave");
            queue.Enqueue(dt);

            queue.Enqueue(new DataTable()); // no params

            factory.Setup(a => a.CreateTable(It.IsAny<IDataAdapter>(), LoadOption.OverwriteChanges)).Returns(queue.Dequeue);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                //------------Execute Test---------------------------
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procProcessor = (command, list, arg3, a) =>
                {
                    somethingAdded = true; return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> funcProcessor = (command, list, arg3, a) =>
                {
                    Assert.AreEqual("select * from Dave.Bob()",a);
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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                sqlServer.Connect("a");
                factory.Verify(a => a.CreateConnection(It.IsAny<string>()));
                conn.Verify(a=>a.Open());



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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var dbTran = new Mock<IDbTransaction>();
            conn.Setup(a=>a.BeginTransaction()).Returns(dbTran.Object );
            factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                sqlServer.Connect("a");
                sqlServer.BeginTransaction();
                factory.Verify(a => a.CreateConnection(It.IsAny<string>()));
                conn.Verify(a => a.Open());


                conn.Verify(a=>a.BeginTransaction());

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_RollbackTransaction")]
        
        public void SqlServer_FetchStoredProcedures_RollbackTransaction()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var dbTran = new Mock<IDbTransaction>();
            conn.Setup(a => a.BeginTransaction()).Returns(dbTran.Object);
            factory.Setup(a => a.CreateConnection(It.IsAny<string>())).Returns(conn.Object);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                sqlServer.Connect("a");
                sqlServer.BeginTransaction();
                sqlServer.RollbackTransaction();
                factory.Verify(a => a.CreateConnection(It.IsAny<string>()));
                conn.Verify(a => a.Open());

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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();
            var queue = new Queue<DataTable>();
            var param = new Mock<IDataParameterCollection>();
            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");
            mockCommand.Setup(a => a.Parameters).Returns(param.Object);
            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            var somethingAdded = false;
            var funcAdded = false;
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, GlobalConstants.SchemaQuery)).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.StoredProcedure, "Dave.Bob")).Returns(mockCommand.Object);
            factory.Setup(a => a.CreateCommand(It.IsAny<IDbConnection>(), CommandType.Text, "sp_helptext 'Dave.Bob'")).Returns(helpTextCommand.Object);
            DataTable dt = new DataTable();

            dt.Columns.Add("ROUTINE_NAME");
            dt.Columns.Add("ROUTINE_TYPE");
            dt.Columns.Add("SPECIFIC_SCHEMA");
            dt.Rows.Add("Bob", "SQL_TABLE_VALUED_FUNCTION", "Dave");
            queue.Enqueue(dt);

            var dtParams = new DataTable();
            dtParams.Columns.Add("PARAMETER_NAME");
            dtParams.Columns.Add("DATA_TYPE");
            dtParams.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            dtParams.Rows.Add("@moo", SqlDbType.VarChar, 25);
            queue.Enqueue(dtParams); // no params

            factory.Setup(a => a.CreateTable(It.IsAny<IDataAdapter>(), It.IsAny<LoadOption>())).Returns(queue.Dequeue);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
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
                param.Verify(a=>a.Add(It.IsAny<object>()),Times.Once);


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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();


            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(new Mock<IDataReader>().Object);
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob"});
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                //------------Execute Test---------------------------
          

                var output =sqlServer.FetchDatabases();
                Assert.AreEqual("Bob",output[0]);
                Assert.AreEqual("Dave",output[1] );
                factory.Verify(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases"), Times.Once());


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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
            var mockReader = new Mock<IDataReader>();


            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Throws(new DbEx("There is no text for object "));
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                //------------Execute Test---------------------------


                var output = sqlServer.FetchDatabases();
                Assert.AreEqual("Bob", output[0]);
                Assert.AreEqual("Dave", output[1]);
                factory.Verify(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases"), Times.Once());


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
            var factory = new Mock<IDbFactory>();
            var mockCommand = new Mock<IDbCommand>();
       


            mockCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Throws(new DbEx("There is no text for object "));
            mockCommand.Setup(a => a.CommandText).Returns("Dave.Bob");

            var helpTextCommand = new Mock<IDbCommand>();
            helpTextCommand.Setup(a => a.ExecuteReader(It.IsAny<CommandBehavior>())).Throws(new DbEx("There is no text for object "));
            DataTable dt = new DataTable();
            dt.Columns.Add("database_name");
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            var sqlServer = new SqlServer();
            try
            {
                PrivateObject pvt = new PrivateObject(sqlServer);
                pvt.SetField("_connection", conn.Object);
                //------------Execute Test---------------------------


                sqlServer.FetchDataTable(mockCommand.Object);
                factory.Verify(a=>a.CreateTable(It.IsAny<IDataAdapter>(),LoadOption.OverwriteChanges));
               


                //------------Assert Results-------------------------
            }
            finally
            {
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
            var sqlServer = new SqlServer();
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
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            factory.Setup(a => a.GetSchema(It.IsAny<IDbConnection>(), "Databases")).Returns(dt);
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            var sqlServer = new SqlServer();
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
        [TestCategory("SqlServer_FetchDataTable_addParams")]
        
        public void SqlServer_Connect_SetsCommandType()

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
            dt.Rows.Add(new object[] { "Bob" });
            dt.Rows.Add(new object[] { "Dave" });
            var conn = new Mock<IDbConnection>();
            conn.Setup(a => a.State).Returns(ConnectionState.Open);
            conn.Setup(a => a.ConnectionString).Returns("bob");
            var mock = new Mock<ISqlConnection>();
            var sqlServer = new SqlServer(mock.Object);
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect("bob", CommandType.StoredProcedure, "select * from ");
                mock.Verify(a => a.CreateCommand());
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
            var sqlServer = new SqlServer();
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
               Assert.IsFalse(  SqlServer.IsTableValueFunction(null,null));

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
    class DbEx :DbException
    {
        public DbEx(string message):base(message){}
    }
}
