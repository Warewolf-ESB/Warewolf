// Decompiled with JetBrains decompiler
// Type: System.FourOctetUnion
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.InteropServices;

namespace System
{
  [StructLayout(LayoutKind.Explicit, Size = 4)]
  public struct FourOctetUnion
  {
    public const int SizeInBytes = 4;
    public static readonly FourOctetUnion Empty;
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

    public bool this[int index]
    {
      get => (this.UInt32 & (uint) (1 << index)) > 0U;
      set => this.UInt32 = value ? this.UInt32 | (uint) (1 << index) : this.UInt32 & (uint) ~(1 << index);
    }

    public FourOctetUnion(byte[] array, int index)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.O1 = array[index];
      this.O2 = array[index + 1];
      this.O3 = array[index + 2];
      this.O4 = array[index + 3];
    }

    public FourOctetUnion(char o1, char o2, char o3, char o4)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.O1 = (byte) o1;
      this.O2 = (byte) o2;
      this.O3 = (byte) o3;
      this.O4 = (byte) o4;
    }

    public FourOctetUnion(byte o1, byte o2, byte o3, byte o4)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.O1 = o1;
      this.O2 = o2;
      this.O3 = o3;
      this.O4 = o4;
    }

    public FourOctetUnion(short half1, short half2)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.Half1 = new TwoOctetUnion(half1);
      this.Half2 = new TwoOctetUnion(half2);
    }

    public FourOctetUnion(TwoOctetUnion half1, TwoOctetUnion half2)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.Half1 = half1;
      this.Half2 = half2;
    }

    public FourOctetUnion(int value)
    {
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.Int32 = value;
    }

    public FourOctetUnion(uint value)
    {
      this.Int32 = 0;
      this.Single = 0.0f;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.UInt32 = value;
    }

    public FourOctetUnion(float value)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      this.Half1 = this.Half2 = TwoOctetUnion.Empty;
      this.Single = value;
    }

    public FourOctetUnion(string word)
    {
      this.Int32 = 0;
      this.UInt32 = 0U;
      this.Single = 0.0f;
      this.O1 = this.O2 = this.O3 = this.O4 = (byte) 0;
      int length = word != null ? word.Length : 0;
      if (length > 1)
      {
        this.Half1 = new TwoOctetUnion(word[0]);
        this.Half2 = new TwoOctetUnion(word[1]);
      }
      else
      {
        this.Half1 = length <= 0 ? TwoOctetUnion.Empty : new TwoOctetUnion(word[0]);
        this.Half2 = TwoOctetUnion.Empty;
      }
    }

    public override string ToString() => this.Int32.ToString();

    public override int GetHashCode() => this.Int32;

    public override bool Equals(object obj) => obj != null && obj is FourOctetUnion fourOctetUnion && fourOctetUnion.Int32 == this.Int32;

    public static bool operator ==(FourOctetUnion l, FourOctetUnion r) => l.Int32 == r.Int32;

    public static bool operator !=(FourOctetUnion l, FourOctetUnion r) => l.Int32 != r.Int32;
  }
}
