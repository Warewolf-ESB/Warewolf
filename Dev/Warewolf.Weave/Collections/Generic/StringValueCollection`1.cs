// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueCollection`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Data;
using System.Diagnostics;

namespace System.Collections.Generic
{
  [DebuggerDisplay("Count = {Count}")]
  [DebuggerTypeProxy(typeof (StringValueCollectionDebugView<>))]
  [Serializable]
  internal class StringValueCollection<T> : 
    StringValueCollectionBase,
    IDictionary<string, T>,
    ICollection<KeyValuePair<string, T>>,
    IEnumerable<KeyValuePair<string, T>>,
    IEnumerable
  {
    protected bool _raiseEvents;
    private StringValueCollectionImplementation _impl;
    private StringValueCollectionBase.KeyCollection _keys;
    private StringValueCollection<T>.ValueCollection _values;
    private bool _hasNullValue;
    private T _nullValue;
    private ExposedList<StringValueCollection<T>.Entry> _items;

    public override int Count => this._items.Count;

    public int Capacity
    {
      get => this._items.Capacity;
      set => this._items.Capacity = value;
    }

    public T this[TPKey lookup, string key] => this.LocateHard(lookup, key).Value;

    public T this[string key]
    {
      get => this.LocateHard(key).Value;
      set => this.AddInternal(key, value, true, false);
    }

    public T this[int index] => this._items[index].Value;

    public StringValueCollectionBase.KeyCollection Keys => this._keys ?? (this._keys = new StringValueCollectionBase.KeyCollection((StringValueCollectionBase) this));

    public StringValueCollection<T>.ValueCollection Values => this._values ?? (this._values = new StringValueCollection<T>.ValueCollection(this));

    bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

    ICollection<string> IDictionary<string, T>.Keys => (ICollection<string>) this.Keys;

    ICollection<T> IDictionary<string, T>.Values => (ICollection<T>) this.Values;

    public StringValueCollection()
    {
      this._impl = (StringValueCollectionImplementation) new StringValueInt16CollectionImplementation();
      this._items = new ExposedList<StringValueCollection<T>.Entry>();
      this._raiseEvents = false;
    }

    public StringValueCollection(int capacity)
    {
      this._impl = capacity < 32759 ? (StringValueCollectionImplementation) new StringValueInt16CollectionImplementation(capacity) : (StringValueCollectionImplementation) new StringValueInt32CollectionImplementation(capacity);
      this._items = new ExposedList<StringValueCollection<T>.Entry>();
      this._raiseEvents = false;
    }

    public StringValueCollection(T nullValue)
    {
      this._impl = (StringValueCollectionImplementation) new StringValueInt16CollectionImplementation();
      this._items = new ExposedList<StringValueCollection<T>.Entry>();
      this.SetNullValue(nullValue);
      this._raiseEvents = false;
    }

    public bool TryGetValue(string key, out T value)
    {
      if (this._items.Count == 0 || string.IsNullOrEmpty(key))
      {
        value = default (T);
        return false;
      }
      StringValueCollection<T>.Entry entry = this.Locate(key);
      value = entry.Value;
      return !entry.IsInvalid;
    }

    public bool TryGetLookupKey(string key, out TPKey value)
    {
      if (this._items.Count == 0 || string.IsNullOrEmpty(key))
      {
        value = TPKey.Invalid;
        return false;
      }
      int num = this._impl.IndexOfEntryTier(key, 0, key.Length);
      if (num == -1)
      {
        value = TPKey.Invalid;
        return false;
      }
      value = new TPKey(num + 1);
      return true;
    }

    public StringValueCollection<T>.Enumerator GetEnumerator() => new StringValueCollection<T>.Enumerator(this);

    IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator() => (IEnumerator<KeyValuePair<string, T>>) new StringValueCollection<T>.Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    protected override string GetEntryKey(int index)
    {
      StringValueCollection<T>.Entry underlying = this._items.UnderlyingArray[index];
      return this._impl.GetEntryKey(index, underlying.Tier, underlying.Index);
    }

    public void SetNullValue(T value)
    {
      this._nullValue = value;
      this._hasNullValue = true;
    }

    public void UnsetNullValue()
    {
      this._nullValue = default (T);
      this._hasNullValue = false;
    }

    void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item) => this.AddInternal(item.Key, item.Value, false, false);

