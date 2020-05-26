/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data;
using TSQL.Statements;
using TSQL.Tokens;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;
using System.Linq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class AdvancedRecordsetActivityWorkerTests
    {
        static AdvancedRecordsetActivity CreateAdvancedRecordsetActivity()
        {
            return new AdvancedRecordsetActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_LoadRecordset()
        {
            const string tableName = "tableName";
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            var mockAdvancedRecordsetFactory = new Mock<IAdvancedRecordsetFactory>();
            mockAdvancedRecordsetFactory.Setup(advancedRecordsetFactory => advancedRecordsetFactory.New(It.IsAny<IExecutionEnvironment>())).Returns(mockAdvancedRecordset.Object);

            using (var viewModel = new AdvancedRecordsetActivityWorker(null, mockAdvancedRecordset.Object, mockAdvancedRecordsetFactory.Object))
            {
                viewModel.LoadRecordset(tableName);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_AddDeclarations()
        {
            const string varName = "varName";
            const string varValue = "varValue";
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.CreateVariableTable());
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.InsertIntoVariableTable(varName, varValue));
            var mockAdvancedRecordsetFactory = new Mock<IAdvancedRecordsetFactory>();
            mockAdvancedRecordsetFactory.Setup(advancedRecordsetFactory => advancedRecordsetFactory.New(It.IsAny<IExecutionEnvironment>())).Returns(mockAdvancedRecordset.Object);

            using (var viewModel = new AdvancedRecordsetActivityWorker(null, mockAdvancedRecordset.Object, mockAdvancedRecordsetFactory.Object))
            {
                viewModel.AddDeclarations(varName, varValue);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.CreateVariableTable(), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.InsertIntoVariableTable(varName, varValue), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_AddDeclarations_ExpectedException_NoThrow()
        {
            const string varName = "varName";
            const string varValue = "varValue";
            var mockAdvancedRecordsetFactory = new Mock<IAdvancedRecordsetFactory>();
            //mockAdvancedRecordsetFactory.Setup(advancedRecordsetFactory => advancedRecordsetFactory.New(It.IsAny<IExecutionEnvironment>())).Returns(mockAdvancedRecordset.Object)

            //TODO: Merge constructors

            using (var viewModel = new AdvancedRecordsetActivityWorker(null, null, mockAdvancedRecordsetFactory.Object))
            {
                viewModel.AddDeclarations(varName, varValue);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_TSQLStatementType_Select()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = "Select * from person";

            const string tableName = "tableName";
            const string sqlQuery = "sqlQuery";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLSelectStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery)).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLSelectStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_UNION()
        {
            const string sqlQuery = "SELECT * from person UNION ALL SELECT * from person;";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;

            const string tableName = "tableName";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>();

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery)).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_CREATE()
        {
            const string sqlQuery = "CREATE TABLE IF NOT EXISTS Variables (Name TEXT PRIMARY KEY, Value BLOB)";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;
            advancedRecordsetActivity.Outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "records_affected", RecordSetName = "person" }
            };

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>
            {
                ("person", "person")
            };

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ReturnSql(It.IsAny<List<TSQLToken>>()));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ReturnSql(It.IsAny<List<TSQLToken>>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_UPDATE()
        {
            const string sqlQuery = "update person set ID = 1 select * from person;";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;
            advancedRecordsetActivity.Outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "records_affected", RecordSetName = "person" }
            };

            const string tableName = "tableName";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>
            {
                ("person", "person")
            };

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person")).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.LoadRecordsetAsTable("person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_DELETE()
        {
            const string sqlQuery = "delete from person where ID = 1 select * from person;";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;
            advancedRecordsetActivity.Outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "records_affected", RecordSetName = "person" }
            };

            const string tableName = "tableName";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>
            {
                ("person", "person")
            };

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person")).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.LoadRecordsetAsTable("person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_INSERT()
        {
            const string sqlQuery = "insert into person where ID = 1 select * from person;";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;
            advancedRecordsetActivity.Outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "records_affected", RecordSetName = "person" }
            };

            const string tableName = "tableName";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>
            {
                ("person", "person")
            };

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person")).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.LoadRecordsetAsTable("person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteSql_ProcessComplexStatement_REPLACE()
        {
            const string sqlQuery = "REPLACE INTO person(name, age) VALUES('Robocop', 1000);";

            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = sqlQuery;
            advancedRecordsetActivity.Outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "records_affected", RecordSetName = "person" }
            };

            const string tableName = "tableName";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var hashedRecSetsList = new List<(string hashCode, string recSet)>
            {
                ("person", "person")
            };

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.HashedRecSets).Returns(hashedRecSetsList);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person")).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.LoadRecordsetAsTable("person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLUnknownStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery("SELECT * FROM person"), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyScalarResultToEnvironment(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_ExecuteRecordset_With_No_DeclareVariables()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.SqlQuery = "Select * from person";

            const string tableName = "tableName";
            const string sqlQuery = "sqlQuery";

            var dataTable = new DataTable("myTable")
            {
                TableName = "table"
            };
            dataTable.Rows.Add();

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            var started = false;

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.LoadRecordsetAsTable(tableName));
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLSelectStatement>())).Returns(sqlQuery);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery)).Returns(dataSet);
            mockAdvancedRecordset.Setup(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started));

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                viewModel.ExecuteSql(0, ref started);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.UpdateSqlWithHashCodes(It.IsAny<TSQLSelectStatement>()), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ExecuteQuery(sqlQuery), Times.Once);
                mockAdvancedRecordset.Verify(advancedRecordset => advancedRecordset.ApplyResultToEnvironment(It.IsAny<string>(), It.IsAny<ICollection<IServiceOutputMapping>>(), It.IsAny<List<DataRow>>(), It.IsAny<bool>(), It.IsAny<int>(), ref started), Times.Once);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_GetIdentifiers_TSQLStatementType_SELECT_GivenNoAnyField_ReturnIdentifierWithZeroValue()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = "Select * from person";

            const string expectedTableName = "person";

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                var identifiers = viewModel.GetIdentifiers();

                Assert.AreEqual(1, identifiers.Count);
                Assert.AreEqual(expectedTableName, identifiers.FirstOrDefault().Key);
                Assert.AreEqual(0, identifiers.FirstOrDefault().Value.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(AdvancedRecordsetActivityWorker))]
        public void AdvancedRecordsetActivityWorker_GetIdentifiers_TSQLStatementType_SELECT_GivenTwoField_ReturnIdentifierWithTwoValues()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            advancedRecordsetActivity.DeclareVariables.Add(new NameValue { Name = "", Value = "" });
            advancedRecordsetActivity.SqlQuery = "Select personid, personname from person";

            const string expectedTableName = "person";

            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();

            using (var viewModel = new AdvancedRecordsetActivityWorker(advancedRecordsetActivity, mockAdvancedRecordset.Object))
            {
                var identifiers = viewModel.GetIdentifiers();
                Assert.AreEqual(1, identifiers.Count);
                Assert.AreEqual(expectedTableName, identifiers.FirstOrDefault().Key);
                Assert.AreEqual(2, identifiers.FirstOrDefault().Value.Count);
            }
        }
    }
}

