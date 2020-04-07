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
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using Warewolf.Auditing.Drivers;
using Warewolf.Interfaces.Auditing;
using Warewolf.Driver.Serilog;

namespace Warewolf.Auditing.Tests
{
   
    [TestClass]
    public class AuditQueryableSqliteTests
    {
        string connstring = @"C:\ProgramData\Warewolf\Audits\AuditTestDB.db";
        string sqlMessage = "SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ";
        [ClassInitialize]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public static void TestFixtureSetup(TestContext context)
        {
            var testDBPath = @"C:\ProgramData\Warewolf\Audits\AuditTestDB.db";
            if (File.Exists(testDBPath))
                File.Delete(testDBPath);

            var testTableName = "Logs";
            var logger = new LoggerConfiguration().WriteTo.SQLite(testDBPath, testTableName).CreateLogger();
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
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        public void AuditQueryableSqlite_QueryTriggerData()
        {
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var query = new Dictionary<string, StringBuilder>();
            var result = auditQueryable.QueryTriggerData(query);
            _ = result.ToArray();
            Assert.AreEqual(null, auditQueryable.Query);
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_NoParameters()
        {
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            _ = results.ToList();
            
            Assert.AreEqual(sqlMessage + "ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_ExecutionId_EventLevel()
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
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Debug' AND json_extract(Message, '$.ExecutionID') = '" + executionID.ToString() + "' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_IncorrectLevel()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Wrong".ToStringBuilder()}
            };
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToList();
            
            Assert.AreEqual(sqlMessage + "ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_ExecutionId()
        {
            var executionID = Guid.NewGuid();
            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()}
            };
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE json_extract(Message, '$.ExecutionID') = '" + executionID.ToString() + "' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_Debug()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Debug".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Debug' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_Information()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Information".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);

            var results = auditQueryable.QueryLogData(query);
            _ = results.ToList();

            Assert.AreEqual(sqlMessage + "WHERE Level = 'Information' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_Warning()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Warning".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);

            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();

            Assert.AreEqual(sqlMessage + "WHERE Level = 'Warning' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_Error()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Error".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query).ToList();
            _ = results.ToList();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Error' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_QueryLogData_FilterBy_EventLevel_Fatal()
        {
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Fatal".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToList();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Fatal' ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_AuditQueryableSqlite_QueryLogData_FilterBy_DateTime()
        {
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                {"StartDateTime", StartDateTime.ToString().ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString().ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE (Timestamp >= '" + StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "' AND Timestamp <= '" + CompletedDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "') ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_AuditQueryableSqlite_QueryLogData_DateTime_EventLevel()
        {
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToString().ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString().ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Debug' AND (Timestamp >= '" + StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "' AND Timestamp <= '" + CompletedDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "') ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_AuditQueryableSqlite_QueryLogData_FilterBy_DateTime_EventLevel_executionID()
        {
            var executionID = Guid.NewGuid();
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);
            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToString().ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString().ToStringBuilder()}
            };
            var auditQueryable = GetAuditQueryable("LegacySettingsData",connstring);
            var results = auditQueryable.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Debug' AND json_extract(Message, '$.ExecutionID') = '" + executionID + "' AND (Timestamp >= '" + StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "' AND Timestamp <= '" + CompletedDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "') ORDER BY TimeStamp Desc LIMIT 20", auditQueryable.Query.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableSqlite))]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void AuditQueryableSqlite_AuditQueryableSqlite_QueryLogData_FilterBy_DateTime_On_UrlEncoded_DateTime_EventLevel_and_executionID_Should_Not_Break()
        {
            var executionID = Guid.NewGuid();
            var StartDateTime = "2019%2F10%2F01+01%3A40%3A18";
            var CompletedDateTime = "2019%2F10%2F03+01%3A40%3A18";

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToString().ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString().ToStringBuilder()}
            };

            var audit = GetAuditQueryable("LegacySettingsData",connstring);
            var results = audit.QueryLogData(query);
            _ = results.ToArray();
            
            Assert.AreEqual(sqlMessage + "WHERE Level = 'Debug' AND json_extract(Message, '$.ExecutionID') = '" + executionID + "' AND (Timestamp >= '2019-10-01T01:40:18' AND Timestamp <= '2019-10-03T01:40:18') ORDER BY TimeStamp Desc LIMIT 20", audit.Query.ToString());
        }
    }
}