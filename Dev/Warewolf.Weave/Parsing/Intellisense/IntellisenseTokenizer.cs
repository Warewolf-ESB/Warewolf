// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.IntellisenseTokenizer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class IntellisenseTokenizer : Tokenizer<Token, TokenKind>
  {
    public IntellisenseTokenizer()
      : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), (ParseEventLog) new TestParseEventLog())
    {
    }

    public IntellisenseTokenizer(ParseEventLog eventLog)
      : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), eventLog)
    {
    }
  }
}
