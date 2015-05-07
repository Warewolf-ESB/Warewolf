
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
    public class SqlServerTests
    {
        // ReSharper disable InconsistentNaming

        public static DbSource CreateDev2TestingDbSource(AuthenticationType authenticationType = AuthenticationType.User)
        {
            var dbSource = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingDB",
                ResourcePath = "Test",
                DatabaseName = "Dev2TestingDB",
                Server = "RSAKLFSVRGENDEV",
                AuthenticationType = authenticationType,
                ServerType = enSourceType.SqlDatabase,
                ReloadActions = true,
                UserID = authenticationType == AuthenticationType.User ? "testUser" : null,
                Password = authenticationType == AuthenticationType.User ? "test123" : null
            };
            return dbSource;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_Connect")]
        [ExpectedException(typeof(SqlException))]
        public void SqlServer_Connect_InvalidLogin_ThrowsSqlException()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();
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
            var dbSource = CreateDev2TestingDbSource();

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
            var dbSource = CreateDev2TestingDbSource();
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
            var dbSource = CreateDev2TestingDbSource();

            var expected = new List<string>();
            using(var connection = new SqlConnection(dbSource.ConnectionString))
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
            var dbSource = CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString, CommandType.StoredProcedure, "Pr_CitiesGetCountries");

                //------------Execute Test---------------------------
                var actual = sqlServer.FetchDataTable(new SqlParameter("@Prefix", "a"));

                //------------Assert Results-------------------------
                Verify_DataTable_CountriesPrefixIsA(actual);
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
            var dbSource = CreateDev2TestingDbSource();

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString, CommandType.StoredProcedure, "Pr_CitiesGetCountries");

                //------------Execute Test---------------------------
                var actualDataSet = sqlServer.FetchDataSet(new SqlParameter("@Prefix", "a"));

                //------------Assert Results-------------------------
                Assert.AreEqual(1, actualDataSet.Tables.Count);

                var actual = actualDataSet.Tables[0];

                Verify_DataTable_CountriesPrefixIsA(actual);
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
            var dbSource = CreateDev2TestingDbSource();

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
            var dbSource = CreateDev2TestingDbSource();

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
            var dbSource = CreateDev2TestingDbSource();

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

                Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procedureProcessor = (dbCommand, list, helpText,bob) =>
                {
                    if(dbCommand.CommandText == "dbo.Pr_CitiesGetCountries")
                    {
                        procedureCommand = dbCommand;
                        procedureCommandParameters = list;
                        procedureHelpText = helpText;
                    }
                    return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string,string, bool> functionProcessor = (dbCommand, list, helpText,bob) =>
                {
                    if(dbCommand.CommandText == "dbo.fn_Greeting")
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
            Verify_FetchStoredProcedures_Pr_CitiesGetCountries(procedureCommand, procedureCommandParameters, procedureHelpText);
            Verify_FetchStoredProcedures_Fn_Greeting(functionCommand, functionCommandParameters, functionHelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlServer_FetchStoredProcedures")]
        public void SqlServer_FetchStoredProcedures_WithClrTypeStoredProcedure_CorrectDataReturned()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();

            List<IDbDataParameter> procedureCommandParameters = null;
            string procedureHelpText = null;

            var sqlServer = new SqlServer();
            try
            {
                sqlServer.Connect(dbSource.ConnectionString);

                Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procedureProcessor = (dbCommand, list, helpText,bob) =>
                {
                    if(dbCommand.CommandText == "Warewolf.RunWorkflowForSql")
                    {
                        procedureCommandParameters = list;
                        procedureHelpText = helpText;
                    }
                    return true;
                };
                Func<IDbCommand, List<IDbDataParameter>, string,string, bool> functionProcessor = (dbCommand, list, helpText,bob) => true;

                //------------Execute Test---------------------------
                sqlServer.FetchStoredProcedures(procedureProcessor, functionProcessor, true);
            }
            finally
            {
                sqlServer.Dispose();
            }

            //------------Assert Results-------------------------
            Verify_FetchStoredProcedures_WarewolfRunForSql(procedureCommandParameters, procedureHelpText);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_FetchTableValuedFunctions")]
        public void SqlServer_FetchTableValuedFunctions_AssertSelectTextIsDifferent()
        {
            //------------Setup for test--------------------------
            var dbSource = CreateDev2TestingDbSource();

            List<IDbDataParameter> procedureCommandParameters = null;
            string procedureHelpText = null;
            string select="";
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
            Assert.IsTrue( procedureHelpText.Contains(@"insert into @Countries
	select CountryID from dbo.Country"));
            Assert.AreEqual("select * from dbo.bob(@country)", select);
        }

        static void Verify_DataTable_CountriesPrefixIsA(DataTable countriesDataTable)
        {
            var expectedIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var expectedNames = new[] { "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan" };

            //------------Assert Results-------------------------
            Assert.AreEqual(2, countriesDataTable.Columns.Count);
            Assert.AreEqual("CountryID", countriesDataTable.Columns[0].ColumnName);
            Assert.AreEqual("Description", countriesDataTable.Columns[1].ColumnName);

            Assert.AreEqual(10, countriesDataTable.Rows.Count);
            var actualIds = countriesDataTable.Rows.Cast<DataRow>().Select(row => (int)row[0]).ToList();
            var actualNames = countriesDataTable.Rows.Cast<DataRow>().Select(row => (string)row[1]).ToList();

            CollectionAssert.AreEqual(expectedIds, actualIds);
            CollectionAssert.AreEqual(expectedNames, actualNames);
        }

        static void Verify_FetchStoredProcedures_Pr_CitiesGetCountries(IDbCommand actualCommand, List<IDbDataParameter> actualParameters, string actualHelpText)
        {
            Assert.IsNotNull(actualCommand);
            Assert.IsNotNull(actualParameters);
            Assert.IsNotNull(actualHelpText);

            Assert.AreEqual(1, actualCommand.Parameters.Count);
            Assert.AreEqual("@Prefix", ((IDbDataParameter)actualCommand.Parameters[0]).ParameterName);

            Assert.AreEqual(1, actualParameters.Count);
            Assert.AreEqual("@Prefix", actualParameters[0].ParameterName);
            Assert.AreEqual(DbType.AnsiString, actualParameters[0].DbType);
            Assert.AreEqual(50, actualParameters[0].Size);

            const string ExpectedHelpText = @"
CREATE procedure [dbo].[spGetCountries]

@Prefix varchar(50)

as

select * from Country 
where [Description] like @Prefix + '%'
order by Description asc

";

            Assert.AreEqual(ExpectedHelpText, actualHelpText);
        }

        static void Verify_FetchStoredProcedures_WarewolfRunForSql(List<IDbDataParameter> actualParameters, string actualHelpText)
        {
            Assert.IsNotNull(actualParameters);
            Assert.IsNotNull(actualHelpText);

            Assert.AreEqual(2, actualParameters.Count);
            Assert.AreEqual("@ServerUri", actualParameters[0].ParameterName);
            Assert.AreEqual(DbType.String, actualParameters[0].DbType);

            Assert.AreEqual("@RecordsetName", actualParameters[1].ParameterName);
            Assert.AreEqual(DbType.String, actualParameters[1].DbType);

            const string ExpectedHelpText = @"There is no text for object 'Warewolf.RunWorkflowForSql'.";

            Assert.AreEqual(ExpectedHelpText, actualHelpText);
        }

        static void Verify_FetchStoredProcedures_Fn_Greeting(IDbCommand actualCommand, List<IDbDataParameter> actualParameters, string actualHelpText)
        {
            Assert.IsNotNull(actualCommand);
            Assert.IsNotNull(actualParameters);
            Assert.IsNotNull(actualHelpText);

            Assert.AreEqual(1, actualCommand.Parameters.Count);
            Assert.AreEqual("@Name", ((IDbDataParameter)actualCommand.Parameters[0]).ParameterName);

            Assert.AreEqual(1, actualParameters.Count);
            Assert.AreEqual("@Name", actualParameters[0].ParameterName);
            Assert.AreEqual(DbType.AnsiString, actualParameters[0].DbType);
            Assert.AreEqual(255, actualParameters[0].Size);

            const string ExpectedHelpText = @"
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fn_Greeting]
(
	-- Add the parameters for the function here
	@Name VARCHAR(255)
)
RETURNS VARCHAR(255)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @MyGreeting VARCHAR(255)

	-- Add the T-SQL statements to compute the return value here
	SELECT @MyGreeting  = 'Hello, ' + CAST(@Name AS VARCHAR);

	-- Return the result of the function
	RETURN @MyGreeting

END

";

            Assert.AreEqual(ExpectedHelpText, actualHelpText);
        }

        // ReSharper restore InconsistentNaming
    }
}
