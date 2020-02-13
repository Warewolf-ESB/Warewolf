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
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogELasticConfig : ISeriLogConfig
    {
        static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    MinimumLogEventLevel = LogEventLevel.Verbose,
                    AutoRegisterTemplate = true,
                    CustomFormatter = new ElasticsearchJsonFormatter()
                })
                .CreateLogger());

        static readonly Settings _staticSettings = new Settings();

        readonly Settings _config;

        public SeriLogELasticConfig()
        {
            _config = new Settings();
        }

        public SeriLogELasticConfig(Settings elasticConfig)
        {
            _config = elasticConfig;
        }

        public ILogger Logger { get => _logger.Value; }
        //TODO: this path needs to come the Config.Server.AuditPath which is still tobe moved to project Framework48
        public string ConnectionString { get => _config.ConnectionString; }  

        public string ServerLoggingAddress { get; set; }

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    MinimumLogEventLevel = LogEventLevel.Verbose,
                    AutoRegisterTemplate = true,
                    CustomFormatter = new ElasticsearchJsonFormatter()
                })
                .CreateLogger();
        }
        public class Settings
        {
            public Settings()
            {
            }
            public string ConnectionString { get; set; }
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
