// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.ExclusionGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Linq;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class ExclusionGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
  {
    private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPostProcessTokens;
    public static readonly GrammerGroup ExclusionGrammerGroup = new GrammerGroup("Exclusion");
    private HashSet<TokenKind> _exclusions;
    private TokenDefinitionMask _exclusionMask;

    public override GrammerGroup GrammerGroup => ExclusionGrammer.ExclusionGrammerGroup;

    public override ASTGrammerBehaviour Behaviour => ASTGrammerBehaviour.DoesPostProcessTokens;

    public HashSet<TokenKind> Exclusions => this._exclusions;

    public ExclusionGrammer() => this._exclusions = new HashSet<TokenKind>();

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      TokenKind[] array = this._exclusions.ToArray<TokenKind>();
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) array));
      this._exclusionMask = new TokenDefinitionMask((TokenDefinition[]) array);
    }

    protected internal override void PostProcessTokens(
      Tokenizer<Token, TokenKind> tokenizer,
      IList<Token> rawTokens,
      int phase)
    {
      if (phase != 0)
        return;
      ExposedList<Token> exposedList = rawTokens as ExposedList<Token>;
      Token[] underlyingArray = exposedList.UnderlyingArray;
      int count = exposedList.Count;
      int num = 0;
      Token token1 = (Token) null;
      Token token2 = (Token) null;
      Token token3 = (Token) null;
      for (int index = 0; index < count; ++index)
      {
        Token token4 = underlyingArray[index];
        if (!this._exclusionMask[(TokenDefinition) token4.Definition])
        {
          token4.TokenIndex = num;
          token4.Previous = token1;
          token4.PreviousNWS = token2;
          token4.PreviousWS = token3;
          if (token1 != null)
            token1.Next = token4;
          int tokenIndex1 = token4.TokenIndex;
          if (token4.Definition.IsWhitespace)
          {
            for (int tokenIndex2 = token3 != null ? token3.TokenIndex : 0; tokenIndex2 < tokenIndex1; ++tokenIndex2)
              underlyingArray[tokenIndex2].NextWS = token4;
            token3 = token4;
          }
          else
          {
            for (int tokenIndex3 = token2 != null ? token2.TokenIndex : 0; tokenIndex3 < tokenIndex1; ++tokenIndex3)
              underlyingArray[tokenIndex3].NextNWS = token4;
            token2 = token4;
          }
          token1 = token4;
          underlyingArray[num++] = token4;
        }
      }
      if (num == count)
        return;
      exposedList.Count = num;
    }

    protected internal override Node BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Token start,
      Token last)
    {
      throw new NotSupportedException();
    }
  }
}
