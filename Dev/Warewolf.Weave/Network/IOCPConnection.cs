// Decompiled with JetBrains decompiler
// Type: System.Network.IOCPConnection
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net.Sockets;

namespace System.Network
{
  public sealed class IOCPConnection : Connection
  {
    private EventHandler<SocketAsyncEventArgs> _onDataSent;
    private EventHandler<SocketAsyncEventArgs> _onDataReceived;

    public IOCPConnection(NetworkHost host, Socket socket, NetworkDirection direction)
      : base(host, socket, direction)
    {
      this.AcquireOutbound();
      host.NotifyCreated((Connection) this);
    }

    public override void Start()
    {
      if (this._disposed || this._alive)
        return;
      this._onDataSent = new EventHandler<SocketAsyncEventArgs>(this.OnDataSent);
      this._onDataReceived = new EventHandler<SocketAsyncEventArgs>(this.OnDataReceived);
      this._alive = true;
      this._wasAlive = true;
      try
      {
        this.BeginReceive();
      }
      catch
      {
        this._wasAlive = false;
        this.Dispose(true);
      }
    }

    public override void SendExtended(Packet p, byte[] extension, int extensionLength)
    {
      if (p == null || this._socket == null || extension == null || extensionLength <= 0)
        return;
      if (p._headerLength == (byte) 0)
        p.Compile();
      byte encoded = p._header[0];
      p._header[0] = Connection.AppendFlags(encoded, PacketHeaderFlags.Extended);
      NetworkAsyncEventArgs args1 = this.SendEncrypted((NetworkAsyncEventArgs) null, p._header, 0, (int) p._headerLength);
      p._header[0] = encoded;
      if (this._socket == null || this._disposed)
      {
        this.ReleaseOutbound(ref args1);
      }
      else
      {
        if (p._dataLength != 0)
          args1 = this.SendInternal(args1, p.Buffer, 0, p._dataLength);
        if (this._socket == null || this._disposed)
        {
          this.ReleaseOutbound(ref args1);
        }
        else
        {
          NetworkAsyncEventArgs args2 = this.SendInternal(args1, extension, 0, extensionLength);
          if (this._socket == null || this._disposed)
          {
            this.ReleaseOutbound(ref args2);
          }
          else
          {
            if (args2 == null)
              return;
            if (args2.Index > 0)
            {
              NetworkAsyncEventArgs args3 = args2;
              try
              {
                int index = args3.Index;
                args3.Index = 0;
                args3.SetBuffer(args3.BufferInternal, 0, index);
                if (this._socket.SendAsync((SocketAsyncEventArgs) args3))
                  return;
                this.ReleaseOutbound(ref args3);
              }
              catch
              {
                this.ReleaseOutbound(ref args3);
                this.Dispose(true);
              }
            }
            else
              this.ReleaseOutbound(ref args2);
          }
        }
      }
    }

    public override void Send(Packet p)
    {
      if (p == null || this._socket == null)
        return;
      if (p._headerLength == (byte) 0)
        p.Compile();
      NetworkAsyncEventArgs args1 = this.SendEncrypted((NetworkAsyncEventArgs) null, p._header, 0, (int) p._headerLength);
      if (this._socket == null || this._disposed)
      {
        this.ReleaseOutbound(ref args1);
      }
      else
      {
        if (p._dataLength != 0)
          args1 = this.SendInternal(args1, p.Buffer, 0, p._dataLength);
        if (this._socket == null || this._disposed)
        {
          this.ReleaseOutbound(ref args1);
        }
        else
        {
          if (args1 == null)
            return;
          if (args1.Index > 0)
          {
            NetworkAsyncEventArgs args2 = args1;
            try
            {
              int index = args2.Index;
              args2.Index = 0;
              args2.SetBuffer(args2.BufferInternal, 0, index);
              if (this._socket.SendAsync((SocketAsyncEventArgs) args2))
                return;
              this.ReleaseOutbound(ref args2);
            }
            catch
            {
              this.ReleaseOutbound(ref args2);
              this.Dispose(true);
            }
          }
          else
            this.ReleaseOutbound(ref args1);
        }
      }
    }

