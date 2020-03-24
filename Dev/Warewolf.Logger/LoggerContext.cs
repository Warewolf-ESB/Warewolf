/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;
using Dev2.Common;
using System.Collections.Generic;
using Warewolf.Common;
using Warewolf.Configuration;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;
using Warewolf.Streams;
using ElasticsearchSource = Dev2.Data.ServiceModel.ElasticsearchSource;

namespace Warewolf.Logger
{
    public class LoggerContext : ILoggerContext
    {
        public IEnumerable<Error> Errors { get; private set; }

        private IArgs _options;
        private ILoggerConfig _loggerConfig;
        private IResourceCatalogProxy _resourceCatalogProxy;
        private ILoggerSource _source;

        public bool Verbose => _options.Verbose;

        public ILoggerSource Source
        {
            get => _source;
            set => _source = value;
        }

        public ILoggerConfig LoggerConfig
        {
            get => _loggerConfig;
            set => _loggerConfig = value;
        }

        public ISourceConnectionFactory LeaderSource { get; set; }
        public IStreamConfig LeaderConfig { get; set; }

        public LoggerContext(IArgs args, IResourceCatalogProxy resourceCatalogProxy)
        {
            _options = args;
            _resourceCatalogProxy = resourceCatalogProxy;

            if (Config.Server.Sink == nameof(AuditingSettingsData))
            {
                _source = new SeriLogElasticsearchSource();
                var id = Config.Auditing.LoggingDataSource.Value;
                var source = _resourceCatalogProxy.GetResourceById<ElasticsearchSource>(GlobalConstants.ServerWorkspaceID, id);
                _loggerConfig = new SeriLogElasticsearchConfig(source);
            }
            else
            {
                _source = new SeriLoggerSource();
                _loggerConfig = new SeriLogSQLiteConfig();
            }
        }
    }
}