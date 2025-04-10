// Decompiled with JetBrains decompiler
// Type: System.Collections.BooleanArray
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Threading;

namespace System.Collections
{
  internal sealed class BooleanArray : ICollection, IEnumerable
  {
    private const int ShrinkThreshold = 256;
    private object _syncRoot;
    private int _version;
    private int[] _array;
    private int _length;

    int ICollection.Count => this._length;

    public bool IsReadOnly => false;

    public bool IsSynchronized => false;

    public bool this[int index]
    {
      get => this.Get(index);
      set => this.Set(index, value);
    }

    public int Length
    {
      get => this._length;
      set => this.SetLength(value);
    }

    public int[] UnderlyingArray => this._array;

    public int UnderlyingLength => this._length + 31 >> 5;

    public object SyncRoot
    {
      get
      {
        if (this._syncRoot == null)
          Interlocked.CompareExchange(ref this._syncRoot, new object(), (object) null);
        return this._syncRoot;
      }
    }

    private static int[] PrepareOperation(
      ref BooleanArray l,
      ref BooleanArray r,
      out int leftLength,
      out int rightLength)
    {
      if ((leftLength = l.UnderlyingLength) > (rightLength = r.UnderlyingLength))
      {
        int num = leftLength;
        leftLength = rightLength;
        rightLength = num;
        BooleanArray booleanArray = l;
        l = r;
        r = booleanArray;
      }
      return new int[rightLength];
    }

    public static BooleanArray Or(BooleanArray l, BooleanArray r)
    {
      int leftLength;
      int rightLength;
      int[] destinationArray = BooleanArray.PrepareOperation(ref l, ref r, out leftLength, out rightLength);
      for (int index = 0; index < leftLength; ++index)
        destinationArray[index] = l._array[index] | r._array[index];
      if (leftLength < rightLength)
        Array.Copy((Array) r._array, leftLength, (Array) destinationArray, leftLength, rightLength - leftLength);
      return new BooleanArray()
      {
        _array = destinationArray,
        _length = r._length
      };
    }

    public static void Or(BooleanArray destination, BooleanArray l, BooleanArray r)
    {
      int num1;
      int num2;
      if ((num1 = l.UnderlyingLength) > (num2 = r.UnderlyingLength))
      {
        int num3 = num1;
        num1 = num2;
        num2 = num3;
        BooleanArray booleanArray = l;
        l = r;
        r = booleanArray;
      }
      int[] array = destination._array;
      if (array.Length < num2)
        throw new InvalidOperationException("Destination length must be at least as large as l or r.");
      for (int index = 0; index < num1; ++index)
        array[index] = l._array[index] | r._array[index];
      if (num1 >= num2)
        return;
      Array.Copy((Array) r._array, num1, (Array) array, num1, num2 - num1);
    }

    private BooleanArray()
    {
    }

    public BooleanArray(char[] letters, int length)
    {
      this._array = new int[length + 31 >> 5];
      this._length = length;
      this._version = 0;
      for (int index = 0; index < letters.Length; ++index)
        this.Set((int) letters[index], true);
    }

    public BooleanArray(char[] letters)
    {
      int num = 1;
      for (int index = 0; index < letters.Length; ++index)
      {
        if ((int) letters[index] >= num)
          num = (int) letters[index] + 1;
      }
      this._array = new int[num + 31 >> 5];
      this._length = num;
      this._version = 0;
      for (int index = 0; index < letters.Length; ++index)
        this.Set((int) letters[index], true);
    }

    public BooleanArray(int length)
      : this(length, false)
    {
    }

    public BooleanArray(bool[] values)
    {
      this._array = values != null ? new int[values.Length + 31] : throw new ArgumentNullException(nameof (values));
      this._length = values.Length;
      if (this._length > 0)
      {
        int num1 = this._length - 1 >> 5;
        int num2 = 0;
        for (int index1 = 0; index1 < num1; ++index1)
        {
          int num3 = 0;
          for (int index2 = 0; index2 < 32; ++index2)
          {
            if (values[num2 + index2])
              num3 |= 1 << index2;
          }
          this._array[index1] = num3;
          num2 += 32;
        }
        int num4 = this._length - (num1 << 5);
        int num5 = 0;
        for (int index = 0; index < num4; ++index)
        {
          if (values[num2 + index])
            num5 |= 1 << index;
        }
        this._array[this._length - 1 >> 5] = num5;
      }
      this._version = 0;
    }

