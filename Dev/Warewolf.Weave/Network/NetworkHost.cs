// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkHost
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace System.Network
{
  public abstract class NetworkHost
  {
    private bool _disposed;
    protected bool _disposing;
    private string _name;
    private Action _onDisposing;
    protected ReaderWriterLockSlim _connectionLock;
    protected List<Connection> _connections;
    internal SynchronizedQueue<NetworkAsyncEventArgs> SocketPool;

    public bool Disposed => this._disposed;

    public string Name => this._name;

    public List<Connection> Connections => this._connections;

    public event Action Disposing
    {
      add
      {
        if (this._disposing)
          return;
        this._onDisposing += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onDisposing -= value;
      }
    }

    public NetworkHost(string name)
    {
      this._name = name;
      this._connectionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this._connections = new List<Connection>();
      this.SocketPool = new SynchronizedQueue<NetworkAsyncEventArgs>();
    }

    public override string ToString() => this._name;

    protected internal abstract bool ValidateChannel(int channel);

    protected internal abstract PacketTemplate AcquirePacketTemplate(int channel, ushort packetID);

    protected internal abstract void Dispatch(
      Connection connection,
      int channel,
      ushort packetID,
      int dataLength,
      byte[] data,
      bool compressed);

    protected internal virtual PacketTemplate AcquireExtensionTemplate(int extension) => (PacketTemplate) null;

    protected internal virtual void Dispatch(
      Connection connection,
      PacketData extension,
      PacketData packet,
      bool isResponse)
    {
    }

    public void Disconnect() => this.Disconnect(NetworkDirection.Bidirectional);

    public void Disconnect(NetworkDirection direction)
    {
      if (this._disposed)
        return;
      Connection[] connectionArray = (Connection[]) null;
      try
      {
        this._connectionLock.EnterReadLock();
        connectionArray = this._connections.ToArray();
      }
      catch
      {
      }
      finally
      {
        this._connectionLock.ExitReadLock();
      }
      for (int index = 0; index < connectionArray.Length; ++index)
      {
        if ((connectionArray[index].Direction & direction) != NetworkDirection.None)
          connectionArray[index].Dispose();
      }
    }

    internal void NotifyCreated(Connection connection)
    {
      if (this._disposed)
        return;
      bool flag = false;
      try
      {
        this._connectionLock.EnterWriteLock();
        this._connections.Add(connection);
        flag = true;
      }
      catch
      {
        connection.Dispose();
      }
      finally
      {
        this._connectionLock.ExitWriteLock();
      }
      if (!flag)
        return;
      this.OnConnectionCreated(connection);
    }

    internal void NotifyAuthenticationFailed(
      Connection connection,
      OutboundAuthenticationBroker broker,
      AuthenticationResponse reason,
      bool expectDisconnect)
    {
      this.OnLoginFailed(connection, broker, reason, expectDisconnect);
    }

    internal void NotifyAuthenticated(
      Connection connection,
      NetworkAccount account,
      AuthenticationBroker broker)
    {
      connection.Crypt = broker.CryptProvider;
      connection.Assembler = new PacketAssembler(connection, (IDecryptionProvider) broker.CryptProvider);
      connection.AuthBroker = (AuthenticationBroker) null;
      this.OnLoginSuccess(connection, account, broker);
    }

    internal virtual void NotifyDeauthenticated(
      Connection connection,
      InboundAuthenticationBroker broker,
      AuthenticationResponse reason)
    {
    }

    internal void NotifyDisposed(Connection connection)
    {
      if (this._disposed)
        return;
      try
      {
        this._connectionLock.EnterWriteLock();
        this._connections.Remove(connection);
      }
      catch
      {
        connection.Dispose();
      }
      finally
      {
        this._connectionLock.ExitWriteLock();
      }
      this.OnConnectionDisposed(connection);
    }

    protected abstract void OnLoginSuccess(
      Connection connection,
      NetworkAccount account,
      AuthenticationBroker broker);

    protected abstract void OnLoginFailed(
      Connection connection,
      OutboundAuthenticationBroker broker,
      AuthenticationResponse reason,
      bool expectDisconnect);

    protected virtual void OnConnectionCreated(Connection connection)
    {
    }

    protected virtual void OnConnectionDisposed(Connection connection)
    {
    }

    ~NetworkHost() => this.Dispose(false);

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (this._disposing)
        return;
      this._disposing = true;
      if (this._onDisposing != null)
      {
        this._onDisposing();
        this._onDisposing = (Action) null;
      }
      this.Disconnect(NetworkDirection.Bidirectional);
      this._disposed = true;
      this.OnDisposing(disposing);
      this._connectionLock.Dispose();
      this._connectionLock = (ReaderWriterLockSlim) null;
      this._connections = (List<Connection>) null;
      this.OnDisposed(disposing);
      SocketAsyncEventArgs socketAsyncEventArgs;
      while ((NetworkAsyncEventArgs) (socketAsyncEventArgs = (SocketAsyncEventArgs) this.SocketPool.TryDequeue()) != null)
        socketAsyncEventArgs.Dispose();
      this.SocketPool = (SynchronizedQueue<NetworkAsyncEventArgs>) null;
      if (!disposing)
        return;
      GC.SuppressFinalize((object) this);
    }

    protected virtual void OnDisposing(bool disposing)
    {
    }

    protected virtual void OnDisposed(bool disposing)
    {
    }
  }
}
