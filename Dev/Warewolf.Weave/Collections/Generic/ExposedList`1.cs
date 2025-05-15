// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.ExposedList`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Generic
{
  [DebuggerDisplay("Count = {Count}")]
  [DebuggerTypeProxy(typeof (OneDimensionCollectionDebugView<>))]
  [Serializable]
  internal sealed class ExposedList<T> : 
    IList<T>,
    ICollection<T>,
    IEnumerable<T>,
    IEnumerable,
    IList,
    ICollection
  {
    private const int _defaultCapacity = 4;
    private static bool _isValueType = typeof (T).IsValueType;
    private static T[] _emptyArray = new T[0];
    private T[] _items;
    private int _capacity;
    private int _size;
    [NonSerialized]
    private object _syncRoot;
    private int _version;

    public int Count
    {
      get => this._size;
      internal set => this._size = value;
    }

    public int Capacity
    {
      get => this._capacity;
      set => this.SetCapacity(value);
    }

    public T this[int index]
    {
      get => index < this._size ? this._items[index] : throw new ArgumentOutOfRangeException();
      set
      {
        if (index >= this._size)
          throw new ArgumentOutOfRangeException();
        this._items[index] = value;
        ++this._version;
      }
    }

    public T[] UnderlyingArray => this._items;

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
      set
      {
        ExposedList<T>.VerifyValueType(value);
        this[index] = (T) value;
      }
    }

    private static void VerifyValueType(object value)
    {
      if (!ExposedList<T>.IsCompatibleObject(value))
        throw new ArgumentException("invalid type");
    }

    private static bool IsCompatibleObject(object value) => value is T || value == null && !ExposedList<T>._isValueType;

    public ExposedList() => this._items = ExposedList<T>._emptyArray;

    public ExposedList(IEnumerable<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException(nameof (collection));
      if (collection is ICollection<T> objs)
      {
        int count = objs.Count;
        this._items = new T[this._capacity = count];
        objs.CopyTo(this._items, 0);
        this._size = count;
      }
      else
      {
        this._size = 0;
        this._items = new T[this._capacity = 4];
        foreach (T obj in collection)
          this.Add(obj);
      }
    }

    public ExposedList(int capacity) => this._items = new T[this._capacity = capacity];

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new ExposedList<T>.Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>) new ExposedList<T>.Enumerator(this);

    public ExposedList<T>.Enumerator GetEnumerator() => new ExposedList<T>.Enumerator(this);

    public ExposedList<T> GetRange(int index, int count)
    {
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
      ExposedList<T> range = new ExposedList<T>(count);
      Array.Copy((Array) this._items, index, (Array) range._items, 0, count);
      range._size = count;
      return range;
    }

    private void SetCapacity(int value)
    {
      if (value == this._capacity)
        return;
      if (value < this._size)
        throw new ArgumentOutOfRangeException(nameof (value));
      if (value > 0)
      {
        this._capacity = value;
        T[] destinationArray = new T[value];
        if (this._size > 0)
          Array.Copy((Array) this._items, 0, (Array) destinationArray, 0, this._size);
        this._items = destinationArray;
      }
      else
      {
        this._items = ExposedList<T>._emptyArray;
        this._capacity = 0;
      }
    }

    int IList.Add(object item)
    {
      ExposedList<T>.VerifyValueType(item);
      this.Add((T) item);
      return this._size - 1;
    }

    public void Add(T item)
    {
      if (this._size == this._capacity)
        this.EnsureCapacity(this._size + 1);
      this._items[this._size++] = item;
      ++this._version;
    }

    public void AddRange(IEnumerable<T> collection) => this.InsertRange(this._size, collection);

    void IList.Insert(int index, object item)
    {
      ExposedList<T>.VerifyValueType(item);
      this.Insert(index, (T) item);
    }

    public void Insert(int index, T item)
    {
      if (index > this._size)
        throw new ArgumentOutOfRangeException(nameof (index));
      if (this._size == this._capacity)
        this.EnsureCapacity(this._size + 1);
      if (index < this._size)
        Array.Copy((Array) this._items, index, (Array) this._items, index + 1, this._size - index);
      this._items[index] = item;
      ++this._size;
      ++this._version;
    }

    public void InsertRange(int destinationIndex, T[] source, int sourceIndex, int sourceLength)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (destinationIndex > this._size)
        throw new ArgumentOutOfRangeException(nameof (destinationIndex));
      if (sourceLength <= 0)
        return;
      this.EnsureCapacity(this._size + sourceLength);
      if (destinationIndex < this._size)
        Array.Copy((Array) this._items, destinationIndex, (Array) this._items, destinationIndex + sourceLength, this._size - destinationIndex);
      Array.Copy((Array) source, sourceIndex, (Array) this._items, destinationIndex, sourceLength);
      this._size += sourceLength;
      ++this._version;
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException(nameof (collection));
      if (index > this._size)
        throw new ArgumentOutOfRangeException(nameof (index));
      if (collection is ICollection<T> objs)
      {
        int count = objs.Count;
        if (count > 0)
        {
          this.EnsureCapacity(this._size + count);
          if (index < this._size)
            Array.Copy((Array) this._items, index, (Array) this._items, index + count, this._size - index);
          if (this == objs)
          {
            Array.Copy((Array) this._items, 0, (Array) this._items, index, index);
            Array.Copy((Array) this._items, index + count, (Array) this._items, index * 2, this._size - index);
          }
          else
          {
            T[] array = new T[count];
            objs.CopyTo(array, 0);
            array.CopyTo((Array) this._items, index);
          }
          this._size += count;
        }
      }
      else
      {
        foreach (T obj in collection)
          this.Insert(index++, obj);
      }
      ++this._version;
    }

    void IList.Remove(object item)
    {
      if (!ExposedList<T>.IsCompatibleObject(item))
        return;
      this.Remove((T) item);
    }

    public bool Remove(T item)
    {
      int index = this.IndexOf(item);
      if (index < 0)
        return false;
      this.RemoveAt(index);
      return true;
    }

    public void RemoveAt(int index)
    {
      if (index >= this._size)
        throw new ArgumentOutOfRangeException();
      --this._size;
      if (index < this._size)
        Array.Copy((Array) this._items, index + 1, (Array) this._items, index, this._size - index);
      this._items[this._size] = default (T);
      ++this._version;
    }

    public void RemoveRange(int index, int count)
    {
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
      if (count <= 0)
        return;
      this._size -= count;
      if (index < this._size)
        Array.Copy((Array) this._items, index + count, (Array) this._items, index, this._size - index);
      Array.Clear((Array) this._items, this._size, count);
      ++this._version;
    }

    public void Clear()
    {
      if (this._size > 0)
      {
        Array.Clear((Array) this._items, 0, this._size);
        this._size = 0;
      }
      ++this._version;
    }

    public int IndexOf(T item) => Array.IndexOf<T>(this._items, item, 0, this._size);

    public int IndexOf(T item, int index)
    {
      if (index > this._size)
        throw new ArgumentOutOfRangeException(nameof (index));
      return Array.IndexOf<T>(this._items, item, index, this._size - index);
    }

    public int IndexOf(T item, int index, int count)
    {
      if (index > this._size)
        throw new ArgumentOutOfRangeException(nameof (index));
      if (count < 0 || index > this._size - count)
        throw new ArgumentOutOfRangeException(nameof (count));
      return Array.IndexOf<T>(this._items, item, index, count);
    }

    int IList.IndexOf(object item) => ExposedList<T>.IsCompatibleObject(item) ? this.IndexOf((T) item) : -1;

    bool IList.Contains(object item) => ExposedList<T>.IsCompatibleObject(item) && this.Contains((T) item);

    public bool Contains(T item)
    {
      if ((object) item == null)
      {
        for (int index = 0; index < this._size; ++index)
        {
          if ((object) this._items[index] == null)
            return true;
        }
        return false;
      }
      EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
      for (int index = 0; index < this._size; ++index)
      {
        if (equalityComparer.Equals(this._items[index], item))
          return true;
      }
      return false;
    }

    public int BinarySearch(T item) => this.BinarySearch(0, this._size, item, (IComparer<T>) null);

    public int BinarySearch(T item, IComparer<T> comparer) => this.BinarySearch(0, this._size, item, comparer);

    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
      return Array.BinarySearch<T>(this._items, index, count, item, comparer);
    }

    public int LastIndexOf(T item) => this.LastIndexOf(item, this._size - 1, this._size);

    public int LastIndexOf(T item, int index) => index < this._size ? this.LastIndexOf(item, index, index + 1) : throw new ArgumentOutOfRangeException(nameof (index));

    public int LastIndexOf(T item, int index, int count)
    {
      if (this._size == 0)
        return -1;
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (index >= this._size || count > index + 1)
        throw new ArgumentOutOfRangeException(index >= this._size ? nameof (index) : nameof (count));
      return Array.LastIndexOf<T>(this._items, item, index, count);
    }

    public ReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>((IList<T>) this);

    public void Reverse() => this.Reverse(0, this._size);

    public void Reverse(int index, int count)
    {
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
			Array.Reverse(this._items, index, count);
      ++this._version;
    }

    public void CopyTo(T[] array) => this.CopyTo(array, 0);

    public void CopyTo(T[] array, int arrayIndex) => Array.Copy((Array) this._items, 0, (Array) array, arrayIndex, this._size);

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
      Array.Copy((Array) this._items, index, (Array) array, arrayIndex, count);
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      if (array != null && array.Rank != 1)
        throw new InvalidOperationException("Multidimensional arrays are not supported.");
      try
      {
        Array.Copy((Array) this._items, 0, array, arrayIndex, this._size);
      }
      catch (ArrayTypeMismatchException ex)
      {
        throw new ArgumentException("Invalid array type.");
      }
    }

    public void Sort() => this.Sort(0, this._size, (IComparer<T>) null);

    public void Sort(IComparer<T> comparer) => this.Sort(0, this._size, comparer);

    public void Sort(Comparison<T> comparison)
    {
      if (comparison == null)
        throw new ArgumentNullException(nameof (comparison));
      if (this._size <= 0)
        return;
      Array.Sort<T>(this._items, 0, this._size, (IComparer<T>) new ExposedList<T>.FunctorComparer(comparison));
    }

    public void Sort(int index, int count, IComparer<T> comparer)
    {
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException(index < 0 ? nameof (index) : nameof (count));
      if (this._size - index < count)
        throw new ArgumentException("invalid offset");
      Array.Sort<T>(this._items, index, count, comparer);
      ++this._version;
    }

    public void TrimExcess()
    {
      if (this._size >= (int) ((double) this._capacity * 0.9))
        return;
      this.SetCapacity(this._size);
    }

    public T[] ToArray()
    {
      T[] destinationArray = new T[this._size];
      Array.Copy((Array) this._items, 0, (Array) destinationArray, 0, this._size);
      return destinationArray;
    }

    private void EnsureCapacity(int min)
    {
      if (this._capacity >= min)
        return;
      int num = this._capacity == 0 ? 4 : this._capacity * 2;
      if (num < min)
        num = min;
      this.SetCapacity(num);
    }

    private sealed class FunctorComparer : IComparer<T>
    {
      private Comparison<T> _comparison;

      public FunctorComparer(Comparison<T> comparison) => this._comparison = comparison;

      public int Compare(T x, T y) => this._comparison(x, y);
    }

    [Serializable]
    public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
      private ExposedList<T> _list;
      private int _index;
      private int _version;
      private T _current;

      public T Current => this._current;

      object IEnumerator.Current
      {
        get
        {
          if (this._index == 0 || this._index == this._list._size + 1)
            throw new InvalidOperationException("Collection changed during foreach loop.");
          return (object) this._current;
        }
      }

      internal Enumerator(ExposedList<T> list)
      {
        this._list = list;
        this._index = 0;
        this._version = list._version;
        this._current = default (T);
      }

      public bool MoveNext()
      {
        ExposedList<T> list = this._list;
        if (this._version != list._version || this._index >= list._size)
          return this.MoveNextRare();
        this._current = list._items[this._index];
        ++this._index;
        return true;
      }

      private bool MoveNextRare()
      {
        if (this._version != this._list._version)
          throw new InvalidOperationException("Collection changed during foreach loop.");
        this._index = this._list._size + 1;
        this._current = default (T);
        return false;
      }

      void IEnumerator.Reset()
      {
        if (this._version != this._list._version)
          throw new InvalidOperationException("Collection changed during foreach loop.");
        this._index = 0;
        this._current = default (T);
      }

      public void Dispose()
      {
      }
    }
  }
}
