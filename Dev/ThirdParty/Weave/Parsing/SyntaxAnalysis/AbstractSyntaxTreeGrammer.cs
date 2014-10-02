
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.Tokenization;
using System.Diagnostics;

namespace System.Parsing.SyntaxAnalysis
{
    public abstract class AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>
        where Token : Token<Token, TokenKind>, new()
        where TokenKind : TokenDefinition
        where ASTNode : ASTNode<Token, TokenKind, ASTNode>
    {
        #region Instance Fields
        private string _name;
        private int _registryIndex;
        private bool _suppressFailure;
        private AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> _activeBuilder;
        #endregion

        #region Internal Properties
        internal int RegistryIndex { get { return _registryIndex; } set { _registryIndex = value; } }
        #endregion

        #region Public Properties
        public string Name { get { return _name; } }
        /// <summary>
        /// Gets a GrammerGroup that represents the group this grammer belongs too. The
        /// AbstractSyntaxTreeBuilder will not allow more than one grammer from any particular
        /// group to be registered.
        /// </summary>
        public abstract GrammerGroup GrammerGroup { get; }
        public virtual ASTGrammerBehaviour Behaviour { get { return ASTGrammerBehaviour.None; } }
        #endregion

        #region Constructor
        protected AbstractSyntaxTreeGrammer()
        {
            _name = GetType().Name;
            _registryIndex = -1;
        }
        #endregion

        #region Token Handling
        protected internal virtual void PrependTokens(ITokenBuilder builder) { }
        protected internal virtual void AppendTokens(ITokenBuilder builder) { }
        protected internal virtual void PostProcessTokens(Tokenizer<Token,TokenKind> tokenizer, IList<Token> tokens, int phase) { }
        #endregion

        #region Build Handling
        public ASTNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder, ASTNode container, Token start, Token last, bool suppressFailure)
        {
            AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> previousActiveBuilder = _activeBuilder;
            bool previousSuppressFailure = _suppressFailure;
            ASTNode result = null;

            {
                _suppressFailure = suppressFailure;
                _activeBuilder = builder;
                result = BuildNode(builder, container, start, last);
            }

            _suppressFailure = previousSuppressFailure;
            _activeBuilder = previousActiveBuilder;
            return result;
        }

        protected internal abstract ASTNode BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder, ASTNode container, Token start, Token last);
        #endregion

        #region Transform Handling
        public ASTNode[] TransformNodes(AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder, ASTNode container, ASTNode[] nodes, bool suppressFailure)
        {
            AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> previousActiveBuilder = _activeBuilder;
            bool previousSuppressFailure = _suppressFailure;
            ASTNode[] result = null;

            {
                _suppressFailure = suppressFailure;
                _activeBuilder = builder;
                result = TransformNodes(builder, container, nodes);
            }

            _suppressFailure = previousSuppressFailure;
            _activeBuilder = previousActiveBuilder;
            return result;
        }
        
        protected internal virtual ASTNode[] TransformNodes(AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder, ASTNode container, ASTNode[] nodes) { throw new NotSupportedException(); }
        #endregion

        #region Failure Handling
        protected T Fail<T>(SyntaxFailureReason reason, Token errorStart)
        {
            if (!_suppressFailure)
            {
                ParseEventLogEntry entry = new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), null, (int)reason, 0, GrammerGroup.DisplayName, _name);
                _activeBuilder.EventLog.Log(entry);
            }

            return default(T);
        }

        protected T Fail<T>(SyntaxFailureReason reason, Token errorLocation, TokenKind arg1)
        {
            if (!_suppressFailure)
            {
                ParseEventLogEntry entry = new ParseEventLogEntry(errorLocation.Source, errorLocation.ToParseEventLogToken(), arg1, (int)reason, 0, GrammerGroup.DisplayName, _name);
                _activeBuilder.EventLog.Log(entry);
            }
            
            return default(T);
        }

        protected T Fail<T>(SyntaxFailureReason reason, Token errorLocation, TokenKind arg1, TokenKind arg2)
        {
            if (!_suppressFailure)
            {
                ParseEventLogEntry entry = new ParseEventLogEntry(errorLocation.Source, errorLocation.ToParseEventLogToken(), arg1, arg2, (int)reason, 0, GrammerGroup.DisplayName, _name);
                _activeBuilder.EventLog.Log(entry);
            }

            return default(T);
        }

        protected T Fail<T>(SyntaxFailureReason reason, Token errorStart, Token errorEnd, TokenKind arg1)
        {
            if (!_suppressFailure)
            {
                ParseEventLogEntry entry = new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), errorEnd.ToParseEventLogToken(), arg1, null, (int)reason, 0, GrammerGroup.DisplayName, _name);
                _activeBuilder.EventLog.Log(entry);
            }

            return default(T);
        }

        protected T Fail<T>(SyntaxFailureReason reason, Token errorStart, Token errorEnd, TokenKind arg1, TokenKind arg2)
        {
            if (!_suppressFailure)
            {
                ParseEventLogEntry entry = new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), errorEnd.ToParseEventLogToken(), arg1, arg2, (int)reason, 0, GrammerGroup.DisplayName, _name);
                _activeBuilder.EventLog.Log(entry);
            }

            return default(T);
        }

        [DebuggerStepThrough]
        protected T Fail<T>(RuntimeFailureReason reason, string arg)
        {
            switch (reason)
            {
                case RuntimeFailureReason.Undefined: throw new Exception(arg);
                case RuntimeFailureReason.ArgumentNullException: throw new ArgumentNullException(arg);
                case RuntimeFailureReason.ArgumentException: throw new ArgumentException(arg);
            }

            return default(T);
        }

        [DebuggerStepThrough]
        protected T Fail<T>(RuntimeFailureReason reason, string arg1, string arg2)
        {
            switch (reason)
            {
                case RuntimeFailureReason.Undefined: throw new Exception(arg1);
                case RuntimeFailureReason.ArgumentNullException: throw new ArgumentNullException(arg1, arg2);
                case RuntimeFailureReason.ArgumentException: throw new ArgumentException(arg2, arg1);
            }

            return default(T);
        }
        #endregion

        #region Configuration Handling
        protected internal virtual void OnRegisterSubGrammers(HashSet<GrammerGroup> existingGrammers, AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder)
        {
        }

        protected internal abstract void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry);

        protected internal virtual void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
        {
        }
        #endregion

        #region RuntimeFailureReason
        /// <summary>
        /// Specifies common failure reasons that occur at runtime and result in an unhandled exception
        /// being thrown.
        /// </summary>
        protected enum RuntimeFailureReason
        {
            Undefined = 0,
            ArgumentNullException = 1,
            ArgumentException = 2
        }
        #endregion

        #region SyntaxFailureReason
        /// <summary>
        /// Specifies common failure reasons that occur at runtime and result in a BuildNode failure
        /// and a parse error log.
        /// </summary>
        protected enum SyntaxFailureReason
        {
            Undefined = 0,
            ExpectedEnterScope = 1,
            ExpectedExitScope = 2,
            ExpectedToken = 3,
            ExpectedIdentifier = 4,
            UnexpectedToken = 5,
            ExpectedTerminal = 6,
            ExpectedNonTerminal = 7
        }
        #endregion
    }

    [Flags]
    public enum ASTGrammerBehaviour
    {
        None = 0,
        DoesPrependTokens = 1,
        DoesAppendTokens = 2,
        DoesPostProcessTokens = 4
    }
}
