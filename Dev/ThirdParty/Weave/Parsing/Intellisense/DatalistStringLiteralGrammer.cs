
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
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
    public class DatalistStringLiteralGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
    {
        #region Constants
        private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPostProcessTokens;
        #endregion

        #region Instance Fields
        private DatalistGrammer _sourceGrammer;
        private IntellisenseTokenizer _tokenizer;
        #endregion

        #region Public Properties
        public override GrammerGroup GrammerGroup { get { return GrammerGroup.StringLiteralGrammers; } }
        public override ASTGrammerBehaviour Behaviour { get { return _behaviourFlags; } }
        #endregion

        #region Constructor
        internal DatalistStringLiteralGrammer(DatalistGrammer sourceGrammer)
        {
            _sourceGrammer = sourceGrammer;
        }
        #endregion

        #region Configuration Handling
        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.RegularString);
            triggerRegistry.Register(TokenKind.VerbatimString);
            triggerRegistry.Register(TokenKind.CompositeVerbatimString);
            triggerRegistry.Register(TokenKind.CompositeRegularString);
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);
            tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
        }
        #endregion

        #region Post Process Handling
        protected internal override void PostProcessTokens(Tokenizer<Token, TokenKind> tokenizer, IList<Token> rawTokens, int phase)
        {
            if (phase != 0) return;
            ExposedList<Token> tokens = rawTokens as ExposedList<Token>;
            bool changed = false;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token current = tokens[i];
                TokenKind definition = current.Definition;

                // check if token is literal string
                if (definition.IsStringLiteral)
                {
                    int index = current.SourceIndex + 1;
                    int innerLength = current.SourceLength - 2;

                    if (current.Definition == TokenKind.VerbatimString)
                    {
                        index++;
                        innerLength--;
                    }

                    if (innerLength > 0 && current.Source.IndexOf(TokenKind.OpenDL.Identifier, index, innerLength, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        if (_tokenizer == null)
                        {
                            _tokenizer = new IntellisenseTokenizer();

                            List<TokenKind> unaryTokens = new List<TokenKind>();
                            unaryTokens.Add(TokenKind.OpenDL);
                            unaryTokens.Add(TokenKind.CloseDL);
                            UnaryTokenizationHandler<Token, TokenKind> unary = new UnaryTokenizationHandler<Token, TokenKind>(unaryTokens);

                            _tokenizer.Handlers.Add(unary);
                        }

                        Token[] nestedTokens = _tokenizer.Tokenize(current.Source, index, innerLength, false);
                        if (nestedTokens == null) continue;
                        if (BuildDatalistRegions(tokenizer, nestedTokens, tokens, ref i)) changed = true;
                    }
                }
            }

            if (changed)
            {
                Token[] underlying = tokens.UnderlyingArray;
                int length = tokens.Count;

                Token previous = null;
                Token previousNWS = null;
                Token previousWS = null;

                for (int i = 0; i < underlying.Length; i++)
                {
                    if (i >= length) break;
                    Token current = underlying[i];
                    current.TokenIndex = i;

                    current.Previous = previous;
                    current.PreviousNWS = previousNWS;
                    current.PreviousWS = previousWS;

                    if (previous != null) previous.Next = current;

                    if (current.Definition.IsWhitespace)
                    {
                        for (int k = previousWS == null ? 0 : previousWS.TokenIndex; k < i; k++) underlying[k].NextWS = current;
                        previousWS = current;
                    }
                    else
                    {
                        for (int k = previousNWS == null ? 0 : previousNWS.TokenIndex; k < i; k++) underlying[k].NextNWS = current;
                        previousNWS = current;
                    }

                    previous = current;
                }
            }
        }

        private bool BuildDatalistRegions(Tokenizer<Token, TokenKind> primaryTokenizer, Token[] regions, ExposedList<Token> rawTokens, ref int outerPosition)
        {
            if (regions.Length == 0) return false;
            bool insertedOuter = false;

            for (Token current = regions[0]; current != null && !current.Definition.IsEndOfFile; current = current.NextNWS)
            {
                if (current.Definition == TokenKind.OpenDL)
                {
                    TokenPair group = TokenUtility.BuildGroupSimple(current, TokenKind.CloseDL);
                    if (group.End == null || group.End.Definition != TokenKind.CloseDL) return insertedOuter;
                    Token[] primaryInner = primaryTokenizer.Tokenize(current.Source, current.SourceIndex, group.End.SourceLength + group.End.SourceIndex - current.SourceIndex, false);

                    if (primaryInner == null)
                    {
                        current = group.End;
                        continue;
                    }

                    if (!insertedOuter)
                    {  
                        Token outerCurrent = rawTokens[outerPosition];

                        Token innerLast = primaryInner[primaryInner.Length - 1];
                        innerLast.Definition = TokenKind.CompositeStringClosure;
                        innerLast.Source = outerCurrent.Source;
                        innerLast.SourceIndex = outerCurrent.SourceIndex + outerCurrent.SourceLength - 1;
                        innerLast.SourceLength = 1;

                        if (outerCurrent.Definition == TokenKind.RegularString)
                        {
                            outerCurrent.Definition = TokenKind.CompositeRegularString;
                            outerCurrent.SourceLength = 1;
                        }
                        else if (outerCurrent.Definition == TokenKind.VerbatimString)
                        {
                            outerCurrent.Definition = TokenKind.CompositeVerbatimString;
                            outerCurrent.SourceLength = 2;
                        }
                        else throw new InvalidOperationException("outerCurrent is not a string literal.");

                        rawTokens.InsertRange(outerPosition + 1, primaryInner);
                        insertedOuter = true;
                        outerPosition += primaryInner.Length;
                    }
                    else
                    {
                        rawTokens.InsertRange(outerPosition + 1, primaryInner, 0, primaryInner.Length - 1);
                        outerPosition += primaryInner.Length - 1;
                    }

                    current = group.End;
                }
            }

            return insertedOuter;
        }
        #endregion

        #region Build Handling
        protected internal override Node BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Token start, Token last)
        {
            if (start.Definition == TokenKind.RegularString || start.Definition == TokenKind.VerbatimString) return new LiteralNode(start);
            else
            {
                TokenPair body = TokenUtility.BuildGroupSimple(start, TokenKind.CompositeStringClosure, start.Definition == TokenKind.CompositeVerbatimString ? TokenKind.CompositeRegularString : TokenKind.CompositeVerbatimString);
                if (body.End == null) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, start, TokenKind.CompositeRegularString);

                CompositeStringLiteralNode node = new CompositeStringLiteralNode();
                node.Declaration = body;
                node.Identifier = start;
                return node;
            }
        }
        #endregion
    }

    public class CompositeStringLiteralNode : CollectionNode
    {
        public CompositeStringLiteralNode()
        {
        }

        public override string GetEvaluatedValue()
        {
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            bool verbatim = Declaration.Start.Definition == TokenKind.CompositeVerbatimString;

            if (Items != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("\"");

                int start = Declaration.Start.SourceIndex + Declaration.Start.SourceLength;
                Node current;
                
                for (int i = 0; i < Items.Length; i++)
                {
                    current = Items[i];
                    int subStart = current.Declaration.Start.SourceIndex;

                    if (subStart > start)
                    {
                        if (verbatim) builder.Append(StringUtility.GetUnescapedVerbatimString(Declaration.Start.Source.Substring(start, subStart - start)));
                        else builder.Append(StringUtility.GetUnescapedRegularString(Declaration.Start.Source.Substring(start, subStart - start)));
                    }

                    builder.Append(Items[i].GetEvaluatedValue());
                    start = current.Declaration.End.SourceIndex + current.Declaration.End.SourceLength;
                }

                int end = Declaration.End.SourceIndex;
                if (end > start)
                {
                    if (verbatim) builder.Append(StringUtility.GetUnescapedVerbatimString(Declaration.Start.Source.Substring(start, end - start)));
                    else builder.Append(StringUtility.GetUnescapedRegularString(Declaration.Start.Source.Substring(start, end - start)));
                }
                builder.Append("\"");

                return builder.ToString();
            }

            if (verbatim) return "\"" + StringUtility.GetUnescapedVerbatimString(Declaration.Start.Source.Substring(Declaration.Start.SourceIndex + Declaration.Start.SourceLength, Declaration.End.SourceIndex - (Declaration.Start.SourceIndex + Declaration.Start.SourceLength))) + "\"";
            return "\"" + StringUtility.GetUnescapedRegularString(Declaration.Start.Source.Substring(Declaration.Start.SourceIndex + Declaration.Start.SourceLength, Declaration.End.SourceIndex - (Declaration.Start.SourceIndex + Declaration.Start.SourceLength))) + "\"";
        }
    }
}
