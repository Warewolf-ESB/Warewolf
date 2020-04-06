/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warewolf.Auditing.Drivers;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class AuditQueryableTests
    {
        string connstring = @"C:\ProgramData\Warewolf\Audits\AuditTestDB.db";
        string sqlMessage = "SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ";

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_QueryTriggerData()
        {
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var query = new Dictionary<string, StringBuilder>();
            var results = auditQueryable.QueryTriggerData(query).ToList();
            Assert.IsNotNull(results);
            Assert.AreEqual(null, auditQueryable.Query);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        public void AuditQueryable_AuditQueryableElastic_QueryLogData()
        {
            var executionID = Guid.NewGuid();

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData",connstring);
            var results = auditQueryable.QueryLogData(query).ToList();
            Assert.IsNotNull(results);
            Assert.IsTrue(auditQueryable is AuditQueryableElastic);
            
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryable))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryable_AuditQueryableSqlite_QueryLogData()
        {
            var executionID = Guid.NewGuid();
            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();

            Assert.IsTrue(auditQueryable is AuditQueryableSqlite);
        }

        private IAuditQueryable GetAuditQueryable(string sink,string connectionString)
        {
            if (sink == "AuditingSettingsData")
            {
                return new AuditQueryableElastic();
            }
            else
            {
                return new AuditQueryableSqlite(connectionString);
            }
        }
    }
}