    public bool TryAdd(string key, T value) => this.AddInternal(key, value, false, true);

    public void Add(string key, T value) => this.AddInternal(key, value, false, false);

    private bool AddInternal(string key, T value, bool overwrite, bool soft)
    {
      if (string.IsNullOrEmpty(key))
      {
        if (soft)
          return false;
        throw new ArgumentNullException(nameof (key));
      }
      if (this._raiseEvents && !this.CanAddValue(key, value))
        return false;
      if (this._impl is StringValueInt16CollectionImplementation && this._impl.Size + key.Length >= this._impl.MaxCapacity)
        this._impl = (StringValueCollectionImplementation) new StringValueInt32CollectionImplementation(this._impl as StringValueInt16CollectionImplementation);
      int resultTier;
      int resultIndex;
      if (!this._impl.Add(key, overwrite, this._items.Count, out resultTier, out resultIndex))
      {
        if (soft)
          return false;
        throw new ArgumentException("An item with the same key has already been added.");
      }
      if (resultTier == -1)
      {
        if (this._raiseEvents)
        {
          if (!this.CanSetValue(key, this._items.UnderlyingArray[resultIndex].Value, value))
            return false;
          this._items.UnderlyingArray[resultIndex].Value = value = this.AcquireValue(key, value);
          this.OnValueAdded(key, value);
        }
        else
          this._items.UnderlyingArray[resultIndex].Value = value;
      }
      else if (this._raiseEvents)
      {
        value = this.AcquireValue(key, value);
        this._items.Add(new StringValueCollection<T>.Entry(value, resultTier, resultIndex));
        this.OnValueAdded(key, value);
      }
      else
        this._items.Add(new StringValueCollection<T>.Entry(value, resultTier, resultIndex));
      ++this._version;
      return true;
    }

    protected virtual bool CanAddValue(string key, T value) => true;

    protected virtual bool CanSetValue(string key, T currentValue, T newValue) => true;

    protected virtual T AcquireValue(string key, T input) => input;

    protected virtual void OnValueAdded(string key, T input)
    {
    }

    bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
    {
      if (this._items.Count == 0 || string.IsNullOrEmpty(item.Key))
        return false;
      StringValueCollection<T>.Entry entry = this.Locate(item.Key);
      return !entry.IsInvalid && EqualityComparer<T>.Default.Equals(entry.Value, item.Value) && this.Remove(item.Key, entry);
    }

    public bool Remove(string key) => this._items.Count != 0 && !string.IsNullOrEmpty(key) && this.Remove(key, this.Locate(key));

    private bool Remove(string key, StringValueCollection<T>.Entry entry)
    {
      if (entry.IsInvalid || this._raiseEvents && !this.CanRemoveValue(key, entry.Value))
        return false;
      StringValueCollection<T>.Entry entry1 = entry;
      int itemIndex = this._impl.BeginRemove(key, entry.Tier, entry.Index);
      StringValueCollection<T>.Entry[] underlyingArray = this._items.UnderlyingArray;
      int lastItemIndex = this._items.Count - 1;
      if (lastItemIndex != itemIndex)
      {
        entry = underlyingArray[lastItemIndex];
        this._impl.EndRemove(entry.Tier, entry.Index, itemIndex, lastItemIndex);
        underlyingArray[itemIndex] = entry;
        underlyingArray[lastItemIndex] = new StringValueCollection<T>.Entry();
        --this._items.Count;
      }
      else
      {
        underlyingArray[itemIndex] = new StringValueCollection<T>.Entry();
        --this._items.Count;
        this._impl.EndRemove(-1, (short) 0, 0, lastItemIndex);
      }
      if (this._raiseEvents)
        this.OnValueRemoved(key, entry1.Value);
      ++this._version;
      return true;
    }

    public void Clear()
    {
      if (this._raiseEvents)
      {
        if (!this.CanClear())
          return;
        this.OnBeforeCleared();
      }
      this._impl.Clear();
      this._items.Clear();
      ++this._version;
    }

    protected virtual bool CanRemoveValue(string key, T value) => true;

