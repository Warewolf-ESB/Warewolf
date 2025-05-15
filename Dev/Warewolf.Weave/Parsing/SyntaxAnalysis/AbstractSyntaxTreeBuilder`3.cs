// Decompiled with JetBrains decompiler
// Type: System.Parsing.SyntaxAnalysis.AbstractSyntaxTreeBuilder`3
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.Tokenization;

namespace System.Parsing.SyntaxAnalysis
{
  public abstract class AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>
    where Token : System.Parsing.Tokenization.Token<Token, TokenKind>, new()
    where TokenKind : TokenDefinition
    where ASTNode : System.Parsing.SyntaxAnalysis.ASTNode<Token, TokenKind, ASTNode>
  {
    private static AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _emptyGrammer = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[0];
    private bool _hasNodeTriggers;
    private AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>.InitializationStore _initStore;
    private Tokenizer<Token, TokenKind> _tokenizer;
    private ParseEventLog _eventLog;
    private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerLookup;
    private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerPrepends;
    private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerAppends;
    private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] _grammerPostProcess;
    private AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[][] _definitionTriggers;
    private Dictionary<Type, AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[]> _nodeTriggers;

    public ParseEventLog EventLog => this._eventLog;

    protected AbstractSyntaxTreeBuilder() => this._initStore = new AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>.InitializationStore();

    public void RegisterGrammer(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode> grammer)
    {
      if (grammer == null)
        throw new ArgumentNullException(nameof (grammer));
      if (grammer.RegistryIndex != -1)
        throw new ArgumentException("AbstractSyntaxTreeGrammer cannot be registered to multiple AbstractSyntaxTreeBuilder's", nameof (grammer));
      if (this._initStore == null)
        throw new InvalidOperationException("AbstractSyntaxTreeGrammer cannot be registered after any build requests have been made.");
      if (!this._initStore.GrammerGroups.Add(grammer.GrammerGroup))
        throw new ArgumentException("This AbstractSyntaxTreeBuilder already has a grammer registered from " + grammer.GrammerGroup.ToString(), nameof (grammer));
      grammer.RegistryIndex = this._initStore.GrammerLookup.Count;
      this._initStore.GrammerLookup.Add(grammer);
    }

