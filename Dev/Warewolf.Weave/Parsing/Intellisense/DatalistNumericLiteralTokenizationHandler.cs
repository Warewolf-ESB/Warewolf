// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistNumericLiteralTokenizationHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class DatalistNumericLiteralTokenizationHandler : 
    TokenizationHandler<Token, TokenKind>
  {
    private const byte FullStop = 46;

    public override bool IdentifyToken(
      Tokenizer<Token, TokenKind> tokenizer,
      TokenizationExecutionStore<Token, TokenKind> store,
      ref TokenKind result)
    {
      if (store.Input.Index != store.Input.Position || !StringUtility.DigitMask[(int) store.Input.Current])
        return false;
      store.Input.End = this.DemarcateNumeric(store, store.Input.Position, out result);
      store.Input.Origin = store.Input.Position;
      return true;
    }

    private int DemarcateNumeric(
      TokenizationExecutionStore<Token, TokenKind> store,
      int index,
      out TokenKind kind)
    {
      kind = TokenKind.IntegerNoSuffix;
      if (index + 1 == store.Input.Length)
        return index + 1;
      byte num = 0;
      BooleanArray digitMask = StringUtility.DigitMask;
      for (int index1 = index + 1; index1 < store.Input.Length; ++index1)
      {
        if (!digitMask[(int) (num = store.Input[index1])])
          return index1;
      }
      return store.Input.Length;
    }
  }
}
