// Decompiled with JetBrains decompiler
// Type: System.IO.ByteWriterBackend
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.IO
{
  internal sealed class ByteWriterBackend : IByteWriterBase, IDisposable
  {
    private const byte True = 1;
    private const byte False = 0;
    private ByteKernal _kernal;
    private Encoding _encoding;

    public int Position
    {
      get => this._kernal.Position;
      set => this._kernal.Position = value;
    }

    public ByteWriterBackend(ByteKernal kernal)
    {
      this._kernal = kernal;
      this._encoding = Encoding.ASCII;
    }

    public void Write7BitEncodedInt(int value)
    {
      uint num;
      for (num = (uint) value; num >= 128U; num >>= 7)
        this.Write((byte) (num | 128U));
      this.Write((byte) num);
    }

    public void Write(bool value) => this.Write(value ? (byte) 1 : (byte) 0);

    public void Write(char value) => this.Write((short) value);

    public void Write(string value)
    {
      int length = value != null ? value.Length : 0;
      if (length == 0)
      {
        this.Write7BitEncodedInt(0);
      }
      else
      {
        int byteCount = this._encoding.GetByteCount(value);
        this.Write7BitEncodedInt(byteCount);
        this._kernal.SetLength(this._kernal.Length + byteCount);
        this._encoding.GetBytes(value, 0, length, this._kernal.Buffer, this._kernal.Position);
        this._kernal.Position += byteCount;
      }
    }

    public void Write(byte[] value) => this.Write(value, 0, value.Length);

    public void Write(byte[] value, int offset, int count)
    {
      int end = this._kernal.Position + count;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      if (count <= 8)
      {
        while (--count >= 0)
          this._kernal.Buffer[this._kernal.Position + count] = value[offset + count];
      }
      else
        Buffer.BlockCopy((Array) value, offset, (Array) this._kernal.Buffer, this._kernal.Position, count);
      this._kernal.Position = end;
    }

    public void Write(char[] value)
    {
      int length = value.Length;
      if (length == 0)
      {
        this.Write7BitEncodedInt(0);
      }
      else
      {
        int byteCount = this._encoding.GetByteCount(value);
        this.Write7BitEncodedInt(byteCount);
        this._kernal.SetLength(this._kernal.Length + byteCount);
        this._encoding.GetBytes(value, 0, length, this._kernal.Buffer, this._kernal.Position);
        this._kernal.Position += byteCount;
      }
    }

    public void Write(char[] value, int offset, int count)
    {
      if (count == 0)
      {
        this.Write7BitEncodedInt(0);
      }
      else
      {
        int byteCount = this._encoding.GetByteCount(value, offset, count);
        this.Write7BitEncodedInt(byteCount);
        this._kernal.SetLength(this._kernal.Length + byteCount);
        this._encoding.GetBytes(value, offset, count, this._kernal.Buffer, this._kernal.Position);
        this._kernal.Position += byteCount;
      }
    }

    public void Write(sbyte value) => this.Write((byte) value);

    public unsafe void Write(short value)
    {
      int end = this._kernal.Position + 2;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(short*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(int value)
    {
      int end = this._kernal.Position + 4;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(int*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(long value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(long*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(Decimal value)
    {
      int end = this._kernal.Position + 16;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(Decimal*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public void Write(byte value)
    {
      if (this._kernal.Position >= this._kernal.Length)
      {
        int num = this._kernal.Position + 1;
        bool flag = this._kernal.Position > this._kernal.Length;
        if (num >= this._kernal.Capacity && this._kernal.EnsureCapacity(num))
          flag = false;
        if (flag)
          Array.Clear((Array) this._kernal.Buffer, this._kernal.Length, this._kernal.Position - this._kernal.Length);
        this._kernal.Length = num;
      }
      this._kernal.Buffer[this._kernal.Position++] = value;
    }

    public unsafe void Write(ushort value)
    {
      int end = this._kernal.Position + 2;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(short*) (numPtr + this._kernal.Position) = (short) value;
      this._kernal.Position = end;
    }

    public unsafe void Write(uint value)
    {
      int end = this._kernal.Position + 4;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(int*) (numPtr + this._kernal.Position) = (int) value;
      this._kernal.Position = end;
    }

    public unsafe void Write(ulong value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(long*) (numPtr + this._kernal.Position) = (long) value;
      this._kernal.Position = end;
    }

    public unsafe void Write(float value)
    {
      int end = this._kernal.Position + 4;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(float*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(double value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(double*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(Version value)
    {
      int end = this._kernal.Position + 16;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
      {
        *(int*) (numPtr + this._kernal.Position) = value.Major;
        *(int*) (numPtr + this._kernal.Position + 4) = value.Minor;
        *(int*) (numPtr + this._kernal.Position + 8) = value.Build;
        *(int*) (numPtr + this._kernal.Position + 12) = value.Revision;
      }
      this._kernal.Position = end;
    }

    public unsafe void Write(DateTime value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(double*) (numPtr + this._kernal.Position) = value.ToOADate();
      this._kernal.Position = end;
    }

    public unsafe void Write(TimeSpan value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(double*) (numPtr + this._kernal.Position) = value.TotalMilliseconds;
      this._kernal.Position = end;
    }

    public void Write(Guid value) => this.Write(value.ToByteArray(), 0, 16);

    public unsafe void Write(TwoOctetUnion value)
    {
      int end = this._kernal.Position + 2;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(TwoOctetUnion*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(FourOctetUnion value)
    {
      int end = this._kernal.Position + 4;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(FourOctetUnion*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    public unsafe void Write(EightOctetUnion value)
    {
      int end = this._kernal.Position + 8;
      if (end > this._kernal.Length)
        this._kernal.EnsureWrite(end);
      fixed (byte* numPtr = this._kernal.Buffer)
        *(EightOctetUnion*) (numPtr + this._kernal.Position) = value;
      this._kernal.Position = end;
    }

    void IDisposable.Dispose()
    {
    }
  }
}
