// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.CharLiteralGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
  public class CharLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
  {
    public override GrammerGroup GrammerGroup => GrammerGroup.CharLiteralGrammers;

    protected internal override JSONNode BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder,
      JSONNode container,
      Token start,
      Token last)
    {
      return (JSONNode) new LiteralNode(start);
    }

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
      triggerRegistry.Register((TokenDefinition) TokenKind.EscapedChar);
      triggerRegistry.Register((TokenDefinition) TokenKind.HexadecimalChar);
      triggerRegistry.Register((TokenDefinition) TokenKind.RegularChar);
      triggerRegistry.Register((TokenDefinition) TokenKind.UnicodeChar);
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new CharacterLiteralTokenizationHandler());
    }
  }
}
