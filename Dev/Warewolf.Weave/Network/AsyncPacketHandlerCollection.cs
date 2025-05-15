// Decompiled with JetBrains decompiler
// Type: System.Network.AsyncPacketHandlerCollection
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Threading;

namespace System.Network
{
  public sealed class AsyncPacketHandlerCollection : IDisposable
  {
    private bool _disposed;
    private AsyncPacketHandler[] _handlers;
    private IMessageContext _context;

    public bool Disposed => this._disposed;

    public IMessageContext Context
    {
      get => this._context;
      set => this._context = value;
    }

    public AsyncPacketHandler[] Handlers => this._handlers;

    public AsyncPacketHandlerCollection() => this._handlers = new AsyncPacketHandler[(int) ushort.MaxValue];

    public void Register(ushort pID, PacketTemplate template, PacketEventHandler handler) => this._handlers[(int) pID] = new AsyncPacketHandler(pID, 0UL, template, handler);

    public void Deregister(ushort pID)
    {
      if (pID < (ushort) 0)
        return;
      this._handlers[(int) pID] = (AsyncPacketHandler) null;
    }

    public void Dispatch(ushort packetID, INetworkOperator op, ByteBuffer reader)
    {
      if (this._context == null)
        this._handlers[(int) packetID].Handler(op, reader);
      else
        this._context.Post((IMessage) new AsyncPacketHandlerCollection.PacketHandlerMessage(this._handlers[(int) packetID].Handler, op, reader));
    }

    ~AsyncPacketHandlerCollection() => this.Dispose(false);

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      this._disposed = true;
      this._handlers = (AsyncPacketHandler[]) null;
      if (!disposing)
        return;
      GC.SuppressFinalize((object) this);
    }

    private sealed class PacketHandlerMessage : IMessage
    {
      private PacketEventHandler _handler;
      private INetworkOperator _op;
      private ByteBuffer _reader;

      public PacketHandlerMessage(
        PacketEventHandler handler,
        INetworkOperator op,
        ByteBuffer reader)
      {
        this._handler = handler;
        this._op = op;
        this._reader = reader;
      }

      public void Execute()
      {
        this._handler(this._op, this._reader);
        this._reader = (ByteBuffer) null;
        this._op = (INetworkOperator) null;
        this._handler = (PacketEventHandler) null;
      }
    }
  }
}
