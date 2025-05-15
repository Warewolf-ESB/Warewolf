// Decompiled with JetBrains decompiler
// Type: System.IO.ByteReaderBackend
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.IO
{
  internal sealed class ByteReaderBackend : IByteReaderBase, IDisposable
  {
    private const byte True = 1;
    private const byte False = 0;
    private static readonly char[] _zeroCharArray = new char[0];
    private ByteKernal _kernal;
    private Encoding _encoding;

    public ByteReaderBackend(ByteKernal kernal)
    {
      this._kernal = kernal;
      this._encoding = Encoding.ASCII;
    }

    public int Read7BitEncodedInt()
    {
      int num1 = 0;
      int num2 = 0;
      while (num2 != 35)
      {
        byte num3 = this.ReadByte();
        num1 |= ((int) num3 & (int) sbyte.MaxValue) << num2;
        num2 += 7;
        if (((int) num3 & 128) == 0)
          return num1;
      }
      throw new FormatException(WeaveUtility.GetResourceString("Format_Bad7BitInt32"));
    }

    public bool ReadBoolean() => this._kernal.EnsureRead(1) && this._kernal.Buffer[this._kernal.Position++] > (byte) 0;

    public char ReadChar() => (char) this.ReadInt16();

    public string ReadString()
    {
      int num = this.Read7BitEncodedInt();
      if (num == 0)
        return string.Empty;
      if (num < 0)
        throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", (object) num));
      if (!this._kernal.EnsureRead(num))
        throw new EndOfStreamException();
      string str = this._encoding.GetString(this._kernal.Buffer, this._kernal.Position, num);
      this._kernal.Position += num;
      return str;
    }

    public byte[] ReadBytes(int amount)
    {
      if (!this._kernal.EnsureRead(amount))
        return new byte[amount];
      byte[] dst = new byte[amount];
      Buffer.BlockCopy((Array) this._kernal.Buffer, this._kernal.Position, (Array) dst, 0, amount);
      this._kernal.Position += amount;
      return dst;
    }

    public int ReadBytes(byte[] buffer, int offset, int count)
    {
      if (!this._kernal.EnsureRead(count))
      {
        count = this._kernal.Length - this._kernal.Position;
        if (count == 0)
          return 0;
      }
      Buffer.BlockCopy((Array) this._kernal.Buffer, this._kernal.Position, (Array) buffer, offset, count);
      this._kernal.Position += count;
      return count;
    }

    public char[] ReadChars(int amount)
    {
      int num = this.Read7BitEncodedInt();
      if (num == 0)
        return ByteReaderBackend._zeroCharArray;
      if (num < 0)
        throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", (object) num));
      if (!this._kernal.EnsureRead(num))
        throw new EndOfStreamException();
      char[] chars = this._encoding.GetChars(this._kernal.Buffer, this._kernal.Position, num);
      this._kernal.Position += num;
      return chars;
    }

    public int ReadChars(char[] buffer, int offset, int count)
    {
      int num = this.Read7BitEncodedInt();
      if (num == 0)
        return 0;
      if (num < 0)
        throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", (object) num));
      if (!this._kernal.EnsureRead(num))
        throw new EndOfStreamException();
      int chars = this._encoding.GetChars(this._kernal.Buffer, this._kernal.Position, num, buffer, offset);
      this._kernal.Position += num;
      return chars;
    }

    public sbyte ReadSByte() => !this._kernal.EnsureRead(1) ? sbyte.MinValue : (sbyte) this._kernal.Buffer[this._kernal.Position++];

    public unsafe short ReadInt16()
    {
      if (!this._kernal.EnsureRead(2))
        return 0;
      short num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(short*) (numPtr + this._kernal.Position);
      this._kernal.Position += 2;
      return num;
    }

    public unsafe int ReadInt32()
    {
      if (!this._kernal.EnsureRead(4))
        return 0;
      int num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(int*) (numPtr + this._kernal.Position);
      this._kernal.Position += 4;
      return num;
    }

    public unsafe long ReadInt64()
    {
      if (!this._kernal.EnsureRead(8))
        return 0;
      long num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(long*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return num;
    }

    public unsafe Decimal ReadDecimal()
    {
      if (!this._kernal.EnsureRead(16))
        return 0M;
      Decimal num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(Decimal*) (numPtr + this._kernal.Position);
      this._kernal.Position += 16;
      return num;
    }

    public byte ReadByte() => !this._kernal.EnsureRead(1) ? (byte) 0 : this._kernal.Buffer[this._kernal.Position++];

    public unsafe ushort ReadUInt16()
    {
      if (!this._kernal.EnsureRead(2))
        return 0;
      ushort num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(ushort*) (numPtr + this._kernal.Position);
      this._kernal.Position += 2;
      return num;
    }

    public unsafe uint ReadUInt32()
    {
      if (!this._kernal.EnsureRead(4))
        return 0;
      uint num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(uint*) (numPtr + this._kernal.Position);
      this._kernal.Position += 4;
      return num;
    }

    public unsafe ulong ReadUInt64()
    {
      if (!this._kernal.EnsureRead(8))
        return 0;
      ulong num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = (ulong) *(long*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return num;
    }

    public unsafe float ReadSingle()
    {
      if (!this._kernal.EnsureRead(4))
        return 0.0f;
      float num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(float*) (numPtr + this._kernal.Position);
      this._kernal.Position += 4;
      return num;
    }

    public unsafe double ReadDouble()
    {
      if (!this._kernal.EnsureRead(8))
        return 0.0;
      double num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(double*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return num;
    }

    public unsafe Version ReadVersion()
    {
      if (!this._kernal.EnsureRead(16))
        return new Version();
      Version version;
      fixed (byte* numPtr = this._kernal.Buffer)
        version = new Version(*(int*) (numPtr + this._kernal.Position), *(int*) (numPtr + this._kernal.Position + 4), *(int*) (numPtr + this._kernal.Position + 8), *(int*) (numPtr + this._kernal.Position + 12));
      this._kernal.Position += 16;
      return version;
    }

    public unsafe DateTime ReadDateTime()
    {
      if (!this._kernal.EnsureRead(8))
        return DateTime.FromOADate(0.0);
      double d;
      fixed (byte* numPtr = this._kernal.Buffer)
        d = *(double*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return DateTime.FromOADate(d);
    }

    public unsafe TimeSpan ReadTimeSpan()
    {
      if (!this._kernal.EnsureRead(8))
        return TimeSpan.FromMilliseconds(0.0);
      double num;
      fixed (byte* numPtr = this._kernal.Buffer)
        num = *(double*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return TimeSpan.FromMilliseconds(num);
    }

    public Guid ReadGuid() => new Guid(this.ReadBytes(16));

    public unsafe TwoOctetUnion ReadTwoOctet()
    {
      if (!this._kernal.EnsureRead(2))
        return TwoOctetUnion.Empty;
      TwoOctetUnion empty = TwoOctetUnion.Empty;
      TwoOctetUnion twoOctetUnion;
      fixed (byte* numPtr = this._kernal.Buffer)
        twoOctetUnion = *(TwoOctetUnion*) (numPtr + this._kernal.Position);
      this._kernal.Position += 2;
      return twoOctetUnion;
    }

    public unsafe FourOctetUnion ReadFourOctet()
    {
      if (!this._kernal.EnsureRead(4))
        return FourOctetUnion.Empty;
      FourOctetUnion empty = FourOctetUnion.Empty;
      FourOctetUnion fourOctetUnion;
      fixed (byte* numPtr = this._kernal.Buffer)
        fourOctetUnion = *(FourOctetUnion*) (numPtr + this._kernal.Position);
      this._kernal.Position += 4;
      return fourOctetUnion;
    }

    public unsafe EightOctetUnion ReadEightOctet()
    {
      if (!this._kernal.EnsureRead(8))
        return EightOctetUnion.Empty;
      EightOctetUnion empty = EightOctetUnion.Empty;
      EightOctetUnion eightOctetUnion;
      fixed (byte* numPtr = this._kernal.Buffer)
        eightOctetUnion = *(EightOctetUnion*) (numPtr + this._kernal.Position);
      this._kernal.Position += 8;
      return eightOctetUnion;
    }

    void IDisposable.Dispose()
    {
    }
  }
}
