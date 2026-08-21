using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Dev2.Services.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Services.Execution.Tests
{
    [TestClass]
    public class DatabaseServiceExecutionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void OnConstruction_GivenDataObject_ShouldConstruct()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var newDatabaseServiceExecution = new DatabaseServiceExecution(new Mock<IDSFDataObject>().Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(newDatabaseServiceExecution, "Cannot create new DatabaseServiceExecution object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TranslateDataTableToEnvironment_Given3PopulatedOutPuts_ShouldMappAll()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDSFDataObject>();
            var dt = GetTable();
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.HasRecordSet(It.IsAny<string>()));
            var newDatabaseServiceExecution = new DatabaseServiceExecution(mock.Object)
            {
                Outputs = new List<IServiceOutputMapping>()
                {
                    new ServiceOutputMapping("rec().a", "rec().a", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                }
            };
            //---------------Assert Precondition----------------
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("TranslateDataTableToEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(newDatabaseServiceExecution, new object[] { dt, env.Object, 0 });
            //---------------Test Result -----------------------
            env.Verify(environment => environment.HasRecordSet(It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TranslateDataTableToEnvironment_Given3OneEmptyPopulatedOutPuts_ShouldMapp2()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDSFDataObject>();
            var dt = GetTable();
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.HasRecordSet(It.IsAny<string>()));
            var newDatabaseServiceExecution = new DatabaseServiceExecution(mock.Object)
            {
                Outputs = new List<IServiceOutputMapping>()
                {
                    new ServiceOutputMapping("rec().a", "rec().a", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                    new ServiceOutputMapping("rec().b", "", "rec"),
                }
            };
            //---------------Assert Precondition----------------
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("TranslateDataTableToEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(newDatabaseServiceExecution, new object[] { dt, env.Object, 0 });
            //---------------Test Result -----------------------
            env.Verify(environment => environment.HasRecordSet(It.IsAny<string>()), Times.Exactly(2));
        }
        
        static DataTable GetTable()
        {
            // Here we create a DataTable with four columns.
            var table = new DataTable();
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));

            // Here we add five DataRows.
            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            return table;
        }

        [DataTestMethod]
        [Owner("Copilot")]
        [DataRow(18456, "Authentication")]
        [DataRow(40615, "Network/firewall")]
        [DataRow(40532, "Network/firewall")]
        [DataRow(40613, "unavailable")]
        [DataRow(-2, "timeout")]
        [DataRow(258, "timeout")]
        [DataRow(99999, "Unclassified")]
        public void ClassifySqlErrorNumber_GivenKnownAndUnknownErrorNumbers_ShouldReturnExpectedClassificationSubstring(int errorNumber, string expectedSubstring)
        {
            //---------------Set up test pack-------------------
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("ClassifySqlErrorNumber", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo, "ClassifySqlErrorNumber method not found via reflection.");
            //---------------Execute Test ----------------------
            var result = (string)methodInfo.Invoke(null, new object[] { errorNumber });
            //---------------Test Result -----------------------
            StringAssert.Contains(result, expectedSubstring);
        }

        [TestMethod]
        [Owner("Copilot")]
        public void BuildSqlErrorDetail_GivenNonSqlException_ShouldReturnPlainMessage()
        {
            //---------------Set up test pack-------------------
            var ex = new InvalidOperationException("some non-sql failure");
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("BuildSqlErrorDetail", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo, "BuildSqlErrorDetail method not found via reflection.");
            //---------------Execute Test ----------------------
            var result = (string)methodInfo.Invoke(null, new object[] { ex });
            //---------------Test Result -----------------------
            Assert.AreEqual("SQL Error: some non-sql failure", result);
        }

        #region IsProcedureTextUnavailable

        // Builds a real SqlException carrying the given error number. SqlException/SqlError have
        // no public constructors, so this goes through the same internal factory ADO.NET itself
        // uses. Kept tolerant of constructor-signature drift across Microsoft.Data.SqlClient
        // versions by binding arguments per-parameter rather than positionally.
        static SqlException CreateSqlException(int errorNumber, string message = "test sql error")
        {
            var errorCtor = typeof(SqlError)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .OrderBy(c => c.GetParameters().Length)
                .First();

            var args = errorCtor.GetParameters().Select(p =>
            {
                if (p.ParameterType == typeof(int) && p.Name.IndexOf("number", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return (object)errorNumber;
                }
                if (p.ParameterType == typeof(string))
                {
                    return p.Name.IndexOf("message", StringComparison.OrdinalIgnoreCase) >= 0 ? message : "test";
                }
                return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
            }).ToArray();

            var error = (SqlError)errorCtor.Invoke(args);

            var collection = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true);
            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });

            var createException = typeof(SqlException)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "CreateException"
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[0].ParameterType == typeof(SqlErrorCollection)
                            && m.GetParameters()[1].ParameterType == typeof(string));

            return (SqlException)createException.Invoke(null, new object[] { collection, "16.0.0" });
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsProcedureTextUnavailable")]
        public void IsProcedureTextUnavailable_GivenSqlError15197_ReturnsTrue()
        {
            //---------------Set up test pack-------------------
            // The exact production failure: sp_helptext raises 15197 because the connecting
            // principal holds EXECUTE but not VIEW DEFINITION on the procedure.
            var ex = CreateSqlException(15197, "There is no text for object 'dbo.usp_jobs1_LogStart'.");
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsProcedureTextUnavailable(ex);
            //---------------Test Result -----------------------
            Assert.IsTrue(result, "SQL error 15197 must be recognised as an unreadable-definition condition.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsProcedureTextUnavailable")]
        public void IsProcedureTextUnavailable_GivenWarewolfDbException_ReturnsTrue()
        {
            //---------------Set up test pack-------------------
            // MssqlGetSqlForProcedure raises this shape when sp_helptext returns zero rows
            // rather than erroring outright.
            var ex = new WarewolfDbException("There is no text for object 'dbo.usp_jobs1_LogStart'.");
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsProcedureTextUnavailable(ex);
            //---------------Test Result -----------------------
            Assert.IsTrue(result, "An empty sp_helptext result must be treated as an unreadable definition.");
        }

        [DataTestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsProcedureTextUnavailable")]
        [DataRow(208)]   // Invalid object name - the procedure genuinely does not exist
        [DataRow(229)]   // EXECUTE permission denied
        [DataRow(18456)] // Login failed
        [DataRow(40613)] // Database not currently available (transient, handled elsewhere)
        public void IsProcedureTextUnavailable_GivenUnrelatedSqlError_ReturnsFalse(int errorNumber)
        {
            //---------------Set up test pack-------------------
            // These are real failures that must still surface to the caller - degrading to the
            // non-FOR XML path would hide a missing procedure or a denied EXECUTE.
            var ex = CreateSqlException(errorNumber);
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsProcedureTextUnavailable(ex);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, $"SQL error {errorNumber} is a genuine failure and must not be swallowed.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsProcedureTextUnavailable")]
        public void IsProcedureTextUnavailable_GivenNonSqlException_ReturnsFalse()
        {
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsProcedureTextUnavailable(new InvalidOperationException("boom"));
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "A non-SQL exception must never be treated as an unreadable definition.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsProcedureTextUnavailable")]
        public void IsProcedureTextUnavailable_GivenNull_ReturnsFalse()
        {
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsProcedureTextUnavailable(null);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "A null exception must not be treated as an unreadable definition.");
        }

        #endregion

        #region IsForXmlProcedureScript

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenForXmlAutoBody_ReturnsTrue()
        {
            //---------------Set up test pack-------------------
            const string script = @"CREATE PROCEDURE dbo.usp_GetCustomers AS BEGIN SELECT Id, Name FROM dbo.Customers FOR XML AUTO END";
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(script);
            //---------------Test Result -----------------------
            Assert.IsTrue(result, "A FOR XML AUTO procedure must be routed to the FOR XML read path.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenForXmlPathBody_ReturnsTrue()
        {
            //---------------Set up test pack-------------------
            const string script = @"CREATE PROCEDURE dbo.usp_GetCustomers AS BEGIN SELECT Id, Name FROM dbo.Customers FOR XML PATH('Customer') END";
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(script);
            //---------------Test Result -----------------------
            Assert.IsTrue(result, "A FOR XML PATH procedure must be routed to the FOR XML read path.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenPlainSelectBody_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            const string script = @"CREATE PROCEDURE dbo.usp_GetCustomers AS BEGIN SELECT Id, Name FROM dbo.Customers END";
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(script);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "A plain SELECT procedure must use the standard read path.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenJobLoggingProcedureShape_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            // Mirrors the usp_jobs1_LogStart shape the ShovelBridge load test exercises: an
            // INSERT followed by a scalar SELECT of the generated identity. No FOR XML anywhere.
            const string script = @"
CREATE PROCEDURE dbo.usp_jobs1_LogStart
    @MessageContent nvarchar(max), @ExecutionEngineInstanceId nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.jobs1 (MessageContent, ExecutionEngineInstanceId, Status)
    VALUES (@MessageContent, @ExecutionEngineInstanceId, 'Started');
    SELECT CAST(SCOPE_IDENTITY() AS bigint) AS JobLogId, 1 AS AttemptNumber;
END";
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(script);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "The job-logging procedures return a normal recordset, not FOR XML.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenEmptyScript_ReturnsFalse()
        {
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(string.Empty);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "An empty script cannot be a FOR XML procedure.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("IsForXmlProcedureScript")]
        public void IsForXmlProcedureScript_GivenForClauseThatIsNotForXml_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            // A cursor declaration also contains the FOR keyword; it must not be mistaken for
            // a FOR XML result shape.
            const string script = @"CREATE PROCEDURE dbo.usp_Loop AS BEGIN DECLARE c CURSOR FOR SELECT Id FROM dbo.Customers END";
            //---------------Execute Test ----------------------
            var result = DatabaseServiceExecution.IsForXmlProcedureScript(script);
            //---------------Test Result -----------------------
            Assert.IsFalse(result, "A non-XML FOR clause must not be detected as FOR XML.");
        }

        #endregion
    }
}
