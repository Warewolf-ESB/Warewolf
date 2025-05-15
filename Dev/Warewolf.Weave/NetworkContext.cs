// Decompiled with JetBrains decompiler
// Type: System.NetworkContext
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Network;

namespace System
{
  public abstract class NetworkContext : IIndexed, INetworkOperator
  {
    private string _username;
    private Guid _accountID;
    private Guid _sessionID;
    private NetworkAccount _account;
    protected Version _version;
    protected PlatformID _platform;
    protected string _servicePack;
    protected uint _fingerprint;
    protected bool _attached;
    protected Connection _connection;

    internal Connection Connection => this._connection;

    public Guid AccountID => this._accountID;

    public Guid SessionID => this._sessionID;

    public Version Version => this._version;

    public PlatformID Platform => this._platform;

    public string ServicePack => this._servicePack;

    public uint Fingerprint => this._fingerprint;

    public bool Attached => this._attached;

    int IIndexed.Index { get; set; }

    public override string ToString() => this._username;

    public void Send(Packet p)
    {
      if (p == null || !this._attached || this._connection == null)
        return;
      this._connection.Send(p);
    }

    public void Kill()
    {
      if (this._connection == null)
        return;
      this._connection.Dispose();
    }

    internal void NotifyLogin(
      InboundAuthenticationBroker broker,
      Connection connection,
      NetworkAccount account)
    {
      (this._connection = connection).Context = this;
      this._version = broker.Version;
      this._platform = broker.Platform;
      this._servicePack = broker.ServicePack;
      this._fingerprint = broker.Fingerprint;
      this._username = account.Username;
      this._accountID = account.AccountID;
      this._sessionID = Guid.NewGuid();
      (this._account = account).BindTo(this, connection.Address);
      this._attached = true;
      this.OnAttached(broker, account);
    }

    internal void NotifyLogout(AuthenticationResponse response)
    {
      if (this._attached)
      {
        this._attached = false;
        this._account.Unbind();
        this.OnDetached();
      }
      this._connection.Context = (NetworkContext) null;
      this._connection = (Connection) null;
    }

    internal void NotifyConnectionDisposed()
    {
      if (this._attached)
      {
        this._attached = false;
        this._account.Unbind();
        this.OnDetached();
      }
      this._connection.Context = (NetworkContext) null;
      this._connection = (Connection) null;
    }

    protected virtual void OnAttached(InboundAuthenticationBroker broker, NetworkAccount account)
    {
    }

    protected virtual void OnDetached()
    {
    }
  }
}
