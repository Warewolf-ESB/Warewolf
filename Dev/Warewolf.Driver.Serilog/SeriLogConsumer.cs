/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{

    public class SeriLogConsumer : ILoggerConsumer
    {
        public SeriLogConsumer()
        {
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

        public Task<ConsumerResult> Consume(byte[] body)
        {
            throw new System.NotImplementedException();
        }

    }
}