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
using System.Text;
using Warewolf.Streams;

namespace Warewolf.Logging
{
    public interface ILoggerConsumer : IConsumer
    {
        List<string[]> GetData(string connectionString, string tableName);
        List<string[]> QueryLogData(Dictionary<string, StringBuilder> values);
    }
}