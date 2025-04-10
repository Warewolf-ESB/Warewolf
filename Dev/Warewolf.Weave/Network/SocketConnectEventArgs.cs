// Decompiled with JetBrains decompiler
// Type: System.Network.SocketConnectEventArgs
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;
using System.Net.Sockets;

namespace System.Network
{
  public sealed class SocketConnectEventArgs : EventArgs
  {
    private Socket _socket;
    private IPAddress _address;
    private string _addressString;
    private bool _accepted;

    public Socket Socket => this._socket;

    public IPAddress Address => this._address;

    public string AddressString => this._addressString;

    public bool Accepted
    {
      get => this._accepted;
      set => this._accepted = value;
    }

    public SocketConnectEventArgs(Socket socket, IPAddress address, string addressString)
    {
      this._socket = socket;
      this._address = address;
      this._addressString = addressString;
      this._accepted = true;
    }
  }
}
