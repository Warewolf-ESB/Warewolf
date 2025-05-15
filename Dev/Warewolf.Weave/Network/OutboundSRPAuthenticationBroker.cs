// Decompiled with JetBrains decompiler
// Type: System.Network.OutboundSRPAuthenticationBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Cryptography;

namespace System.Network
{
  public abstract class OutboundSRPAuthenticationBroker : OutboundAuthenticationBroker
  {
    private static byte[] _outboundSeed = new byte[16]
    {
      (byte) 244,
      (byte) 102,
      (byte) 49,
      (byte) 89,
      (byte) 252,
      (byte) 131,
      (byte) 110,
      (byte) 49,
      (byte) 49,
      (byte) 2,
      (byte) 81,
      (byte) 213,
      (byte) 68,
      (byte) 49,
      (byte) 103,
      (byte) 152
    };
    private static byte[] _inboundSeed = new byte[16]
    {
      (byte) 34,
      (byte) 190,
      (byte) 229,
      (byte) 207,
      (byte) 187,
      (byte) 7,
      (byte) 100,
      (byte) 217,
      (byte) 0,
      (byte) 69,
      (byte) 27,
      (byte) 208,
      (byte) 36,
      (byte) 184,
      (byte) 213,
      (byte) 69
    };
    private int _stage;
    private BigInteger _credentials;
    protected string _username;
    protected bool _remoteFirewall;
    protected int _localIdentifier;
    protected Version _localVersion;
    protected PlatformID _localPlatform;
    protected string _localServicePack;
    protected uint _localFingerprint;
    protected byte[] _ksrnSeed;
    protected SecureRemotePassword _srp;

    protected OutboundSRPAuthenticationBroker(string username, string password)
    {
      this._stage = -1;
      this._username = username;
      this._credentials = new BigInteger(SecureRemotePassword.GenerateCredentialsHash(username, password));
      this._srp = new SecureRemotePassword(this._username, this._credentials, false);
    }

    protected override void Reset()
    {
      this._stage = -1;
      this._ksrnSeed = (byte[]) null;
      this._srp = new SecureRemotePassword(this._username, this._credentials, false);
    }

    protected override void OnBeginAuthentication()
    {
      this._stage = 0;
      ByteBuffer byteBuffer = new ByteBuffer(128);
      if (this._localIdentifier != 0)
        byteBuffer.Write(this._localIdentifier);
      if (this._localVersion != (Version) null)
        byteBuffer.Write(this._localVersion);
      byteBuffer.Write((byte) this._localPlatform);
      byteBuffer.Write(this._localServicePack);
      byteBuffer.Write(this._localFingerprint);
      if (this._remoteFirewall)
        byteBuffer.Write(this._connection.ToString());
      byteBuffer.Write(this._username);
      byteBuffer.Write(this._srp.PublicEphemeralValueA.GetBytes(32));
      this._connection.Send(byteBuffer.Data, 0, (int) byteBuffer.Length);
    }

    protected override bool OnDataReceived(byte[] data, int length)
    {
      if (this._stage == -1 || length <= 0)
        return false;
      switch (this._stage)
      {
        case 0:
          ByteBuffer byteBuffer1 = new ByteBuffer(data);
          AuthenticationResponse reason1 = (AuthenticationResponse) byteBuffer1.ReadByte();
          switch (reason1)
          {
            case AuthenticationResponse.InvalidCredentials:
              this.Reset();
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason1, false);
              return true;
            case AuthenticationResponse.Success:
              ++this._stage;
              this._ksrnSeed = new byte[4];
              for (int index = 0; index < this._ksrnSeed.Length; ++index)
                this._ksrnSeed[index] = byteBuffer1.ReadByte();
              this._srp.Salt = new BigInteger(byteBuffer1.ReadBytes((int) byteBuffer1.ReadByte()));
              this._srp.PublicEphemeralValueB = new BigInteger(byteBuffer1.ReadBytes(32));
              this._connection.Send(this._srp.ClientSessionKeyProof.GetBytes(20), 0, 20);
              break;
            default:
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason1, true);
              return false;
          }
          break;
        case 1:
          ByteBuffer byteBuffer2 = new ByteBuffer(data);
          AuthenticationResponse reason2 = (AuthenticationResponse) byteBuffer2.ReadByte();
          switch (reason2)
          {
            case AuthenticationResponse.InvalidCredentials:
              this.Reset();
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason2, false);
              return true;
            case AuthenticationResponse.Success:
              ++this._stage;
              AuthenticationResponse authenticationResponse;
              try
              {
                authenticationResponse = this._srp.IsServerProofValid(new BigInteger(byteBuffer2.ReadBytes(20))) ? AuthenticationResponse.Success : AuthenticationResponse.InvalidCredentials;
              }
              catch
              {
                authenticationResponse = AuthenticationResponse.InvalidCredentials;
              }
              this._connection.Send(new byte[1]
              {
                (byte) authenticationResponse
              }, 0, 1);
              break;
            default:
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason2, true);
              return false;
          }
          break;
        case 2:
          ByteBuffer buffer = new ByteBuffer(data);
          AuthenticationResponse reason3 = (AuthenticationResponse) buffer.ReadByte();
          switch (reason3)
          {
            case AuthenticationResponse.InUse:
              this.Reset();
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason3, false);
              return true;
            case AuthenticationResponse.Success:
              this.OnAuthenticated(buffer);
              this._cryptProvider = (ICryptProvider) new ARCProvider(((int) this._ksrnSeed[0] + (int) this._ksrnSeed[2]) / (int) this._ksrnSeed[1] * (int) this._ksrnSeed[3] + 50, this._srp.SessionKey.GetBytes(40), OutboundSRPAuthenticationBroker._inboundSeed, OutboundSRPAuthenticationBroker._outboundSeed);
              this._connection.Host.NotifyAuthenticated(this._connection, (NetworkAccount) null, (AuthenticationBroker) this);
              return true;
            default:
              this._connection.Host.NotifyAuthenticationFailed(this._connection, (OutboundAuthenticationBroker) this, reason3, true);
              return false;
          }
      }
      return true;
    }
  }
}
