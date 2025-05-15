// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.CharacterLiteralTokenizationHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Collections.Generic;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class CharacterLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
  {
    private const byte SingleQuote = 39;
    private const byte HexadecimalChar = 120;
    private const byte UnicodeChar = 117;
    private const byte Backslash = 92;
    private const int ToggleState = 0;
    private const int PendingState = 1;
    private static readonly char[] RegularStringEscapeCharacters = new char[14]
    {
      '\'',
      '"',
      '\\',
      '0',
      'a',
      'b',
      'f',
      'n',
      'r',
      't',
      'u',
      'U',
      'x',
      'v'
    };
    private static readonly BooleanArray RegularStringEscapeMask = new BooleanArray(CharacterLiteralTokenizationHandler.RegularStringEscapeCharacters, 256);

    public override sealed bool IdentifyBeforeUnaryTokens => true;

    public override sealed bool IdentifyAfterUnaryTokens => true;

    public override void Reset(TokenizationExecutionStore<Token, TokenKind> store)
    {
      TokenizationHandlerExecutionStore handlerStore = store.GetHandlerStore((TokenizationHandler<Token, TokenKind>) this);
      if (handlerStore == null)
      {
        TokenizationHandlerExecutionStore store1 = new TokenizationHandlerExecutionStore();
        store.SetHandlerStore((TokenizationHandler<Token, TokenKind>) this, store1);
      }
      else
        handlerStore.State = new BitVector();
    }

    public override bool IdentifyToken(
      Tokenizer<Token, TokenKind> tokenizer,
      TokenizationExecutionStore<Token, TokenKind> store,
      ref TokenKind result)
    {
      TokenizationHandlerExecutionStore handlerStore = store.GetHandlerStore((TokenizationHandler<Token, TokenKind>) this);
      bool flag = handlerStore[0];
      handlerStore[0] = !flag;
      if (flag)
      {
        if (!handlerStore[1])
          return false;
        handlerStore[1] = false;
        store.Input.Origin = store.Input.Position - 1;
        if (store.Input.Current == (byte) 92)
        {
          if (store.Input.Next == (byte) 120)
          {
            result = TokenKind.HexadecimalChar;
            int num = 0;
            for (int index1 = store.Input.Position + 2; index1 < store.Input.Length; ++index1)
            {
              byte index2;
              if ((index2 = store.Input[index1]) == (byte) 92)
              {
                store.Input.End = -1;
                return true;
              }
              if (index2 == (byte) 39)
              {
                store.Input.End = num > 0 ? index1 + 1 : -1;
                return true;
              }
              if (num++ > 3 || !StringUtility.HexadecimalMask[(int) index2])
              {
                store.Input.End = -1;
                return true;
              }
            }
            store.Input.End = -1;
          }
          else if (store.Input.Next == (byte) 117)
          {
            result = TokenKind.UnicodeChar;
            int num = 0;
            for (int index3 = store.Input.Position + 2; index3 < store.Input.Length; ++index3)
            {
              byte index4;
              if ((index4 = store.Input[index3]) == (byte) 92)
              {
                store.Input.End = -1;
                return true;
              }
              if (index4 == (byte) 39)
              {
                store.Input.End = num == 4 ? index3 + 1 : -1;
                return true;
              }
              if (num++ > 3 || !StringUtility.HexadecimalMask[(int) index4])
              {
                store.Input.End = -1;
                return true;
              }
            }
            store.Input.End = -1;
          }
          else if (store.Input.Position + 2 >= store.Input.Length || store.Input[store.Input.Position + 2] != (byte) 39 || !CharacterLiteralTokenizationHandler.RegularStringEscapeMask[(int) store.Input.Next])
          {
            store.Input.End = -1;
          }
          else
          {
            result = TokenKind.EscapedChar;
            store.Input.End = store.Input.Position + 3;
          }
        }
        else if (store.Input.Current == (byte) 39 || store.Input.Next != (byte) 39)
        {
          store.Input.End = -1;
        }
        else
        {
          result = TokenKind.RegularChar;
          store.Input.End = store.Input.Position + 2;
        }
        return true;
      }
      if (store.Input.Current != (byte) 39)
        return false;
      if (handlerStore[1])
        store.Input.End = -1;
      handlerStore[1] = true;
      return true;
    }
  }
}