    public override void Send(byte[] data, int index, int length)
    {
      if (data == null || length <= 0 || this._socket == null)
        return;
      NetworkAsyncEventArgs args1 = this.SendInternal((NetworkAsyncEventArgs) null, data, index, length);
      if (this._socket == null || this._disposed)
      {
        this.ReleaseOutbound(ref args1);
      }
      else
      {
        if (args1 == null)
          return;
        if (args1.Index > 0)
        {
          NetworkAsyncEventArgs args2 = args1;
          try
          {
            index = args2.Index;
            args2.Index = 0;
            args2.SetBuffer(args2.BufferInternal, 0, index);
            if (this._socket.SendAsync((SocketAsyncEventArgs) args2))
              return;
            this.ReleaseOutbound(ref args2);
          }
          catch
          {
            this.ReleaseOutbound(ref args2);
            this.Dispose(true);
          }
        }
        else
          this.ReleaseOutbound(ref args1);
      }
    }

    public override void SendRange(params Packet[] packets)
    {
      if (packets == null || this._socket == null)
        return;
      NetworkAsyncEventArgs args1 = (NetworkAsyncEventArgs) null;
      for (int index = 0; index < packets.Length; ++index)
      {
        if (this._socket == null || this._disposed)
        {
          this.ReleaseOutbound(ref args1);
          return;
        }
        Packet packet;
        if ((packet = packets[index])._headerLength == (byte) 0)
          packet.Compile();
        args1 = this.SendEncrypted(args1, packet._header, 0, (int) packet._headerLength);
        if (this._socket == null || this._disposed)
        {
          this.ReleaseOutbound(ref args1);
          return;
        }
        if (packet._dataLength != 0)
          args1 = this.SendInternal(args1, packet.Buffer, 0, packet._dataLength);
      }
      if (this._socket == null || this._disposed)
      {
        this.ReleaseOutbound(ref args1);
      }
      else
      {
        if (args1 == null)
          return;
        if (args1.Index > 0)
        {
          NetworkAsyncEventArgs args2 = args1;
          try
          {
            int index = args2.Index;
            args2.Index = 0;
            args2.SetBuffer(args2.BufferInternal, 0, index);
            if (this._socket.SendAsync((SocketAsyncEventArgs) args2))
              return;
            this.ReleaseOutbound(ref args2);
          }
          catch
          {
            this.ReleaseOutbound(ref args2);
            this.Dispose(true);
          }
        }
        else
          this.ReleaseOutbound(ref args1);
      }
    }

    private NetworkAsyncEventArgs SendEncrypted(
      NetworkAsyncEventArgs sendBuffer,
      byte[] data,
      int index,
      int length)
    {
      while (index != length)
      {
        if (sendBuffer == null)
          sendBuffer = this.AcquireOutbound();
        int count1 = length - index;
        int count2 = 8192 - sendBuffer.Index;
        int num = count1 - count2;
        bool flag = false;
        if (num >= 0)
        {
          this._crypt.Encrypt(data, index, sendBuffer.BufferInternal, sendBuffer.Index, count2);
          index += count2;
          flag = true;
        }
        else
        {
          this._crypt.Encrypt(data, index, sendBuffer.BufferInternal, sendBuffer.Index, count1);
          sendBuffer.Index += count1;
          index += count1;
        }
        if (flag)
        {
          NetworkAsyncEventArgs args = sendBuffer;
          sendBuffer = (NetworkAsyncEventArgs) null;
          try
          {
            args.Index = 0;
            args.SetBuffer(args.BufferInternal, 0, 8192);
            if (!this._socket.SendAsync((SocketAsyncEventArgs) args))
              this.ReleaseOutbound(ref args);
          }
          catch
          {
            this.ReleaseOutbound(ref args);
            this.Dispose(true);
            return (NetworkAsyncEventArgs) null;
          }
        }
      }
      return sendBuffer;
    }

    private NetworkAsyncEventArgs SendInternal(
      NetworkAsyncEventArgs sendBuffer,
      byte[] data,
      int index,
      int length)
    {
      while (index != length)
      {
        if (sendBuffer == null)
          sendBuffer = this.AcquireOutbound();
        int count1 = length - index;
        int count2 = 8192 - sendBuffer.Index;
        int num = count1 - count2;
        bool flag = false;
        if (num >= 0)
        {
          Buffer.BlockCopy((Array) data, index, (Array) sendBuffer.BufferInternal, sendBuffer.Index, count2);
          index += count2;
          flag = true;
        }
        else
        {
          Buffer.BlockCopy((Array) data, index, (Array) sendBuffer.BufferInternal, sendBuffer.Index, count1);
          sendBuffer.Index += count1;
          index += count1;
        }
        if (flag)
        {
          NetworkAsyncEventArgs args = sendBuffer;
          sendBuffer = (NetworkAsyncEventArgs) null;
          try
          {
            args.Index = 0;
            args.SetBuffer(args.BufferInternal, 0, 8192);
            if (!this._socket.SendAsync((SocketAsyncEventArgs) args))
              this.ReleaseOutbound(ref args);
          }
          catch
          {
            this.ReleaseOutbound(ref args);
            this.Dispose(true);
            return (NetworkAsyncEventArgs) null;
          }
        }
      }
      return sendBuffer;
    }

