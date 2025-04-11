// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.Tokenizer`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Parsing.Tokenization
{
  public class Tokenizer<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    private RequiredTokenDefinitions<K> _requiredDefinitions;
    private List<TokenizationHandler<T, K>> _handlers;
    private StringValueCollection<K> _keywords;
    private ParseEventLog _eventLog;

    internal StringValueCollection<K> RawKeywords => this._keywords;

    public RequiredTokenDefinitions<K> RequiredDefinitions => this._requiredDefinitions;

    public List<TokenizationHandler<T, K>> Handlers => this._handlers;

    public IDictionary<string, K> Keywords => (IDictionary<string, K>) this._keywords;

    public ParseEventLog EventLog => this._eventLog;

    public Tokenizer(RequiredTokenDefinitions<K> requiredDefinitions, ParseEventLog eventLog)
    {
      if (requiredDefinitions == null)
        throw new ArgumentNullException(nameof (requiredDefinitions));
      if (eventLog == null)
        throw new ArgumentNullException(nameof (eventLog));
      this._requiredDefinitions = requiredDefinitions;
      this._handlers = new List<TokenizationHandler<T, K>>();
      this._keywords = new StringValueCollection<K>();
      this._keywords.SetNullValue(default (K));
      this._eventLog = eventLog;
    }

    public Tokenizer(RequiredTokenDefinitions<K> requiredDefinitions)
      : this(requiredDefinitions, ParseEventLog.NullEventLog)
    {
    }

    public T[] Tokenize(string source) => this.Tokenize(source, false);

    public T[] Tokenize(string source, bool expectPartialToken)
    {
      if (string.IsNullOrEmpty(source))
      {
        T obj = new T();
        obj.Source = source;
        obj.TokenIndex = 0;
        obj.SourceIndex = 0;
        obj.SourceLength = 0;
        obj.Definition = this._requiredDefinitions.EndOfFile;
        return new T[1]{ obj };
      }
      TokenizationExecutionStore<T, K> executionStore = new TokenizationExecutionStore<T, K>(this, source, expectPartialToken);
      bool success = this.TokenizeCore(source, executionStore);
      return this.PostProcessTokensCore(executionStore, success);
    }

    public T[] Tokenize(string source, int index, int length, bool expectPartialToken)
    {
      if (string.IsNullOrEmpty(source) || length <= 0)
      {
        T obj = new T();
        obj.Source = source;
        obj.TokenIndex = 0;
        obj.SourceIndex = 0;
        obj.SourceLength = 0;
        obj.Definition = this._requiredDefinitions.EndOfFile;
        return new T[1]{ obj };
      }
      TokenizationExecutionStore<T, K> executionStore = new TokenizationExecutionStore<T, K>(this, source, index, length, expectPartialToken);
      bool success = this.TokenizeCore(source, executionStore);
      return this.PostProcessTokensCore(executionStore, success);
    }

    internal T[] PostProcessTokensCore(
      TokenizationExecutionStore<T, K> executionStore,
      bool success)
    {
      if (success)
      {
        this.PostProcessTokens((IList<T>) executionStore.Builder.Contents);
        executionStore.Builder.Append(executionStore.Source.Length - 1, 0, this._requiredDefinitions.EndOfFile);
      }
      return !success ? (T[]) null : executionStore.Builder.Contents.ToArray();
    }

    internal bool TokenizeCore(string source, TokenizationExecutionStore<T, K> executionStore)
    {
      for (int index = 0; index < this._handlers.Count; ++index)
        this._handlers[index].Reset(executionStore);
      bool flag = false;
      while (executionStore.Input.BeginAdvance())
      {
        K result = default (K);
        int index = 0;
        while (index < this._handlers.Count && !this._handlers[index].IdentifyToken(this, executionStore, ref result))
          ++index;
        if (!executionStore.Input.EndAdvance(result))
        {
          flag = true;
          break;
        }
      }
      return !flag;
    }

    protected virtual void PostProcessTokens(IList<T> tokens)
    {
    }
  }
}
