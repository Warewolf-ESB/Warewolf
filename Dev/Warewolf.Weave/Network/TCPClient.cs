// Decompiled with JetBrains decompiler
// Type: System.Network.TCPClient
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;
using System.Net.Sockets;

namespace System.Network
{
  public class TCPClient : NetworkHost
  {
    private NetworkStateEventHandler _onNetworkStateChanged;
    private LoginStateEventHandler _onLoginStateChanged;
    protected AsyncPacketHandlerCollection[] _channels;
    protected AsyncExtensionHandler[] _extensions;
    private volatile bool _loggedIn;
    private volatile NetworkState _networkState;
    private volatile Connection _primaryConnection;

    public AsyncPacketHandlerCollection[] Channels => this._channels;

    public AsyncExtensionHandler[] Extensions => this._extensions;

    public NetworkState NetworkState => this._networkState;

    public bool LoggedIn => this._loggedIn;

    public event NetworkStateEventHandler NetworkStateChanged
    {
      add
      {
        if (this._disposing)
          return;
        this._onNetworkStateChanged += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onNetworkStateChanged -= value;
      }
    }

    public event LoginStateEventHandler LoginStateChanged
    {
      add
      {
        if (this._disposing)
          return;
        this._onLoginStateChanged += value;
      }
      remove
      {
        if (this._disposing)
          return;
        this._onLoginStateChanged -= value;
      }
    }

    public TCPClient(string name)
      : base(name)
    {
      this._channels = new AsyncPacketHandlerCollection[16];
      this._extensions = new AsyncExtensionHandler[16];
    }

    public void SetNetworkState(NetworkState state) => this.SetNetworkState(state, false, "");

    public void SetNetworkState(NetworkState state, string message) => this.SetNetworkState(state, true, message);

    public void SetNetworkState(NetworkState state, bool isError, string message)
    {
      NetworkState networkState = this._networkState;
      this._networkState = state;
      if (this._onNetworkStateChanged != null)
        this._onNetworkStateChanged((NetworkHost) this, new NetworkStateEventArgs(networkState, state, isError, message));
      if (state != NetworkState.Offline || !this._loggedIn)
        return;
      this._loggedIn = false;
      if (this._onLoginStateChanged == null)
        return;
      this._onLoginStateChanged((NetworkHost) this, new LoginStateEventArgs(AuthenticationResponse.Logout, false));
    }

    public void Login(OutboundAuthenticationBroker broker)
    {
      Connection primaryConnection = this._primaryConnection;
      if (primaryConnection == null || !primaryConnection.Alive)
      {
        if (this._onLoginStateChanged != null)
          this._onLoginStateChanged((NetworkHost) this, new LoginStateEventArgs(AuthenticationResponse.Unspecified, false, true, "You must be connected to the server in order to login."));
        this._loggedIn = false;
      }
      else
      {
        if (this._loggedIn)
          return;
        broker.BeginAuthentication(primaryConnection);
      }
    }

    public void Logout()
    {
      Connection primaryConnection = this._primaryConnection;
      if (primaryConnection == null || !primaryConnection.Alive)
      {
        if (this._onLoginStateChanged != null)
          this._onLoginStateChanged((NetworkHost) this, new LoginStateEventArgs(AuthenticationResponse.Unspecified, false, true, "You must be connected to the server in order to logout."));
        this._loggedIn = false;
      }
      else
      {
        int num = this._loggedIn ? 1 : 0;
      }
    }

    protected override void OnLoginSuccess(
      Connection connection,
      NetworkAccount account,
      AuthenticationBroker broker)
    {
      this._loggedIn = true;
      if (this._onLoginStateChanged == null)
        return;
      this._onLoginStateChanged((NetworkHost) this, new LoginStateEventArgs(AuthenticationResponse.Success, false));
    }

    protected override void OnLoginFailed(
      Connection connection,
      OutboundAuthenticationBroker broker,
      AuthenticationResponse reason,
      bool expectDisconnect)
    {
      this._loggedIn = false;
      if (this._onLoginStateChanged == null)
        return;
      this._onLoginStateChanged((NetworkHost) this, new LoginStateEventArgs(reason, expectDisconnect));
    }

