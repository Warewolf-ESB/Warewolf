/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Sql.Tests
{
    /// <summary>
    /// Coverage for <see cref="ODBCServer"/> (previously 0% covered,
    /// ~511 uncovered lines in the merged pipeline report). These tests
    /// drive the argument-validation, lifecycle and helper paths that do
    /// not require a live ODBC data source.
    /// </summary>
    [TestClass]
    public class ODBCServerTests
    {
        // -----------------------------------------------------------------
        // Construction / property defaults
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_DefaultCtor_DoesNotThrow()
        {
            using (var server = new ODBCServer())
            {
                Assert.IsFalse(server.IsConnected);
                Assert.IsNull(server.ConnectionString);
                Assert.IsNull(server.CommandTimeout);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_FactoryCtor_DoesNotThrow()
        {
            var factory = new Mock<IDbFactory>();
            using (var server = new ODBCServer(factory.Object))
            {
                Assert.IsFalse(server.IsConnected);
                Assert.IsNull(server.ConnectionString);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_TestingCtor_ReportsIsConnectedTrue()
        {
            var factory = new Mock<IDbFactory>();
            var command = new Mock<IDbCommand>();
            var transaction = new Mock<IDbTransaction>();

            using (var server = new ODBCServer(factory.Object, command.Object, transaction.Object))
            {
                Assert.IsTrue(server.IsConnected, "_testing branch should report connected");
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_CommandTimeout_RoundTrips()
        {
            using (var server = new ODBCServer())
            {
                server.CommandTimeout = 42;
                Assert.AreEqual(42, server.CommandTimeout);
            }
        }

        // -----------------------------------------------------------------
        // SetCommandType branches
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_SetCommandType_SelectForcedToText()
        {
            using (var server = new ODBCServer())
            {
                Assert.AreEqual(CommandType.Text,
                    server.SetCommandType("select * from t", CommandType.StoredProcedure));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_SetCommandType_UpdateForcedToText()
        {
            using (var server = new ODBCServer())
            {
                Assert.AreEqual(CommandType.Text,
                    server.SetCommandType("update t set x=1", CommandType.StoredProcedure));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_SetCommandType_DeleteForcedToText()
        {
            using (var server = new ODBCServer())
            {
                Assert.AreEqual(CommandType.Text,
                    server.SetCommandType("delete from t", CommandType.StoredProcedure));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_SetCommandType_StoredProcedurePreserved()
        {
            using (var server = new ODBCServer())
            {
                Assert.AreEqual(CommandType.StoredProcedure,
                    server.SetCommandType("sp_help", CommandType.StoredProcedure));
            }
        }

        // -----------------------------------------------------------------
        // Connect string-only overload short-circuits when _testing
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_Connect_TestingMode_DoesNotTouchFactory()
        {
            var factory = new Mock<IDbFactory>();
            using (var server = new ODBCServer(factory.Object,
                                               new Mock<IDbCommand>().Object,
                                               new Mock<IDbTransaction>().Object))
            {
                // Should not throw - _testing is true so Connect(string) is a no-op.
                server.Connect("anything");
            }

            factory.Verify(f => f.CreateConnection(It.IsAny<string>()), Times.Never);
        }

        // -----------------------------------------------------------------
        // VerifyConnection paths - throws "Please connect first"
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(Exception))]
        public void ODBCServer_CreateCommand_NotConnected_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.CreateCommand();
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_CreateCommand_TestingMode_ReturnsNull()
        {
            using (var server = new ODBCServer(new Mock<IDbFactory>().Object,
                                               new Mock<IDbCommand>().Object,
                                               new Mock<IDbTransaction>().Object))
            {
                Assert.IsNull(server.CreateCommand());
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(Exception))]
        public void ODBCServer_FetchXmlData_NotConnected_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.FetchXmlData();
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(Exception))]
        public void ODBCServer_FetchDataTable_Parameterless_NotConnected_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.FetchDataTable();
            }
        }

        // -----------------------------------------------------------------
        // Argument validation
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ODBCServer_FetchDataTable_NullCommand_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.FetchDataTable((IDbCommand)null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ODBCServer_FetchDataSet_NullCommand_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.FetchDataSet(null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ODBCServer_ExecuteNonQuery_NullCommand_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.ExecuteNonQuery(null);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ODBCServer_ExecuteScalar_NullCommand_Throws()
        {
            using (var server = new ODBCServer())
            {
                server.ExecuteScalar(null);
            }
        }

        // -----------------------------------------------------------------
        // ExecuteNonQuery / ExecuteScalar - delegate to factory
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_ExecuteNonQuery_DelegatesToFactory()
        {
            var factory = new Mock<IDbFactory>();
            var cmd = new Mock<IDbCommand>().Object;
            factory.Setup(f => f.ExecuteNonQuery(cmd)).Returns(17);

            using (var server = new ODBCServer(factory.Object))
            {
                Assert.AreEqual(17, server.ExecuteNonQuery(cmd));
            }

            factory.Verify(f => f.ExecuteNonQuery(cmd), Times.Once);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_ExecuteScalar_DelegatesToFactory()
        {
            var factory = new Mock<IDbFactory>();
            var cmd = new Mock<IDbCommand>().Object;
            factory.Setup(f => f.ExecuteScalar(cmd)).Returns(31);

            using (var server = new ODBCServer(factory.Object))
            {
                Assert.AreEqual(31, server.ExecuteScalar(cmd));
            }

            factory.Verify(f => f.ExecuteScalar(cmd), Times.Once);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_FetchDataSet_DelegatesToFactory()
        {
            var factory = new Mock<IDbFactory>();
            var cmd = new Mock<IDbCommand>().Object;
            var ds = new DataSet();
            factory.Setup(f => f.FetchDataSet(cmd)).Returns(ds);

            using (var server = new ODBCServer(factory.Object))
            {
                Assert.AreSame(ds, server.FetchDataSet(cmd));
            }
        }

        // -----------------------------------------------------------------
        // FetchStoredProcedures - all four overloads throw NotImplementedException
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(NotImplementedException))]
        public void ODBCServer_FetchStoredProcedures_Outparam_TwoArg_Throws()
        {
            using (var server = new ODBCServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, o, h, n) => false;
                server.FetchStoredProcedures(func, func);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(NotImplementedException))]
        public void ODBCServer_FetchStoredProcedures_Outparam_FourArg_Throws()
        {
            using (var server = new ODBCServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, o, h, n) => false;
                server.FetchStoredProcedures(func, func, false, "db");
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(NotImplementedException))]
        public void ODBCServer_FetchStoredProcedures_NoOut_TwoArg_Throws()
        {
            using (var server = new ODBCServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, h, n) => false;
                server.FetchStoredProcedures(func, func);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        [ExpectedException(typeof(NotImplementedException))]
        public void ODBCServer_FetchStoredProcedures_NoOut_FourArg_Throws()
        {
            using (var server = new ODBCServer())
            {
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> func =
                    (c, p, h, n) => false;
                server.FetchStoredProcedures(func, func, true, "db");
            }
        }

        // -----------------------------------------------------------------
        // Transactions
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_BeginTransaction_NotConnected_Noop()
        {
            using (var server = new ODBCServer())
            {
                server.BeginTransaction(); // IsConnected is false, returns immediately
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_RollbackTransaction_NoTransaction_Noop()
        {
            using (var server = new ODBCServer())
            {
                server.RollbackTransaction(); // _transaction is null, no-op
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_RollbackTransaction_WithTransaction_RollsBackAndClears()
        {
            var factory = new Mock<IDbFactory>();
            var tx = new Mock<IDbTransaction>();

            using (var server = new ODBCServer(factory.Object,
                                               new Mock<IDbCommand>().Object,
                                               tx.Object))
            {
                server.RollbackTransaction();
                // Calling again should be a safe no-op now that _transaction is null.
                server.RollbackTransaction();
            }

            tx.Verify(t => t.Rollback(), Times.Once);
            tx.Verify(t => t.Dispose(), Times.Once);
        }

        // -----------------------------------------------------------------
        // FetchDatabases (GetDSN) - registry-backed, returns a list
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_FetchDatabases_ReturnsList()
        {
            using (var server = new ODBCServer())
            {
                var dbs = server.FetchDatabases();
                Assert.IsNotNull(dbs);
                // The list may be empty on a vanilla CI agent - we just
                // need GetDSN to walk the registry branches without throwing.
            }
        }

        // -----------------------------------------------------------------
        // Dispose - idempotency
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_Dispose_CalledTwice_DoesNotThrow()
        {
            var server = new ODBCServer();
            server.Dispose();
            server.Dispose();
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(ODBCServer))]
        public void ODBCServer_Dispose_TestingMode_DisposesCommandAndTransaction()
        {
            var factory = new Mock<IDbFactory>();
            var cmd = new Mock<IDbCommand>();
            var tx = new Mock<IDbTransaction>();

            var server = new ODBCServer(factory.Object, cmd.Object, tx.Object);
            server.Dispose();

            cmd.Verify(c => c.Dispose(), Times.Once);
            tx.Verify(t => t.Dispose(), Times.Once);
        }
    }
}