    private void EnsureInitialized()
    {
      if (this._initStore == null)
        return;
      int count1;
      do
      {
        count1 = this._initStore.GrammerLookup.Count;
        for (int index = 0; index < count1; ++index)
          this._initStore.GrammerLookup[index].OnRegisterSubGrammers(this._initStore.GrammerGroups, this);
      }
      while (count1 < this._initStore.GrammerLookup.Count);
      this._tokenizer = this.CreateTokenizerInstance();
      this._eventLog = this._tokenizer.EventLog;
      ASTGrammerBehaviourRegistry triggerRegistry = new ASTGrammerBehaviourRegistry(TokenDefinition.GetTotalDefinitionsOfType(typeof (TokenKind)), typeof (ASTNode));
      this._grammerLookup = this._initStore.GrammerLookup.ToArray();
      List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> syntaxTreeGrammerList1 = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
      List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> syntaxTreeGrammerList2 = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
      List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> syntaxTreeGrammerList3 = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
      for (int index = 0; index < this._grammerLookup.Length; ++index)
      {
        triggerRegistry.GrammerRegistryIndex = index;
        this._grammerLookup[index].OnRegisterTriggers(triggerRegistry);
        ASTGrammerBehaviour behaviour = this._grammerLookup[index].Behaviour;
        if ((behaviour & ASTGrammerBehaviour.DoesPrependTokens) == ASTGrammerBehaviour.DoesPrependTokens)
          syntaxTreeGrammerList1.Add(this._grammerLookup[index]);
        if ((behaviour & ASTGrammerBehaviour.DoesAppendTokens) == ASTGrammerBehaviour.DoesAppendTokens)
          syntaxTreeGrammerList2.Add(this._grammerLookup[index]);
        if ((behaviour & ASTGrammerBehaviour.DoesPostProcessTokens) == ASTGrammerBehaviour.DoesPostProcessTokens)
          syntaxTreeGrammerList3.Add(this._grammerLookup[index]);
      }
      triggerRegistry.GrammerRegistryIndex = -1;
      if (syntaxTreeGrammerList1.Count > 0)
        this._grammerPrepends = syntaxTreeGrammerList1.ToArray();
      if (syntaxTreeGrammerList2.Count > 0)
        this._grammerAppends = syntaxTreeGrammerList2.ToArray();
      if (syntaxTreeGrammerList3.Count > 0)
        this._grammerPostProcess = syntaxTreeGrammerList3.ToArray();
      for (int index = 0; index < this._grammerLookup.Length; ++index)
        this._grammerLookup[index].OnConfigureTokenizer(this._tokenizer);
      this._definitionTriggers = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[triggerRegistry.DefinitionTriggers.Length][];
      this._nodeTriggers = new Dictionary<Type, AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[]>();
      foreach (TokenDefinition keywordDefinition in triggerRegistry.KeywordDefinitions)
        this._tokenizer.Keywords.Add(keywordDefinition.Identifier, (TokenKind) keywordDefinition);
      List<TokenizationHandler<Token, TokenKind>> collection = new List<TokenizationHandler<Token, TokenKind>>();
      HashSet<Type> typeSet = new HashSet<Type>();
      for (int index = 0; index < this._tokenizer.Handlers.Count; ++index)
      {
        TokenizationHandler<Token, TokenKind> handler;
        if ((handler = this._tokenizer.Handlers[index]).IdentifyBeforeUnaryTokens)
          collection.Add(handler);
        Type type = handler.GetType();
        if (type != typeof (UnaryTokenizationHandler<Token, TokenKind>) && !typeSet.Add(type))
          throw new InvalidOperationException("Multiple grammers have registered the same type of a non-unary TokenizationHandler.");
      }
      int count2 = collection.Count;
      collection.Add((TokenizationHandler<Token, TokenKind>) null);
      for (int index = 0; index < this._tokenizer.Handlers.Count; ++index)
      {
        TokenizationHandler<Token, TokenKind> handler;
        if ((handler = this._tokenizer.Handlers[index]).IdentifyAfterUnaryTokens)
          collection.Add(handler);
      }
      for (int index = 0; index < this._tokenizer.Handlers.Count; ++index)
      {
        TokenizationHandler<Token, TokenKind> handler;
        if (!(handler = this._tokenizer.Handlers[index]).IdentifyBeforeUnaryTokens && !handler.IdentifyAfterUnaryTokens)
        {
          if (handler is UnaryTokenizationHandler<Token, TokenKind>)
          {
            foreach (KeyValuePair<string, TokenKind> definition in (handler as UnaryTokenizationHandler<Token, TokenKind>).Definitions)
              triggerRegistry.UnaryDefinitions.Add((TokenDefinition) definition.Value);
          }
          else
            collection.Add(handler);
        }
      }
      TokenKind[] definitions;
      if (triggerRegistry.UnaryDefinitions.Count > 0)
      {
        int num = 0;
        definitions = new TokenKind[triggerRegistry.UnaryDefinitions.Count];
        foreach (TokenDefinition unaryDefinition in triggerRegistry.UnaryDefinitions)
          definitions[num++] = (TokenKind) unaryDefinition;
      }
      else
        definitions = new TokenKind[0];
      collection[count2] = (TokenizationHandler<Token, TokenKind>) new UnaryTokenizationHandler<Token, TokenKind>((IEnumerable<TokenKind>) definitions);
      for (int index1 = 0; index1 < this._definitionTriggers.Length; ++index1)
      {
        List<int> definitionTrigger = triggerRegistry.DefinitionTriggers[index1];
        if (definitionTrigger == null)
        {
          this._definitionTriggers[index1] = AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>._emptyGrammer;
        }
        else
        {
          AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] syntaxTreeGrammerArray = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[definitionTrigger.Count];
          for (int index2 = 0; index2 < syntaxTreeGrammerArray.Length; ++index2)
            syntaxTreeGrammerArray[index2] = this._grammerLookup[definitionTrigger[index2]];
          this._definitionTriggers[index1] = syntaxTreeGrammerArray;
        }
      }
      foreach (KeyValuePair<Type, List<int>> nodeTrigger in triggerRegistry.NodeTriggers)
      {
        List<int> intList = nodeTrigger.Value;
        if (intList != null && intList.Count > 0)
        {
          AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] syntaxTreeGrammerArray = new AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[intList.Count];
          for (int index = 0; index < syntaxTreeGrammerArray.Length; ++index)
            syntaxTreeGrammerArray[index] = this._grammerLookup[intList[index]];
          this._nodeTriggers.Add(nodeTrigger.Key, syntaxTreeGrammerArray);
        }
      }
      this._hasNodeTriggers = this._nodeTriggers.Count != 0;
      this._tokenizer.Handlers.Clear();
      this._tokenizer.Handlers.AddRange((IEnumerable<TokenizationHandler<Token, TokenKind>>) collection);
      this._tokenizer.Handlers.Add((TokenizationHandler<Token, TokenKind>) new WhitespaceTokenizationHandler<Token, TokenKind>());
      this._initStore = (AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>.InitializationStore) null;
    }

    public ASTNode[] Build(string input) => this.Build(input, false, out Token[] _);

    public ASTNode[] Build(string input, bool expectPartialTokens) => this.Build(input, expectPartialTokens, out Token[] _);

    private static string SanitizeInput(string input)
    {
      if (!string.IsNullOrEmpty(input))
      {
        input = input.Replace("\r", string.Empty);
        input = input.Replace("\n", string.Empty);
        input = input.Replace("]][[", "]]+[[");
      }
      return input;
    }

    public ASTNode[] Build(string input, out Token[] tokens) => this.Build(input, false, out tokens);

    public ASTNode[] Build(string input, bool expectPartialTokens, out Token[] tokens)
    {
      input = AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode>.SanitizeInput(input);
      this.EnsureInitialized();
      this._eventLog.Clear();
      tokens = (Token[]) null;
      if (string.IsNullOrEmpty(input))
      {
        tokens = this._tokenizer.Tokenize(input);
      }
      else
      {
        TokenizationExecutionStore<Token, TokenKind> executionStore = new TokenizationExecutionStore<Token, TokenKind>(this._tokenizer, input, expectPartialTokens);
        ITokenBuilder builder = (ITokenBuilder) executionStore.Builder;
        if (this._grammerPrepends != null)
        {
          for (int index = 0; index < this._grammerPrepends.Length; ++index)
            this._grammerPrepends[index].PrependTokens(builder);
        }
        bool success = this._tokenizer.TokenizeCore(input, executionStore);
        if (success)
        {
          if (this._grammerAppends != null)
          {
            for (int index = 0; index < this._grammerAppends.Length; ++index)
              this._grammerAppends[index].AppendTokens(builder);
          }
          if (this._grammerPostProcess != null)
          {
            IList<Token> contents = (IList<Token>) executionStore.Builder.Contents;
            for (int index = 0; index < this._grammerPostProcess.Length; ++index)
              this._grammerPostProcess[index].PostProcessTokens(this._tokenizer, contents, 0);
            for (int index = 0; index < this._grammerPostProcess.Length; ++index)
              this._grammerPostProcess[index].PostProcessTokens(this._tokenizer, contents, 1);
          }
        }
        tokens = this._tokenizer.PostProcessTokensCore(executionStore, success);
      }
      ASTNode[] astNodeArray = (ASTNode[]) null;
      if (tokens != null)
        astNodeArray = this.BuildNodes(default (ASTNode), tokens[0], tokens[tokens.Length - 1]);
      return astNodeArray;
    }

    public ASTNode[] BuildNodes(ASTNode container, Token start, Token last)
    {
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] syntaxTreeGrammerArray = (AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[]) null;
      ASTNode[] nodes = this.BuildNodeDeclarations(container, start, last);
      if (this._eventLog.HasEventLogs)
        return nodes;
      if (this._hasNodeTriggers && (object) container != null)
      {
        if (this._nodeTriggers.TryGetValue(container.GetType(), out syntaxTreeGrammerArray))
        {
          for (int index = 0; index < syntaxTreeGrammerArray.Length; ++index)
          {
            ASTNode[] astNodeArray;
            if ((astNodeArray = syntaxTreeGrammerArray[index].TransformNodes(this, container, nodes, false)) != null)
            {
              nodes = astNodeArray;
              break;
            }
          }
        }
        if (this._eventLog.HasEventLogs)
          return nodes;
      }
      ASTNode astNode = default (ASTNode);
      for (int index = 0; index < nodes.Length; ++index)
      {
        (astNode = nodes[index]).Visit(VisitPurpose.BuildNodeDeclarations, this);
        if (this._eventLog.HasEventLogs)
          return nodes;
      }
      return nodes;
    }

    private ASTNode[] BuildNodeDeclarations(ASTNode container, Token start, Token last)
    {
      List<ASTNode> astNodeList = new List<ASTNode>();
      ASTNode astNode1 = default (ASTNode);
      for (Token oken = start; (object) oken != null && oken.TokenIndex <= last.TokenIndex; oken = oken.NextNWS)
      {
        AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>[] definitionTrigger = this._definitionTriggers[oken.Definition.Serial];
        ASTNode astNode2 = default (ASTNode);
        for (int index = 0; index < definitionTrigger.Length; ++index)
        {
          if ((object) (astNode2 = definitionTrigger[index].BuildNode(this, container, oken, last, false)) != null)
          {
            astNodeList.Add(astNode2);
            oken = astNode2.End;
            break;
          }
        }
        if (!this._eventLog.HasEventLogs)
        {
          if ((object) astNode2 == null && !oken.Definition.IsEndOfFile)
          {
            this.OnUnhandledTokenEncountered(container, oken);
            if (this._eventLog.HasEventLogs)
              break;
          }
        }
        else
          break;
      }
      return astNodeList.ToArray();
    }

    protected virtual void OnUnhandledTokenEncountered(ASTNode container, Token token)
    {
    }

    protected abstract Tokenizer<Token, TokenKind> CreateTokenizerInstance();

    private sealed class InitializationStore
    {
      public List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>> GrammerLookup;
      public HashSet<GrammerGroup> GrammerGroups;

      public InitializationStore()
      {
        this.GrammerLookup = new List<AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>>();
        this.GrammerGroups = new HashSet<GrammerGroup>();
      }
    }
  }
}
