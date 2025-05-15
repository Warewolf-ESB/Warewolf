// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.IndexedList`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Generic
{
  [DebuggerTypeProxy(typeof (OneDimensionCollectionDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  [Serializable]
  public class IndexedList<T> : 
    IList<T>,
    ICollection<T>,
    IEnumerable<T>,
    IEnumerable,
    IList,
    ICollection
    where T : class, IIndexed
  {
    private const int _defaultCapacity = 4;
    private static T[] _emptyArray = new T[0];
    protected T[] _items;
    protected int _count;
    protected int _capacity;
    [NonSerialized]
    protected object _syncRoot;
    protected int _version;

    public int Count => this._count;

    public int Capacity
    {
      get => this._capacity;
      set => this.SetCapacity(value);
    }

    public T this[int index]
    {
      get => this._items[index];
      set => this.SetIndex(index, value);
    }

    bool ICollection<T>.IsReadOnly => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot
    {
      get
      {
        if (this._syncRoot == null)
          Interlocked.CompareExchange(ref this._syncRoot, new object(), (object) null);
        return this._syncRoot;
      }
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index]
    {
      get => (object) this[index];
      set => this[index] = (T) value;
    }

    public IndexedList() => this._items = IndexedList<T>._emptyArray;

    public IndexedList(IEnumerable<T> collection)
    {
      if (collection == null)
        return;
      this.AddRange(collection);
    }

    public IndexedList(int capacity) => this._items = capacity >= 0 ? new T[this._capacity = capacity] : throw new ArgumentOutOfRangeException(nameof (capacity), "Capacity must be a non-negative integer.");

    public T[] GetInternalArray() => this._items ?? IndexedList<T>._emptyArray;

    private void SetIndex(int index, T value)
    {
      if (index < 0 || index >= this._count)
        throw new ArgumentOutOfRangeException(nameof (index));
      if ((object) value == null)
        this.RemoveAt(index);
      this._items[index].Index = -1;
      this._items[index] = value;
      value.Index = index;
    }

    private void SetCapacity(int value)
    {
      if (value == this._capacity)
        return;
      if (value < this._count)
        throw new ArgumentOutOfRangeException(nameof (value), "Capacity cannot be less than the size of the collection.");
      if (value > 0)
      {
        T[] destinationArray = new T[value];
        Array.Copy((Array) this._items, 0, (Array) destinationArray, 0, this._count);
        this._items = destinationArray;
        this._capacity = value;
      }
      else
      {
        this._items = IndexedList<T>._emptyArray;
        this._capacity = 0;
      }
    }

    public void Add(T item)
    {
      if ((object) item == null)
        return;
      this.EnsureCapacity(1);
      this._items[this._count] = item;
      item.Index = this._count++;
      ++this._version;
    }

    public void AddRange(IEnumerable<T> collection)
    {
      if (collection == null)
        return;
      if (collection is T[] array)
      {
        this.AddRange(array);
      }
      else
      {
        foreach (T obj in collection)
          this.Add(obj);
      }
    }

    public void AddRange(T[] array)
    {
      if (array == null)
        return;
      this.AddRange(array, 0, array.Length);
    }

    public void AddRange(T[] array, int index, int length)
    {
      if (array == null || length == 0)
        return;
      this.EnsureCapacity(length);
      T obj1 = default (T);
      for (int index1 = 0; index1 < length; ++index1)
      {
        T obj2;
        if ((object) (obj2 = array[index1 + index]) != null)
        {
          this._items[this._count] = obj2;
          obj2.Index = this._count++;
        }
      }
      ++this._version;
    }

    int IList.Add(object item)
    {
      this.Add(item as T);
      return this._count - 1;
    }

    public void Insert(int index, T item) => throw new NotSupportedException("Insert operation is not supported by this collection.");

    public void InsertRange(int index, IEnumerable<T> collection) => throw new NotSupportedException("Insert operation is not supported by this collection.");

    void IList.Insert(int index, object item) => throw new NotSupportedException("Insert operation is not supported by this collection.");

    private void EnsureCapacity(int amount)
    {
      if (this._count + amount <= this._capacity)
        return;
      if (this._count == 0)
      {
        this._items = new T[this._capacity = Math.Max(4, amount)];
      }
      else
      {
        int length = Math.Max(this._capacity * 2, this._count + amount);
        T[] destinationArray = new T[length];
        Array.Copy((Array) this._items, (Array) destinationArray, this._count);
        this._items = destinationArray;
        this._capacity = length;
      }
    }

    public bool Remove(T item)
    {
      if (this._count == 0 || (object) item == null || item.Index < 0 || item.Index >= this._count || (object) item != (object) this._items[item.Index])
        return false;
      this.RemoveAt(item.Index);
      return true;
    }

    public void RemoveAt(int index)
    {
      if (index >= this._count)
        return;
      --this._count;
      if (this._count > index)
      {
        T obj = this._items[this._count];
        this._items[index] = obj;
        obj.Index = index;
      }
      else
        this._items[index] = default (T);
      ++this._version;
    }

    void IList.Remove(object item)
    {
      if (this._count == 0 || !(item is IIndexed indexed) || indexed.Index < 0 || indexed.Index >= this._count || indexed != (object) this._items[indexed.Index])
        return;
      this.RemoveAt(indexed.Index);
    }

    public bool Contains(T item) => this._count != 0 && (object) item != null && item.Index >= 0 && item.Index < this._count && (object) item == (object) this._items[item.Index];

    public int IndexOf(T item) => this._count == 0 || (object) item == null || item.Index < 0 || item.Index >= this._count || (object) item != (object) this._items[item.Index] ? -1 : item.Index;

    bool IList.Contains(object item) => this._count != 0 && item is IIndexed indexed && indexed.Index >= 0 && indexed.Index < this._count && indexed == (object) this._items[indexed.Index];

    int IList.IndexOf(object item) => this._count == 0 || !(item is IIndexed indexed) || indexed.Index < 0 || indexed.Index >= this._count || indexed != (object) this._items[indexed.Index] ? -1 : indexed.Index;

    public void CopyTo(T[] array) => this.CopyTo(array, 0);

    public void CopyTo(T[] array, int arrayIndex) => Array.Copy((Array) this._items, 0, (Array) array, arrayIndex, this._count);

    public void CopyTo(int index, T[] array, int arrayIndex, int count) => Array.Copy((Array) this._items, index, (Array) array, arrayIndex, count);

    void ICollection.CopyTo(Array array, int arrayIndex) => Array.Copy((Array) this._items, 0, array, arrayIndex, this._count);

    public void ForEach(Action<T> action)
    {
      if (action == null || this._count == 0)
        return;
      for (int index = 0; index < this._items.Length && index < this._count; ++index)
        action(this._items[index]);
    }

    public bool TrueForAll(Predicate<T> match)
    {
      if (match == null)
        throw new ArgumentNullException(nameof (match));
      if (this._count == 0)
        return true;
      for (int index = 0; index < this._items.Length && index < this._count; ++index)
      {
        if (!match(this._items[index]))
          return false;
      }
      return true;
    }

    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>) new IndexedList<T>.Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>) new IndexedList<T>.Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new IndexedList<T>.Enumerator(this);

    public T[] ToArray()
    {
      if (this._count == 0)
        return new T[0];
      T[] destinationArray = new T[this._count];
      Array.Copy((Array) this._items, 0, (Array) destinationArray, 0, this._count);
      return destinationArray;
    }

    public void TrimExcess()
    {
      if (this._capacity == 0 || this._count >= (int) ((double) this._capacity * 0.9))
        return;
      this.SetCapacity(this._count);
    }

    public ReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>((IList<T>) this);

    public void Clear()
    {
      if (this._count > 0)
      {
        Array.Clear((Array) this._items, 0, this._count);
        this._count = 0;
      }
      ++this._version;
    }

    [Serializable]
    public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
      private IndexedList<T> list;
      private int index;
      private int version;
      private T current;

      public T Current => this.current;

      object IEnumerator.Current
      {
        get
        {
          if (this.index == 0 || this.index == this.list._count + 1)
            throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
          return (object) this.Current;
        }
      }

      internal Enumerator(IndexedList<T> list)
      {
        this.list = list;
        this.index = 0;
        this.version = list._version;
        this.current = default (T);
      }

      public bool MoveNext()
      {
        if (this.version != this.list._version || this.index >= this.list._count)
          return this.MoveNextRare();
        this.current = this.list._items[this.index];
        ++this.index;
        return true;
      }

      private bool MoveNextRare()
      {
        if (this.version != this.list._version)
          throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
        this.index = this.list._count + 1;
        this.current = default (T);
        return false;
      }

      void IEnumerator.Reset()
      {
        if (this.version != this.list._version)
          throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
        this.index = 0;
        this.current = default (T);
      }

      public void Dispose()
      {
      }
    }
  }
}
