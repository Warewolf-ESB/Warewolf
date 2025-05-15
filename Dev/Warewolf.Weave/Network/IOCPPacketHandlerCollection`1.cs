// Decompiled with JetBrains decompiler
// Type: System.Network.IOCPPacketHandlerCollection`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Threading;

namespace System.Network
{
  public sealed class IOCPPacketHandlerCollection<T> : IDisposable where T : NetworkContext
  {
    private bool _disposed;
    private int _channel;
    private IOCPPacketHandler<T>[] _handlers;
    private IMessageContext _context;

    internal IOCPPacketHandler<T>[] Handlers => this._handlers;

    public bool Disposed => this._disposed;

    public int Channel => this._channel;

    public IOCPPacketHandlerCollection(int channel, IMessageContext context)
    {
      this._channel = channel;
      this._context = context;
      this._handlers = new IOCPPacketHandler<T>[(int) ushort.MaxValue];
    }

    public void Register(ushort pID, PacketTemplate template, PacketEventHandler<T> handler) => this._handlers[(int) pID] = new IOCPPacketHandler<T>(pID, 0UL, template, handler);

    public void Deregister(ushort pID)
    {
      if (pID < (ushort) 0)
        return;
      this._handlers[(int) pID] = (IOCPPacketHandler<T>) null;
    }

    internal void Dispatch(ushort packetID, INetworkOperator op, T context, ByteBuffer reader)
    {
      if (this._context == null)
        this._handlers[(int) packetID].Handler(op, context, reader);
      else
        this._context.Post((IMessage) new IOCPPacketHandlerCollection<T>.PacketHandlerMessage(this._handlers[(int) packetID].Handler, op, context, reader));
    }

    ~IOCPPacketHandlerCollection() => this.Dispose(false);

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      this._disposed = true;
      this._handlers = (IOCPPacketHandler<T>[]) null;
      if (!disposing)
        return;
      GC.SuppressFinalize((object) this);
    }

    private sealed class PacketHandlerMessage : IMessage
    {
      private PacketEventHandler<T> _handler;
      private INetworkOperator _op;
      private T _context;
      private ByteBuffer _reader;

      public PacketHandlerMessage(
        PacketEventHandler<T> handler,
        INetworkOperator op,
        T context,
        ByteBuffer reader)
      {
        this._handler = handler;
        this._op = op;
        this._context = context;
        this._reader = reader;
      }

      public void Execute()
      {
        this._handler(this._op, this._context, this._reader);
        this._reader = (ByteBuffer) null;
        this._context = default (T);
        this._op = (INetworkOperator) null;
        this._handler = (PacketEventHandler<T>) null;
      }
    }
  }
}
