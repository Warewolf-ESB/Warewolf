// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenizerInputWalker`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public sealed class TokenizerInputWalker<T, K>
    where T : Token<T, K>, new()
    where K : TokenDefinition
  {
    private TokenizationExecutionStore<T, K> _store;
    private string _source;
    private byte[] _data;
    private int _length;
    private int _range;
    private byte _previous;
    private byte _current;
    private byte _next;
    private int _origin;
    private int _index;
    private int _position;
    private int _end;
    private int _offset;

    public string Source => this._source;

    public int Length => this._length;

    public byte Previous
    {
      get => this._previous;
      set => this._previous = value;
    }

    public byte Current
    {
      get => this._current;
      set => this._current = value;
    }

    public byte Next
    {
      get => this._next;
      set => this._next = value;
    }

    public int Origin
    {
      get => this._origin;
      set => this._origin = value;
    }

    public int Index => this._index;

    public int Position => this._position;

    public int End
    {
      get => this._end;
      set => this._end = value;
    }

    public byte this[int index] => this._data[index - this._offset];

    internal TokenizerInputWalker(TokenizationExecutionStore<T, K> tokenizer) => this._store = tokenizer;

    internal void Reset(string source) => this.Reset(source, 0, source.Length);

    internal unsafe void Reset(string source, int index, int length)
    {
      this._source = source;
      this._offset = index;
      this._range = (this._length = length) + this._offset + 1;
      this._data = new byte[this._length + (int) byte.MaxValue];
      IntPtr num;
      if (source == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = source)
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      for (int index1 = 0; index1 < this._length; ++index1)
        this._data[index1] = (byte) chPtr1[index1 + this._offset];
      this._previous = this._next = (byte) 0;
      this._origin = this._index = this._position = this._offset;
      this._end = 0;
      this._current = this._data[0];
      this._length += this._offset;
    }

    internal bool BeginAdvance()
    {
      if (this._position >= this._range)
      {
        if (this._index < this._length)
          this._store.IdentifyKeyword(this._index, this._length - this._index);
        return false;
      }
      this._next = this._data[this._position + 1 - this._offset];
      return true;
    }

    internal bool EndAdvance(K advanceDefinition)
    {
      if (this._end == -1)
        return false;
      if (this._end != 0)
      {
        if (this._index < this._origin)
          this._store.IdentifyKeyword(this._index, this._origin - this._index);
        this._store.Builder.Append(this._origin, this._end - this._origin, advanceDefinition);
        this._index = this._end;
        if (--this._end != this._position)
        {
          this._position = this._end;
          this._current = this._data[this._position - this._offset];
          this._next = this._data[this._position + 1 - this._offset];
        }
        this._end = 0;
      }
      this._previous = this._current;
      this._current = this._next;
      ++this._position;
      return true;
    }
  }
}
