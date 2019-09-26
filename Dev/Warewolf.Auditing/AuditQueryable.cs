/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.Text;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Auditing
{
    public class AuditQueryable
    {
        private string _connectionString;
        private string _tableName;

        public AuditQueryable()
        {
        }

        public AuditQueryable(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public IEnumerable<dynamic> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionID = GetValue<string>("ExecutionID", values);

            var sql = BuildSQLWebUIFilterString(startTime, endTime, eventLevel);

            return GetLogData(executionID, sql);

        }
        public IEnumerable<dynamic> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);
            return GetQueueLogData(resourceId);
        }
        public IEnumerable<dynamic> GetQueueLogData(string resourceId)
        {
            GetAuditLogs logs = new GetAuditLogs();
            return logs.Queues(_connectionString, resourceId);
        }
        public IEnumerable<dynamic> GetLogData(string executionID, StringBuilder sql)
        {
            GetAuditLogs logs = new GetAuditLogs();
            return logs.Logs(_connectionString, executionID, sql);
        }

        private StringBuilder BuildSQLWebUIFilterString(string startTime, string endTime, string eventLevel)
        {
            var sql = new StringBuilder($"SELECT * FROM {_tableName} ");

            if (eventLevel != null)
            {
                switch (eventLevel)
                {
                    case "Information":
                        sql.Append("WHERE Level = 'Information'");
                        break;
                    case "Warning":
                        sql.Append("WHERE Level = 'Warning'");
                        break;
                    case "Error":
                        sql.Append("WHERE Level = 'Error'");
                        break;
                    case "Fatal":
                        sql.Append("WHERE Level = 'Fatal'");
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sql.Append(" AND (Timestamp >= '" + startTime + "' AND ");
                sql.Append(" Timestamp <= '" + endTime + "') ");
            }
            if (string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sql.Append(" WHERE Timestamp >= '" + startTime + "' AND ");
                sql.Append(" Timestamp <= '" + endTime + "' ");
            }

            return sql;
        }

        T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T)Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }
    }
    class GetAuditLogs : AuditQueryableSqlite
    {
        String[] ExecuteDatabase(string connectionString, StringBuilder sql)
        {
            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + connectionString + ";"))
            {
                using (var command = new SQLiteCommand(sql.ToString(), sqlConn))
                {
                    sqlConn.Open();
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var results = reader.GetValues();
                            return results.GetValues("Properties");
                        }
                    }
                };
            }
            return new string[0];
        }
        public override IEnumerable<dynamic> Logs(string connectionString, string executionID, StringBuilder sql)
        {
            var results = ExecuteDatabase(connectionString, sql);
            var serilogData = JsonConvert.DeserializeObject<SeriLogData>(results[0]);
            var auditJson = JsonConvert.DeserializeObject<Audit>(serilogData.Message);

            if (string.IsNullOrEmpty(executionID) || executionID == auditJson.ExecutionID)
                yield return auditJson;
        }
        public override IEnumerable<dynamic> Queues(string connectionString, string resourceId)
        {
            //TODO: This sql query still needs to change. Waiting for valid data to save to the DB
            var sql = new StringBuilder($"SELECT Logs.* from Logs, json_each(Logs.RenderedMessage)");
            sql.Append("WHERE json_extract(value, '$.ResourceId') = '" + resourceId + "'");
            var value = ExecuteDatabase(connectionString, sql);

            var serilogData = JsonConvert.DeserializeObject<SeriLogData>(value[0]);
            var historyJson = JsonConvert.DeserializeObject<ExecutionHistory>(serilogData.Message);

            if (string.IsNullOrEmpty(resourceId) || resourceId == historyJson.ResourceId.ToString())
                yield return historyJson;
        }
    }
    abstract class AuditQueryableSqlite
    {
        public abstract IEnumerable<dynamic> Queues(string connectionString, string resourceId);

        public abstract IEnumerable<dynamic> Logs(string connectionString, string executionID, StringBuilder sql);
    }
}
