// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueCollectionBase
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Data;
using System.Diagnostics;

namespace System.Collections.Generic
{
  internal abstract class StringValueCollectionBase
  {
    protected int _version;

    public abstract int Count { get; }

    internal StringValueCollectionBase()
    {
    }

    protected abstract string GetEntryKey(int index);

    public abstract bool ContainsKey(string key);

    [DebuggerTypeProxy(typeof (KeyedValueKeyCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class KeyCollection : ICollection<string>, IEnumerable<string>, IEnumerable
    {
      private StringValueCollectionBase _owner;

      public int Count => this._owner.Count;

      bool ICollection<string>.IsReadOnly => true;

      internal KeyCollection(StringValueCollectionBase owner) => this._owner = owner;

      public StringValueCollectionBase.KeyCollection.Enumerator GetEnumerator() => new StringValueCollectionBase.KeyCollection.Enumerator(this._owner);

      IEnumerator<string> IEnumerable<string>.GetEnumerator() => (IEnumerator<string>) new StringValueCollectionBase.KeyCollection.Enumerator(this._owner);

      IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new StringValueCollectionBase.KeyCollection.Enumerator(this._owner);

      void ICollection<string>.Add(string item) => throw new ReadOnlyException("Collection is read-only.");

      bool ICollection<string>.Remove(string item) => throw new ReadOnlyException("Collection is read-only.");

      void ICollection<string>.Clear() => throw new ReadOnlyException("Collection is read-only.");

      bool ICollection<string>.Contains(string item) => this._owner.ContainsKey(item);

      public void CopyTo(string[] array, int arrayIndex)
      {
        if (array == null)
          throw new ArgumentNullException(nameof (array));
        if (arrayIndex < 0 || arrayIndex > array.Length)
          throw new ArgumentOutOfRangeException(nameof (arrayIndex));
        if (array.Length - arrayIndex < this._owner.Count)
          throw new ArgumentException("Array too small.");
        int count = this._owner.Count;
        for (int index = 0; index < count; ++index)
          array[index + arrayIndex] = this._owner.GetEntryKey(index);
      }

      public struct Enumerator : IEnumerator<string>, IEnumerator, IDisposable
      {
        private int _index;
        private int _version;
        private StringValueCollectionBase _owner;
        private string _current;

        public string Current => this._current;

        object IEnumerator.Current => (object) this._current;

        internal Enumerator(StringValueCollectionBase owner)
        {
          this._owner = owner;
          this._version = this._owner._version;
          this._current = (string) null;
          this._index = 0;
        }

        public bool MoveNext()
        {
          if (this._version != this._owner._version)
            throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
          if (this._index < this._owner.Count)
          {
            this._current = this._owner.GetEntryKey(this._index);
            ++this._index;
            return true;
          }
          this._current = (string) null;
          return false;
        }

        public void Reset()
        {
          if (this._version != this._owner._version)
            throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
          this._index = 0;
          this._current = (string) null;
        }

        public void Dispose()
        {
          this._owner = (StringValueCollectionBase) null;
          this._version = -1;
          this._current = (string) null;
        }
      }
    }
  }
}
