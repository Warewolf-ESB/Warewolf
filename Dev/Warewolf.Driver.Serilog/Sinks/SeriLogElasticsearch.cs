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
using Serilog.Sinks.Elasticsearch;
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogElasticsearchConfig : ISeriLogConfig
    {
        static readonly Settings _staticSettings = new Settings();
        readonly Settings _config;
        static ILogger _logger;

        public SeriLogElasticsearchConfig()
        {
        }

        public SeriLogElasticsearchConfig(ILoggerSource source)
        {
            if (source != null)
            {
                _config = new Settings(source as SerilogElasticsearchSource);
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Sink(new ElasticsearchSink(new ElasticsearchSinkOptions(new Uri(_config.Url))
                    {
                        AutoRegisterTemplate = true,
                        IndexDecider = (e, o) => _config.SearchIndex,
                    }))
                    .CreateLogger();
            }
            else
            {
                _logger = new LoggerConfiguration().CreateLogger();
            }
        }

        public ILogger Logger
        {
            get => _logger;
        }

        public string Endpoint { get; set; } = Config.Auditing.Endpoint;

        private class Settings
        {
            public Settings()
            {
            }

            public Settings(SerilogElasticsearchSource source)
            {
                Url = source.HostName + ":" + source.Port;
                Password = source.Password;
                Username = source.Username;
                AuthenticationType = source.AuthenticationType;
                SearchIndex = source.SearchIndex;
            }

            public string SearchIndex { get; set; }

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
        private string _searchIndex { get; set; }

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
            _searchIndex = src.SearchIndex;
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

        public string SearchIndex
        {
            get => _searchIndex;
            set => _searchIndex = value;
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