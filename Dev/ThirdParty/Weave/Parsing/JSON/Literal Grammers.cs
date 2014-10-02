
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

namespace System.Parsing.JSON
{
    #region BooleanLiteralGrammer
    public class BooleanLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
    {
        public override GrammerGroup GrammerGroup { get { return GrammerGroup.BooleanLiteralGrammers; } }

        protected internal override JSONNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            return new LiteralNode(start);
        }

        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.BooleanTrue);
            triggerRegistry.Register(TokenKind.BooleanFalse);
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);
            tokenizer.Keywords.Add(TokenKind.BooleanFalse.Identifier, TokenKind.BooleanFalse);
            tokenizer.Keywords.Add(TokenKind.BooleanTrue.Identifier, TokenKind.BooleanTrue);
        }
    }
    #endregion

    #region CharLiteralGrammer
    public class CharLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
    {
        public override GrammerGroup GrammerGroup { get { return GrammerGroup.CharLiteralGrammers; } }

        protected internal override JSONNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            return new LiteralNode(start);
        }

        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.EscapedChar);
            triggerRegistry.Register(TokenKind.HexadecimalChar);
            triggerRegistry.Register(TokenKind.RegularChar);
            triggerRegistry.Register(TokenKind.UnicodeChar);
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);
            tokenizer.Handlers.Add(new CharacterLiteralTokenizationHandler());
        }
    }
    #endregion

    #region StringLiteralGrammer
    public class StringLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
    {
        public override GrammerGroup GrammerGroup { get { return GrammerGroup.StringLiteralGrammers; } }

        protected internal override JSONNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            return new LiteralNode(start);
        }

        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.RegularString);
            triggerRegistry.Register(TokenKind.VerbatimString);
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);
            tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
        }
    }
    #endregion

    #region CSharpNumericLiteralGrammer
    public class CSharpNumericLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, JSONNode>
    {
        public override GrammerGroup GrammerGroup { get { return GrammerGroup.NumericLiteralGrammers; } }

        protected internal override JSONNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder, JSONNode container, Token start, Token last)
        {
            return new LiteralNode(start);
        }

        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.IntegerHexadecimal);
            triggerRegistry.Register(TokenKind.IntegerNoSuffix);
            triggerRegistry.Register(TokenKind.IntegerSuffixL);
            triggerRegistry.Register(TokenKind.IntegerSuffixU);
            triggerRegistry.Register(TokenKind.IntegerSuffixUL);

            triggerRegistry.Register(TokenKind.RealNoSuffix);
            triggerRegistry.Register(TokenKind.RealSuffixD);
            triggerRegistry.Register(TokenKind.RealSuffixF);
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);
            tokenizer.Handlers.Add(new CSharpNumericLiteralTokenizationHandler());
        }
    }
    #endregion
}
