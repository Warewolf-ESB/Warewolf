// Decompiled with JetBrains decompiler
// Type: System.Network.PacketAssembler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class PacketAssembler
  {
    private const int NotSupported = -1;
    private const int NotFixed = -2;
    private const byte PHFMask = 15;
    private const byte PHIMask = 240;
    private NetworkHost _host;
    private Connection _owner;
    private IDecryptionProvider _decryptor;
    private int _secureStage;
    private int _channel;
    private PacketHeaderFlags _flags;
    private ushort _packetID;
    private PacketTemplate _template;
    private int _dataLength;
    private int _dataIndex;
    private byte[] _dataStorage;
    private PacketData _assembled;
    private bool _extended;

    public PacketAssembler(Connection owner, IDecryptionProvider decryptor)
    {
      this._host = owner.Host;
      this._owner = owner;
      this._decryptor = decryptor;
    }

    public bool Assemble(byte[] data, int index, int totalLength)
    {
      while (index >= 0 && index != totalLength)
        index = this.AssembleSecured(data, index, totalLength);
      return index != -1;
    }

    private int AssembleSecured(byte[] data, int index, int totalLength)
    {
      if (this._secureStage < 2)
        index = this.AssembleHeader(data, index, totalLength);
      if (this._secureStage < 4)
        index = this.AssembleData(data, index, totalLength);
      if (this._secureStage < 6)
        index = this.AssembleExtensionHeader(data, index, totalLength);
      if (this._secureStage < 8)
        index = this.AssembleExtensionData(data, index, totalLength);
      if (this._secureStage == 8 && index != -1)
      {
        if (this._extended)
        {
          this._host.Dispatch(this._owner, new PacketData(this._channel, this._dataLength, this._dataStorage, this._packetID), this._assembled, (this._flags & PacketHeaderFlags.Extended) != 0);
          this._assembled.Data = (byte[]) null;
        }
        else
          this._host.Dispatch(this._owner, this._channel, this._packetID, this._dataLength, this._dataStorage, (this._flags & PacketHeaderFlags.Extended) != 0);
        this._template = (PacketTemplate) null;
        this._dataStorage = (byte[]) null;
        this._secureStage = 0;
        this._extended = false;
      }
      return index;
    }

    private int AssembleHeader(byte[] data, int index, int totalLength)
    {
      if (index == -1)
        return -1;
      if (this._secureStage == 0)
      {
        if (index >= totalLength)
          return index;
        ++this._secureStage;
        byte num = this._decryptor.Decrypt(data[index++]);
        this._channel = (int) num & -241;
        if (!this._host.ValidateChannel(this._channel))
        {
          this.Fail(PacketAssembler.FailureReason.InvalidChannel);
          return -1;
        }
        this._flags = (PacketHeaderFlags) ((uint) num & 4294967280U);
        this._extended = false;
      }
      if (this._secureStage == 1)
      {
        if ((this._flags & PacketHeaderFlags.Identifier16) != (PacketHeaderFlags) 0)
        {
          if (index + 1 >= totalLength)
            return index;
          this._decryptor.Decrypt(data, index, 2);
          this._packetID = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
          index += 2;
        }
        else
        {
          if (index >= totalLength)
            return index;
          this._packetID = (ushort) this._decryptor.Decrypt(data[index++]);
        }
        if ((this._template = this._host.AcquirePacketTemplate(this._channel, this._packetID)) == null)
        {
          this.Fail(PacketAssembler.FailureReason.InvalidPacketID);
          return -1;
        }
        this._dataIndex = 0;
        if (this._template._dataComponent)
        {
          if (this._template._variableDataLength)
          {
            ++this._secureStage;
          }
          else
          {
            this._dataStorage = new byte[this._dataLength = this._template._dataLength];
            this._secureStage += 2;
          }
        }
        else
          this._secureStage += 3;
      }
      return index;
    }

    private int AssembleData(byte[] data, int index, int totalLength)
    {
      if (index == -1)
        return -1;
      if (this._secureStage == 2)
      {
        if ((this._flags & PacketHeaderFlags.Length32) != (PacketHeaderFlags) 0)
        {
          if (index + 3 >= totalLength)
            return index;
          this._decryptor.Decrypt(data, index, 4);
          this._dataLength = new FourOctetUnion(data[index], data[index + 1], data[index + 2], data[index + 3]).Int32;
          index += 4;
        }
        else if ((this._flags & PacketHeaderFlags.Length16) != (PacketHeaderFlags) 0)
        {
          if (index + 1 >= totalLength)
            return index;
          this._decryptor.Decrypt(data, index, 2);
          this._dataLength = (int) new TwoOctetUnion(data[index], data[index + 1]).UInt16;
          index += 2;
        }
        else
        {
          if (index >= totalLength)
            return index;
          this._dataLength = (int) this._decryptor.Decrypt(data[index++]);
        }
        this._dataStorage = new byte[this._dataLength];
        ++this._secureStage;
      }
      if (this._secureStage == 3)
      {
        int count = this._dataLength - this._dataIndex;
        if (index + count > totalLength)
        {
          int num;
          Buffer.BlockCopy((Array) data, index, (Array) this._dataStorage, this._dataIndex, num = totalLength - index);
          this._dataIndex += num;
          return totalLength;
        }
        if (count != 0)
        {
          Buffer.BlockCopy((Array) data, index, (Array) this._dataStorage, this._dataIndex, count);
          index += count;
        }
        ++this._secureStage;
      }
      return index;
    }

    private int AssembleExtensionHeader(byte[] data, int index, int totalLength)
    {
      if (index == -1)
        return -1;
      if (this._secureStage == 4)
      {
        if ((this._flags & PacketHeaderFlags.Extended) == (PacketHeaderFlags) 0)
        {
          this._extended = false;
          this._secureStage += 4;
          return index;
        }
        if (index >= totalLength)
          return index;
        this._assembled = new PacketData(this._channel, this._dataLength, this._dataStorage, this._packetID);
        this._extended = true;
        this._template = (PacketTemplate) null;
        this._dataStorage = (byte[]) null;
        ++this._secureStage;
        byte num = data[index++];
        this._channel = (int) num & -241;
        if ((this._template = this._host.AcquireExtensionTemplate(this._channel)) == null)
        {
          this.Fail(PacketAssembler.FailureReason.InvalidExtension);
          return -1;
        }
        this._flags = (PacketHeaderFlags) ((uint) num & 4294967280U);
      }
      if (this._secureStage == 5)
      {
        if ((this._flags & PacketHeaderFlags.Identifier16) != (PacketHeaderFlags) 0)
        {
          if (index + 1 >= totalLength)
            return index;
          this._packetID = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
          index += 2;
        }
        else
        {
          if (index >= totalLength)
            return index;
          this._packetID = (ushort) data[index++];
        }
        this._dataIndex = 0;
        if (this._template._dataComponent)
        {
          if (this._template._variableDataLength)
          {
            ++this._secureStage;
          }
          else
          {
            this._dataStorage = new byte[this._dataLength = this._template._dataLength];
            this._secureStage += 2;
          }
        }
        else
          this._secureStage += 3;
      }
      return index;
    }

    private int AssembleExtensionData(byte[] data, int index, int totalLength)
    {
      if (index == -1)
        return -1;
      if (this._secureStage == 6)
      {
        if ((this._flags & PacketHeaderFlags.Length32) != (PacketHeaderFlags) 0)
        {
          if (index + 3 >= totalLength)
            return index;
          this._dataLength = new FourOctetUnion(data[index], data[index + 1], data[index + 2], data[index + 3]).Int32;
          index += 4;
        }
        else if ((this._flags & PacketHeaderFlags.Length16) != (PacketHeaderFlags) 0)
        {
          if (index + 1 >= totalLength)
            return index;
          this._dataLength = (int) new TwoOctetUnion(data[index], data[index + 1]).UInt16;
          index += 2;
        }
        else
        {
          if (index >= totalLength)
            return index;
          this._dataLength = (int) data[index++];
        }
        this._dataStorage = new byte[this._dataLength];
        ++this._secureStage;
      }
      if (this._secureStage == 7)
      {
        int count = this._dataLength - this._dataIndex;
        if (index + count > totalLength)
        {
          int num;
          Buffer.BlockCopy((Array) data, index, (Array) this._dataStorage, this._dataIndex, num = totalLength - index);
          this._dataIndex += num;
          return totalLength;
        }
        if (count != 0)
        {
          Buffer.BlockCopy((Array) data, index, (Array) this._dataStorage, this._dataIndex, count);
          index += count;
        }
        ++this._secureStage;
      }
      return index;
    }

    private void Fail(PacketAssembler.FailureReason reason) => throw new NotImplementedException();

    private enum FailureReason
    {
      Unknown,
      InvalidChannel,
      InvalidPacketID,
      InvalidExtension,
    }
  }
}
