// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.TokenClassification
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public enum TokenClassification
  {
    Invalid = -1, // 0xFFFFFFFF
    Unknown = 0,
    Whitespace = 1,
    EndOfFile = 2,
    Operator = 3,
    StringLiteral = 4,
    CharLiteral = 5,
    IntegerLiteral = 6,
    RealLiteral = 7,
    BooleanLiteral = 8,
    NullLiteral = 9,
    TypeLiteral = 10, // 0x0000000A
  }
}
