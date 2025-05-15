// Decompiled with JetBrains decompiler
// Type: System.IO.BinaryFileWriter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.IO
{
  public sealed class BinaryFileWriter : IByteWriterBase, IDisposable
  {
    private const byte True = 1;
    private const byte False = 0;
    private const int LargeBufferSize = 256;
    private Stream _stream;
    private Encoder _encoder;
    private Encoding _encoding;
    private byte[] _largeBuffer;
    private byte[] _buffer;
    private int _maxChars;
    private bool _closeStream;

    public Stream BaseStream
    {
      get
      {
        this.Flush();
        return this._stream;
      }
    }

    public BinaryFileWriter(string path, bool append)
      : this((Stream) File.Open(path, append ? FileMode.Open : FileMode.Create, FileAccess.Write, FileShare.None), Encoding.ASCII)
    {
    }

    public BinaryFileWriter(Stream output)
      : this(output, Encoding.ASCII)
    {
    }

    public BinaryFileWriter(Stream output, Encoding encoding, bool closeStream = true)
    {
      if (output == null)
        throw new ArgumentNullException(nameof (output));
      if (encoding == null)
        throw new ArgumentNullException(nameof (encoding));
      this._stream = output.CanWrite ? output : throw new ArgumentException("Stream is not writable.");
      this._buffer = new byte[16];
      this._encoding = encoding;
      this._closeStream = closeStream;
    }

    public void Flush() => this._stream.Flush();

    public long Seek(int offset, SeekOrigin origin) => this._stream.Seek((long) offset, origin);

    public void Write7BitEncodedInt(int value)
    {
      uint num;
      for (num = (uint) value; num >= 128U; num >>= 7)
        this.Write((byte) (num | 128U));
      this.Write((byte) num);
    }

    public void Write(bool value) => this.Write(value ? (byte) 1 : (byte) 0);

    public unsafe void Write(char value)
    {
      if (char.IsSurrogate(value))
        throw new ArgumentException();
      if (this._encoder == null)
        this._encoder = this._encoding.GetEncoder();
      int bytes1;
      fixed (byte* bytes2 = this._buffer)
        bytes1 = this._encoder.GetBytes(&value, 1, bytes2, 16, true);
      this._stream.Write(this._buffer, 0, bytes1);
    }

    public unsafe void Write(string value)
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
        if (this._largeBuffer == null)
        {
          this._largeBuffer = new byte[256];
          this._maxChars = 256 / this._encoding.GetMaxByteCount(1);
        }
        if (byteCount <= 256)
        {
          this._encoding.GetBytes(value, 0, length, this._largeBuffer, 0);
          this._stream.Write(this._largeBuffer, 0, byteCount);
        }
        else
        {
          if (this._encoder == null)
            this._encoder = this._encoding.GetEncoder();
          int num1 = 0;
          int charCount;
          for (int index = length; index > 0; index -= charCount)
          {
            charCount = index > this._maxChars ? this._maxChars : index;
            IntPtr num2;
            if (value == null)
            {
              num2 = IntPtr.Zero;
            }
            else
            {
              fixed (char* chPtr = value)
                num2 = (IntPtr) chPtr;
            }
            char* chPtr1 = (char*) num2;
            int bytes1;
            fixed (byte* bytes2 = this._largeBuffer)
              bytes1 = this._encoder.GetBytes(chPtr1 + num1, charCount, bytes2, 256, charCount == index);
            this._stream.Write(this._largeBuffer, 0, bytes1);
            num1 += charCount;
          }
        }
      }
    }

    public void Write(byte[] value) => this._stream.Write(value, 0, value.Length);

    public void Write(byte[] value, int offset, int count) => this._stream.Write(value, offset, count);

    public void Write(char[] value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      byte[] bytes = this._encoding.GetBytes(value, 0, value.Length);
      this._stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(char[] value, int offset, int count)
    {
      byte[] bytes = this._encoding.GetBytes(value, offset, count);
      this._stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(sbyte value) => this._stream.WriteByte((byte) value);

    public unsafe void Write(short value)
    {
      fixed (byte* numPtr = this._buffer)
        *(short*) numPtr = value;
      this._stream.Write(this._buffer, 0, 2);
    }

    public unsafe void Write(int value)
    {
      fixed (byte* numPtr = this._buffer)
        *(int*) numPtr = value;
      this._stream.Write(this._buffer, 0, 4);
    }

    public unsafe void Write(long value)
    {
      fixed (byte* numPtr = this._buffer)
        *(long*) numPtr = value;
      this._stream.Write(this._buffer, 0, 8);
    }

    public unsafe void Write(Decimal value)
    {
      fixed (byte* numPtr = this._buffer)
        *(Decimal*) numPtr = value;
      this._stream.Write(this._buffer, 0, 16);
    }

    public void Write(byte value) => this._stream.WriteByte(value);

    public unsafe void Write(ushort value)
    {
      fixed (byte* numPtr = this._buffer)
        *(short*) numPtr = (short) value;
      this._stream.Write(this._buffer, 0, 2);
    }

    public unsafe void Write(uint value)
    {
      fixed (byte* numPtr = this._buffer)
        *(int*) numPtr = (int) value;
      this._stream.Write(this._buffer, 0, 4);
    }

    public unsafe void Write(ulong value)
    {
      fixed (byte* numPtr = this._buffer)
        *(long*) numPtr = (long) value;
      this._stream.Write(this._buffer, 0, 8);
    }

    public unsafe void Write(float value)
    {
      fixed (byte* numPtr = this._buffer)
        *(float*) numPtr = value;
      this._stream.Write(this._buffer, 0, 4);
    }

    public unsafe void Write(double value)
    {
      fixed (byte* numPtr = this._buffer)
        *(double*) numPtr = value;
      this._stream.Write(this._buffer, 0, 8);
    }

    public unsafe void Write(Version value)
    {
      fixed (byte* numPtr = this._buffer)
      {
        *(int*) numPtr = value.Major;
        *(int*) (numPtr + 4) = value.Minor;
        *(int*) (numPtr + 8) = value.Build;
        *(int*) (numPtr + 12) = value.Revision;
      }
      this._stream.Write(this._buffer, 0, 16);
    }

    public unsafe void Write(DateTime value)
    {
      fixed (byte* numPtr = this._buffer)
        *(double*) numPtr = value.ToOADate();
      this._stream.Write(this._buffer, 0, 8);
    }

    public unsafe void Write(TimeSpan value)
    {
      fixed (byte* numPtr = this._buffer)
        *(double*) numPtr = value.TotalMilliseconds;
      this._stream.Write(this._buffer, 0, 8);
    }

    public void Write(Guid value) => this.Write(value.ToByteArray(), 0, 16);

    public unsafe void Write(TwoOctetUnion value)
    {
      fixed (byte* numPtr = this._buffer)
        *(TwoOctetUnion*) numPtr = value;
      this._stream.Write(this._buffer, 0, 2);
    }

    public unsafe void Write(FourOctetUnion value)
    {
      fixed (byte* numPtr = this._buffer)
        *(FourOctetUnion*) numPtr = value;
      this._stream.Write(this._buffer, 0, 4);
    }

    public unsafe void Write(EightOctetUnion value)
    {
      fixed (byte* numPtr = this._buffer)
        *(EightOctetUnion*) numPtr = value;
      this._stream.Write(this._buffer, 0, 8);
    }

    public void Dispose()
    {
      if (!this._closeStream)
        return;
      this._stream.Close();
    }
  }
}
