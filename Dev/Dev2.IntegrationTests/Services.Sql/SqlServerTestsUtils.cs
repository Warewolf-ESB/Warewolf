
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Services.Sql
{
    [TestClass]
    public class SqlServerTestsUtils
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        [ExpectedException(typeof(SqlException))]
        public void SqlServer_Connect_InvalidLogin_ThrowsSqlException()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            dbSource.Password = Guid.NewGuid().ToString(); // Random invalid password

            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect(dbSource.ConnectionString);

                //------------Assert Results-------------------------
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        public void SqlServer_Connect_ValidLogin_IsConnectedIsTrue()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                sqlServer.Connect(dbSource.ConnectionString);

                //------------Assert Results-------------------------
                Assert.IsTrue(sqlServer.IsConnected);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        public void SqlServer_Connect_UserIDInDomainFormat_ConnectionStringIsNotIntegratedSecurityEqualsSSPI()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();
            dbSource.UserID = "Dev2\\TestUser";

            var sqlServer = new SqlServer();
            try
            {
                //------------Execute Test---------------------------
                try
                {
                    // expect this call to throw a 'login failed' exception
                    sqlServer.Connect(dbSource.ConnectionString);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }

                //------------Assert Results-------------------------
                Assert.IsFalse(sqlServer.ConnectionString.Contains("Integrated Security=SSPI"));
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDatabases")]
        public void SqlServer_FetchDatabases_SortedListOfNames()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var expected = new List<string>();
            using (var connection = new SqlConnection(dbSource.ConnectionString))
            {
                connection.Open();
                var databases = connection.GetSchema("Databases");
                connection.Close();

                var names = databases.Rows.Cast<DataRow>().Select(row => (row["database_name"] ?? string.Empty).ToString()).Distinct().OrderBy(s => s);
                expected.AddRange(names);
            }

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                //------------Execute Test---------------------------
                var actual = sqlServer.FetchDatabases();

                //------------Assert Results-------------------------
                CollectionAssert.AreEqual(expected, actual);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataTable")]
        public void SqlServer_FetchDataTable_CorrectDataReturned()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString, CommandType.StoredProcedure, "Pr_CitiesGetCountries");

                //------------Execute Test---------------------------
                var actual = sqlServer.FetchDataTable(new SqlParameter("@Prefix", "a"));

                //------------Assert Results-------------------------
                SqlServerTests.Verify_DataTable_CountriesPrefixIsA(actual);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchDataSet")]
        public void SqlServer_FetchDataSet_CorrectDataReturned()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString, CommandType.StoredProcedure, "Pr_CitiesGetCountries");

                //------------Execute Test---------------------------
                var actualDataSet = sqlServer.FetchDataSet(new SqlParameter("@Prefix", "a"));

                //------------Assert Results-------------------------
                Assert.AreEqual(1, actualDataSet.Tables.Count);

                var actual = actualDataSet.Tables[0];

                SqlServerTests.Verify_DataTable_CountriesPrefixIsA(actual);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_CreateCommand")]
        public void SqlServer_CreateCommand_TranactionNotStarted_CommandTransactionIsNull()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                //------------Execute Test---------------------------
                var command = sqlServer.CreateCommand();

                //------------Assert Results-------------------------
                Assert.IsNotNull(command);
                Assert.IsNull(command.Transaction);
            }
            finally
            {
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_CreateCommand")]
        public void SqlServer_CreateCommand_TranactionStarted_CommandTransactionIsNotNull()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);
                sqlServer.BeginTransaction();

                //------------Execute Test---------------------------
                var command = sqlServer.CreateCommand();

                //------------Assert Results-------------------------
                Assert.IsNotNull(command);
                Assert.IsNotNull(command.Transaction);
            }
            finally
            {
                sqlServer.RollbackTransaction();
                sqlServer.Dispose();
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchStoredProcedures")]
        public void SqlServer_FetchStoredProcedures_CorrectDataReturned()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            IDbCommand procedureCommand = null;
            List<IDbDataParameter> procedureCommandParameters = null;
            string procedureHelpText = null;

            IDbCommand functionCommand = null;
            List<IDbDataParameter> functionCommandParameters = null;
            string functionHelpText = null;

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor = (dbCommand, list, helpText, bob) =>
                {
                    if (dbCommand.CommandText == "dbo.Pr_CitiesGetCountries")
                    {
                        procedureCommand = dbCommand;
                        procedureCommandParameters = list;
                        procedureHelpText = helpText;
                    }
                    return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor = (dbCommand, list, helpText, bob) =>
                {
                    if (dbCommand.CommandText == "dbo.fn_Greeting")
                    {
                        functionCommand = dbCommand;
                        functionCommandParameters = list;
                        functionHelpText = helpText;
                    }
                    return true;
                };

                //------------Execute Test---------------------------
                sqlServer.FetchStoredProcedures(procedureProcessor, functionProcessor, true);
            }
            finally
            {
                sqlServer.Dispose();
            }

            //------------Assert Results-------------------------
            SqlServerTests.Verify_FetchStoredProcedures_Pr_CitiesGetCountries(procedureCommand, procedureCommandParameters, procedureHelpText);
            SqlServerTests.Verify_FetchStoredProcedures_Fn_Greeting(functionCommand, functionCommandParameters, functionHelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchStoredProcedures")]
        public void SqlServer_FetchStoredProcedures_WithClrTypeStoredProcedure_CorrectDataReturned()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            List<IDbDataParameter> procedureCommandParameters = null;
            string procedureHelpText = null;

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor = (dbCommand, list, helpText, bob) =>
                {
                    if (dbCommand.CommandText == "Warewolf.RunWorkflowForSql")
                    {
                        procedureCommandParameters = list;
                        procedureHelpText = helpText;
                    }
                    return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor = (dbCommand, list, helpText, bob) => true;

                //------------Execute Test---------------------------
                sqlServer.FetchStoredProcedures(procedureProcessor, functionProcessor, true);
            }
            finally
            {
                sqlServer.Dispose();
            }

            //------------Assert Results-------------------------
            SqlServerTests.Verify_FetchStoredProcedures_WarewolfRunForSql(procedureCommandParameters, procedureHelpText);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchTableValuedFunctions")]
        public void SqlServer_FetchTableValuedFunctions_AssertSelectTextIsDifferent()
        {
            //------------Setup for test--------------------------
            var dbSource = SqlServerTests.CreateDev2TestingDbSource();

            List<IDbDataParameter> procedureCommandParameters = null;
            string procedureHelpText = null;
            string select = "";
            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor = (dbCommand, list, helpText, bob) =>
                {
                    if (dbCommand.CommandText == "dbo.bob")
                    {
                        procedureCommandParameters = list;
                        procedureHelpText = helpText;
                        select = bob;
                    }
                    return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor = (dbCommand, list, helpText, bob) => true;

                //------------Execute Test---------------------------
                sqlServer.FetchStoredProcedures(procedureProcessor, functionProcessor, true);
            }
            finally
            {
                sqlServer.Dispose();
            }

            //------------Assert Results-------------------------
            Assert.AreEqual("@country", procedureCommandParameters.First().ParameterName);
            Assert.IsTrue(procedureHelpText.Contains(@"insert into @Countries
	select CountryID from dbo.Country"));
            Assert.AreEqual("select * from dbo.bob(@country)", select);
        }
    }
}