    public BooleanArray(byte[] bytes)
    {
      this._array = bytes != null ? new int[bytes.Length + 3 >> 2] : throw new ArgumentNullException(nameof (bytes));
      this._length = bytes.Length << 3;
      int index1 = 0;
      int index2;
      for (index2 = 0; bytes.Length - index2 >= 4; index2 += 4)
        this._array[index1++] = (int) bytes[index2] & (int) byte.MaxValue | ((int) bytes[index2 + 1] & (int) byte.MaxValue) << 8 | ((int) bytes[index2 + 2] & (int) byte.MaxValue) << 16 | ((int) bytes[index2 + 3] & (int) byte.MaxValue) << 24;
      switch (bytes.Length - index2)
      {
        case 1:
          this._array[index1] |= (int) bytes[index2] & (int) byte.MaxValue;
          break;
        case 2:
          this._array[index1] |= ((int) bytes[index2 + 1] & (int) byte.MaxValue) << 8;
          this._array[index1] |= (int) bytes[index2] & (int) byte.MaxValue;
          break;
        case 3:
          this._array[index1] = ((int) bytes[index2 + 2] & (int) byte.MaxValue) << 16;
          this._array[index1] |= ((int) bytes[index2 + 1] & (int) byte.MaxValue) << 8;
          this._array[index1] |= (int) bytes[index2] & (int) byte.MaxValue;
          break;
      }
      this._version = 0;
    }

    public BooleanArray(int[] values)
    {
      this._array = values != null ? new int[values.Length] : throw new ArgumentNullException(nameof (values));
      this._length = values.Length << 5;
      Array.Copy((Array) values, (Array) this._array, values.Length);
      this._version = 0;
    }

    public BooleanArray(BooleanArray bits)
    {
      this._array = bits != null ? new int[bits._length + 31 >> 5] : throw new ArgumentNullException(nameof (bits));
      this._length = bits._length;
      Array.Copy((Array) bits._array, (Array) this._array, bits._length + 31 >> 5);
      this._version = bits._version;
    }

    public BooleanArray(int length, bool defaultValue)
    {
      this._array = length >= 0 ? new int[length + 31 >> 5] : throw new ArgumentOutOfRangeException(nameof (length));
      this._length = length;
      int num = defaultValue ? -1 : 0;
      for (int index = 0; index < this._array.Length; ++index)
        this._array[index] = num;
      this._version = 0;
    }

    public bool Get(char letter) => (int) letter < this._length && this.Get((int) letter);

    public bool Get(int index)
    {
      if (index < 0 || index >= this._length)
        throw new ArgumentOutOfRangeException(nameof (index));
      int index1 = index >> 5;
      return (this._array[index1] & 1 << index - (index1 << 5)) != 0;
    }

    public bool Get(int index, bool outOfRangeValue)
    {
      if (index < 0 || index >= this._length)
        return outOfRangeValue;
      int index1 = index >> 5;
      return (this._array[index1] & 1 << index - (index1 << 5)) != 0;
    }

    public void Set(int index, bool value)
    {
      if (index < 0 || index >= this._length)
        throw new ArgumentOutOfRangeException(nameof (index));
      int index1 = index >> 5;
      if (value)
        this._array[index1] |= 1 << index - (index1 << 5);
      else
        this._array[index1] &= ~(1 << index - (index1 << 5));
      ++this._version;
    }

    public void SetAll(bool value)
    {
      int num1 = value ? -1 : 0;
      int num2 = this._length + 31 >> 5;
      for (int index = 0; index < num2; ++index)
        this._array[index] = num1;
      ++this._version;
    }

