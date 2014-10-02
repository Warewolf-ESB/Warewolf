
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
using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct TwoOctetUnion
    {
        #region Constants
        public const int SizeInBytes = 2;
        #endregion

        #region Readonly Fields
        public static readonly TwoOctetUnion Empty = new TwoOctetUnion();
        #endregion

        #region Instance Fields
        [FieldOffset(0)]
        public byte O1;
        [FieldOffset(1)]
        public byte O2;
        [FieldOffset(0)]
        public short Int16;
        [FieldOffset(0)]
        public ushort UInt16;
        [FieldOffset(0)]
        public char Char;
        #endregion

        #region Constructors
        public TwoOctetUnion(byte[] array, int index)
        {
            Char = Char.MinValue;
            Int16 = 0;
            UInt16 = 0;
            O1 = array[index];
            O2 = array[index + 1];
        }

        public TwoOctetUnion(byte o1, byte o2)
        {
            Char = Char.MinValue;
            Int16 = 0;
            UInt16 = 0;
            O1 = o1;
            O2 = o2;
        }

        public TwoOctetUnion(short value)
        {
            Char = Char.MinValue;
            UInt16 = 0;
            O1 = Byte.MinValue;
            O2 = Byte.MinValue;
            Int16 = value;
        }

        public TwoOctetUnion(ushort value)
        {
            Char = Char.MinValue;
            Int16 = 0;
            O1 = Byte.MinValue;
            O2 = Byte.MinValue;
            UInt16 = value;
        }

        public TwoOctetUnion(char value)
        {
            Int16 = 0;
            UInt16 = 0;
            O1 = Byte.MinValue;
            O2 = Byte.MinValue;
            Char = value;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Int16.ToString();
        }

        public override int GetHashCode()
        {
            return Int16;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TwoOctetUnion)) return false;
            return ((TwoOctetUnion)obj).Int16 == Int16;
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(TwoOctetUnion l, TwoOctetUnion r)
        {
            return l.Int16 == r.Int16;
        }

        public static bool operator !=(TwoOctetUnion l, TwoOctetUnion r)
        {
            return l.Int16 != r.Int16;
        }
        #endregion
    }

    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct FourOctetUnion
    {
        #region Constants
        public const int SizeInBytes = 4;
        #endregion

        #region Readonly Fields
        public static readonly FourOctetUnion Empty = new FourOctetUnion();
        #endregion

        #region Instance Fields
        [FieldOffset(0)]
        public byte O1;
        [FieldOffset(1)]
        public byte O2;
        [FieldOffset(2)]
        public byte O3;
        [FieldOffset(3)]
        public byte O4;

        [FieldOffset(0)]
        public TwoOctetUnion Half1;
        [FieldOffset(2)]
        public TwoOctetUnion Half2;

        [FieldOffset(0)]
        public int Int32;
        [FieldOffset(0)]
        public uint UInt32;
        [FieldOffset(0)]
        public float Single;
        #endregion

        #region Public Properties
        public bool this[int index] { get { return (UInt32 & (1u << index)) != 0u; } set { UInt32 = (value ? (UInt32 | (1u << index)) : (UInt32 & ~(1u << index))); } }
        #endregion

        #region Constructors
        public FourOctetUnion(byte[] array, int index)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            Half1 = Half2 = TwoOctetUnion.Empty;
            O1 = array[index];
            O2 = array[index + 1];
            O3 = array[index + 2];
            O4 = array[index + 3];
        }

        public FourOctetUnion(char o1, char o2, char o3, char o4)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            Half1 = Half2 = TwoOctetUnion.Empty;
            O1 = (byte)o1;
            O2 = (byte)o2;
            O3 = (byte)o3;
            O4 = (byte)o4;
        }

        public FourOctetUnion(byte o1, byte o2, byte o3, byte o4)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            Half1 = Half2 = TwoOctetUnion.Empty;
            O1 = o1;
            O2 = o2;
            O3 = o3;
            O4 = o4;
        }

        public FourOctetUnion(short half1, short half2)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            Half1 = new TwoOctetUnion(half1);
            Half2 = new TwoOctetUnion(half2);
        }

        public FourOctetUnion(TwoOctetUnion half1, TwoOctetUnion half2)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            Half1 = half1;
            Half2 = half2;
        }

        public FourOctetUnion(int value)
        {
            UInt32 = 0u;
            Single = 0.0F;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            Half1 = Half2 = TwoOctetUnion.Empty;
            Int32 = value;
        }

        public FourOctetUnion(uint value)
        {
            Int32 = 0;
            Single = 0.0F;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            Half1 = Half2 = TwoOctetUnion.Empty;
            UInt32 = value;
        }

        public FourOctetUnion(float value)
        {
            Int32 = 0;
            UInt32 = 0u;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            Half1 = Half2 = TwoOctetUnion.Empty;
            Single = value;
        }

        public FourOctetUnion(string word)
        {
            Int32 = 0;
            UInt32 = 0u;
            Single = 0.0F;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            int length = word == null ? 0 : word.Length;

            if (length > 1)
            {
                Half1 = new TwoOctetUnion(word[0]);
                Half2 = new TwoOctetUnion(word[1]);
            }
            else
            {
                if (length > 0) Half1 = new TwoOctetUnion(word[0]);
                else Half1 = TwoOctetUnion.Empty;
                Half2 = TwoOctetUnion.Empty;
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Int32.ToString();
        }

        public override int GetHashCode()
        {
            return Int32;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FourOctetUnion)) return false;
            return ((FourOctetUnion)obj).Int32 == Int32;
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(FourOctetUnion l, FourOctetUnion r)
        {
            return l.Int32 == r.Int32;
        }

        public static bool operator !=(FourOctetUnion l, FourOctetUnion r)
        {
            return l.Int32 != r.Int32;
        }
        #endregion
    }

    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct EightOctetUnion
    {
        #region Constants
        public const int SizeInBytes = 8;
        #endregion

        #region Readonly Fields
        public static readonly EightOctetUnion Empty = new EightOctetUnion();
        #endregion

        #region Static Members
        private static long[] _setMasks = new long[]
        {
            -256L,
            -65281L,
            -16711681L,
            -4278190081L,
            -1095216660481L,
            -280375465082881L,
            -71776119061217281L,
            72057594037927935L,
        };

        private static long[] _getMasks = new long[]
        {
            255L,
            65280L,
            16711680L,
            4278190080L,
            1095216660480L,
            280375465082880L,
            71776119061217280L,
            -72057594037927936L,
        };
        #endregion

        #region Instance Fields
        [FieldOffset(0)]
        public byte O1;
        [FieldOffset(1)]
        public byte O2;
        [FieldOffset(2)]
        public byte O3;
        [FieldOffset(3)]
        public byte O4;
        [FieldOffset(4)]
        public byte O5;
        [FieldOffset(5)]
        public byte O6;
        [FieldOffset(6)]
        public byte O7;
        [FieldOffset(7)]
        public byte O8;

        [FieldOffset(0)]
        public FourOctetUnion Half1;
        [FieldOffset(4)]
        public FourOctetUnion Half2;

        [FieldOffset(0)]
        public long Int64;
        [FieldOffset(0)]
        public ulong UInt64;
        [FieldOffset(0)]
        public double Double;
        #endregion

        #region Public Properties
        public bool this[int index] { get { return (Int64 & (1L << index)) != 0L; } set { Int64 = (value ? Int64 | (1L << index) : Int64 & ~(1L << index)); } }
        //public byte this[short index] { get { return (byte)((Int64 & _getMasks[index]) >> (index << 3)); } set { long add = (long)value << (index << 3); Int64 = (Int64 & _setMasks[index]) | add; } }
        #endregion

        #region Constructors
        public EightOctetUnion(byte[] array, int index)
        {
            Int64 = 0L;
            UInt64 = 0ul;
            Double = 0.0;
            Half1 = Half2 = FourOctetUnion.Empty;
            O1 = array[index];
            O2 = array[index + 1];
            O3 = array[index + 2];
            O4 = array[index + 3];
            O5 = array[index + 4];
            O6 = array[index + 5];
            O7 = array[index + 6];
            O8 = array[index + 7];
        }

        public EightOctetUnion(byte o1, byte o2, byte o3, byte o4, byte o5, byte o6, byte o7, byte o8)
        {
            Int64 = 0L;
            UInt64 = 0ul;
            Double = 0.0;
            Half1 = Half2 = FourOctetUnion.Empty;
            O1 = o1;
            O2 = o2;
            O3 = o3;
            O4 = o4;
            O5 = o5;
            O6 = o6;
            O7 = o7;
            O8 = o8;
        }

        public EightOctetUnion(FourOctetUnion half1, FourOctetUnion half2)
        {
            Int64 = 0L;
            UInt64 = 0ul;
            Double = 0.0;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            O5 = O6 = O7 = O8 = Byte.MinValue;
            Half1 = half1;
            Half2 = half2;
        }

        public EightOctetUnion(long value)
        {
            UInt64 = 0ul;
            Double = 0.0;
            Half1 = Half2 = FourOctetUnion.Empty;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            O5 = O6 = O7 = O8 = Byte.MinValue;
            Int64 = value;
        }

        public EightOctetUnion(ulong value)
        {
            Int64 = 0L;
            Double = 0.0;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            O5 = O6 = O7 = O8 = Byte.MinValue;
            Half1 = Half2 = FourOctetUnion.Empty;
            UInt64 = value;
        }

        public EightOctetUnion(double value)
        {
            Int64 = 0L;
            UInt64 = 0ul;
            O1 = O2 = O3 = O4 = Byte.MinValue;
            O5 = O6 = O7 = O8 = Byte.MinValue;
            Half1 = Half2 = FourOctetUnion.Empty;
            Double = value;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Int64.ToString();
        }

        public override int GetHashCode()
        {
            return Int64.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is EightOctetUnion)) return false;
            return ((EightOctetUnion)obj).Int64 == Int64;
        }
        #endregion

        #region [Get/Set] Handling
        public bool GetBit(int index)
        {
            return (Int64 & (1L << index)) != 0L;
        }

        public byte GetByte(int index)
        {
            return (byte)((Int64 & _getMasks[index]) >> (index << 3));
        }

        public TwoOctetUnion GetQuarter(int quarter)
        {
            if (quarter < 2)
            {
                if (quarter == 0) return Half1.Half1;
                else return Half1.Half2;
            }
            else
            {
                if (quarter == 2) return Half2.Half1;
                else return Half2.Half2;
            }
        }

        public void SetBit(int index, bool value)
        {
            Int64 = (value ? Int64 | (1L << index) : Int64 & ~(1L << index));
        }

        public void SetByte(int index, byte value)
        {
            long add = (long)value << (index << 3); Int64 = (Int64 & _setMasks[index]) | add;
        }

        public void SetQuarter(int quarter, TwoOctetUnion value)
        {
            if (quarter < 2)
            {
                if (quarter == 0) Half1.Half1 = value;
                else Half1.Half2 = value;
            }
            else
            {
                if (quarter == 2) Half2.Half1 = value;
                else Half2.Half2 = value;
            }
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(EightOctetUnion l, EightOctetUnion r)
        {
            return l.Int64 == r.Int64;
        }

        public static bool operator !=(EightOctetUnion l, EightOctetUnion r)
        {
            return l.Int64 != r.Int64;
        }
        #endregion
    }
}
