// Decompiled with JetBrains decompiler
// Type: System.IO.BinaryFileReader
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.IO
{
  public sealed class BinaryFileReader : IByteReaderBase, IDisposable
  {
    private const byte True = 1;
    private const byte False = 0;
    private const int MaxCharBytesSize = 128;
    private bool _twoBytesPerChar;
    private byte[] _buffer;
    private char[] _charBuffer;
    private byte[] _charBytes;
    private Decoder _decoder;
    private int _maxCharsSize;
    private char[] _singleChar;
    private Stream _stream;
    private bool _closeStream;
    private int _bufferLength;

    public Stream BaseStream => this._stream;

    public BinaryFileReader(string path)
      : this((Stream) File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None), Encoding.ASCII, true)
    {
    }

    public BinaryFileReader(Stream input, bool closeStream = true)
      : this(input, Encoding.ASCII, closeStream)
    {
    }

    public BinaryFileReader(Stream input, Encoding encoding, bool closeStream)
      : this(encoding, closeStream)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      this._stream = input.CanRead ? input : throw new ArgumentException("Stream is not readable.");
    }

    public BinaryFileReader(Encoding encoding, bool closeStream)
    {
      this._decoder = encoding != null ? encoding.GetDecoder() : throw new ArgumentNullException(nameof (encoding));
      this._maxCharsSize = encoding.GetMaxCharCount(128);
      this._bufferLength = encoding.GetMaxByteCount(1);
      if (this._bufferLength < 16)
        this._bufferLength = 16;
      this._buffer = new byte[this._bufferLength];
      this._charBuffer = (char[]) null;
      this._charBytes = (byte[]) null;
      this._twoBytesPerChar = encoding is UnicodeEncoding;
      this._closeStream = closeStream;
    }

    public void Reset(Stream input, bool closeStream = true)
    {
      if (this._stream != null)
      {
        if (this._closeStream)
          this._stream.Close();
        this._stream = (Stream) null;
      }
      this._stream = input;
      if (this._stream != null && !this._stream.CanRead)
        throw new ArgumentException("Stream is not readable.");
      this._closeStream = closeStream;
    }

    public Exception Perform(Stream input, bool closeStream, Action<IByteReaderBase> operation)
    {
      if (this._stream != null)
      {
        if (this._closeStream)
          this._stream.Close();
        this._stream = (Stream) null;
      }
      Exception exception = (Exception) null;
      this._closeStream = closeStream;
      this._stream = input;
      try
      {
        operation((IByteReaderBase) this);
      }
      catch (Exception ex)
      {
        exception = ex;
      }
      finally
      {
        if (this._closeStream)
        {
          this._stream.Close();
          this._stream = (Stream) null;
        }
      }
      return exception;
    }

    private void FillBuffer(int numBytes)
    {
      if (this._buffer != null && (numBytes < 0 || numBytes > this._bufferLength))
        throw new ArgumentOutOfRangeException(nameof (numBytes), WeaveUtility.GetResourceString("ArgumentOutOfRange_BinaryReaderFillBuffer"));
      if (this._stream == null)
        __Error.FileNotOpen();
      int offset = 0;
      if (numBytes == 1)
      {
        int num = this._stream.ReadByte();
        if (num == -1)
          __Error.EndOfFile();
        this._buffer[0] = (byte) num;
      }
      else
      {
        do
        {
          int num = this._stream.Read(this._buffer, offset, numBytes - offset);
          if (num == 0)
            __Error.EndOfFile();
          offset += num;
        }
        while (offset < numBytes);
      }
    }

    private unsafe int InternalReadChars(char[] buffer, int index, int count)
    {
      int charCount = count;
      if (this._charBytes == null)
        this._charBytes = new byte[128];
      while (charCount > 0)
      {
        int count1 = charCount;
        if (this._twoBytesPerChar)
          count1 <<= 1;
        if (count1 > 128)
          count1 = 128;
        int num = 0;
        byte[] charBytes = this._charBytes;
        int byteCount = this._stream.Read(charBytes, 0, count1);
        if (byteCount == 0)
          return count - charCount;
        int chars;
        fixed (byte* numPtr = charBytes)
          fixed (char* chPtr = buffer)
            chars = this._decoder.GetChars(numPtr + num, byteCount, chPtr + index, charCount, false);
        charCount -= chars;
        index += chars;
      }
      return count - charCount;
    }

    private int InternalReadOneChar()
    {
      int num1 = 0;
      long num2 = 0;
      if (this._stream.CanSeek)
        num2 = this._stream.Position;
      if (this._charBytes == null)
        this._charBytes = new byte[128];
      if (this._singleChar == null)
        this._singleChar = new char[1];
      while (num1 == 0)
      {
        int byteCount = !this._twoBytesPerChar ? 1 : 2;
        int num3 = this._stream.ReadByte();
        this._charBytes[0] = (byte) num3;
        if (num3 == -1)
          byteCount = 0;
        if (byteCount == 2)
        {
          int num4 = this._stream.ReadByte();
          this._charBytes[1] = (byte) num4;
          if (num4 == -1)
            byteCount = 1;
        }
        if (byteCount == 0)
          return -1;
        try
        {
          num1 = this._decoder.GetChars(this._charBytes, 0, byteCount, this._singleChar, 0);
        }
        catch
        {
          if (this._stream.CanSeek)
            this._stream.Seek(num2 - this._stream.Position, SeekOrigin.Current);
          throw;
        }
      }
      return num1 == 0 ? -1 : (int) this._singleChar[0];
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

    public bool ReadBoolean()
    {
      this.FillBuffer(1);
      return this._buffer[0] > (byte) 0;
    }

    public char ReadChar()
    {
      if (this._stream == null)
        __Error.FileNotOpen();
      int num = this.InternalReadOneChar();
      if (num == -1)
        __Error.EndOfFile();
      return (char) num;
    }

    public string ReadString()
    {
      if (this._stream == null)
        __Error.FileNotOpen();
      int capacity = this.Read7BitEncodedInt();
      if (capacity == 0)
        return string.Empty;
      if (capacity < 0)
        throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", (object) capacity));
      if (this._charBytes == null)
        this._charBytes = new byte[128];
      if (this._charBuffer == null)
        this._charBuffer = new char[this._maxCharsSize];
      StringBuilder stringBuilder = (StringBuilder) null;
      int num = 0;
      do
      {
        int byteCount = this._stream.Read(this._charBytes, 0, capacity - num > 128 ? 128 : capacity - num);
        if (byteCount == 0)
          __Error.EndOfFile();
        int chars = this._decoder.GetChars(this._charBytes, 0, byteCount, this._charBuffer, 0);
        if (num == 0 && byteCount == capacity)
          return new string(this._charBuffer, 0, chars);
        if (stringBuilder == null)
          stringBuilder = new StringBuilder(capacity);
        stringBuilder.Append(this._charBuffer, 0, chars);
        num += byteCount;
      }
      while (num < capacity);
      return stringBuilder.ToString();
    }

    public byte[] ReadBytes(int count)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (this._stream == null)
        __Error.FileNotOpen();
      byte[] numArray = new byte[count];
      int length = 0;
      do
      {
        int num = this._stream.Read(numArray, length, count);
        if (num != 0)
        {
          length += num;
          count -= num;
        }
        else
          break;
      }
      while (count > 0);
      if (length != numArray.Length)
      {
        byte[] dst = new byte[length];
        Buffer.BlockCopy((Array) numArray, 0, (Array) dst, 0, length);
        numArray = dst;
      }
      return numArray;
    }

    public int ReadBytes(byte[] buffer, int offset, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer), WeaveUtility.GetResourceString("ArgumentNull_Buffer"));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (this._stream == null)
        __Error.FileNotOpen();
      return this._stream.Read(buffer, offset, count);
    }

    public char[] ReadChars(int count)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (this._stream == null)
        __Error.FileNotOpen();
      char[] chArray = new char[count];
      int length = this.InternalReadChars(chArray, 0, count);
      if (length != count)
      {
        char[] dst = new char[length];
        Buffer.BlockCopy((Array) chArray, 0, (Array) dst, 0, 2 * length);
        chArray = dst;
      }
      return chArray;
    }

    public int ReadChars(char[] buffer, int offset, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer), WeaveUtility.GetResourceString("ArgumentNull_Buffer"));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
      if (this._stream == null)
        __Error.FileNotOpen();
      return this.InternalReadChars(buffer, offset, count);
    }

    public sbyte ReadSByte()
    {
      this.FillBuffer(1);
      return (sbyte) this._buffer[0];
    }

    public unsafe short ReadInt16()
    {
      this.FillBuffer(2);
      short num;
      fixed (byte* numPtr = this._buffer)
        num = *(short*) numPtr;
      return num;
    }

    public unsafe int ReadInt32()
    {
      this.FillBuffer(4);
      int num;
      fixed (byte* numPtr = this._buffer)
        num = *(int*) numPtr;
      return num;
    }

    public unsafe long ReadInt64()
    {
      this.FillBuffer(8);
      long num;
      fixed (byte* numPtr = this._buffer)
        num = *(long*) numPtr;
      return num;
    }

    public unsafe Decimal ReadDecimal()
    {
      this.FillBuffer(16);
      Decimal num;
      fixed (byte* numPtr = this._buffer)
        num = *(Decimal*) numPtr;
      return num;
    }

    public byte ReadByte()
    {
      if (this._stream == null)
        __Error.FileNotOpen();
      int num = this._stream.ReadByte();
      if (num == -1)
        __Error.EndOfFile();
      return (byte) num;
    }

    public unsafe ushort ReadUInt16()
    {
      this.FillBuffer(2);
      ushort num;
      fixed (byte* numPtr = this._buffer)
        num = *(ushort*) numPtr;
      return num;
    }

    public unsafe uint ReadUInt32()
    {
      this.FillBuffer(4);
      uint num;
      fixed (byte* numPtr = this._buffer)
        num = *(uint*) numPtr;
      return num;
    }

    public unsafe ulong ReadUInt64()
    {
      this.FillBuffer(8);
      ulong num;
      fixed (byte* numPtr = this._buffer)
        num = (ulong) *(long*) numPtr;
      return num;
    }

    public unsafe float ReadSingle()
    {
      this.FillBuffer(4);
      float num;
      fixed (byte* numPtr = this._buffer)
        num = *(float*) numPtr;
      return num;
    }

    public unsafe double ReadDouble()
    {
      this.FillBuffer(8);
      double num;
      fixed (byte* numPtr = this._buffer)
        num = *(double*) numPtr;
      return num;
    }

    public unsafe Version ReadVersion()
    {
      this.FillBuffer(16);
      Version version;
      fixed (byte* numPtr = this._buffer)
        version = new Version(*(int*) numPtr, *(int*) (numPtr + 4), *(int*) (numPtr + 8), *(int*) (numPtr + 12));
      return version;
    }

    public unsafe DateTime ReadDateTime()
    {
      this.FillBuffer(8);
      double d;
      fixed (byte* numPtr = this._buffer)
        d = *(double*) numPtr;
      return DateTime.FromOADate(d);
    }

    public unsafe TimeSpan ReadTimeSpan()
    {
      this.FillBuffer(8);
      double num;
      fixed (byte* numPtr = this._buffer)
        num = *(double*) numPtr;
      return TimeSpan.FromMilliseconds(num);
    }

    public Guid ReadGuid() => new Guid(this.ReadBytes(16));

    public unsafe TwoOctetUnion ReadTwoOctet()
    {
      this.FillBuffer(2);
      TwoOctetUnion empty = TwoOctetUnion.Empty;
      TwoOctetUnion twoOctetUnion;
      fixed (byte* numPtr = this._buffer)
        twoOctetUnion = *(TwoOctetUnion*) numPtr;
      return twoOctetUnion;
    }

    public unsafe FourOctetUnion ReadFourOctet()
    {
      this.FillBuffer(4);
      FourOctetUnion empty = FourOctetUnion.Empty;
      FourOctetUnion fourOctetUnion;
      fixed (byte* numPtr = this._buffer)
        fourOctetUnion = *(FourOctetUnion*) numPtr;
      return fourOctetUnion;
    }

    public unsafe EightOctetUnion ReadEightOctet()
    {
      this.FillBuffer(8);
      EightOctetUnion empty = EightOctetUnion.Empty;
      EightOctetUnion eightOctetUnion;
      fixed (byte* numPtr = this._buffer)
        eightOctetUnion = *(EightOctetUnion*) numPtr;
      return eightOctetUnion;
    }

    public void Dispose()
    {
      Stream stream = this._stream;
      this._stream = (Stream) null;
      if (stream != null && this._closeStream)
        stream.Close();
      this._stream = (Stream) null;
      this._buffer = (byte[]) null;
      this._decoder = (Decoder) null;
      this._charBytes = (byte[]) null;
      this._singleChar = (char[]) null;
      this._charBuffer = (char[]) null;
    }
  }
}