    private void SetLength(int value)
    {
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof (value));
      int length = value + 31 >> 5;
      if (length > this._array.Length || length + 256 < this._array.Length)
      {
        int[] destinationArray = new int[length];
        Array.Copy((Array) this._array, (Array) destinationArray, length > this._array.Length ? this._array.Length : length);
        this._array = destinationArray;
      }
      if (value > this._length)
      {
        int index = (this._length + 31 >> 5) - 1;
        int num = this._length - (this._length >> 5 << 5);
        if (num > 0)
          this._array[index] &= (1 << num) - 1;
        Array.Clear((Array) this._array, index + 1, length - index - 1);
      }
      this._length = value;
      ++this._version;
    }

    public BooleanArray And(BooleanArray value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (this._length != value._length)
        throw new ArgumentException();
      int num = this._length + 31 >> 5;
      for (int index = 0; index < num; ++index)
        this._array[index] &= value._array[index];
      ++this._version;
      return this;
    }

    public BooleanArray Not()
    {
      int num = this._length + 31 >> 5;
      for (int index = 0; index < num; ++index)
        this._array[index] = ~this._array[index];
      ++this._version;
      return this;
    }

    public BooleanArray Or(BooleanArray value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (this._length != value._length)
        throw new ArgumentException();
      int num = this._length + 31 >> 5;
      for (int index = 0; index < num; ++index)
        this._array[index] |= value._array[index];
      ++this._version;
      return this;
    }

    public BooleanArray Xor(BooleanArray value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (this._length != value._length)
        throw new ArgumentException();
      int num = this._length + 31 >> 5;
      for (int index = 0; index < num; ++index)
        this._array[index] ^= value._array[index];
      ++this._version;
      return this;
    }

    public void CopyTo(Array array, int index)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof (index));
      if (array.Rank != 1)
        throw new ArgumentException();
      if (this._length == 0)
        return;
      switch (array)
      {
        case int[] _:
          Array.Copy((Array) this._array, 0, array, index, this._length + 31 >> 5);
          break;
        case byte[] _:
          int num1 = this._length + 7 >> 3;
          if (array.Length - index < num1)
            throw new ArgumentException();
          byte[] numArray = (byte[]) array;
          for (int index1 = 0; index1 < num1; ++index1)
          {
            int index2 = index1 >> 2;
            numArray[index + index1] = (byte) (this._array[index2] >> (index1 - (index2 << 2) << 3) & (int) byte.MaxValue);
          }
          break;
        case bool[] _:
          if (array.Length - index < this._length)
            throw new ArgumentException();
          bool[] flagArray = (bool[]) array;
          int index3 = this._length - 1 >> 5;
          for (int index4 = 0; index4 < index3; ++index4)
          {
            int num2 = this._array[index4];
            for (int index5 = 0; index5 < 32; ++index5)
              flagArray[index + index5] = (num2 >> index5 & 1) != 0;
            index += 32;
          }
          int num3 = this._array[index3];
          int num4 = this._length - (index3 << 5);
          for (int index6 = 0; index6 < num4; ++index6)
            flagArray[index + index6] = (num3 >> index6 & 1) != 0;
          break;
        default:
          throw new ArgumentException();
      }
    }

    public IEnumerator GetEnumerator() => (IEnumerator) new BooleanArray.BooleanArrayEnumeratorSimple(this);

    private sealed class BooleanArrayEnumeratorSimple : IEnumerator
    {
      private BooleanArray _booleanArray;
      private bool _currentElement;
      private int _index;
      private int _version;

      public object Current
      {
        get
        {
          if (this._index == -1 || this._index >= this._booleanArray.Length)
            throw new InvalidOperationException();
          return (object) this._currentElement;
        }
      }

      internal BooleanArrayEnumeratorSimple(BooleanArray booleanArray)
      {
        this._booleanArray = booleanArray;
        this._index = -1;
        this._version = booleanArray._version;
      }

      public bool MoveNext()
      {
        if (this._version != this._booleanArray._version)
          throw new InvalidOperationException();
        if (this._index < this._booleanArray.Length - 1)
        {
          ++this._index;
          this._currentElement = this._booleanArray.Get(this._index);
          return true;
        }
        this._index = this._booleanArray.Length;
        return false;
      }

      public void Reset()
      {
        if (this._version != this._booleanArray._version)
          throw new InvalidOperationException();
        this._index = -1;
      }
    }
  }
}
