using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class DatabaseBrokerTests
    {

        #region GetMethods

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AbstractDataBaseBroker_GetMethods")]
        public void AbstractDataBaseBroker_GetServiceMethods_WhenNotCached_FreshResults()
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
        [TestCategory("AbstractDataBaseBroker_GetMethods")]
        public void AbstractDataBaseBroker_GetServiceMethods_WhenCached_CachedResults()
        {
            //------------Setup for test--------------------------
            TestDatabaseBroker broker = new TestDatabaseBroker();
            
            DbSource source  = new DbSource();

            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            var methodList = new ServiceMethodList();
            methodList.Add(new ServiceMethod("bob", "bob src", null, null, null));

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
        [TestCategory("AbstractDataBaseBroker_GetMethods")]
        public void AbstractDataBaseBroker_GetServiceMethods_WhenCachedNoRefreshRequested_FreshResults()
        {
            //------------Setup for test--------------------------
            TestDatabaseBroker broker = new TestDatabaseBroker();

            DbSource source = new DbSource();
            source.ReloadActions = true;

            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            var methodList = new ServiceMethodList();
            methodList.Add(new ServiceMethod("bob", "bob src", null, null, null));

            TestDatabaseBroker.TheCache.TryAdd(source.ConnectionString, methodList);
            //------------Execute Test---------------------------

            var result = broker.GetServiceMethods(source);

            // set back to empty ;)
            TestDatabaseBroker.TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
            //------------Assert Results-------------------------

            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region DbTest

        [TestMethod]
        public void TestServiceWithValidDbServiceThatReturnsNullExpectedRecordsetWithNullColumn()
        {
            var service = CreateNullValuesDbService();
            TestDatabaseBroker broker  = new TestDatabaseBroker();
            IOutputDescription outputDescription = broker.TestService(service);
            Assert.AreEqual(1,outputDescription.DataSourceShapes.Count);
            IDataSourceShape dataSourceShape = outputDescription.DataSourceShapes[0];
            Assert.IsNotNull(dataSourceShape);
            Assert.AreEqual(2,dataSourceShape.Paths.Count);
            StringAssert.Contains(dataSourceShape.Paths[0].DisplayPath,"FailedOn"); //This is the field that contains a null value. Previously this column would not have been returned.
        }
        #endregion

        #region CreateDbService
        public static DbService CreateNullValuesDbService()
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "BenchmarkService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                Method = new ServiceMethod
                {
                    Name = "Policy.proc_GetCollections",
                    Parameters = new List<MethodParameter>(new[]
                    {
                        new MethodParameter { Name = "@PolicyNo", EmptyToNull = false, IsRequired = true, Value = null, DefaultValue = "DDL12964" }
                    })
                },
                Recordset = new Recordset
                {
                    Name = "Collections",
                },
                Source = new DbSource
                {

                    ResourceID = Guid.NewGuid(),
                    ResourceName = "Benchmark",
                    ResourceType = ResourceType.DbSource,
                    ResourcePath = "Test",
                    Server = "RSAKLFSVRGENDEV",
                    DatabaseName = "Benchmark",
                    AuthenticationType = AuthenticationType.Windows,
                }
            };
            return service;
        }

        //serviceMethods.Add(serviceMethod);

        class TestDatabaseBroker : AbstractDatabaseBroker
        {
            #region Overrides of AbstractDatabaseBroker

            /// <summary>
            /// Override to return stored procedures.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="procedureProcessor">The procedure processor.</param>
            /// <param name="functionProcessor">The function processor.</param>
            /// <param name="continueOnProcessorException">if set to <c>true</c> [continue on processor exception].</param>
            protected override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
            {
            }

            /// <summary>
            /// Override to execute a select command.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <returns></returns>
            //protected abstract DataSet ExecuteSelect(IDbCommand command);
            protected override DataTable ExecuteSelect(IDbCommand command)
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("FailedOn");
                dataTable.Columns.Add("CollectedOn");
                DataRow dataRow = dataTable.NewRow();
                dataRow["FailedOn"] = null;
                dataRow["CollectedOn"] = DateTime.Now;
                dataTable.Rows.Add(dataRow);
                dataTable.AcceptChanges();
                return dataTable;
            }

            /// <summary>
            /// Override to create an implementation specific connection.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            /// <returns></returns>
            protected override IDbConnection CreateConnection(string connectionString)
            {
                Mock<IDbConnection> mockConnection = new Mock<IDbConnection>();
                var mockTransaction = new Mock<IDbTransaction>();
                var mockCommand = new Mock<IDbCommand>();
                var mockParameter = new Mock<IDbDataParameter>();
                var mockParameterCollection = new Mock<IDataParameterCollection>();
                mockCommand.Setup(command => command.CreateParameter()).Returns(mockParameter.Object);
                mockCommand.Setup(command => command.Parameters).Returns(mockParameterCollection.Object);
                mockConnection.Setup(connection => connection.BeginTransaction()).Returns(mockTransaction.Object);
                mockConnection.Setup(connection => connection.CreateCommand()).Returns(mockCommand.Object);
                return mockConnection.Object;
            }

            #endregion
        }

        #endregion
    }
}