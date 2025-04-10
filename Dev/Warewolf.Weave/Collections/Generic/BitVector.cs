// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.BitVector
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
  [StructLayout(LayoutKind.Explicit, Size = 4)]
  public struct BitVector
  {
    public const int SizeInBytes = 4;
    internal static readonly int[] IncludeMasks = new int[4]
    {
      new BitVector() { O1 = byte.MaxValue }._value,
      new BitVector() { O1 = byte.MaxValue, O2 = byte.MaxValue }._value,
      new BitVector()
      {
        O1 = byte.MaxValue,
        O2 = byte.MaxValue,
        O3 = byte.MaxValue
      }._value,
      new BitVector()
      {
        O1 = byte.MaxValue,
        O2 = byte.MaxValue,
        O3 = byte.MaxValue,
        O4 = byte.MaxValue
      }._value
    };
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

    public int Value => this._value;

    public int TotalSetBits => BitVector.GetTotalSetBits(this._value);

    public bool AllTrue => this._value == -1;

    public bool AllFalse => this._value == 0;

    public bool this[int index]
    {
      get => (this._value & 1 << index) != 0;
      set => this._value = value ? this._value | 1 << index : this._value & ~(1 << index);
    }

		private static int GetTotalSetBits(int n)
		{
			uint num1 = (uint)n;
			uint num2 = num1 - (num1 >> 1 & 1431655765U);
			uint num3 = (uint)(((int)num2 & 858993459) + ((int)(num2 >> 2) & 858993459));
			return (int)((((int)num3 + (int)(num3 >> 4) & 252645135) * 16843009) >> 24);
		}

    public static BitVector Construct(params bool[] bits)
    {
      BitVector bitVector = new BitVector();
      for (int index = 0; index < bits.Length; ++index)
        bitVector[index] = bits[index];
      return bitVector;
    }

    public BitVector(BitVector source)
    {
      this.H1 = this.H2 = (short) 0;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this._value = source._value;
    }

    public BitVector(int value)
    {
      this.H1 = this.H2 = (short) 0;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this._value = value;
    }

    public override string ToString()
    {
      char[] chArray = new char[32];
      int num = this._value;
      for (int index = 0; index < 32; ++index)
      {
        chArray[index] = ((long) num & 2147483648L) != 0L ? '1' : '0';
        num <<= 1;
      }
      return "BitVector{" + new string(chArray) + "}";
    }

    public override int GetHashCode() => this._value;

    public override bool Equals(object other) => other is BitVector bitVector && this._value == bitVector._value;

    public void SetAll(bool value) => this._value = value ? -1 : 0;

    public void SetMask(int mask, bool value) => this._value = value ? this._value | mask : this._value & ~mask;

    public bool AreAllTrue(int mask) => (this._value & mask) == mask;

    public bool AreAllFalse(int mask) => (this._value & mask) == 0;

    public bool AreAnyTrue(int mask) => (this._value & mask) != 0;

    public bool AreAnyFalse(int mask) => (this._value & mask) != mask;

    public static bool operator ==(BitVector l, BitVector r) => l._value == r._value;

    public static bool operator !=(BitVector l, BitVector r) => l._value != r._value;
  }
}
