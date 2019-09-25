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
using System.Data.SQLite;
using System.Text;
using Warewolf.Driver.Serilog;

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
            var triggerID = GetValue<string>("ResourceId", values);
            var sql = new StringBuilder($"SELECT * FROM {_tableName} WHERE ResourceId = '" + triggerID + "'");
            return GetLogData(null, sql);
        }
        public IEnumerable<dynamic> GetLogData(string executionID, StringBuilder sql)
        {
            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
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
                            var value = results.GetValues("Properties");

                            var serilogData = JsonConvert.DeserializeObject<SeriLogData>(value[0]);
                            var auditJson = JsonConvert.DeserializeObject<Audit>(serilogData.Message);

                            if (string.IsNullOrEmpty(executionID) || executionID == auditJson.ExecutionID)
                                yield return auditJson;
                        }
                    }
                };

            }
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
}
