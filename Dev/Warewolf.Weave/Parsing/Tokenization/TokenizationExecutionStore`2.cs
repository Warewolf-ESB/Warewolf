// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenizationExecutionStore`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public sealed class TokenizationExecutionStore<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    private System.Parsing.Tokenization.Tokenizer<T, K> _tokenizer;
    private TokenBuilder<T, K> _builder;
    private TokenizerInputWalker<T, K> _input;
    private TokenizationHandlerExecutionStore[] _handlerStates;
    private string _source;
    private bool _expectPartialToken;

    internal TokenBuilder<T, K> Builder => this._builder;

    internal TokenizerInputWalker<T, K> Input => this._input;

    public System.Parsing.Tokenization.Tokenizer<T, K> Tokenizer => this._tokenizer;

    public string Source => this._source;

    public bool ExpectPartialToken
    {
      get => this._expectPartialToken;
      set => this._expectPartialToken = value;
    }

    internal TokenizationExecutionStore(
      System.Parsing.Tokenization.Tokenizer<T, K> tokenizer,
      string source,
      bool expectPartialToken)
    {
      this._tokenizer = tokenizer;
      this._source = source;
      this._expectPartialToken = expectPartialToken;
      this._handlerStates = new TokenizationHandlerExecutionStore[this._tokenizer.Handlers.Count];
      this._builder = new TokenBuilder<T, K>(source);
      this._input = new TokenizerInputWalker<T, K>(this);
      this._input.Reset(source);
    }

    internal TokenizationExecutionStore(
      System.Parsing.Tokenization.Tokenizer<T, K> tokenizer,
      string source,
      int index,
      int length,
      bool expectPartialToken)
    {
      this._tokenizer = tokenizer;
      this._source = source;
      this._expectPartialToken = expectPartialToken;
      this._handlerStates = new TokenizationHandlerExecutionStore[this._tokenizer.Handlers.Count];
      this._builder = new TokenBuilder<T, K>(source);
      this._input = new TokenizerInputWalker<T, K>(this);
      this._input.Reset(source, index, length);
    }

    public void SetHandlerStore(
      TokenizationHandler<T, K> handler,
      TokenizationHandlerExecutionStore store)
    {
      if (handler.Index == 0)
        handler.Index = this._tokenizer.Handlers.IndexOf(handler) + 1;
      this._handlerStates[handler.Index - 1] = store;
    }

    public TokenizationHandlerExecutionStore GetHandlerStore(TokenizationHandler<T, K> handler)
    {
      if (handler.Index == 0)
        handler.Index = this._tokenizer.Handlers.IndexOf(handler) + 1;
      return this._handlerStates[handler.Index - 1];
    }

    internal void IdentifyKeyword(int index, int length)
    {
      int index1 = this._tokenizer.RawKeywords.IndexOf(this._input.Source, index, length);
      if (index1 == -1)
        this._builder.Append(index, length, this._tokenizer.RequiredDefinitions.Unknown);
      else
        this._builder.Append(index, length, this._tokenizer.RawKeywords[index1]);
    }
  }
}
