// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistStringLiteralGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class DatalistStringLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
  {
    private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPostProcessTokens;
    private DatalistGrammer _sourceGrammer;
    private IntellisenseTokenizer _tokenizer;

    public override GrammerGroup GrammerGroup => GrammerGroup.StringLiteralGrammers;

    public override ASTGrammerBehaviour Behaviour => ASTGrammerBehaviour.DoesPostProcessTokens;

    internal DatalistStringLiteralGrammer(DatalistGrammer sourceGrammer) => this._sourceGrammer = sourceGrammer;

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
      triggerRegistry.Register((TokenDefinition) TokenKind.RegularString);
      triggerRegistry.Register((TokenDefinition) TokenKind.VerbatimString);
      triggerRegistry.Register((TokenDefinition) TokenKind.CompositeVerbatimString);
      triggerRegistry.Register((TokenDefinition) TokenKind.CompositeRegularString);
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new StringLiteralTokenizationHandler());
    }

    protected internal override void PostProcessTokens(
      Tokenizer<Token, TokenKind> tokenizer,
      IList<Token> rawTokens,
      int phase)
    {
      if (phase != 0)
        return;
      ExposedList<Token> rawTokens1 = rawTokens as ExposedList<Token>;
      bool flag = false;
      for (int outerPosition = 0; outerPosition < rawTokens1.Count; ++outerPosition)
      {
        Token token = rawTokens1[outerPosition];
        if (token.Definition.IsStringLiteral)
        {
          int num1 = token.SourceIndex + 1;
          int num2 = token.SourceLength - 2;
          if (token.Definition == TokenKind.VerbatimString)
          {
            ++num1;
            --num2;
          }
          if (num2 > 0 && token.Source.IndexOf(TokenKind.OpenDL.Identifier, num1, num2, StringComparison.OrdinalIgnoreCase) != -1)
          {
            if (this._tokenizer == null)
            {
              this._tokenizer = new IntellisenseTokenizer();
              this._tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) new List<TokenKind>()
              {
                TokenKind.OpenDL,
                TokenKind.CloseDL
              }));
            }
            Token[] regions = this._tokenizer.Tokenize(token.Source, num1, num2, false);
            if (regions != null && this.BuildDatalistRegions(tokenizer, regions, rawTokens1, ref outerPosition))
              flag = true;
          }
        }
      }
      if (!flag)
        return;
      Token[] underlyingArray = rawTokens1.UnderlyingArray;
      int count = rawTokens1.Count;
      Token token1 = (Token) null;
      Token token2 = (Token) null;
      Token token3 = (Token) null;
      for (int index = 0; index < underlyingArray.Length && index < count; ++index)
      {
        Token token4 = underlyingArray[index];
        token4.TokenIndex = index;
        token4.Previous = token1;
        token4.PreviousNWS = token2;
        token4.PreviousWS = token3;
        if (token1 != null)
          token1.Next = token4;
        if (token4.Definition.IsWhitespace)
        {
          for (int tokenIndex = token3 != null ? token3.TokenIndex : 0; tokenIndex < index; ++tokenIndex)
            underlyingArray[tokenIndex].NextWS = token4;
          token3 = token4;
        }
        else
        {
          for (int tokenIndex = token2 != null ? token2.TokenIndex : 0; tokenIndex < index; ++tokenIndex)
            underlyingArray[tokenIndex].NextNWS = token4;
          token2 = token4;
        }
        token1 = token4;
      }
    }

    private bool BuildDatalistRegions(
      Tokenizer<Token, TokenKind> primaryTokenizer,
      Token[] regions,
      ExposedList<Token> rawTokens,
      ref int outerPosition)
    {
      if (regions.Length == 0)
        return false;
      bool flag = false;
      for (Token start = regions[0]; start != null && !start.Definition.IsEndOfFile; start = start.NextNWS)
      {
        if (start.Definition == TokenKind.OpenDL)
        {
          TokenPair tokenPair = TokenUtility.BuildGroupSimple(start, TokenKind.CloseDL);
          if (tokenPair.End == null || tokenPair.End.Definition != TokenKind.CloseDL)
            return flag;
          Token[] tokenArray = primaryTokenizer.Tokenize(start.Source, start.SourceIndex, tokenPair.End.SourceLength + tokenPair.End.SourceIndex - start.SourceIndex, false);
          if (tokenArray == null)
          {
            start = tokenPair.End;
          }
          else
          {
            if (!flag)
            {
              Token rawToken = rawTokens[outerPosition];
              Token token = tokenArray[tokenArray.Length - 1];
              token.Definition = TokenKind.CompositeStringClosure;
              token.Source = rawToken.Source;
              token.SourceIndex = rawToken.SourceIndex + rawToken.SourceLength - 1;
              token.SourceLength = 1;
              if (rawToken.Definition == TokenKind.RegularString)
              {
                rawToken.Definition = TokenKind.CompositeRegularString;
                rawToken.SourceLength = 1;
              }
              else
              {
                if (rawToken.Definition != TokenKind.VerbatimString)
                  throw new InvalidOperationException("outerCurrent is not a string literal.");
                rawToken.Definition = TokenKind.CompositeVerbatimString;
                rawToken.SourceLength = 2;
              }
              rawTokens.InsertRange(outerPosition + 1, (IEnumerable<Token>) tokenArray);
              flag = true;
              outerPosition += tokenArray.Length;
            }
            else
            {
              rawTokens.InsertRange(outerPosition + 1, tokenArray, 0, tokenArray.Length - 1);
              outerPosition += tokenArray.Length - 1;
            }
            start = tokenPair.End;
          }
        }
      }
      return flag;
    }

    protected internal override Node BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Token start,
      Token last)
    {
      if (start.Definition == TokenKind.RegularString || start.Definition == TokenKind.VerbatimString)
        return (Node) new LiteralNode(start);
      TokenPair tokenPair = TokenUtility.BuildGroupSimple(start, TokenKind.CompositeStringClosure, start.Definition == TokenKind.CompositeVerbatimString ? TokenKind.CompositeRegularString : TokenKind.CompositeVerbatimString);
      if (tokenPair.End == null)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, start, TokenKind.CompositeRegularString);
      CompositeStringLiteralNode stringLiteralNode = new CompositeStringLiteralNode();
      stringLiteralNode.Declaration = tokenPair;
      stringLiteralNode.Identifier = (TokenPair) start;
      return (Node) stringLiteralNode;
    }
  }
}
