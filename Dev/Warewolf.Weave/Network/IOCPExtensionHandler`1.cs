// Decompiled with JetBrains decompiler
// Type: System.Network.IOCPExtensionHandler`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public abstract class IOCPExtensionHandler<T> where T : NetworkContext
  {
    private PacketTemplate _template;

    public PacketTemplate Template => this._template;

    public IOCPExtensionHandler(PacketTemplate template) => this._template = template != null ? template : throw new ArgumentNullException(nameof (template));

    protected internal abstract void Dispatch(
      IOCPPacketHandlerCollection<T> channel,
      T context,
      PacketData extension,
      PacketData packet);

    protected internal abstract void Response(
      IOCPPacketHandlerCollection<T> channel,
      T context,
      PacketData extension,
      PacketData packet);
  }
}
