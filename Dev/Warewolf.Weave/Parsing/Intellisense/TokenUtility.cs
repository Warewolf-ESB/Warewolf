// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.TokenUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public static class TokenUtility
  {
    public static TokenPair BuildGroupSimple(Token start, TokenKind close)
    {
      if (start == null)
        return new TokenPair();
      TokenKind definition = start.Definition;
      Token end = (Token) null;
      int num = 1;
      for (Token nextNws = start.NextNWS; nextNws != null; nextNws = nextNws.NextNWS)
      {
        if (nextNws.Definition == definition)
          ++num;
        else if (nextNws.Definition == close && --num == 0)
        {
          end = nextNws;
          break;
        }
      }
      return new TokenPair(start, end);
    }

    public static TokenPair BuildGroupSimple(Token start, TokenKind close, TokenKind alternateOpen)
    {
      if (start == null)
        return new TokenPair();
      TokenKind definition = start.Definition;
      Token end = (Token) null;
      int num = 1;
      for (Token nextNws = start.NextNWS; nextNws != null; nextNws = nextNws.NextNWS)
      {
        if (nextNws.Definition == definition || nextNws.Definition == alternateOpen)
          ++num;
        else if (nextNws.Definition == close && --num == 0)
        {
          end = nextNws;
          break;
        }
      }
      return new TokenPair(start, end);
    }

    public static TokenPair BuildReverseGroupSimple(Token end, TokenKind open)
    {
      if (end == null)
        return new TokenPair();
      TokenKind definition = end.Definition;
      Token start = (Token) null;
      int num = -1;
      for (Token previousNws = end.PreviousNWS; previousNws != null; previousNws = previousNws.PreviousNWS)
      {
        if (previousNws.Definition == open)
        {
          if (++num == 0)
          {
            start = previousNws;
            break;
          }
        }
        else if (previousNws.Definition == definition && --num == 0)
        {
          start = previousNws;
          break;
        }
      }
      return new TokenPair(start, end);
    }
  }
}
