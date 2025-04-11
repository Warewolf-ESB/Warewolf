// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkServer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;
using System.Net.Sockets;

namespace System.Network
{
  public abstract class NetworkServer : NetworkHost
  {
    public NetworkServer(string name)
      : base(name)
    {
    }

    internal void NotifySocketConnected(Listener source, Socket socket)
    {
      IPAddress none = IPAddress.None;
      IPAddress address;
      int port;
      string addressString;
      try
      {
        IPEndPoint remoteEndPoint = (IPEndPoint) socket.RemoteEndPoint;
        address = remoteEndPoint.Address;
        port = remoteEndPoint.Port;
        addressString = address.ToString();
      }
      catch
      {
        address = IPAddress.None;
        port = -1;
        addressString = "(error)";
      }
      if (!this.ValidateSocketConnection(source, socket, address, addressString, port))
      {
        NetworkHelper.ReleaseSocket(ref socket);
      }
      else
      {
        try
        {
          this.ApplySocketOptions(source, socket);
          this.OnSocketConnectionAccepted(source, socket, address, addressString, port);
        }
        catch
        {
          NetworkHelper.ReleaseSocket(ref socket);
        }
      }
    }

    protected abstract bool ValidateSocketConnection(
      Listener source,
      Socket socket,
      IPAddress address,
      string addressString,
      int port);

    protected abstract void ApplySocketOptions(Listener source, Socket socket);

    protected abstract void OnSocketConnectionAccepted(
      Listener source,
      Socket socket,
      IPAddress address,
      string addressString,
      int port);

    internal override sealed void NotifyDeauthenticated(
      Connection connection,
      InboundAuthenticationBroker broker,
      AuthenticationResponse reason)
    {
      this.OnLogoutRequired(connection, broker, reason);
    }

    protected abstract void OnLogoutRequired(
      Connection connection,
      InboundAuthenticationBroker broker,
      AuthenticationResponse reason);
  }
}
