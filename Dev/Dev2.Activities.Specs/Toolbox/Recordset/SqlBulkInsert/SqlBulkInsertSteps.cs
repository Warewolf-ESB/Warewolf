using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Integration.Tests.Services.Sql;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Recordset.SqlBulkInsert
{
    [Binding]
    public class SqlBulkInsertSteps : BaseActivityUnitTest
    {
        Guid _dlID;
        DsfSqlBulkInsertActivity _sqlBulkInsert;
        IDataListCompiler _compiler;
        DbSource _dbSource;
        private string _dataShape;

        public void SetupScenerio()
        {
            _sqlBulkInsert = new DsfSqlBulkInsertActivity();
            _dbSource = SqlServerTests.CreateDev2TestingDbSource();
            _sqlBulkInsert.Database = _dbSource;
            _sqlBulkInsert.TableName = "SqlBulkInsertSpecFlowTestTable";
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[rs(*).Col1]]",
                    OutputColumn = new DbColumn {ColumnName = "Col1",
                    DataType = typeof(Int32),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[rs(*).Col2]]",
                    OutputColumn = new DbColumn { ColumnName = "Col2",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[rs(*).Col3]]",
                    OutputColumn = new DbColumn { ColumnName = "Col3",
                    DataType = typeof(Guid),
                    MaxLength = 100 }
                }
            };
            _sqlBulkInsert.InputMappings = dataColumnMappings;
            TestStartNode = new FlowStep
            {
                Action = _sqlBulkInsert
            };
            ErrorResultTO errors;
            var data = _compiler.ConvertFrom(_dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            CurrentDl = _dataShape;
            TestData = data;
        }

        [Given(@"I have this data")]
        public void GivenIHaveThisData(Table table)
        {
            AddTableToDataList(table);
            SetupScenerio();
        }

        void AddTableToDataList(Table table)
        {
            _compiler = DataListFactory.CreateDataListCompiler();
            // build up DataTable
            var dbData = new DataTable("rs");
            foreach(var columnName in table.Header)
            {
                dbData.Columns.Add(columnName);
            }
            foreach(TableRow row in table.Rows)
            {
                dbData.Rows.Add(row[0],row[1],row[2]);    
            }
            // Execute Translator
            ErrorResultTO errors;
            _dataShape = "<root><rs><Col1/><Col2/><Col3/></rs></root>";
            _dlID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, _dataShape, out errors);

        }
        
        [Given(@"Check constraints is disabled")]
        public void GivenCheckConstraintsIsNotenabled()
        {
            _sqlBulkInsert.CheckConstraints = false;
        }
        
        [When(@"the tool is executed")]
        public void WhenTheToolIsExecuted()
        {
            ExecuteProcess();
        }

        [Then(@"the new table will have")]
        public void ThenTheNewTableWillHave(Table table)
        {
            var t1 = new DataTable();
            using(var connection = new SqlConnection(_dbSource.ConnectionString))
            {
                connection.Open();
                const string Query = "SELECT * FROM SqlBulkInsertSpecFlowTestTable";

                using(var cmd = new SqlCommand(Query, connection))
                {
                    using(var a = new SqlDataAdapter(cmd))
                    {
                        a.Fill(t1);
                    }
                }
            }


            List<DataRow> dataRows = t1.Rows.Cast<DataRow>().ToList();
            List<TableRow> tableRows = table.Rows.ToList();

            using (var connection = new SqlConnection(_dbSource.ConnectionString))
            {
                connection.Open();
                string q1 = "truncate table SqlBulkInsertSpecFlowTestTable";

                using (var cmd = new SqlCommand(q1, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(dataRows.Count, tableRows.Count);
           
            for (int i = 0; i < dataRows.Count; i++)
            {
                Assert.AreEqual(dataRows[i][0].ToString(), tableRows[i][0]);
                Assert.AreEqual(dataRows[i][1].ToString(), tableRows[i][1]);
                Assert.AreEqual(dataRows[i][2].ToString(), tableRows[i][2]);
            }
        }

        [Given(@"Check constraints is enabled")]
        public void GivenCheckConstraintsIsenabled()
        {
            _sqlBulkInsert.CheckConstraints = true;
        }

        [Given(@"Keep identity is disabled")]
        public void GivenKeepIdentityIsNotenabled()
        {
            _sqlBulkInsert.KeepIdentity = false;
        }

        [Given(@"Keep identity is enabled")]
        public void GivenKeepIdentityIsenabled()
        {
            _sqlBulkInsert.KeepIdentity = true;
        }

        [Then(@"the new table will will have (.*) of rows")]
        public void ThenTheNewTableWillWillHaveOfRows(int numberOfRows)
        {
            object numberOfRowsInDb;
            using (var connection = new SqlConnection(_dbSource.ConnectionString))
            {
                connection.Open();
                const string q1 = "SELECT Count(*) FROM SqlBulkInsertSpecFlowTestTable";
                using (var cmd = new SqlCommand(q1, connection))
                {
                   numberOfRowsInDb = cmd.ExecuteScalar();
                }
            }

            using (var connection = new SqlConnection(_dbSource.ConnectionString))
            {
                connection.Open();
                const string q2 = "truncate table SqlBulkInsertSpecFlowTestTable";
                using (var cmd = new SqlCommand(q2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(numberOfRows, numberOfRowsInDb);
        }

        [Given(@"Skip rows is enabled")]
        public void GivenSkipRowsIsenabled()
        {
            _sqlBulkInsert.IgnoreBlankRows = true;
        }

        [Given(@"Skip rows is disabled")]
        public void GivenSkipRowsIsNotenabled()
        {
            _sqlBulkInsert.IgnoreBlankRows = false;
        }

        [Given(@"Fire triggers is disabled")]
        public void GivenFireTriggersIsNotenabled()
        {
            _sqlBulkInsert.FireTriggers = false;
        }

        [Given(@"Fire triggers is enabled")]
        public void GivenFireTriggersIsenabled()
        {
            _sqlBulkInsert.FireTriggers = true;
        }
    }
}
