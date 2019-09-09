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
    internal class SeriLogPublisher : ILoggerPublisher
    {
        private readonly Logger _logger;

        public SeriLogPublisher(Logger logger)
        {
            _logger = logger;
        }

        public void Error(string outputTemplate)
        {
            _logger.Error(outputTemplate);
        }

        public void Fatal(string outputTemplate)
        {
            _logger.Fatal(outputTemplate);
        }

        public void Info(string outputTemplate)
        {
            _logger.Information(outputTemplate);
        }

        public void Publish(byte[] value) => throw new System.NotImplementedException();

        public void Warn(string outputTemplate)
        {
            _logger.Warning(outputTemplate);
        }
    }
}