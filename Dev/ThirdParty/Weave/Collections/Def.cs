
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct BitVector
    {
        #region Constants
        public const int SizeInBytes = 4;
        #endregion

        #region Readonly Fields
        internal static readonly int[] IncludeMasks = new int[]
        {
            new BitVector() { O1 = Byte.MaxValue }._value,
            new BitVector() { O1 = Byte.MaxValue, O2 = Byte.MaxValue }._value,
            new BitVector() { O1 = Byte.MaxValue, O2 = Byte.MaxValue, O3 = Byte.MaxValue }._value,
            new BitVector() { O1 = Byte.MaxValue, O2 = Byte.MaxValue, O3 = Byte.MaxValue, O4 = Byte.MaxValue }._value
        };
        #endregion

        #region Static Members
        private static int GetTotalSetBits(int n)
        {
            uint v = (uint)n;
            v = v - ((v >> 1) & 0x55555555);
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
            return (int)(((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24);
        }

        public static BitVector Construct(params bool[] bits)
        {
            BitVector result = new BitVector();
            for (int i = 0; i < bits.Length; i++) result[i] = bits[i];
            return result;
        }
        #endregion

        #region Instance Fields
        [FieldOffset(0)]
        private int _value;

        [FieldOffset(0)]
        public short H1;
        [FieldOffset(2)]
        public short H2;

        [FieldOffset(0)]
        public byte O1;
        [FieldOffset(1)]
        public byte O2;
        [FieldOffset(2)]
        public byte O3;
        [FieldOffset(3)]
        public byte O4;
        #endregion

        #region Public Properties
        public int Value { get { return _value; } }
        public int TotalSetBits { get { return GetTotalSetBits(_value); } }
        public bool AllTrue { get { return _value == -1; } }
        public bool AllFalse { get { return _value == 0; } }
        public bool this[int index] { get { return (_value & (1 << index)) != 0; } set { _value = value ? (_value | (1 << index)) : (_value & ~(1 << index)); } }
        #endregion

        #region Constructor
        public BitVector(BitVector source)
        {
            H1 = H2 = 0;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            _value = source._value;
        }

        public BitVector(int value)
        {
            H1 = H2 = 0;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            _value = value;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            char[] bits = new char[32];
            int value = _value;

            for (int i = 0; i < 32; i++)
            {
                bits[i] = ((value & 0x80000000L) != 0L) ? '1' : '0';
                value <<= 1;
            }
            
            return "BitVector{" + new string(bits) + "}";
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override bool Equals(object other)
        {
            return (other is BitVector) && (_value == ((BitVector)other)._value);
        }
        #endregion

        #region [Get/Set] Handling
        public void SetAll(bool value)
        {
            _value = value ? -1 : 0;
        }

        public void SetMask(int mask, bool value)
        {
            _value = value ? (_value | mask) : (_value & ~mask);
        }
        #endregion

        #region Are(...)
        public bool AreAllTrue(int mask)
        {
            return (_value & mask) == mask;
        }

        public bool AreAllFalse(int mask)
        {
            return (_value & mask) == 0;
        }

        public bool AreAnyTrue(int mask)
        {
            return (_value & mask) != 0;
        }

        public bool AreAnyFalse(int mask)
        {
            return (_value & mask) != mask;
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(BitVector l, BitVector r)
        {
            return l._value == r._value;
        }

        public static bool operator !=(BitVector l, BitVector r)
        {
            return l._value != r._value;
        }
        #endregion
    }

    public interface IIndexed
    {
        int Index { get; set; }
    }

    internal sealed class OneDimensionCollectionDebugView<T>
    {
        #region Instance Fields
        private ICollection<T> _collection;
        #endregion

        #region Public Properties
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[_collection.Count];
                _collection.CopyTo(array, 0);
                return array;
            }
        }
        #endregion

        #region Constructor
        public OneDimensionCollectionDebugView(ICollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            _collection = collection;
        }
        #endregion
    }

    internal sealed class StringValueCollectionDebugView<V>
    {
        #region Instance Fields
        private IDictionary<string, V> _dictionary;
        #endregion

        #region Public Properties
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, V>[] Items
        {
            get
            {
                KeyValuePair<string, V>[] array = new KeyValuePair<string, V>[_dictionary.Count];
                _dictionary.CopyTo(array, 0);
                return array;
            }
        }
        #endregion

        #region Constructor
        public StringValueCollectionDebugView(IDictionary<string, V> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            _dictionary = dictionary;
        }
        #endregion
    }

    internal sealed class KeyedValueKeyCollectionDebugView<K, T>
    {
        #region Instance Fields
        private ICollection<K> _collection;
        #endregion

        #region Public Properties
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public K[] Items
        {
            get
            {
                K[] array = new K[_collection.Count];
                _collection.CopyTo(array, 0);
                return array;
            }
        }
        #endregion

        #region Constructor
        public KeyedValueKeyCollectionDebugView(ICollection<K> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            _collection = collection;
        }
        #endregion
    }

    internal sealed class KeyedValueValueCollectionDebugView<K, T>
    {
        #region Instance Fields
        private ICollection<T> _collection;
        #endregion

        #region Public Properties
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[_collection.Count];
                _collection.CopyTo(array, 0);
                return array;
            }
        }
        #endregion

        #region Constructor
        public KeyedValueValueCollectionDebugView(ICollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            _collection = collection;
        }
        #endregion
    }

    internal static class CollectionUtility
    {
        internal static readonly int[] Primes = new int[] { 
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 
        293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 
        5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 
        108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 
        2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        };

        /// <summary>
        /// Swaps the objects at index <paramref name="a"/> and index <paramref name="b"/>.
        /// </summary>
        /// <typeparam name="TSource">The <see cref="System.Type"/> of the elements of <paramref name="list"/></typeparam>
        /// <param name="list">The source list</param>
        /// <param name="a">The index of the item to be moved to index <paramref name="b"/></param>
        /// <param name="b">The index of the item to be moved to index <paramref name="a"/></param>
        public static void Swap<TSource>(IList<TSource> list, int a, int b)
        {
            if (a == b) return;
            TSource temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        /// <summary>
        /// Gets the value associated with the specified key if the key is found; otherwise,
        /// the <see langword="default"/> value for the type of <typeparamref name="TValue"/> parameter.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value associated with the specified key if the key is found; otherwise, the <see langword="default"/> value for the type of <typeparamref name="TValue"/> parameter.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value)) value = default(TValue);
            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key if the key is found; otherwise,
        /// returns the provided <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="key"/> is not found.</param>
        /// <returns>The value associated with the specified key if the key is found; otherwise, <paramref name="defaultValue"/>.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value)) value = defaultValue;
            return value;
        }

        /// <summary>
        /// Creates a dictionary and populates it using the elements of <paramref name="keys"/> and
        /// <paramref name="values"/> as the key/value pairs.
        /// </summary>
        /// <remarks>
        /// The dictionary will only be populated while there are elements in <paramref name="keys"/>
        /// and in <paramref name="values"/>, the population will stop when either of the two collections
        /// are exhausted.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="keys">The collection whose elements are used as keys in new dictionary.</param>
        /// <param name="values">The collection whose elements are used as values in new dictionary.</param>
        /// <returns>A populated dictionary.</returns>
        public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            using (IEnumerator<TKey> keyIterator = keys.GetEnumerator())
            using (IEnumerator<TValue> valueIterator = values.GetEnumerator())
                while (keyIterator.MoveNext() && valueIterator.MoveNext())
                    result.Add(keyIterator.Current, valueIterator.Current);
           

            return result;
        }

        internal static int GetPrime(int min)
        {
            if (min < 0) throw new ArgumentException();

            for (int i = 0; i < Primes.Length; i++)
            {
                int num2 = Primes[i];
                if (num2 >= min) return num2;
            }

            for (int j = min | 1; j < 2147483647; j += 2)
                if (IsPrime(j))
                    return j;

            return min;
        }

        internal static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0) return candidate == 2;
            int num = (int)Math.Sqrt(candidate);

            for (int i = 3; i <= num; i += 2)
                if ((candidate % i) == 0)
                    return false;

            return true;
        }
    }
}
