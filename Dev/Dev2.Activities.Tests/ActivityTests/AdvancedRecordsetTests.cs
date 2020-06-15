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
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TSQL;
using TSQL.Statements;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class AdvancedRecordsetTests : BaseActivityTests
    {
        public AdvancedRecordset CreatePersonAddressWorkers()
        {
            var personRecordsetName = "person";
            var addressRecordsetName = "address";

            /*
			Person
			| Name | Age | address_id |
			Address
			| id | Addr | Postcode |

			| bob | 21 | 1 |
			| sue | 22 | 2 | # unique address
			| jef | 24 | 1 | # matching address
			| zak | 19 | 9 | # fail finding address

			| 1 | 11 test lane | 3421 |
			| 2 | 16 test lane | 3422 |
			 * */

            var l = new List<AssignValue>();
            l.Add(new AssignValue("[[person().name]]", "bob"));
            l.Add(new AssignValue("[[person().age]]", "21"));
            l.Add(new AssignValue("[[person().address_id]]", "1"));

            l.Add(new AssignValue("[[person().name]]", "sue"));
            l.Add(new AssignValue("[[person().age]]", "22"));
            l.Add(new AssignValue("[[person().address_id]]", "2"));

            l.Add(new AssignValue("[[person().name]]", "jef"));
            l.Add(new AssignValue("[[person().age]]", "24"));
            l.Add(new AssignValue("[[person().address_id]]", "1"));

            l.Add(new AssignValue("[[person().name]]", "zak"));
            l.Add(new AssignValue("[[person().age]]", "19"));
            l.Add(new AssignValue("[[person().address_id]]", "9"));

            l.Add(new AssignValue("[[address().id]]", "1"));
            l.Add(new AssignValue("[[address().addr]]", "11 test lane"));
            l.Add(new AssignValue("[[address().postcode]]", "3421"));

            l.Add(new AssignValue("[[address().id]]", "2"));
            l.Add(new AssignValue("[[address().addr]]", "16 test lane"));
            l.Add(new AssignValue("[[address().postcode]]", "3422"));
            var env = CreateExecutionEnvironment();
            env.AssignWithFrame(l, 0);
            env.CommitAssign();

            var Worker = new AdvancedRecordset(env);
            Worker.LoadRecordsetAsTable(personRecordsetName);
            Worker.LoadRecordsetAsTable(addressRecordsetName);
            return Worker;
        }

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_FromRecordset()
        {
            var Worker = CreatePersonAddressWorkers();
            Assert.IsNotNull(Worker);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ConvertDataTableToRecordset_ExpectDataInIEnvironment()
        {
            string returnRecordsetName = "person";
            string query = "select * from person";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);
            var started = false;
            var serviceOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("person", "[[person().name]]", "bob")
            };
            worker.ApplyResultToEnvironment(returnRecordsetName,
                serviceOutputs,
                results.Tables[0].Rows.Cast<DataRow>().ToList(),
                false, 0, ref started);

            var internalResult = worker.Environment.EvalAsList("[[person(*).name]]", 0);
            var e = internalResult.GetEnumerator();
            if (e.MoveNext())
            {
                Assert.AreEqual("bob", e.Current.ToString());
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunSimpleQuery()
        {
            string query = "select * from person";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);
            Assert.AreEqual(4, results.Tables[0].Rows.Count);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunQueryContainingAlias()
        {
            string query = "select name as username from person";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);
            Assert.AreEqual(4, results.Tables[0].Rows.Count);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunJoinQuery_ExpectAllResults()
        {
            var worker = CreatePersonAddressWorkers();
            string query = "select * from person p join address a on p.address_id=a.id";
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);

            Assert.IsInstanceOfType(results, typeof(DataSet));
            Assert.AreEqual(3, results.Tables[0].Rows.Count);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_SelectStatementWithAllias_Join_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            string query = "SELECT * FROM person JOIN address on person.address_id = address.id";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);

            //------------Assert Results-------------------------
            Assert.AreEqual("bob", Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Name"] as byte[]));
            Assert.AreEqual(21, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Age"] as byte[])));
            Assert.AreEqual(1, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["address_id"] as byte[])));
            Assert.AreEqual("11 test lane", Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Addr"] as byte[]));
            Assert.AreEqual(3421, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Postcode"] as byte[])));
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunWhereQuery_ExpectFilteredResults()
        {
            string query = "select * from person p join address a on p.address_id=a.id where a.addr=\"11 test lane\" order by Name";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var results = worker.ExecuteQuery(updatedQuery);
            Assert.AreEqual("bob", Encoding.UTF8.GetString(results.Tables[0].Rows[0]["name"] as byte[]));
            Assert.AreEqual(21, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["age"] as byte[])));
            Assert.AreEqual(1, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["address_id"] as byte[])));
            Assert.AreEqual("11 test lane", Encoding.UTF8.GetString(results.Tables[0].Rows[0]["addr"] as byte[]));
            Assert.AreEqual(3421, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["postcode"] as byte[])));
            Assert.AreEqual("jef", Encoding.UTF8.GetString(results.Tables[0].Rows[1]["name"] as byte[]));
            Assert.AreEqual(24, int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[1]["age"] as byte[])));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunWhereQuery_ExpectNoResults()
        {
            string query = "select * from person p join address a on p.address_id=a.id where p.Name=\"zak\"";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            var results = worker.ExecuteQuery(updatedQuery);

            Assert.AreEqual(0, results.Tables[0].Rows.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectCanRunMultipleQueries()
        {
            string query = "select CURRENT_TIMESTAMP;" +
                "select * from address;update person set Age=20 where Name=\"zak\";" +
                "select * from person p join address a on p.address_id=a.id where a.addr=\"11 test lane\" order by Name";
            var worker = CreatePersonAddressWorkers();
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = "";
            foreach (var statement in statements)
            {
                updatedQuery += worker.UpdateSqlWithHashCodes(statement) + ";";
            }
            var results = worker.ExecuteQuery(updatedQuery);

            Assert.AreEqual("bob", Encoding.UTF8.GetString(results.Tables[2].Rows[0]["Name"] as byte[]));
            Assert.AreEqual(21, int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[0]["Age"] as byte[])));
            Assert.AreEqual(1, int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[0]["address_id"] as byte[])));
            Assert.AreEqual("11 test lane", Encoding.UTF8.GetString(results.Tables[1].Rows[0]["Addr"] as byte[]));
            Assert.AreEqual(3421, int.Parse(Encoding.UTF8.GetString(results.Tables[1].Rows[0]["Postcode"] as byte[])));
            Assert.AreEqual("jef", Encoding.UTF8.GetString(results.Tables[2].Rows[1]["Name"] as byte[]));
            Assert.AreEqual(24, int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[1]["Age"] as byte[])));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectUpdateAffectedRows()
        {
            var worker = CreatePersonAddressWorkers();
            string query = "update person set Age=65 where Name=\"zak\";";
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            var results = worker.ExecuteNonQuery(updatedQuery);

            Assert.AreEqual(1, results);

            query = "select * from person where Name=\"zak\";";
            statements = TSQLStatementReader.ParseStatements(query);
            updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);

            var result = worker.ExecuteQuery(updatedQuery);

            Assert.AreEqual("zak", Encoding.UTF8.GetString(result.Tables[0].Rows[0]["Name"] as byte[]));
            Assert.AreEqual(65, int.Parse(Encoding.UTF8.GetString(result.Tables[0].Rows[0]["Age"] as byte[])));
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ExecuteScalar()
        {
            var worker = CreatePersonAddressWorkers();
            var query = "select * from person where Name=\"zak\";";
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            var result = worker.ExecuteScalar(updatedQuery);
            Assert.AreEqual(3, result);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ExecuteScalar_Exception()
        {
            string query = "select from person";
            var Worker = CreatePersonAddressWorkers();
            Assert.ThrowsException<Exception>(() => Worker.ExecuteScalar(query));

        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ExecuteNonQuery_ExpectBadSQLToError()
        {
            string query = "select from person";
            var Worker = CreatePersonAddressWorkers();
            Assert.ThrowsException<Exception>(() => Worker.ExecuteNonQuery(query));
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectBadSQLToError()
        {
            string query = "select from person";
            var Worker = CreatePersonAddressWorkers();
            Assert.ThrowsException<Exception>(() => Worker.ExecuteQuery(query));
        }




        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ReturnSql()
        {
            List<TSQLStatement> statements = TSQLStatementReader.ParseStatements("select * from person;", includeWhitespace: true);
            TSQLSelectStatement select = statements[0] as TSQLSelectStatement;
            var Worker = CreatePersonAddressWorkers();
            var sql = Worker.ReturnSql(select.Tokens);
            Assert.AreEqual("select   *   from   person", sql);
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_SetRecordsetName()
        {
            var advancedRecordset = new AdvancedRecordset { RecordsetName = "TestRecordsetName" };
            Assert.AreEqual("TestRecordsetName", advancedRecordset.RecordsetName);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_Environment()
        {
            var env = CreateExecutionEnvironment();
            var advancedRecordset = new AdvancedRecordset
            {
                Environment = env
            };
            Assert.AreEqual(env, advancedRecordset.Environment);
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_CreateVariableTable()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                Assert.IsTrue(true, "CreateVariableTable Passed");
            }
            catch
            {
                Assert.IsTrue(false, "CreateVariableTable Failed");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_InsertIntoVariableTable()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "testdata");
                Assert.IsTrue(true, "InsertIntoVariableTable");
            }
            catch
            {
                Assert.IsTrue(false, "InsertIntoVariableTable");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_GetVariableValue_String()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "testdata");
                Assert.AreEqual("'testdata'", worker.GetVariableValue("TestVariable"));
            }
            catch
            {
                Assert.IsTrue(false, "GetVariableValue");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_GetVariableValue_Int()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "100");
                Assert.AreEqual("100", worker.GetVariableValue("TestVariable"));
            }
            catch
            {
                Assert.IsTrue(false, "GetVariableValue");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_GetVariableValue_Double()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "100.00");
                Assert.AreEqual("100.00", worker.GetVariableValue("TestVariable"));
            }
            catch
            {
                Assert.IsTrue(false, "GetVariableValue");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_BuildInsertStatement()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "100.00");
                Assert.AreEqual("100.00", worker.GetVariableValue("TestVariable"));
            }
            catch
            {
                Assert.IsTrue(false, "GetVariableValue");
            }
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        public void AdvancedRecordset_BuildInsertStatement_Array()
        {
            try
            {
                var worker = CreatePersonAddressWorkers();
                worker.CreateVariableTable();
                worker.InsertIntoVariableTable("TestVariable", "100,200,300");
                Assert.AreEqual("100,200,300", worker.GetVariableValue("TestVariable"));
            }
            catch
            {
                Assert.IsTrue(false, "GetVariableValue");
            }
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ApplyResultToEnvironment_updatedTrue()
        {
            string returnRecordsetName = "person";
            var worker = CreatePersonAddressWorkers();
            string query = "update person set Age=65 where Name=\"zak\";";
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            worker.ExecuteQuery(updatedQuery);
            var started = false;

            query = "select * from person where Name=\"zak\";";
            statements = TSQLStatementReader.ParseStatements(query);
            updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            var results = worker.ExecuteQuery(updatedQuery);
            var serviceOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("person", "[[person().name]]", "bob")
            };
            foreach (DataTable dt in results.Tables)
            {
                worker.ApplyResultToEnvironment(returnRecordsetName, serviceOutputs, dt.Rows.Cast<DataRow>().ToList(), true, 0, ref started);
            }
            var internalResult = worker.Environment.EvalAsList("[[person(*).age]]", 0);
            var e = internalResult.GetEnumerator();
            if (e.MoveNext())
            {
                Assert.AreEqual("65", e.Current.ToString());
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_ApplyScalarResultToEnvironment_updatedTrue()
        {
            var worker = CreatePersonAddressWorkers();
            var query = "select * from person where Name=\"zak\";";
            var statements = TSQLStatementReader.ParseStatements(query);
            var updatedQuery = worker.UpdateSqlWithHashCodes(statements[0]);
            var result = worker.ExecuteScalar(updatedQuery);

            worker.ApplyScalarResultToEnvironment("[[Table1Copy().records_affected]]", result);

            var internalResult = worker.Environment.EvalAsList("[[Table1Copy().records_affected]]", 0);
            var e = internalResult.GetEnumerator();
            if (e.MoveNext())
            {
                Assert.AreEqual("3", e.Current.ToString());
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_AddRecordsetAsTable_ExecuteStatement_Select()
        {
            var l = new List<string>();
            l.Add("name");
            l.Add("age");
            l.Add("address_id");

            var advancedRecordset = new AdvancedRecordset();
            advancedRecordset.AddRecordsetAsTable(("person", l));

            var statements = TSQLStatementReader.ParseStatements("Select * from person");
            var results = advancedRecordset.ExecuteStatement(statements[0], "Select * from person");
            Assert.AreEqual("name", results.Tables[0].Columns[1].ColumnName);
            Assert.AreEqual("age", results.Tables[0].Columns[2].ColumnName);
            Assert.AreEqual("address_id", results.Tables[0].Columns[3].ColumnName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_AddRecordsetAsTable_ExecuteStatement_Insert()
        {
            var l = new List<string>();
            l.Add("name");
            l.Add("age");
            l.Add("address_id");

            var advancedRecordset = new AdvancedRecordset();
            advancedRecordset.AddRecordsetAsTable(("person", l));

            const string query = "INSERT OR REPLACE INTO person VALUES (1,'testName', 10, 1);";

            var statements = TSQLStatementReader.ParseStatements(query);
            var results = advancedRecordset.ExecuteStatement(statements[0], query);
            Assert.AreEqual(1, results.Tables[0].Rows[0]["records_affected"]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_LoadRecordsetAsTable_LoadIntoSqliteDb_BuildInsertStatement_colTypeTests()
        {
            var personRecordsetName = "person";

            var l = new List<AssignValue>();
            l.Add(new AssignValue("[[person().datatypesFloat]]", "4.5f"));
            l.Add(new AssignValue("[[person().testNothing]]", ""));
            l.Add(new AssignValue("[[person().testWarewolfAtom]]", "1"));

            var env = CreateExecutionEnvironment();
            env.AssignWithFrame(l, 0);
            env.CommitAssign();

            var advancedRecordset = new AdvancedRecordset(env);
            advancedRecordset.LoadRecordsetAsTable(personRecordsetName);

            Assert.IsNotNull(advancedRecordset);
        }
    }
}