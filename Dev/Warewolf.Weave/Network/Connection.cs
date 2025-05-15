// Decompiled with JetBrains decompiler
// Type: System.Network.Connection
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;
using System.Net.Sockets;

namespace System.Network
{
  public abstract class Connection : INetworkOperator, IDisposable
  {
    private const byte PHFMask = 15;
    private const byte PHIMask = 240;
    protected bool _disposed;
    protected bool _alive;
    protected NetworkHost _host;
    protected Socket _socket;
    protected NetworkDirection _direction;
    protected IPAddress _address;
    protected int _port;
    protected string _addressString;
    protected PacketAssembler _assembler;
    protected AuthenticationBroker _authBroker;
    protected ICryptProvider _crypt;
    protected NetworkContext _context;
    protected bool _wasAlive;

    public AuthenticationBroker AuthBroker
    {
      get => this._authBroker;
      set => this._authBroker = value;
    }

    public ICryptProvider Crypt
    {
      get => this._crypt;
      set => this._crypt = value;
    }

    public PacketAssembler Assembler
    {
      get => this._assembler;
      set => this._assembler = value;
    }

    internal bool WasAlive => this._wasAlive;

    public bool Disposed => this._disposed;

    public bool Alive => this._alive;

    public NetworkHost Host => this._host;

    public NetworkDirection Direction => this._direction;

    public NetworkContext Context
    {
      get => this._context;
      set => this._context = value;
    }

    public IPAddress Address => this._address;

    public int Port => this._port;

    public Socket Socket => this._socket;

    protected static byte AppendFlags(byte encoded, PacketHeaderFlags flags)
    {
      int num = (int) encoded & -241;
      flags |= (PacketHeaderFlags) ((uint) encoded & 4294967280U);
      return (byte) ((PacketHeaderFlags) ((int) (byte) num & -241) | flags & (PacketHeaderFlags.Extended | PacketHeaderFlags.Identifier16 | PacketHeaderFlags.Length16 | PacketHeaderFlags.Length32));
    }

    public Connection(NetworkHost host, Socket socket, NetworkDirection direction)
    {
      this._crypt = (ICryptProvider) NullCryptProvider.Singleton;
      this._host = host;
      this._socket = socket;
      this._direction = direction;
      try
      {
        IPEndPoint remoteEndPoint = (IPEndPoint) this._socket.RemoteEndPoint;
        this._address = remoteEndPoint.Address;
        this._port = remoteEndPoint.Port;
        this._addressString = this._address.ToString();
      }
      catch
      {
        this._address = IPAddress.None;
        this._port = -1;
        this._addressString = "(error)";
      }
    }

    public override string ToString() => this._addressString;

    public abstract void Start();

    public abstract void Send(Packet p);

    public abstract void Send(byte[] data, int index, int length);

    public abstract void SendRange(params Packet[] packets);

    public abstract void SendExtended(Packet p, byte[] extension, int extensionLength);

    ~Connection() => this.Dispose(false);

    public void Dispose() => this.Dispose(true);

    protected abstract void Dispose(bool disposing);

    protected sealed class PendingData
    {
      public byte[] Header;
      public byte[] Data;
      public int Index;
      public int Length;

      public PendingData(byte[] header, byte[] data, int index, int length)
      {
        this.Header = header;
        this.Data = data;
        this.Index = index;
        this.Length = length;
      }
    }
  }
}
