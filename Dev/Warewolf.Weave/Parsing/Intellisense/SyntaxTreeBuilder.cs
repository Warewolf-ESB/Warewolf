// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.SyntaxTreeBuilder
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class SyntaxTreeBuilder : AbstractSyntaxTreeBuilder<Token, TokenKind, Node>
  {
    public SyntaxTreeBuilder()
    {
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new InfrigisticNumericLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new BooleanLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new InfrigisticsGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new DatalistGrammer());
    }

    protected override Tokenizer<Token, TokenKind> CreateTokenizerInstance() => (Tokenizer<Token, TokenKind>) new IntellisenseTokenizer();

    protected override void OnUnhandledTokenEncountered(Node container, Token token)
    {
      if (container != null && container is CompositeStringLiteralNode || token.Definition.IsWhitespace)
        return;
      this.EventLog.Log(new ParseEventLogEntry(token.Source, token.ToParseEventLogToken(), (TokenDefinition) null, 5, 0, nameof (SyntaxTreeBuilder), nameof (OnUnhandledTokenEncountered)));
    }
  }
}
