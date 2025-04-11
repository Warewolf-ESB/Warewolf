// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.StringLiteralTokenizationHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
  public sealed class StringLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
  {
    private const byte CommercialAt = 64;
    private const byte DoubleQuote = 34;
    private const byte Backslash = 92;

    public override bool IdentifyToken(
      Tokenizer<Token, TokenKind> tokenizer,
      TokenizationExecutionStore<Token, TokenKind> store,
      ref TokenKind result)
    {
      if (store.Input.Current != (byte) 34)
        return false;
      if (store.Input.Previous == (byte) 64)
      {
        result = TokenKind.VerbatimString;
        byte num = 0;
        for (int index = store.Input.Position + 1; index < store.Input.Length; ++index)
        {
          if ((num = store.Input[index]) == (byte) 34)
          {
            if (index + 1 >= store.Input.Length || store.Input[index + 1] != (byte) 34)
            {
              store.Input.End = index + 1;
              store.Input.Origin = store.Input.Position - 1;
              return true;
            }
            ++index;
          }
        }
        if (store.ExpectPartialToken)
        {
          store.ExpectPartialToken = false;
          store.Input.End = store.Input.Length;
          store.Input.Origin = store.Input.Position - 1;
          return true;
        }
        store.Tokenizer.EventLog.Log(new ParseEventLogEntry(store.Source, new ParseEventLogToken()
        {
          SourceIndex = store.Input.Position - 1,
          SourceLength = 2,
          Contents = "@\"",
          TokenIndex = store.Builder.Contents.Count,
          Definition = (TokenDefinition) result
        }, (TokenDefinition) null, -1, 1, "Tokenizer", nameof (StringLiteralTokenizationHandler)));
        store.Input.Origin = store.Input.Position - 1;
        store.Input.End = -1;
      }
      else
      {
        result = TokenKind.RegularString;
        for (int index = store.Input.Position + 1; index < store.Input.Length; ++index)
        {
          byte num;
          if ((num = store.Input[index]) == (byte) 92)
          {
            if (index + 1 >= store.Input.Length || !StringUtility.RegularStringEscapeMask[(int) store.Input[index + 1]])
            {
              if (store.ExpectPartialToken)
              {
                store.ExpectPartialToken = false;
                store.Input.End = index + 1 >= store.Input.Length ? index + 1 : index + 2;
                store.Input.Origin = store.Input.Position;
                return true;
              }
              break;
            }
            ++index;
          }
          else if (num == (byte) 34)
          {
            store.Input.End = index + 1;
            store.Input.Origin = store.Input.Position;
            return true;
          }
        }
        if (store.ExpectPartialToken)
        {
          store.ExpectPartialToken = false;
          store.Input.End = store.Input.Length;
          store.Input.Origin = store.Input.Position;
          return true;
        }
        store.Tokenizer.EventLog.Log(new ParseEventLogEntry(store.Source, new ParseEventLogToken()
        {
          SourceIndex = store.Input.Position,
          SourceLength = 1,
          Contents = "\"",
          TokenIndex = store.Builder.Contents.Count,
          Definition = (TokenDefinition) result
        }, (TokenDefinition) null, -1, 2, "Tokenizer", nameof (StringLiteralTokenizationHandler)));
        store.Input.Origin = store.Input.Position;
        store.Input.End = -1;
      }
      return true;
    }
  }
}
