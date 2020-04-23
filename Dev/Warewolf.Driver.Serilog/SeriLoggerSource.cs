/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Configuration;
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLoggerSource : ILoggerSource
    {
        public string HostName { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Port { get; set; }
        public AuthenticationType AuthenticationType { get; set; }

        public ILoggerConnection NewConnection(ILoggerConfig loggerConfig)
        {
            return new SeriLogConnection(loggerConfig);
        }
    }
}
