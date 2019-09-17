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

        Settings _config;

        public SeriLogSQLiteConfig()
        {
            _config = new Settings();
            Logger = CreateLogger();
        }

        public SeriLogSQLiteConfig(Settings config)
        {
            _config = config;
            Logger = CreateLogger();
        } 

        public ILogger Logger { get; private set; }
        public string ServerLoggingAddress { get; set; }

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo
                .SQLite(sqliteDbPath:_config.Path, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, retentionPeriod: _config.RetentionPeriod, storeTimestampInUtc: _config.StoreTimestampInUtc, formatProvider: _config.FormatProvider)
                .CreateLogger();
        }

        //TODO: This is the options that should be controlled by the Studio using IOptions
        public class Settings
        {
            //TODO: _config.Server.AuditFilePath - where is this suppose to come from????
            public string Path { get; set; } = System.IO.Path.Combine("", "auditDB.db");
            public string TableName { get; set; } = "Logs"; 
            public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Verbose;
            public IFormatProvider FormatProvider { get; set; } = null;
            public bool StoreTimestampInUtc { get; set; } = false;
            public TimeSpan? RetentionPeriod { get; set; } = null;
        }
    }
}
