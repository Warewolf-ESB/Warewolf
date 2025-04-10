// Decompiled with JetBrains decompiler
// Type: System.Network.Packet
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.IO;

namespace System.Network
{
  public sealed class Packet : IByteWriterBase, IDisposable
  {
    private const byte PHFMask = 15;
    private const byte PHCMask = 240;
    private PacketTemplate _template;
    private ByteKernal _kernal;
    private IByteWriterBase _writer;
    internal PacketFlags _flags;
    internal byte[] _header;
    internal byte _headerLength;
    internal int _dataLength;

    public int Position
    {
      get => this._kernal.Position;
      set => this._kernal.Position = value;
    }

    public int Length => this._kernal.Length;

    public PacketFlags Flags => this._flags;

    public byte[] Buffer => this._kernal.Buffer;

    public Packet(PacketTemplate template)
    {
      this._template = template;
      if (!this._template._dataComponent)
        return;
      this._kernal = new ByteKernal();
      this._kernal.Capacity = this._template._variableDataLength ? 32 : this._template._dataLength;
      this._kernal.Buffer = new byte[this._kernal.Capacity];
      this._writer = (IByteWriterBase) new ByteWriterBackend(this._kernal);
    }

    public void Compile()
    {
      if ((this._flags & PacketFlags.Compiled) != PacketFlags.None)
        return;
      this._flags |= PacketFlags.Compiled;
      this._dataLength = this._kernal != null ? this._kernal.Length : 0;
      if (!this._template._variableDataLength && this._dataLength != this._template._dataLength)
        throw new ArgumentOutOfRangeException("Compiled length not equal to data length in a fixed length packet.");
      PacketHeaderFlags packetHeaderFlags;
      if (this._template._extendedID)
      {
        packetHeaderFlags = this.InstantiateHeader((byte) 3) | PacketHeaderFlags.Identifier16;
        TwoOctetUnion ushortId = this._template._ushortID;
        this._header[1] = ushortId.O1;
        this._header[2] = ushortId.O2;
      }
      else
      {
        packetHeaderFlags = this.InstantiateHeader((byte) 2);
        this._header[1] = this._template._byteID;
      }
      this._header[0] = (byte) ((PacketHeaderFlags) ((int) this._template._channel & -241) | packetHeaderFlags & (PacketHeaderFlags.Extended | PacketHeaderFlags.Identifier16 | PacketHeaderFlags.Length16 | PacketHeaderFlags.Length32));
    }

    internal PacketHeaderFlags InstantiateHeader(byte length)
    {
      this._headerLength = length;
      PacketHeaderFlags packetHeaderFlags = (PacketHeaderFlags) 0;
      if (this._template._dataComponent && this._template._variableDataLength)
      {
        if (this._dataLength > (int) byte.MaxValue)
        {
          if (this._dataLength > (int) ushort.MaxValue)
          {
            packetHeaderFlags |= PacketHeaderFlags.Length32;
            this._header = new byte[(int) (this._headerLength += (byte) 4)];
            FourOctetUnion fourOctetUnion = new FourOctetUnion(this._dataLength);
            this._header[(int) length] = fourOctetUnion.O1;
            this._header[(int) length + 1] = fourOctetUnion.O2;
            this._header[(int) length + 2] = fourOctetUnion.O3;
            this._header[(int) length + 3] = fourOctetUnion.O4;
          }
          else
          {
            packetHeaderFlags |= PacketHeaderFlags.Length16;
            this._header = new byte[(int) (this._headerLength += (byte) 2)];
            TwoOctetUnion twoOctetUnion = new TwoOctetUnion((ushort) this._dataLength);
            this._header[(int) length] = twoOctetUnion.O1;
            this._header[(int) length + 1] = twoOctetUnion.O2;
          }
        }
        else
        {
          this._header = new byte[(int) ++this._headerLength];
          this._header[(int) length] = (byte) this._dataLength;
        }
      }
      else
        this._header = new byte[(int) this._headerLength];
      return packetHeaderFlags;
    }

    public void Write(bool value) => this._writer.Write(value);

    public void Write(char value) => this._writer.Write(value);

    public void Write(string value) => this._writer.Write(value);

    public void Write(byte[] value) => this._writer.Write(value);

    public void Write(byte[] buffer, int offset, int count) => this._writer.Write(buffer, offset, count);

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

    void IDisposable.Dispose()
    {
    }
  }
}
