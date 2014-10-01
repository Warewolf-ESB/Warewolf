
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Collections
{
    internal sealed class BooleanArray : ICollection, IEnumerable
    {
        #region Constants
        private const int ShrinkThreshold = 0x100;
        #endregion

        #region Static Members
        private static int[] PrepareOperation(ref BooleanArray l, ref BooleanArray r, out int leftLength, out int rightLength)
        {
            if ((leftLength = l.UnderlyingLength) > (rightLength = r.UnderlyingLength))
            {
                int length = leftLength;
                leftLength = rightLength;
                rightLength = length;
                BooleanArray array = l;
                l = r;
                r = array;
            }

            return new int[rightLength];
        }

        public static BooleanArray Or(BooleanArray l, BooleanArray r)
        {
            int leftLength, rightLength;
            int[] result = PrepareOperation(ref l, ref r, out leftLength, out rightLength);

            for (int i = 0; i < leftLength; i++) result[i] = l._array[i] | r._array[i];

            if (leftLength < rightLength) Array.Copy(r._array, leftLength, result, leftLength, rightLength - leftLength);
            BooleanArray toReturn = new BooleanArray();
            toReturn._array = result;
            toReturn._length = r._length;
            return toReturn;
        }

        public static void Or(BooleanArray destination, BooleanArray l, BooleanArray r)
        {
            int leftLength, rightLength;

            if ((leftLength = l.UnderlyingLength) > (rightLength = r.UnderlyingLength))
            {
                int length = leftLength;
                leftLength = rightLength;
                rightLength = length;
                BooleanArray array = l;
                l = r;
                r = array;
            }

            int[] result = destination._array;
            if (result.Length < rightLength) throw new InvalidOperationException("Destination length must be at least as large as l or r.");
            for (int i = 0; i < leftLength; i++) result[i] = l._array[i] | r._array[i];
            if (leftLength < rightLength) Array.Copy(r._array, leftLength, result, leftLength, rightLength - leftLength);
        }
        #endregion

        #region Instance Fields
        private object _syncRoot;
        private int _version;
        private int[] _array;
        private int _length;
        #endregion

        #region Public Properties
        int ICollection.Count { get { return _length; } }
        public bool IsReadOnly { get { return false; } }
        public bool IsSynchronized { get { return false; } }
        public bool this[int index] { get { return Get(index); } set { Set(index, value); } }
        public int Length { get { return _length; } set { SetLength(value); } }
        public int[] UnderlyingArray { get { return _array; } }
        public int UnderlyingLength { get { return (_length + 31) >> 5; } }
        public object SyncRoot { get { if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null); return _syncRoot; } }
        #endregion

        #region Constructors
        private BooleanArray()
        {
        }

        public BooleanArray(char[] letters, int length)
        {
            _array = new int[(length + 31) >> 5];
            _length = length;
            _version = 0;
            for (int i = 0; i < letters.Length; i++) Set(letters[i], true);
        }

        public BooleanArray(char[] letters)
        {
            int length = 1;
            for (int i = 0; i < letters.Length; i++) if (letters[i] >= length) length = letters[i] + 1;
            _array = new int[(length + 31) >> 5];
            _length = length;
            _version = 0;
            for (int i = 0; i < letters.Length; i++) Set(letters[i], true);
        }

        public BooleanArray(int length)
            : this(length, false)
        {
        }

        public BooleanArray(bool[] values)
        {
            if (values == null) throw new ArgumentNullException("values");
            _array = new int[(values.Length + 31) >> 32];
            _length = values.Length;

            if (_length > 0)
            {
                int iterations = (_length - 1) >> 5;
                int index = 0;

                for (int k = 0; k < iterations; k++)
                {
                    int entry = 0;

                    for (int i = 0; i < 32; i++)
                        if (values[index + i])
                            entry |= 1 << i;

                    _array[k] = entry;
                    index += 32;
                }

                {
                    iterations = _length - (iterations << 5);
                    int entry = 0;

                    for (int i = 0; i < iterations; i++)
                        if (values[index + i])
                            entry |= 1 << i;

                    _array[(_length - 1) >> 5] = entry;
                }
            }

            _version = 0;
        }

        public BooleanArray(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            _array = new int[(bytes.Length + 3) >> 2];
            _length = bytes.Length << 3;
            int index = 0;
            int num2 = 0;

            while ((bytes.Length - num2) >= 4)
            {
                _array[index++] = (((bytes[num2] & 0xff) | ((bytes[num2 + 1] & 0xff) << 8)) | ((bytes[num2 + 2] & 0xff) << 0x10)) | ((bytes[num2 + 3] & 0xff) << 0x18);
                num2 += 4;
            }

            switch ((bytes.Length - num2))
            {
                case 1: _array[index] |= bytes[num2] & 0xff; break;
                case 2:
                    {
                        _array[index] |= (bytes[num2 + 1] & 0xff) << 8;
                        _array[index] |= bytes[num2] & 0xff;
                        break;
                    }
                case 3:
                    {
                        _array[index] = (bytes[num2 + 2] & 0xff) << 0x10;
                        _array[index] |= (bytes[num2 + 1] & 0xff) << 8;
                        _array[index] |= bytes[num2] & 0xff;
                        break;
                    }
            }

            _version = 0;
        }

        public BooleanArray(int[] values)
        {
            if (values == null) throw new ArgumentNullException("values");
            _array = new int[values.Length];
            _length = values.Length << 5;
            Array.Copy(values, _array, values.Length);
            _version = 0;
        }

        public BooleanArray(BooleanArray bits)
        {
            if (bits == null) throw new ArgumentNullException("bits");
            _array = new int[(bits._length + 31) >> 5];
            _length = bits._length;
            Array.Copy(bits._array, _array, (int)((bits._length + 31) >> 5));
            _version = bits._version;
        }

        public BooleanArray(int length, bool defaultValue)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length");
            _array = new int[(length + 31) >> 5];
            _length = length;
            int num = defaultValue ? -1 : 0;
            for (int i = 0; i < _array.Length; i++) _array[i] = num;
            _version = 0;
        }
        #endregion

        #region [Get/Set] Handling
        public bool Get(char letter)
        {
            int index = letter;
            return index < _length && Get(index);
        }

        public bool Get(int index)
        {
            if ((index < 0) || (index >= _length)) throw new ArgumentOutOfRangeException("index");
            int bucket = index >> 5;
            return ((_array[bucket] & (1 << (index - (bucket << 5)))) != 0);
        }

        public bool Get(int index, bool outOfRangeValue)
        {
            if ((index < 0) || (index >= _length)) return outOfRangeValue;
            int bucket = index >> 5;
            return ((_array[bucket] & (1 << (index - (bucket << 5)))) != 0);
        }

        public void Set(int index, bool value)
        {
            if ((index < 0) || (index >= _length)) throw new ArgumentOutOfRangeException("index");
            int bucket = index >> 5;
            if (value) _array[bucket] |= 1 << (index - (bucket << 5));
            else _array[bucket] &= ~(1 << (index - (bucket << 5)));
            _version++;
        }

        public void SetAll(bool value)
        {
            int num = value ? -1 : 0;
            int num2 = (_length + 31) >> 5;
            for (int i = 0; i < num2; i++) _array[i] = num;
            _version++;
        }

        private void SetLength(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("value");
            int num = (value + 31) >> 5;

            if ((num > _array.Length) || ((num + ShrinkThreshold) < _array.Length))
            {
                int[] destinationArray = new int[num];
                Array.Copy(_array, destinationArray, (num > _array.Length) ? _array.Length : num);
                _array = destinationArray;
            }

            if (value > _length)
            {
                int index = ((_length + 31) >> 5) - 1;
                int num3 = _length - ((_length >> 5) << 5);
                if (num3 > 0) _array[index] &= (1 << num3) - 1;
                Array.Clear(_array, index + 1, (num - index) - 1);
            }

            _length = value;
            _version++;
        }
        #endregion

        #region Unary Manipulation
        public BooleanArray And(BooleanArray value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (_length != value._length) throw new ArgumentException();
            int num = (_length + 31) >> 5;
            for (int i = 0; i < num; i++) _array[i] &= value._array[i];
            _version++;
            return this;
        }

        public BooleanArray Not()
        {
            int num = (_length + 31) >> 5;
            for (int i = 0; i < num; i++) _array[i] = ~_array[i];
            _version++;
            return this;
        }

        public BooleanArray Or(BooleanArray value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (_length != value._length) throw new ArgumentException();
            int num = (_length + 31) >> 5;
            for (int i = 0; i < num; i++) _array[i] |= value._array[i];
            _version++;
            return this;
        }

        public BooleanArray Xor(BooleanArray value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (_length != value._length) throw new ArgumentException();
            int num = (_length + 31) >> 5;
            for (int i = 0; i < num; i++) _array[i] ^= value._array[i];
            _version++;
            return this;
        }
        #endregion

        #region Array Handling
        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            if (array.Rank != 1) throw new ArgumentException();
            if (_length == 0) return;

            if (array is int[]) Array.Copy(_array, 0, array, index, (_length + 31) >> 5);
            else if (array is byte[])
            {
                int length = (_length + 7) >> 3;
                if ((array.Length - index) < length) throw new ArgumentException();
                byte[] buffer = (byte[])array;
                int bucket = 0;

                for (int i = 0; i < length; i++)
                {
                    bucket = i >> 2;
                    buffer[index + i] = (byte)((_array[bucket] >> ((i - (bucket << 2)) << 3)) & 0xff);
                }
            }
            else
            {
                if (!(array is bool[])) throw new ArgumentException();
                if ((array.Length - index) < _length) throw new ArgumentException();
                bool[] flagArray = (bool[])array;
                int length = (_length - 1) >> 5;

                for (int k = 0; k < length; k++)
                {
                    int entry = _array[k];
                    for (int i = 0; i < 32; i++) flagArray[index + i] = ((entry >> i) & 1) != 0;
                    index += 32;
                }

                {
                    int entry = _array[length];
                    length = _length - (length << 5);
                    for (int i = 0; i < length; i++) flagArray[index + i] = ((entry >> i) & 1) != 0;
                }
            }
        }
        #endregion

        #region Enumeration Handling
        public IEnumerator GetEnumerator()
        {
            return new BooleanArrayEnumeratorSimple(this);
        }
        #endregion

        private sealed class BooleanArrayEnumeratorSimple : IEnumerator
        {
            #region Instance Fields
            private BooleanArray _booleanArray;
            private bool _currentElement;
            private int _index;
            private int _version;
            #endregion

            #region Public Properties
            public object Current { get { if (_index == -1 || _index >= _booleanArray.Length) throw new InvalidOperationException(); return _currentElement; } }
            #endregion

            #region Constructor
            internal BooleanArrayEnumeratorSimple(BooleanArray booleanArray)
            {
                _booleanArray = booleanArray;
                _index = -1;
                _version = booleanArray._version;
            }
            #endregion

            #region [MoveNext/Reset] Handling
            public bool MoveNext()
            {
                if (_version != _booleanArray._version) throw new InvalidOperationException();

                if (_index < (_booleanArray.Length - 1))
                {
                    _index++;
                    _currentElement = _booleanArray.Get(_index);
                    return true;
                }

                _index = _booleanArray.Length;
                return false;
            }

            public void Reset()
            {
                if (_version != _booleanArray._version) throw new InvalidOperationException();
                _index = -1;
            }
            #endregion
        }
    }
}
