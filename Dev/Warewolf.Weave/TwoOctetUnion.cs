// Decompiled with JetBrains decompiler
// Type: System.TwoOctetUnion
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.InteropServices;

namespace System
{
  [StructLayout(LayoutKind.Explicit, Size = 2)]
  public struct TwoOctetUnion
  {
    public const int SizeInBytes = 2;
    public static readonly TwoOctetUnion Empty;
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

    public TwoOctetUnion(byte[] array, int index)
    {
      this.Char = char.MinValue;
      this.Int16 = (short) 0;
      this.UInt16 = (ushort) 0;
      this.O1 = array[index];
      this.O2 = array[index + 1];
    }

    public TwoOctetUnion(byte o1, byte o2)
    {
      this.Char = char.MinValue;
      this.Int16 = (short) 0;
      this.UInt16 = (ushort) 0;
      this.O1 = o1;
      this.O2 = o2;
    }

    public TwoOctetUnion(short value)
    {
      this.Char = char.MinValue;
      this.UInt16 = (ushort) 0;
      this.O1 = (byte) 0;
      this.O2 = (byte) 0;
      this.Int16 = value;
    }

    public TwoOctetUnion(ushort value)
    {
      this.Char = char.MinValue;
      this.Int16 = (short) 0;
      this.O1 = (byte) 0;
      this.O2 = (byte) 0;
      this.UInt16 = value;
    }

    public TwoOctetUnion(char value)
    {
      this.Int16 = (short) 0;
      this.UInt16 = (ushort) 0;
      this.O1 = (byte) 0;
      this.O2 = (byte) 0;
      this.Char = value;
    }

    public override string ToString() => this.Int16.ToString();

    public override int GetHashCode() => (int) this.Int16;

    public override bool Equals(object obj) => obj != null && obj is TwoOctetUnion twoOctetUnion && (int) twoOctetUnion.Int16 == (int) this.Int16;

    public static bool operator ==(TwoOctetUnion l, TwoOctetUnion r) => (int) l.Int16 == (int) r.Int16;

    public static bool operator !=(TwoOctetUnion l, TwoOctetUnion r) => (int) l.Int16 != (int) r.Int16;
  }
}
