// Decompiled with JetBrains decompiler
// Type: System.EightOctetUnion
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.InteropServices;

namespace System
{
  [StructLayout(LayoutKind.Explicit, Size = 8)]
  public struct EightOctetUnion
  {
    public const int SizeInBytes = 8;
    public static readonly EightOctetUnion Empty = new EightOctetUnion();
    private static long[] _setMasks = new long[8]
    {
      -256L,
      -65281L,
      -16711681L,
      -4278190081L,
      -1095216660481L,
      -280375465082881L,
      -71776119061217281L,
      72057594037927935L
    };
    private static long[] _getMasks = new long[8]
    {
      (long) byte.MaxValue,
      65280L,
      16711680L,
      4278190080L,
      1095216660480L,
      280375465082880L,
      71776119061217280L,
      -72057594037927936L
    };
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

    public bool this[int index]
    {
      get => (this.Int64 & 1L << index) != 0L;
      set => this.Int64 = value ? this.Int64 | 1L << index : this.Int64 & ~(1L << index);
    }

    public EightOctetUnion(byte[] array, int index)
    {
      this.Int64 = 0L;
      this.UInt64 = 0UL;
      this.Double = 0.0;
      this.Half1 = this.Half2 = FourOctetUnion.Empty;
      this.O1 = array[index];
      this.O2 = array[index + 1];
      this.O3 = array[index + 2];
      this.O4 = array[index + 3];
      this.O5 = array[index + 4];
      this.O6 = array[index + 5];
      this.O7 = array[index + 6];
      this.O8 = array[index + 7];
    }

    public EightOctetUnion(
      byte o1,
      byte o2,
      byte o3,
      byte o4,
      byte o5,
      byte o6,
      byte o7,
      byte o8)
    {
      this.Int64 = 0L;
      this.UInt64 = 0UL;
      this.Double = 0.0;
      this.Half1 = this.Half2 = FourOctetUnion.Empty;
      this.O1 = o1;
      this.O2 = o2;
      this.O3 = o3;
      this.O4 = o4;
      this.O5 = o5;
      this.O6 = o6;
      this.O7 = o7;
      this.O8 = o8;
    }

    public EightOctetUnion(FourOctetUnion half1, FourOctetUnion half2)
    {
      this.Int64 = 0L;
      this.UInt64 = 0UL;
      this.Double = 0.0;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.O5 = this.O6 = this.O7 = this.O8 = (byte) 0;
      this.Half1 = half1;
      this.Half2 = half2;
    }

    public EightOctetUnion(long value)
    {
      this.UInt64 = 0UL;
      this.Double = 0.0;
      this.Half1 = this.Half2 = FourOctetUnion.Empty;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.O5 = this.O6 = this.O7 = this.O8 = (byte) 0;
      this.Int64 = value;
    }

    public EightOctetUnion(ulong value)
    {
      this.Int64 = 0L;
      this.Double = 0.0;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.O5 = this.O6 = this.O7 = this.O8 = (byte) 0;
      this.Half1 = this.Half2 = FourOctetUnion.Empty;
      this.UInt64 = value;
    }

    public EightOctetUnion(double value)
    {
      this.Int64 = 0L;
      this.UInt64 = 0UL;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.O5 = this.O6 = this.O7 = this.O8 = (byte) 0;
      this.Half1 = this.Half2 = FourOctetUnion.Empty;
      this.Double = value;
    }

    public override string ToString() => this.Int64.ToString();

    public override int GetHashCode() => this.Int64.GetHashCode();

    public override bool Equals(object obj) => obj != null && obj is EightOctetUnion eightOctetUnion && eightOctetUnion.Int64 == this.Int64;

    public bool GetBit(int index) => (this.Int64 & 1L << index) != 0L;

    public byte GetByte(int index) => (byte) ((this.Int64 & EightOctetUnion._getMasks[index]) >> (index << 3));

    public TwoOctetUnion GetQuarter(int quarter) => quarter < 2 ? (quarter == 0 ? this.Half1.Half1 : this.Half1.Half2) : (quarter == 2 ? this.Half2.Half1 : this.Half2.Half2);

    public void SetBit(int index, bool value) => this.Int64 = value ? this.Int64 | 1L << index : this.Int64 & ~(1L << index);

    public void SetByte(int index, byte value)
    {
      long num = (long) value << (index << 3);
      this.Int64 = this.Int64 & EightOctetUnion._setMasks[index] | num;
    }

    public void SetQuarter(int quarter, TwoOctetUnion value)
    {
      if (quarter < 2)
      {
        if (quarter == 0)
          this.Half1.Half1 = value;
        else
          this.Half1.Half2 = value;
      }
      else if (quarter == 2)
        this.Half2.Half1 = value;
      else
        this.Half2.Half2 = value;
    }

    public static bool operator ==(EightOctetUnion l, EightOctetUnion r) => l.Int64 == r.Int64;

    public static bool operator !=(EightOctetUnion l, EightOctetUnion r) => l.Int64 != r.Int64;
  }
}
