// Decompiled with JetBrains decompiler
// Type: System.Parsing.ParseEventLogEntry
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.Tokenization;

namespace System.Parsing
{
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

    public string Source => this._source;

    public ParseEventLogToken ErrorStart => this._errorStart;

    public ParseEventLogToken ErrorEnd => this._errorEnd;

    public TokenDefinition Arg1 => this._arg1;

    public TokenDefinition Arg2 => this._arg2;

    public int EventType => this._eventType;

    public int EventData => this._eventData;

    public string Module => this._module;

    public string Component => this._component;

    public ParseEventLogEntry(
      string source,
      ParseEventLogToken errorStart,
      ParseEventLogToken errorEnd,
      TokenDefinition arg1,
      TokenDefinition arg2,
      int eventType,
      int eventData,
      string module,
      string component)
    {
      this._source = source;
      this._errorStart = errorStart;
      this._errorEnd = errorEnd;
      this._arg1 = arg1;
      this._arg2 = arg2;
      this._eventType = eventType;
      this._eventData = eventData;
      this._module = module;
      this._component = component;
    }

    public ParseEventLogEntry(
      string source,
      ParseEventLogToken errorLocation,
      TokenDefinition arg1,
      TokenDefinition arg2,
      int eventType,
      int eventData,
      string module,
      string component)
      : this(source, errorLocation, errorLocation, arg1, arg2, eventType, eventData, module, component)
    {
    }

    public ParseEventLogEntry(
      string source,
      ParseEventLogToken errorLocation,
      TokenDefinition arg1,
      int eventType,
      int eventData,
      string module,
      string component)
      : this(source, errorLocation, errorLocation, arg1, (TokenDefinition) null, eventType, eventData, module, component)
    {
    }
  }
}
