/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Warewolf.Data;
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogElasticsearchConfig : ISeriLogConfig
    {
        static readonly Settings _staticSettings = new Settings();
        readonly Settings _config;

        static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_staticSettings.Url))
            {
                MinimumLogEventLevel = LogEventLevel.Verbose,
                AutoRegisterTemplate = true,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
            })
            .CreateLogger());

        public SeriLogElasticsearchConfig(ElasticsearchSource source)
        {
             _config = new Settings(source);
        }


        public ILogger Logger
        {
            get => _logger.Value;
        }

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

        public string Endpoint { get; set; } = Config.Auditing.Endpoint;
        
        private class Settings
        {
            public Settings(ElasticsearchSource source)
            {
                Url = source.HostName + ":" + source.Port;
            }

            public Settings()
            {
               
            }
            public string Url { get; set; }
        }
    }

    public class SeriLogElasticsearchSource : ILoggerSource
    {
        private ILoggerSource _loggerSourceImplementation;
        public NamedGuid LoggingDataSource { get; set; }

        public SeriLogElasticsearchSource()
        {
            LoggingDataSource = Config.Auditing.LoggingDataSource;
        }
        public ILoggerConnection NewConnection(ILoggerConfig loggerConfig)
        {
            return new SeriLogConnection(loggerConfig);
        }
    }
}