    protected virtual void OnValueRemoved(string key, T value)
    {
    }

    protected virtual bool CanClear() => true;

    protected virtual void OnBeforeCleared()
    {
    }

    bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
    {
      if (this._items.Count == 0 || string.IsNullOrEmpty(item.Key))
        return false;
      StringValueCollection<T>.Entry entry = this.Locate(item.Key);
      return !entry.IsInvalid && EqualityComparer<T>.Default.Equals(entry.Value, item.Value);
    }

    public override bool ContainsKey(string key) => this.IndexOf(key) != -1;

    public bool ContainsValue(T value) => this.IndexOf(value) != -1;

    public int IndexOf(T value)
    {
      if (this._items.Count == 0)
        return -1;
      StringValueCollection<T>.Entry[] underlyingArray = this._items.UnderlyingArray;
      int count = this._items.Count;
      if ((object) value == null)
      {
        for (int index = 0; index < count; ++index)
        {
          if ((object) underlyingArray[index].Value == null)
            return index;
        }
      }
      else
      {
        EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
        for (int index = 0; index < count; ++index)
        {
          if (equalityComparer.Equals(underlyingArray[index].Value, value))
            return index;
        }
      }
      return -1;
    }

    public int IndexOf(string key) => this._items.Count == 0 || string.IsNullOrEmpty(key) ? -1 : this._impl.IndexOfEntry(key, 0, key.Length, this._items.Count);

    protected internal int IndexOf(string key, int index, int count) => this._items.Count == 0 || string.IsNullOrEmpty(key) ? -1 : this._impl.IndexOfEntry(key, index, count, this._items.Count);

    protected internal int IndexOf(string key, int index, char seperator, out int consumed) => this._items.Count == 0 || string.IsNullOrEmpty(key) ? (consumed = -1) : this._impl.IndexOfEntry(key, index, seperator, this._items.Count, out consumed);

    private StringValueCollection<T>.Entry LocateHard(TPKey lookup, string key)
    {
      if (!lookup.Valid)
        throw new KeyNotFoundException();
      StringValueCollection<T>.Entry entry = this._items.Count == 0 || string.IsNullOrEmpty(key) ? StringValueCollection<T>.Entry.Invalid : this.Locate(lookup, key);
      if (entry.IsInvalid)
      {
        if (!this._hasNullValue)
          throw new KeyNotFoundException();
        entry.Value = this._nullValue;
      }
      return entry;
    }

    private StringValueCollection<T>.Entry LocateHard(string key)
    {
      StringValueCollection<T>.Entry entry = this._items.Count == 0 || string.IsNullOrEmpty(key) ? StringValueCollection<T>.Entry.Invalid : this.Locate(key);
      if (entry.IsInvalid)
      {
        if (!this._hasNullValue)
          throw new KeyNotFoundException();
        entry.Value = this._nullValue;
      }
      return entry;
    }

    private StringValueCollection<T>.Entry Locate(TPKey lookup, string key)
    {
      int index = this._impl.IndexOfEntry(lookup.Value - 1, key, 0, key.Length, this._items.Count);
      return index != -1 ? this._items[index] : StringValueCollection<T>.Entry.Invalid;
    }

    private StringValueCollection<T>.Entry Locate(string key)
    {
      int index = this._impl.IndexOfEntry(key, 0, key.Length, this._items.Count);
      return index != -1 ? this._items[index] : StringValueCollection<T>.Entry.Invalid;
    }

    void ICollection<KeyValuePair<string, T>>.CopyTo(
      KeyValuePair<string, T>[] array,
      int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (arrayIndex < 0 || arrayIndex > array.Length)
        throw new ArgumentOutOfRangeException(nameof (arrayIndex));
      if (array.Length - arrayIndex < this._items.Count)
        throw new ArgumentException("Array too small.");
      StringValueCollection<T>.Entry[] underlyingArray = this._items.UnderlyingArray;
      int count = this._items.Count;
      for (int index = 0; index < count; ++index)
        array[arrayIndex + index] = new KeyValuePair<string, T>(this.GetEntryKey(index), underlyingArray[index].Value);
    }

    private struct Entry
    {
      public static readonly StringValueCollection<T>.Entry Invalid = new StringValueCollection<T>.Entry(default (T), -1, -1);
      public T Value;
      public int Tier;
      public short Index;

