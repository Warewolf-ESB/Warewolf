/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Auditing
{
    public class AuditCommand
    {
        public string Type { get; set; }
        public Audit Audit { get; set; }
        public Dictionary<string, StringBuilder> Query { get; set; }
        public ExecutionHistory ExecutionHistory { get; set; }
        public LogEntry LogEntry { get; set; }
    }

    public class LogEntry : IAuditEntry
    {
        public LogLevel LogLevel { get; set; }
        public string OutputTemplate { get; set; }
        public object[] Args { get; set; }
        public Exception Exception { get; set; }
        public string AuditType { get; set; }

        public LogEntry(LogLevel logLevel, string outputTemplate, object[] args)
        {
            this.LogLevel = logLevel;
            this.OutputTemplate = outputTemplate;
            this.Args = args;
        }
    }
}
