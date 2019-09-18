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
using System;
using Warewolf.Logging;
using Warewolf.Streams;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogConnection : ILoggerConnection
    {
        private readonly ILogger _logger;

        public void StartConsuming(ILoggerConfig config, IConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public ILoggerPublisher NewPublisher()
        { 
            return new SeriLogPublisher(_logger);
        }

        private bool _disposedValue = false;

        public SeriLogConnection(ILoggerConfig loggerConfig )
        {
            var serilogConfig = loggerConfig as ISeriLogConfig;
            _logger = serilogConfig.Logger;
        }

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