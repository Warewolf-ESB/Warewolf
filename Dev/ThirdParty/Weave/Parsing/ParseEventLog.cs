
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Parsing.Tokenization;

namespace System.Parsing
{
    public abstract class ParseEventLog
    {
        public static readonly ParseEventLog NullEventLog = new __NoOpParseEventLog();

        public abstract bool HasEventLogs { get; }

        public abstract ParseEventLogEntry[] GetEventLogs();

        public abstract void Log(ParseEventLogEntry entry);

        public abstract void Clear();
        
    }

    public class TestParseEventLog : ParseEventLog
    {
        private List<ParseEventLogEntry> _entries;
        private bool _hasEntries;

        public override bool HasEventLogs { get { return _hasEntries; } }

        public TestParseEventLog()
        {
            _entries = new List<ParseEventLogEntry>();
        }

        public override ParseEventLogEntry[] GetEventLogs()
        {
            return _entries.ToArray();
        }

        public override void Log(ParseEventLogEntry entry)
        {
            _entries.Add(entry);
            _hasEntries = true;
        }

        public override void Clear()
        {
            _entries.Clear();
            _hasEntries = false;
        }
    }

    internal sealed class __NoOpParseEventLog : ParseEventLog
    {
        public override bool HasEventLogs { get { return false; } }

        public override ParseEventLogEntry[] GetEventLogs()
        {
            return new ParseEventLogEntry[0];
        }

        public override void Log(ParseEventLogEntry entry)
        {
        }

        public override void Clear()
        {
            
        }
    }

    public sealed class ParseEventLogEntry
    {
        private string _source;
        private ParseEventLogToken _errorStart;
        private ParseEventLogToken _errorEnd;
        private TokenDefinition _arg1;
        private TokenDefinition _arg2;

        private int _eventType;
        private int _eventData;
        private string _module;
        private string _component;

        public string Source { get { return _source; } }
        public ParseEventLogToken ErrorStart { get { return _errorStart; } }
        public ParseEventLogToken ErrorEnd { get { return _errorEnd; } }
        public TokenDefinition Arg1 { get { return _arg1; } }
        public TokenDefinition Arg2 { get { return _arg2; } }
        public int EventType { get { return _eventType; } }
        public int EventData { get { return _eventData; } }
        public string Module { get { return _module; } }
        public string Component { get { return _component; } }

        public ParseEventLogEntry(string source, ParseEventLogToken errorStart, ParseEventLogToken errorEnd, TokenDefinition arg1, TokenDefinition arg2, int eventType, int eventData, string module, string component)
        {
            _source = source;
            _errorStart = errorStart;
            _errorEnd = errorEnd;
            _arg1 = arg1;
            _arg2 = arg2;
            _eventType = eventType;
            _eventData = eventData;
            _module = module;
            _component = component;
        }

        public ParseEventLogEntry(string source, ParseEventLogToken errorLocation, TokenDefinition arg1, TokenDefinition arg2, int eventType, int eventData, string module, string component)
            : this(source, errorLocation, errorLocation, arg1, arg2, eventType, eventData, module, component)
        {
        }

        public ParseEventLogEntry(string source, ParseEventLogToken errorLocation, TokenDefinition arg1, int eventType, int eventData, string module, string component)
            : this(source, errorLocation, errorLocation, arg1, null, eventType, eventData, module, component)
        {
        }
    }

    public sealed class ParseEventLogToken
    {
        /// <summary>
        /// The index at which this token can be located in the array of tokens returned by the tokenization request.
        /// </summary>
        public int TokenIndex;
        /// <summary>
        /// The index in <see cref="Source"/> at which this token begins representing characters.
        /// </summary>
        public int SourceIndex;
        /// <summary>
        /// The number of characters in <see cref="Source"/> starting at <see cref="SourceIndex"/> that this token represents.
        /// </summary>
        public int SourceLength;
        /// <summary>
        /// The definition that this token represents.
        /// </summary>
        public TokenDefinition Definition;
        /// <summary>
        /// The string content in <see cref="Source"/> that this token represents.
        /// </summary>
        public string Contents;
    }
}
