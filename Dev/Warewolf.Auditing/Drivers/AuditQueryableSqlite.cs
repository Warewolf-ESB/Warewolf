/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Newtonsoft.Json;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Warewolf.Auditing.Drivers
{
    public class AuditQueryableSqlite : AuditQueryable
    {
        private readonly SeriLogSQLiteSource _source;
        private string _query;

        public AuditQueryableSqlite(string connectionString)
        {
            _source = new SeriLogSQLiteSource
            {
                ConnectionString = connectionString
            };
        }

        public override string Query
        {
            get => _query;
            set => _query = value;
        }

        public AuditQueryableSqlite()
        {
            _source = new SeriLogSQLiteSource();
        }

        public override IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);
            var sql = new StringBuilder($"SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ");

            if (resourceId != null)
            {
                sql.Append("WHERE json_extract(Message, '$.ResourceId') = '" + resourceId + "' ");
                sql.Append("ORDER BY TimeStamp Desc LIMIT 20");
                var results = ExecuteDatabase();
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

        public override IEnumerable<IAudit> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionId = GetValue<string>("ExecutionID", values);
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

            BuildQuery(executionId, startTime, endTime, eventLevel);
            var results = ExecuteDatabase();
            if (results.Length > 0)
            {
                foreach (var result in results)
                {
                    var audit = JsonConvert.DeserializeObject<Audit>(result);
                    if (audit != null)
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

        private void BuildQuery(string executionId, string startTime, string endTime, string eventLevel)
        {
            var sb = new StringBuilder($"SELECT * FROM (SELECT json_extract(Properties, '$.Message') AS Message, Level, TimeStamp FROM Logs) ");

            if (eventLevel != null)
            {
                switch (eventLevel)
                {
                    case "Debug":
                        sb.Append("WHERE Level = 'Debug' ");
                        break;
                    case "Information":
                        sb.Append("WHERE Level = 'Information' ");
                        break;
                    case "Warning":
                        sb.Append("WHERE Level = 'Warning' ");
                        break;
                    case "Error":
                        sb.Append("WHERE Level = 'Error' ");
                        break;
                    case "Fatal":
                        sb.Append("WHERE Level = 'Fatal' ");
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(executionId))
            {
                sb.Append("AND json_extract(Message, '$.ExecutionID') = '" + executionId + "' ");
            }

            if (string.IsNullOrEmpty(eventLevel) && !string.IsNullOrEmpty(executionId))
            {
                sb.Append("WHERE json_extract(Message, '$.ExecutionID') = '" + executionId + "' ");
            }

            if ((!string.IsNullOrEmpty(eventLevel) || !string.IsNullOrEmpty(executionId)) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sb.Append("AND (Timestamp >= '" + startTime + "' ");
                sb.Append("AND Timestamp <= '" + endTime + "') ");
            }

            if ((string.IsNullOrEmpty(eventLevel) && string.IsNullOrEmpty(executionId)) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                sb.Append("WHERE (Timestamp >= '" + startTime + "' ");
                sb.Append("AND Timestamp <= '" + endTime + "') ");
            }

            sb.Append("ORDER BY TimeStamp Desc LIMIT 20");
            _query = sb.ToString();
        }

        private String[] ExecuteDatabase()
        {
            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _source.ConnectionString + ";"))
            {
                using (var command = new SQLiteCommand(_query.ToString(), sqlConn))
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
                }
            }

            return new string[0];
        }
    }
}