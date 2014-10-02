
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ServiceModel.Esb.Brokers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AbstractDataBaseBrokerTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_GetServiceMethods")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetServiceMethods_DbSourceIsNull_ThrowsArgumentNullException()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var broker = new TestDatabaseBroker();

            //------------Execute Test---------------------------

#pragma warning disable 168
            var result = broker.GetServiceMethods(null);
#pragma warning restore 168

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AbstractDataBaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetServiceMethods_WhenNotCached_FreshResults()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            TestDatabaseBroker broker = new TestDatabaseBroker();
            Mock<DbSource> source = new Mock<DbSource>();
            //------------Execute Test---------------------------

            var result = broker.GetServiceMethods(source.Object);

            //------------Assert Results-------------------------

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AbstractDataBaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetServiceMethods_WhenCached_CachedResults()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            TestDatabaseBroker broker = new TestDatabaseBroker();

            DbSource source = new DbSource();

            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
// ReSharper disable UseObjectOrCollectionInitializer
            var methodList = new ServiceMethodList();
// ReSharper restore UseObjectOrCollectionInitializer
            methodList.Add(new ServiceMethod("bob", "bob src", null, null, null,""));

            TestDatabaseBroker.TheCache.TryAdd(source.ConnectionString, methodList);
            //------------Execute Test---------------------------

            var result = broker.GetServiceMethods(source);

            // set back to empty ;)
            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            //------------Assert Results-------------------------

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bob", result[0].Name);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AbstractDataBaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetServiceMethods_WhenCachedNoRefreshRequested_FreshResults()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            TestDatabaseBroker broker = new TestDatabaseBroker();

            DbSource source = new DbSource { ReloadActions = true };

            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            var methodList = new ServiceMethodList { new ServiceMethod("bob", "bob src", null, null, null, null) };

            TestDatabaseBroker.TheCache.TryAdd(source.ConnectionString, methodList);
            //------------Execute Test---------------------------

            var result = broker.GetServiceMethods(source);

            // set back to empty ;)
            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            //------------Assert Results-------------------------

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_GetServiceMethods")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetServiceMethods_InvokesDbServerFetchStoredProcedures_Done()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource();

            var procedureCommand = new SqlCommand("procedureCommand");
            procedureCommand.Parameters.Add(new SqlParameter("@p1", "proc"));

            var functionCommand = new SqlCommand("functionCommand");
            functionCommand.Parameters.Add(new SqlParameter("@f1", "func"));

            var dbServer = new Mock<TestDbServer>();
            dbServer.Setup(s => s.Connect(It.IsAny<string>())).Verifiable();
            dbServer.Setup(s => s.FetchStoredProcedures
                (
                    It.IsAny<Func<IDbCommand, List<IDbDataParameter>, string,string, bool>>(),
                    It.IsAny<Func<IDbCommand, List<IDbDataParameter>, string, string, bool>>(),
                    It.IsAny<bool>())
                )
                .Callback(
                    (
                        Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
                        Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
                        bool continueOnProcessorException
                        ) =>
                    {
                        procedureProcessor(procedureCommand, procedureCommand.Parameters.Cast<IDbDataParameter>().ToList(), "procHelpText","");
                        functionProcessor(functionCommand, functionCommand.Parameters.Cast<IDbDataParameter>().ToList(), "funcHelpText","");
                    })
                .Verifiable();

            var broker = new TestDatabaseBroker(dbServer.Object);

            //------------Execute Test---------------------------
            var result = broker.GetServiceMethods(dbSource);

            //------------Assert Results-------------------------
            dbServer.Verify(s => s.Connect(It.IsAny<string>()));
            dbServer.Verify(s => s.FetchStoredProcedures
                (
                    It.IsAny<Func<IDbCommand, List<IDbDataParameter>, string, string, bool>>(),
                    It.IsAny<Func<IDbCommand, List<IDbDataParameter>, string, string, bool>>(),
                    It.IsAny<bool>())
                );


            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            for(var i = 0; i < result.Count; i++)
            {
                var command = i == 0 ? procedureCommand : functionCommand;
                var serviceMethod = result[i];

                Assert.AreEqual(command.CommandText, serviceMethod.Name);

                for(var j = 0; j < serviceMethod.Parameters.Count; j++)
                {
                    var methodParam = serviceMethod.Parameters[j];
                    var commandParam = (IDbDataParameter)command.Parameters[j];

                    Assert.AreEqual(commandParam.ParameterName, "@" + methodParam.Name);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_TestService")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_TestService_DbServiceIsNull_ThrowsArgumentNullException()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var broker = new TestDatabaseBroker();

            //------------Execute Test---------------------------

            broker.TestService(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_TestService")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_TestService_DbServiceWithNullSource_ThrowsArgumentNullException()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var broker = new TestDatabaseBroker();

            var dbService = new DbService { Source = null };

            //------------Execute Test---------------------------

            broker.TestService(dbService);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_TestService")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_TestService_InvokesDbServiceMethodInsideTransaction_Done()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var dbService = new DbService
            {
                Method = new ServiceMethod
                {
                    Name = "TestMethod",
                    Parameters = new List<MethodParameter>
                    {
                        new MethodParameter { Name = "Param1", Type = typeof(string), Value = "Hello" },
                        new MethodParameter { Name = "Param2", Type = typeof(int), Value = "99" }
                    },
                    ExecuteAction = "BobTestMethod"
                    
                }
            };

            var testServiceResults = new DataTable("TestTableName");
            testServiceResults.Columns.Add("Col1", typeof(int));
            testServiceResults.Columns.Add("Col2", typeof(string));
            testServiceResults.Rows.Add(new object[] { 1, "row1" });
            testServiceResults.Rows.Add(new object[] { 2, "row2" });
            testServiceResults.Rows.Add(new object[] { 3, null });

            IDbCommand fetchDataTableCommand = null;

            var dbServer = new Mock<TestDbServer>();
            dbServer.Setup(s => s.Connect(It.IsAny<string>())).Verifiable();
            dbServer.Setup(s => s.BeginTransaction()).Verifiable();
            dbServer.Setup(s => s.FetchDataTable(It.IsAny<IDbCommand>()))
                .Callback((IDbCommand command) => fetchDataTableCommand = command)
                .Returns(testServiceResults)
                .Verifiable();
            dbServer.Setup(s => s.RollbackTransaction()).Verifiable();
            dbServer.Setup(s => s.CreateCommand()).Returns(new SqlCommand());

            var broker = new TestDatabaseBroker(dbServer.Object);

            //------------Execute Test---------------------------
            var result = broker.TestService(dbService);

            //------------Assert Results-------------------------
            dbServer.Verify(s => s.Connect(It.IsAny<string>()));
            dbServer.Verify(s => s.BeginTransaction());
            dbServer.Verify(s => s.FetchDataTable(It.IsAny<IDbCommand>()));
            dbServer.Verify(s => s.RollbackTransaction());

            Assert.IsNotNull(fetchDataTableCommand);
            Assert.AreEqual("BobTestMethod", fetchDataTableCommand.CommandText);
            Assert.AreEqual(dbService.Method.Parameters.Count, fetchDataTableCommand.Parameters.Count);

            for(var i = 0; i < dbService.Method.Parameters.Count; i++)
            {
                var methodParam = dbService.Method.Parameters[i];
                var commandParam = (IDbDataParameter)fetchDataTableCommand.Parameters[i];

                Assert.AreEqual("@" + methodParam.Name, commandParam.ParameterName);
                Assert.AreEqual(methodParam.Value, commandParam.Value);
                Assert.AreEqual(DbType.String, commandParam.DbType);
            }

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DataSourceShapes);
            Assert.AreEqual(1, result.DataSourceShapes.Count);
            Assert.IsNotNull(result.DataSourceShapes[0]);
            Assert.AreEqual(2, result.DataSourceShapes[0].Paths.Count);

            Assert.AreEqual("TestTableName().Col1", result.DataSourceShapes[0].Paths[0].ActualPath);
            Assert.AreEqual("TestTableName().Col1", result.DataSourceShapes[0].Paths[0].DisplayPath);
            Assert.AreEqual("1__COMMA__2__COMMA__3", result.DataSourceShapes[0].Paths[0].SampleData);

            Assert.AreEqual("TestTableName().Col2", result.DataSourceShapes[0].Paths[1].ActualPath);
            Assert.AreEqual("TestTableName().Col2", result.DataSourceShapes[0].Paths[1].DisplayPath);
            Assert.AreEqual("row1__COMMA__row2__COMMA__", result.DataSourceShapes[0].Paths[1].SampleData);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_GetDatabases")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetDatabases_DbSourceIsNull_ThrowsArgumentNullException()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var broker = new TestDatabaseBroker();

            //------------Execute Test---------------------------
            broker.GetDatabases(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AbstractDataBaseBroker_GetDatabases")]
// ReSharper disable InconsistentNaming
        public void AbstractDataBaseBroker_GetDatabases_InvokesDbServerFetchDatabases_Done()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var dbSource = new DbSource();

            var dbServer = new Mock<TestDbServer>();
            dbServer.Setup(s => s.Connect(It.IsAny<string>())).Verifiable();
            dbServer.Setup(s => s.FetchDatabases()).Verifiable();

            var broker = new TestDatabaseBroker(dbServer.Object);

            //------------Execute Test---------------------------
            broker.GetDatabases(dbSource);

            //------------Assert Results-------------------------
            dbServer.Verify(s => s.Connect(It.IsAny<string>()));
            dbServer.Verify(s => s.FetchDatabases());
        }
    }
}
