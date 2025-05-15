// Decompiled with JetBrains decompiler
// Type: System.ByteBuffer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.IO;

namespace System
{
  public class ByteBuffer : Stream, IByteStreamBase, IByteReaderBase, IDisposable, IByteWriterBase
  {
    protected ByteKernal _kernal;
    private IByteReaderBase _reader;
    private IByteWriterBase _writer;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => (long) this._kernal.Length;

    public override long Position
    {
      get => (long) this._kernal.Position;
      set => this._kernal.Position = (int) value;
    }

    public byte[] Data => this._kernal.Buffer;

    public ByteBuffer()
      : this(32)
    {
    }

    public ByteBuffer(int initialCapacity)
    {
      this._kernal = new ByteKernal();
      this._kernal.Capacity = Math.Max(initialCapacity, 32);
      this._kernal.Buffer = new byte[this._kernal.Capacity];
      this._reader = this.CreateByteReader();
      this._writer = this.CreateByteWriter();
    }

    public ByteBuffer(byte[] data)
      : this(data, data.Length)
    {
    }

    public ByteBuffer(byte[] data, int length)
      : this(data, 0, length, length)
    {
    }

    public ByteBuffer(byte[] data, int index, int length)
      : this(data, index, length, length)
    {
    }

    public ByteBuffer(byte[] data, int index, int length, int capacity)
    {
      this._kernal = new ByteKernal();
      this._kernal.Capacity = capacity;
      this._kernal.Length = length;
      this._kernal.Position = index;
      this._kernal.Buffer = data;
      this._reader = this.CreateByteReader();
      this._writer = this.CreateByteWriter();
    }

    public override void SetLength(long value) => this._kernal.SetLength((int) value);

    protected virtual IByteReaderBase CreateByteReader() => (IByteReaderBase) new ByteReaderBackend(this._kernal);

    protected virtual IByteWriterBase CreateByteWriter() => (IByteWriterBase) new ByteWriterBackend(this._kernal);

    public override void Flush() => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          this._kernal.Position = (int) offset;
          break;
        case SeekOrigin.Current:
          this._kernal.Position += (int) offset;
          break;
        case SeekOrigin.End:
          this._kernal.Position = this._kernal.Length + (int) offset;
          break;
      }
      return (long) this._kernal.Position;
    }

    public override int Read(byte[] buffer, int offset, int count) => this._reader.ReadBytes(buffer, offset, count);

    public bool ReadBoolean() => this._reader.ReadBoolean();

    public char ReadChar() => this._reader.ReadChar();

    public string ReadString() => this._reader.ReadString();

    public byte[] ReadBytes(int amount) => this._reader.ReadBytes(amount);

    public int ReadBytes(byte[] buffer, int offset, int count) => this._reader.ReadBytes(buffer, offset, count);

    public char[] ReadChars(int amount) => this._reader.ReadChars(amount);

    public int ReadChars(char[] buffer, int offset, int count) => this._reader.ReadChars(buffer, offset, count);

    public sbyte ReadSByte() => this._reader.ReadSByte();

    public short ReadInt16() => this._reader.ReadInt16();

    public int ReadInt32() => this._reader.ReadInt32();

    public long ReadInt64() => this._reader.ReadInt64();

    public Decimal ReadDecimal() => this._reader.ReadDecimal();

    public byte ReadByte() => this._reader.ReadByte();

    public ushort ReadUInt16() => this._reader.ReadUInt16();

    public uint ReadUInt32() => this._reader.ReadUInt32();

    public ulong ReadUInt64() => this._reader.ReadUInt64();

    public float ReadSingle() => this._reader.ReadSingle();

    public double ReadDouble() => this._reader.ReadDouble();

    public Version ReadVersion() => this._reader.ReadVersion();

    public DateTime ReadDateTime() => this._reader.ReadDateTime();

    public TimeSpan ReadTimeSpan() => this._reader.ReadTimeSpan();

    public Guid ReadGuid() => this._reader.ReadGuid();

    public TwoOctetUnion ReadTwoOctet() => this._reader.ReadTwoOctet();

    public FourOctetUnion ReadFourOctet() => this._reader.ReadFourOctet();

    public EightOctetUnion ReadEightOctet() => this._reader.ReadEightOctet();

    public void Write(bool value) => this._writer.Write(value);

    public void Write(char value) => this._writer.Write(value);

    public void Write(string value) => this._writer.Write(value);

    public void Write(byte[] value) => this._writer.Write(value);

    public override void Write(byte[] buffer, int offset, int count) => this._writer.Write(buffer, offset, count);

    public void Write(char[] value) => this._writer.Write(value);

    public void Write(char[] value, int offset, int count) => this._writer.Write(value, offset, count);

    public void Write(sbyte value) => this._writer.Write(value);

    public void Write(short value) => this._writer.Write(value);

    public void Write(int value) => this._writer.Write(value);

    public void Write(long value) => this._writer.Write(value);

    public void Write(Decimal value) => this._writer.Write(value);

    public void Write(byte value) => this._writer.Write(value);

    public void Write(ushort value) => this._writer.Write(value);

    public void Write(uint value) => this._writer.Write(value);

    public void Write(ulong value) => this._writer.Write(value);

    public void Write(float value) => this._writer.Write(value);

    public void Write(double value) => this._writer.Write(value);

    public void Write(Version value) => this._writer.Write(value);

    public void Write(DateTime value) => this._writer.Write(value);

    public void Write(TimeSpan value) => this._writer.Write(value);

    public void Write(Guid value) => this._writer.Write(value);

    public void Write(TwoOctetUnion value) => this._writer.Write(value);

    public void Write(FourOctetUnion value) => this._writer.Write(value);

    public void Write(EightOctetUnion value) => this._writer.Write(value);
  }
}
