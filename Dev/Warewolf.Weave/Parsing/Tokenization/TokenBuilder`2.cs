// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenBuilder`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Parsing.Tokenization
{
  internal sealed class TokenBuilder<T, K> : ITokenBuilder
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    private string _source;
    private T _previous;
    private T _previousNWS;
    private T _previousWS;
    private ExposedList<T> _contents;

    internal ExposedList<T> Contents => this._contents;

    internal TokenBuilder(string source)
    {
      this._source = source;
      this._contents = new ExposedList<T>();
    }

    public void Append(TokenDefinition definition) => this.Append(0, 0, (K) definition);

    public T Append(int index, int length, K definition)
    {
      T obj1 = new T();
      obj1.Source = this._source;
      obj1.TokenIndex = this._contents.Count;
      obj1.SourceIndex = index;
      obj1.SourceLength = length;
      obj1.Definition = definition;
      obj1.Previous = this._previous;
      obj1.PreviousNWS = this._previousNWS;
      obj1.PreviousWS = this._previousWS;
      T obj2 = obj1;
      if ((object) this._previous != null)
        this._previous.Next = obj2;
      if (definition.IsWhitespace)
      {
        for (int tokenIndex = (object) this._previousWS != null ? this._previousWS.TokenIndex : 0; tokenIndex < this._contents.Count; ++tokenIndex)
          this._contents[tokenIndex].NextWS = obj2;
        this._previousWS = obj2;
      }
      else
      {
        for (int tokenIndex = (object) this._previousNWS != null ? this._previousNWS.TokenIndex : 0; tokenIndex < this._contents.Count; ++tokenIndex)
          this._contents[tokenIndex].NextNWS = obj2;
        this._previousNWS = obj2;
      }
      this._previous = obj2;
      this._contents.Add(obj2);
      return obj2;
    }
  }
}
