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
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogConnection : ILoggerConnection
    {
        private readonly ILogger _logger;
        private readonly ISeriLogConfig _seriLogConfig;

        public SeriLogConnection(ISeriLogConfig loggerConfig)
        {
            _logger = loggerConfig.Logger;
            _seriLogConfig = loggerConfig;
        }

        public ILoggerPublisher NewPublisher()
        { 
            return new SeriLogPublisher(_logger);
        }

        public ILoggerConsumer NewConsumer()
        {
            return new SeriLogConsumer(_seriLogConfig.ConnectionString);
        }

        private bool _disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Log.CloseAndFlush();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}