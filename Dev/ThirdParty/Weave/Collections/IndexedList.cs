
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
    public class IndexedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable where T : class, IIndexed
    {
        #region Constants
        private const int _defaultCapacity = 4;
        #endregion

        #region Static Members
        private static T[] _emptyArray = new T[0];
        #endregion

        #region Instance Fields
        protected T[] _items;
        protected int _count;
        protected int _capacity;
        [NonSerialized]
        protected object _syncRoot;
        protected int _version;
        #endregion

        #region Public Properties
        public int Count { get { return _count; } }
        public int Capacity { get { return _capacity; } set { SetCapacity(value); } }
        public T this[int index] { get { return _items[index]; } set { SetIndex(index, value); } }

        bool ICollection<T>.IsReadOnly { get { return false; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null); return _syncRoot; } }
        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return false; } }
        object IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }
        #endregion

        #region Constructors
        public IndexedList()
        {
            _items = _emptyArray;
        }

        public IndexedList(IEnumerable<T> collection)
        {
            if (collection != null) AddRange(collection);
        }

        public IndexedList(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", "Capacity must be a non-negative integer.");
            _items = new T[_capacity = capacity];
        }
        #endregion

        #region [Get/Set] Handling
        public T[] GetInternalArray()
        {
            return _items ?? _emptyArray;
        }

        private void SetIndex(int index, T value)
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException("index");
            if (value == null) RemoveAt(index);
            _items[index].Index = -1;
            _items[index] = value;
            value.Index = index;
        }

        private void SetCapacity(int value)
        {
            if (value == _capacity) return;
            if (value < _count) throw new ArgumentOutOfRangeException("value", "Capacity cannot be less than the size of the collection.");

            if (value > 0)
            {
                T[] nItems = new T[value];
                Array.Copy(_items, 0, nItems, 0, _count);
                _items = nItems;
                _capacity = value;
            }
            else
            {
                _items = _emptyArray;
                _capacity = 0;
            }
        }
        #endregion

        #region [Add/Insert] Handling
        public void Add(T item)
        {
            if (item == null) return;
            EnsureCapacity(1);
            _items[_count] = item;
            item.Index = _count++;
            _version++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;
            T[] array = collection as T[];

            if (array != null)
                AddRange(array);
            else
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                    while (enumerator.MoveNext())
                        Add(enumerator.Current);
            }
        }

        public void AddRange(T[] array)
        {
            if (array == null) return;
            AddRange(array, 0, array.Length);
        }

        public void AddRange(T[] array, int index, int length)
        {
            if (array == null || length == 0) return;
            EnsureCapacity(length);
            T value = null;

            for (int i = 0; i < length; i++)
                if ((value = array[i + index]) != null)
                {
                    _items[_count] = value;
                    value.Index = _count++;
                }

            _version++;
        }

        int IList.Add(object item)
        {
            Add(item as T);
            return (_count - 1);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insert operation is not supported by this collection.");
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            throw new NotSupportedException("Insert operation is not supported by this collection.");
        }

        void IList.Insert(int index, object item)
        {
            throw new NotSupportedException("Insert operation is not supported by this collection.");
        }

        private void EnsureCapacity(int amount)
        {
            if (_count + amount > _capacity)
            {
                if (_count == 0)
                    _items = new T[_capacity = Math.Max(_defaultCapacity, amount)];
                else
                {
                    int nCapacity = Math.Max(_capacity * 2, _count + amount);
                    T[] nItems = new T[nCapacity];
                    Array.Copy(_items, nItems, _count);
                    _items = nItems;
                    _capacity = nCapacity;
                }
            }
        }
        #endregion

        #region Removal Handling
        public bool Remove(T item)
        {
            if (_count == 0 || item == null) return false;
            if (item.Index < 0 || item.Index >= _count) return false;

            if (item == _items[item.Index])
            {
                RemoveAt(item.Index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= _count) return;
            _count--;

            if (_count > index)
            {
                T moved = _items[_count];
                _items[index] = moved;
                moved.Index = index;
            }
            else _items[index] = null;

            _version++;
        }

        void IList.Remove(object item)
        {
            if (_count == 0) return;
            IIndexed indexed = item as IIndexed;
            if (indexed == null) return;
            if (indexed.Index < 0 || indexed.Index >= _count) return;
            if (indexed == _items[indexed.Index]) RemoveAt(indexed.Index);
        }
        #endregion

        #region [Contains/IndexOf] Handling
        public bool Contains(T item)
        {
            if (_count == 0 || item == null) return false;
            if (item.Index < 0 || item.Index >= _count) return false;
            return item == _items[item.Index];
        }

        public int IndexOf(T item)
        {
            if (_count == 0 || item == null) return -1;
            if (item.Index < 0 || item.Index >= _count) return -1;
            return item == _items[item.Index] ? item.Index : -1;
        }

        bool IList.Contains(object item)
        {
            if (_count == 0) return false;
            IIndexed indexed = item as IIndexed;
            if (indexed == null) return false;
            if (indexed.Index < 0 || indexed.Index >= _count) return false;
            return indexed == _items[indexed.Index];
        }

        int IList.IndexOf(object item)
        {
            if (_count == 0) return -1;
            IIndexed indexed = item as IIndexed;
            if (indexed == null) return -1;
            if (indexed.Index < 0 || indexed.Index >= _count) return -1;
            return indexed == _items[indexed.Index] ? indexed.Index : -1;
        }
        #endregion

        #region Copy Handling
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            Array.Copy(_items, index, array, arrayIndex, count);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _count);
        }
        #endregion

        #region Enumeration Handling
        public void ForEach(Action<T> action)
        {
            if (action == null || _count == 0) return;

            for (int i = 0; i < _items.Length; i++)
            {
                if (i >= _count) return;
                action(_items[i]);
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");
            if (_count == 0) return true;

            for (int i = 0; i < _items.Length; i++)
            {
                if (i >= _count) return true;
                if (!match(_items[i])) return false;
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region Array Handling
        public T[] ToArray()
        {
            if (_count == 0) return new T[0];
            T[] destinationArray = new T[_count];
            Array.Copy(_items, 0, destinationArray, 0, _count);
            return destinationArray;
        }

        public void TrimExcess()
        {
            if (_capacity == 0) return;
            int num = (int)(_capacity * 0.9);
            if (_count < num) SetCapacity(_count);
        }
        #endregion

        #region [Readonly/Clear] Handling
        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }

        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_items, 0, _count);
                _count = 0;
            }

            _version++;
        }
        #endregion

        #region Enumerator
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private IndexedList<T> list;
            private int index;
            private int version;
            private T current;

            public T Current { get { return current; } }

            object IEnumerator.Current
            {
                get
                {
                    if ((index == 0) || (index == (list._count + 1))) throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
                    return this.Current;
                }
            }

            internal Enumerator(IndexedList<T> list)
            {
                this.list = list;
                this.index = 0;
                this.version = list._version;
                this.current = default(T);
            }

            public bool MoveNext()
            {
                if ((version == list._version) && (index < list._count))
                {
                    current = list._items[index];
                    index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (version != list._version) throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
                index = list._count + 1;
                current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (version != list._version) throw new InvalidOperationException("Collection has changed since enumerator instantiation.");
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }
        }
        #endregion
    }
}
