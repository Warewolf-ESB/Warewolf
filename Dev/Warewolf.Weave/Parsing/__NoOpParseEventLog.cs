// Decompiled with JetBrains decompiler
// Type: System.Parsing.__NoOpParseEventLog
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing
{
  internal sealed class __NoOpParseEventLog : ParseEventLog
  {
    public override bool HasEventLogs => false;

    public override ParseEventLogEntry[] GetEventLogs() => new ParseEventLogEntry[0];

    public override void Log(ParseEventLogEntry entry)
    {
    }

    public override void Clear()
    {
    }
  }
}
