// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.WhitespaceTokenizationHandler`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public sealed class WhitespaceTokenizationHandler<T, K> : TokenizationHandler<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    public override bool IdentifyToken(
      Tokenizer<T, K> tokenizer,
      TokenizationExecutionStore<T, K> store,
      ref K result)
    {
      if (!StringUtility.WhitespaceMask[(int) store.Input.Current])
        return false;
      result = store.Input.Current == (byte) 10 ? tokenizer.RequiredDefinitions.LineBreak : tokenizer.RequiredDefinitions.Whitespace;
      store.Input.End = store.Input.Length;
      store.Input.Origin = store.Input.Position;
      for (int index = store.Input.Position + 1; index < store.Input.Length; ++index)
      {
        if (!StringUtility.WhitespaceMask[(int) (store.Input.Previous = store.Input[index])])
        {
          store.Input.End = index;
          break;
        }
        if (store.Input.Previous == (byte) 10)
          result = tokenizer.RequiredDefinitions.LineBreak;
      }
      return true;
    }
  }
}
