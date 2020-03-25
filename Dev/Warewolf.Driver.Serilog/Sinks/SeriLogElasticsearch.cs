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
using Dev2.Runtime.ServiceModel.Data;
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
        static readonly Settings _staticSettings = new Settings(new SerilogElasticsearchSource());
        readonly Settings _config;

        static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_staticSettings.Url))
            {
                MinimumLogEventLevel = LogEventLevel.Verbose,
                IndexDecider = (e, o) => "warewolfaudits",
                AutoRegisterTemplate = true,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
            })
            .CreateLogger());

        public SeriLogElasticsearchConfig(ILoggerSource source)
        {
            _config = source == null ? new Settings(new SerilogElasticsearchSource()) : new Settings(source as SerilogElasticsearchSource);
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
                    IndexDecider = (e, o) => "warewolfaudits",
                    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                })
                .CreateLogger();
        }

        public string Endpoint { get; set; } = Config.Auditing.Endpoint;

        private class Settings
        {
            public Settings(SerilogElasticsearchSource source)
            {
                Url = source.HostName + ":" + source.Port;
                Password = source.Password;
                Username = source.Username;
                AuthenticationType = source.AuthenticationType;
            }

            public string Password { get; }
            public string Username { get; }
            public AuthenticationType AuthenticationType { get; }
            public string Url { get; }
        }
    }

    public class SerilogElasticsearchSource : ILoggerSource
    {
        private Guid _resourceId;
        private string _resourceName;
        private string _hostName;
        private string _port;
        private string _password;
        private string _username;
        private AuthenticationType _authenticationType;

        public SerilogElasticsearchSource()
        {
            var src = new ElasticsearchSource();
            _resourceId = src.ResourceID;
            _resourceName = src.ResourceName;
            _hostName = src.HostName;
            _port = src.Port;
            _password = src.Password;
            _username = src.Username;
            _authenticationType = src.AuthenticationType;
        }

        public Guid ResourceID
        {
            get => _resourceId;
            set => _resourceId = value;
        }

        public string ResourceName
        {
            get => _resourceName;
            set => _resourceName = value;
        }

        public string HostName
        {
            get => _hostName;
            set => _hostName = value;
        }

        public string Port
        {
            get => _port;
            set => _port = value;
        }

        public AuthenticationType AuthenticationType
        {
            get => _authenticationType;
            set => _authenticationType = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public ILoggerConnection NewConnection(ILoggerConfig loggerConfig)
        {
            return new SeriLogConnection(loggerConfig);
        }
    }
}