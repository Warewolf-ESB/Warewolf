// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.CSharpNumericLiteralGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class CSharpNumericLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
  {
    public override GrammerGroup GrammerGroup => GrammerGroup.NumericLiteralGrammers;

    protected internal override Node BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Token start,
      Token last)
    {
      return (Node) new LiteralNode(start);
    }

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
      triggerRegistry.Register((TokenDefinition) TokenKind.IntegerHexadecimal);
      triggerRegistry.Register((TokenDefinition) TokenKind.IntegerNoSuffix);
      triggerRegistry.Register((TokenDefinition) TokenKind.IntegerSuffixL);
      triggerRegistry.Register((TokenDefinition) TokenKind.IntegerSuffixU);
      triggerRegistry.Register((TokenDefinition) TokenKind.IntegerSuffixUL);
      triggerRegistry.Register((TokenDefinition) TokenKind.RealNoSuffix);
      triggerRegistry.Register((TokenDefinition) TokenKind.RealSuffixD);
      triggerRegistry.Register((TokenDefinition) TokenKind.RealSuffixF);
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new CSharpNumericLiteralTokenizationHandler());
    }
  }
}
