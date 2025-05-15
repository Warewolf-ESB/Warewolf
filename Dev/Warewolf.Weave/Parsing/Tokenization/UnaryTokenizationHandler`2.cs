// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.UnaryTokenizationHandler`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Parsing.Tokenization
{
  public sealed class UnaryTokenizationHandler<T, K> : TokenizationHandler<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    private StringValueCollection<K> _definitions;
    private BooleanArray _mask;
    private int _longest;

    internal StringValueCollection<K> Definitions => this._definitions;

    public override sealed bool IdentifyBeforeUnaryTokens => false;

    public override sealed bool IdentifyAfterUnaryTokens => false;

    public UnaryTokenizationHandler(IEnumerable<K> definitions)
    {
      this._mask = new BooleanArray(256, false);
      this._definitions = new StringValueCollection<K>();
      this._definitions.SetNullValue(default (K));
      foreach (K definition in definitions)
      {
        int length = definition.Identifier.Length;
        if (length > this._longest)
          this._longest = length;
        for (int index = 0; index < length; ++index)
          this._mask[(int) definition.Identifier[index]] = true;
        this._definitions.Add(definition.Identifier, definition);
      }
    }

    public override bool IdentifyToken(
      Tokenizer<T, K> tokenizer,
      TokenizationExecutionStore<T, K> store,
      ref K result)
    {
      if (!this._mask[(int) store.Input.Current])
        return false;
      store.Input.End = 1;
      for (int index = 1; index < this._longest && this._mask[(int) store.Input[store.Input.Position + index]]; ++index)
        store.Input.End = index + 1;
      for (int end = store.Input.End; end > 0 && (store.Input.Origin = this._definitions.IndexOf(store.Input.Source, store.Input.Position, store.Input.End)) == -1; --end)
        store.Input.End = end - 1;
      result = default (K);
      if (store.Input.End == 0)
        return false;
      result = this._definitions[store.Input.Origin];
      store.Input.Origin = store.Input.Position;
      store.Input.End += store.Input.Position;
      return true;
    }
  }
}
