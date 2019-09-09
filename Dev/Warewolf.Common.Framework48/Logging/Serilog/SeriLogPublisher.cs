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
using System.Text;

namespace Warewolf.Logging.SeriLog
{
    internal class SeriLogPublisher : ILoggerPublisher
    {
        private readonly Logger _logger;

        public SeriLogPublisher(Logger logger)
        {
            _logger = logger;
        }

        public void Error(string outputTemplate, params object[] args)
        {
            _logger.Error(outputTemplate, args);
        }

        public void Fatal(string outputTemplate, params object[] args)
        {
            _logger.Fatal(outputTemplate, args);
        }

        public void Info(string outputTemplate, params object[] args)
        {
            _logger.Information(outputTemplate, args);
        }

        public void Publish(byte[] value) => Info(Encoding.UTF8.GetString(value), null);

        public void Warn(string outputTemplate, params object[] args)
        {
            _logger.Warning(outputTemplate, args);
        }
    }
}