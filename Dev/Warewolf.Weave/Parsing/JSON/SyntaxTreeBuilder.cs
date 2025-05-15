// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.SyntaxTreeBuilder
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
  public class SyntaxTreeBuilder : AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode>
  {
    public SyntaxTreeBuilder()
    {
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>) new CharLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>) new StringLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>) new CSharpNumericLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>) new BooleanLiteralGrammer());
      this.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>) new JSONGrammer());
    }

    protected override Tokenizer<Token, TokenKind> CreateTokenizerInstance() => (Tokenizer<Token, TokenKind>) new JSONTokenizer();

    protected override void OnUnhandledTokenEncountered(JSONNode container, Token token)
    {
      int num = token.Definition.IsWhitespace ? 1 : 0;
    }
  }
}
