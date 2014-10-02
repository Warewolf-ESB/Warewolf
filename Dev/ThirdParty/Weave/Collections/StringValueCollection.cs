
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StringValueCollectionDebugView<>))]
    internal class StringValueCollection<T> : StringValueCollectionBase, IDictionary<string, T>
    {
        #region Instance Fields
        protected bool _raiseEvents;
        private StringValueCollectionImplementation _impl;
        private KeyCollection _keys;
        private ValueCollection _values;
        private bool _hasNullValue;
        private T _nullValue;
        private ExposedList<Entry> _items;
        #endregion

        #region Public Properties
        public override int Count { get { return _items.Count; } }
        public int Capacity { get { return _items.Capacity; } set { _items.Capacity = value; } }
        public T this[TPKey lookup, string key] { get { return LocateHard(lookup, key).Value; } }
        public T this[string key] { get { return LocateHard(key).Value; } set { AddInternal(key, value, true, false); } }
        public T this[int index] { get { return _items[index].Value; } }
        public KeyCollection Keys { get { return (_keys ?? (_keys = new KeyCollection(this))); } }
        public ValueCollection Values { get { return (_values ?? (_values = new ValueCollection(this))); } }

        bool ICollection<KeyValuePair<string, T>>.IsReadOnly { get { return false; } }
        ICollection<string> IDictionary<string, T>.Keys { get { return Keys; } }
        ICollection<T> IDictionary<string, T>.Values { get { return Values; } }
        #endregion

        #region Constructors
        public StringValueCollection()
        {
            _impl = new StringValueInt16CollectionImplementation();
            _items = new ExposedList<Entry>();
            _raiseEvents = false;
        }

        public StringValueCollection(int capacity)
        {
            if (capacity >= Int16.MaxValue - 8) _impl = new StringValueInt32CollectionImplementation(capacity);
            else _impl = new StringValueInt16CollectionImplementation(capacity);
            _items = new ExposedList<Entry>();
            _raiseEvents = false;
        }

        public StringValueCollection(T nullValue)
        {
            _impl = new StringValueInt16CollectionImplementation();
            _items = new ExposedList<Entry>();
            SetNullValue(nullValue);
            _raiseEvents = false;
        }
        #endregion

        #region [Get/Set] Handling
        public bool TryGetValue(string key, out T value)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key))
            {
                value = default(T);
                return false;
            }

            Entry entry = Locate(key);
            value = entry.Value;
            if (entry.IsInvalid) return false;
            return true;
        }

        public bool TryGetLookupKey(string key, out TPKey value)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key))
            {
                value = TPKey.Invalid;
                return false;
            }

            int index = _impl.IndexOfEntryTier(key, 0, key.Length);

            if (index == -1)
            {
                value = TPKey.Invalid;
                return false;
            }

            value = new TPKey(index + 1);
            return true;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override string GetEntryKey(int index)
        {
            Entry entry = _items.UnderlyingArray[index];
            return _impl.GetEntryKey(index, entry.Tier, entry.Index);
        }

        public void SetNullValue(T value)
        {
            _nullValue = value;
            _hasNullValue = true;
        }

        public void UnsetNullValue()
        {
            _nullValue = default(T);
            _hasNullValue = false;
        }
        #endregion

        #region Addition Handling
        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
        {
            AddInternal(item.Key, item.Value, false, false);
        }

        public bool TryAdd(string key, T value)
        {
            return AddInternal(key, value, false, true);
        }

        public void Add(string key, T value)
        {
            AddInternal(key, value, false, false);
        }

        private unsafe bool AddInternal(string key, T value, bool overwrite, bool soft)
        {
            if (String.IsNullOrEmpty(key))
            {
                if (soft) return false;
                else throw new ArgumentNullException("key");
            }

            if (_raiseEvents && !CanAddValue(key, value)) return false;

            if (_impl is StringValueInt16CollectionImplementation && _impl.Size + key.Length >= _impl.MaxCapacity)
                _impl = new StringValueInt32CollectionImplementation(_impl as StringValueInt16CollectionImplementation);

            int resultIndex;
            int resultTier;

            if (!_impl.Add(key, overwrite, _items.Count, out resultTier, out resultIndex))
            {
                if (soft) return false;
                else throw new ArgumentException("An item with the same key has already been added.");
            }

            if (resultTier == -1)
            {
                if (_raiseEvents)
                {
                    if (!CanSetValue(key, _items.UnderlyingArray[resultIndex].Value, value)) return false;
                    _items.UnderlyingArray[resultIndex].Value = value = AcquireValue(key, value);
                    OnValueAdded(key, value);
                }
                else _items.UnderlyingArray[resultIndex].Value = value;
            }
            else
            {
                if (_raiseEvents)
                {
                    value = AcquireValue(key, value);
                    _items.Add(new Entry(value, resultTier, resultIndex));
                    OnValueAdded(key, value);
                }
                else _items.Add(new Entry(value, resultTier, resultIndex));
            }

            _version++;

            return true;
        }

        protected virtual bool CanAddValue(string key, T value) { return true; }
        protected virtual bool CanSetValue(string key, T currentValue, T newValue) { return true; }
        protected virtual T AcquireValue(string key, T input) { return input; }
        protected virtual void OnValueAdded(string key, T input) { }
        #endregion

        #region Subtraction Handling
        bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(item.Key)) return false;
            Entry entry = Locate(item.Key);
            if (entry.IsInvalid || !EqualityComparer<T>.Default.Equals(entry.Value, item.Value)) return false;
            return Remove(item.Key, entry);
        }

        public bool Remove(string key)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key)) return false;
            return Remove(key, Locate(key));
        }

        private bool Remove(string key, Entry entry)
        {
            if (entry.IsInvalid) return false;
            if (_raiseEvents && !CanRemoveValue(key, entry.Value)) return false;
            Entry removed = entry;
            int itemIndex = _impl.BeginRemove(key, entry.Tier, entry.Index);
            Entry[] rawItems = _items.UnderlyingArray;
            int lastItemIndex = _items.Count - 1;

            if (lastItemIndex != itemIndex)
            {
                entry = rawItems[lastItemIndex];
                _impl.EndRemove(entry.Tier, entry.Index, itemIndex, lastItemIndex);
                rawItems[itemIndex] = entry;
                rawItems[lastItemIndex] = new Entry();
                --_items.Count;
            }
            else
            {
                rawItems[itemIndex] = new Entry();
                --_items.Count;

                _impl.EndRemove(-1, 0, 0, lastItemIndex);
            }

            if (_raiseEvents) OnValueRemoved(key, removed.Value);
            _version++;
            return true;
        }

        public void Clear()
        {
            if (_raiseEvents)
            {
                if (!CanClear()) return;
                OnBeforeCleared();
            }

            _impl.Clear();

            _items.Clear();
            _version++;
        }

        protected virtual bool CanRemoveValue(string key, T value) { return true; }
        protected virtual void OnValueRemoved(string key, T value) { }
        protected virtual bool CanClear() { return true; }
        protected virtual void OnBeforeCleared() { }
        #endregion

        #region Search Handling
        bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(item.Key)) return false;
            Entry entry = Locate(item.Key);
            if (entry.IsInvalid || !EqualityComparer<T>.Default.Equals(entry.Value, item.Value)) return false;
            return true;
        }

        public override bool ContainsKey(string key)
        {
            return IndexOf(key) != -1;
        }

        public bool ContainsValue(T value)
        {
            return IndexOf(value) != -1;
        }

        public int IndexOf(T value)
        {
            if (_items.Count == 0) return -1;
            Entry[] rawItems = _items.UnderlyingArray;
            int count = _items.Count;

            if (value == null)
            {
                for (int i = 0; i < count; i++)
                    if (rawItems[i].Value == null)
                        return i;
            }
            else
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;

                for (int i = 0; i < count; i++)
                    if (comparer.Equals(rawItems[i].Value, value))
                        return i;
            }

            return -1;
        }

        public int IndexOf(string key)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key)) return -1;
            return _impl.IndexOfEntry(key, 0, key.Length, _items.Count);
        }

        protected internal int IndexOf(string key, int index, int count)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key)) return -1;
            return _impl.IndexOfEntry(key, index, count, _items.Count);
        }

        protected internal int IndexOf(string key, int index, char seperator, out int consumed)
        {
            if (_items.Count == 0 || String.IsNullOrEmpty(key)) return consumed = -1;
            return _impl.IndexOfEntry(key, index, seperator, _items.Count, out consumed);
        }

        private Entry LocateHard(TPKey lookup, string key)
        {
            if (!lookup.Valid) throw new KeyNotFoundException();
            Entry result = _items.Count == 0 || String.IsNullOrEmpty(key) ? Entry.Invalid : Locate(lookup, key);

            if (result.IsInvalid)
            {
                if (_hasNullValue) result.Value = _nullValue;
                else throw new KeyNotFoundException();
            }

            return result;
        }

        private Entry LocateHard(string key)
        {
            Entry result = _items.Count == 0 || String.IsNullOrEmpty(key) ? Entry.Invalid : Locate(key);

            if (result.IsInvalid)
            {
                if (_hasNullValue) result.Value = _nullValue;
                else throw new KeyNotFoundException();
            }

            return result;
        }

        private Entry Locate(TPKey lookup, string key)
        {
            int index = _impl.IndexOfEntry(lookup.Value - 1, key, 0, key.Length, _items.Count);
            return index == -1 ? Entry.Invalid : _items[index];
        }

        private Entry Locate(string key)
        {
            int index = _impl.IndexOfEntry(key, 0, key.Length, _items.Count);
            return index == -1 ? Entry.Invalid : _items[index];
        }
        #endregion

        #region Array Handling
        void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Length - arrayIndex < _items.Count) throw new ArgumentException("Array too small.");
            Entry[] entries = _items.UnderlyingArray;
            int count = _items.Count;

            for (int i = 0; i < count; i++) array[arrayIndex + i] = new KeyValuePair<string, T>(GetEntryKey(i), entries[i].Value);
        }
        #endregion

        #region Entry
        private struct Entry
        {
            public static readonly Entry Invalid = new Entry(default(T), -1, -1);

            public T Value;
            public int Tier;
            public short Index;

            public bool IsInvalid { get { return Tier == -1 && Index == -1; } }

            public Entry(T value, int tier, int index)
            {
                Value = value;
                Tier = tier;
                Index = (short)index;
            }
        }
        #endregion

        public struct Enumerator : IEnumerator<KeyValuePair<string, T>>
        {
            #region Instance Fields
            private int _index;
            private int _version;
            private StringValueCollection<T> _owner;
            private KeyValuePair<string, T> _current;
            #endregion

            #region Public Properties
            public KeyValuePair<string, T> Current { get { return _current; } }
            object IEnumerator.Current { get { return _current; } }
            #endregion

            #region Constructor
            internal Enumerator(StringValueCollection<T> owner)
            {
                _owner = owner;
                _version = _owner._version;
                _current = new KeyValuePair<string, T>();
                _index = 0;
            }
            #endregion

            #region [MoveNext/Reset] Handling
            public bool MoveNext()
            {
                if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                if (_index < _owner._items.Count)
                {
                    _current = new KeyValuePair<string, T>(_owner.GetEntryKey(_index), _owner._items.UnderlyingArray[_index].Value);
                    _index++;
                    return true;
                }

                _current = new KeyValuePair<string, T>();
                return false;
            }

            public void Reset()
            {
                if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                _index = 0;
                _current = new KeyValuePair<string, T>();
            }
            #endregion

            #region Disposal Handling
            public void Dispose()
            {
                _owner = null;
                _version = -1;
                _current = new KeyValuePair<string, T>();
            }
            #endregion
        }

        [Serializable]
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(KeyedValueValueCollectionDebugView<,>))]
        public sealed class ValueCollection : ICollection<T>
        {
            #region Instance Fields
            private StringValueCollection<T> _owner;
            #endregion

            #region Public Properties
            public int Count { get { return _owner.Count; } }
            bool ICollection<T>.IsReadOnly { get { return true; } }
            #endregion

            #region Constructor
            internal ValueCollection(StringValueCollection<T> owner)
            {
                _owner = owner;
            }
            #endregion

            #region [Get/Set] Handling
            public Enumerator GetEnumerator()
            {
                return new Enumerator(_owner);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new Enumerator(_owner);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(_owner);
            }
            #endregion

            #region Readonly Enforcement
            void ICollection<T>.Add(T item)
            {
                throw new ReadOnlyException("Collection is read-only.");
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new ReadOnlyException("Collection is read-only.");
            }

            void ICollection<T>.Clear()
            {
                throw new ReadOnlyException("Collection is read-only.");
            }
            #endregion

            #region Search Handling
            bool ICollection<T>.Contains(T item)
            {
                return _owner.ContainsValue(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException("array");
                if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");
                if (array.Length - arrayIndex < _owner.Count) throw new ArgumentException("Array too small.");
                Entry[] rawItems = _owner._items.UnderlyingArray;
                int count = _owner.Count;
                for (int i = 0; i < count; i++) array[i + arrayIndex] = rawItems[i].Value;
            }
            #endregion

            public struct Enumerator : IEnumerator<T>
            {
                #region Instance Fields
                private int _index;
                private int _version;
                private StringValueCollection<T> _owner;
                private T _current;
                #endregion

                #region Public Properties
                public T Current { get { return _current; } }
                object IEnumerator.Current { get { return _current; } }
                #endregion

                #region Constructor
                internal Enumerator(StringValueCollection<T> owner)
                {
                    _owner = owner;
                    _version = _owner._version;
                    _current = default(T);
                    _index = 0;
                }
                #endregion

                #region [MoveNext/Reset] Handling
                public bool MoveNext()
                {
                    if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                    if (_index < _owner.Count)
                    {
                        _current = _owner._items[_index].Value;
                        _index++;
                        return true;
                    }

                    _current = default(T);
                    return false;
                }

                public void Reset()
                {
                    if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    _index = 0;
                    _current = default(T);
                }
                #endregion

                #region Disposal Handling
                public void Dispose()
                {
                    _owner = null;
                    _version = -1;
                    _current = default(T);
                }
                #endregion
            }
        }
    }

    internal sealed class StringValueInt16CollectionImplementation : StringValueCollectionImplementation
    {
        #region Static Members
        private static StringTier[] _emptyArray = new StringTier[0];
        #endregion

        #region Instance Fields
        internal StringTier _firstTier;
        internal StringTier[] _tiers;
        internal int _released;
        internal int _capacity;
        internal int _size;
        #endregion

        #region Internal Properties
        internal override int Size { get { return _tiers.Length; } }
        internal override int MaxCapacity { get { return Int16.MaxValue - 8; } }
        #endregion

        #region Constructors
        public StringValueInt16CollectionImplementation()
        {
            _tiers = _emptyArray;
        }

        public StringValueInt16CollectionImplementation(int capacity)
        {
            _tiers = new StringTier[_capacity = capacity];
        }
        #endregion

        #region [Get/Set] Handling
        internal override string GetEntryKey(int index, int tier, short entryIndex)
        {
            int capacity = CharStorageCapacity;
            char[] storage = CharStorage;
            string key = String.Empty;

            StringTier current = _tiers[tier];
            index = capacity;
            storage[--index] = (char)entryIndex;

            int parentIndex = current.ParentIndex;

            while (current.Parent != NegativeOne)
            {
                storage[--index] = (char)parentIndex;

                if (index == 0)
                {
                    key += new string(storage);
                    index = capacity;
                }

                current = _tiers[current.Parent];
                parentIndex = current.ParentIndex;
            }

            if (index != capacity) key += new string(storage, index, capacity - index);
            return key;
        }

        private void SetCapacity(int value)
        {
            if (value != _capacity)
            {
                if (value < _size) throw new ArgumentOutOfRangeException("value");

                if (value > 0)
                {
                    _capacity = value;
                    StringTier[] destinationArray = new StringTier[value];
                    if (_size > 0) Array.Copy(_tiers, 0, destinationArray, 0, _size);
                    _tiers = destinationArray;
                }
                else
                {
                    _tiers = _emptyArray;
                    _capacity = 0;
                }
            }
        }
        #endregion

        #region Addition Handling
        internal unsafe override bool Add(string key, bool overwrite, int itemCount, out int resultTier, out int resultIndex)
        {
            if (_firstTier == null)
            {
                if (_size == _capacity) EnsureCapacity(_size + 1);
                _tiers[_size] = _firstTier = new StringTier(NegativeOne, NegativeOne, _size);
                ++_size;
            }

            StringTier currentTier = _firstTier, maskTier = null;
            int index = 0, length = key.Length - 1;
            int slot = 0;
            short route = Unused;

            fixed (char* source = key)
            {
                for (int t = 0; t < length; t++)
                {
                    index = source[t];

                    if ((route = currentTier[index]) == Unused)
                    {
                        slot = EnsureSlot(slot);
                        currentTier[index] = route = (short)slot;
                        _tiers[slot] = new StringTier(currentTier.Index, (short)index, slot);
                    }
                    else if (route < Unused)
                    {
                        short occupant = route;
                        slot = EnsureSlot(slot);
                        currentTier[index] = route = (short)slot;
                        (_tiers[slot] = new StringTier(currentTier.Index, (short)index, slot)).Item = occupant;
                    }

                    currentTier = _tiers[route];
                }

                if ((route = currentTier[index = source[length]]) != Unused)
                {
                    if (route < Unused || (maskTier = _tiers[route]).Item != Unused)
                    {
                        if (overwrite)
                        {
                            resultIndex = ((route < Unused ? route : maskTier.Item) * -1) + -1;
                            resultTier = -1;
                            return true;
                        }
                        else
                        {
                            resultIndex = Unused;
                            resultTier = -1;
                            return false;
                        }
                    }

                    currentTier[index] = route;
                    maskTier.Item = (short)(-(itemCount + 1));
                }
                else currentTier[index] = (short)(-(itemCount + 1));

                resultIndex = (short)index;
                resultTier = currentTier.Index;
            }

            return true;
        }
        #endregion

        #region Subtraction Handling
        internal override int BeginRemove(string key, int entryTier, short entryIndex)
        {
            StringTier origin = _tiers[entryTier];
            StringTier current = null;
            short route = origin[entryIndex];
            short itemIndex = Unused;

            if (route < Unused)
            {
                itemIndex = (short)((route * NegativeOne) + NegativeOne);
                origin[entryIndex] = Unused;
                current = origin;

                while (current.Parent != NegativeOne && current.Routes == 0)
                {
                    origin = _tiers[current.Parent];

                    if (current.Item != Unused)
                    {
                        origin[current.ParentIndex] = current.Item;
                        current.Item = Unused;
                    }
                    else origin[current.ParentIndex] = Unused;

                    _tiers[current.Index] = null;
                    ++_released;
                    current = origin;
                }
            }
            else
            {
                current = _tiers[route];
                itemIndex = (short)((current.Item * NegativeOne) + NegativeOne);
                current.Item = Unused;
            }

            return itemIndex;
        }

        internal override void EndRemove(int entryTier, short entryIndex, int itemIndex, int lastItemIndex)
        {
            if (entryTier != -1)
            {
                StringTier origin = _tiers[entryTier];
                short route = origin[entryIndex];

                if (route < Unused) origin[entryIndex] = (short)(-(itemIndex + 1));
                else
                {
                    StringTier current = _tiers[route];
                    current.Item = (short)(-(itemIndex + 1));
                }
            }

            if (lastItemIndex == 0)
            {
                _size = _capacity = _released = 0;
                _tiers = _emptyArray;
                _firstTier = null;
            }
        }

        internal override void Clear()
        {
            _size = _capacity = _released = 0;
            _tiers = _emptyArray;
            _firstTier = null;
        }
        #endregion

        #region Search Handling
        internal unsafe override int IndexOfEntryTier(string key, int index, int count)
        {
            short tierIndex = Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                short previous = 0;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Unused) return -1;
                    if (i + 1 == count) return tierIndex;
                    if (tierIndex < Unused) return -1;
                    else tier = _tiers[tierIndex];
                }
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(int rawTierIndex, string key, int index, int count, int itemCount)
        {
            short tierIndex = (short)rawTierIndex;
            if (tierIndex == Unused) return IndexOfEntry(key, index, count, itemCount);

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                short previous = 0;
                StringTier tier = _tiers[tierIndex];

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Unused) return -1;

                    if (tierIndex < Unused)
                    {
                        if (i + 1 != count) return -1;

                        tierIndex *= NegativeOne;
                        tierIndex += NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * NegativeOne) + NegativeOne;
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(string key, int index, int count, int itemCount)
        {
            short tierIndex = Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                short previous = 0;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Unused) return -1;

                    if (tierIndex < Unused)
                    {
                        if (i + 1 != count) return -1;

                        tierIndex *= NegativeOne;
                        tierIndex += NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * NegativeOne) + NegativeOne;
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(string key, int index, char seperator, int itemCount, out int consumed)
        {
            consumed = 0;
            int count = key.Length - index;
            short tierIndex = Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                short previous = 0;
                int breakout = seperator;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    consumed++;

                    if (current == breakout) break;
                    if ((tierIndex = tier[current]) == Unused) return -1;

                    if (tierIndex < Unused)
                    {
                        if (i + 1 != count)
                            if (source[i + 1] != breakout) return -1;
                            else consumed++;

                        tierIndex *= NegativeOne;
                        tierIndex += NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * NegativeOne) + NegativeOne;
            }

            return -1;
        }
        #endregion

        #region Array Handling
        internal override void EnsureCapacity(int min)
        {
            if (_capacity < min)
            {
                int num = _capacity == 0 ? 4 : _capacity * 2;
                if (num < min) num = min;

                if (num > MaxCapacity)
                {
                    num = MaxCapacity;
                }


                SetCapacity(num);
            }
        }

        internal override int EnsureSlot(int start)
        {
            if (_released == 0)
            {
                if (_size == _capacity) EnsureCapacity(_size + 1);
                return _size++;
            }
            else
            {
                for (int i = start; i < _size; i++)
                    if (_tiers[i] == null)
                    {
                        --_released;
                        return i;
                    }

                throw new InvalidOperationException();
            }
        }
        #endregion

        internal sealed class StringTier
        {
            #region Static Members
            internal static short[] EmptyPartition = new short[PartitionCapacity];
            #endregion

            #region Instance Fields
            private short _item;
            private short _parent;
            private short _parentIndex;
            private short _index;
            private short _terminators;
            private short _routes;
            private short[][] _table;
            private int _assigned;
            #endregion

            #region Public Properties
            public short Item { get { return _item; } set { _item = value; } }
            public short Parent { get { return _parent; } set { _parent = value; } }
            public short ParentIndex { get { return _parentIndex; } set { _parentIndex = value; } }
            public short Index { get { return _index; } set { _index = value; } }
            public short Terminators { get { return _terminators; } }
            public short Routes { get { return _routes; } }
            public int Assigned { get { return _assigned; } }
            public short[][] Table { get { return _table; } }
            [IndexerName("TableRoutes")]
            public short this[int index] { get { int partition = index >> PartitionShift; return _table[partition][index & PartitionModulo]; } set { SetValue(index, value); } }
            #endregion

            #region Constructor
            public StringTier(short parent, short parentIndex, int index)
            {
                _parent = parent;
                _parentIndex = parentIndex;
                _index = (short)index;
                _table = new short[TotalPartitions][];
                for (int i = 0; i < _table.Length; i++) _table[i] = EmptyPartition;
            }
            #endregion

            #region [Get/Set] Handling
            public short[] GetPartition(int partition)
            {
                if ((_assigned & (1 << partition)) == 0) return null;
                return _table[partition];
            }

            private void SetValue(int index, short value)
            {
                int partition = index >> PartitionShift;
                index = index & PartitionModulo;
                short previous = Unused;
                short[] dest = null;

                if ((_assigned & (1 << partition)) == 0)
                {
                    if (value == Unused) return;
                    _assigned |= (1 << partition);
                    _table[partition] = dest = new short[PartitionCapacity];
                }
                else
                {
                    previous = (dest = _table[partition])[index];
                    if (previous == value) return;
                }

                if (value == Unused)
                {
                    if (previous < Unused) --_terminators;
                    --_routes;

                    if (--dest[PartitionLength] == Unused)
                    {
                        _table[partition] = EmptyPartition;
                        _assigned &= ~(1 << partition);
                    }
                    else dest[index] = value;
                }
                else
                {
                    if (previous != Unused)
                    {
                        if (previous < Unused)
                        {
                            if (value > Unused)
                                --_terminators;
                        }
                        else if (value < Unused) ++_terminators;
                    }
                    else
                    {
                        if (value < Unused) ++_terminators;
                        ++dest[PartitionLength];
                        ++_routes;
                    }

                    dest[index] = value;
                }
            }
            #endregion
        }
    }

    internal sealed class StringValueInt32CollectionImplementation : StringValueCollectionImplementation
    {
        #region Static Members
        private static StringTier[] _emptyArray = new StringTier[0];
        #endregion

        #region Instance Fields
        private StringTier _firstTier;
        private StringTier[] _tiers;
        private int _released;
        private int _capacity;
        private int _size;
        #endregion

        #region Internal Properties
        internal override int Size { get { return _tiers.Length; } }
        internal override int MaxCapacity { get { return Int32.MaxValue - 8; } }
        #endregion

        #region Constructors
        public StringValueInt32CollectionImplementation()
        {
            _tiers = _emptyArray;
        }

        public StringValueInt32CollectionImplementation(int capacity)
        {
            _tiers = new StringTier[_capacity = capacity];
        }

        public StringValueInt32CollectionImplementation(StringValueInt16CollectionImplementation from)
        {
            _released = from._released;
            _capacity = from._capacity;
            _size = from._size;

            if (from._firstTier != null) _firstTier = new StringTier(from._firstTier);

            if (from._tiers != null && from._tiers.Length != 0)
            {
                _tiers = new StringTier[from._tiers.Length];

                for (int i = 0; i < _tiers.Length; i++)
                {
                    if (from._tiers[i] != null)
                    {
                        if (from._tiers[i] == from._firstTier) _tiers[i] = _firstTier;
                        else _tiers[i] = new StringTier(from._tiers[i]);
                    }
                }
            }
            else _tiers = _emptyArray;
        }
        #endregion

        #region [Get/Set] Handling
        internal override string GetEntryKey(int index, int tier, short entryIndex)
        {
            int capacity = CharStorageCapacity;
            char[] storage = CharStorage;
            string key = String.Empty;

            StringTier current = _tiers[tier];
            index = capacity;
            storage[--index] = (char)entryIndex;

            int parentIndex = current.ParentIndex;

            while (current.Parent != Int32NegativeOne)
            {
                storage[--index] = (char)parentIndex;

                if (index == 0)
                {
                    key += new string(storage);
                    index = capacity;
                }

                current = _tiers[current.Parent];
                parentIndex = current.ParentIndex;
            }

            if (index != capacity) key += new string(storage, index, capacity - index);
            return key;
        }

        private void SetCapacity(int value)
        {
            if (value != _capacity)
            {
                if (value < _size) throw new ArgumentOutOfRangeException("value");

                if (value > 0)
                {
                    _capacity = value;
                    StringTier[] destinationArray = new StringTier[value];
                    if (_size > 0) Array.Copy(_tiers, 0, destinationArray, 0, _size);
                    _tiers = destinationArray;
                }
                else
                {
                    _tiers = _emptyArray;
                    _capacity = 0;
                }
            }
        }
        #endregion

        #region Addition Handling
        internal unsafe override bool Add(string key, bool overwrite, int itemCount, out int resultTier, out int resultIndex)
        {
            if (_firstTier == null)
            {
                if (_size == _capacity) EnsureCapacity(_size + 1);
                _tiers[_size] = _firstTier = new StringTier(Int32NegativeOne, Int32NegativeOne, _size);
                ++_size;
            }

            StringTier currentTier = _firstTier, maskTier = null;
            int index = 0, length = key.Length - 1;
            int slot = 0;
            int route = Int32Unused;

            fixed (char* source = key)
            {
                for (int t = 0; t < length; t++)
                {
                    index = source[t];

                    if ((route = currentTier[index]) == Int32Unused)
                    {
                        slot = EnsureSlot(slot);
                        currentTier[index] = route = slot;
                        _tiers[slot] = new StringTier(currentTier.Index, index, slot);
                    }
                    else if (route < Int32Unused)
                    {
                        int occupant = route;
                        slot = EnsureSlot(slot);
                        currentTier[index] = route = slot;
                        (_tiers[slot] = new StringTier(currentTier.Index, index, slot)).Item = occupant;
                    }

                    currentTier = _tiers[route];
                }

                if ((route = currentTier[index = source[length]]) != Int32Unused)
                {
                    if (route < Int32Unused || (maskTier = _tiers[route]).Item != Int32Unused)
                    {
                        if (overwrite)
                        {
                            resultIndex = ((route < Int32Unused ? route : maskTier.Item) * -1) + -1;
                            resultTier = -1;
                            return true;
                        }
                        else
                        {
                            resultIndex = Int32Unused;
                            resultTier = -1;
                            return false;
                        }
                    }

                    currentTier[index] = route;
                    maskTier.Item = -(itemCount + 1);
                }
                else currentTier[index] = -(itemCount + 1);

                resultIndex = index;
                resultTier = currentTier.Index;
            }

            return true;
        }
        #endregion

        #region Subtraction Handling
        internal override int BeginRemove(string key, int entryTier, short entryIndex)
        {
            StringTier origin = _tiers[entryTier];
            StringTier current = null;
            int route = origin[entryIndex];
            int itemIndex = Int32Unused;

            if (route < Int32Unused)
            {
                itemIndex = (route * Int32NegativeOne) + Int32NegativeOne;
                origin[entryIndex] = Int32Unused;
                current = origin;

                while (current.Parent != Int32NegativeOne && current.Routes == 0)
                {
                    origin = _tiers[current.Parent];

                    if (current.Item != Unused)
                    {
                        origin[current.ParentIndex] = current.Item;
                        current.Item = Int32Unused;
                    }
                    else origin[current.ParentIndex] = Int32Unused;

                    _tiers[current.Index] = null;
                    ++_released;
                    current = origin;
                }
            }
            else
            {
                current = _tiers[route];
                itemIndex = (current.Item * Int32NegativeOne) + Int32NegativeOne;
                current.Item = Int32Unused;
            }

            return itemIndex;
        }

        internal override void EndRemove(int entryTier, short entryIndex, int itemIndex, int lastItemIndex)
        {
            if (entryTier != -1)
            {
                StringTier origin = _tiers[entryTier];
                int route = origin[entryIndex];

                if (route < Int32Unused) origin[entryIndex] = -(itemIndex + 1);
                else
                {
                    StringTier current = _tiers[route];
                    current.Item = -(itemIndex + 1);
                }
            }

            if (lastItemIndex == 0)
            {
                _size = _capacity = _released = 0;
                _tiers = _emptyArray;
                _firstTier = null;
            }
        }

        internal override void Clear()
        {
            _size = _capacity = _released = 0;
            _tiers = _emptyArray;
            _firstTier = null;
        }
        #endregion

        #region Search Handling
        internal unsafe override int IndexOfEntryTier(string key, int index, int count)
        {
            int tierIndex = Int32Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                int previous = 0;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Int32Unused) return -1;
                    if (i + 1 == count) return tierIndex;
                    if (tierIndex < Int32Unused) return -1;
                    else tier = _tiers[tierIndex];
                }
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(int tierIndex, string key, int index, int count, int itemCount)
        {
            if (tierIndex == Int32Unused) return IndexOfEntry(key, index, count, itemCount);

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                int previous = 0;
                StringTier tier = _tiers[tierIndex];

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Int32Unused) return -1;

                    if (tierIndex < Int32Unused)
                    {
                        if (i + 1 != count) return -1;

                        tierIndex *= Int32NegativeOne;
                        tierIndex += Int32NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Int32Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * Int32NegativeOne) + Int32NegativeOne;
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(string key, int index, int count, int itemCount)
        {
            int tierIndex = Int32Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                int previous = 0;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    if ((tierIndex = tier[current]) == Int32Unused) return -1;

                    if (tierIndex < Int32Unused)
                    {
                        if (i + 1 != count) return -1;

                        tierIndex *= Int32NegativeOne;
                        tierIndex += Int32NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Int32Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * Int32NegativeOne) + Int32NegativeOne;
            }

            return -1;
        }

        internal unsafe override int IndexOfEntry(string key, int index, char seperator, int itemCount, out int consumed)
        {
            consumed = 0;
            int count = key.Length - index;
            int tierIndex = Int32Unused;

            fixed (char* pointer = key)
            {
                char* source = pointer + index;
                int current = 0;
                int previous = 0;
                int breakout = seperator;
                StringTier tier = _firstTier;

                for (int i = 0; i < count; i++)
                {
                    previous = tierIndex;
                    current = source[i];
                    consumed++;

                    if (current == breakout) break;
                    if ((tierIndex = tier[current]) == Int32Unused) return -1;

                    if (tierIndex < Int32Unused)
                    {
                        if (i + 1 != count)
                            if (source[i + 1] != breakout) return -1;
                            else consumed++;

                        tierIndex *= Int32NegativeOne;
                        tierIndex += Int32NegativeOne;

                        if (tierIndex < itemCount) return tierIndex;
                        return -1;
                    }
                    else tier = _tiers[tierIndex];
                }

                if (tier.Item != Int32Unused && tier != _firstTier && tier.Parent == previous) return (tier.Item * Int32NegativeOne) + Int32NegativeOne;
            }

            return -1;
        }
        #endregion

        #region Array Handling
        internal override void EnsureCapacity(int min)
        {
            if (_capacity < min)
            {
                int num = _capacity == 0 ? 4 : _capacity * 2;
                if (num < min) num = min;
                SetCapacity(num);
            }
        }

        internal override int EnsureSlot(int start)
        {
            if (_released == 0)
            {
                if (_size == _capacity) EnsureCapacity(_size + 1);
                return _size++;
            }
            else
            {
                for (int i = start; i < _size; i++)
                    if (_tiers[i] == null)
                    {
                        --_released;
                        return i;
                    }

                throw new InvalidOperationException();
            }
        }
        #endregion

        private sealed class StringTier
        {
            #region Static Members
            private static int[] EmptyPartition = new int[PartitionCapacity];
            #endregion

            #region Instance Fields
            private int _item;
            private int _parent;
            private int _parentIndex;
            private int _index;
            private int _terminators;
            private int _routes;
            private int[][] _table;
            private int _assigned;
            #endregion

            #region Public Properties
            public int Item { get { return _item; } set { _item = value; } }
            public int Parent { get { return _parent; } set { _parent = value; } }
            public int ParentIndex { get { return _parentIndex; } set { _parentIndex = value; } }
            public int Index { get { return _index; } set { _index = value; } }
            public int Terminators { get { return _terminators; } }
            public int Routes { get { return _routes; } }
            [IndexerName("TableRoutes")]
            public int this[int index] { get { int partition = index >> PartitionShift; return _table[partition][index & PartitionModulo]; } set { SetValue(index, value); } }
            #endregion

            #region Constructor
            public StringTier(int parent, int parentIndex, int index)
            {
                _parent = parent;
                _parentIndex = parentIndex;
                _index = index;
                _table = new int[TotalPartitions][];
                for (int i = 0; i < _table.Length; i++) _table[i] = EmptyPartition;
            }

            public StringTier(StringValueInt16CollectionImplementation.StringTier source)
            {
                _item = source.Item;
                _parent = source.Parent;
                _parentIndex = source.ParentIndex;
                _index = source.Index;
                _terminators = source.Terminators;
                _routes = source.Routes;
                _assigned = source.Assigned;

                _table = new int[TotalPartitions][];

                for (int i = 0; i < _table.Length; i++)
                {
                    if (source.Table[i] == StringValueInt16CollectionImplementation.StringTier.EmptyPartition) _table[i] = EmptyPartition;
                    else
                    {
                        short[] sourceRow = source.Table[i];
                        int[] row = new int[PartitionCapacity];
                        for (int k = 0; k < sourceRow.Length; k++) row[k] = sourceRow[k];
                        _table[i] = row;
                    }
                }
            }
            #endregion

            #region [Get/Set] Handling
            public int[] GetPartition(int partition)
            {
                if ((_assigned & (1 << partition)) == 0) return null;
                return _table[partition];
            }

            private void SetValue(int index, int value)
            {
                int partition = index >> PartitionShift;
                index = index & PartitionModulo;
                int previous = Int32Unused;
                int[] dest = null;

                if ((_assigned & (1 << partition)) == 0)
                {
                    if (value == Int32Unused) return;
                    _assigned |= (1 << partition);
                    _table[partition] = dest = new int[PartitionCapacity];
                }
                else
                {
                    previous = (dest = _table[partition])[index];
                    if (previous == value) return;
                }

                if (value == Int32Unused)
                {
                    if (previous < Int32Unused) --_terminators;
                    --_routes;

                    if (--dest[PartitionLength] == Int32Unused)
                    {
                        _table[partition] = EmptyPartition;
                        _assigned &= ~(1 << partition);
                    }
                    else dest[index] = value;
                }
                else
                {
                    if (previous != Int32Unused)
                    {
                        if (previous < Int32Unused)
                        {
                            if (value > Int32Unused)
                                --_terminators;
                        }
                        else if (value < Int32Unused) ++_terminators;
                    }
                    else
                    {
                        if (value < Int32Unused) ++_terminators;
                        ++dest[PartitionLength];
                        ++_routes;
                    }

                    dest[index] = value;
                }
            }
            #endregion
        }
    }

    internal abstract class StringValueCollectionImplementation
    {
        #region Constants
        protected const int TotalLength = 256;
        protected const int PartitionShift = 5;
        protected const int PartitionLength = 1 << PartitionShift;
        protected const int PartitionModulo = PartitionLength - 1;
        protected const int TotalPartitions = TotalLength / PartitionLength;
        protected const int PartitionCapacity = PartitionLength + 1;

        protected const short Unused = 0;
        protected const short NegativeOne = -1;

        protected const int Int32Unused = 0;
        protected const int Int32NegativeOne = -1;

        protected const int CharStorageCapacity = 1024;
        #endregion

        #region Static Members
        protected static char[] CharStorage = new char[CharStorageCapacity];
        #endregion

        #region Internal Properties
        internal abstract int Size { get; }
        internal abstract int MaxCapacity { get; }
        #endregion

        #region [Get/Set] Handling
        internal abstract string GetEntryKey(int index, int tier, short entryIndex);
        #endregion

        #region Addition Handling
        internal abstract bool Add(string key, bool overwrite, int itemCount, out int resultTier, out int resultIndex);
        #endregion

        #region Subtraction Handling
        internal abstract int BeginRemove(string key, int entryTier, short entryIndex);
        internal abstract void EndRemove(int entryTier, short entryIndex, int itemIndex, int lastItemIndex);
        internal abstract void Clear();
        #endregion

        #region Search Handling
        internal abstract int IndexOfEntry(string key, int index, int count, int itemCount);
        internal abstract int IndexOfEntry(int tierIndex, string key, int index, int count, int itemCount);
        internal abstract int IndexOfEntryTier(string key, int index, int count);
        internal abstract int IndexOfEntry(string key, int index, char seperator, int itemCount, out int consumed);
        #endregion

        #region Array Handling
        internal abstract void EnsureCapacity(int min);
        internal abstract int EnsureSlot(int start);
        #endregion
    }

    internal abstract class StringValueCollectionBase
    {
        #region Instance Fields
        protected int _version;
        #endregion

        #region Public Properties
        public abstract int Count { get; }
        #endregion

        #region Constructor
        internal StringValueCollectionBase()
        {
        }
        #endregion

        #region [Get/Set] Handling
        protected abstract string GetEntryKey(int index);
        #endregion

        #region Search Handling
        public abstract bool ContainsKey(string key);
        #endregion

        [Serializable]
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(KeyedValueKeyCollectionDebugView<,>))]
        public sealed class KeyCollection : ICollection<string>
        {
            #region Instance Fields
            private StringValueCollectionBase _owner;
            #endregion

            #region Public Properties
            public int Count { get { return _owner.Count; } }
            bool ICollection<string>.IsReadOnly { get { return true; } }
            #endregion

            #region Constructor
            internal KeyCollection(StringValueCollectionBase owner)
            {
                _owner = owner;
            }
            #endregion

            #region [Get/Set] Handling
            public Enumerator GetEnumerator()
            {
                return new Enumerator(_owner);
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                return new Enumerator(_owner);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(_owner);
            }
            #endregion

            #region Readonly Enforcement
            void ICollection<string>.Add(string item)
            {
                throw new ReadOnlyException("Collection is read-only.");
            }

            bool ICollection<string>.Remove(string item)
            {
                throw new ReadOnlyException("Collection is read-only.");
            }

            void ICollection<string>.Clear()
            {
                throw new ReadOnlyException("Collection is read-only.");
            }
            #endregion

            #region Search Handling
            bool ICollection<string>.Contains(string item)
            {
                return _owner.ContainsKey(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException("array");
                if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");
                if (array.Length - arrayIndex < _owner.Count) throw new ArgumentException("Array too small.");
                int count = _owner.Count;
                for (int i = 0; i < count; i++) array[i + arrayIndex] = _owner.GetEntryKey(i);
            }
            #endregion

            public struct Enumerator : IEnumerator<string>
            {
                #region Instance Fields
                private int _index;
                private int _version;
                private StringValueCollectionBase _owner;
                private string _current;
                #endregion

                #region Public Properties
                public string Current { get { return _current; } }
                object IEnumerator.Current { get { return _current; } }
                #endregion

                #region Constructor
                internal Enumerator(StringValueCollectionBase owner)
                {
                    _owner = owner;
                    _version = _owner._version;
                    _current = null;
                    _index = 0;
                }
                #endregion

                #region [MoveNext/Reset] Handling
                public bool MoveNext()
                {
                    if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                    if (_index < _owner.Count)
                    {
                        _current = _owner.GetEntryKey(_index);
                        _index++;
                        return true;
                    }

                    _current = null;
                    return false;
                }

                public void Reset()
                {
                    if (_version != _owner._version) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    _index = 0;
                    _current = null;
                }
                #endregion

                #region Disposal Handling
                public void Dispose()
                {
                    _owner = null;
                    _version = -1;
                    _current = null;
                }
                #endregion
            }
        }
    }
}
