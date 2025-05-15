// Decompiled with JetBrains decompiler
// Type: System.Network.PacketTemplate
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class PacketTemplate
  {
    internal byte _channel;
    internal bool _extendedID;
    internal TwoOctetUnion _ushortID;
    internal byte _byteID;
    internal bool _dataComponent;
    internal bool _variableDataLength;
    internal int _dataLength;

    public PacketTemplate(int channel, int id)
      : this(channel, id, -1)
    {
    }

    public PacketTemplate(int channel, int id, bool dataComponent)
      : this(channel, id, !dataComponent ? -1 : 0)
    {
    }

    public PacketTemplate(int channel, int id, int dataLength)
    {
      this._channel = (byte) channel;
      if (this._extendedID = id > (int) byte.MaxValue)
        this._ushortID = new TwoOctetUnion((ushort) id);
      else
        this._byteID = (byte) id;
      if (!(this._dataComponent = dataLength >= 0))
        return;
      this._variableDataLength = (this._dataLength = dataLength) == 0;
    }
  }
}
