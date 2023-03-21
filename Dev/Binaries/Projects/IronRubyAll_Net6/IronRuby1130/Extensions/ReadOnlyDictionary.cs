using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Scripting.Utils.Extensions
{
    // Microsoft.Scripting.Utils.ReadOnlyDictionary<TKey,TValue>
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Scripting.Utils;

    [Serializable]
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private sealed class ReadOnlyWrapper<T> : ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly ICollection<T> _collection;

            public int Count => _collection.Count;

            public bool IsReadOnly => true;

            internal ReadOnlyWrapper(ICollection<T> collection)
            {
                _collection = collection;
            }

            public void Add(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public bool Contains(T item)
            {
                return _collection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _collection.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }

        private readonly IDictionary<TKey, TValue> _dict;

        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> keys = _dict.Keys;
                if (!keys.IsReadOnly)
                {
                    return new ReadOnlyWrapper<TKey>(keys);
                }
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values = _dict.Values;
                if (!values.IsReadOnly)
                {
                    return new ReadOnlyWrapper<TValue>(values);
                }
                return values;
            }
        }

        public TValue this[TKey key] => _dict[key];

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public int Count => _dict.Count;

        public bool IsReadOnly => true;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
        {
            ReadOnlyDictionary<TKey, TValue> readOnlyDictionary = dict as ReadOnlyDictionary<TKey, TValue>;
            _dict = ((readOnlyDictionary != null) ? readOnlyDictionary._dict : dict);
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }

}
