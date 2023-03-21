using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Scripting.Math
{
    [Serializable]
    public sealed class BigInteger : IFormattable, IComparable, IEquatable<Microsoft.Scripting.Math.BigInteger>
    {
        internal readonly System.Numerics.BigInteger Value;

        public static readonly Microsoft.Scripting.Math.BigInteger Zero = new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)0);

        public static readonly Microsoft.Scripting.Math.BigInteger One = new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)1);

        public int Sign => Value.Sign;

        public bool IsEven => Value.IsEven;

        public bool IsPowerOfTwo => Value.IsPowerOfTwo;

        public BigInteger(System.Numerics.BigInteger value)
        {
            Value = value;
        }

        [CLSCompliant(false)]
        public static Microsoft.Scripting.Math.BigInteger Create(ulong v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        [CLSCompliant(false)]
        public static Microsoft.Scripting.Math.BigInteger Create(uint v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        public static Microsoft.Scripting.Math.BigInteger Create(long v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        public static Microsoft.Scripting.Math.BigInteger Create(int v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        public static Microsoft.Scripting.Math.BigInteger Create(decimal v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        public static Microsoft.Scripting.Math.BigInteger Create(byte[] v)
        {
            return new Microsoft.Scripting.Math.BigInteger(v);
        }

        public static Microsoft.Scripting.Math.BigInteger Create(double v)
        {
            return new Microsoft.Scripting.Math.BigInteger(new System.Numerics.BigInteger(v));
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(byte i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        [CLSCompliant(false)]
        public static implicit operator Microsoft.Scripting.Math.BigInteger(sbyte i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(short i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        [CLSCompliant(false)]
        public static implicit operator Microsoft.Scripting.Math.BigInteger(ushort i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        [CLSCompliant(false)]
        public static implicit operator Microsoft.Scripting.Math.BigInteger(uint i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(int i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        [CLSCompliant(false)]
        public static implicit operator Microsoft.Scripting.Math.BigInteger(ulong i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(long i)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)i);
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(decimal self)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)self);
        }

        public static explicit operator Microsoft.Scripting.Math.BigInteger(double self)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)self);
        }

        public static explicit operator Microsoft.Scripting.Math.BigInteger(float self)
        {
            return new Microsoft.Scripting.Math.BigInteger((System.Numerics.BigInteger)self);
        }

        public static explicit operator double(Microsoft.Scripting.Math.BigInteger self)
        {
            return (double)self.Value;
        }

        public static explicit operator float(Microsoft.Scripting.Math.BigInteger self)
        {
            return (float)self.Value;
        }

        public static explicit operator decimal(Microsoft.Scripting.Math.BigInteger self)
        {
            return (decimal)self.Value;
        }

        public static explicit operator byte(Microsoft.Scripting.Math.BigInteger self)
        {
            return (byte)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(Microsoft.Scripting.Math.BigInteger self)
        {
            return (sbyte)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator ushort(Microsoft.Scripting.Math.BigInteger self)
        {
            return (ushort)self.Value;
        }

        public static explicit operator short(Microsoft.Scripting.Math.BigInteger self)
        {
            return (short)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator uint(Microsoft.Scripting.Math.BigInteger self)
        {
            return (uint)self.Value;
        }

        public static explicit operator int(Microsoft.Scripting.Math.BigInteger self)
        {
            return (int)self.Value;
        }

        public static explicit operator long(Microsoft.Scripting.Math.BigInteger self)
        {
            return (long)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(Microsoft.Scripting.Math.BigInteger self)
        {
            return (ulong)self.Value;
        }

        public static implicit operator Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger value)
        {
            return new Microsoft.Scripting.Math.BigInteger(value);
        }

        public static implicit operator System.Numerics.BigInteger(Microsoft.Scripting.Math.BigInteger value)
        {
            return value.Value;
        }

        public BigInteger(Microsoft.Scripting.Math.BigInteger copy)
        {
            if (object.ReferenceEquals(copy, null))
            {
                throw new ArgumentNullException("copy");
            }
            Value = copy.Value;
        }

        public BigInteger(byte[] data)
        {
            ContractUtils.RequiresNotNull(data, "data");
            Value = new System.Numerics.BigInteger(data);
        }

        public BigInteger(int sign, byte[] data)
        {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= 1, "sign");
            Value = new System.Numerics.BigInteger(data);
            if (sign < 0)
            {
                Value = -Value;
            }
        }

        [CLSCompliant(false)]
        public BigInteger(int sign, uint[] data)
        {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= 1, "sign");
            int length = GetLength(data);
            ContractUtils.Requires(length == 0 || sign != 0, "sign");
            if (length == 0)
            {
                Value = 0;
                return;
            }
            bool flag = (data[length - 1] & 0x80000000u) != 0;
            byte[] array = new byte[length * 4 + (flag ? 1 : 0)];
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                ulong num2 = data[i];
                array[num++] = (byte)(num2 & 0xFF);
                array[num++] = (byte)((num2 >> 8) & 0xFF);
                array[num++] = (byte)((num2 >> 16) & 0xFF);
                array[num++] = (byte)((num2 >> 24) & 0xFF);
            }
            Value = new System.Numerics.BigInteger(array);
            if (sign < 0)
            {
                Value = -Value;
            }
        }

        [CLSCompliant(false)]
        public uint[] GetWords()
        {
            return Value.GetWords();
        }

        public int GetBitCount()
        {
            return Value.GetBitCount();
        }

        public int GetWordCount()
        {
            return Value.GetWordCount();
        }

        public int GetByteCount()
        {
            return Value.GetByteCount();
        }

        public bool AsInt64(out long ret)
        {
            if (Value >= long.MinValue && Value <= long.MaxValue)
            {
                ret = (long)Value;
                return true;
            }
            ret = 0L;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt32(out uint ret)
        {
            if (Value >= 0L && Value <= 4294967295L)
            {
                ret = (uint)Value;
                return true;
            }
            ret = 0u;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt64(out ulong ret)
        {
            if (Value >= 0uL && Value <= ulong.MaxValue)
            {
                ret = (ulong)Value;
                return true;
            }
            ret = 0uL;
            return false;
        }

        public bool AsInt32(out int ret)
        {
            if (Value >= -2147483648L && Value <= 2147483647L)
            {
                ret = (int)Value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public uint ToUInt32()
        {
            return (uint)Value;
        }

        public int ToInt32()
        {
            return (int)Value;
        }

        public decimal ToDecimal()
        {
            return (decimal)Value;
        }

        [CLSCompliant(false)]
        public ulong ToUInt64()
        {
            return (ulong)Value;
        }

        public long ToInt64()
        {
            return (long)Value;
        }

        private static int GetLength(uint[] data)
        {
            int num = data.Length - 1;
            while (num >= 0 && data[num] == 0)
            {
                num--;
            }
            return num + 1;
        }

        public static int Compare(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return System.Numerics.BigInteger.Compare(x.Value, y.Value);
        }

        public static bool operator ==(Microsoft.Scripting.Math.BigInteger x, int y)
        {
            return x.Value == y;
        }

        public static bool operator !=(Microsoft.Scripting.Math.BigInteger x, int y)
        {
            return x.Value != y;
        }

        public static bool operator ==(Microsoft.Scripting.Math.BigInteger x, double y)
        {
            if (object.ReferenceEquals(x, null))
            {
                throw new ArgumentNullException("x");
            }
            if (y % 1.0 != 0.0)
            {
                return false;
            }
            return x.Value == (System.Numerics.BigInteger)y;
        }

        public static bool operator ==(double x, Microsoft.Scripting.Math.BigInteger y)
        {
            return y == x;
        }

        public static bool operator !=(Microsoft.Scripting.Math.BigInteger x, double y)
        {
            return !(x == y);
        }

        public static bool operator !=(double x, Microsoft.Scripting.Math.BigInteger y)
        {
            return !(x == y);
        }

        public static bool operator ==(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) == 0;
        }

        public static bool operator !=(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) != 0;
        }

        public static bool operator <(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) < 0;
        }

        public static bool operator <=(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) <= 0;
        }

        public static bool operator >(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) > 0;
        }

        public static bool operator >=(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return Compare(x, y) >= 0;
        }

        public static Microsoft.Scripting.Math.BigInteger Add(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x + y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator +(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value + y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger Subtract(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x - y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator -(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value - y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger Multiply(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x * y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator *(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value * y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger Divide(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x / y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator /(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            Microsoft.Scripting.Math.BigInteger remainder;
            return DivRem(x, y, out remainder);
        }

        public static Microsoft.Scripting.Math.BigInteger Mod(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x % y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator %(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            DivRem(x, y, out var remainder);
            return remainder;
        }

        public static Microsoft.Scripting.Math.BigInteger DivRem(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y, out Microsoft.Scripting.Math.BigInteger remainder)
        {
            System.Numerics.BigInteger remainder2;
            System.Numerics.BigInteger value = System.Numerics.BigInteger.DivRem(x.Value, y.Value, out remainder2);
            remainder = new Microsoft.Scripting.Math.BigInteger(remainder2);
            return new Microsoft.Scripting.Math.BigInteger(value);
        }

        public static Microsoft.Scripting.Math.BigInteger BitwiseAnd(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x & y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator &(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value & y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger BitwiseOr(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x | y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator |(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value | y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger Xor(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return x ^ y;
        }

        public static Microsoft.Scripting.Math.BigInteger operator ^(Microsoft.Scripting.Math.BigInteger x, Microsoft.Scripting.Math.BigInteger y)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value ^ y.Value);
        }

        public static Microsoft.Scripting.Math.BigInteger LeftShift(Microsoft.Scripting.Math.BigInteger x, int shift)
        {
            return x << shift;
        }

        public static Microsoft.Scripting.Math.BigInteger operator <<(Microsoft.Scripting.Math.BigInteger x, int shift)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value << shift);
        }

        public static Microsoft.Scripting.Math.BigInteger RightShift(Microsoft.Scripting.Math.BigInteger x, int shift)
        {
            return x >> shift;
        }

        public static Microsoft.Scripting.Math.BigInteger operator >>(Microsoft.Scripting.Math.BigInteger x, int shift)
        {
            return new Microsoft.Scripting.Math.BigInteger(x.Value >> shift);
        }

        public static Microsoft.Scripting.Math.BigInteger Negate(Microsoft.Scripting.Math.BigInteger x)
        {
            return -x;
        }

        public static Microsoft.Scripting.Math.BigInteger operator -(Microsoft.Scripting.Math.BigInteger x)
        {
            return new Microsoft.Scripting.Math.BigInteger(-x.Value);
        }

        public Microsoft.Scripting.Math.BigInteger OnesComplement()
        {
            return ~this;
        }

        public static Microsoft.Scripting.Math.BigInteger operator ~(Microsoft.Scripting.Math.BigInteger x)
        {
            return new Microsoft.Scripting.Math.BigInteger(~x.Value);
        }

        public Microsoft.Scripting.Math.BigInteger Abs()
        {
            return new Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger.Abs(Value));
        }

        public Microsoft.Scripting.Math.BigInteger Power(int exp)
        {
            return new Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger.Pow(Value, exp));
        }

        public Microsoft.Scripting.Math.BigInteger ModPow(int power, Microsoft.Scripting.Math.BigInteger mod)
        {
            return new Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger.ModPow(Value, power, mod.Value));
        }

        public Microsoft.Scripting.Math.BigInteger ModPow(Microsoft.Scripting.Math.BigInteger power, Microsoft.Scripting.Math.BigInteger mod)
        {
            return new Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger.ModPow(Value, power.Value, mod.Value));
        }

        public Microsoft.Scripting.Math.BigInteger Square()
        {
            return this * this;
        }

        public static Microsoft.Scripting.Math.BigInteger Parse(string str)
        {
            return new Microsoft.Scripting.Math.BigInteger(System.Numerics.BigInteger.Parse(str));
        }

        public override string ToString()
        {
            return ToString(10);
        }

        public string ToString(int @base)
        {
            return Microsoft.Scripting.Math.Extensions.MathUtils.BigIntegerToString(GetWords(), Sign, @base, false);
        }

        public override int GetHashCode()
        {
            System.Numerics.BigInteger value = Value;
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Microsoft.Scripting.Math.BigInteger);
        }

        public bool Equals(Microsoft.Scripting.Math.BigInteger other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this == other;
        }

        public bool IsNegative()
        {
            return Value.Sign < 0;
        }

        public bool IsZero()
        {
            return Value.Sign == 0;
        }

        public bool IsPositive()
        {
            return Value.Sign > 0;
        }

        public double Log(double newBase)
        {
            return System.Numerics.BigInteger.Log(Value, newBase);
        }

        public double Log()
        {
            return System.Numerics.BigInteger.Log(Value);
        }

        public double Log10()
        {
            return System.Numerics.BigInteger.Log10(Value);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Microsoft.Scripting.Math.BigInteger bigInteger = obj as Microsoft.Scripting.Math.BigInteger;
            if (object.ReferenceEquals(bigInteger, null))
            {
                throw new ArgumentException("expected integer");
            }
            return Compare(this, bigInteger);
        }

        public byte[] ToByteArray()
        {
            return Value.ToByteArray();
        }

        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }
    }

}