    public void Connect(string hostNameOrAddress, int port)
    {
      if (this._networkState != NetworkState.Offline)
        return;
      this.SetNetworkState(NetworkState.Connecting);
      IPAddress address = (IPAddress) null;
      if (IPAddress.TryParse(hostNameOrAddress, out address))
      {
        Socket state = (Socket) null;
        try
        {
          state = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          state.BeginConnect((EndPoint) new IPEndPoint(address, port), new AsyncCallback(this.OnEndConnect), (object) state);
        }
        catch (Exception ex)
        {
          try
          {
            state.Shutdown(SocketShutdown.Both);
          }
          catch
          {
          }
          try
          {
            state.Close();
          }
          catch
          {
          }
          this.SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + ex.Message);
        }
      }
      else
      {
        Envoy envoy = new Envoy(hostNameOrAddress, port);
        envoy.EnvoyReturned += new EnvoyReturnedEventHandler(this.OnEnvoyReturned);
        Exception exception = envoy.BeginResolution();
        if (exception == null)
          return;
        envoy.EnvoyReturned -= new EnvoyReturnedEventHandler(this.OnEnvoyReturned);
        envoy.Dispose();
        this.SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + exception.Message);
      }
    }

    private void OnEnvoyReturned(
      Envoy envoy,
      bool successful,
      string sourceHostNameOrAddress,
      IPHostEntry resolvedHostEntry)
    {
      int port = envoy.Port;
      envoy.EnvoyReturned -= new EnvoyReturnedEventHandler(this.OnEnvoyReturned);
      envoy.Dispose();
      if (!successful)
      {
        this.SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
      }
      else
      {
        Socket state = (Socket) null;
        try
        {
          state = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          IPAddress address = (IPAddress) null;
          for (int index = 0; index < resolvedHostEntry.AddressList.Length; ++index)
          {
            if (resolvedHostEntry.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
            {
              address = resolvedHostEntry.AddressList[index];
              break;
            }
          }
          if (address == null)
          {
            this.SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : The specified server could not be found.");
            try
            {
              state.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            try
            {
              state.Close();
            }
            catch
            {
            }
          }
          else
            state.BeginConnect((EndPoint) new IPEndPoint(address, port), new AsyncCallback(this.OnEndConnect), (object) state);
        }
        catch (Exception ex)
        {
          try
          {
            state.Shutdown(SocketShutdown.Both);
          }
          catch
          {
          }
          try
          {
            state.Close();
          }
          catch
          {
          }
          this.SetNetworkState(NetworkState.Offline, true, "A connection to the server could not be established. Reason : " + ex.Message);
        }
      }
    }

    private void OnEndConnect(IAsyncResult asyncResult)
    {
      Socket socket = (Socket) null;
      try
      {
        socket = (Socket) asyncResult.AsyncState;
        socket.EndConnect(asyncResult);
      }
      catch (Exception ex)
      {
        try
        {
          socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
        }
        try
        {
          socket.Close();
        }
        catch
        {
        }
        this.SetNetworkState(NetworkState.Offline, false, "A connection to the server could not be established. Reason : " + ex.Message);
        return;
      }
      this.ConstructConnection(socket);
    }

    private void ConstructConnection(Socket socket)
    {
      if (this._disposing)
      {
        NetworkHelper.ReleaseSocket(ref socket);
      }
      else
      {
        bool flag = false;
        try
        {
          Connection connection = (Connection) new AsyncConnection((NetworkHost) this, socket, NetworkDirection.Outbound);
          connection.Start();
          if (connection.Alive)
          {
            if (this._primaryConnection == null)
            {
              this._primaryConnection = connection;
              this.SetNetworkState(NetworkState.Online);
            }
            else if (this.NetworkState == NetworkState.Offline)
              connection.Dispose();
          }
          else
          {
            connection.Dispose();
            if (this._primaryConnection == null)
              flag = true;
          }
        }
        catch
        {
          NetworkHelper.ReleaseSocket(ref socket);
        }
        if (!flag)
          return;
        this.Disconnect(NetworkDirection.Bidirectional);
      }
    }

    protected override void OnConnectionDisposed(Connection connection)
    {
      if (connection != this._primaryConnection)
        return;
      this._primaryConnection = (Connection) null;
      this.Disconnect(NetworkDirection.Bidirectional);
      this.SetNetworkState(NetworkState.Offline);
    }

    public void Send(byte[] data, int index, int length)
    {
      if (this._primaryConnection == null)
        return;
      this._primaryConnection.Send(data, index, length);
    }

    public void Send(Packet p)
    {
      if (this._primaryConnection == null)
        return;
      this._primaryConnection.Send(p);
    }

    public void SendExtended(Packet p, byte[] extension, int extensionLength)
    {
      if (this._primaryConnection == null)
        return;
      this._primaryConnection.SendExtended(p, extension, extensionLength);
    }

    public void SendRange(params Packet[] p)
    {
      if (this._primaryConnection == null)
        return;
      this._primaryConnection.SendRange(p);
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
      this._channels[channel].Dispatch(packetID, (INetworkOperator) connection, new ByteBuffer(data, dataLength));
    }

    protected internal override PacketTemplate AcquireExtensionTemplate(int extension) => this._extensions[extension]?.Template;

    protected internal override void Dispatch(
      Connection connection,
      PacketData extension,
      PacketData packet,
      bool isResponse)
    {
      if (isResponse)
        this._extensions[extension.Channel].Response(this._channels[packet.Channel], connection, extension, packet);
      else
        this._extensions[extension.Channel].Dispatch(this._channels[packet.Channel], connection, extension, packet);
    }

    protected override void OnDisposed(bool disposing)
    {
      this._primaryConnection = (Connection) null;
      this._onNetworkStateChanged = (NetworkStateEventHandler) null;
      this._onLoginStateChanged = (LoginStateEventHandler) null;
    }
  }
}
