using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Parsing.Tokenization;
using System.Text;

namespace System.Parsing.Intellisense
{
    public class InfrigisticsGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
    {
        #region Constants
        private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPrependTokens | ASTGrammerBehaviour.DoesPostProcessTokens | ASTGrammerBehaviour.DoesAppendTokens;
        #endregion

        #region Readonly Members
        private static readonly ParameterCollectionNode[] EmptyParameters = new ParameterCollectionNode[0];
        private static readonly TokenDefinitionMask _signConcatenationMask = new TokenDefinitionMask(TokenKind.LeftCurlyBracket, TokenKind.LeftParenthesis, TokenKind.Plus, TokenKind.Minus, TokenKind.Comma);
        private static readonly int[] _operatorPrecedenceLookup = BuildOperatorPrecedenceLookup();

        public static readonly GrammerGroup CalcGrammerGroup = new GrammerGroup("Infrigistics");

        private static readonly Tokenization.TokenDefinitionMask _evaluationOperators = new Tokenization.TokenDefinitionMask(TokenKind.LessThanOrEqual, TokenKind.GreaterThanOrEqual, TokenKind.LessThan, TokenKind.GreaterThan, TokenKind.Inequality, TokenKind.Equality);
        private static readonly Tokenization.TokenDefinitionMask _binaryOperators = new Tokenization.TokenDefinitionMask(TokenKind.Plus, TokenKind.Minus, TokenKind.Comma, TokenKind.Asterisk, TokenKind.ForwardSlash, TokenKind.Ampersand, TokenKind.Circumflex, TokenKind.Colon, TokenKind.Mod);
        private static readonly Tokenization.TokenDefinitionMask _statementOperators = new Tokenization.TokenDefinitionMask(TokenKind.Plus, TokenKind.Minus, TokenKind.Asterisk, TokenKind.ForwardSlash, TokenKind.Ampersand, TokenKind.Circumflex, TokenKind.Colon, TokenKind.LessThanOrEqual, TokenKind.GreaterThanOrEqual, TokenKind.LessThan, TokenKind.GreaterThan, TokenKind.Inequality, TokenKind.Equality, TokenKind.Mod);
        #endregion

        #region Static Members
        private static int[] BuildOperatorPrecedenceLookup()
        {
            int[] result = new int[TokenDefinition.GetTotalDefinitionsOfType(typeof(TokenKind))];
            int precedence = 1;

            result[TokenKind.Ampersand.Serial] = precedence++;
            result[TokenKind.Plus.Serial] = precedence;
            result[TokenKind.Minus.Serial] = precedence++;
            result[TokenKind.Asterisk.Serial] = precedence;
            result[TokenKind.ForwardSlash.Serial] = precedence++;
            result[TokenKind.Circumflex.Serial] = precedence++;
            result[TokenKind.Colon.Serial] = precedence++;

            return result;
        }
        #endregion

        #region Public Properties
        public override GrammerGroup GrammerGroup { get { return CalcGrammerGroup; } }
        public override ASTGrammerBehaviour Behaviour { get { return _behaviourFlags; } }
        #endregion

        #region Configuration Handling
        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.Unknown);
            triggerRegistry.Register(TokenKind.LeftParenthesis);

            triggerRegistry.Register(TokenKind.Plus);
            triggerRegistry.Register(TokenKind.Mod);
            triggerRegistry.Register(TokenKind.Minus);
            triggerRegistry.Register(TokenKind.Comma);
            triggerRegistry.Register(TokenKind.Asterisk);
            triggerRegistry.Register(TokenKind.ForwardSlash);
            triggerRegistry.Register(TokenKind.Ampersand);
            triggerRegistry.Register(TokenKind.Circumflex);
            triggerRegistry.Register(TokenKind.Colon);

            triggerRegistry.Register(TokenKind.LessThanOrEqual);
            triggerRegistry.Register(TokenKind.GreaterThanOrEqual);
            triggerRegistry.Register(TokenKind.Inequality);
            triggerRegistry.Register(TokenKind.LessThan);
            triggerRegistry.Register(TokenKind.GreaterThan);
            triggerRegistry.Register(TokenKind.Equality);

