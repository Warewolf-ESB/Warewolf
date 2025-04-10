// Decompiled with JetBrains decompiler
// Type: System.Network.IOCPPacketHandler`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class IOCPPacketHandler<T> where T : NetworkContext
  {
    private ushort _packetID;
    private ulong _flags;
    private PacketTemplate _template;
    private PacketEventHandler<T> _handler;

    internal PacketEventHandler<T> Handler => this._handler;

    public ushort PacketID => this._packetID;

    public ulong Flags => this._flags;

    public PacketTemplate Template => this._template;

    public IOCPPacketHandler(
      ushort packetID,
      ulong flags,
      PacketTemplate template,
      PacketEventHandler<T> handler)
    {
      this._packetID = packetID;
      this._flags = flags;
      this._template = template;
      this._handler = handler;
    }
  }
}
