using System;
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