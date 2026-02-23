/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Serilog;
using Serilog.Events;
using System;
using Dev2.Common;
using Warewolf.Logging;
using System.IO;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogSQLiteConfig : ISeriLogConfig
    {
        static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
        {
            try
            {
                return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo
                    .SQLite(
                        sqliteDbPath: _staticSettings.ConnectionString,
                        tableName: _staticSettings.TableName,
                        restrictedToMinimumLevel: _staticSettings.RestrictedToMinimumLevel,
                        formatProvider: _staticSettings.FormatProvider,
                        storeTimestampInUtc: _staticSettings.StoreTimestampInUtc,
                        retentionPeriod: _staticSettings.RetentionPeriod)
                    .CreateLogger();
            }
            catch (Exception e) when (e is DllNotFoundException || e is BadImageFormatException)
            {
                // SQLite.Interop.dll is unavailable or wrong architecture (e.g. Windows Nano Server
                // has no MSVCRT/Win32 subsystem). Fall back to a rolling flat-file sink so the
                // logger still works and the Fleck WebSocket handshake can complete normally.
                return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(
                        path: System.IO.Path.Combine(_staticSettings.Path, "AuditLog-.txt"),
                        restrictedToMinimumLevel: _staticSettings.RestrictedToMinimumLevel,
                        rollingInterval: RollingInterval.Day,
                        formatProvider: _staticSettings.FormatProvider)
                    .CreateLogger();
            }
        });

        static readonly Settings _staticSettings = new Settings();

        readonly Settings _config;

        public SeriLogSQLiteConfig()
        {
            _config = new Settings();
        }

        public SeriLogSQLiteConfig(Settings sqlConfig)
        {
            _config = sqlConfig;
        }

        public ILogger Logger => _logger.Value;

        public string ConnectionString => _config.ConnectionString;

        public string Endpoint { get; set; } = Config.Legacy.Endpoint;

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .SQLite(sqliteDbPath: ConnectionString, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, formatProvider: _config.FormatProvider, storeTimestampInUtc: _config.StoreTimestampInUtc, retentionPeriod: _config.RetentionPeriod)
                .CreateLogger();
        }

        public class Settings
        {
            public Settings()
            {
                Path = Config.Legacy.AuditFilePath;
                Database = "AuditDB.db";
                TableName = "Logs";
                RestrictedToMinimumLevel = LogEventLevel.Verbose;
                FormatProvider = null;
                StoreTimestampInUtc = false; // TODO: this should default to true...
            }
            public string ConnectionString => System.IO.Path.Combine(Path, Database);
            public string Database { get; set; }
            public string Path { get; set; }
            public string TableName { get; set; }
            public LogEventLevel RestrictedToMinimumLevel { get; set; }
            public IFormatProvider FormatProvider { get; set; }
            public bool StoreTimestampInUtc { get; set; }
            public TimeSpan? RetentionPeriod { get; set; }
        }
    }

    public interface ISeriLogSQLiteSource : ILoggerSource
    {
        string ConnectionString { get; set; }
        string TableName { get; set; }
    }

    public class SeriLogSQLiteSource: ILoggerSource
    {
        public string ConnectionString { get; set; } = Path.Combine(Config.Legacy.AuditFilePath, "AuditDB.db");
        public string TableName { get; set; } = "Logs";
        public ILoggerConnection NewConnection(ILoggerConfig loggerConfig)
        {
            return new SeriLogConnection(loggerConfig);
        }
    }
}