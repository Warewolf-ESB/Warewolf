
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

namespace System.Parsing.Intellisense
{
    public class ExclusionGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
    {
        #region Constants
        private const ASTGrammerBehaviour _behaviourFlags = ASTGrammerBehaviour.DoesPostProcessTokens;
        #endregion

        #region Readonly Members
        public static readonly GrammerGroup ExclusionGrammerGroup = new GrammerGroup("Exclusion");
        #endregion

        #region Instance Fields
        private HashSet<TokenKind> _exclusions;
        private TokenDefinitionMask _exclusionMask;
        #endregion

        #region Public Properties
        public override GrammerGroup GrammerGroup { get { return ExclusionGrammerGroup; } }
        public override ASTGrammerBehaviour Behaviour { get { return _behaviourFlags; } }
        public HashSet<TokenKind> Exclusions { get { return _exclusions; } }
        #endregion

        #region Constructor
        public ExclusionGrammer()
        {
            _exclusions = new HashSet<TokenKind>();
        }
        #endregion

        #region Configuration Handling
        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);

            TokenKind[] exclusions = _exclusions.ToArray();
            tokenizer.Handlers.Add(new Tokenization.UnaryTokenizationHandler<Token, TokenKind>(exclusions));
            _exclusionMask = new TokenDefinitionMask(exclusions);
        }
        #endregion

        #region Postprocess Handling
        protected internal override void PostProcessTokens(Tokenizer<Token, TokenKind> tokenizer, IList<Token> rawTokens, int phase)
        {
            if (phase != 0) return;
            ExposedList<Token> tokens = rawTokens as ExposedList<Token>;

            // get underlying array of tokens and number of elements used
            Token[] underlying = tokens.UnderlyingArray;
            int length = tokens.Count;
            int destinationIndex = 0;
            Token previous = null, previousNWS = null, previousWS = null;

            for (int k = 0; k < length; k++)
            {
                Token token = underlying[k];
                TokenKind definition = token.Definition;

                if (!_exclusionMask[definition])
                {
                    token.TokenIndex = destinationIndex;
                    token.Previous = previous;
                    token.PreviousNWS = previousNWS;
                    token.PreviousWS = previousWS;

                    if (previous != null) previous.Next = token;
                    int count = token.TokenIndex;

                    if (token.Definition.IsWhitespace)
                    {
                        for (int i = previousWS == null ? 0 : previousWS.TokenIndex; i < count; i++) underlying[i].NextWS = token;
                        previousWS = token;
                    }
                    else
                    {
                        for (int i = previousNWS == null ? 0 : previousNWS.TokenIndex; i < count; i++) underlying[i].NextNWS = token;
                        previousNWS = token;
                    }

                    previous = token;
                    underlying[destinationIndex++] = token;
                }
            }

            if (destinationIndex == length) return;
            tokens.Count = destinationIndex;
        }
        #endregion

        #region Build Handling
        protected internal override Node BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Token start, Token last)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
