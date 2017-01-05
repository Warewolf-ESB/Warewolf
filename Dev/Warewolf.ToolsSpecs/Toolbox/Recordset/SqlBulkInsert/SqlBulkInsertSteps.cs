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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Data.Util;
using Dev2.Integration.Tests.Services.Sql;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using WarewolfParserInterop;

namespace Warewolf.ToolsSpecs.Toolbox.Recordset.SqlBulkInsert
{
    [Binding]
    public class SqlBulkInsertSteps : BaseActivityUnitTest
    {
        private readonly ScenarioContext scenarioContext;

        public SqlBulkInsertSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        public void SetupScenerio()
        {
            var sqlBulkInsert = new DsfSqlBulkInsertActivity();
            var dbSource = SqlServerTestUtils.CreateDev2TestingDbSource();
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource,"");
            scenarioContext.Add("dbSource", dbSource);
            sqlBulkInsert.Database = dbSource;
            sqlBulkInsert.TableName = "SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');
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
           
            scenarioContext.Add("activity", sqlBulkInsert);
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
            var dbSource = scenarioContext.Get<DbSource>("dbSource");
            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "update SqlBulkInsertSpecFlowTestTableForeign_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_') + " " +
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
            // build up DataTable
            foreach (TableRow row in table.Rows)
            {
                var i = 0;
                foreach (string columnName in table.Header)
                {
                    string value = row[i] == "blank" ? "" : row[i];
                    if (value.ToUpper() == "NULL")
                    {
                        var recordsetDisplayValue = DataListUtil.CreateRecordsetDisplayValue("rs", columnName, "");

                        DataObject.Environment.AssignDataShape(DataListUtil.AddBracketsToValueIfNotExist(recordsetDisplayValue));
                    }
                    else
                    {
                        var recordsetDisplayValue = DataListUtil.CreateRecordsetDisplayValue("rs", columnName, "");
                        var assignValue = new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(recordsetDisplayValue), row[i]);
                        DataObject.Environment.AssignWithFrame(assignValue, 0);
                    }
                   
                    i++;
                }
                DataObject.Environment.CommitAssign();
            }
            // Execute Translator            
        }

        [Given(@"Check constraints is disabled")]
        public void GivenCheckConstraintsIsNotenabled()
        {
            scenarioContext.Add("checkConstraints", false);
        }

        [When(@"the tool is executed")]
        public void WhenTheToolIsExecuted()
        {
            bool checkConstraints;
            scenarioContext.TryGetValue("checkConstraints", out checkConstraints);
            bool keepIdentity;
            scenarioContext.TryGetValue("keepIdentity", out keepIdentity);
            bool ignoreBlankRows;
            scenarioContext.TryGetValue("ignoreBlankRows", out ignoreBlankRows);
            bool fireTriggers;
            scenarioContext.TryGetValue("fireTriggers", out fireTriggers);
            string batchSize;
            scenarioContext.TryGetValue("batchSize", out batchSize);
            string timeout;
            scenarioContext.TryGetValue("timeout", out timeout);

            var sqlBulkInsert = scenarioContext.Get<DsfSqlBulkInsertActivity>("activity");

            sqlBulkInsert.CheckConstraints = checkConstraints;
            sqlBulkInsert.IgnoreBlankRows = ignoreBlankRows;
            sqlBulkInsert.KeepIdentity = keepIdentity;
            sqlBulkInsert.FireTriggers = fireTriggers;
            sqlBulkInsert.BatchSize = batchSize;
            sqlBulkInsert.Timeout = timeout;
            sqlBulkInsert.Result = "[[result]]";
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the new table will have")]
        public void ThenTheNewTableWillHave(Table table)
        {
            var t1 = new DataTable();

            var dbSource = scenarioContext.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var query = "SELECT * FROM SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');

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
                string q1 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');

                using(var cmd = new SqlCommand(q1, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(dataRows.Count, tableRows.Count);

            for(int i = 0; i < dataRows.Count; i++)
            {
                Assert.AreEqual<string>(dataRows[i][0].ToString(), tableRows[i][0]);
                Assert.AreEqual<string>(dataRows[i][1].ToString(), tableRows[i][1]);
                Assert.AreEqual<string>(dataRows[i][2].ToString(), tableRows[i][2]);
            }
        }

        [Given(@"Check constraints is enabled")]
        public void GivenCheckConstraintsIsenabled()
        {
            scenarioContext.Add("checkConstraints", true);
        }

        [Given(@"Keep identity is disabled")]
        public void GivenKeepIdentityIsNotenabled()
        {
            scenarioContext.Add("keepIdentity", false);
        }

        [Given(@"Keep identity is enabled")]
        public void GivenKeepIdentityIsenabled()
        {
            scenarioContext.Add("keepIdentity", true);
        }

        [Then(@"the new table will will have (.*) of rows")]
        public void ThenTheNewTableWillWillHaveOfRows(int numberOfRows)
        {
            object numberOfRowsInDb;
            var dbSource = scenarioContext.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q1 = "SELECT Count(*) FROM SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');
                using(var cmd = new SqlCommand(q1, connection))
                {
                    numberOfRowsInDb = cmd.ExecuteScalar();
                }
            }

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');
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
            scenarioContext.Add("ignoreBlankRows", true);
        }

        [Given(@"Skip rows is disabled")]
        public void GivenSkipRowsIsNotenabled()
        {
            scenarioContext.Add("ignoreBlankRows", false);
        }

        [Given(@"Fire triggers is disabled")]
        public void GivenFireTriggersIsNotenabled()
        {
            scenarioContext.Add("fireTriggers", false);
        }

        [Given(@"Fire triggers is enabled")]
        public void GivenFireTriggersIsenabled()
        {
            scenarioContext.Add("fireTriggers", true);
        }

        [Given(@"Batch size is (.*)")]
        public void GivenBatchSizeIs(string batchSize)
        {
            scenarioContext.Add("batchSize", batchSize);
            scenarioContext.Add("fireTriggers", true);
        }

        [Given(@"Timeout in (.*) seconds")]
        public void GivenTimeoutInSeconds(string timeout)
        {
            scenarioContext.Add("timeout", timeout);
            scenarioContext.Add("fireTriggers", true);
        }


        [Then(@"number of inserts is (.*)")]
        public void ThenNumberOfInsertsIs(string numOfInserts)
        {
            object actualInserts;
            var dbSource = scenarioContext.Get<DbSource>("dbSource");

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q1 = "select col2 from SqlBulkInsertSpecFlowTestTableForeign_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_') + " " +
                                  "where Col1 = '23EF3ADB-5A4F-4785-B311-E121FF7ACB67'";
                using(var cmd = new SqlCommand(q1, connection))
                {
                    actualInserts = cmd.ExecuteScalar();
                }
            }

            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var q2 = "truncate table SqlBulkInsertSpecFlowTestTable_for_" + scenarioContext.ScenarioInfo.Title.Replace(' ', '_');
                using(var cmd = new SqlCommand(q2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Assert.AreEqual(numOfInserts, actualInserts);
        }
    }
}
