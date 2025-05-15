// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkAccount
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Net;

namespace System.Network
{
  public class NetworkAccount : SerializableEntity
  {
    protected string _username;
    protected byte[] _password;
    protected Guid _accountID;
    protected NetworkContext _owner;
    protected DateTime _blockedTill;
    protected AuthenticationResponse _blockedLoginResponse;
    protected DateTime _createdAt;
    protected DateTime _lastLogin;
    protected DateTime _previousLogin;
    protected TimeSpan _totalTimeLoggedIn;
    protected StandardAccountPermissions _permissions;
    protected List<IPAddress> _loginIPs;

    public string Username
    {
      get => this._username;
      internal set => this._username = value;
    }

    public byte[] Password
    {
      get => this._password;
      set => this.SetPassword(value);
    }

    public Guid AccountID => this._accountID;

    public bool Blocked => this.GetBlocked();

    public AuthenticationResponse BlockedLoginResponse => this._blockedLoginResponse;

    public bool InUse => this.GetInUse();

    public NetworkContext Owner => this._owner;

    public DateTime CreatedAt => this._createdAt;

    public DateTime PreviousLoginTime => this._previousLogin;

    public DateTime CurrentLoginTime => !this.GetInUse() ? DateTime.MinValue : this._lastLogin;

    public TimeSpan TotalTimeLoggedIn => this._totalTimeLoggedIn;

    public List<IPAddress> LoginAddresses => this._loginIPs;

    protected NetworkAccount()
    {
    }

    public NetworkAccount(IByteReaderBase reader)
      : base(reader)
    {
      this._username = reader.ReadString();
      this._password = reader.ReadBytes(reader.ReadInt32());
      this._accountID = reader.ReadGuid();
      this._blockedTill = DateTime.FromOADate(reader.ReadDouble());
      this._blockedLoginResponse = (AuthenticationResponse) reader.ReadByte();
      this._createdAt = DateTime.FromOADate(reader.ReadDouble());
      this._lastLogin = DateTime.FromOADate(reader.ReadDouble());
      this._totalTimeLoggedIn = TimeSpan.FromTicks(reader.ReadInt64());
      this._previousLogin = this._lastLogin;
      StandardAccountPermissions accountPermissions = (StandardAccountPermissions) reader.ReadByte();
      int num = reader.ReadInt32();
      this._loginIPs = new List<IPAddress>();
      for (int index = 0; index < num; ++index)
      {
        IPAddress address;
        if (IPAddress.TryParse(reader.ReadString().Trim(), out address))
          this._loginIPs.Add(address);
      }
      this._owner = (NetworkContext) null;
    }

    public NetworkAccount(string username, byte[] password)
    {
      this._username = username;
      this.Password = password;
      this._accountID = Guid.NewGuid();
      this._blockedTill = DateTime.MinValue;
      this._blockedLoginResponse = AuthenticationResponse.Success;
      this._owner = (NetworkContext) null;
      this._createdAt = DateTime.Now;
      this._lastLogin = DateTime.MinValue;
      this._totalTimeLoggedIn = TimeSpan.Zero;
      this._loginIPs = new List<IPAddress>();
    }

    public override string ToString() => this._username;

    protected virtual bool GetBlocked()
    {
      if (this._blockedTill < DateTime.Now)
        this._blockedTill = DateTime.MinValue;
      return this._blockedTill != DateTime.MinValue;
    }

    protected virtual bool GetInUse()
    {
      if (this._owner == null || this._owner.Attached && this._owner.Connection != null && this._owner.Connection.Alive)
        return false;
      this.Unbind();
      return false;
    }

    protected virtual void SetPassword(byte[] value) => this._password = value;

    public void BlockTill(DateTime blockEnd, AuthenticationResponse blockedLoginResponse)
    {
      this._blockedTill = blockEnd;
      this._blockedLoginResponse = blockedLoginResponse;
      this.NotifyBlocked();
    }

    public void BlockFor(TimeSpan blockDuration, AuthenticationResponse blockedLoginResponse)
    {
      this._blockedTill = DateTime.Now.Add(blockDuration);
      this._blockedLoginResponse = blockedLoginResponse;
      this.NotifyBlocked();
    }

    public void BlockIndefinatly(AuthenticationResponse blockedLoginResponse)
    {
      this._blockedTill = DateTime.MaxValue;
      this._blockedLoginResponse = blockedLoginResponse;
      this.NotifyBlocked();
    }

    public void Unblock()
    {
      this._blockedTill = DateTime.MinValue;
      this._blockedLoginResponse = AuthenticationResponse.Success;
    }

    protected virtual void NotifyBlocked()
    {
      if (!this.InUse || this._owner.Connection == null)
        return;
      this._owner.Connection.Dispose();
    }

    public virtual void BindTo(NetworkContext client, IPAddress address)
    {
      this._owner = client;
      this._previousLogin = this._lastLogin;
      this._lastLogin = DateTime.Now;
      bool flag = false;
      for (int index = 0; index < this._loginIPs.Count; ++index)
      {
        if (this._loginIPs[index].Equals((object) address))
        {
          flag = true;
          break;
        }
      }
      if (flag)
        return;
      this._loginIPs.Add(address);
    }

    public virtual void Unbind()
    {
      this._totalTimeLoggedIn += DateTime.Now - this._lastLogin;
      this._owner = (NetworkContext) null;
    }

    public bool HasAllPermissions(StandardAccountPermissions permissions) => (this._permissions & permissions) == permissions;

    public bool HasAnyPermissions(StandardAccountPermissions permissions) => (this._permissions & permissions) != 0;

    public void SetPermissions(StandardAccountPermissions permissions, bool hasPermissions)
    {
      if (hasPermissions)
        this._permissions |= permissions;
      else
        this._permissions &= ~permissions;
    }

    public virtual bool CheckCredentials(IPAddress source, string username, byte[] password) => this.ValidateCredentials(username, password);

    public bool AccessedBy(IPAddress address) => this._loginIPs.Contains(address);

    protected virtual bool ValidateCredentials(string username, byte[] password) => this._username.Equals(username, StringComparison.OrdinalIgnoreCase) && WeaveUtility.Equals(this._password, password);

    protected override void Serialize(IByteWriterBase writer)
    {
      writer.Write(this._username);
      writer.Write(this._password.Length);
      writer.Write(this._password);
      writer.Write(this._accountID);
      writer.Write(this._blockedTill.ToOADate());
      writer.Write((byte) this._blockedLoginResponse);
      writer.Write(this._createdAt.ToOADate());
      writer.Write(this._lastLogin.ToOADate());
      writer.Write(this._totalTimeLoggedIn.Ticks);
      writer.Write((byte) this._permissions);
      writer.Write(this._loginIPs.Count);
      for (int index = 0; index < this._loginIPs.Count; ++index)
        writer.Write(this._loginIPs[index].ToString());
    }
  }
}
