
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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DataList.Contract;
using Dev2.Integration.Tests.Services.Sql;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Recordset.SqlBulkInsert
{
    [Binding]
    public class SqlBulkInsertSteps : BaseActivityUnitTest
    {
        public void SetupScenerio()
        {
            var sqlBulkInsert = new DsfSqlBulkInsertActivity();
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            ScenarioContext.Current.Add("dbSource", dbSource);
            sqlBulkInsert.Database = dbSource;
            sqlBulkInsert.TableName = "SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');
            var dataColumnMappings = new List<DataColumnMapping>
                {
                    new DataColumnMapping
                        {
                            InputColumn = "[[rs(*).Col1]]",
                            OutputColumn = new DbColumn
                                {
                                    ColumnName = "Col1",
                                    DataType = typeof (Int32),
                                    MaxLength = 100
                                },
                        },
                    new DataColumnMapping
                        {
                            InputColumn = "[[rs(*).Col2]]",
                            OutputColumn = new DbColumn
                                {
                                    ColumnName = "Col2",
                                    DataType = typeof (String),
                                    MaxLength = 100
                                }
                        },
                    new DataColumnMapping
                        {
                            InputColumn = "[[rs(*).Col3]]",
                            OutputColumn = new DbColumn
                                {
                                    ColumnName = "Col3",
                                    DataType = typeof (Guid),
                                    MaxLength = 100
                                }
                        }
                };
            sqlBulkInsert.InputMappings = dataColumnMappings;
            TestStartNode = new FlowStep
                {
                    Action = sqlBulkInsert
                };
            ErrorResultTO errors;

            var compiler = ScenarioContext.Current.Get<IDataListCompiler>("compiler");
            var dlId = ScenarioContext.Current.Get<Guid>("dlID");
            var dataShape = ScenarioContext.Current.Get<string>("dataShape");
            var data = compiler.ConvertFrom(dlId, DataListFormat.CreateFormat(GlobalConstants._XML),
                                               enTranslationDepth.Data, out errors);
            CurrentDl = dataShape;
            TestData = data.ToString();
            ScenarioContext.Current.Add("activity", sqlBulkInsert);
        }

        [Given(@"I have this data")]
        public void GivenIHaveThisData(Table table)
        {
            AddTableToDataList(table);
            SetupScenerio();
            ClearCountColumn();
        }

        private void ClearCountColumn()
        {
            var dbSource = ScenarioContext.Current.Get<DbSource>("dbSource");
            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "update SqlBulkInsertSpecFlowTestTableForeign_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_') + " " +
                                  "set Col2 = 0 " +
                                  "where Col1 = '23EF3ADB-5A4F-4785-B311-E121FF7ACB67'";
                using(var cmd = new SqlCommand(q2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddTableToDataList(Table table)
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ScenarioContext.Current.Add("compiler", compiler);
            // build up DataTable
            var dbData = new DataTable("rs");
            foreach(string columnName in table.Header)
            {
                dbData.Columns.Add(columnName);
            }
            foreach(TableRow row in table.Rows)
            {
                dbData.Rows.Add(row[0], row[1], row[2]);
            }
            // Execute Translator
            ErrorResultTO errors;
            var dataShape = "<root><rs><Col1/><Col2/><Col3/></rs></root>";
            var dlId = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, dataShape.ToStringBuilder(),
                                        out errors);
            dataShape = "<root><rs><Col1/><Col2/><Col3/></rs><result/></root>";
            ScenarioContext.Current.Add("dlID", dlId);
            ScenarioContext.Current.Add("dataShape", dataShape);
        }

        [Given(@"Check constraints is disabled")]
        public void GivenCheckConstraintsIsNotenabled()
        {
            ScenarioContext.Current.Add("checkConstraints", false);
        }

        [When(@"the tool is executed")]
        public void WhenTheToolIsExecuted()
        {
            bool checkConstraints;
            ScenarioContext.Current.TryGetValue("checkConstraints", out checkConstraints);
            bool keepIdentity;
            ScenarioContext.Current.TryGetValue("keepIdentity", out keepIdentity);
            bool ignoreBlankRows;
            ScenarioContext.Current.TryGetValue("ignoreBlankRows", out ignoreBlankRows);
            bool fireTriggers;
            ScenarioContext.Current.TryGetValue("fireTriggers", out fireTriggers);
            string batchSize;
            ScenarioContext.Current.TryGetValue("batchSize", out batchSize);
            string timeout;
            ScenarioContext.Current.TryGetValue("timeout", out timeout);

            var sqlBulkInsert = ScenarioContext.Current.Get<DsfSqlBulkInsertActivity>("activity");

            sqlBulkInsert.CheckConstraints = checkConstraints;
            sqlBulkInsert.IgnoreBlankRows = ignoreBlankRows;
            sqlBulkInsert.KeepIdentity = keepIdentity;
            sqlBulkInsert.FireTriggers = fireTriggers;
            sqlBulkInsert.BatchSize = batchSize;
            sqlBulkInsert.Timeout = timeout;
            sqlBulkInsert.Result = "[[result]]";
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the new table will have")]
        public void ThenTheNewTableWillHave(Table table)
        {
            var t1 = new DataTable();

            var dbSource = ScenarioContext.Current.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var query = "SELECT * FROM SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');

                using(var cmd = new SqlCommand(query, connection))
                {
                    using(var a = new SqlDataAdapter(cmd))
                    {
                        a.Fill(t1);
                    }
                }
            }


            List<DataRow> dataRows = t1.Rows.Cast<DataRow>().ToList();
            List<TableRow> tableRows = table.Rows.ToList();

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                string q1 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');

                using(var cmd = new SqlCommand(q1, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(dataRows.Count, tableRows.Count);

            for(int i = 0; i < dataRows.Count; i++)
            {
                Assert.AreEqual(dataRows[i][0].ToString(), tableRows[i][0]);
                Assert.AreEqual(dataRows[i][1].ToString(), tableRows[i][1]);
                Assert.AreEqual(dataRows[i][2].ToString(), tableRows[i][2]);
            }
        }

        [Given(@"Check constraints is enabled")]
        public void GivenCheckConstraintsIsenabled()
        {
            ScenarioContext.Current.Add("checkConstraints", true);
        }

        [Given(@"Keep identity is disabled")]
        public void GivenKeepIdentityIsNotenabled()
        {
            ScenarioContext.Current.Add("keepIdentity", false);
        }

        [Given(@"Keep identity is enabled")]
        public void GivenKeepIdentityIsenabled()
        {
            ScenarioContext.Current.Add("keepIdentity", true);
        }

        [Then(@"the new table will will have (.*) of rows")]
        public void ThenTheNewTableWillWillHaveOfRows(int numberOfRows)
        {
            object numberOfRowsInDb;
            var dbSource = ScenarioContext.Current.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q1 = "SELECT Count(*) FROM SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');
                using(var cmd = new SqlCommand(q1, connection))
                {
                    numberOfRowsInDb = cmd.ExecuteScalar();
                }
            }

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');
                using(var cmd = new SqlCommand(q2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(numberOfRows, numberOfRowsInDb);
        }

        [Given(@"Skip rows is enabled")]
        public void GivenSkipRowsIsenabled()
        {
            ScenarioContext.Current.Add("ignoreBlankRows", true);
        }

        [Given(@"Skip rows is disabled")]
        public void GivenSkipRowsIsNotenabled()
        {
            ScenarioContext.Current.Add("ignoreBlankRows", false);
        }

        [Given(@"Fire triggers is disabled")]
        public void GivenFireTriggersIsNotenabled()
        {
            ScenarioContext.Current.Add("fireTriggers", false);
        }

        [Given(@"Fire triggers is enabled")]
        public void GivenFireTriggersIsenabled()
        {
            ScenarioContext.Current.Add("fireTriggers", true);
        }

        [Given(@"Batch size is (.*)")]
        public void GivenBatchSizeIs(string batchSize)
        {
            ScenarioContext.Current.Add("batchSize", batchSize);
            ScenarioContext.Current.Add("fireTriggers", true);
        }

        [Given(@"Timeout in (.*) seconds")]
        public void GivenTimeoutInSeconds(string timeout)
        {
            ScenarioContext.Current.Add("timeout", timeout);
            ScenarioContext.Current.Add("fireTriggers", true);
        }


        [Then(@"number of inserts is (.*)")]
        public void ThenNumberOfInsertsIs(string numOfInserts)
        {
            object actualInserts;
            var dbSource = ScenarioContext.Current.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q1 = "select col2 from SqlBulkInsertSpecFlowTestTableForeign_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_') + " " +
                                  "where Col1 = '23EF3ADB-5A4F-4785-B311-E121FF7ACB67'";
                using(var cmd = new SqlCommand(q1, connection))
                {
                    actualInserts = cmd.ExecuteScalar();
                }
            }

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + ScenarioContext.Current.ScenarioInfo.Title.Replace(' ', '_');
                using(var cmd = new SqlCommand(q2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(numOfInserts, actualInserts);
        }
    }
}
