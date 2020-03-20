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
using System;
using System.Collections.Generic;
using Warewolf.Configuration;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class LoggerContext : ILoggerContext
    {
        public IEnumerable<Error> Errors { get; private set; }
        
        private IArgs _options;

        public bool Verbose { get => _options.Verbose; }
        public ILoggerSource Source => new SeriLoggerSource();

        public ILoggerConfig LoggerConfig { get; set; }

        public LoggerContext(IArgs args)
        {
            _options = args;
            if (Config.Server.Sink == nameof(AuditingSettingsData))
            {
                //TODO: we will LoggingDataSource depending on that open the correct sink for now we only have elastic so we will default
                LoggerConfig =  new SeriLogElasticsearchConfig();
            }
            else
            {
                LoggerConfig =  new SeriLogSQLiteConfig();
            }
        }
    }
}
