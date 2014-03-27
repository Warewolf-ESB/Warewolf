using System.Collections.Generic;
using System.Data;
using System.Parsing.Tokenization;

namespace System.Parsing.SyntaxAnalysis
{
    /// <summary>
    /// An implementation of an LR Parser
    /// Input Evaluation: Left to right, top to bottom
    /// Produces: Reversed right most derivation
    /// </summary>
    /// <typeparam name="Token">The concrete Token type.</typeparam>
    /// <typeparam name="TokenKind">The concrete TokenDefinition type.</typeparam>
    /// <typeparam name="ASTNode">The concrete ASTNode type.</typeparam>
    public abstract class AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>
        where Token : Token<Token, TokenKind>, new()
        where TokenKind : TokenDefinition
        where ASTNode : ASTNode<Token, TokenKind, ASTNode>
    {
        #region Static Members
        private static AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _emptyGrammer = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[0];
        #endregion

        #region Instance Fields
        private bool _hasNodeTriggers;
        private InitializationStore _initStore;
        private Tokenizer<Token, TokenKind> _tokenizer;
        private ParseEventLog _eventLog;
        private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerLookup;

        private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerPrepends;
        private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerAppends;
        private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerPostProcess;

        private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[][] _definitionTriggers;
        private Dictionary<Type, AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[]> _nodeTriggers;
        #endregion

        #region Public Properties
        public ParseEventLog EventLog { get { return _eventLog; } }
        #endregion

        #region Constructor
        protected AbstractSyntaxTreeBuilder()
        {
            _initStore = new InitializationStore();
        }
        #endregion

        #region Registration Handling
        public void RegisterGrammer(AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode> grammer)
        {
            if(grammer == null) throw new ArgumentNullException("grammer");
            if(grammer.RegistryIndex != -1) throw new ArgumentException("AbstractSyntaxTreeGrammer cannot be registered to multiple AbstractSyntaxTreeBuilder's", "grammer");
            if(_initStore == null) throw new InvalidOperationException("AbstractSyntaxTreeGrammer cannot be registered after any build requests have been made.");
            if(!_initStore.GrammerGroups.Add(grammer.GrammerGroup)) throw new ArgumentException("This AbstractSyntaxTreeBuilder already has a grammer registered from " + grammer.GrammerGroup.ToString(), "grammer");
            grammer.RegistryIndex = _initStore.GrammerLookup.Count;
            _initStore.GrammerLookup.Add(grammer);
        }
        #endregion

        #region Initialization Handling
        private void EnsureInitialized()
        {
            if(_initStore == null) return;

            int totalGrammers = 0;

            do
            {
                totalGrammers = _initStore.GrammerLookup.Count;

                for(int i = 0; i < totalGrammers; i++)
                {
                    _initStore.GrammerLookup[i].OnRegisterSubGrammers(_initStore.GrammerGroups, this);
                }
            }
            while(totalGrammers < _initStore.GrammerLookup.Count);

            _tokenizer = CreateTokenizerInstance();
            _eventLog = _tokenizer.EventLog;
            ASTGrammerBehaviourRegistry registry = new ASTGrammerBehaviourRegistry(TokenDefinition.GetTotalDefinitionsOfType(typeof(TokenKind)), typeof(ASTNode));
            _grammerLookup = _initStore.GrammerLookup.ToArray();

            List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> prepends = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
            List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> appends = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
            List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> postprocess = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();

            for(int i = 0; i < _grammerLookup.Length; i++)
            {
                registry.GrammerRegistryIndex = i;
                _grammerLookup[i].OnRegisterTriggers(registry);

                ASTGrammerBehaviour flags = _grammerLookup[i].Behaviour;

                if((flags & ASTGrammerBehaviour.DoesPrependTokens) == ASTGrammerBehaviour.DoesPrependTokens) prepends.Add(_grammerLookup[i]);
                if((flags & ASTGrammerBehaviour.DoesAppendTokens) == ASTGrammerBehaviour.DoesAppendTokens) appends.Add(_grammerLookup[i]);
                if((flags & ASTGrammerBehaviour.DoesPostProcessTokens) == ASTGrammerBehaviour.DoesPostProcessTokens) postprocess.Add(_grammerLookup[i]);
            }

            registry.GrammerRegistryIndex = -1;

            if(prepends.Count > 0) _grammerPrepends = prepends.ToArray();
            if(appends.Count > 0) _grammerAppends = appends.ToArray();
            if(postprocess.Count > 0) _grammerPostProcess = postprocess.ToArray();


            for(int i = 0; i < _grammerLookup.Length; i++) _grammerLookup[i].OnConfigureTokenizer(_tokenizer);
            _definitionTriggers = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[registry.DefinitionTriggers.Length][];
            _nodeTriggers = new Dictionary<Type, AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[]>();

            foreach(TokenDefinition keyword in registry.KeywordDefinitions) _tokenizer.Keywords.Add(keyword.Identifier, (TokenKind)keyword);



            List<TokenizationHandler<Token, TokenKind>> orderedHandlers = new List<TokenizationHandler<Token, TokenKind>>();
            TokenizationHandler<Token, TokenKind> currentHandler = null;
            HashSet<Type> uniqueHandlers = new HashSet<Type>();

            for(int i = 0; i < _tokenizer.Handlers.Count; i++)
            {
                if((currentHandler = _tokenizer.Handlers[i]).IdentifyBeforeUnaryTokens) orderedHandlers.Add(currentHandler);
                Type hType = currentHandler.GetType();

                if(hType != typeof(UnaryTokenizationHandler<Token, TokenKind>))
                    if(!uniqueHandlers.Add(hType))
                        throw new InvalidOperationException("Multiple grammers have registered the same type of a non-unary TokenizationHandler.");
            }

            int unaryPlaceholder = orderedHandlers.Count;
            orderedHandlers.Add(null);

            for(int i = 0; i < _tokenizer.Handlers.Count; i++)
                if((currentHandler = _tokenizer.Handlers[i]).IdentifyAfterUnaryTokens)
                    orderedHandlers.Add(currentHandler);

            for(int i = 0; i < _tokenizer.Handlers.Count; i++)
                if(!(currentHandler = _tokenizer.Handlers[i]).IdentifyBeforeUnaryTokens && !currentHandler.IdentifyAfterUnaryTokens)
                {
                    if(currentHandler is UnaryTokenizationHandler<Token, TokenKind>)
                    {
                        UnaryTokenizationHandler<Token, TokenKind> existingUnary = currentHandler as UnaryTokenizationHandler<Token, TokenKind>;

                        foreach(KeyValuePair<string, TokenKind> kvp in existingUnary.Definitions)
                            registry.UnaryDefinitions.Add(kvp.Value);
                    }
                    else orderedHandlers.Add(currentHandler);
                }

            TokenKind[] unaryDefinitions;

            if(registry.UnaryDefinitions.Count > 0)
            {
                int index = 0;
                unaryDefinitions = new TokenKind[registry.UnaryDefinitions.Count];
                foreach(TokenDefinition currentUnaryDefinition in registry.UnaryDefinitions) unaryDefinitions[index++] = (TokenKind)currentUnaryDefinition;
            }
            else unaryDefinitions = new TokenKind[0];

            orderedHandlers[unaryPlaceholder] = new UnaryTokenizationHandler<Token, TokenKind>(unaryDefinitions);

            for(int i = 0; i < _definitionTriggers.Length; i++)
            {
                List<int> mapping = registry.DefinitionTriggers[i];

                if(mapping == null) _definitionTriggers[i] = _emptyGrammer;
                else
                {
                    AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] row = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[mapping.Count];
                    for(int k = 0; k < row.Length; k++) row[k] = _grammerLookup[mapping[k]];
                    _definitionTriggers[i] = row;
                }
            }

            foreach(KeyValuePair<Type, List<int>> kvp in registry.NodeTriggers)
            {
                List<int> mapping = kvp.Value;

                if(mapping != null && mapping.Count > 0)
                {
                    AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] row = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[mapping.Count];
                    for(int i = 0; i < row.Length; i++) row[i] = _grammerLookup[mapping[i]];
                    _nodeTriggers.Add(kvp.Key, row);
                }
            }

