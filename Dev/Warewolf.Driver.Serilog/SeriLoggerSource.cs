﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLoggerSource : ILoggerSource
    {
        public SeriLoggerSource()
        {
        }

        //TODO: this path needs to come the Config.Server.AuditPath which is still tobe moved to project Framework48
        public string ConnectionString { get; set; } = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
        public string TableName { get; set; } = "Logs";

        public ILoggerConnection NewConnection(ILoggerConfig loggerConfig)
        {
            return new SeriLogConnection(loggerConfig);
        }
    }
}
