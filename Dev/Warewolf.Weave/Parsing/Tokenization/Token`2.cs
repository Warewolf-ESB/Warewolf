// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.Token`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public class Token<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    public string Source;
    public T Previous;
    public T PreviousNWS;
    public T PreviousWS;
    public T Next;
    public T NextNWS;
    public T NextWS;
    public int TokenIndex;
    public int SourceIndex;
    public int SourceLength;
    public K Definition;

    public string Content => this.Source != null && this.SourceLength > 0 ? this.Source.Substring(this.SourceIndex, this.SourceLength) : "(empty)";

    public string GetContentBetween(T other) => this.SourceIndex >= other.SourceIndex ? this.Source.Substring(other.SourceIndex, this.SourceIndex - other.SourceIndex + this.SourceLength) : this.Source.Substring(this.SourceIndex, other.SourceIndex - this.SourceIndex + other.SourceLength);

    public ParseEventLogToken ToParseEventLogToken() => new ParseEventLogToken()
    {
      SourceIndex = this.SourceIndex,
      SourceLength = this.SourceLength,
      Definition = (TokenDefinition) this.Definition,
      TokenIndex = this.TokenIndex,
      Contents = this.Content
    };

    public static bool operator <(Token<T, K> l, Token<T, K> r)
    {
      if (l == r || l == null)
        return false;
      return r == null || l.SourceIndex < r.SourceIndex;
    }

    public static bool operator <=(Token<T, K> l, Token<T, K> r)
    {
      if (l == r)
        return true;
      if (l == null)
        return false;
      return r == null || l.SourceIndex <= r.SourceIndex;
    }

    public static bool operator >(Token<T, K> l, Token<T, K> r)
    {
      if (l == r || l == null)
        return false;
      return r == null || l.SourceIndex > r.SourceIndex;
    }

    public static bool operator >=(Token<T, K> l, Token<T, K> r)
    {
      if (l == r)
        return true;
      if (l == null)
        return false;
      return r == null || l.SourceIndex >= r.SourceIndex;
    }
  }
}
