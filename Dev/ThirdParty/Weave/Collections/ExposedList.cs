
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.Generic
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(OneDimensionCollectionDebugView<>))]
    internal sealed class ExposedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        #region Constants
        private const int _defaultCapacity = 4;
        #endregion

        #region Static Members
        private static bool _isValueType = typeof(T).IsValueType;
        private static T[] _emptyArray = new T[0];

        private static void VerifyValueType(object value)
        {
            if (!IsCompatibleObject(value))
                throw new ArgumentException("invalid type");
        }

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && (value != null || _isValueType)) return false;
            return true;
        }
        #endregion

        #region Instance Fields
        private T[] _items;
        private int _capacity;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _version;
        #endregion

        #region Public Properties
        public int Count { get { return _size; } internal set { _size = value; } }
        public int Capacity { get { return _capacity; } set { SetCapacity(value); } }
        public T this[int index] { get { if (index >= _size) throw new ArgumentOutOfRangeException(); return _items[index]; } set { if (index >= _size) throw new ArgumentOutOfRangeException(); _items[index] = value; _version++; } }
        public T[] UnderlyingArray { get { return _items; } }

        bool ICollection<T>.IsReadOnly { get { return false; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null); return _syncRoot; } }
        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return false; } }
        object IList.this[int index] { get { return this[index]; } set { VerifyValueType(value); this[index] = (T)value; } }
        #endregion

        #region Constructors
        public ExposedList()
        {
            _items = _emptyArray;
        }

        public ExposedList(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            ICollection<T> is2 = collection as ICollection<T>;

            if (is2 != null)
            {
                int count = is2.Count;
                _items = new T[_capacity = count];
                is2.CopyTo(_items, 0);
                _size = count;
            }
            else
            {
                _size = 0;
                _items = new T[_capacity = 4];

                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                    while (enumerator.MoveNext())
                        Add(enumerator.Current);
            }
        }

        public ExposedList(int capacity)
        {
            _items = new T[_capacity = capacity];
        }
        #endregion

        #region [Get/Set] Handling
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator((ExposedList<T>)this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator((ExposedList<T>)this);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator((ExposedList<T>)this);
        }

        public ExposedList<T> GetRange(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if ((_size - index) < count) throw new ArgumentException("invalid offset");
            ExposedList<T> list = new ExposedList<T>(count);
            Array.Copy(_items, index, list._items, 0, count);
            list._size = count;
            return list;
        }

        private void SetCapacity(int value)
        {
            if (value != _capacity)
            {
                if (value < _size) throw new ArgumentOutOfRangeException("value");

                if (value > 0)
                {
                    _capacity = value;
                    T[] destinationArray = new T[value];
                    if (_size > 0) Array.Copy(_items, 0, destinationArray, 0, _size);
                    _items = destinationArray;
                }
                else
                {
                    _items = _emptyArray;
                    _capacity = 0;
                }
            }
        }
        #endregion

        #region Addition Handling
        int IList.Add(object item)
        {
            VerifyValueType(item);
            Add((T)item);
            return (_size - 1);
        }

        public void Add(T item)
        {
            if (_size == _capacity) EnsureCapacity(_size + 1);
            _items[_size++] = item;
            _version++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_size, collection);
        }

        void IList.Insert(int index, object item)
        {
            VerifyValueType(item);
            Insert(index, (T)item);
        }

        public void Insert(int index, T item)
        {
            if (index > _size) throw new ArgumentOutOfRangeException("index");
            if (_size == _capacity) EnsureCapacity(_size + 1);
            if (index < _size) Array.Copy(_items, index, _items, index + 1, _size - index);
            _items[index] = item;
            _size++;
            _version++;
        }

        public void InsertRange(int destinationIndex, T[] source, int sourceIndex, int sourceLength)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destinationIndex > _size) throw new ArgumentOutOfRangeException("destinationIndex");

            if (sourceLength > 0)
            {
                EnsureCapacity(_size + sourceLength);
                if (destinationIndex < _size) Array.Copy(_items, destinationIndex, _items, destinationIndex + sourceLength, _size - destinationIndex);
                Array.Copy(source, sourceIndex, _items, destinationIndex, sourceLength);
                _size += sourceLength;
                _version++;
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (index > _size) throw new ArgumentOutOfRangeException("index");
            ICollection<T> is2 = collection as ICollection<T>;

            if (is2 != null)
            {
                int count = is2.Count;

                if (count > 0)
                {
                    EnsureCapacity(_size + count);

                    if (index < _size) Array.Copy(_items, index, _items, index + count, _size - index);

                    if (this == is2)
                    {
                        Array.Copy(_items, 0, _items, index, index);
                        Array.Copy(_items, (int)(index + count), _items, (int)(index * 2), (int)(_size - index));
                    }
                    else
                    {
                        T[] array = new T[count];
                        is2.CopyTo(array, 0);
                        array.CopyTo(_items, index);
                    }
                    _size += count;
                }
            }
            else using (IEnumerator<T> enumerator = collection.GetEnumerator())
                    while (enumerator.MoveNext())
                        Insert(index++, enumerator.Current);

            _version++;
        }
        #endregion

        #region Subtraction Handling
        void IList.Remove(object item)
        {
            if (IsCompatibleObject(item)) Remove((T)item);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= _size) throw new ArgumentOutOfRangeException();
            _size--;
            if (index < _size) Array.Copy(_items, index + 1, _items, index, _size - index);
            _items[_size] = default(T);
            _version++;
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if ((_size - index) < count) throw new ArgumentException("invalid offset");

            if (count > 0)
            {
                _size -= count;
                if (index < _size) Array.Copy(_items, index + count, _items, index, _size - index);
                Array.Clear(_items, _size, count);
                _version++;
            }
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }

            _version++;
        }
        #endregion

        #region Search Handling
        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(_items, item, 0, _size);
        }

        public int IndexOf(T item, int index)
        {
            if (index > _size) throw new ArgumentOutOfRangeException("index");
            return Array.IndexOf<T>(_items, item, index, _size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index > _size) throw new ArgumentOutOfRangeException("index");
            if ((count < 0) || (index > (_size - count))) throw new ArgumentOutOfRangeException("count");
            return Array.IndexOf<T>(_items, item, index, count);
        }

        int IList.IndexOf(object item)
        {
            if (IsCompatibleObject(item)) return IndexOf((T)item);
            return -1;
        }

        bool IList.Contains(object item)
        {
            return (IsCompatibleObject(item) && Contains((T)item));
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < _size; j++)
                    if (_items[j] == null)
                        return true;

                return false;
            }

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < _size; i++)
                if (comparer.Equals(_items[i], item))
                    return true;

            return false;
        }

        public int BinarySearch(T item)
        {
            return BinarySearch(0, _size, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return this.BinarySearch(0, _size, item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if (_size - index < count) throw new ArgumentException("invalid offset");
            return Array.BinarySearch<T>(_items, index, count, item, comparer);
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOf(item, _size - 1, _size);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= _size) throw new ArgumentOutOfRangeException("index");
            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (_size == 0) return -1;
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if ((index >= _size) || (count > (index + 1))) throw new ArgumentOutOfRangeException(index >= _size ? "index" : "count");
            return Array.LastIndexOf<T>(_items, item, index, count);
        }
        #endregion

        #region Array Handling
        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }

        public void Reverse()
        {
            Reverse(0, _size);
        }

        public void Reverse(int index, int count)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if ((_size - index) < count) throw new ArgumentException("invalid offset");

            Array.Reverse(_items, index, count);
            _version++;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (_size - index < count) throw new ArgumentException("invalid offset");
            Array.Copy(_items, index, array, arrayIndex, count);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1)) throw new InvalidOperationException("Multidimensional arrays are not supported.");
            try { Array.Copy(_items, 0, array, arrayIndex, _size); }
            catch (ArrayTypeMismatchException) { throw new ArgumentException("Invalid array type."); }
        }

        public void Sort()
        {
            Sort(0, _size, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            Sort(0, _size, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");

            if (_size > 0)
            {
                IComparer<T> comparer = new FunctorComparer(comparison);
                Array.Sort<T>(_items, 0, _size, comparer);
            }
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            if ((_size - index) < count) throw new ArgumentException("invalid offset");
            Array.Sort<T>(_items, index, count, comparer);
            _version++;
        }

        public void TrimExcess()
        {
            int num = (int)(_capacity * 0.9);
            if (_size < num) SetCapacity(_size);
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[_size];
            Array.Copy(_items, 0, destinationArray, 0, _size);
            return destinationArray;
        }

        private void EnsureCapacity(int min)
        {
            if (_capacity < min)
            {
                int num = _capacity == 0 ? 4 : _capacity * 2;
                if (num < min) num = min;
                SetCapacity(num);
            }
        }
        #endregion

        #region FunctorComparer
        private sealed class FunctorComparer : IComparer<T>
        {
            private Comparison<T> _comparison;

            public FunctorComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }
        }
        #endregion

        #region Enumerator
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private ExposedList<T> _list;
            private int _index;
            private int _version;
            private T _current;

            public T Current { get { return _current; } }
            object IEnumerator.Current { get { if (_index == 0 || _index == (_list._size + 1)) throw new InvalidOperationException("Collection changed during foreach loop."); return _current; } }

            internal Enumerator(ExposedList<T> list)
            {
                _list = list;
                _index = 0;
                _version = list._version;
                _current = default(T);
            }

            public bool MoveNext()
            {
                ExposedList<T> list = _list;

                if (_version == list._version && _index < list._size)
                {
                    _current = list._items[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _list._version) throw new InvalidOperationException("Collection changed during foreach loop.");
                _index = _list._size + 1;
                _current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _list._version) throw new InvalidOperationException("Collection changed during foreach loop.");
                _index = 0;
                _current = default(T);
            }

            public void Dispose()
            {
            }
        }
        #endregion
    }
}
