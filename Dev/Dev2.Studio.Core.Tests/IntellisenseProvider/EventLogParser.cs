
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Parsing;

namespace Dev2.Core.Tests.IntellisenseProvider
{
    public class EventLogParser : ParseEventLog
    {
        #region Overrides of ParseEventLog

        readonly ParseEventLogEntry[] _parseEventLogEntries;

        public EventLogParser()
        {
            _parseEventLogEntries = new ParseEventLogEntry[] { };
        }

        public EventLogParser(ParseEventLogEntry[] parseEventLogEntries)
        {
            _parseEventLogEntries = parseEventLogEntries;
        }

        public override ParseEventLogEntry[] GetEventLogs()
        {
            return _parseEventLogEntries;
        }

        public override void Log(ParseEventLogEntry entry)
        {
        }

        public override void Clear()
        {
        }

        public override bool HasEventLogs
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
