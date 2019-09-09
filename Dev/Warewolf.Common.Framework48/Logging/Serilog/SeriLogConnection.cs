/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Serilog.Core;

namespace Warewolf.Logging.SeriLog
{
    internal class SeriLogConnection : ILoggerConnection
    {
        private readonly Logger _logger;

        public SeriLogConnection(ISeriLogConfig loggerConfig)
        {
            _logger = loggerConfig.Logger;
        }

        public ILoggerPublisher NewPublisher()
        { 
            return new SeriLogPublisher(_logger);
        }

        private bool _disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.Dispose();
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