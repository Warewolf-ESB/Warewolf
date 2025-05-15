// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.CSharpNumericLiteralTokenizationHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class CSharpNumericLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
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
      bool flag1 = false;
      bool flag2 = false;
      int num1 = -2;
      BooleanArray booleanArray = StringUtility.DigitMask;
      for (int index1 = index + 1; index1 < store.Input.Length; ++index1)
      {
        byte num2;
        if (!booleanArray[(int) (num2 = store.Input[index1])])
        {
          if (!flag2)
          {
            if (num2 == (byte) 46)
            {
              if (num1 != -2 | flag1 || index1 + 1 == store.Input.Length || !booleanArray[(int) store.Input[index1 + 1]])
                return index1;
              kind = TokenKind.RealNoSuffix;
              flag1 = true;
              continue;
            }
            if (num2 == (byte) 101 || num2 == (byte) 69)
            {
              byte num3;
              if (num1 != -2 || index1 + 1 == store.Input.Length || !booleanArray[(int) (num3 = store.Input[index1 + 1])] && num3 != (byte) 45 && num3 != (byte) 43)
                return index1;
              kind = TokenKind.RealNoSuffix;
              num1 = index1;
              continue;
            }
            if (num2 == (byte) 120 && index1 == index + 1 && store.Input[index] == (byte) 48)
            {
              flag2 = true;
              kind = TokenKind.IntegerHexadecimal;
              booleanArray = StringUtility.HexadecimalMask;
              if (index1 + 1 == store.Input.Length || !booleanArray[(int) store.Input[index1 + 1]])
                return index1;
              continue;
            }
            if (num2 == (byte) 45 || num2 == (byte) 43)
            {
              if (num1 != index1 - 1)
                return index1;
              continue;
            }
            if (num2 == (byte) 100 || num2 == (byte) 68)
            {
              kind = TokenKind.RealSuffixD;
              return index1 + 1;
            }
            if (num2 == (byte) 102 || num2 == (byte) 70)
            {
              kind = TokenKind.RealSuffixF;
              return index1 + 1;
            }
            if (num2 == (byte) 117 || num2 == (byte) 85)
            {
              if (flag1 || num1 != -2)
                return -1;
              byte num4;
              if (index1 + 1 == store.Input.Length || (num4 = store.Input[index1 + 1]) != (byte) 108 && num4 != (byte) 76)
              {
                kind = TokenKind.IntegerSuffixU;
                return index1 + 1;
              }
              kind = TokenKind.IntegerSuffixUL;
              return index1 + 2;
            }
            if (num2 == (byte) 108 || num2 == (byte) 76)
            {
              if (flag1 || num1 != -2)
                return -1;
              byte num5;
              if (index1 + 1 == store.Input.Length || (num5 = store.Input[index1 + 1]) != (byte) 117 && num5 != (byte) 85)
              {
                kind = TokenKind.IntegerSuffixL;
                return index1 + 1;
              }
              kind = TokenKind.IntegerSuffixUL;
              return index1 + 2;
            }
          }
          return index1;
        }
      }
      return store.Input.Length;
    }
  }
}
