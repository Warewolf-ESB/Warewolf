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
    public class SeriLogElasticsearchConfig : ISeriLogConfig
    {
        static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_staticSettings.Url))
                {
                    MinimumLogEventLevel = _staticSettings.RestrictedToMinimumLevel,
                    AutoRegisterTemplate = true,
                    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                })
                .CreateLogger());

        static readonly Settings _staticSettings = new Settings();
        readonly Settings _config;

        public SeriLogElasticsearchConfig()
        {
            _config = new Settings();
        }

        public SeriLogElasticsearchConfig(Settings elasticConfig)
        {
            _config = elasticConfig;
        }

        public ILogger Logger { get => _logger.Value; }

        public string ServerLoggingAddress { set { } get => _config.Url; }

        public string Url { set { } get => _config.Url; }


        public string ConnectionString => throw new NotImplementedException();

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_config.Url))
                {
                    MinimumLogEventLevel = LogEventLevel.Verbose,
                    AutoRegisterTemplate = true,
                    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                })
                .CreateLogger();
        }
        public class Settings
        {
            public Settings()
            {
                Url = @"http://localhost:9200";
                RestrictedToMinimumLevel = LogEventLevel.Verbose;
            }
            public string Url { get; set; }
            public LogEventLevel RestrictedToMinimumLevel { get; set; }
        }
    }
}
