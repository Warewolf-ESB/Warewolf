// Decompiled with JetBrains decompiler
// Type: System.Network.AsyncPacketHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class AsyncPacketHandler
  {
    private ushort _packetID;
    private ulong _flags;
    private PacketTemplate _template;
    private PacketEventHandler _handler;

    public ushort PacketID => this._packetID;

    public ulong Flags => this._flags;

    public PacketTemplate Template => this._template;

    public PacketEventHandler Handler => this._handler;

    public AsyncPacketHandler(
      ushort packetID,
      ulong flags,
      PacketTemplate template,
      PacketEventHandler handler)
    {
      this._packetID = packetID;
      this._flags = flags;
      this._template = template;
      this._handler = handler;
    }
  }
}
