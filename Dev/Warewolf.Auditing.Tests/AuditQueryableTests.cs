/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class AuditQueryableTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_QueryTriggerData()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var query = new Dictionary<string, StringBuilder>();
            var result = auditQueryable.QueryTriggerData(query);
            var historyJson = result.ToArray();
            Assert.AreEqual(null, auditQueryable.ConnectionString);
            Assert.AreEqual(null, auditQueryable.SqlString);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_QueryTriggerData_FilterByResourceId()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var resourceId = Guid.NewGuid();
            var query = new Dictionary<string, StringBuilder>
            {
                {"ResourceId", resourceId.ToString().ToStringBuilder()}               
            };

            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");          
            var result = auditQueryable.QueryTriggerData(query);
            var historyJson = result.ToArray();
            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual("SELECT * from Logs WHERE json_extract(Properties, '$.ResourceId') = '" + resourceId + "' ", auditQueryable.SqlString.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_FilterBy_NoParameters()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual("SELECT * FROM Logs ", auditQueryable.SqlString.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_FilterBy_ExecutionId_EventLevel()
        {
            var executionID = Guid.NewGuid();
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel","Debug".ToStringBuilder() }
            };
            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual(@"SELECT * FROM Logs WHERE Level = 'Debug' AND json_extract(Properties, '$.ExecutionID') = '" + executionID.ToString() + "' ", auditQueryable.SqlString.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_FilterBy_EventLevel()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel","Debug".ToStringBuilder() }
            };

            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");

            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual(@"SELECT * FROM Logs WHERE Level = 'Debug' ", auditQueryable.SqlString.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_FilterBy_DateTime()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                {"StartDateTime",StartDateTime.ToString().ToStringBuilder() },
                {"CompletedDateTime",CompletedDateTime.ToString().ToStringBuilder() }
            };

            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual(@"SELECT * FROM Logs WHERE (Timestamp >= '" + StartDateTime + "' AND Timestamp <= '" + CompletedDateTime + "') ", auditQueryable.SqlString.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_DateTime_EventLevel()
        {
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                { "EventLevel","Debug".ToStringBuilder() },
                {"StartDateTime",StartDateTime.ToString().ToStringBuilder() },
                {"CompletedDateTime",CompletedDateTime.ToString().ToStringBuilder() }
            };

            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual(@"SELECT * FROM Logs WHERE Level = 'Debug' AND (Timestamp >= '" + StartDateTime.ToString() + "' AND Timestamp <= '" + CompletedDateTime.ToString() + "') ", auditQueryable.SqlString.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData_FilterBy_DateTime_EventLevel_executionID()
        {
            var executionID = Guid.NewGuid();
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                { "ExecutionID", executionID.ToString().ToStringBuilder()},
                { "EventLevel","Debug".ToStringBuilder() },
                {"StartDateTime",StartDateTime.ToString().ToStringBuilder() },
                {"CompletedDateTime",CompletedDateTime.ToString().ToStringBuilder() }
            };

            var auditQueryable = new AuditQueryableForTesting(connstring, "Logs");
            var results = auditQueryable.QueryLogData(query);
            var historyJson = results.ToArray();

            Assert.AreEqual(connstring, auditQueryable.ConnectionString);
            Assert.AreEqual(@"SELECT * FROM Logs WHERE Level = 'Debug' AND json_extract(Properties, '$.ExecutionID') = '"+ executionID + "' AND (Timestamp >= '" + StartDateTime.ToString() + "' AND Timestamp <= '" + CompletedDateTime.ToString() + "') ", auditQueryable.SqlString.ToString());
        }
        public class AuditStub : IAudit
        {
            public string AdditionalDetail { get; set; }
            public DateTime AuditDate { get; set; }
            public string AuditType { get; set; }
            public string Environment { get; set; }
            public Exception Exception { get; set; }
            public string ExecutingUser { get; set; }
            public string ExecutionID { get; set; }
            public long ExecutionOrigin { get; set; }
            public string ExecutionOriginDescription { get; set; }
            public string ExecutionToken { get; set; }
            public int Id { get; set; }
            public bool IsRemoteWorkflow { get; set; }
            public bool IsSubExecution { get; set; }
            public string NextActivity { get; set; }
            public string NextActivityId { get; set; }
            public string NextActivityType { get; set; }
            public string ParentID { get; set; }
            public string PreviousActivity { get; set; }
            public string PreviousActivityId { get; set; }
            public string PreviousActivityType { get; set; }
            public string ServerID { get; set; }
            public string VersionNumber { get; set; }
            public string WorkflowID { get; set; }
            public string WorkflowName { get; set; }
        }

        public class AuditQueryableForTesting : AuditQueryable
        {
            public string ConnectionString { get; set; }
            public StringBuilder SqlString { get; private set; }

            public AuditQueryableForTesting(string connectionString, string tableName) : base(connectionString, tableName)
            {
            }

            protected override string[] ExecuteDatabase(string connectionString, StringBuilder sql)
            {
                ConnectionString = connectionString;
                SqlString = sql;
                string[] v = new string[0];
                return v;
            }
        }
    }
}
