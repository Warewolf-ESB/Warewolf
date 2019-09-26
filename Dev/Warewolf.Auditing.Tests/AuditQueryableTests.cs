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
            var resId = Guid.NewGuid();
            var auditQueryable = new Mock<IAuditQueryable>();
            var query = new Dictionary<string, StringBuilder>
            {
                {
                    "ResourceId", resId.ToString().ToStringBuilder()
                }
            };
            auditQueryable.Setup(o => o.QueryTriggerData(query)).Verifiable();

            var queryObject = auditQueryable.Object;
            queryObject.QueryTriggerData(query);
            auditQueryable.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_QueryLogData()
        {
            var executionID = Guid.NewGuid();
            var audit = new AuditStub();
            audit.AuditType = "Information";
            audit.ExecutionID = executionID.ToString();
            var list = new List<AuditStub>
            {
                audit
            };
            var connstring = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
            var auditQueryable = new AuditQueryable(connstring,"Logs");
            var query = new Dictionary<string, StringBuilder>
            {
                {
                    "ExecutionID", executionID.ToString().ToStringBuilder()
                }
            };
            var results = auditQueryable.QueryLogData(query);
            Assert.IsNotNull(results) ;
            var historyJson = JsonConvert.SerializeObject(results);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_GetQueueLogData()
        {
            var resourceID = Guid.NewGuid();
            var auditQueryable = new Mock<IAuditQueryable>();
            auditQueryable.Setup(o => o.GetQueueLogData(resourceID.ToString())).Verifiable();
            var queryObject = auditQueryable.Object;
            queryObject.GetQueueLogData(resourceID.ToString());
            auditQueryable.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_GetLogData()
        {
            var resourceID = Guid.NewGuid();
            var auditQueryable = new Mock<IAuditQueryable>();
            var sql = new StringBuilder($"SELECT * FROM Logs ");
            auditQueryable.Setup(o => o.GetLogData(resourceID.ToString(), sql)).Verifiable();
            var queryObject = auditQueryable.Object;
            queryObject.GetLogData(resourceID.ToString(), sql);
            auditQueryable.Verify();
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
    }
}
