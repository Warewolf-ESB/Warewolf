/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;
using Newtonsoft.Json;
using System.Linq;
using Serilog;

namespace Warewolf.Driver.Serilog
{

    public class SeriLogConsumer : ILoggerConsumer
    {
        private ILogger _logger;

        public SeriLogConsumer(ISeriLogConfig loggerConfig)
        {
            _logger = loggerConfig.Logger;
        }

        public Task<ConsumerResult> Consume(byte[] body)
        {
            var message = Encoding.Default.GetString(body);
            _logger.Debug(message);
            return Task.FromResult(ConsumerResult.Success);
        }

    }
}