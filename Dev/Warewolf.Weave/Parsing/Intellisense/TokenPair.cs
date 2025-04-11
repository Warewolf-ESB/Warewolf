// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.TokenPair
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public struct TokenPair
  {
    public Token Start;
    public Token End;

    public string Content => this.Start.Source.Substring(this.Start.SourceIndex, this.End.SourceIndex + this.End.SourceLength - this.Start.SourceIndex);

    public TokenPair(TokenPair pair)
    {
      this.Start = pair.Start;
      this.End = pair.End;
    }

    public TokenPair(Token token)
    {
      this.Start = token;
      this.End = token;
    }

    public TokenPair(Token start, Token end)
    {
      this.Start = start;
      this.End = end;
    }

    public bool Contains(int sourceIndex)
    {
      sourceIndex -= this.Start.SourceIndex;
      return sourceIndex >= 0 && sourceIndex < this.End.SourceIndex + this.End.SourceLength - this.Start.SourceIndex;
    }

    public static implicit operator TokenPair(Token token) => new TokenPair(token);
  }
}
