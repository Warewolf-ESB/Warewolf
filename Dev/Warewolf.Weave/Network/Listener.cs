// Decompiled with JetBrains decompiler
// Type: System.Network.Listener
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace System.Network
{
  public sealed class Listener : IDisposable
  {
    private bool _disposed;
    private bool _valid;
    private bool _listening;
    private NetworkServer _server;
    private Socket _listener;
    private IPAddress _address;
    private string _addressString;
    private int _port;
    private int _backlog;

    public bool Disposed => this._disposed;

    public bool Valid => this._valid;

    public bool Listening => this._listening;

    public NetworkServer Server => this._server;

    public IPAddress Address => this._address;

    public int Port => this._port;

    public int Backlog => this._backlog;

    private static Socket Bind(
      NetworkServer host,
      ref string addressString,
      int port,
      int backlog,
      out bool valid,
      out IPAddress address)
    {
      valid = IPAddress.TryParse(addressString, out address);
      if (!valid)
      {
        address = NetworkHelper.GetIPAddress(NetworkHelper.GetNICFromID(addressString), AddressFamily.InterNetwork);
        if (address == IPAddress.None)
          return (Socket) null;
        addressString = address.ToString();
        valid = true;
      }
      IPEndPoint localEP = new IPEndPoint(address, port);
      Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      try
      {
        socket.LingerState.Enabled = false;
        socket.ExclusiveAddressUse = false;
        socket.Bind((EndPoint) localEP);
        socket.Listen(backlog);
        valid = true;
      }
      catch
      {
        NetworkHelper.ReleaseSocket(ref socket);
        valid = false;
      }
      return socket;
    }

    public Listener(NetworkServer server, ListenerConfig configuration)
    {
      this._server = server;
      this._server.Disposing += new Action(this.Dispose);
      this._addressString = configuration.Address;
      this._port = configuration.Port;
      this._backlog = configuration.Backlog;
      this._disposed = false;
      this._listening = false;
      this._valid = true;
    }

		public override string ToString()
		{
			if (!this._valid)
				return "Invalid";
			return $"{this._addressString}:{this._port}";
		}

    public bool Start()
    {
      if (this._disposed || this._listening || !this._valid)
        return false;
      if (this._listener == null)
      {
        this._listener = Listener.Bind(this._server, ref this._addressString, this._port, this._backlog, out this._valid, out this._address);
        if (!this._valid)
          return false;
      }
      this._listening = true;
      try
      {
        SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
        socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnAccept);
        if (!this._listener.AcceptAsync(socketAsyncEventArgs))
          this.OnAccept((object) this, socketAsyncEventArgs);
      }
      catch
      {
        this.Dispose();
        return false;
      }
      this._valid = true;
      return true;
    }

    public void Stop()
    {
      if (this._disposed || !this._listening)
        return;
      this._listening = false;
    }

    private void OnAccept(object sender, SocketAsyncEventArgs args)
    {
      try
      {
        Socket acceptSocket = args.AcceptSocket;
        if (this._disposed || !this._listening)
        {
          NetworkHelper.ReleaseSocket(ref acceptSocket);
        }
        else
        {
          if (acceptSocket == null)
            return;
          this._server.NotifySocketConnected(this, acceptSocket);
        }
      }
      catch (ObjectDisposedException ex)
      {
        this.Dispose();
      }
      catch (SocketException ex)
      {
        this.Dispose();
      }
      catch (Exception ex)
      {
        this.Dispose();
      }
      finally
      {
        if (!this._disposed && this._listener != null && this._listening)
        {
          args.AcceptSocket = (Socket) null;
          if (!this._listener.AcceptAsync(args))
            this.OnAccept((object) this, args);
        }
        else
        {
          try
          {
            args.AcceptSocket = (Socket) null;
            args.Dispose();
          }
          catch
          {
          }
        }
      }
    }

    ~Listener() => this.Dispose(false);

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      this._disposed = true;
      this._valid = this._listening = false;
      if (this._listener != null)
        NetworkHelper.ReleaseSocket(ref this._listener);
      if (!disposing)
        return;
      this._server.Disposing -= new Action(this.Dispose);
      this._server = (NetworkServer) null;
      GC.SuppressFinalize((object) this);
    }
  }
}