    private void OnDataSent(object sender, SocketAsyncEventArgs args)
    {
      NetworkAsyncEventArgs args1 = args as NetworkAsyncEventArgs;
      this.ReleaseOutbound(ref args1);
    }

    private void BeginReceive()
    {
      if (this._socket == null || this._disposed)
        return;
      NetworkAsyncEventArgs networkAsyncEventArgs = this.AcquireInbound();
      if (this._socket.ReceiveAsync((SocketAsyncEventArgs) networkAsyncEventArgs))
        return;
      this.OnDataReceived((object) this, (SocketAsyncEventArgs) networkAsyncEventArgs);
    }

    private void OnDataReceived(object sender, SocketAsyncEventArgs args)
    {
      NetworkAsyncEventArgs args1 = args as NetworkAsyncEventArgs;
      if (this._socket == null)
      {
        this.ReleaseInbound(ref args1);
      }
      else
      {
        try
        {
          int bytesTransferred = args1.BytesTransferred;
          if (bytesTransferred > 0)
          {
            if (this._authBroker != null)
            {
              if (!this._authBroker.NotifyDataReceived(args1.BufferInternal, bytesTransferred))
                this.Dispose(true);
              else
                this.BeginReceive();
            }
            else
            {
              if (this._assembler == null)
                return;
              if (!this._assembler.Assemble(args1.BufferInternal, 0, bytesTransferred))
                this.Dispose(true);
              else
                this.BeginReceive();
            }
          }
          else
            this.Dispose(true);
        }
        catch (ObjectDisposedException ex)
        {
          this.Dispose(true);
        }
        catch (Exception ex)
        {
          this.Dispose(true);
        }
        finally
        {
          this.ReleaseInbound(ref args1);
        }
      }
    }

    private NetworkAsyncEventArgs AcquireOutbound()
    {
      NetworkAsyncEventArgs networkAsyncEventArgs = this._host.SocketPool.TryDequeue();
      if (networkAsyncEventArgs == null)
        networkAsyncEventArgs = new NetworkAsyncEventArgs(8192);
      else
        networkAsyncEventArgs.Index = 0;
      networkAsyncEventArgs.Completed += this._onDataSent;
      networkAsyncEventArgs.UserToken = (object) this;
      return networkAsyncEventArgs;
    }

    private NetworkAsyncEventArgs AcquireInbound()
    {
      NetworkAsyncEventArgs networkAsyncEventArgs = this._host.SocketPool.TryDequeue() ?? new NetworkAsyncEventArgs(8192);
      networkAsyncEventArgs.SetBuffer(networkAsyncEventArgs.BufferInternal, 0, 8192);
      networkAsyncEventArgs.Completed += this._onDataReceived;
      networkAsyncEventArgs.UserToken = (object) this;
      return networkAsyncEventArgs;
    }

    private void ReleaseOutbound(ref NetworkAsyncEventArgs args)
    {
      if (args == null)
        return;
      args.Completed -= this._onDataSent;
      if (this._host.SocketPool != null)
        this._host.SocketPool.Enqueue(args);
      else
        args.Dispose();
      args = (NetworkAsyncEventArgs) null;
    }

    private void ReleaseInbound(ref NetworkAsyncEventArgs args)
    {
      if (args == null)
        return;
      args.Completed -= this._onDataReceived;
      if (this._host.SocketPool != null)
        this._host.SocketPool.Enqueue(args);
      else
        args.Dispose();
      args = (NetworkAsyncEventArgs) null;
    }

    protected override void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      this._disposed = true;
      if (this._socket != null)
        NetworkHelper.ReleaseSocket(ref this._socket);
      this._alive = false;
      if (!disposing)
        return;
      this._host.NotifyDisposed((Connection) this);
      GC.SuppressFinalize((object) this);
    }
  }
}
