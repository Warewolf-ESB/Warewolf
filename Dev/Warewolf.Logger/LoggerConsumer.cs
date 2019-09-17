/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class LoggerConsumer : ILoggerConsumer
    {
        public LoggerConsumer()
        {

        }
        public Task<ConsumerResult> Consume(byte[] body) => throw new System.NotImplementedException();
        public List<string[]> GetData(string connectionString, string tableName) => throw new System.NotImplementedException();
    }
}