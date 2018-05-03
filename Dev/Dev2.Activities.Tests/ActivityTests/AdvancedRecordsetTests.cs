/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
using System.Data;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces.DB;
using System.Text;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class AdvancedRecordsetTests : BaseActivityTests
    {
        public AdvancedRecordset CreatePersonAddressWorkers()
        {
            var personRecordsetName = "person";
            var addressRecordsetName = "address";
            var env = new ExecutionEnvironment();
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

            env.AssignWithFrame(l, 0);
            env.CommitAssign();

            var Worker = new AdvancedRecordset(env);
            Worker.LoadRecordsetAsTable(personRecordsetName);
            Worker.LoadRecordsetAsTable(addressRecordsetName);
            return Worker;
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Converter")]
        public void AdvancedRecordset_Converter_FromRecordset()
        {
            var Worker = CreatePersonAddressWorkers();
            Assert.IsNotNull(Worker);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ConvertDataTableToRecordset_ExpectDataInIEnvironment()
        {
            string returnRecordsetName = "person";
            string query = "select * from person";
            var worker = CreatePersonAddressWorkers();
            var results = worker.ExecuteQuery(query);
            var started = false;

            // apply sql results to environment
            worker.ApplyResultToEnvironment(returnRecordsetName,new List<IServiceOutputMapping>(), results.Tables[0].Rows.Cast<DataRow>().ToList(),false,0, ref started);

            // fetch newly inserted data from environment
            var internalResult = worker.Environment.EvalAsList("[[person(*).name]]", 0);

            // assert that data fetched is what we expect from sql
            var e = internalResult.GetEnumerator();
            if (e.MoveNext())
            {
                Assert.AreEqual(e.Current, "bob");
            }
            else
            {
                Assert.Fail();
            }
        }

        #region Tests

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunSimpleQuery()
        {
            string query = "select * from person";
            var Worker = CreatePersonAddressWorkers();

            var Results = Worker.ExecuteQuery(query);
            Assert.AreEqual(4, Results.Tables[0].Rows.Count);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunQueryContainingAlias()
        {
            string query = "select name as username from person";
            var Worker = CreatePersonAddressWorkers();

            var Results = Worker.ExecuteQuery(query);
            Assert.AreEqual(4, Results.Tables[0].Rows.Count);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunJoinQuery_ExpectAllResults()
        {
            var worker = CreatePersonAddressWorkers();
            string query = "select * from person p join address a on p.address_id=a.id";

            var results = worker.ExecuteQuery(query);

            Assert.IsInstanceOfType(results, typeof(DataSet));
            Assert.AreEqual(results.Tables[0].Rows.Count, 3);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_SelectStatementWithAllias_Join_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            string query = "SELECT * FROM person JOIN address on person.address_id = address.id";
            var Worker = CreatePersonAddressWorkers();
            var results = Worker.ExecuteQuery(query);

            //------------Assert Results-------------------------
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Name"] as byte[]), "bob");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Age"] as byte[])), (Int32)21);
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["address_id"] as byte[])), (Int32)1);
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Addr"] as byte[]), "11 test lane");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["Postcode"] as byte[])), (Int32)3421);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunWhereQuery_ExpectFilteredResults()
        {
            string query = "select * from person p join address a on p.address_id=a.id where a.addr=\"11 test lane\" order by Name";
            var Worker = CreatePersonAddressWorkers();
            var results = Worker.ExecuteQuery(query);
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["name"] as byte[]), "bob");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["age"] as byte[])), (Int32)21);
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["address_id"] as byte[])), (Int32)1);
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["addr"] as byte[]), "11 test lane");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[0]["postcode"] as byte[])), (Int32)3421);
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[0].Rows[1]["name"] as byte[]), "jef");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[0].Rows[1]["age"] as byte[])), (Int32)24);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_CanRunWhereQuery_ExpectNoResults()
        {
            string query = "select * from person p join address a on p.address_id=a.id where p.Name=\"zak\"";
            var Worker = CreatePersonAddressWorkers();

            var results = Worker.ExecuteQuery(query);
            Assert.AreEqual(results.Tables[0].Rows.Count, 0);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectCanRunMultipleQueries()
        {
            string query = "select CURRENT_TIMESTAMP;" +
                "select * from address;update person set Age=20 where Name=\"zak\";" +
                "select * from person p join address a on p.address_id=a.id where a.addr=\"11 test lane\" order by Name";
            var Worker = CreatePersonAddressWorkers();

            var results = Worker.ExecuteQuery(query);
           
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[2].Rows[0]["Name"] as byte[]), "bob");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[0]["Age"] as byte[])), (Int32)21);
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[0]["address_id"] as byte[])), (Int32)1);
            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[1].Rows[0]["Addr"] as byte[]), "11 test lane");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[1].Rows[0]["Postcode"] as byte[])), (Int32)3421);

            Assert.AreEqual(Encoding.UTF8.GetString(results.Tables[2].Rows[1]["Name"] as byte[]), "jef");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(results.Tables[2].Rows[1]["Age"] as byte[])), (Int32)24);
           
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectUpdateAffectedRows()
        {
            var Worker = CreatePersonAddressWorkers();
            string query = "update person set Age=65 where Name=\"zak\";";
            var results = Worker.ExecuteNonQuery(query);
            Assert.AreEqual(1, results);
            query = "select * from person where Name=\"zak\";";
            var result = Worker.ExecuteQuery(query);
            Assert.AreEqual(Encoding.UTF8.GetString(result.Tables[0].Rows[0]["Name"] as byte[]), "zak");
            Assert.AreEqual(int.Parse(Encoding.UTF8.GetString(result.Tables[0].Rows[0]["Age"] as byte[])), (Int32)65);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AdvancedRecordset_Converter_ExpectBadSQLToError()
        {
            string query = "select from person";
            var Worker = CreatePersonAddressWorkers();

            Assert.ThrowsException<Exception>(() => Worker.ExecuteQuery(query));
        }

        #endregion

    }
}