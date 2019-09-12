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

namespace Warewolf.Driver.Serilog
{

    public class SeriLogConsumer : ILoggerConsumer
    {
        string _connectionString;

        public SeriLogConsumer()
        {
            _connectionString = Config.Server.AuditFilePath;
        }
        public SeriLogConsumer(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<string[]> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var properties = new List<string[]>();

            var executionID = GetValue<string>("ExecutionID", values);
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
           
            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
            {
                var sql = new StringBuilder("SELECT * FROM Logs  ");
                if (executionID.Length > 0)
                {
                    sql.Append("@").Append(" WHERE ExecutionID = '" + executionID + "'");
                }
                if (executionID.Length > 0 && startTime.Length > 0 && endTime.Length > 0)
                {
                    sql.Append("@").Append(" AND (Timestamp >= '" + startTime + "' AND ");
                    sql.Append("@").Append(" Timestamp <= '" + endTime + "') ");
                }
                if (executionID.Length <= 0 && startTime.Length > 0 && endTime.Length > 0)
                {
                    sql.Append("@").Append(" WHERE (Timestamp >= '" + startTime + "' AND ");
                    sql.Append("@").Append(" Timestamp <= '" + endTime + "') ");
                }
                var command = new SQLiteCommand(sql.ToString(), sqlConn);
                sqlConn.Open();
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var results = reader.GetValues();
                        properties.Add(results.GetValues("Properties"));
                    }
                }
            }
            return properties;
        }

        public List<string[]> GetData(string connectionString, string tableName)
        {
            var properties = new List<string[]>();

            using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + connectionString + ";"))
            {
                var command = new SQLiteCommand("SELECT * FROM " + tableName, sqlConn);
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