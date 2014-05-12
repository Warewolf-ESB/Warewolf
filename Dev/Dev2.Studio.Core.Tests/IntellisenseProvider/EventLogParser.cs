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