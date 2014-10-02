
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

namespace System.Parsing.Tokenization
{
    /// <summary>
    /// Transforms an input string into a sequence of <typeparamref name="T"/> of definition <typeparamref name="K"/>
    /// </summary>
    /// <typeparam name="T">The type of Token used by this tokenizer.</typeparam>
    /// <typeparam name="K">The type of TokenDefinition used by this tokenizer.</typeparam>
    public class Tokenizer<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        #region Instance Fields
        private RequiredTokenDefinitions<K> _requiredDefinitions;
        private List<TokenizationHandler<T, K>> _handlers;
        private StringValueCollection<K> _keywords;
        private ParseEventLog _eventLog;
        #endregion

        #region Internal Properties
        internal StringValueCollection<K> RawKeywords { get { return _keywords; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns a reference to the required definitions object that was given to this tokenizer during construction.
        /// </summary>
        public RequiredTokenDefinitions<K> RequiredDefinitions { get { return _requiredDefinitions; } }
        /// <summary>
        /// Returns a list of tokenization handlers that have been registered with this tokenizer. You can operate on this list
        /// to add or remove tokenization handlers.
        /// </summary>
        public List<TokenizationHandler<T, K>> Handlers { get { return _handlers; } }
        /// <summary>
        /// Returns a dictionary of keywords registered with this tokenizer. You can operate on this dictionary
        /// to add or remove keywords.
        /// </summary>
        public IDictionary<string, K> Keywords { get { return _keywords; } }
        /// <summary>
        /// Returns a reference to the ParseEventLog that was given to this tokenizer during construction.
        /// </summary>
        public ParseEventLog EventLog { get { return _eventLog; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a tokenizer.
        /// </summary>
        /// <param name="requiredDefinitions">A reference to a RequiredTokenDefinitions object that specifies common token definitions.</param>
        /// <param name="eventLog">A reference to a ParseEventLog object that handles tokenization event logging.</param>
        public Tokenizer(RequiredTokenDefinitions<K> requiredDefinitions, ParseEventLog eventLog)
        {
            if (requiredDefinitions == null) throw new ArgumentNullException("requiredDefinitions");
            if (eventLog == null) throw new ArgumentNullException("eventLog");

            _requiredDefinitions = requiredDefinitions;
            _handlers = new List<TokenizationHandler<T, K>>();
            _keywords = new StringValueCollection<K>();
            _keywords.SetNullValue(null);
            _eventLog = eventLog;
        }

        /// <summary>
        /// Constructs a tokenizer, using ParseEventLog.NullEventLog as the event logger.
        /// </summary>
        /// <param name="requiredDefinitions">A reference to a RequiredTokenDefinitions object that specifies common token definitions.</param>
        public Tokenizer(RequiredTokenDefinitions<K> requiredDefinitions)
            : this(requiredDefinitions, ParseEventLog.NullEventLog)
        {
        }
        #endregion

        #region Tokenization Handling
                /// <summary>
        /// Transforms the <paramref name="source"/> string into a sequence of tokens.
        /// </summary>
        /// <param name="source">The input string to tokenize</param>
        /// <returns>A sequence of Tokens</returns>
        public T[] Tokenize(string source)
        {
            return Tokenize(source, false);
        }

        /// <summary>
        /// Transforms the <paramref name="source"/> string into a sequence of tokens.
        /// </summary>
        /// <param name="source">The input string to tokenize</param>
        /// <param name="expectPartialToken">Whether or not partially formed tokens are to be expected, and handled such that
        /// the tokenization process does not fail.</param>
        /// <returns>A sequence of Tokens</returns>
        public T[] Tokenize(string source, bool expectPartialToken)
        {
            if (String.IsNullOrEmpty(source))
            {
                T token = new T();
                token.Source = source;
                token.TokenIndex = 0;
                token.SourceIndex = 0;
                token.SourceLength = 0;
                token.Definition = _requiredDefinitions.EndOfFile;
                return new T[] { token };
            }

            TokenizationExecutionStore<T, K> executionStore = new TokenizationExecutionStore<T, K>(this, source, expectPartialToken);
            bool success = TokenizeCore(source, executionStore);
            return PostProcessTokensCore(executionStore, success);
        }

        /// <summary>
        /// Transforms the <paramref name="source"/> string into a sequence of tokens.
        /// </summary>
        /// <param name="source">The input string to tokenize</param>
        /// <param name="index">The index of the first character in the input string to tokenize</param>
        /// <param name="length">The number of characters in the input string to tokenize</param>
        /// <param name="expectPartialToken">Whether or not partially formed tokens are to be expected, and handled such that
        /// the tokenization process does not fail.</param>
        /// <returns>A sequence of Tokens</returns>
        public T[] Tokenize(string source, int index, int length, bool expectPartialToken)
        {
            if (String.IsNullOrEmpty(source) || length <= 0)
            {
                T token = new T();
                token.Source = source;
                token.TokenIndex = 0;
                token.SourceIndex = 0;
                token.SourceLength = 0;
                token.Definition = _requiredDefinitions.EndOfFile;
                return new T[] { token };
            }

            TokenizationExecutionStore<T, K> executionStore = new TokenizationExecutionStore<T, K>(this, source, index, length, expectPartialToken);
            bool success = TokenizeCore(source, executionStore);
            return PostProcessTokensCore(executionStore, success);
        }

        internal T[] PostProcessTokensCore(TokenizationExecutionStore<T, K> executionStore, bool success)
        {
            if (success)
            {
                PostProcessTokens(executionStore.Builder.Contents);
                executionStore.Builder.Append(executionStore.Source.Length - 1, 0, _requiredDefinitions.EndOfFile);
            }

            T[] tokens = success ? executionStore.Builder.Contents.ToArray() : null;
            return tokens;
        }

        internal bool TokenizeCore(string source, TokenizationExecutionStore<T, K> executionStore)
        {
            for (int i = 0; i < _handlers.Count; i++) _handlers[i].Reset(executionStore);
            bool failed = false;

            while (executionStore.Input.BeginAdvance())
            {
                K result = null;

                for (int i = 0; i < _handlers.Count; i++)
                    if (_handlers[i].IdentifyToken(this, executionStore, ref result))
                        break;

                if (!executionStore.Input.EndAdvance(result))
                {
                    failed = true;
                    break;
                }
            }

            return !failed;
        }
        
        /// <summary>
        /// Any post processing logic that needs to operate on the entire stream of tokens can be implemented here.
        /// </summary>
        /// <param name="tokens">The output stream of tokens generated by the tokenization process.</param>
        protected virtual void PostProcessTokens(IList<T> tokens) { }
        #endregion
    }

    /// <summary>
    /// A state object that is created on a per tokenization request basis. Allows
    /// multiple concurrent tokenization requests to be processed without any
    /// need for derivitive thread safety considerations to be handled.
    /// </summary>
    /// <typeparam name="T">The type of the token used by the Tokenizer.</typeparam>
    /// <typeparam name="K">The type of the token definition used by the Tokenizer.</typeparam>
    public sealed class TokenizationExecutionStore<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        #region Instance Fields
        private Tokenizer<T, K> _tokenizer;
        private TokenBuilder<T, K> _builder;
        private TokenizerInputWalker<T, K> _input;
        private TokenizationHandlerExecutionStore[] _handlerStates;
        private string _source;
        private bool _expectPartialToken;
        #endregion

        #region Internal Properties
        internal TokenBuilder<T, K> Builder { get { return _builder; } }
        internal TokenizerInputWalker<T, K> Input { get { return _input; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// The tokenizer that created this object.
        /// </summary>
        public Tokenizer<T, K> Tokenizer { get { return _tokenizer; } }
        /// <summary>
        /// The source string that is being tokenized.
        /// </summary>
        public string Source { get { return _source; } }
        /// <summary>
        /// Whether or not tokenization handlers should expect a token to be partially formed.
        /// </summary>
        public bool ExpectPartialToken { get { return _expectPartialToken; } set { _expectPartialToken = value; } }
        #endregion

        #region Constructor
        internal TokenizationExecutionStore(Tokenizer<T, K> tokenizer, string source, bool expectPartialToken)
        {
            _tokenizer = tokenizer;
            _source = source;
            _expectPartialToken = expectPartialToken;
            _handlerStates = new TokenizationHandlerExecutionStore[_tokenizer.Handlers.Count];

            _builder = new TokenBuilder<T, K>(source);
            _input = new TokenizerInputWalker<T, K>(this);
            _input.Reset(source);
        }

        internal TokenizationExecutionStore(Tokenizer<T, K> tokenizer, string source, int index, int length, bool expectPartialToken)
        {
            _tokenizer = tokenizer;
            _source = source;
            _expectPartialToken = expectPartialToken;
            _handlerStates = new TokenizationHandlerExecutionStore[_tokenizer.Handlers.Count];

            _builder = new TokenBuilder<T, K>(source);
            _input = new TokenizerInputWalker<T, K>(this);
            _input.Reset(source, index, length);
        }
        #endregion

        #region [Get/Set] Handling
        /// <summary>
        /// Persists the provided <paramref name="store"/> for the given <paramref name="handler"/> for the
        /// duration of the tokenization request.
        /// </summary>
        /// <param name="handler">The TokenizationHandler that is storing the object.</param>
        /// <param name="store">The handler store used by the <paramref name="handler"/>.</param>
        public void SetHandlerStore(TokenizationHandler<T, K> handler, TokenizationHandlerExecutionStore store)
        {
            if (handler.Index == 0) handler.Index = _tokenizer.Handlers.IndexOf(handler) + 1;
            _handlerStates[handler.Index - 1] = store;
        }

        /// <summary>
        /// Returns the handler execution store previously persisted for this <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The TokenizationHandler that stored the object.</param>
        /// <returns>The handler execution store persisted for the <paramref name="handler"/>.</returns>
        public TokenizationHandlerExecutionStore GetHandlerStore(TokenizationHandler<T, K> handler)
        {
            if (handler.Index == 0) handler.Index = _tokenizer.Handlers.IndexOf(handler) + 1;
            return _handlerStates[handler.Index - 1];
        }
        #endregion

        #region Keyword Handling
        internal void IdentifyKeyword(int index, int length)
        {
            int keyword = _tokenizer.RawKeywords.IndexOf(_input.Source, index, length);
            if (keyword == -1) _builder.Append(index, length, _tokenizer.RequiredDefinitions.Unknown);
            else _builder.Append(index, length, _tokenizer.RawKeywords[keyword]);
        }
        #endregion
    }

    #region TokenizationHandlerExecutionStore
    public class TokenizationHandlerExecutionStore
    {
        private BitVector _state;

        public BitVector State { get { return _state; } set { _state = value; } }
        /// <summary>
        /// Gets or sets the boolean value at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">A integer value between 0 and 31.</param>
        /// <returns>The current value at <paramref name="index"/>.</returns>
        public bool this[int index] { get { return _state[index]; } set { _state[index] = value; } }
    }
    #endregion

    #region RequiredTokenDefinitions<K>
    public sealed class RequiredTokenDefinitions<K> where K : TokenDefinition
    {
        /// <summary>
        /// The token definition that represents whitespace characters in the input string.
        /// </summary>
        public readonly K Whitespace;
        /// <summary>
        /// The token definition that represents line breaks in the input string.
        /// </summary>
        public readonly K LineBreak;
        /// <summary>
        /// The token definition that represents consecutive non-whitespace characters that do not match any other
        /// token definitions in the input string.
        /// </summary>
        public readonly K Unknown;
        /// <summary>
        /// The token definition that represents the end of the input string.
        /// </summary>
        public readonly K EndOfFile;

        /// <summary>
        /// Constructs a RequiredTokenDefinitions instance.
        /// </summary>
        /// <param name="whitespace">The token definition that represents whitespace characters in the input string.</param>
        /// <param name="lineBreak">The token definition that represents line breaks in the input string.</param>
        /// <param name="unknown">The token definition that represents consecutive non-whitespace characters that do not match any other token definitions in the input string.</param>
        /// <param name="endOfFile">The token definition that represents the end of the input string.</param>
        /// <exception cref="ArgumentNullException">A parameter was null.</exception>
        public RequiredTokenDefinitions(K whitespace, K lineBreak, K unknown, K endOfFile)
        {
            if ((Whitespace = whitespace) == null) throw new ArgumentNullException("whitespace");
            if ((LineBreak = lineBreak) == null) throw new ArgumentNullException("lineBreak");
            if ((Unknown = unknown) == null) throw new ArgumentNullException("unknown");
            if ((EndOfFile = endOfFile) == null) throw new ArgumentNullException("endOfFile");
        }
    }
    #endregion
}
