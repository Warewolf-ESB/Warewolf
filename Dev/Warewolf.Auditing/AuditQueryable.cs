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
using System.Net;
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

        private bool IsUrlEncoded(string text)
        {
            return (WebUtility.UrlDecode(text)) != text;
        }

        private string GetDecoded(string encodedText)
        {
            return WebUtility.UrlDecode(encodedText);
        }

        public IEnumerable<Audit> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionID = GetValue<string>("ExecutionID", values);

            var dtFormat = "yyyy-MM-ddTHH:mm:ss";

            if (!string.IsNullOrEmpty(startTime) & !string.IsNullOrEmpty(endTime))
            {
                if (IsUrlEncoded(startTime) & IsUrlEncoded(endTime))
                {
                    var decodedStartTime = Convert.ToDateTime(GetDecoded(startTime));
                    var decodedEndTime = Convert.ToDateTime(GetDecoded(endTime));

                    startTime = decodedStartTime.ToString(dtFormat);
                    endTime = decodedEndTime.ToString(dtFormat);
                }
                else 
                {
                    startTime = Convert.ToDateTime(startTime).ToString(dtFormat);
                    endTime = Convert.ToDateTime(endTime).ToString(dtFormat);

                }
            }

            var sql = BuildSQLWebUIFilterString(executionID.Replace("''",""), startTime, endTime, eventLevel);

            var results = ExecuteDatabase(_connectionString, sql);
            if (results.Length > 0)
            {
                foreach (var result in results)
                {
                    var audit = JsonConvert.DeserializeObject<Audit>(result);
                    if (audit.ExecutionID != null)
                    {
                        yield return audit;
                    }
                }
            }
            else
            {
                yield return null;
            }

        }
        public IEnumerable<ExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);
            var sql = new StringBuilder($"SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ");

            if (resourceId != null)
            {
                sql.Append("WHERE json_extract(Message, '$.ResourceId') = '" + resourceId + "' ");
                sql.Append("ORDER BY TimeStamp Desc LIMIT 20");
                var results = ExecuteDatabase(_connectionString, sql);
                if (results.Length > 0)
                {
                    foreach (var result in results)
                    {
                        var executionHistory = JsonConvert.DeserializeObject<ExecutionHistory>(result);
                        yield return executionHistory;
                    }
                }
            }
            else
            {
                yield return null;
            }
        }

        protected abstract String[] ExecuteDatabase(string connectionString, StringBuilder sql);
        private StringBuilder BuildSQLWebUIFilterString(string executionID, string startTime, string endTime, string eventLevel)
        {
            var sql = new StringBuilder($"SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ");

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
                sql.Append("AND json_extract(Message, '$.ExecutionID') = '" + executionID + "' ");
            }
            if (string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(executionID))
            {
                sql.Append("WHERE json_extract(Message, '$.ExecutionID') = '" + executionID.Replace("'","") + "' ");
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
            sql.Append("ORDER BY TimeStamp Desc LIMIT 100");
            
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
                        var ret = new List<string>();
                        while (reader.Read())
                        {
                            var results = reader.GetValues();
                            ret.Add(results.GetValues("Message")[0]); // TODO: should not just hope that the Message key exists
                        }
                        return ret.ToArray();
                    }
                };
            }
            return new string[0];
        }
    }
}
