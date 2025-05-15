// Decompiled with JetBrains decompiler
// Type: System.Cryptography.BigInteger
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Cryptography
{
  public sealed class BigInteger
  {
    private const int MaxLength = 70;
    private static readonly int[] _primesBelow2000 = new int[303]
    {
      2,
      3,
      5,
      7,
      11,
      13,
      17,
      19,
      23,
      29,
      31,
      37,
      41,
      43,
      47,
      53,
      59,
      61,
      67,
      71,
      73,
      79,
      83,
      89,
      97,
      101,
      103,
      107,
      109,
      113,
      (int) sbyte.MaxValue,
      131,
      137,
      139,
      149,
      151,
      157,
      163,
      167,
      173,
      179,
      181,
      191,
      193,
      197,
      199,
      211,
      223,
      227,
      229,
      233,
      239,
      241,
      251,
      257,
      263,
      269,
      271,
      277,
      281,
      283,
      293,
      307,
      311,
      313,
      317,
      331,
      337,
      347,
      349,
      353,
      359,
      367,
      373,
      379,
      383,
      389,
      397,
      401,
      409,
      419,
      421,
      431,
      433,
      439,
      443,
      449,
      457,
      461,
      463,
      467,
      479,
      487,
      491,
      499,
      503,
      509,
      521,
      523,
      541,
      547,
      557,
      563,
      569,
      571,
      577,
      587,
      593,
      599,
      601,
      607,
      613,
      617,
      619,
      631,
      641,
      643,
      647,
      653,
      659,
      661,
      673,
      677,
      683,
      691,
      701,
      709,
      719,
      727,
      733,
      739,
      743,
      751,
      757,
      761,
      769,
      773,
      787,
      797,
      809,
      811,
      821,
      823,
      827,
      829,
      839,
      853,
      857,
      859,
      863,
      877,
      881,
      883,
      887,
      907,
      911,
      919,
      929,
      937,
      941,
      947,
      953,
      967,
      971,
      977,
      983,
      991,
      997,
      1009,
      1013,
      1019,
      1021,
      1031,
      1033,
      1039,
      1049,
      1051,
      1061,
      1063,
      1069,
      1087,
      1091,
      1093,
      1097,
      1103,
      1109,
      1117,
      1123,
      1129,
      1151,
      1153,
      1163,
      1171,
      1181,
      1187,
      1193,
      1201,
      1213,
      1217,
      1223,
      1229,
      1231,
      1237,
      1249,
      1259,
      1277,
      1279,
      1283,
      1289,
      1291,
      1297,
      1301,
      1303,
      1307,
      1319,
      1321,
      1327,
      1361,
      1367,
      1373,
      1381,
      1399,
      1409,
      1423,
      1427,
      1429,
      1433,
      1439,
      1447,
      1451,
      1453,
      1459,
      1471,
      1481,
      1483,
      1487,
      1489,
      1493,
      1499,
      1511,
      1523,
      1531,
      1543,
      1549,
      1553,
      1559,
      1567,
      1571,
      1579,
      1583,
      1597,
      1601,
      1607,
      1609,
      1613,
      1619,
      1621,
      1627,
      1637,
      1657,
      1663,
      1667,
      1669,
      1693,
      1697,
      1699,
      1709,
      1721,
      1723,
      1733,
      1741,
      1747,
      1753,
      1759,
      1777,
      1783,
      1787,
      1789,
      1801,
      1811,
      1823,
      1831,
      1847,
      1861,
      1867,
      1871,
      1873,
      1877,
      1879,
      1889,
      1901,
      1907,
      1913,
      1931,
      1933,
      1949,
      1951,
      1973,
      1979,
      1987,
      1993,
      1997,
      1999
    };
    private uint[] _data;
    private int _length;

    private static BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
    {
      int length = n._length;
      int index1 = length + 1;
      int num1 = length - 1;
      BigInteger bigInteger1 = new BigInteger();
      int index2 = num1;
      int index3 = 0;
      while (index2 < x._length)
      {
        bigInteger1._data[index3] = x._data[index2];
        ++index2;
        ++index3;
      }
      bigInteger1._length = x._length - num1;
      if (bigInteger1._length <= 0)
        bigInteger1._length = 1;
      BigInteger bigInteger2 = bigInteger1 * constant;
      BigInteger bigInteger3 = new BigInteger();
      int index4 = index1;
      int index5 = 0;
      while (index4 < bigInteger2._length)
      {
        bigInteger3._data[index5] = bigInteger2._data[index4];
        ++index4;
        ++index5;
      }
      bigInteger3._length = bigInteger2._length - index1;
      if (bigInteger3._length <= 0)
        bigInteger3._length = 1;
      BigInteger bigInteger4 = new BigInteger();
      int num2 = x._length > index1 ? index1 : x._length;
      for (int index6 = 0; index6 < num2; ++index6)
        bigInteger4._data[index6] = x._data[index6];
      bigInteger4._length = num2;
      BigInteger bigInteger5 = new BigInteger();
      for (int index7 = 0; index7 < bigInteger3._length; ++index7)
      {
        if (bigInteger3._data[index7] != 0U)
        {
          ulong num3 = 0;
          int index8 = index7;
          for (int index9 = 0; index9 < n._length && index8 < index1; ++index8)
          {
            ulong num4 = (ulong) bigInteger3._data[index7] * (ulong) n._data[index9] + (ulong) bigInteger5._data[index8] + num3;
            bigInteger5._data[index8] = (uint) (num4 & (ulong) uint.MaxValue);
            num3 = num4 >> 32;
            ++index9;
          }
          if (index8 < index1)
            bigInteger5._data[index8] = (uint) num3;
        }
      }
      bigInteger5._length = index1;
      while (bigInteger5._length > 1 && bigInteger5._data[bigInteger5._length - 1] == 0U)
        --bigInteger5._length;
      BigInteger bigInteger6 = bigInteger4 - bigInteger5;
      if (((int) bigInteger6._data[69] & int.MinValue) != 0)
      {
        BigInteger bigInteger7 = new BigInteger();
        bigInteger7._data[index1] = 1U;
        bigInteger7._length = index1 + 1;
        bigInteger6 += bigInteger7;
      }
      while (bigInteger6 >= n)
        bigInteger6 -= n;
      return bigInteger6;
    }

    private static bool LucasStrongTestHelper(BigInteger thisVal)
    {
      long a = 5;
      long num1 = -1;
      long num2 = 0;
      bool flag1 = false;
      while (!flag1)
      {
        switch (BigInteger.Jacobi((BigInteger) a, thisVal))
        {
          case -1:
            flag1 = true;
            break;
          case 0:
            if (thisVal > Math.Abs(a))
              return false;
            goto default;
          default:
            if (num2 == 20L)
            {
              BigInteger bigInteger = thisVal.Sqrt();
              if (bigInteger * bigInteger == thisVal)
                return false;
            }
            a = (Math.Abs(a) + 2L) * num1;
            num1 = -num1;
            break;
        }
        ++num2;
      }
      long num3 = 1L - a >> 2;
      BigInteger bigInteger1 = thisVal + 1;
      int num4 = 0;
      for (int index1 = 0; index1 < bigInteger1._length; ++index1)
      {
        uint num5 = 1;
        for (int index2 = 0; index2 < 32; ++index2)
        {
          if (((int) bigInteger1._data[index1] & (int) num5) != 0)
          {
            index1 = bigInteger1._length;
            break;
          }
          num5 <<= 1;
          ++num4;
        }
      }
      BigInteger k = bigInteger1 >> num4;
      BigInteger bigInteger2 = new BigInteger();
      int index3 = thisVal._length << 1;
      bigInteger2._data[index3] = 1U;
      bigInteger2._length = index3 + 1;
      BigInteger constant = bigInteger2 / thisVal;
      BigInteger[] bigIntegerArray1 = BigInteger.LucasSequenceHelper((BigInteger) 1, (BigInteger) num3, k, thisVal, constant, 0);
      bool flag2 = false;
      if (bigIntegerArray1[0]._length == 1 && bigIntegerArray1[0]._data[0] == 0U || bigIntegerArray1[1]._length == 1 && bigIntegerArray1[1]._data[0] == 0U)
        flag2 = true;
      for (int index4 = 1; index4 < num4; ++index4)
      {
        if (!flag2)
        {
          bigIntegerArray1[1] = BigInteger.BarrettReduction(bigIntegerArray1[1] * bigIntegerArray1[1], thisVal, constant);
          bigIntegerArray1[1] = (bigIntegerArray1[1] - (bigIntegerArray1[2] << 1)) % thisVal;
          if (bigIntegerArray1[1]._length == 1 && bigIntegerArray1[1]._data[0] == 0U)
            flag2 = true;
        }
        bigIntegerArray1[2] = BigInteger.BarrettReduction(bigIntegerArray1[2] * bigIntegerArray1[2], thisVal, constant);
      }
      if (flag2)
      {
        BigInteger bigInteger3 = thisVal.GCD((BigInteger) num3);
        if (bigInteger3._length == 1 && bigInteger3._data[0] == 1U)
        {
          if (((int) bigIntegerArray1[2]._data[69] & int.MinValue) != 0)
          {
            BigInteger[] bigIntegerArray2;
            (bigIntegerArray2 = bigIntegerArray1)[2] = bigIntegerArray2[2] + thisVal;
          }
          BigInteger bigInteger4 = num3 * (long) BigInteger.Jacobi((BigInteger) num3, thisVal) % thisVal;
          if (((int) bigInteger4._data[69] & int.MinValue) != 0)
            bigInteger4 += thisVal;
          if (bigIntegerArray1[2] != bigInteger4)
            flag2 = false;
        }
      }
      return flag2;
    }

    private static int Jacobi(BigInteger a, BigInteger b)
    {
      if (((int) b._data[0] & 1) == 0)
        throw new ArgumentException("Jacobi defined only for odd integers.");
      if (a >= b)
        a %= b;
      if (a._length == 1 && a._data[0] == 0U)
        return 0;
      if (a._length == 1 && a._data[0] == 1U)
        return 1;
      if (a < 0)
        return ((int) (b - 1)._data[0] & 2) == 0 ? BigInteger.Jacobi(-a, b) : -BigInteger.Jacobi(-a, b);
      int num1 = 0;
      for (int index1 = 0; index1 < a._length; ++index1)
      {
        uint num2 = 1;
        for (int index2 = 0; index2 < 32; ++index2)
        {
          if (((int) a._data[index1] & (int) num2) != 0)
          {
            index1 = a._length;
            break;
          }
          num2 <<= 1;
          ++num1;
        }
      }
      BigInteger b1 = a >> num1;
      int num3 = 1;
      if ((num1 & 1) != 0 && (((int) b._data[0] & 7) == 3 || ((int) b._data[0] & 7) == 5))
        num3 = -1;
      if (((int) b._data[0] & 3) == 3 && ((int) b1._data[0] & 3) == 3)
        num3 = -num3;
      return b1._length == 1 && b1._data[0] == 1U ? num3 : num3 * BigInteger.Jacobi(b % b1, b1);
    }

    private static void Reverse<T>(T[] buffer, int length)
    {
      for (int index = 0; index < length / 2; ++index)
      {
        T obj = buffer[index];
        buffer[index] = buffer[length - index - 1];
        buffer[length - index - 1] = obj;
      }
    }

    private static void Reverse<T>(T[] buffer) => BigInteger.Reverse<T>(buffer, buffer.Length);

    private static BigInteger[] LucasSequence(
      BigInteger P,
      BigInteger Q,
      BigInteger k,
      BigInteger n)
    {
      if (k._length == 1 && k._data[0] == 0U)
        return new BigInteger[3]
        {
          (BigInteger) 0,
          2 % n,
          1 % n
        };
      BigInteger bigInteger = new BigInteger();
      int index1 = n._length << 1;
      bigInteger._data[index1] = 1U;
      bigInteger._length = index1 + 1;
      BigInteger constant = bigInteger / n;
      int s = 0;
      for (int index2 = 0; index2 < k._length; ++index2)
      {
        uint num = 1;
        for (int index3 = 0; index3 < 32; ++index3)
        {
          if (((int) k._data[index2] & (int) num) != 0)
          {
            index2 = k._length;
            break;
          }
          num <<= 1;
          ++s;
        }
      }
      BigInteger k1 = k >> s;
      return BigInteger.LucasSequenceHelper(P, Q, k1, n, constant, s);
    }

    private static BigInteger[] LucasSequenceHelper(
      BigInteger P,
      BigInteger Q,
      BigInteger k,
      BigInteger n,
      BigInteger constant,
      int s)
    {
      BigInteger[] bigIntegerArray = new BigInteger[3];
      if (((int) k._data[0] & 1) == 0)
        throw new ArgumentException("Argument k must be odd.");
      uint num = (uint) (1 << (k.BitCount() & 31) - 1);
      BigInteger bigInteger1 = 2 % n;
      BigInteger bigInteger2 = 1 % n;
      BigInteger bigInteger3 = P % n;
      BigInteger bigInteger4 = bigInteger2;
      bool flag = true;
      for (int index = k._length - 1; index >= 0; --index)
      {
        for (; num != 0U && (index != 0 || num != 1U); num >>= 1)
        {
          if (((int) k._data[index] & (int) num) != 0)
          {
            bigInteger4 = bigInteger4 * bigInteger3 % n;
            bigInteger1 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
            bigInteger3 = (BigInteger.BarrettReduction(bigInteger3 * bigInteger3, n, constant) - (bigInteger2 * Q << 1)) % n;
            if (flag)
              flag = false;
            else
              bigInteger2 = BigInteger.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
            bigInteger2 = bigInteger2 * Q % n;
          }
          else
          {
            bigInteger4 = (bigInteger4 * bigInteger1 - bigInteger2) % n;
            bigInteger3 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
            bigInteger1 = (BigInteger.BarrettReduction(bigInteger1 * bigInteger1, n, constant) - (bigInteger2 << 1)) % n;
            if (flag)
            {
              bigInteger2 = Q % n;
              flag = false;
            }
            else
              bigInteger2 = BigInteger.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
          }
        }
        num = 2147483648U;
      }
      BigInteger bigInteger5 = (bigInteger4 * bigInteger1 - bigInteger2) % n;
      BigInteger bigInteger6 = (bigInteger1 * bigInteger3 - P * bigInteger2) % n;
      if (flag)
        flag = false;
      else
        bigInteger2 = BigInteger.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
      BigInteger bigInteger7 = bigInteger2 * Q % n;
      for (int index = 0; index < s; ++index)
      {
        bigInteger5 = bigInteger5 * bigInteger6 % n;
        bigInteger6 = (bigInteger6 * bigInteger6 - (bigInteger7 << 1)) % n;
        if (flag)
        {
          bigInteger7 = Q % n;
          flag = false;
        }
        else
          bigInteger7 = BigInteger.BarrettReduction(bigInteger7 * bigInteger7, n, constant);
      }
      bigIntegerArray[0] = bigInteger5;
      bigIntegerArray[1] = bigInteger6;
      bigIntegerArray[2] = bigInteger7;
      return bigIntegerArray;
    }

    public BigInteger()
    {
      this._data = new uint[70];
      this._length = 1;
    }

    public BigInteger(long value)
    {
      this._data = new uint[70];
      long num = value;
      for (this._length = 0; value != 0L && this._length < 70; ++this._length)
      {
        this._data[this._length] = (uint) ((ulong) value & (ulong) uint.MaxValue);
        value >>= 32;
      }
      if (num > 0L)
      {
        if (value != 0L || ((int) this._data[69] & int.MinValue) != 0)
          throw new ArithmeticException("Positive overflow in constructor.");
      }
      else if (num < 0L && (value != -1L || ((int) this._data[this._length - 1] & int.MinValue) == 0))
        throw new ArithmeticException("Negative underflow in constructor.");
      if (this._length != 0)
        return;
      this._length = 1;
    }

    public BigInteger(ulong value)
    {
      this._data = new uint[70];
      for (this._length = 0; value != 0UL && this._length < 70; ++this._length)
      {
        this._data[this._length] = (uint) (value & (ulong) uint.MaxValue);
        value >>= 32;
      }
      if (value != 0UL || ((int) this._data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive overflow in constructor.");
      if (this._length != 0)
        return;
      this._length = 1;
    }

    public BigInteger(BigInteger bi) => this.SetValue(bi);

    public void SetValue(BigInteger bi)
    {
      this._data = new uint[70];
      this._length = bi._length;
      for (int index = 0; index < this._length; ++index)
        this._data[index] = bi._data[index];
    }

    public BigInteger(string value, int radix)
    {
      BigInteger bigInteger1 = new BigInteger(1L);
      BigInteger bigInteger2 = new BigInteger();
      value = value.ToUpper().Trim();
      int num1 = 0;
      if (value[0] == '-')
        num1 = 1;
      for (int index = value.Length - 1; index >= num1; --index)
      {
        int num2 = (int) value[index];
        int num3 = num2 < 48 || num2 > 57 ? (num2 < 65 || num2 > 90 ? 9999999 : num2 - 65 + 10) : num2 - 48;
        if (num3 >= radix)
          throw new ArithmeticException("Invalid string in constructor.");
        if (value[0] == '-')
          num3 = -num3;
        bigInteger2 += bigInteger1 * num3;
        if (index - 1 >= num1)
          bigInteger1 *= radix;
      }
      if (value[0] == '-')
      {
        if (((int) bigInteger2._data[69] & int.MinValue) == 0)
          throw new ArithmeticException("Negative underflow in constructor.");
      }
      else if (((int) bigInteger2._data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive overflow in constructor.");
      this._data = new uint[70];
      for (int index = 0; index < bigInteger2._length; ++index)
        this._data[index] = bigInteger2._data[index];
      this._length = bigInteger2._length;
    }

    public BigInteger(byte[] inData)
    {
      inData = (byte[]) inData.Clone();
      BigInteger.Reverse<byte>(inData);
      this._length = inData.Length >> 2;
      int num = inData.Length & 3;
      if (num != 0)
        ++this._length;
      if (this._length > 70)
        throw new ArithmeticException("Byte overflow in constructor.");
      this._data = new uint[70];
      int index1 = inData.Length - 1;
      int index2 = 0;
      while (index1 >= 3)
      {
        this._data[index2] = (uint) (((int) inData[index1 - 3] << 24) + ((int) inData[index1 - 2] << 16) + ((int) inData[index1 - 1] << 8)) + (uint) inData[index1];
        index1 -= 4;
        ++index2;
      }
      switch (num)
      {
        case 1:
          this._data[this._length - 1] = (uint) inData[0];
          break;
        case 2:
          this._data[this._length - 1] = ((uint) inData[0] << 8) + (uint) inData[1];
          break;
        case 3:
          this._data[this._length - 1] = (uint) (((int) inData[0] << 16) + ((int) inData[1] << 8)) + (uint) inData[2];
          break;
      }
      while (this._length > 1 && this._data[this._length - 1] == 0U)
        --this._length;
    }

    public BigInteger(byte[] inData, int inLen)
    {
      this._length = inLen >> 2;
      int num = inLen & 3;
      if (num != 0)
        ++this._length;
      if (this._length > 70 || inLen > inData.Length)
        throw new ArithmeticException("Byte overflow in constructor.");
      this._data = new uint[70];
      int index1 = inLen - 1;
      int index2 = 0;
      while (index1 >= 3)
      {
        this._data[index2] = (uint) (((int) inData[index1 - 3] << 24) + ((int) inData[index1 - 2] << 16) + ((int) inData[index1 - 1] << 8)) + (uint) inData[index1];
        index1 -= 4;
        ++index2;
      }
      switch (num)
      {
        case 1:
          this._data[this._length - 1] = (uint) inData[0];
          break;
        case 2:
          this._data[this._length - 1] = ((uint) inData[0] << 8) + (uint) inData[1];
          break;
        case 3:
          this._data[this._length - 1] = (uint) (((int) inData[0] << 16) + ((int) inData[1] << 8)) + (uint) inData[2];
          break;
      }
      if (this._length == 0)
        this._length = 1;
      while (this._length > 1 && this._data[this._length - 1] == 0U)
        --this._length;
    }

    public BigInteger(uint[] inData)
    {
      this._length = inData.Length;
      if (this._length > 70)
        throw new ArithmeticException("Byte overflow in constructor.");
      this._data = new uint[70];
      int index1 = this._length - 1;
      int index2 = 0;
      while (index1 >= 0)
      {
        this._data[index2] = inData[index1];
        --index1;
        ++index2;
      }
      while (this._length > 1 && this._data[this._length - 1] == 0U)
        --this._length;
    }

    public BigInteger(Random rand, int bitLength)
    {
      this._data = new uint[70];
      this._length = 1;
      this.GenerateRandomBits(bitLength, rand);
    }

    public string ToString(int radix)
    {
      if (radix < 2 || radix > 36)
        throw new ArgumentException("Radix must be >= 2 and <= 36");
      string str1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      string str2 = "";
      BigInteger bi1 = this;
      bool flag = false;
      if (((int) bi1._data[69] & int.MinValue) != 0)
      {
        flag = true;
        try
        {
          bi1 = -bi1;
        }
        catch (Exception ex)
        {
        }
      }
      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger();
      BigInteger bi2 = new BigInteger((long) radix);
      if (bi1._length == 1 && bi1._data[0] == 0U)
      {
        str2 = "0";
      }
      else
      {
        for (; bi1._length > 1 || bi1._length == 1 && bi1._data[0] != 0U; bi1 = outQuotient)
        {
          BigInteger.singleByteDivide(bi1, bi2, outQuotient, outRemainder);
          str2 = outRemainder._data[0] >= 10U ? str1[(int) outRemainder._data[0] - 10].ToString() + str2 : outRemainder._data[0].ToString() + str2;
        }
        if (flag)
          str2 = "-" + str2;
      }
      return str2;
    }

    public string ToHexString()
    {
      string hexString = this._data[this._length - 1].ToString("X");
      for (int index = this._length - 2; index >= 0; --index)
        hexString += this._data[index].ToString("X8");
      return hexString;
    }

    public override string ToString() => "0x" + this.ToString(16);

    public override int GetHashCode() => this.ToString().GetHashCode();

    public byte[] GetBytes()
    {
      int num = this.BitCount();
      int numBytes = num >> 3;
      if ((num & 7) != 0)
        ++numBytes;
      return this.GetBytes(numBytes);
    }

    public byte[] GetBytesBE()
    {
      int num = this.BitCount();
      int numBytes = num >> 3;
      if ((num & 7) != 0)
        ++numBytes;
      return this.GetBytesBE(numBytes);
    }

    public byte[] GetBytes(int numBytes)
    {
      byte[] bytes = new byte[numBytes];
      int num1 = this.BitCount();
      int num2 = num1 >> 3;
      if ((num1 & 7) != 0)
        ++num2;
      for (int index1 = 0; index1 < num2; ++index1)
      {
        for (int index2 = 0; index2 < 4; ++index2)
        {
          if (index1 * 4 + index2 >= num2)
            return bytes;
          bytes[index1 * 4 + index2] = (byte) (this._data[index1] >> index2 * 8 & (uint) byte.MaxValue);
        }
      }
      return bytes;
    }

    public byte[] GetBytesBE(int numBytes)
    {
      byte[] bytes = this.GetBytes(numBytes);
      BigInteger.Reverse<byte>(bytes);
      return bytes;
    }

    public static explicit operator BigInteger(long value) => new BigInteger(value);

    public static explicit operator BigInteger(ulong value) => new BigInteger(value);

    public static explicit operator BigInteger(int value) => new BigInteger((long) value);

    public static explicit operator BigInteger(uint value) => new BigInteger((ulong) value);

    public static implicit operator BigInteger(byte[] value) => new BigInteger(value);

    public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      bigInteger._length = bi1._length > bi2._length ? bi1._length : bi2._length;
      long num1 = 0;
      for (int index = 0; index < bigInteger._length; ++index)
      {
        long num2 = (long) bi1._data[index] + (long) bi2._data[index] + num1;
        num1 = num2 >> 32;
        bigInteger._data[index] = (uint) ((ulong) num2 & (ulong) uint.MaxValue);
      }
      if (num1 != 0L && bigInteger._length < 70)
      {
        bigInteger._data[bigInteger._length] = (uint) num1;
        ++bigInteger._length;
      }
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      int index1 = 69;
      if (((int) bi1._data[index1] & int.MinValue) == ((int) bi2._data[index1] & int.MinValue) && ((int) bigInteger._data[index1] & int.MinValue) != ((int) bi1._data[index1] & int.MinValue))
        throw new ArithmeticException();
      return bigInteger;
    }

    public static BigInteger operator +(BigInteger bi1, long bi2) => bi1 + (BigInteger) bi2;

    public static BigInteger operator +(BigInteger bi1, ulong bi2) => bi1 + (BigInteger) bi2;

    public static BigInteger operator +(BigInteger bi1, int bi2) => bi1 + (BigInteger) bi2;

    public static BigInteger operator +(BigInteger bi1, uint bi2) => bi1 + (BigInteger) bi2;

    public static BigInteger operator ++(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      long num1 = 1;
      int index1;
      for (index1 = 0; num1 != 0L && index1 < 70; ++index1)
      {
        long num2 = (long) bigInteger._data[index1] + 1L;
        bigInteger._data[index1] = (uint) ((ulong) num2 & (ulong) uint.MaxValue);
        num1 = num2 >> 32;
      }
      if (index1 > bigInteger._length)
      {
        bigInteger._length = index1;
      }
      else
      {
        while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
          --bigInteger._length;
      }
      int index2 = 69;
      if (((int) bi1._data[index2] & int.MinValue) == 0 && ((int) bigInteger._data[index2] & int.MinValue) != ((int) bi1._data[index2] & int.MinValue))
        throw new ArithmeticException("Overflow in ++.");
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1)
    {
      if (bi1._length == 1 && bi1._data[0] == 0U)
        return new BigInteger();
      BigInteger bigInteger = new BigInteger(bi1);
      for (int index = 0; index < 70; ++index)
        bigInteger._data[index] = ~bi1._data[index];
      long num1 = 1;
      for (int index = 0; num1 != 0L && index < 70; ++index)
      {
        long num2 = (long) bigInteger._data[index] + 1L;
        bigInteger._data[index] = (uint) ((ulong) num2 & (ulong) uint.MaxValue);
        num1 = num2 >> 32;
      }
      if (((int) bi1._data[69] & int.MinValue) == ((int) bigInteger._data[69] & int.MinValue))
        throw new ArithmeticException("Overflow in negation.\n");
      bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      bigInteger._length = bi1._length > bi2._length ? bi1._length : bi2._length;
      long num1 = 0;
      for (int index = 0; index < bigInteger._length; ++index)
      {
        long num2 = (long) bi1._data[index] - (long) bi2._data[index] - num1;
        bigInteger._data[index] = (uint) ((ulong) num2 & (ulong) uint.MaxValue);
        num1 = num2 >= 0L ? 0L : 1L;
      }
      if (num1 != 0L)
      {
        for (int length = bigInteger._length; length < 70; ++length)
          bigInteger._data[length] = uint.MaxValue;
        bigInteger._length = 70;
      }
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      int index1 = 69;
      if (((int) bi1._data[index1] & int.MinValue) != ((int) bi2._data[index1] & int.MinValue) && ((int) bigInteger._data[index1] & int.MinValue) != ((int) bi1._data[index1] & int.MinValue))
        throw new ArithmeticException();
      return bigInteger;
    }

    public static BigInteger operator -(BigInteger bi1, long bi2) => bi1 - (BigInteger) bi2;

    public static BigInteger operator -(BigInteger bi1, ulong bi2) => bi1 - (BigInteger) bi2;

    public static BigInteger operator -(BigInteger bi1, int bi2) => bi1 - (BigInteger) bi2;

    public static BigInteger operator -(BigInteger bi1, uint bi2) => bi1 - (BigInteger) bi2;

    public static BigInteger operator --(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bool flag = true;
      int index1;
      for (index1 = 0; flag && index1 < 70; ++index1)
      {
        long num = (long) bigInteger._data[index1] - 1L;
        bigInteger._data[index1] = (uint) ((ulong) num & (ulong) uint.MaxValue);
        if (num >= 0L)
          flag = false;
      }
      if (index1 > bigInteger._length)
        bigInteger._length = index1;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      int index2 = 69;
      if (((int) bi1._data[index2] & int.MinValue) != 0 && ((int) bigInteger._data[index2] & int.MinValue) != ((int) bi1._data[index2] & int.MinValue))
        throw new ArithmeticException("Underflow in --.");
      return bigInteger;
    }

    public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      bool flag1 = false;
      bool flag2 = false;
      try
      {
        if (((int) bi1._data[index1] & int.MinValue) != 0)
        {
          flag1 = true;
          bi1 = -bi1;
        }
        if (((int) bi2._data[index1] & int.MinValue) != 0)
        {
          flag2 = true;
          bi2 = -bi2;
        }
      }
      catch (Exception ex)
      {
      }
      BigInteger bigInteger = new BigInteger();
      try
      {
        for (int index2 = 0; index2 < bi1._length; ++index2)
        {
          if (bi1._data[index2] != 0U)
          {
            ulong num1 = 0;
            int index3 = 0;
            int index4 = index2;
            while (index3 < bi2._length)
            {
              ulong num2 = (ulong) bi1._data[index2] * (ulong) bi2._data[index3] + (ulong) bigInteger._data[index4] + num1;
              bigInteger._data[index4] = (uint) (num2 & (ulong) uint.MaxValue);
              num1 = num2 >> 32;
              ++index3;
              ++index4;
            }
            if (num1 != 0UL)
              bigInteger._data[index2 + bi2._length] = (uint) num1;
          }
        }
      }
      catch (Exception ex)
      {
        throw new ArithmeticException("Multiplication overflow.");
      }
      bigInteger._length = bi1._length + bi2._length;
      if (bigInteger._length > 70)
        bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      if (((int) bigInteger._data[index1] & int.MinValue) != 0)
      {
        if (flag1 != flag2 && bigInteger._data[index1] == 2147483648U)
        {
          if (bigInteger._length == 1)
            return bigInteger;
          bool flag3 = true;
          for (int index5 = 0; index5 < bigInteger._length - 1 && flag3; ++index5)
          {
            if (bigInteger._data[index5] != 0U)
              flag3 = false;
          }
          if (flag3)
            return bigInteger;
        }
        throw new ArithmeticException("Multiplication overflow.");
      }
      return flag1 != flag2 ? -bigInteger : bigInteger;
    }

    public static BigInteger operator *(BigInteger bi1, long bi2) => bi1 * (BigInteger) bi2;

    public static BigInteger operator *(BigInteger bi1, ulong bi2) => bi1 * (BigInteger) bi2;

    public static BigInteger operator *(BigInteger bi1, int bi2) => bi1 * (BigInteger) bi2;

    public static BigInteger operator *(BigInteger bi1, uint bi2) => bi1 * (BigInteger) bi2;

    private static void multiByteDivide(
      BigInteger bi1,
      BigInteger bi2,
      BigInteger outQuotient,
      BigInteger outRemainder)
    {
      uint[] numArray = new uint[70];
      int length1 = bi1._length + 1;
      uint[] buffer = new uint[length1];
      uint num1 = 2147483648;
      uint num2 = bi2._data[bi2._length - 1];
      int shiftVal = 0;
      int num3 = 0;
      for (; num1 != 0U && ((int) num2 & (int) num1) == 0; num1 >>= 1)
        ++shiftVal;
      for (int index = 0; index < bi1._length; ++index)
        buffer[index] = bi1._data[index];
      BigInteger.shiftLeft(buffer, shiftVal);
      bi2 <<= shiftVal;
      int num4 = length1 - bi2._length;
      int index1 = length1 - 1;
      ulong num5 = (ulong) bi2._data[bi2._length - 1];
      ulong num6 = (ulong) bi2._data[bi2._length - 2];
      int length2 = bi2._length + 1;
      uint[] inData = new uint[length2];
      for (; num4 > 0; --num4)
      {
        ulong num7 = ((ulong) buffer[index1] << 32) + (ulong) buffer[index1 - 1];
        ulong num8 = num7 / num5;
        ulong num9 = num7 % num5;
        bool flag = false;
        while (!flag)
        {
          flag = true;
          if (num8 == 4294967296UL || num8 * num6 > (num9 << 32) + (ulong) buffer[index1 - 2])
          {
            --num8;
            num9 += num5;
            if (num9 < 4294967296UL)
              flag = false;
          }
        }
        for (int index2 = 0; index2 < length2; ++index2)
          inData[index2] = buffer[index1 - index2];
        BigInteger bigInteger1 = new BigInteger(inData);
        BigInteger bigInteger2 = bi2 * (long) num8;
        while (bigInteger2 > bigInteger1)
        {
          --num8;
          bigInteger2 -= bi2;
        }
        BigInteger bigInteger3 = bigInteger1 - bigInteger2;
        for (int index3 = 0; index3 < length2; ++index3)
          buffer[index1 - index3] = bigInteger3._data[bi2._length - index3];
        numArray[num3++] = (uint) num8;
        --index1;
      }
      outQuotient._length = num3;
      int index4 = 0;
      int index5 = outQuotient._length - 1;
      while (index5 >= 0)
      {
        outQuotient._data[index4] = numArray[index5];
        --index5;
        ++index4;
      }
      for (; index4 < 70; ++index4)
        outQuotient._data[index4] = 0U;
      while (outQuotient._length > 1 && outQuotient._data[outQuotient._length - 1] == 0U)
        --outQuotient._length;
      if (outQuotient._length == 0)
        outQuotient._length = 1;
      outRemainder._length = BigInteger.shiftRight(buffer, shiftVal);
      int index6;
      for (index6 = 0; index6 < outRemainder._length; ++index6)
        outRemainder._data[index6] = buffer[index6];
      for (; index6 < 70; ++index6)
        outRemainder._data[index6] = 0U;
    }

    private static void singleByteDivide(
      BigInteger bi1,
      BigInteger bi2,
      BigInteger outQuotient,
      BigInteger outRemainder)
    {
      uint[] numArray = new uint[70];
      int num1 = 0;
      for (int index = 0; index < 70; ++index)
        outRemainder._data[index] = bi1._data[index];
      outRemainder._length = bi1._length;
      while (outRemainder._length > 1 && outRemainder._data[outRemainder._length - 1] == 0U)
        --outRemainder._length;
      ulong num2 = (ulong) bi2._data[0];
      int index1 = outRemainder._length - 1;
      ulong num3 = (ulong) outRemainder._data[index1];
      if (num3 >= num2)
      {
        ulong num4 = num3 / num2;
        numArray[num1++] = (uint) num4;
        outRemainder._data[index1] = (uint) (num3 % num2);
      }
      ulong num5;
      for (int index2 = index1 - 1; index2 >= 0; outRemainder._data[index2--] = (uint) (num5 % num2))
      {
        num5 = ((ulong) outRemainder._data[index2 + 1] << 32) + (ulong) outRemainder._data[index2];
        ulong num6 = num5 / num2;
        numArray[num1++] = (uint) num6;
        outRemainder._data[index2 + 1] = 0U;
      }
      outQuotient._length = num1;
      int index3 = 0;
      int index4 = outQuotient._length - 1;
      while (index4 >= 0)
      {
        outQuotient._data[index3] = numArray[index4];
        --index4;
        ++index3;
      }
      for (; index3 < 70; ++index3)
        outQuotient._data[index3] = 0U;
      while (outQuotient._length > 1 && outQuotient._data[outQuotient._length - 1] == 0U)
        --outQuotient._length;
      if (outQuotient._length == 0)
        outQuotient._length = 1;
      while (outRemainder._length > 1 && outRemainder._data[outRemainder._length - 1] == 0U)
        --outRemainder._length;
    }

    public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
    {
      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger();
      int index = 69;
      bool flag1 = false;
      bool flag2 = false;
      if (((int) bi1._data[index] & int.MinValue) != 0)
      {
        bi1 = -bi1;
        flag2 = true;
      }
      if (((int) bi2._data[index] & int.MinValue) != 0)
      {
        bi2 = -bi2;
        flag1 = true;
      }
      if (bi1 < bi2)
        return outQuotient;
      if (bi2._length == 1)
        BigInteger.singleByteDivide(bi1, bi2, outQuotient, outRemainder);
      else
        BigInteger.multiByteDivide(bi1, bi2, outQuotient, outRemainder);
      return flag2 != flag1 ? -outQuotient : outQuotient;
    }

    public static BigInteger operator /(BigInteger bi1, long bi2) => bi1 / (BigInteger) bi2;

    public static BigInteger operator /(BigInteger bi1, ulong bi2) => bi1 / (BigInteger) bi2;

    public static BigInteger operator /(BigInteger bi1, int bi2) => bi1 / (BigInteger) bi2;

    public static BigInteger operator /(BigInteger bi1, uint bi2) => bi1 / (BigInteger) bi2;

    public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
    {
      BigInteger outQuotient = new BigInteger();
      BigInteger outRemainder = new BigInteger(bi1);
      int index = 69;
      bool flag = false;
      if (((int) bi1._data[index] & int.MinValue) != 0)
      {
        bi1 = -bi1;
        flag = true;
      }
      if (((int) bi2._data[index] & int.MinValue) != 0)
        bi2 = -bi2;
      if (bi1 < bi2)
        return outRemainder;
      if (bi2._length == 1)
        BigInteger.singleByteDivide(bi1, bi2, outQuotient, outRemainder);
      else
        BigInteger.multiByteDivide(bi1, bi2, outQuotient, outRemainder);
      return flag ? -outRemainder : outRemainder;
    }

    public static BigInteger operator %(BigInteger bi1, long bi2) => bi1 % (BigInteger) bi2;

    public static BigInteger operator %(BigInteger bi1, ulong bi2) => bi1 % (BigInteger) bi2;

    public static BigInteger operator %(BigInteger bi1, int bi2) => bi1 % (BigInteger) bi2;

    public static BigInteger operator %(BigInteger bi1, uint bi2) => bi1 % (BigInteger) bi2;

    public static BigInteger operator %(long bi1, BigInteger bi2) => (BigInteger) bi1 % bi2;

    public static BigInteger operator %(ulong bi1, BigInteger bi2) => (BigInteger) bi1 % bi2;

    public static BigInteger operator %(int bi1, BigInteger bi2) => (BigInteger) bi1 % bi2;

    public static BigInteger operator %(uint bi1, BigInteger bi2) => (BigInteger) bi1 % bi2;

    public static BigInteger operator <<(BigInteger bi1, int shiftVal)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bigInteger._length = BigInteger.shiftLeft(bigInteger._data, shiftVal);
      return bigInteger;
    }

    private static int shiftLeft(uint[] buffer, int shiftVal)
    {
      int num1 = 32;
      int length = buffer.Length;
      while (length > 1 && buffer[length - 1] == 0U)
        --length;
      for (int index1 = shiftVal; index1 > 0; index1 -= num1)
      {
        if (index1 < num1)
          num1 = index1;
        ulong num2 = 0;
        for (int index2 = 0; index2 < length; ++index2)
        {
          ulong num3 = (ulong) buffer[index2] << num1 | num2;
          buffer[index2] = (uint) (num3 & (ulong) uint.MaxValue);
          num2 = num3 >> 32;
        }
        if (num2 != 0UL && length + 1 <= buffer.Length)
        {
          buffer[length] = (uint) num2;
          ++length;
        }
      }
      return length;
    }

    public static BigInteger operator >>(BigInteger bi1, int shiftVal)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      bigInteger._length = BigInteger.shiftRight(bigInteger._data, shiftVal);
      if (((int) bi1._data[69] & int.MinValue) != 0)
      {
        for (int index = 69; index >= bigInteger._length; --index)
          bigInteger._data[index] = uint.MaxValue;
        uint num = 2147483648;
        for (int index = 0; index < 32 && ((int) bigInteger._data[bigInteger._length - 1] & (int) num) == 0; ++index)
        {
          bigInteger._data[bigInteger._length - 1] |= num;
          num >>= 1;
        }
        bigInteger._length = 70;
      }
      return bigInteger;
    }

    private static int shiftRight(uint[] buffer, int shiftVal)
    {
      int num1 = 32;
      int num2 = 0;
      int length = buffer.Length;
      while (length > 1 && buffer[length - 1] == 0U)
        --length;
      for (int index1 = shiftVal; index1 > 0; index1 -= num1)
      {
        if (index1 < num1)
        {
          num1 = index1;
          num2 = 32 - num1;
        }
        ulong num3 = 0;
        for (int index2 = length - 1; index2 >= 0; --index2)
        {
          ulong num4 = (ulong) buffer[index2] >> num1 | num3;
          num3 = (ulong) buffer[index2] << num2;
          buffer[index2] = (uint) num4;
        }
      }
      while (length > 1 && buffer[length - 1] == 0U)
        --length;
      return length;
    }

    public static BigInteger operator ~(BigInteger bi1)
    {
      BigInteger bigInteger = new BigInteger(bi1);
      for (int index = 0; index < 70; ++index)
        bigInteger._data[index] = ~bi1._data[index];
      bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      return bigInteger;
    }

    public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1._length > bi2._length ? bi1._length : bi2._length;
      for (int index = 0; index < num1; ++index)
      {
        uint num2 = bi1._data[index] & bi2._data[index];
        bigInteger._data[index] = num2;
      }
      bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      return bigInteger;
    }

    public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1._length > bi2._length ? bi1._length : bi2._length;
      for (int index = 0; index < num1; ++index)
      {
        uint num2 = bi1._data[index] | bi2._data[index];
        bigInteger._data[index] = num2;
      }
      bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      return bigInteger;
    }

    public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
    {
      BigInteger bigInteger = new BigInteger();
      int num1 = bi1._length > bi2._length ? bi1._length : bi2._length;
      for (int index = 0; index < num1; ++index)
      {
        uint num2 = bi1._data[index] ^ bi2._data[index];
        bigInteger._data[index] = num2;
      }
      bigInteger._length = 70;
      while (bigInteger._length > 1 && bigInteger._data[bigInteger._length - 1] == 0U)
        --bigInteger._length;
      return bigInteger;
    }

    public static bool operator ==(BigInteger bi1, BigInteger bi2)
    {
      if ((object) bi1 == null && (object) bi2 == null)
        return true;
      return (object) bi1 != null && (object) bi2 != null && bi1.Equals((object) bi2);
    }

    public static bool operator ==(BigInteger bi1, uint bi2) => bi1 == (BigInteger) bi2;

    public static bool operator ==(BigInteger bi1, int bi2) => bi1 == (BigInteger) bi2;

    public static bool operator ==(BigInteger bi1, long bi2) => bi1 == (BigInteger) bi2;

    public static bool operator ==(BigInteger bi1, ulong bi2) => bi1 == (BigInteger) bi2;

    public static bool operator !=(BigInteger bi1, BigInteger bi2)
    {
      if ((object) bi1 == null && (object) bi2 == null)
        return false;
      return (object) bi1 == null || (object) bi2 == null || !bi1.Equals((object) bi2);
    }

    public static bool operator !=(BigInteger bi1, uint bi2) => bi1 != (BigInteger) bi2;

    public static bool operator !=(BigInteger bi1, int bi2) => bi1 != (BigInteger) bi2;

    public static bool operator !=(BigInteger bi1, long bi2) => bi1 != (BigInteger) bi2;

    public static bool operator !=(BigInteger bi1, ulong bi2) => bi1 != (BigInteger) bi2;

    public override bool Equals(object o)
    {
      BigInteger bigInteger = (BigInteger) o;
      if (this._length != bigInteger._length)
        return false;
      for (int index = 0; index < this._length; ++index)
      {
        if ((int) this._data[index] != (int) bigInteger._data[index])
          return false;
      }
      return true;
    }

    public static bool operator >(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      if (((int) bi1._data[index1] & int.MinValue) != 0 && ((int) bi2._data[index1] & int.MinValue) == 0)
        return false;
      if (((int) bi1._data[index1] & int.MinValue) == 0 && ((int) bi2._data[index1] & int.MinValue) != 0)
        return true;
      int index2 = (bi1._length > bi2._length ? bi1._length : bi2._length) - 1;
      while (index2 >= 0 && (int) bi1._data[index2] == (int) bi2._data[index2])
        --index2;
      return index2 >= 0 && bi1._data[index2] > bi2._data[index2];
    }

    public static bool operator >(BigInteger bi1, long bi2) => bi1 > (BigInteger) bi2;

    public static bool operator >(BigInteger bi1, ulong bi2) => bi1 > (BigInteger) bi2;

    public static bool operator >(BigInteger bi1, int bi2) => bi1 > (BigInteger) bi2;

    public static bool operator >(BigInteger bi1, uint bi2) => bi1 > (BigInteger) bi2;

    public static bool operator <(BigInteger bi1, BigInteger bi2)
    {
      int index1 = 69;
      if (((int) bi1._data[index1] & int.MinValue) != 0 && ((int) bi2._data[index1] & int.MinValue) == 0)
        return true;
      if (((int) bi1._data[index1] & int.MinValue) == 0 && ((int) bi2._data[index1] & int.MinValue) != 0)
        return false;
      int index2 = (bi1._length > bi2._length ? bi1._length : bi2._length) - 1;
      while (index2 >= 0 && (int) bi1._data[index2] == (int) bi2._data[index2])
        --index2;
      return index2 >= 0 && bi1._data[index2] < bi2._data[index2];
    }

    public static bool operator <(BigInteger bi1, long bi2) => bi1 < (BigInteger) bi2;

    public static bool operator <(BigInteger bi1, ulong bi2) => bi1 < (BigInteger) bi2;

    public static bool operator <(BigInteger bi1, int bi2) => bi1 < (BigInteger) bi2;

    public static bool operator <(BigInteger bi1, uint bi2) => bi1 < (BigInteger) bi2;

    public static bool operator >=(BigInteger bi1, BigInteger bi2) => bi1 == bi2 || bi1 > bi2;

    public static bool operator >=(BigInteger bi1, long bi2) => bi1 >= (BigInteger) bi2;

    public static bool operator >=(BigInteger bi1, ulong bi2) => bi1 >= (BigInteger) bi2;

    public static bool operator >=(BigInteger bi1, int bi2) => bi1 >= (BigInteger) bi2;

    public static bool operator >=(BigInteger bi1, uint bi2) => bi1 >= (BigInteger) bi2;

    public static bool operator <=(BigInteger bi1, BigInteger bi2) => bi1 == bi2 || bi1 < bi2;

    public static bool operator <=(BigInteger bi1, long bi2) => bi1 <= (BigInteger) bi2;

    public static bool operator <=(BigInteger bi1, ulong bi2) => bi1 <= (BigInteger) bi2;

    public static bool operator <=(BigInteger bi1, int bi2) => bi1 <= (BigInteger) bi2;

    public static bool operator <=(BigInteger bi1, uint bi2) => bi1 <= (BigInteger) bi2;

    public BigInteger Max(BigInteger bi) => this > bi ? new BigInteger(this) : new BigInteger(bi);

    public BigInteger Min(BigInteger bi) => this < bi ? new BigInteger(this) : new BigInteger(bi);

    public BigInteger Abs() => ((int) this._data[69] & int.MinValue) != 0 ? -this : new BigInteger(this);

    public bool FermatLittleTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      if (bigInteger1._length == 1)
      {
        if (bigInteger1._data[0] == 0U || bigInteger1._data[0] == 1U)
          return false;
        if (bigInteger1._data[0] == 2U || bigInteger1._data[0] == 3U)
          return true;
      }
      if (((int) bigInteger1._data[0] & 1) == 0)
        return false;
      int num = bigInteger1.BitCount();
      BigInteger bigInteger2 = new BigInteger();
      BigInteger exp = bigInteger1 - new BigInteger(1L);
      Random rand = new Random();
      for (int index = 0; index < confidence; ++index)
      {
        bool flag = false;
        while (!flag)
        {
          int bits = 0;
          while (bits < 2)
            bits = (int) (rand.NextDouble() * (double) num);
          bigInteger2.GenerateRandomBits(bits, rand);
          int length = bigInteger2._length;
          if (length > 1 || length == 1 && bigInteger2._data[0] != 1U)
            flag = true;
        }
        BigInteger bigInteger3 = bigInteger2.GCD(bigInteger1);
        if (bigInteger3._length == 1 && bigInteger3._data[0] != 1U)
          return false;
        BigInteger bigInteger4 = bigInteger2.ModPow(exp, bigInteger1);
        int length1 = bigInteger4._length;
        if (length1 > 1 || length1 == 1 && bigInteger4._data[0] != 1U)
          return false;
      }
      return true;
    }

    public bool RabinMillerTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      if (bigInteger1._length == 1)
      {
        if (bigInteger1._data[0] == 0U || bigInteger1._data[0] == 1U)
          return false;
        if (bigInteger1._data[0] == 2U || bigInteger1._data[0] == 3U)
          return true;
      }
      if (((int) bigInteger1._data[0] & 1) == 0)
        return false;
      BigInteger bigInteger2 = bigInteger1 - new BigInteger(1L);
      int num1 = 0;
      for (int index1 = 0; index1 < bigInteger2._length; ++index1)
      {
        uint num2 = 1;
        for (int index2 = 0; index2 < 32; ++index2)
        {
          if (((int) bigInteger2._data[index1] & (int) num2) != 0)
          {
            index1 = bigInteger2._length;
            break;
          }
          num2 <<= 1;
          ++num1;
        }
      }
      BigInteger exp = bigInteger2 >> num1;
      int num3 = bigInteger1.BitCount();
      BigInteger bigInteger3 = new BigInteger();
      Random rand = new Random();
      for (int index3 = 0; index3 < confidence; ++index3)
      {
        bool flag1 = false;
        while (!flag1)
        {
          int bits = 0;
          while (bits < 2)
            bits = (int) (rand.NextDouble() * (double) num3);
          bigInteger3.GenerateRandomBits(bits, rand);
          int length = bigInteger3._length;
          if (length > 1 || length == 1 && bigInteger3._data[0] != 1U)
            flag1 = true;
        }
        BigInteger bigInteger4 = bigInteger3.GCD(bigInteger1);
        if (bigInteger4._length == 1 && bigInteger4._data[0] != 1U)
          return false;
        BigInteger bigInteger5 = bigInteger3.ModPow(exp, bigInteger1);
        bool flag2 = false;
        if (bigInteger5._length == 1 && bigInteger5._data[0] == 1U)
          flag2 = true;
        for (int index4 = 0; !flag2 && index4 < num1; ++index4)
        {
          if (bigInteger5 == bigInteger2)
          {
            flag2 = true;
            break;
          }
          bigInteger5 = bigInteger5 * bigInteger5 % bigInteger1;
        }
        if (!flag2)
          return false;
      }
      return true;
    }

    public bool SolovayStrassenTest(int confidence)
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      if (bigInteger1._length == 1)
      {
        if (bigInteger1._data[0] == 0U || bigInteger1._data[0] == 1U)
          return false;
        if (bigInteger1._data[0] == 2U || bigInteger1._data[0] == 3U)
          return true;
      }
      if (((int) bigInteger1._data[0] & 1) == 0)
        return false;
      int num = bigInteger1.BitCount();
      BigInteger a = new BigInteger();
      BigInteger bigInteger2 = bigInteger1 - 1;
      BigInteger exp = bigInteger2 >> 1;
      Random rand = new Random();
      for (int index = 0; index < confidence; ++index)
      {
        bool flag = false;
        while (!flag)
        {
          int bits = 0;
          while (bits < 2)
            bits = (int) (rand.NextDouble() * (double) num);
          a.GenerateRandomBits(bits, rand);
          int length = a._length;
          if (length > 1 || length == 1 && a._data[0] != 1U)
            flag = true;
        }
        BigInteger bigInteger3 = a.GCD(bigInteger1);
        if (bigInteger3._length == 1 && bigInteger3._data[0] != 1U)
          return false;
        BigInteger bigInteger4 = a.ModPow(exp, bigInteger1);
        if (bigInteger4 == bigInteger2)
					bigInteger4 = new BigInteger(-1);
        BigInteger bigInteger5 = (BigInteger) BigInteger.Jacobi(a, bigInteger1);
        if (bigInteger4 != bigInteger5)
          return false;
      }
      return true;
    }

    public bool LucasStrongTest()
    {
      BigInteger thisVal = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      if (thisVal._length == 1)
      {
        if (thisVal._data[0] == 0U || thisVal._data[0] == 1U)
          return false;
        if (thisVal._data[0] == 2U || thisVal._data[0] == 3U)
          return true;
      }
      return ((int) thisVal._data[0] & 1) != 0 && BigInteger.LucasStrongTestHelper(thisVal);
    }

    public bool IsProbablePrime(int confidence)
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      for (int index = 0; index < BigInteger._primesBelow2000.Length; ++index)
      {
        BigInteger bigInteger2 = (BigInteger) BigInteger._primesBelow2000[index];
        if (!(bigInteger2 >= bigInteger1))
        {
          if ((bigInteger1 % bigInteger2).IntValue() == 0)
            return false;
        }
        else
          break;
      }
      return bigInteger1.RabinMillerTest(confidence);
    }

    public bool IsProbablePrime()
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      if (bigInteger1._length == 1)
      {
        if (bigInteger1._data[0] == 0U || bigInteger1._data[0] == 1U)
          return false;
        if (bigInteger1._data[0] == 2U || bigInteger1._data[0] == 3U)
          return true;
      }
      if (((int) bigInteger1._data[0] & 1) == 0)
        return false;
      for (int index = 0; index < BigInteger._primesBelow2000.Length; ++index)
      {
        BigInteger bigInteger2 = (BigInteger) BigInteger._primesBelow2000[index];
        if (!(bigInteger2 >= bigInteger1))
        {
          if ((bigInteger1 % bigInteger2).IntValue() == 0)
            return false;
        }
        else
          break;
      }
      BigInteger bigInteger3 = bigInteger1 - new BigInteger(1L);
      int num1 = 0;
      for (int index1 = 0; index1 < bigInteger3._length; ++index1)
      {
        uint num2 = 1;
        for (int index2 = 0; index2 < 32; ++index2)
        {
          if (((int) bigInteger3._data[index1] & (int) num2) != 0)
          {
            index1 = bigInteger3._length;
            break;
          }
          num2 <<= 1;
          ++num1;
        }
      }
      BigInteger bigInteger4 = ((BigInteger) 2).ModPow(bigInteger3 >> num1, bigInteger1);
      bool flag = false;
      if (bigInteger4._length == 1 && bigInteger4._data[0] == 1U)
        flag = true;
      for (int index = 0; !flag && index < num1; ++index)
      {
        if (bigInteger4 == bigInteger3)
        {
          flag = true;
          break;
        }
        bigInteger4 = bigInteger4 * bigInteger4 % bigInteger1;
      }
      if (flag)
        flag = BigInteger.LucasStrongTestHelper(bigInteger1);
      return flag;
    }

    public static BigInteger genPseudoPrime(int bits, int confidence, Random rand)
    {
      BigInteger bigInteger = new BigInteger();
      for (bool flag = false; !flag; flag = bigInteger.IsProbablePrime(confidence))
      {
        bigInteger.GenerateRandomBits(bits, rand);
        bigInteger._data[0] |= 1U;
      }
      return bigInteger;
    }

    public BigInteger genCoPrime(int bits, Random rand)
    {
      bool flag = false;
      BigInteger bigInteger1 = new BigInteger();
      while (!flag)
      {
        bigInteger1.GenerateRandomBits(bits, rand);
        BigInteger bigInteger2 = bigInteger1.GCD(this);
        if (bigInteger2._length == 1 && bigInteger2._data[0] == 1U)
          flag = true;
      }
      return bigInteger1;
    }

    public void SetBit(uint bitNum)
    {
      uint index = bitNum >> 5;
      uint num = 1U << (int) (byte) (bitNum & 31U);
      this._data[(int) index] |= num;
      if ((long) index < (long) this._length)
        return;
      this._length = (int) index + 1;
    }

    public void UnsetBit(uint bitNum)
    {
      uint index = bitNum >> 5;
      if ((long) index >= (long) this._length)
        return;
      uint num = uint.MaxValue ^ 1U << (int) (byte) (bitNum & 31U);
      this._data[(int) index] &= num;
      if (this._length <= 1 || this._data[this._length - 1] != 0U)
        return;
      --this._length;
    }

    public BigInteger GCD(BigInteger bi)
    {
      BigInteger bigInteger1 = ((int) this._data[69] & int.MinValue) == 0 ? this : -this;
      BigInteger bigInteger2 = ((int) bi._data[69] & int.MinValue) == 0 ? bi : -bi;
      BigInteger bigInteger3 = bigInteger2;
      while (bigInteger1._length > 1 || bigInteger1._length == 1 && bigInteger1._data[0] != 0U)
      {
        bigInteger3 = bigInteger1;
        bigInteger1 = bigInteger2 % bigInteger1;
        bigInteger2 = bigInteger3;
      }
      return bigInteger3;
    }

    public void GenerateRandomBits(int bits, Random rand)
    {
      int num1 = bits >> 5;
      int num2 = bits & 31;
      if (num2 != 0)
        ++num1;
      if (num1 > 70)
        throw new ArithmeticException("Number of required bits > maxLength.");
      for (int index = 0; index < num1; ++index)
        this._data[index] = (uint) (rand.NextDouble() * 4294967296.0);
      for (int index = num1; index < 70; ++index)
        this._data[index] = 0U;
      if (num2 != 0)
      {
        uint num3 = (uint) (1 << num2 - 1);
        this._data[num1 - 1] |= num3;
        uint num4 = uint.MaxValue >> 32 - num2;
        this._data[num1 - 1] &= num4;
      }
      else
        this._data[num1 - 1] |= 2147483648U;
      this._length = num1;
      if (this._length != 0)
        return;
      this._length = 1;
    }

    public int BitCount()
    {
      while (this._length > 1 && this._data[this._length - 1] == 0U)
        --this._length;
      uint num1 = this._data[this._length - 1];
      uint num2 = 2147483648;
      int num3;
      for (num3 = 32; num3 > 0 && ((int) num1 & (int) num2) == 0; num2 >>= 1)
        --num3;
      return num3 + (this._length - 1 << 5);
    }

    public byte LeastSignificantByte() => this.ByteValue();

    public byte ByteValue() => (byte) this._data[0];

    public int IntValue() => (int) this._data[0];

    public long LongValue()
    {
      long num = (long) this._data[0];
      try
      {
        num |= (long) this._data[1] << 32;
      }
      catch (Exception ex)
      {
        if (((int) this._data[0] & int.MinValue) != 0)
          num = (long) (int) this._data[0];
      }
      return num;
    }

    public BigInteger ModPow(BigInteger exp, BigInteger n)
    {
      if (((int) exp._data[69] & int.MinValue) != 0)
        throw new ArithmeticException("Positive exponents only.");
      BigInteger bigInteger1 = (BigInteger) 1;
      bool flag = false;
      BigInteger bigInteger2;
      if (((int) this._data[69] & int.MinValue) != 0)
      {
        bigInteger2 = -this % n;
        flag = true;
      }
      else
        bigInteger2 = this % n;
      if (((int) n._data[69] & int.MinValue) != 0)
        n = -n;
      BigInteger bigInteger3 = new BigInteger();
      int index1 = n._length << 1;
      bigInteger3._data[index1] = 1U;
      bigInteger3._length = index1 + 1;
      BigInteger constant = bigInteger3 / n;
      int num1 = exp.BitCount();
      int num2 = 0;
      for (int index2 = 0; index2 < exp._length; ++index2)
      {
        uint num3 = 1;
        for (int index3 = 0; index3 < 32; ++index3)
        {
          if (((int) exp._data[index2] & (int) num3) != 0)
            bigInteger1 = BigInteger.BarrettReduction(bigInteger1 * bigInteger2, n, constant);
          num3 <<= 1;
          bigInteger2 = BigInteger.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
          if (bigInteger2._length == 1 && bigInteger2._data[0] == 1U)
            return flag && ((int) exp._data[0] & 1) != 0 ? -bigInteger1 : bigInteger1;
          ++num2;
          if (num2 == num1)
            break;
        }
      }
      return flag && ((int) exp._data[0] & 1) != 0 ? -bigInteger1 : bigInteger1;
    }

    public BigInteger ModInverse(BigInteger modulus)
    {
      BigInteger[] bigIntegerArray1 = new BigInteger[2]
      {
        (BigInteger) 0,
        (BigInteger) 1
      };
      BigInteger[] bigIntegerArray2 = new BigInteger[2];
      BigInteger[] bigIntegerArray3 = new BigInteger[2]
      {
        (BigInteger) 0,
        (BigInteger) 0
      };
      int num = 0;
      BigInteger bi1 = modulus;
      BigInteger bi2 = this;
      while (bi2._length > 1 || bi2._length == 1 && bi2._data[0] != 0U)
      {
        BigInteger outQuotient = new BigInteger();
        BigInteger outRemainder = new BigInteger();
        if (num > 1)
        {
          BigInteger bigInteger = (bigIntegerArray1[0] - bigIntegerArray1[1] * bigIntegerArray2[0]) % modulus;
          bigIntegerArray1[0] = bigIntegerArray1[1];
          bigIntegerArray1[1] = bigInteger;
        }
        if (bi2._length == 1)
          BigInteger.singleByteDivide(bi1, bi2, outQuotient, outRemainder);
        else
          BigInteger.multiByteDivide(bi1, bi2, outQuotient, outRemainder);
        bigIntegerArray2[0] = bigIntegerArray2[1];
        bigIntegerArray3[0] = bigIntegerArray3[1];
        bigIntegerArray2[1] = outQuotient;
        bigIntegerArray3[1] = outRemainder;
        bi1 = bi2;
        bi2 = outRemainder;
        ++num;
      }
      if (bigIntegerArray3[0]._length > 1 || bigIntegerArray3[0]._length == 1 && bigIntegerArray3[0]._data[0] != 1U)
        throw new ArithmeticException("No inverse!");
      BigInteger bigInteger1 = (bigIntegerArray1[0] - bigIntegerArray1[1] * bigIntegerArray2[0]) % modulus;
      if (((int) bigInteger1._data[69] & int.MinValue) != 0)
        bigInteger1 += modulus;
      return bigInteger1;
    }

    public BigInteger Sqrt()
    {
      uint num1 = (uint) this.BitCount();
      uint num2 = ((int) num1 & 1) == 0 ? num1 >> 1 : (num1 >> 1) + 1U;
      uint num3 = num2 >> 5;
      byte num4 = (byte) (num2 & 31U);
      BigInteger bigInteger = new BigInteger();
      uint num5;
      if (num4 == (byte) 0)
      {
        num5 = 2147483648U;
      }
      else
      {
        num5 = 1U << (int) num4;
        ++num3;
      }
      bigInteger._length = (int) num3;
      for (int index = (int) num3 - 1; index >= 0; --index)
      {
        for (; num5 != 0U; num5 >>= 1)
        {
          bigInteger._data[index] ^= num5;
          if (bigInteger * bigInteger > this)
            bigInteger._data[index] ^= num5;
        }
        num5 = 2147483648U;
      }
      return bigInteger;
    }
  }
}
