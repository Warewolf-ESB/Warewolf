
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.SyntaxAnalysis;
using System.Diagnostics;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
    public class JSONGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
    {
        #region Constants
        private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.None;// ASTGrammerBehaviour.DoesPrependTokens | ASTGrammerBehaviour.DoesPostProcessTokens | ASTGrammerBehaviour.DoesAppendTokens;
        #endregion

        #region Readonly Members
        public static readonly GrammerGroup JSONGrammerGroup = new GrammerGroup("JSON");
        #endregion

        #region Public Properties
        public override GrammerGroup GrammerGroup { get { return JSONGrammerGroup; } }
        public override ASTGrammerBehaviour Behaviour { get { return _behaviourFlags; } }
        #endregion

        #region Constructor
        public JSONGrammer()
        {

        }
        #endregion

        #region Registration Handling
        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.LeftCurlyBracket);
        }

        protected internal override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);

            List<TokenKind> operators = new List<TokenKind>();

            operators.Add(TokenKind.LeftCurlyBracket);
            operators.Add(TokenKind.RightCurlyBracket);
            operators.Add(TokenKind.Comma);
            operators.Add(TokenKind.Colon);

            tokenizer.Handlers.Add(new Tokenization.UnaryTokenizationHandler<Token, TokenKind>(operators));
        }
        #endregion

        #region Build Handling
        protected internal override JSONNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            if (start.Definition != TokenKind.LeftCurlyBracket) return Fail<JSONNode>(RuntimeFailureReason.ArgumentException, "start");
            List<JSONNode> allNodes = new List<JSONNode>();
            Token bodyStart = start;

            while (start != null)
            {
                if ((start = start.NextNWS) == null) return Fail<JSONNode>(SyntaxFailureReason.ExpectedExitScope, bodyStart, TokenKind.RightCurlyBracket, TokenKind.RegularString);
                if (start.Definition == TokenKind.RightCurlyBracket) break;
                JSONNode node = BuildJSONEntryNode(builder, container, start, last);
                if (node == null) return null;
                if ((start = node.End) == null) return Fail<JSONNode>(SyntaxFailureReason.ExpectedExitScope, bodyStart, TokenKind.RightCurlyBracket, TokenKind.RegularString);
                allNodes.Add(node);
                if (start.Definition == TokenKind.RightCurlyBracket) break;
            }

            return new JSONRoot(new TokenPair(bodyStart, start), allNodes.ToArray());
        }

        private JSONNode BuildJSONEntryNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            if (start == null || !start.Definition.IsStringLiteral) return Fail<JSONNode>(SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightCurlyBracket, TokenKind.RegularString);

            Token identifier = start;
            if ((start = start.NextNWS) == null || start.Definition != TokenKind.Colon) return Fail<JSONNode>(SyntaxFailureReason.ExpectedTerminal, identifier, TokenKind.Colon);

            Token colon = start;

            if ((start = start.NextNWS) == null) return Fail<JSONNode>(SyntaxFailureReason.ExpectedTerminal, identifier, TokenKind.LeftCurlyBracket, TokenKind.RegularString);

            if (start.Definition.IsStringLiteral)
            {
                return new JSONScalar(identifier, start);
            }
            else if (start.Definition == TokenKind.LeftCurlyBracket)
            {
                List<JSONNode> allNodes = new List<JSONNode>();

                while (start != null)
                {
                    if ((start = start.NextNWS) == null) return Fail<JSONNode>(SyntaxFailureReason.ExpectedExitScope, colon, TokenKind.RightCurlyBracket, TokenKind.RegularString);
                    if (start.Definition == TokenKind.RightCurlyBracket) break;
                    JSONNode node = BuildJSONEntryNode(builder, container, start, last);
                    if (node == null) return null;
                    if ((start = node.End) == null) return Fail<JSONNode>(SyntaxFailureReason.ExpectedExitScope, colon, TokenKind.RightCurlyBracket, TokenKind.RegularString);
                    allNodes.Add(node);
                    if (start.Definition == TokenKind.RightCurlyBracket) break;
                }

                return new JSONComposite(identifier, start) { Items = allNodes.ToArray() };
            }

            return Fail<JSONNode>(SyntaxFailureReason.ExpectedTerminal, identifier, TokenKind.LeftCurlyBracket, TokenKind.RegularString);
        }
        #endregion
    }

    public class JSONScalar : JSONNode
    {
        private Token _value;

        public Token Value { get { return _value; } }

        public JSONScalar(Token identifier, Token value)
        {
            Declaration = new TokenPair(identifier, value);
            Identifier = new TokenPair(identifier);
            _value = value;
        }
    }

    public class JSONRoot : CollectionNode
    {
        public JSONRoot(TokenPair declaration, JSONNode[] items)
        {
            Declaration = declaration;
            Identifier = new TokenPair(declaration.Start);
            Items = items;
        }
    }

    public class JSONComposite : CollectionNode
    {
        public JSONComposite(Token identifier, Token end)
        {
            Declaration = new TokenPair(identifier, end);
            Identifier = new TokenPair(identifier);
        }
    }
}
