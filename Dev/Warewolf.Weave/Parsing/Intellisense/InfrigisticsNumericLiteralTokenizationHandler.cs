// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.InfrigisticsNumericLiteralTokenizationHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class InfrigisticsNumericLiteralTokenizationHandler : 
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
      bool flag = false;
      int num1 = -2;
      BooleanArray digitMask = StringUtility.DigitMask;
      for (int index1 = index + 1; index1 < store.Input.Length; ++index1)
      {
        byte num2;
        if (!digitMask[(int) (num2 = store.Input[index1])])
        {
          switch (num2)
          {
            case 37:
              kind = TokenKind.RealSuffixPercent;
              return index1 + 1;
            case 43:
            case 45:
              if (num1 != index1 - 1)
                return index1;
              continue;
            case 46:
              if (num1 != -2 | flag || index1 + 1 == store.Input.Length || !digitMask[(int) store.Input[index1 + 1]])
                return index1;
              kind = TokenKind.RealNoSuffix;
              flag = true;
              continue;
            case 69:
            case 101:
              byte num3;
              if (num1 != -2 || index1 + 1 == store.Input.Length || !digitMask[(int) (num3 = store.Input[index1 + 1])] && num3 != (byte) 45 && num3 != (byte) 43)
                return index1;
              kind = TokenKind.RealNoSuffix;
              num1 = index1;
              continue;
            default:
              return index1;
          }
        }
      }
      return store.Input.Length;
    }
  }
}
