/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Warewolf.Logging.SeriLog;

namespace Warewolf.Common.Framework48
{
    public class SeriLogSQLiteConfig : ISeriLogConfig
    {

        readonly Config _config;

        public SeriLogSQLiteConfig(Config config)
        {
            _config = config;
        } 

        public Logger Logger => CreateLogger();

        private Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo
                .SQLite(sqliteDbPath:_config.SqliteDbPath, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, retentionPeriod: _config.RetentionPeriod, storeTimestampInUtc: _config.StoreTimestampInUtc, formatProvider: _config.FormatProvider)
                .CreateLogger();
        }

        public class Config
        {
            public string SqliteDbPath { get; }
            public string TableName { get; set; } = "Logs"; 
            public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Verbose;
            public IFormatProvider FormatProvider { get; set; } = null;
            public bool StoreTimestampInUtc { get; set; } = false;
            public TimeSpan? RetentionPeriod { get; set; } = null;
        }
    }
}
