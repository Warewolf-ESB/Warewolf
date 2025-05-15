// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenizationHandler`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public abstract class TokenizationHandler<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    internal int Index;

    public virtual bool IdentifyBeforeUnaryTokens => false;

    public virtual bool IdentifyAfterUnaryTokens => false;

    public virtual void Reset(TokenizationExecutionStore<T, K> store)
    {
    }

    public abstract bool IdentifyToken(
      Tokenizer<T, K> tokenizer,
      TokenizationExecutionStore<T, K> store,
      ref K result);
  }
}