      public bool IsInvalid => this.Tier == -1 && this.Index == (short) -1;

      public Entry(T value, int tier, int index)
      {
        this.Value = value;
        this.Tier = tier;
        this.Index = (short) index;
      }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<string, T>>, IEnumerator, IDisposable
    {
      private int _index;
      private int _version;
      private StringValueCollection<T> _owner;
      private KeyValuePair<string, T> _current;

      public KeyValuePair<string, T> Current => this._current;

      object IEnumerator.Current => (object) this._current;

      internal Enumerator(StringValueCollection<T> owner)
      {
        this._owner = owner;
        this._version = this._owner._version;
        this._current = new KeyValuePair<string, T>();
        this._index = 0;
      }

      public bool MoveNext()
      {
        if (this._version != this._owner._version)
          throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
        if (this._index < this._owner._items.Count)
        {
          this._current = new KeyValuePair<string, T>(this._owner.GetEntryKey(this._index), this._owner._items.UnderlyingArray[this._index].Value);
          ++this._index;
          return true;
        }
        this._current = new KeyValuePair<string, T>();
        return false;
      }

      public void Reset()
      {
        if (this._version != this._owner._version)
          throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
        this._index = 0;
        this._current = new KeyValuePair<string, T>();
      }

      public void Dispose()
      {
        this._owner = (StringValueCollection<T>) null;
        this._version = -1;
        this._current = new KeyValuePair<string, T>();
      }
    }

    [DebuggerTypeProxy(typeof (KeyedValueValueCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class ValueCollection : ICollection<T>, IEnumerable<T>, IEnumerable
    {
      private StringValueCollection<T> _owner;

      public int Count => this._owner.Count;

      bool ICollection<T>.IsReadOnly => true;

      internal ValueCollection(StringValueCollection<T> owner) => this._owner = owner;

      public StringValueCollection<T>.ValueCollection.Enumerator GetEnumerator() => new StringValueCollection<T>.ValueCollection.Enumerator(this._owner);

      IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>) new StringValueCollection<T>.ValueCollection.Enumerator(this._owner);

      IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new StringValueCollection<T>.ValueCollection.Enumerator(this._owner);

      void ICollection<T>.Add(T item) => throw new ReadOnlyException("Collection is read-only.");

      bool ICollection<T>.Remove(T item) => throw new ReadOnlyException("Collection is read-only.");

      void ICollection<T>.Clear() => throw new ReadOnlyException("Collection is read-only.");

      bool ICollection<T>.Contains(T item) => this._owner.ContainsValue(item);

      public void CopyTo(T[] array, int arrayIndex)
      {
        if (array == null)
          throw new ArgumentNullException(nameof (array));
        if (arrayIndex < 0 || arrayIndex > array.Length)
          throw new ArgumentOutOfRangeException(nameof (arrayIndex));
        if (array.Length - arrayIndex < this._owner.Count)
          throw new ArgumentException("Array too small.");
        StringValueCollection<T>.Entry[] underlyingArray = this._owner._items.UnderlyingArray;
        int count = this._owner.Count;
        for (int index = 0; index < count; ++index)
          array[index + arrayIndex] = underlyingArray[index].Value;
      }

      public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
      {
        private int _index;
        private int _version;
        private StringValueCollection<T> _owner;
        private T _current;

        public T Current => this._current;

        object IEnumerator.Current => (object) this._current;

        internal Enumerator(StringValueCollection<T> owner)
        {
          this._owner = owner;
          this._version = this._owner._version;
          this._current = default (T);
          this._index = 0;
        }

        public bool MoveNext()
        {
          if (this._version != this._owner._version)
            throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
          if (this._index < this._owner.Count)
          {
            this._current = this._owner._items[this._index].Value;
            ++this._index;
            return true;
          }
          this._current = default (T);
          return false;
        }

        public void Reset()
        {
          if (this._version != this._owner._version)
            throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
          this._index = 0;
          this._current = default (T);
        }

        public void Dispose()
        {
          this._owner = (StringValueCollection<T>) null;
          this._version = -1;
          this._current = default (T);
        }
      }
    }
  }
}
