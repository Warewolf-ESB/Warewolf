/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;
using Dev2.Common;
using System.IO;
using Warewolf.Logger;
using Newtonsoft.Json;
using System.Linq;

namespace Warewolf.Driver.Serilog
{

    public class SeriLogConsumer : ILoggerConsumer
    {
        readonly string _connectionString;

        public SeriLogConsumer()
        {
            _connectionString = Path.Combine(Config.Server.AuditFilePath, "auditDB.db");
        }
        public SeriLogConsumer(string connectionString, string database)
        {
            _connectionString = Path.Combine(connectionString, database);
        }
        public IEnumerable<dynamic> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var tableName = "Logs"; //TODO: should be dynamic?
            var audits = new List<Audit>();

            var executionID = GetValue<string>("ExecutionID", values);
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);

            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
            {
                var sql = new StringBuilder($"SELECT * FROM {tableName}  ");
                //if (executionID.Length > 0)
                //{
                //    sql.Append(" WHERE ExecutionID = '" + executionID + "'");
                //}
                //if (executionID.Length > 0 && startTime.Length > 0 && endTime.Length > 0)
                //{
                //    sql.Append(" AND (Timestamp >= '" + startTime + "' AND ");
                //    sql.Append(" Timestamp <= '" + endTime + "') ");
                //}
                //if (executionID.Length <= 0 && startTime.Length > 0 && endTime.Length > 0)
                //{
                    sql.Append(" WHERE Timestamp >= '" + startTime + "' AND ");
                    sql.Append(" Timestamp <= '" + endTime + "' ");
                //}

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
                            
                            audits.Add(auditJson);
                        }
                    }
                };

            }
            return audits;
        }

        public List<string[]> GetData(string connectionString, string tableName)
        {
            var properties = new List<string[]>();

            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + connectionString + ";"))
            {
                using (var command = new SQLiteCommand("SELECT * FROM " + tableName, sqlConn))
                {
                    sqlConn.Open();

                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var values = reader.GetValues();
                            properties.Add(values.GetValues("Properties"));
                        }
                    }
                }
            }

            return properties;
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

        public Task<ConsumerResult> Consume(byte[] body)
        {
            throw new System.NotImplementedException();
        }

    }
}