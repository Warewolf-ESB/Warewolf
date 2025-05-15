// Decompiled with JetBrains decompiler
// Type: System.Parsing.TestParseEventLog
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Parsing
{
  public class TestParseEventLog : ParseEventLog
  {
    private List<ParseEventLogEntry> _entries;
    private bool _hasEntries;

    public override bool HasEventLogs => this._hasEntries;

    public TestParseEventLog() => this._entries = new List<ParseEventLogEntry>();

    public override ParseEventLogEntry[] GetEventLogs() => this._entries.ToArray();

    public override void Log(ParseEventLogEntry entry)
    {
      this._entries.Add(entry);
      this._hasEntries = true;
    }

    public override void Clear()
    {
      this._entries.Clear();
      this._hasEntries = false;
    }
  }
}
