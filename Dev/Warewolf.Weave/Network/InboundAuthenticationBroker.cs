// Decompiled with JetBrains decompiler
// Type: System.Network.InboundAuthenticationBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public abstract class InboundAuthenticationBroker : AuthenticationBroker
  {
    protected Firewall _firewall;
    protected int _localIdentifier;
    protected int _targetIdentifier;
    protected Version _localVersion;
    protected Version _targetVersion;
    protected PlatformID _targetPlatform;
    protected string _targetServicePack;
    protected uint _targetFingerprint;
    protected string _targetHostname;
    protected string _targetAccountName;
    protected NetworkAccount _targetAccount;

    public int Identifier => this._targetIdentifier;

    public Version Version => this._targetVersion;

    public PlatformID Platform => this._targetPlatform;

    public string ServicePack => this._targetServicePack;

    public uint Fingerprint => this._targetFingerprint;

    public string Hostname => this._targetHostname;

    internal InboundAuthenticationBroker Instantiate(Firewall firewall, Connection connection)
    {
      InboundAuthenticationBroker authenticationBroker = this.OnInstantiate();
      authenticationBroker._firewall = firewall;
      authenticationBroker._connection = connection;
      authenticationBroker.OnInstantiated();
      return authenticationBroker;
    }

    protected abstract InboundAuthenticationBroker OnInstantiate();

    protected abstract void OnInstantiated();
  }
}
