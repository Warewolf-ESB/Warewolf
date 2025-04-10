// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class DatalistGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
  {
    public static readonly GrammerGroup DatalistGrammerGroup = new GrammerGroup("Datalist");

    public override GrammerGroup GrammerGroup => DatalistGrammer.DatalistGrammerGroup;

    protected internal override Node BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Token start,
      Token last)
    {
      if (start == null)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.RuntimeFailureReason.ArgumentNullException, nameof (start));
      if (start.Definition == TokenKind.Iteration)
        return (Node) new IterationNode(start);
      if (start.Definition != TokenKind.OpenDL)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.RuntimeFailureReason.ArgumentException, nameof (start), "DataListGrammer BuildNode requests must always start on a TokenKind.OpenDL ([[) token.");
      TokenPair dStart = new TokenPair(start);
      TokenPair tokenPair = new TokenPair();
      if ((start = start.Next) == null)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedIdentifier, dStart.Start, TokenKind.Unknown);
      Node result = (Node) null;
      Node node1;
      if (!start.Definition.IsUnknown)
      {
        if (start.Definition != TokenKind.OpenDL)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedToken, start, TokenKind.Unknown, TokenKind.OpenDL);
        Node node2 = this.BuildNode(builder, container, start, last);
        if (node2 == null)
          return (Node) null;
        if ((start = node2.End.Next) == null)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedToken, node2.End, TokenKind.CloseDL, TokenKind.LeftParenthesis);
        TokenPair declaration = node2.Declaration;
        if (start.Definition != TokenKind.CloseDL)
        {
          if (start.Definition != TokenKind.LeftParenthesis)
            return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedToken, start, TokenKind.CloseDL, TokenKind.LeftParenthesis);
          node1 = this.BuildDataListPostIdentifier(builder, start, last, dStart, declaration, result);
          Node node3;
          switch (node1)
          {
            case null:
              return (Node) null;
            case DatalistRecordSetFieldNode _:
              DatalistRecordSetFieldNode recordSetFieldNode = (DatalistRecordSetFieldNode) node1;
              recordSetFieldNode.RecordSet.NestedIdentifier = node2;
              node3 = (Node) recordSetFieldNode;
              break;
            case DatalistRecordSetNode _:
              DatalistRecordSetNode datalistRecordSetNode = (DatalistRecordSetNode) node1;
              datalistRecordSetNode.NestedIdentifier = node2;
              node3 = (Node) datalistRecordSetNode;
              break;
          }
          start = node1.Declaration.End;
          dStart.End = start;
        }
        else
        {
          dStart.End = start;
          DatalistNestedReferenceNode nestedReferenceNode = new DatalistNestedReferenceNode();
          nestedReferenceNode.NestedIdentifier = node2;
          nestedReferenceNode.Identifier = declaration;
          nestedReferenceNode.Declaration = dStart;
          node1 = (Node) nestedReferenceNode;
        }
      }
      else
      {
        TokenPair identifier = (TokenPair) start;
        if ((start = identifier.End.Next) == null)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, dStart.Start, TokenKind.CloseDL);
        node1 = this.BuildDataListPostIdentifier(builder, start, last, dStart, identifier, result);
      }
      return node1;
    }

    private Node BuildDataListPostIdentifier(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Token start,
      Token last,
      TokenPair dStart,
      TokenPair identifier,
      Node result)
    {
      if (start.Definition == TokenKind.LeftParenthesis)
      {
        TokenPair tokenPair = TokenUtility.BuildGroupSimple(start, TokenKind.RightParenthesis);
        if (tokenPair.End == null)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightParenthesis);
        DatalistRecordSetNode container = new DatalistRecordSetNode();
        ParenthesisGroupNode parenthesisGroupNode = new ParenthesisGroupNode();
        parenthesisGroupNode.Identifier = identifier;
        parenthesisGroupNode.Declaration = tokenPair;
        container.Parameter = parenthesisGroupNode;
        if (parenthesisGroupNode.Declaration.Start.NextNWS == parenthesisGroupNode.Declaration.End.PreviousNWS && parenthesisGroupNode.Declaration.Start.NextNWS.Definition == TokenKind.Asterisk)
          parenthesisGroupNode.Declaration.Start.NextNWS.Definition = TokenKind.Iteration;
        if ((start = tokenPair.End.Next) == null)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedToken, tokenPair.End, TokenKind.FullStop, TokenKind.CloseDL);
        if (start.Definition == TokenKind.FullStop)
        {
          container.Identifier = identifier;
          container.Declaration = new TokenPair(dStart.Start, tokenPair.End);
          if (start.Next == null)
            return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedIdentifier, start, TokenKind.Unknown, TokenKind.OpenDL);
          if (!(start = start.Next).Definition.IsUnknown)
          {
            if (start.Definition != TokenKind.OpenDL)
              return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedToken, start, TokenKind.Unknown, TokenKind.OpenDL);
            Node node = this.BuildNode(builder, (Node) container, start, last);
            if (node == null)
              return (Node) null;
            DatalistRecordSetFieldNode recordSetFieldNode = new DatalistRecordSetFieldNode();
            recordSetFieldNode.Identifier = node.Declaration;
            recordSetFieldNode.RecordSet = container;
            recordSetFieldNode.Field = node;
            if ((start = node.Declaration.End.Next) == null || start.Definition != TokenKind.CloseDL)
              return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, container.Declaration.Start, TokenKind.CloseDL);
            dStart.End = start;
            recordSetFieldNode.Declaration = dStart;
            result = (Node) recordSetFieldNode;
          }
          else
          {
            DatalistRecordSetFieldNode recordSetFieldNode = new DatalistRecordSetFieldNode();
            recordSetFieldNode.Identifier = (TokenPair) start;
            recordSetFieldNode.RecordSet = container;
            if ((start = start.Next) == null || start.Definition != TokenKind.CloseDL)
              return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, container.Declaration.Start, TokenKind.CloseDL);
            dStart.End = start;
            recordSetFieldNode.Declaration = dStart;
            result = (Node) recordSetFieldNode;
          }
        }
        else
        {
          if (start.Definition != TokenKind.CloseDL)
            return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.UnexpectedToken, start, TokenKind.FullStop, TokenKind.CloseDL);
          dStart.End = start;
          result = (Node) container;
          result.Declaration = dStart;
          result.Identifier = identifier;
        }
      }
      else
      {
        if (start.Definition != TokenKind.CloseDL)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.UnexpectedToken, start, TokenKind.LeftParenthesis, TokenKind.CloseDL);
        result = (Node) new DatalistReferenceNode();
        dStart.End = start;
        result.Declaration = dStart;
        result.Identifier = identifier;
      }
      return result;
    }

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
      triggerRegistry.Register((TokenDefinition) TokenKind.OpenDL);
      triggerRegistry.Register((TokenDefinition) TokenKind.Iteration);
    }

    protected internal override void OnRegisterSubGrammers(
      HashSet<GrammerGroup> existingGrammers,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      if (!existingGrammers.Contains(GrammerGroup.StringLiteralGrammers))
        builder.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new DatalistStringLiteralGrammer(this));
      if (existingGrammers.Contains(GrammerGroup.NumericLiteralGrammers))
        return;
      builder.RegisterGrammer((AbstractSyntaxTreeGrammer<Token, TokenKind, Node>) new DatalistNumericLiteralGrammer());
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) new List<TokenKind>()
      {
        TokenKind.CloseDL,
        TokenKind.LeftParenthesis,
        TokenKind.RightParenthesis,
        TokenKind.FullStop
      }));
    }
  }
}
