// Decompiled with JetBrains decompiler
// Type: System.Network.AsyncConnection
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net.Sockets;

namespace System.Network
{
  public sealed class AsyncConnection : Connection
  {
    private const int PendingState = 2;
    private byte[] _recBuffer;
    private object _syncLock;
    private int _state;
    private AsyncCallback _onDataSent;
    private AsyncCallback _onDataReceived;

    public AsyncConnection(NetworkHost host, Socket socket, NetworkDirection direction)
      : base(host, socket, direction)
    {
      this._socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, true);
      this._recBuffer = new byte[8192];
      this._syncLock = new object();
      host.NotifyCreated((Connection) this);
    }

    public override void Start()
    {
      if (this._disposed || this._alive)
        return;
      this._onDataSent = new AsyncCallback(this.OnDataSent);
      this._onDataReceived = new AsyncCallback(this.OnDataReceived);
      this._alive = true;
      bool flag = true;
      try
      {
        lock (this._syncLock)
        {
          if ((this._state & 2) == 0)
            this.BeginReceive();
        }
      }
      catch
      {
        flag = false;
        this.Dispose(true);
      }
      this._wasAlive = flag;
    }

    public override void Send(Packet p)
    {
      if (p == null || this._socket == null)
        return;
      if (p._headerLength == (byte) 0)
        p.Compile();
      if (!this.SendEncrypted(p._header, 0, (int) p._headerLength) || p._dataLength == 0)
        return;
      this.SendInternal(p.Buffer, 0, p._dataLength);
    }

    public override void SendExtended(Packet p, byte[] extension, int extensionLength)
    {
      if (p == null || this._socket == null)
        return;
      if (p._headerLength == (byte) 0)
        p.Compile();
      if (!this.SendEncrypted(Connection.AppendFlags(p._header[0], PacketHeaderFlags.Extended), p._header, 0, (int) p._headerLength) || p._dataLength != 0 && !this.SendInternal(p.Buffer, 0, p._dataLength))
        return;
      this.SendInternal(extension, 0, extensionLength);
    }

    public override void Send(byte[] data, int index, int length)
    {
      if (data == null || length <= 0 || this._socket == null)
        return;
      this.SendInternal(data, index, length);
    }

    public override void SendRange(params Packet[] packets)
    {
      if (packets == null || this._socket == null)
        return;
      byte[] numArray = new byte[8];
      for (int index = 0; index < packets.Length && !this._disposed && this._socket != null; ++index)
      {
        Packet packet;
        if ((packet = packets[index])._headerLength == (byte) 0)
          packet.Compile();
        if (!this.SendEncrypted(packet._header, 0, (int) packet._headerLength) || packet._dataLength != 0 && !this.SendInternal(packet.Buffer, 0, packet._dataLength))
          break;
      }
    }

    private bool SendEncrypted(byte[] data, int index, int length)
    {
      byte[] numArray = new byte[length];
      this._crypt.Encrypt(data, index, numArray, 0, length);
      try
      {
        this._socket.BeginSend(numArray, 0, length, SocketFlags.None, this._onDataSent, (object) null);
      }
      catch
      {
        this.Dispose(true);
        return false;
      }
      return true;
    }

    private bool SendEncrypted(byte extended, byte[] data, int index, int length)
    {
      byte[] numArray = new byte[length];
      numArray[0] = this._crypt.Encrypt(extended);
      this._crypt.Encrypt(data, index + 1, numArray, 1, length - 1);
      try
      {
        this._socket.BeginSend(numArray, 0, length, SocketFlags.None, this._onDataSent, (object) null);
      }
      catch
      {
        this.Dispose(true);
        return false;
      }
      return true;
    }

    private bool SendInternal(byte[] data, int index, int length)
    {
      try
      {
        this._socket.BeginSend(data, index, length, SocketFlags.None, this._onDataSent, (object) null);
      }
      catch
      {
        this.Dispose(true);
        return false;
      }
      return true;
    }

    private void OnDataSent(IAsyncResult asyncResult)
    {
      if (this._socket == null)
        return;
      try
      {
        if (this._socket.EndSend(asyncResult) > 0)
          return;
        this.Dispose(true);
      }
      catch
      {
        this.Dispose(true);
      }
    }

    private void BeginReceive()
    {
      this._state |= 2;
      this._socket.BeginReceive(this._recBuffer, 0, 8192, SocketFlags.None, this._onDataReceived, (object) null);
    }

    private void OnDataReceived(IAsyncResult asyncResult)
    {
      if (this._socket == null)
        return;
      try
      {
        int num = this._socket.EndReceive(asyncResult);
        if (num > 0)
        {
          byte[] recBuffer = this._recBuffer;
          if (this._authBroker != null)
          {
            if (!this._authBroker.NotifyDataReceived(recBuffer, num))
            {
              this.Dispose(true);
              return;
            }
          }
          else if (this._assembler != null && !this._assembler.Assemble(recBuffer, 0, num))
          {
            this.Dispose(true);
            return;
          }
          lock (this._syncLock)
          {
            this._state &= -3;
            this.BeginReceive();
          }
        }
        else
          this.Dispose(true);
      }
      catch
      {
        this.Dispose(true);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      this._disposed = true;
      if (this._socket != null)
        NetworkHelper.ReleaseSocket(ref this._socket);
      this._alive = false;
      this._recBuffer = (byte[]) null;
      this._onDataSent = (AsyncCallback) null;
      this._onDataReceived = (AsyncCallback) null;
      if (!disposing)
        return;
      this._host.NotifyDisposed((Connection) this);
      GC.SuppressFinalize((object) this);
    }
  }
}
