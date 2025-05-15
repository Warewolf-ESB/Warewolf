// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.JSONGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
  public class JSONGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
  {
    private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.None;
    public static readonly GrammerGroup JSONGrammerGroup = new GrammerGroup("JSON");

    public override GrammerGroup GrammerGroup => JSONGrammer.JSONGrammerGroup;

    public override ASTGrammerBehaviour Behaviour => ASTGrammerBehaviour.None;

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry) => triggerRegistry.Register((TokenDefinition) TokenKind.LeftCurlyBracket);

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) new List<TokenKind>()
      {
        TokenKind.LeftCurlyBracket,
        TokenKind.RightCurlyBracket,
        TokenKind.Comma,
        TokenKind.Colon
      }));
    }

    protected internal override JSONNode BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder,
      JSONNode container,
      Token start,
      Token last)
    {
      if (start.Definition != TokenKind.LeftCurlyBracket)
        return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.RuntimeFailureReason.ArgumentException, nameof (start));
      List<JSONNode> jsonNodeList = new List<JSONNode>();
      Token token = start;
      while (start != null)
      {
        if ((start = start.NextNWS) == null)
          return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedExitScope, token, TokenKind.RightCurlyBracket, TokenKind.RegularString);
        if (start.Definition != TokenKind.RightCurlyBracket)
        {
          JSONNode jsonNode = this.BuildJSONEntryNode(builder, container, start, last);
          if (jsonNode == null)
            return (JSONNode) null;
          if ((start = jsonNode.End) == null)
            return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedExitScope, token, TokenKind.RightCurlyBracket, TokenKind.RegularString);
          jsonNodeList.Add(jsonNode);
          if (start.Definition == TokenKind.RightCurlyBracket)
            break;
        }
        else
          break;
      }
      return (JSONNode) new JSONRoot(new TokenPair(token, start), jsonNodeList.ToArray());
    }

    private JSONNode BuildJSONEntryNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder,
      JSONNode container,
      Token start,
      Token last)
    {
      if (start == null || !start.Definition.IsStringLiteral)
        return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightCurlyBracket, TokenKind.RegularString);
      Token token = start;
      if ((start = start.NextNWS) == null || start.Definition != TokenKind.Colon)
        return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedTerminal, token, TokenKind.Colon);
      Token errorLocation = start;
      if ((start = start.NextNWS) == null)
        return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedTerminal, token, TokenKind.LeftCurlyBracket, TokenKind.RegularString);
      if (start.Definition.IsStringLiteral)
        return (JSONNode) new JSONScalar(token, start);
      if (start.Definition != TokenKind.LeftCurlyBracket)
        return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedTerminal, token, TokenKind.LeftCurlyBracket, TokenKind.RegularString);
      List<JSONNode> jsonNodeList = new List<JSONNode>();
      while (start != null)
      {
        if ((start = start.NextNWS) == null)
          return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedExitScope, errorLocation, TokenKind.RightCurlyBracket, TokenKind.RegularString);
        if (start.Definition != TokenKind.RightCurlyBracket)
        {
          JSONNode jsonNode = this.BuildJSONEntryNode(builder, container, start, last);
          if (jsonNode == null)
            return (JSONNode) null;
          if ((start = jsonNode.End) == null)
            return this.Fail<JSONNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>.SyntaxFailureReason.ExpectedExitScope, errorLocation, TokenKind.RightCurlyBracket, TokenKind.RegularString);
          jsonNodeList.Add(jsonNode);
          if (start.Definition == TokenKind.RightCurlyBracket)
            break;
        }
        else
          break;
      }
      JSONComposite jsonComposite = new JSONComposite(token, start);
      jsonComposite.Items = jsonNodeList.ToArray();
      return (JSONNode) jsonComposite;
    }
  }
}
