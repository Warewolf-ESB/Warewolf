/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MySql.Data.MySqlClient;

namespace Dev2.Sql.Tests
{
    /// <summary>
    /// Pure-unit coverage for <see cref="MySqlServer"/>. The class wraps the
    /// MySql .NET connector and previously had 0% coverage in the merged
    /// pipeline Cobertura report (1031 uncovered lines). These tests exercise
    /// the argument validation, lifecycle (ctor/Dispose), property accessors,
    /// transaction helpers, the static <see cref="MySqlServer.AddParameters"/>
    /// helper, and the various Verify* error paths without requiring a live
    /// MySQL server.
    /// </summary>
    [TestClass]
    public class MySqlServerTests
    {
        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_DefaultCtor_DoesNotThrow()
        {
            using (var server = new MySqlServer())
            {
                Assert.IsFalse(server.IsConnected);
                Assert.IsNull(server.ConnectionString);
                Assert.IsNull(server.CommandTimeout);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_FactoryCtor_StoresFactory()
        {
            var factory = new Mock<IDbFactory>();
            using (var server = new MySqlServer(factory.Object))
            {
                Assert.IsFalse(server.IsConnected);
                Assert.IsNull(server.ConnectionString);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_CommandTimeout_RoundTrips()
        {
            using (var server = new MySqlServer())
            {
                server.CommandTimeout = 73;
                Assert.AreEqual(73, server.CommandTimeout);
            }
        }

        // -----------------------------------------------------------------
        // Connect - argument validation
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_Connect3_NullCommandText_ThrowsArgumentNullException()
        {
            var factory = new Mock<IDbFactory>();
            factory.Setup(f => f.CreateConnection(It.IsAny<string>()))
                   .Returns(new MySqlConnection());

            using (var server = new MySqlServer(factory.Object))
            {
                server.Connect("server=localhost", CommandType.Text, null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_Connect3_SelectQuery_ForcesTextCommandType()
        {
            // The Connect overload switches CommandType to Text when the
            // command starts with "select " regardless of what was passed in.
            // CreateConnection succeeds (MySqlConnection.Open will fail) so
            // we expect the call to throw a MySqlException once it tries to
            // open the connection - but the CommandType branch has already
            // executed and the factory was asked to create a Text command.
            var factory = new Mock<IDbFactory>();
            factory.Setup(f => f.CreateConnection(It.IsAny<string>()))
                   .Returns(new MySqlConnection());
            factory.Setup(f => f.CreateCommand(It.IsAny<IDbConnection>(),
                                               It.IsAny<CommandType>(),
                                               It.IsAny<string>(),
                                               It.IsAny<int?>()))
                   .Returns(new Mock<IDbCommand>().Object);

            using (var server = new MySqlServer(factory.Object))
            {
                try
                {
                    server.Connect("server=localhost;uid=u;pwd=p",
                                   CommandType.StoredProcedure, "select 1");
                }
                catch
                {
                    // Open() against a fake server is allowed to fail; we
                    // only care that the select-branch executed and that
                    // the factory was invoked with CommandType.Text.
                }
            }

            factory.Verify(f => f.CreateCommand(It.IsAny<IDbConnection>(),
                                                CommandType.Text,
                                                "select 1",
                                                It.IsAny<int?>()),
                           Times.Once);
        }

        // -----------------------------------------------------------------
        // VerifyConnection - "Please connect first" path
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(Exception))]
        public void MySqlServer_FetchDatabases_NotConnected_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.FetchDatabases();
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(Exception))]
        public void MySqlServer_CreateCommand_NotConnected_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.CreateCommand();
            }
        }

        // -----------------------------------------------------------------
        // FetchStoredProcedures - argument validation
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchStoredProcedures_ProcessorIsNull_Throws()
        {
            using (var server = new MySqlServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, h, n) => false;
                server.FetchStoredProcedures(null, func);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchStoredProcedures_FunctionProcessorIsNull_Throws()
        {
            using (var server = new MySqlServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, h, n) => false;
                server.FetchStoredProcedures(func, null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchStoredProcedures_3Arg_ProcessorIsNull_Throws()
        {
            using (var server = new MySqlServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, o, h, n) => false;
                server.FetchStoredProcedures((Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool>)null, func);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchStoredProcedures_3Arg_FunctionProcessorIsNull_Throws()
        {
            using (var server = new MySqlServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, o, h, n) => false;
                server.FetchStoredProcedures(func, null);
            }
        }

        // -----------------------------------------------------------------
        // FetchDataTable / FetchDataSet null arg validation
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchDataTable_NullCommand_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.FetchDataTable((IDbCommand)null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MySqlServer_FetchDataSet_NullCommand_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.FetchDataSet(null);
            }
        }

        // -----------------------------------------------------------------
        // ExecuteNonQuery / ExecuteScalar - wrong command type
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(Exception))]
        public void MySqlServer_ExecuteNonQuery_NotMySqlCommand_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.ExecuteNonQuery(new Mock<IDbCommand>().Object);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        [ExpectedException(typeof(Exception))]
        public void MySqlServer_ExecuteScalar_NotMySqlCommand_Throws()
        {
            using (var server = new MySqlServer())
            {
                server.ExecuteScalar(new Mock<IDbCommand>().Object);
            }
        }

        // -----------------------------------------------------------------
        // Transactions - no-op paths
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_BeginTransaction_NotConnected_Noop()
        {
            using (var server = new MySqlServer())
            {
                // Should not throw - IsConnected is false so the method
                // returns without creating a transaction.
                server.BeginTransaction();
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_RollbackTransaction_NoTransaction_Noop()
        {
            using (var server = new MySqlServer())
            {
                // Should not throw - _transaction is null so the method
                // returns without doing anything.
                server.RollbackTransaction();
            }
        }

        // -----------------------------------------------------------------
        // AddParameters static helper
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_AddParameters_Null_DoesNotThrow()
        {
            var mockCommand = new Mock<IDbCommand>();
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(c => c.Parameters).Returns(added);

            MySqlServer.AddParameters(mockCommand.Object, null);

            Assert.AreEqual(0, added.Count);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_AddParameters_Empty_DoesNotAddAny()
        {
            var mockCommand = new Mock<IDbCommand>();
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(c => c.Parameters).Returns(added);

            MySqlServer.AddParameters(mockCommand.Object, new List<IDbDataParameter>());

            Assert.AreEqual(0, added.Count);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_AddParameters_MultipleParameters_AllAdded()
        {
            var mockCommand = new Mock<IDbCommand>();
            var added = new SqlCommand().Parameters;
            mockCommand.Setup(c => c.Parameters).Returns(added);

            var parameters = new IDbDataParameter[]
            {
                new SqlParameter("a", "a"),
                new SqlParameter("b", "b"),
                new SqlParameter("c", "c"),
            };

            MySqlServer.AddParameters(mockCommand.Object, parameters);

            Assert.AreEqual(3, added.Count);
        }

        // -----------------------------------------------------------------
        // Dispose - idempotency
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(MySqlServer))]
        public void MySqlServer_Dispose_CalledTwice_DoesNotThrow()
        {
            var server = new MySqlServer();
            server.Dispose();
            server.Dispose();
        }
    }
}
