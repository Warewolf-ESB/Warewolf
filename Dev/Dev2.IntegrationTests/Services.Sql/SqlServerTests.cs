
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

        internal static void Verify_DataTable_CountriesPrefixIsA(DataTable countriesDataTable)
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

        internal static void Verify_FetchStoredProcedures_Pr_CitiesGetCountries(IDbCommand actualCommand, List<IDbDataParameter> actualParameters, string actualHelpText)
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

        internal static void Verify_FetchStoredProcedures_WarewolfRunForSql(List<IDbDataParameter> actualParameters, string actualHelpText)
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

        internal static void Verify_FetchStoredProcedures_Fn_Greeting(IDbCommand actualCommand, List<IDbDataParameter> actualParameters, string actualHelpText)
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
