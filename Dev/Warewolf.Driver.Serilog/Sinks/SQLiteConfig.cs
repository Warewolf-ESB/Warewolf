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
using Serilog.Events;
using System;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogSQLiteConfig : ISeriLogConfig
    {

        readonly Settings _config;

        public SeriLogSQLiteConfig()
        {
            _config = new Settings();
        }

        public SeriLogSQLiteConfig(Settings sqlConfig)
        {
            _config = sqlConfig;
        }

        public ILogger Logger { get => CreateLogger(); }
        //TODO: this path needs to come the Config.Server.AuditPath which is still tobe moved to project Framework48
        public string ConnectionString { get => _config.ConnectionString; }  

        public string ServerLoggingAddress { get; set; }

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo
                .SQLite(sqliteDbPath: ConnectionString, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, formatProvider: _config.FormatProvider, storeTimestampInUtc: _config.StoreTimestampInUtc, retentionPeriod: _config.RetentionPeriod)
                .CreateLogger();
        }

        //TODO: This is the options that should be controlled by the Studio using IOptions
        public class Settings
        {
            public Settings()
            {
                Path = @"C:\ProgramData\Warewolf\Audits\"; //TODO: Config.Server.AuditFilePath;
                Database = "AuditDB.db";
                TableName = "Logs";
                RestrictedToMinimumLevel = LogEventLevel.Verbose;
                FormatProvider = null;
                StoreTimestampInUtc = false;
            }
            public string ConnectionString
            {
                get
                {
                    return System.IO.Path.Combine(Path, Database);
                }
            }
            public string Database { get; set; }
            public string Path { get; set; }
            public string TableName { get; set; }
            public LogEventLevel RestrictedToMinimumLevel { get; set; }
            public IFormatProvider FormatProvider { get; set; }
            public bool StoreTimestampInUtc { get; set; }
            public TimeSpan? RetentionPeriod { get; set; }
        }
    }
}
