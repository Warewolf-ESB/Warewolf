// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.InfrigisticsGrammer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public class InfrigisticsGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
  {
    private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPrependTokens | ASTGrammerBehaviour.DoesAppendTokens | ASTGrammerBehaviour.DoesPostProcessTokens;
    private static readonly ParameterCollectionNode[] EmptyParameters = new ParameterCollectionNode[0];
    private static readonly TokenDefinitionMask _signConcatenationMask = new TokenDefinitionMask(new TokenDefinition[5]
    {
      (TokenDefinition) TokenKind.LeftCurlyBracket,
      (TokenDefinition) TokenKind.LeftParenthesis,
      (TokenDefinition) TokenKind.Plus,
      (TokenDefinition) TokenKind.Minus,
      (TokenDefinition) TokenKind.Comma
    });
    private static readonly int[] _operatorPrecedenceLookup = InfrigisticsGrammer.BuildOperatorPrecedenceLookup();
    public static readonly GrammerGroup CalcGrammerGroup = new GrammerGroup("Infrigistics");
    private static readonly TokenDefinitionMask _evaluationOperators = new TokenDefinitionMask(new TokenDefinition[6]
    {
      (TokenDefinition) TokenKind.LessThanOrEqual,
      (TokenDefinition) TokenKind.GreaterThanOrEqual,
      (TokenDefinition) TokenKind.LessThan,
      (TokenDefinition) TokenKind.GreaterThan,
      (TokenDefinition) TokenKind.Inequality,
      (TokenDefinition) TokenKind.Equality
    });
    private static readonly TokenDefinitionMask _binaryOperators = new TokenDefinitionMask(new TokenDefinition[9]
    {
      (TokenDefinition) TokenKind.Plus,
      (TokenDefinition) TokenKind.Minus,
      (TokenDefinition) TokenKind.Comma,
      (TokenDefinition) TokenKind.Asterisk,
      (TokenDefinition) TokenKind.ForwardSlash,
      (TokenDefinition) TokenKind.Ampersand,
      (TokenDefinition) TokenKind.Circumflex,
      (TokenDefinition) TokenKind.Colon,
      (TokenDefinition) TokenKind.Mod
    });
    private static readonly TokenDefinitionMask _statementOperators = new TokenDefinitionMask(new TokenDefinition[14]
    {
      (TokenDefinition) TokenKind.Plus,
      (TokenDefinition) TokenKind.Minus,
      (TokenDefinition) TokenKind.Asterisk,
      (TokenDefinition) TokenKind.ForwardSlash,
      (TokenDefinition) TokenKind.Ampersand,
      (TokenDefinition) TokenKind.Circumflex,
      (TokenDefinition) TokenKind.Colon,
      (TokenDefinition) TokenKind.LessThanOrEqual,
      (TokenDefinition) TokenKind.GreaterThanOrEqual,
      (TokenDefinition) TokenKind.LessThan,
      (TokenDefinition) TokenKind.GreaterThan,
      (TokenDefinition) TokenKind.Inequality,
      (TokenDefinition) TokenKind.Equality,
      (TokenDefinition) TokenKind.Mod
    });

    public override GrammerGroup GrammerGroup => InfrigisticsGrammer.CalcGrammerGroup;

    public override ASTGrammerBehaviour Behaviour => ASTGrammerBehaviour.DoesPrependTokens | ASTGrammerBehaviour.DoesAppendTokens | ASTGrammerBehaviour.DoesPostProcessTokens;

    private static int[] BuildOperatorPrecedenceLookup()
    {
      int[] numArray1 = new int[TokenDefinition.GetTotalDefinitionsOfType(typeof (TokenKind))];
      int num1 = 1;
      int[] numArray2 = numArray1;
      int serial1 = TokenKind.Ampersand.Serial;
      int num2 = num1;
      int num3 = num2 + 1;
      numArray2[serial1] = num2;
      numArray1[TokenKind.Plus.Serial] = num3;
      int[] numArray3 = numArray1;
      int serial2 = TokenKind.Minus.Serial;
      int num4 = num3;
      int num5 = num4 + 1;
      numArray3[serial2] = num4;
      numArray1[TokenKind.Asterisk.Serial] = num5;
      int[] numArray4 = numArray1;
      int serial3 = TokenKind.ForwardSlash.Serial;
      int num6 = num5;
      int num7 = num6 + 1;
      numArray4[serial3] = num6;
      int[] numArray5 = numArray1;
      int serial4 = TokenKind.Circumflex.Serial;
      int num8 = num7;
      int num9 = num8 + 1;
      numArray5[serial4] = num8;
      int[] numArray6 = numArray1;
      int serial5 = TokenKind.Colon.Serial;
      int num10 = num9;
      int num11 = num10 + 1;
      numArray6[serial5] = num10;
      return numArray1;
    }

    protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
    {
      triggerRegistry.Register((TokenDefinition) TokenKind.Unknown);
      triggerRegistry.Register((TokenDefinition) TokenKind.LeftParenthesis);
      triggerRegistry.Register((TokenDefinition) TokenKind.Plus);
      triggerRegistry.Register((TokenDefinition) TokenKind.Mod);
      triggerRegistry.Register((TokenDefinition) TokenKind.Minus);
      triggerRegistry.Register((TokenDefinition) TokenKind.Comma);
      triggerRegistry.Register((TokenDefinition) TokenKind.Asterisk);
      triggerRegistry.Register((TokenDefinition) TokenKind.ForwardSlash);
      triggerRegistry.Register((TokenDefinition) TokenKind.Ampersand);
      triggerRegistry.Register((TokenDefinition) TokenKind.Circumflex);
      triggerRegistry.Register((TokenDefinition) TokenKind.Colon);
      triggerRegistry.Register((TokenDefinition) TokenKind.LessThanOrEqual);
      triggerRegistry.Register((TokenDefinition) TokenKind.GreaterThanOrEqual);
      triggerRegistry.Register((TokenDefinition) TokenKind.Inequality);
      triggerRegistry.Register((TokenDefinition) TokenKind.LessThan);
      triggerRegistry.Register((TokenDefinition) TokenKind.GreaterThan);
      triggerRegistry.Register((TokenDefinition) TokenKind.Equality);
      triggerRegistry.Register(typeof (ParenthesisGroupNode));
      triggerRegistry.Register(typeof (ParameterCollectionNode));
    }

    protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
      base.OnConfigureTokenizer(tokenizer);
      tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) new List<TokenKind>()
      {
        TokenKind.LeftParenthesis,
        TokenKind.RightParenthesis,
        TokenKind.Plus,
        TokenKind.Minus,
        TokenKind.Comma,
        TokenKind.Asterisk,
        TokenKind.ForwardSlash,
        TokenKind.Ampersand,
        TokenKind.Circumflex,
        TokenKind.Colon,
        TokenKind.Mod,
        TokenKind.LessThanOrEqual,
        TokenKind.GreaterThanOrEqual,
        TokenKind.Inequality,
        TokenKind.LessThan,
        TokenKind.GreaterThan,
        TokenKind.Equality
      }));
    }

    protected internal override void PrependTokens(ITokenBuilder builder) => builder.Append((TokenDefinition) TokenKind.LeftParenthesis);

    protected internal override void AppendTokens(ITokenBuilder builder) => builder.Append((TokenDefinition) TokenKind.RightParenthesis);

    protected internal override void PostProcessTokens(
      Tokenizer<Token, TokenKind> tokenizer,
      IList<Token> rawTokens,
      int phase)
    {
      if (phase != 1)
        return;
      ExposedList<Token> exposedList = rawTokens as ExposedList<Token>;
      Token[] underlyingArray = exposedList.UnderlyingArray;
      int num = exposedList.Count;
      for (int index = 0; index < num; ++index)
      {
        Token token1 = underlyingArray[index];
        TokenKind definition = token1.Definition;
        if (definition.IsIntegerLiteral || definition.IsRealLiteral)
        {
          Token previousNws1 = token1.PreviousNWS;
          if (previousNws1 != null && (previousNws1.Definition == TokenKind.Plus || previousNws1.Definition == TokenKind.Minus))
          {
            Token previousNws2 = previousNws1.PreviousNWS;
            if (previousNws2 == null || !definition.IsUnknown && (InfrigisticsGrammer._signConcatenationMask[(TokenDefinition) previousNws2.Definition] || InfrigisticsGrammer._statementOperators[(TokenDefinition) previousNws2.Definition]))
            {
              HashSet<Token> tokenSet = new HashSet<Token>();
              for (Token token2 = token1; token2 != previousNws1; token2 = token2.Previous)
                tokenSet.Add(token2);
              Token token3 = previousNws1.Previous == null ? previousNws1 : (!((Token<Token, TokenKind>) previousNws1.PreviousNWS < (Token<Token, TokenKind>) previousNws1.Previous) ? ((Token<Token, TokenKind>) previousNws1.Previous < (Token<Token, TokenKind>) previousNws1.PreviousWS ? previousNws1.Previous : previousNws1.PreviousWS) : ((Token<Token, TokenKind>) previousNws1.PreviousNWS < (Token<Token, TokenKind>) previousNws1.PreviousWS ? previousNws1.PreviousNWS : previousNws1.PreviousWS));
              Token token4 = token1.Next == null ? token1 : (!((Token<Token, TokenKind>) token1.NextNWS > (Token<Token, TokenKind>) token1.Next) ? ((Token<Token, TokenKind>) token1.Next > (Token<Token, TokenKind>) token1.NextWS ? token1.Next : token1.NextWS) : ((Token<Token, TokenKind>) token1.NextNWS > (Token<Token, TokenKind>) token1.NextWS ? token1.NextNWS : token1.NextWS));
              for (; token3 != previousNws1; token3 = token3.Next)
              {
                if (tokenSet.Contains(token3.NextWS))
                  token3.NextWS = token1.NextWS;
              }
              Token previous;
              for (; token4 != token1; token4 = previous)
              {
                previous = token4.Previous;
                if (tokenSet.Contains(token4.Previous))
                  token4.Previous = token3;
                if (tokenSet.Contains(token4.PreviousNWS))
                  token4.PreviousNWS = token3;
                if (tokenSet.Contains(token4.PreviousWS))
                  token4.PreviousWS = previousNws1.PreviousWS;
              }
              Token token5 = previousNws1.Next = token1.Next;
              previousNws1.NextNWS = token1.NextNWS;
              previousNws1.NextWS = token1.NextWS;
              previousNws1.SourceLength = token1.SourceIndex - previousNws1.SourceIndex + token1.SourceLength;
              previousNws1.Definition = token1.Definition;
              num = previousNws1.TokenIndex + 1;
              for (; token5 != null; token5 = token5.Next)
                underlyingArray[token5.TokenIndex = num++] = token5;
              index = previousNws1.TokenIndex;
            }
          }
        }
      }
      exposedList.Count = num;
    }

    protected internal override Node BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Token start,
      Token last)
    {
      if (start == null)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.RuntimeFailureReason.ArgumentNullException, nameof (start));
      switch (start.Definition.Kind)
      {
        case TokenClassification.Unknown:
          return start.NextNWS != null && start.NextNWS.Definition == TokenKind.LeftParenthesis ? this.BuildMethodInvocation(builder, start, last) : (Node) null;
        case TokenClassification.Operator:
          if (InfrigisticsGrammer._binaryOperators[(TokenDefinition) start.Definition])
          {
            PlaceholderOperatorNode placeholderOperatorNode = new PlaceholderOperatorNode();
            placeholderOperatorNode.Identifier = (TokenPair) start;
            placeholderOperatorNode.Declaration = (TokenPair) start;
            return (Node) placeholderOperatorNode;
          }
          if (InfrigisticsGrammer._evaluationOperators[(TokenDefinition) start.Definition])
          {
            PlaceholderOperatorNode placeholderOperatorNode = new PlaceholderOperatorNode();
            placeholderOperatorNode.Identifier = (TokenPair) start;
            placeholderOperatorNode.Declaration = (TokenPair) start;
            return (Node) placeholderOperatorNode;
          }
          if (start.Definition == TokenKind.LeftParenthesis)
            return this.BuildParenthesisGroup(start, last);
          break;
      }
      return (Node) null;
    }

    private Node BuildParenthesisGroup(Token start, Token last)
    {
      TokenPair tokenPair = TokenUtility.BuildGroupSimple(start, TokenKind.RightParenthesis);
      if (tokenPair.End == null)
        return (Node) this.Fail<ParenthesisGroupNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightParenthesis);
      if (tokenPair.End.PreviousNWS == tokenPair.Start)
        return (Node) this.Fail<ParenthesisGroupNode>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedIdentifier, tokenPair.Start, TokenKind.Unknown);
      ParenthesisGroupNode parenthesisGroupNode = new ParenthesisGroupNode();
      parenthesisGroupNode.Declaration = tokenPair;
      parenthesisGroupNode.Identifier = tokenPair;
      return (Node) parenthesisGroupNode;
    }

    private Node BuildMethodInvocation(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Token start,
      Token last)
    {
      if (start.NextNWS == null || start.NextNWS.Definition != TokenKind.LeftParenthesis)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedEnterScope, start, TokenKind.LeftParenthesis);
      TokenPair tokenPair = TokenUtility.BuildGroupSimple(start.NextNWS, TokenKind.RightParenthesis);
      if (tokenPair.End == null)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedExitScope, start.NextNWS, TokenKind.RightParenthesis);
      ParameterCollectionNode parameterCollectionNode = new ParameterCollectionNode();
      parameterCollectionNode.Identifier = (TokenPair) start;
      parameterCollectionNode.Declaration = tokenPair;
      MethodInvocationNode methodInvocationNode = new MethodInvocationNode();
      methodInvocationNode.Declaration = new TokenPair(start, tokenPair.End);
      methodInvocationNode.Identifier = (TokenPair) start;
      methodInvocationNode.Parameters = parameterCollectionNode;
      return (Node) methodInvocationNode;
    }

    protected internal override Node[] TransformNodes(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node container,
      Node[] nodes)
    {
      if (container is ParameterCollectionNode)
        return this.PerformParameterTransform(builder, container, nodes);
      int length = nodes.Length;
      if (length == 0)
        return new Node[0];
      return new Node[1]
      {
        this.PerformStatementTransform(builder, nodes, 0, length)
      };
    }

    private Node[] PerformParameterTransform(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node collection,
      Node[] nodes)
    {
      Token end = collection.Declaration.End;
      List<ParameterNode> parameterNodeList = new List<ParameterNode>();
      int index1 = 0;
      for (int index2 = 0; index2 < nodes.Length; ++index2)
      {
        if (nodes[index2].Declaration.Start.Definition == TokenKind.Comma)
        {
          int length = index2 - index1;
          if (length == 0)
            return this.Fail<Node[]>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedIdentifier, nodes[index2].Declaration.Start, TokenKind.Unknown);
          TokenPair tokenPair = new TokenPair(nodes[index1].Declaration.Start, nodes[index1 + length - 1].End);
          Node node = this.PerformStatementTransform(builder, nodes, index1, length);
          ParameterNode parameterNode1 = new ParameterNode();
          parameterNode1.Statement = node;
          parameterNode1.Declaration = tokenPair;
          ParameterNode parameterNode2 = parameterNode1;
          parameterNode2.Identifier = parameterNode2.Declaration;
          parameterNodeList.Add(parameterNode2);
          index1 = index2 + 1;
        }
      }
      if (index1 != nodes.Length)
      {
        int length = nodes.Length - index1;
        TokenPair tokenPair = new TokenPair(nodes[index1].Declaration.Start, nodes[index1 + length - 1].End);
        Node node = this.PerformStatementTransform(builder, nodes, index1, length);
        ParameterNode parameterNode3 = new ParameterNode();
        parameterNode3.Statement = node;
        parameterNode3.Declaration = tokenPair;
        ParameterNode parameterNode4 = parameterNode3;
        parameterNode4.Identifier = parameterNode4.Declaration;
        parameterNodeList.Add(parameterNode4);
      }
      else if (end.PreviousNWS.Definition == TokenKind.Comma)
        return this.Fail<Node[]>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedIdentifier, end.PreviousNWS, TokenKind.Unknown);
      return (Node[]) parameterNodeList.ToArray();
    }

    private Node PerformStatementTransform(
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder,
      Node[] nodes,
      int index,
      int length)
    {
      if (length == 0)
        return (Node) null;
      BitVector bitVector = new BitVector();
      int num1 = index + length;
      int index1 = -1;
      int num2 = 0;
      int num3 = 32;
      int num4 = 0;
      if (!nodes[index].IsTerminal)
        return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedTerminal, nodes[index].Declaration.Start);
      for (int index2 = index + 1; index2 < num1; index2 += 2)
      {
        Node node;
        if ((node = nodes[index2]).IsTerminal)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedNonTerminal, node.Declaration.Start);
        if (!InfrigisticsGrammer._statementOperators[(TokenDefinition) node.Declaration.Start.Definition])
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.UnexpectedToken, node.Declaration.Start);
        int serial = node.Declaration.Start.Definition.Serial;
        index1 = InfrigisticsGrammer._operatorPrecedenceLookup[serial];
        bitVector[index1] = true;
        if (index1 > num2)
          num2 = index1;
        if (index1 < num3)
          num3 = index1;
        if (index2 + 1 >= num1)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedTerminal, node.Declaration.Start);
        if (!nodes[index2 + 1].IsTerminal)
          return this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedTerminal, nodes[index2 + 1].Declaration.Start);
        ++num4;
      }
      Node node1 = (Node) null;
      if (index1 != -1)
      {
        for (int index3 = num2; index3 >= num3; --index3)
        {
          if (bitVector[index3])
          {
            for (int index4 = index + 1; index4 < num1; index4 += 2)
            {
              Node node2;
              if (!(node2 = nodes[index4]).IsTerminal)
              {
                int serial = node2.Declaration.Start.Definition.Serial;
                if (InfrigisticsGrammer._operatorPrecedenceLookup[serial] == index3)
                {
                  Node transientReference1 = nodes[index4 - 1].TransientReference;
                  Node transientReference2 = nodes[index4 + 1].TransientReference;
                  Node node3 = nodes[index4] = (Node) new BinaryOperatorNode(node2, transientReference1, transientReference2);
                  transientReference1.GetLeftMostNode().TransientReference = node3;
                  transientReference2.GetRightMostNode().TransientReference = node3;
                  if (--num4 == 0)
                  {
                    node1 = node3;
                    index3 = -1;
                    break;
                  }
                }
              }
            }
          }
        }
      }
      else
        node1 = nodes[index];
      return node1 ?? this.Fail<Node>(AbstractSyntaxTreeGrammer<Token, TokenKind, Node>.SyntaxFailureReason.ExpectedTerminal, nodes[index].Declaration.Start);
    }
  }
}
