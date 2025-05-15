// Decompiled with JetBrains decompiler
// Type: System.Parsing.ParseEventLog
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing
{
  public abstract class ParseEventLog
  {
    public static readonly ParseEventLog NullEventLog = (ParseEventLog) new __NoOpParseEventLog();

    public abstract bool HasEventLogs { get; }

    public abstract ParseEventLogEntry[] GetEventLogs();

    public abstract void Log(ParseEventLogEntry entry);

    public abstract void Clear();
  }
}