            triggerRegistry.Register(typeof(ParenthesisGroupNode));
            triggerRegistry.Register(typeof(ParameterCollectionNode));
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);

            List<TokenKind> operators = new List<TokenKind>();

            operators.Add(TokenKind.LeftParenthesis);
            operators.Add(TokenKind.RightParenthesis);

            operators.Add(TokenKind.Plus);
            operators.Add(TokenKind.Minus);
            operators.Add(TokenKind.Comma);
            operators.Add(TokenKind.Asterisk);
            operators.Add(TokenKind.ForwardSlash);
            operators.Add(TokenKind.Ampersand);
            operators.Add(TokenKind.Circumflex);
            operators.Add(TokenKind.Colon);
            operators.Add(TokenKind.Mod);

            operators.Add(TokenKind.LessThanOrEqual);
            operators.Add(TokenKind.GreaterThanOrEqual);
            operators.Add(TokenKind.Inequality);
            operators.Add(TokenKind.LessThan);
            operators.Add(TokenKind.GreaterThan);
            operators.Add(TokenKind.Equality);

            tokenizer.Handlers.Add(new Tokenization.UnaryTokenizationHandler<Token, TokenKind>(operators));
        }
        #endregion

        #region Preprocess Handling
        protected internal override void PrependTokens(ITokenBuilder builder)
        {
            builder.Append(TokenKind.LeftParenthesis);
        }

        protected internal override void AppendTokens(ITokenBuilder builder)
        {
            builder.Append(TokenKind.RightParenthesis);
        }

        protected internal override void PostProcessTokens(Tokenizer<Token, TokenKind> tokenizer, IList<Token> rawTokens, int phase)
        {
            if(phase != 1) return;
            ExposedList<Token> tokens = rawTokens as ExposedList<Token>;

            // get underlying array of tokens and number of elements used
            Token[] underlying = tokens.UnderlyingArray;
            int length = tokens.Count;

            for(int i = 0; i < length; i++)
            {
                Token current = underlying[i];

                TokenKind definition = current.Definition;

                // check if token is literal number
                if(definition.IsIntegerLiteral || definition.IsRealLiteral)
                {
                    Token sign = current.PreviousNWS;
                    // check if previous token exists and is a plus or minus sign
                    if(sign == null || (sign.Definition != TokenKind.Plus && sign.Definition != TokenKind.Minus)) continue;

                    Token temp = sign.PreviousNWS;

                    if(temp != null)
                    {
                        // if token before sign is text, then this is a binary operator
                        if(definition.IsUnknown) continue;
                        if(!_signConcatenationMask[temp.Definition] && !_statementOperators[temp.Definition]) continue;
                    }

                    // create a set of the tokens we need to remove
                    HashSet<Token> pendingRemoval = new HashSet<Token>();
                    for(temp = current; temp != sign; temp = temp.Previous) pendingRemoval.Add(temp);

                    // get first token before sign that has any reference to sign or the tokens after
                    // sign, also get last token after literal number that has any reference to literal number
                    // or the tokens before literal number
                    Token firstAffected = sign.Previous == null ? sign : (sign.PreviousNWS < sign.Previous ? (sign.PreviousNWS < sign.PreviousWS ? sign.PreviousNWS : sign.PreviousWS) : (sign.Previous < sign.PreviousWS ? sign.Previous : sign.PreviousWS));
                    Token lastAffected = current.Next == null ? current : (current.NextNWS > current.Next ? (current.NextNWS > current.NextWS ? current.NextNWS : current.NextWS) : (current.Next > current.NextWS ? current.Next : current.NextWS));

                    // remap references to point to tokens after literal number
                    while(firstAffected != sign)
                    {
                        if(pendingRemoval.Contains(firstAffected.NextWS)) firstAffected.NextWS = current.NextWS;
                        firstAffected = firstAffected.Next;
                    }

                    // remap references to point to tokens before and including sign
                    while(lastAffected != current)
                    {
                        temp = lastAffected.Previous;
                        if(pendingRemoval.Contains(lastAffected.Previous)) lastAffected.Previous = firstAffected;
                        if(pendingRemoval.Contains(lastAffected.PreviousNWS)) lastAffected.PreviousNWS = firstAffected;
                        if(pendingRemoval.Contains(lastAffected.PreviousWS)) lastAffected.PreviousWS = sign.PreviousWS;
                        lastAffected = temp;
                    }

                    // remap sign references
                    sign.Next = temp = current.Next;
                    sign.NextNWS = current.NextNWS;
                    sign.NextWS = current.NextWS;
                    // remap sign content and definition
                    sign.SourceLength = (current.SourceIndex - sign.SourceIndex) + current.SourceLength;
                    sign.Definition = current.Definition;

                    // remap TokenIndex of all tokens after sign and keep track of new length
                    length = sign.TokenIndex + 1;

                    while(temp != null)
                    {
                        underlying[temp.TokenIndex = length++] = temp;
                        temp = temp.Next;
                    }

                    // set i to index of sign, so that next loop will be token after sign
                    i = sign.TokenIndex;
                }
            }

            tokens.Count = length;
        }
        #endregion

        #region Build Handling
        protected internal override Node BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Token start, Token last)
        {
            if(start == null) return Fail<Node>(RuntimeFailureReason.ArgumentNullException, "start");

            switch(start.Definition.Kind)
            {
                case TokenClassification.Unknown:
                    {
                        if(start.NextNWS != null && start.NextNWS.Definition == TokenKind.LeftParenthesis) return BuildMethodInvocation(builder, start, last);
                        return null;
                    }
                case TokenClassification.Operator:
                    {
                        if(_binaryOperators[start.Definition])
                        {
                            PlaceholderOperatorNode node = new PlaceholderOperatorNode();
                            node.Identifier = start;
                            node.Declaration = start;
                            return node;
                        }
                        else if(_evaluationOperators[start.Definition])
                        {
                            PlaceholderOperatorNode node = new PlaceholderOperatorNode();
                            node.Identifier = start;
                            node.Declaration = start;
                            return node;
                        }
                        else if(start.Definition == TokenKind.LeftParenthesis)
                        {
                            return BuildParenthesisGroup(start, last);
                        }

                        break;
                    }
            }

            return null;
        }
        #endregion

        #region Parenthesis Handling
        private Node BuildParenthesisGroup(Token start, Token last)
        {
            TokenPair body = TokenUtility.BuildGroupSimple(start, TokenKind.RightParenthesis);
            if(body.End == null) return Fail<ParenthesisGroupNode>(SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightParenthesis);
            if(body.End.PreviousNWS == body.Start) return Fail<ParenthesisGroupNode>(SyntaxFailureReason.ExpectedIdentifier, body.Start, TokenKind.Unknown);
            ParenthesisGroupNode result = new ParenthesisGroupNode();
            result.Declaration = body;
            result.Identifier = body;
            return result;
        }
        #endregion

        #region Method Handling
        private Node BuildMethodInvocation(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Token start, Token last)
        {
            if(start.NextNWS == null || start.NextNWS.Definition != TokenKind.LeftParenthesis) return Fail<Node>(SyntaxFailureReason.ExpectedEnterScope, start, TokenKind.LeftParenthesis);
            TokenPair body = TokenUtility.BuildGroupSimple(start.NextNWS, TokenKind.RightParenthesis);
            if(body.End == null) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, start.NextNWS, TokenKind.RightParenthesis);

            ParameterCollectionNode parameters = new ParameterCollectionNode();
            parameters.Identifier = start;
            parameters.Declaration = body;

            return new MethodInvocationNode() { Declaration = new TokenPair(start, body.End), Identifier = start, Parameters = parameters };
        }
        #endregion

        #region Transform Handling
        protected internal override Node[] TransformNodes(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Node[] nodes)
        {
            if(container is ParameterCollectionNode)
            {
                return PerformParameterTransform(builder, container, nodes);
            }
            else
            {
                int length = nodes.Length;
                if(length == 0) return new Node[0];
                return new Node[] { PerformStatementTransform(builder, nodes, 0, length) };
            }
        }

        private Node[] PerformParameterTransform(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node collection, Node[] nodes)
        {
            Token end = collection.Declaration.End;
            List<ParameterNode> parameters = new List<ParameterNode>();
            int index = 0;

            for(int i = 0; i < nodes.Length; i++)
                if(nodes[i].Declaration.Start.Definition == TokenKind.Comma)
                {
                    int count = i - index;
                    if(count == 0) return Fail<Node[]>(SyntaxFailureReason.ExpectedIdentifier, nodes[i].Declaration.Start, TokenKind.Unknown);

                    {
                        TokenPair decl = new TokenPair(nodes[index].Declaration.Start, nodes[index + count - 1].End);
                        Node statement = PerformStatementTransform(builder, nodes, index, count);
                        ParameterNode currentParameter = new ParameterNode() { Statement = statement, Declaration = decl };
                        currentParameter.Identifier = currentParameter.Declaration;
                        parameters.Add(currentParameter);
                    }

                    index = i + 1;
                }

            if(index != nodes.Length)
            {
                int count = nodes.Length - index;
                TokenPair decl = new TokenPair(nodes[index].Declaration.Start, nodes[index + count - 1].End);
                Node statement = PerformStatementTransform(builder, nodes, index, count);
                ParameterNode currentParameter = new ParameterNode() { Statement = statement, Declaration = decl };
                currentParameter.Identifier = currentParameter.Declaration;
                parameters.Add(currentParameter);
            }
            else if(end.PreviousNWS.Definition == TokenKind.Comma) return Fail<Node[]>(SyntaxFailureReason.ExpectedIdentifier, end.PreviousNWS, TokenKind.Unknown);

            return parameters.ToArray();
        }

        private Node PerformStatementTransform(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node[] nodes, int index, int length)
        {
            if(length == 0) return null;

            Node currentNode = null;
            BitVector availableLevels = new BitVector();
            int end = index + length;
            int defSerial = 0;
            int defPrecedence = -1;
            int maxAvailable = 0, minAvailable = 32;
            int totalNonTerminals = 0;

            if(!nodes[index].IsTerminal) return Fail<Node>(SyntaxFailureReason.ExpectedTerminal, nodes[index].Declaration.Start);

            for(int i = index + 1; i < end; i += 2)
            {
                if(!(currentNode = nodes[i]).IsTerminal)
                {
                    if(!_statementOperators[currentNode.Declaration.Start.Definition]) return Fail<Node>(SyntaxFailureReason.UnexpectedToken, currentNode.Declaration.Start);
                    defSerial = currentNode.Declaration.Start.Definition.Serial;
                    defPrecedence = _operatorPrecedenceLookup[defSerial];
                    availableLevels[defPrecedence] = true;
                    if(defPrecedence > maxAvailable) maxAvailable = defPrecedence;
                    if(defPrecedence < minAvailable) minAvailable = defPrecedence;
                    if(i + 1 >= end) return Fail<Node>(SyntaxFailureReason.ExpectedTerminal, currentNode.Declaration.Start);
                    if(!nodes[i + 1].IsTerminal) return Fail<Node>(SyntaxFailureReason.ExpectedTerminal, nodes[i + 1].Declaration.Start);
                    totalNonTerminals++;
                }
                else return Fail<Node>(SyntaxFailureReason.ExpectedNonTerminal, currentNode.Declaration.Start);
            }

            Node result = null;

            if(defPrecedence != -1)
            {
                for(int targetPrecedence = maxAvailable; targetPrecedence >= minAvailable; targetPrecedence--)
                {
                    if(!availableLevels[targetPrecedence]) continue;

                    for(int i = index + 1; i < end; i += 2)
                    {
                        if(!(currentNode = nodes[i]).IsTerminal)
                        {
                            defSerial = currentNode.Declaration.Start.Definition.Serial;
                            defPrecedence = _operatorPrecedenceLookup[defSerial];
                            if(defPrecedence != targetPrecedence) continue;
                            Node left = nodes[i - 1].TransientReference;
                            Node right = nodes[i + 1].TransientReference;

                            nodes[i] = currentNode = new BinaryOperatorNode(currentNode, left, right);
                            left.GetLeftMostNode().TransientReference = currentNode;
                            right.GetRightMostNode().TransientReference = currentNode;

                            if(--totalNonTerminals == 0)
                            {
                                result = currentNode;
                                targetPrecedence = -1;
                                break;
                            }
                        }
                    }
                }
            }
            else result = nodes[index];

            if(result == null) return Fail<Node>(SyntaxFailureReason.ExpectedTerminal, nodes[index].Declaration.Start);
            return result;
        }
        #endregion
    }

    public class PlaceholderOperatorNode : Node
    {
        public override bool IsTerminal { get { return false; } }

        public PlaceholderOperatorNode()
        {
        }
    }

    public class BinaryOperatorNode : Node
    {
        public Node Left;
        public Node Right;

        public override bool IsTerminal { get { return true; } }

        public BinaryOperatorNode()
        {
        }

        public BinaryOperatorNode(Node placeholderOperator, Node left, Node right)
        {
            Identifier = placeholderOperator.Identifier;
            Left = left;
            Right = right;
            Declaration = new TokenPair(Left.Declaration.Start, Right.End);
        }

        public override string GetEvaluatedValue()
        {
            if(EvaluatedValue == null) return GetRepresentationForEvaluation();
            return EvaluatedValue;
        }

        public override string GetRepresentationForEvaluation()
        {
            return Left.GetEvaluatedValue() + Identifier.Content + Right.GetEvaluatedValue();
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if(Left != null)
                if(Left.Declaration.Contains(sourceIndex))
                    return Left.GetNodeAt(sourceIndex);

            if(Right != null)
                if(Right.Declaration.Contains(sourceIndex))
                    return Right.GetNodeAt(sourceIndex);

            return this;
        }

        internal override Node GetLeftMostNode()
        {
            return Left.GetLeftMostNode();
        }

        internal override Node GetRightMostNode()
        {
            return Right.GetRightMostNode();
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if(Left != null) Left.CollectNodes(nodes);
            if(Right != null) Right.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);
            if(Left != null) Left.Visit(purpose, builder);
            if(Right != null) Right.Visit(purpose, builder);
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);
            if(Left != null) Left.AppendText(text, indent + 1);
            if(Right != null) Right.AppendText(text, indent + 1);
        }
    }

    public class ParenthesisGroupNode : CollectionNode
    {
        public Node Statement;

        public ParenthesisGroupNode()
        {
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if(Statement != null)
            {
                if(Statement.Declaration.Contains(sourceIndex)) return Statement.GetNodeAt(sourceIndex);
                return this;
            }
            else return base.GetNodeAt(sourceIndex);
        }

        public override string GetEvaluatedValue()
        {
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            if(EvaluatedValue != null) return TokenKind.LeftParenthesis.Identifier + EvaluatedValue + TokenKind.RightParenthesis.Identifier;

            if(Statement == null)
            {
                if(Items != null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(TokenKind.LeftParenthesis.Identifier);
                    for(int i = 0; i < Items.Length; i++) builder.Append(Items[0].GetEvaluatedValue());
                    builder.Append(TokenKind.RightParenthesis.Identifier);
                    return builder.ToString();
                }

                return TokenKind.LeftParenthesis.Identifier + TokenKind.RightParenthesis.Identifier;
            }

            return TokenKind.LeftParenthesis.Identifier + Statement.GetEvaluatedValue() + TokenKind.RightParenthesis.Identifier;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            if(Statement != null)
            {
                nodes.Add(this);
                Statement.CollectNodes(nodes);
            }
            else base.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);
            if(Statement != null) Statement.Visit(purpose, builder);
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);
            if(Statement != null) Statement.AppendText(text, indent + 1);
        }
    }

    public class ParameterNode : Node
    {
        public Node Statement;

        public ParameterNode()
        {
        }

        public override string GetEvaluatedValue()
        {
            return Statement.GetEvaluatedValue();
        }

        public override string GetRepresentationForEvaluation()
        {
            return Statement.GetEvaluatedValue();
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if(Statement != null)
                if(Statement.Declaration.Contains(sourceIndex))
                    return Statement.GetNodeAt(sourceIndex);

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if(Statement != null) Statement.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);
            if(Statement != null) Statement.Visit(purpose, builder);
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);
            if(Statement != null) Statement.AppendText(text, indent + 1);
        }
    }

    public class ParameterCollectionNode : CollectionNode
    {
        public ParameterCollectionNode()
        {
        }

        public override string GetEvaluatedValue()
        {
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            if(Items != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(TokenKind.LeftParenthesis.Identifier);

                for(int i = 0; i < Items.Length; i++)
                {
                    if(i > 0) builder.Append(TokenKind.Comma.Identifier);
                    builder.Append(Items[i].GetEvaluatedValue());
                }

                builder.Append(TokenKind.RightParenthesis.Identifier);

                return builder.ToString();
            }

            return TokenKind.LeftParenthesis.Identifier + TokenKind.RightParenthesis.Identifier;
        }
    }

    public class MethodInvocationNode : Node
    {
        public ParameterCollectionNode Parameters;

        public MethodInvocationNode()
        {
        }

        public MethodInvocationNode(Token token)
            : base(token)
        {
        }

        public override string GetEvaluatedValue()
        {
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            return Identifier.Content + Parameters.GetEvaluatedValue();
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if(Parameters != null)
                if(Parameters.Declaration.Contains(sourceIndex))
                    return Parameters.GetNodeAt(sourceIndex);

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if(Parameters != null) Parameters.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);
            if(Parameters != null) Parameters.Visit(purpose, builder);
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);
            if(Parameters != null) Parameters.AppendText(text, indent + 1);
        }
    }
}
