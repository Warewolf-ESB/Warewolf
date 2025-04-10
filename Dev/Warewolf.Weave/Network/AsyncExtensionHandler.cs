// Decompiled with JetBrains decompiler
// Type: System.Network.AsyncExtensionHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public abstract class AsyncExtensionHandler
  {
    private PacketTemplate _template;

    public PacketTemplate Template => this._template;

    public AsyncExtensionHandler(PacketTemplate template) => this._template = template != null ? template : throw new ArgumentNullException(nameof (template));

    public abstract void Dispatch(
      AsyncPacketHandlerCollection channel,
      Connection connection,
      PacketData extension,
      PacketData packet);

    public abstract void Response(
      AsyncPacketHandlerCollection channel,
      Connection connection,
      PacketData extension,
      PacketData packet);
  }
}
