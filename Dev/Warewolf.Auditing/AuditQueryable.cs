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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Warewolf.Driver.Serilog;

namespace Warewolf.Auditing
{
    public abstract class AuditQueryable
    {
        private string _connectionString;
        private string _tableName;

        protected AuditQueryable()
        {
        }

        protected AuditQueryable(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public IEnumerable<Audit> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionID = GetValue<string>("ExecutionID", values);

            var sql = BuildSQLWebUIFilterString(executionID, startTime, endTime, eventLevel);

            var results = ExecuteDatabase(_connectionString, sql);
            if (results.Length > 0)
            {
                var serilogData = JsonConvert.DeserializeObject(results[0]) as JObject;
                var audit = serilogData.Property("Message").Value.ToObject<Audit>();
                yield return audit;
            }
            else
            {
                yield return null;
            }

        }
        public IEnumerable<ExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);

            var sql = new StringBuilder($"SELECT * from Logs ");
            if (resourceId != null)
            {
                sql.Append("WHERE json_extract(Properties, '$.ResourceId') = '" + resourceId + "' ");

                var results = ExecuteDatabase(_connectionString, sql);
                if (results.Length > 0)
                {
                    var serilogData = JsonConvert.DeserializeObject(results[0]) as JObject;
                    var executionHistory = serilogData.Property("Message").Value.ToObject<ExecutionHistory>();

                    yield return executionHistory;
                }
            }
            yield return null;
        }

        protected abstract String[] ExecuteDatabase(string connectionString, StringBuilder sql);
        private StringBuilder BuildSQLWebUIFilterString(string executionID, string startTime, string endTime, string eventLevel)
        {
            var sql = new StringBuilder($"SELECT * FROM {_tableName} ");

            if (eventLevel != null)
            {
                switch (eventLevel)
                {
                    case "Debug":
                        sql.Append("WHERE Level = 'Debug' ");
                        break;
                    case "Information":
                        sql.Append("WHERE Level = 'Information' ");
                        break;
                    case "Warning":
                        sql.Append("WHERE Level = 'Warning' ");
                        break;
                    case "Error":
                        sql.Append("WHERE Level = 'Error' ");
                        break;
                    case "Fatal":
                        sql.Append("WHERE Level = 'Fatal' ");
                        break;
                    default:
                        break;
                }
            }
            if (!string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(executionID))
            {
                sql.Append("AND json_extract(Properties, '$.ExecutionID') = '" + executionID + "' ");

            }
            if (string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(executionID))
            {
                sql.Append("WHERE json_extract(Properties, '$.ExecutionID') = '" + executionID + "' ");
            }
            if ((!string.IsNullOrEmpty(eventLevel) || !string.IsNullOrEmpty(executionID)) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sql.Append("AND (Timestamp >= '" + startTime + "' ");
                sql.Append("AND Timestamp <= '" + endTime + "') ");
            }
            if ((string.IsNullOrEmpty(eventLevel) && string.IsNullOrEmpty(executionID)) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sql.Append("WHERE (Timestamp >= '" + startTime + "' ");
                sql.Append("AND Timestamp <= '" + endTime + "') ");
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
    public class AuditQueryableSqlite : AuditQueryable
    {
        public AuditQueryableSqlite(string connectionString, string tableName) : base(connectionString, tableName)
        {
        }

        protected override String[] ExecuteDatabase(string connectionString, StringBuilder sql)
        {
            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + connectionString + ";"))
            {
                using (var command = new SQLiteCommand(sql.ToString(), sqlConn))
                {
                    sqlConn.Open();
                    sqlConn.EnableExtensions(true);
                    sqlConn.LoadExtension("SQLite.Interop.dll", "sqlite3_json_init");
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
    }
}
