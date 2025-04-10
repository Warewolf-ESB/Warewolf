// Decompiled with JetBrains decompiler
// Type: System.Parsing.SyntaxAnalysis.AbstractSyntaxTreeGrammer`3
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Parsing.Tokenization;

namespace System.Parsing.SyntaxAnalysis
{
  public abstract class AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>
    where Token : System.Parsing.Tokenization.Token<Token, TokenKind>, new()
    where TokenKind : TokenDefinition
    where ASTNode : System.Parsing.SyntaxAnalysis.ASTNode<Token, TokenKind, ASTNode>
  {
    private string _name;
    private int _registryIndex;
    private bool _suppressFailure;
    private AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> _activeBuilder;

    internal int RegistryIndex
    {
      get => this._registryIndex;
      set => this._registryIndex = value;
    }

    public string Name => this._name;

    public abstract GrammerGroup GrammerGroup { get; }

    public virtual ASTGrammerBehaviour Behaviour => ASTGrammerBehaviour.None;

    protected AbstractSyntaxTreeGrammer()
    {
      this._name = this.GetType().Name;
      this._registryIndex = -1;
    }

    protected internal virtual void PrependTokens(ITokenBuilder builder)
    {
    }

    protected internal virtual void AppendTokens(ITokenBuilder builder)
    {
    }

    protected internal virtual void PostProcessTokens(
      Tokenizer<Token, TokenKind> tokenizer,
      IList<Token> tokens,
      int phase)
    {
    }

    public ASTNode BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder,
      ASTNode container,
      Token start,
      Token last,
      bool suppressFailure)
    {
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> activeBuilder = this._activeBuilder;
      bool suppressFailure1 = this._suppressFailure;
      ASTNode astNode1 = default (ASTNode);
      this._suppressFailure = suppressFailure;
      this._activeBuilder = builder;
      ASTNode astNode2 = this.BuildNode(builder, container, start, last);
      this._suppressFailure = suppressFailure1;
      this._activeBuilder = activeBuilder;
      return astNode2;
    }

    protected internal abstract ASTNode BuildNode(
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder,
      ASTNode container,
      Token start,
      Token last);

    public ASTNode[] TransformNodes(
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder,
      ASTNode container,
      ASTNode[] nodes,
      bool suppressFailure)
    {
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> activeBuilder = this._activeBuilder;
      bool suppressFailure1 = this._suppressFailure;
      this._suppressFailure = suppressFailure;
      this._activeBuilder = builder;
      ASTNode[] astNodeArray = this.TransformNodes(builder, container, nodes);
      this._suppressFailure = suppressFailure1;
      this._activeBuilder = activeBuilder;
      return astNodeArray;
    }

    protected internal virtual ASTNode[] TransformNodes(
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder,
      ASTNode container,
      ASTNode[] nodes)
    {
      throw new NotSupportedException();
    }

    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.SyntaxFailureReason reason,
      Token errorStart)
    {
      if (!this._suppressFailure)
        this._activeBuilder.EventLog.Log(new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), (TokenDefinition) null, (int) reason, 0, this.GrammerGroup.DisplayName, this._name));
      return default (T);
    }

    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.SyntaxFailureReason reason,
      Token errorLocation,
      TokenKind arg1)
    {
      if (!this._suppressFailure)
        this._activeBuilder.EventLog.Log(new ParseEventLogEntry(errorLocation.Source, errorLocation.ToParseEventLogToken(), (TokenDefinition) arg1, (int) reason, 0, this.GrammerGroup.DisplayName, this._name));
      return default (T);
    }

    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.SyntaxFailureReason reason,
      Token errorLocation,
      TokenKind arg1,
      TokenKind arg2)
    {
      if (!this._suppressFailure)
        this._activeBuilder.EventLog.Log(new ParseEventLogEntry(errorLocation.Source, errorLocation.ToParseEventLogToken(), (TokenDefinition) arg1, (TokenDefinition) arg2, (int) reason, 0, this.GrammerGroup.DisplayName, this._name));
      return default (T);
    }

    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.SyntaxFailureReason reason,
      Token errorStart,
      Token errorEnd,
      TokenKind arg1)
    {
      if (!this._suppressFailure)
        this._activeBuilder.EventLog.Log(new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), errorEnd.ToParseEventLogToken(), (TokenDefinition) arg1, (TokenDefinition) null, (int) reason, 0, this.GrammerGroup.DisplayName, this._name));
      return default (T);
    }

    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.SyntaxFailureReason reason,
      Token errorStart,
      Token errorEnd,
      TokenKind arg1,
      TokenKind arg2)
    {
      if (!this._suppressFailure)
        this._activeBuilder.EventLog.Log(new ParseEventLogEntry(errorStart.Source, errorStart.ToParseEventLogToken(), errorEnd.ToParseEventLogToken(), (TokenDefinition) arg1, (TokenDefinition) arg2, (int) reason, 0, this.GrammerGroup.DisplayName, this._name));
      return default (T);
    }

    [DebuggerStepThrough]
    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason reason,
      string arg)
    {
      switch (reason)
      {
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.Undefined:
          throw new Exception(arg);
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.ArgumentNullException:
          throw new ArgumentNullException(arg);
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.ArgumentException:
          throw new ArgumentException(arg);
        default:
          return default (T);
      }
    }

    [DebuggerStepThrough]
    protected T Fail<T>(
      AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason reason,
      string arg1,
      string arg2)
    {
      switch (reason)
      {
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.Undefined:
          throw new Exception(arg1);
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.ArgumentNullException:
          throw new ArgumentNullException(arg1, arg2);
        case AbstractSyntaxTreeGrammer<Token, TokenKind, ASTNode>.RuntimeFailureReason.ArgumentException:
          throw new ArgumentException(arg2, arg1);
        default:
          return default (T);
      }
    }

    protected internal virtual void OnRegisterSubGrammers(
      HashSet<GrammerGroup> existingGrammers,
      AbstractSyntaxTreeBuilder<Token, TokenKind, ASTNode> builder)
    {
    }

    protected internal abstract void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry);

    protected internal virtual void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
    {
    }

    protected enum RuntimeFailureReason
    {
      Undefined,
      ArgumentNullException,
      ArgumentException,
    }

    protected enum SyntaxFailureReason
    {
      Undefined,
      ExpectedEnterScope,
      ExpectedExitScope,
      ExpectedToken,
      ExpectedIdentifier,
      UnexpectedToken,
      ExpectedTerminal,
      ExpectedNonTerminal,
    }
  }
}
