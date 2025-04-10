// Decompiled with JetBrains decompiler
// Type: System.Network.TCPServer`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace System.Network
{
  public abstract class TCPServer<T> : NetworkServer where T : NetworkContext, new()
  {
    private bool _running;
    private Firewall _firewall;
    private Listener[] _listeners;
    protected InboundAuthenticationBroker _authenticationBroker;
    private SocketEventHandler _onSocketConnected;
    private ConnectionEventHandler _onConnectionCreated;
    private ConnectionEventHandler _onConnectionDisposed;
    private NetworkContextEventHandler<T> _onContextAttached;
    private NetworkContextEventHandler<T> _onContextDetached;
    protected IOCPPacketHandlerCollection<T>[] _channels;
    protected IOCPExtensionHandler<T>[] _extensions;
    protected ReaderWriterLockSlim _contextLock;
    protected IndexedList<T> _attachedContexts;

    public bool Running => this._running;

    public IOCPPacketHandlerCollection<T>[] Channels => this._channels;

    public IOCPExtensionHandler<T>[] Extensions => this._extensions;

    public IndexedList<T> AttachedContexts => this._attachedContexts;

    public event SocketEventHandler SocketConnected
    {
      add
      {
        if (this._disposing)
          return;
        this._onSocketConnected += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onSocketConnected -= value;
      }
    }

    public event ConnectionEventHandler ConnectionCreated
    {
      add
      {
        if (this._disposing)
          return;
        this._onConnectionCreated += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onConnectionCreated -= value;
      }
    }

    public event ConnectionEventHandler ConnectionDisposed
    {
      add
      {
        if (this._disposing)
          return;
        this._onConnectionDisposed += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onConnectionDisposed -= value;
      }
    }

    public event NetworkContextEventHandler<T> ContextAttached
    {
      add
      {
        if (this._disposing)
          return;
        this._onContextAttached += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onContextAttached -= value;
      }
    }

    public event NetworkContextEventHandler<T> ContextDetached
    {
      add
      {
        if (this._disposing)
          return;
        this._onContextDetached += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onContextDetached -= value;
      }
    }

    public TCPServer(string name, InboundAuthenticationBroker authenticationBroker)
      : base(name)
    {
      this._firewall = new Firewall();
      this._authenticationBroker = authenticationBroker;
      this._channels = new IOCPPacketHandlerCollection<T>[16];
      this._extensions = new IOCPExtensionHandler<T>[16];
      this._contextLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this._attachedContexts = new IndexedList<T>();
      this._channels[15] = new IOCPPacketHandlerCollection<T>(15, (IMessageContext) null);
      this._channels[15].Register((ushort) 1, InternalTemplates.Server_OnExecuteStringCommandReceived, new PacketEventHandler<T>(this.OnExecuteStringCommandReceived));
      this._channels[15].Register((ushort) 2, InternalTemplates.Server_OnExecuteBinaryCommandReceived, new PacketEventHandler<T>(this.OnExecuteBinaryCommandReceived));
    }

    public bool Start(params ListenerConfig[] listeners)
    {
      List<Listener> listenerList = new List<Listener>();
      if (this._listeners != null && this._listeners.Length != 0)
      {
        if (!this._running)
        {
          for (int index = 0; index < this._listeners.Length; ++index)
          {
            if (this._listeners[index].Start())
              listenerList.Add(this._listeners[index]);
          }
        }
        else
          listenerList.AddRange((IEnumerable<Listener>) this._listeners);
      }
      if (listeners != null && listeners.Length != 0)
      {
        for (int index = 0; index < listeners.Length; ++index)
        {
          Listener listener = new Listener((NetworkServer) this, listeners[index]);
          if (listener.Start())
            listenerList.Add(listener);
        }
      }
      this._listeners = listenerList.ToArray();
      this.OnStarted(listeners);
      return this._running = this._listeners.Length != 0;
    }

    public void Stop()
    {
      if (!this._running)
        return;
      this._running = false;
      this.OnStopped();
      for (int index = 0; index < this._listeners.Length; ++index)
        this._listeners[index].Stop();
    }

    protected virtual void OnStarted(ListenerConfig[] configs)
    {
    }

    protected virtual void OnStopped()
    {
    }

    private void OnExecuteStringCommandReceived(INetworkOperator op, T context, ByteBuffer reader)
    {
      long num = reader.ReadInt64();
      Guid dataListID = reader.ReadGuid();
      string payload = reader.ReadString();
      string str = this.OnExecuteCommand(context, payload, dataListID);
      Packet p = new Packet(InternalTemplates.Client_OnExecuteStringCommandReceived);
      p.Write(num);
      p.Write(dataListID);
      p.Write(str);
      op.Send(p);
    }

    private void OnExecuteBinaryCommandReceived(INetworkOperator op, T context, ByteBuffer reader)
    {
      long num = reader.ReadInt64();
      Packet packet = new Packet(InternalTemplates.Client_OnExecuteBinaryCommandReceived);
      packet.Write(num);
      this.OnExecuteCommand(context, reader, packet);
      op.Send(packet);
    }

    protected abstract string OnExecuteCommand(T context, string payload, Guid dataListID);

    protected abstract void OnExecuteCommand(T context, ByteBuffer payload, Packet writer);

    protected override bool ValidateSocketConnection(
      Listener source,
      Socket socket,
      IPAddress address,
      string addressString,
      int port)
    {
      return this._firewall == null || !this._firewall.IsBlocked(address);
    }

    protected override void ApplySocketOptions(Listener source, Socket socket) => socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, true);

    protected override void OnSocketConnectionAccepted(
      Listener source,
      Socket socket,
      IPAddress address,
      string addressString,
      int port)
    {
      try
      {
        IOCPConnection iocpConnection = new IOCPConnection((NetworkHost) this, socket, NetworkDirection.Inbound);
        iocpConnection.AuthBroker = (AuthenticationBroker) this._authenticationBroker.Instantiate(this._firewall, (Connection) iocpConnection);
        iocpConnection.Start();
        if (iocpConnection.Alive)
          return;
        iocpConnection.Dispose();
      }
      catch
      {
        NetworkHelper.ReleaseSocket(ref socket);
      }
    }

    protected override void OnConnectionCreated(Connection connection)
    {
      if (this._onConnectionCreated == null)
        return;
      this._onConnectionCreated((NetworkHost) this, connection);
    }

    protected override void OnConnectionDisposed(Connection connection)
    {
      if (!connection.WasAlive)
        return;
      if (connection.Context != null)
      {
        if (connection.Context is T context)
        {
          try
          {
            this._contextLock.EnterWriteLock();
            this._attachedContexts.Remove(context);
          }
          finally
          {
            this._contextLock.ExitWriteLock();
          }
          try
          {
            if (context.Attached)
            {
              if (this._onContextDetached != null)
                this._onContextDetached(this, context);
            }
          }
          finally
          {
            context.NotifyConnectionDisposed();
          }
          if (this._onConnectionDisposed == null)
            return;
          this._onConnectionDisposed((NetworkHost) this, connection);
        }
        else
        {
          if (this._onConnectionDisposed == null)
            return;
          this._onConnectionDisposed((NetworkHost) this, connection);
        }
      }
      else
      {
        if (this._onConnectionDisposed == null)
          return;
        this._onConnectionDisposed((NetworkHost) this, connection);
      }
    }

    protected override void OnLoginSuccess(
      Connection connection,
      NetworkAccount account,
      AuthenticationBroker broker)
    {
      if (this._disposing)
        return;
      T context = new T();
      try
      {
        this._contextLock.EnterWriteLock();
        this._attachedContexts.Add(context);
        context.NotifyLogin(broker as InboundAuthenticationBroker, connection, account);
      }
      catch
      {
      }
      finally
      {
        this._contextLock.ExitWriteLock();
      }
      if (this._onContextAttached == null)
        return;
      this._onContextAttached(this, context);
    }

    protected override void OnLoginFailed(
      Connection connection,
      OutboundAuthenticationBroker broker,
      AuthenticationResponse reason,
      bool expectDisconnect)
    {
    }

    protected override void OnLogoutRequired(
      Connection connection,
      InboundAuthenticationBroker broker,
      AuthenticationResponse reason)
    {
      Packet p = new Packet(InternalTemplates.Client_LogoutReceived);
      p.Write((byte) reason);
      connection.Send(p);
      if (connection.Context == null)
        return;
      T context = connection.Context as T;
      this._contextLock.EnterWriteLock();
      try
      {
        this._attachedContexts.Remove(context);
      }
      finally
      {
        this._contextLock.ExitWriteLock();
      }
      try
      {
        if (context.Attached)
        {
          if (this._onContextDetached != null)
            this._onContextDetached(this, context);
        }
      }
      finally
      {
        context.NotifyLogout(reason);
      }
      connection.AuthBroker = (AuthenticationBroker) this._authenticationBroker.Instantiate(this._firewall, connection);
      connection.Crypt = (ICryptProvider) NullCryptProvider.Singleton;
      connection.Assembler = (PacketAssembler) null;
    }

    private void OnLogoutReceived(INetworkOperator op, T context, ByteBuffer reader)
    {
      Console.WriteLine("Logout Received From " + context.ToString());
      AuthenticationResponse response = AuthenticationResponse.Logout;
      Packet p = new Packet(InternalTemplates.Client_LogoutReceived);
      p.Write((byte) response);
      op.Send(p);
      Connection connection = context.Connection;
      this._contextLock.EnterWriteLock();
      try
      {
        this._attachedContexts.Remove(context);
      }
      finally
      {
        this._contextLock.ExitWriteLock();
      }
      try
      {
        if (context.Attached)
        {
          if (this._onContextDetached != null)
            this._onContextDetached(this, context);
        }
      }
      finally
      {
        context.NotifyLogout(response);
      }
      connection.AuthBroker = (AuthenticationBroker) this._authenticationBroker.Instantiate(this._firewall, connection);
      connection.Crypt = (ICryptProvider) NullCryptProvider.Singleton;
      connection.Assembler = (PacketAssembler) null;
    }

    protected internal override bool ValidateChannel(int channel) => this._channels[channel] != null;

    protected internal override PacketTemplate AcquirePacketTemplate(int channel, ushort packetID) => this._channels[channel].Handlers[(int) packetID]?.Template;

    protected internal override void Dispatch(
      Connection connection,
      int channel,
      ushort packetID,
      int dataLength,
      byte[] data,
      bool compressed)
    {
      this._channels[channel].Dispatch(packetID, (INetworkOperator) connection.Context, connection.Context as T, new ByteBuffer(data, dataLength));
    }

    protected internal override PacketTemplate AcquireExtensionTemplate(int extension) => this._extensions[extension]?.Template;

    protected internal override void Dispatch(
      Connection connection,
      PacketData extension,
      PacketData packet,
      bool isResponse)
    {
      if (isResponse)
        this._extensions[extension.Channel].Response(this._channels[packet.Channel], connection.Context as T, extension, packet);
      else
        this._extensions[extension.Channel].Dispatch(this._channels[packet.Channel], connection.Context as T, extension, packet);
    }

    protected override void OnDisposing(bool disposing)
    {
      this._running = false;
      this._onSocketConnected = (SocketEventHandler) null;
      this._onConnectionCreated = (ConnectionEventHandler) null;
      this._onConnectionDisposed = (ConnectionEventHandler) null;
      this._onContextAttached = (NetworkContextEventHandler<T>) null;
      this._onContextDetached = (NetworkContextEventHandler<T>) null;
    }
  }
}