            _hasNodeTriggers = _nodeTriggers.Count != 0;

            _tokenizer.Handlers.Clear();
            _tokenizer.Handlers.AddRange(orderedHandlers);
            _tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());

            _initStore = null;
        }
        #endregion

        #region Build Handling
        /// <summary>
        /// Builds an abstract syntax tree from <paramref name="input"/>
        /// </summary>
        /// <param name="input">The source string used to construct the abstract syntax tree</param>
        /// <returns>A sequence of nodes that represent the top level of the abstract syntax tree</returns>
        public ASTNode[] Build(string input)
        {
            Token[] tokens;
            return Build(input, false, out tokens);
        }

        /// <summary>
        /// Builds an abstract syntax tree from <paramref name="input"/>
        /// </summary>
        /// <param name="input">The source string used to construct the abstract syntax tree</param>
        /// <param name="expectPartialTokens">Whether or not partially formed tokens are to be expected, and handled such that
        /// the tokenization process does not fail.</param>
        /// <returns>A sequence of nodes that represent the top level of the abstract syntax tree</returns>
        public ASTNode[] Build(string input, bool expectPartialTokens)
        {
            Token[] tokens;
            return Build(input, expectPartialTokens, out tokens);
        }

        static string SanitizeInput(string input)
        {
            if(!string.IsNullOrEmpty(input))
            {
                input = input.Replace("\r", string.Empty);
                input = input.Replace("\n", string.Empty);
                input = input.Replace("]][[", "]]+[[");
            }
            return input;
        }

        /// <summary>
        /// Builds an abstract syntax tree from <paramref name="input"/>
        /// </summary>
        /// <param name="input">The source string used to construct the abstract syntax tree</param>
        /// <param name="tokens">The tokens that were constructed by the tokenizer.</param>
        /// <returns>A sequence of nodes that represent the top level of the abstract syntax tree</returns>
        public ASTNode[] Build(string input, out Token[] tokens)
        {
            return Build(input, false, out tokens);
        }

        /// <summary>
        /// Builds an abstract syntax tree from <paramref name="input"/>
        /// </summary>
        /// <param name="input">The source string used to construct the abstract syntax tree</param>
        /// <param name="expectPartialTokens">Whether or not partially formed tokens are to be expected, and handled such that
        /// the tokenization process does not fail.</param>
        /// <param name="tokens">The tokens that were constructed by the tokenizer.</param>
        /// <returns>A sequence of nodes that represent the top level of the abstract syntax tree</returns>
        public ASTNode[] Build(string input, bool expectPartialTokens, out Token[] tokens)
        {
            input = SanitizeInput(input);
            EnsureInitialized();
            _eventLog.Clear();
            tokens = null;

            if(String.IsNullOrEmpty(input))
                tokens = _tokenizer.Tokenize(input);
            else
            {
                TokenizationExecutionStore<Token, TokenKind> executionStore = new TokenizationExecutionStore<Token, TokenKind>(_tokenizer, input, expectPartialTokens);
                ITokenBuilder tBuilder = executionStore.Builder;

                if(_grammerPrepends != null)
                    for(int i = 0; i < _grammerPrepends.Length; i++)
                        _grammerPrepends[i].PrependTokens(tBuilder);

                bool success = _tokenizer.TokenizeCore(input, executionStore);

                if(success)
                {
                    if(_grammerAppends != null)
                        for(int i = 0; i < _grammerAppends.Length; i++)
                            _grammerAppends[i].AppendTokens(tBuilder);

                    if(_grammerPostProcess != null)
                    {
                        IList<Token> deltaTokens = executionStore.Builder.Contents;

                        for(int i = 0; i < _grammerPostProcess.Length; i++)
                            _grammerPostProcess[i].PostProcessTokens(_tokenizer, deltaTokens, 0);

                        for(int i = 0; i < _grammerPostProcess.Length; i++)
                            _grammerPostProcess[i].PostProcessTokens(_tokenizer, deltaTokens, 1);
                    }
                }

                tokens = _tokenizer.PostProcessTokensCore(executionStore, success);
            }

            ASTNode[] result = null;

            if(tokens != null)
            {
                result = BuildNodes(null, tokens[0], tokens[tokens.Length - 1]);
            }

            return result;
        }

        public ASTNode[] BuildNodes(ASTNode container, Token start, Token last)
        {
            AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] subscribers = null;
            ASTNode[] result = BuildNodeDeclarations(container, start, last);
            if(_eventLog.HasEventLogs) return result;

            if(_hasNodeTriggers && container != null)
            {
                if(_nodeTriggers.TryGetValue(container.GetType(), out subscribers))
                {
                    ASTNode[] current = null;

                    for(int k = 0; k < subscribers.Length; k++)
                    {
                        if((current = subscribers[k].TransformNodes(this, container, result, false)) != null)
                        {
                            result = current;
                            break;
                        }
                    }
                }

                if(_eventLog.HasEventLogs) return result;
            }

            ASTNode currentNode = null;

            for(int i = 0; i < result.Length; i++)
            {
                (currentNode = result[i]).Visit(VisitPurpose.BuildNodeDeclarations, this);
                if(_eventLog.HasEventLogs) return result;
            }

            return result;
        }

        /// <summary>
        /// Builds the nodes occuring between <paramref name="start"/> and <paramref name="last"/> within the node <paramref name="container"/>
        /// </summary>
        /// <param name="container">The node that will container the generated nodes</param>
        /// <param name="start">The token to start building nodes from</param>
        /// <param name="last">The last token that nodes will be built from</param>
        /// <returns>A sequence of nodes that represent the top level of the nodes within <paramref name="container"/></returns>
        private ASTNode[] BuildNodeDeclarations(ASTNode container, Token start, Token last)
        {
            AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] subscribers = null;
            List<ASTNode> allNamespaces = new List<ASTNode>();
            ASTNode current = null;

            for(Token token = start; token != null && token.TokenIndex <= last.TokenIndex; token = token.NextNWS)
            {
                subscribers = _definitionTriggers[token.Definition.Serial];
                current = null;

                for(int k = 0; k < subscribers.Length; k++)
                {
                    if((current = subscribers[k].BuildNode(this, container, token, last, false)) != null)
                    {
                        allNamespaces.Add(current);
                        token = current.End;
                        break;
                    }
                }

                if(_eventLog.HasEventLogs) break;

                if(current == null)
                {
                    if(!token.Definition.IsEndOfFile)
                    {
                        OnUnhandledTokenEncountered(container, token);
                        if(_eventLog.HasEventLogs) break;
                    }
                }
            }

            return allNamespaces.ToArray();
        }

        protected virtual void OnUnhandledTokenEncountered(ASTNode container, Token token) { }
        #endregion

        #region Tokenization Handling
        protected abstract Tokenizer<Token, TokenKind> CreateTokenizerInstance();
        #endregion

        #region InitializationStore
        private sealed class InitializationStore
        {
            public List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> GrammerLookup;
            public HashSet<GrammerGroup> GrammerGroups;

            public InitializationStore()
            {
                GrammerLookup = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
                GrammerGroups = new HashSet<GrammerGroup>();
            }
        }
        #endregion
    }

    public sealed class ASTGrammerBehaviourRegistry
    {
        #region Instance Fields
        private List<int>[] _definitionTriggers;
        private int _length;
        private int _grammerRegistryIndex;
        private HashSet<TokenDefinition> _unaryDefinitions;
        private HashSet<TokenDefinition> _keywordDefinitions;

        private Dictionary<Type, List<int>> _nodeTriggers;

        private Type _baseNodeType;
        #endregion

        #region Internal Properties
        internal List<int>[] DefinitionTriggers { get { return _definitionTriggers; } }
        internal Dictionary<Type, List<int>> NodeTriggers { get { return _nodeTriggers; } }
        internal int GrammerRegistryIndex { get { return _grammerRegistryIndex; } set { _grammerRegistryIndex = value; } }
        internal HashSet<TokenDefinition> UnaryDefinitions { get { return _unaryDefinitions; } }
        internal HashSet<TokenDefinition> KeywordDefinitions { get { return _keywordDefinitions; } }
        #endregion

        #region Constructor
        internal ASTGrammerBehaviourRegistry(int maxUniqueDefinitions, Type baseNodeType)
        {
            _grammerRegistryIndex = -1;
            _definitionTriggers = new List<int>[_length = maxUniqueDefinitions];
            _unaryDefinitions = new HashSet<TokenDefinition>();
            _keywordDefinitions = new HashSet<TokenDefinition>();
            _baseNodeType = baseNodeType;
            _nodeTriggers = new Dictionary<Type, List<int>>();
        }
        #endregion

        #region Registration Handling
        /// <summary>
        /// Registers trigger that will fire when a token is encountered
        /// that has the same definition as <paramref name="triggeringDefinition"/>
        /// </summary>
        /// <param name="triggeringDefinition">The TokenDefinition that will be used as the trigger condition.</param>
        /// <exception cref="ArgumentNullException"><paramref name="triggeringDefinition"/> is null</exception>
        /// <exception cref="ReadOnlyException">Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers</exception>
        public void Register(TokenDefinition triggeringDefinition)
        {
            if(triggeringDefinition == null) throw new ArgumentNullException("triggeringDefinition");
            if(_grammerRegistryIndex == -1) throw new ReadOnlyException("Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers");
            int index = triggeringDefinition.Serial;
            (_definitionTriggers[index] ?? (_definitionTriggers[index] = new List<int>())).Add(_grammerRegistryIndex);

            if(!triggeringDefinition.IsUnknown && !triggeringDefinition.IsWhitespace && !triggeringDefinition.IsEndOfFile)
            {
                if(!String.IsNullOrEmpty(triggeringDefinition.Identifier))
                {
                    if(triggeringDefinition.IsKeyword) _keywordDefinitions.Add(triggeringDefinition);
                    else _unaryDefinitions.Add(triggeringDefinition);
                }
            }
        }

        /// <summary>
        /// Registers trigger that will fire a transform node request when a containing node of type <paramref name="triggeringNodeType"/> is encountered.
        /// </summary>
        /// <param name="triggeringNodeType">The type of node that will be used as the trigger condition.</param>
        /// <exception cref="ArgumentNullException"><paramref name="triggeringNodeType"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="triggeringNodeType"/> is not a descendant of the AbstractSyntaxTreeBuilder's ASTNode type</exception>
        /// <exception cref="ReadOnlyException">Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers</exception>
        public void Register(Type triggeringNodeType)
        {
            if(triggeringNodeType == null) throw new ArgumentNullException("triggeringNodeType");
            if(!_baseNodeType.IsAssignableFrom(triggeringNodeType)) throw new ArgumentException("triggeringNodeType must be a descendant of \"" + _baseNodeType.Name + "\"");
            if(_grammerRegistryIndex == -1) throw new ReadOnlyException("Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers");

            List<int> existing;
            if(!_nodeTriggers.TryGetValue(triggeringNodeType, out existing)) _nodeTriggers.Add(triggeringNodeType, existing = new List<int>());
            existing.Add(_grammerRegistryIndex);
        }
        #endregion
    }

    #region VisitPurpose
    public enum VisitPurpose
    {
        None = 0,
        BuildNodeDeclarations = 1
    }
    #endregion
}
