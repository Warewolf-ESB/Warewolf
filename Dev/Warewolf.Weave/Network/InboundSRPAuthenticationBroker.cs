// Decompiled with JetBrains decompiler
// Type: System.Network.InboundSRPAuthenticationBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Cryptography;

namespace System.Network
{
  public abstract class InboundSRPAuthenticationBroker : InboundAuthenticationBroker
  {
    private static byte[] _outboundSeed = new byte[16]
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
    private static byte[] _inboundSeed = new byte[16]
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
    private int _stage;
    private int _minLength;
    protected byte[] _ksrnSeed;
    protected SecureRemotePassword _srp;

    protected override void Reset()
    {
      this._stage = 0;
      this._targetIdentifier = 0;
      this._targetVersion = (Version) null;
      this._targetPlatform = PlatformID.Win32S;
      this._targetServicePack = (string) null;
      this._targetFingerprint = 0U;
      this._targetHostname = (string) null;
      this._targetAccountName = (string) null;
      this._targetAccount = (NetworkAccount) null;
      this._ksrnSeed = (byte[]) null;
      this._srp = (SecureRemotePassword) null;
    }

    protected override void OnInstantiated()
    {
      this._minLength = 45;
      if (this._localIdentifier != 0)
        this._minLength += 4;
      if (this._localVersion != (Version) null)
        this._minLength += 16;
      if (this._firewall == null)
        return;
      ++this._minLength;
    }

    protected override sealed bool OnDataReceived(byte[] data, int length)
    {
      if (this._stage == -1)
        return false;
      AuthenticationResponse authenticationResponse = AuthenticationResponse.Unspecified;
      switch (this._stage)
      {
        case 0:
          authenticationResponse = this.HandleIntroduction(data, length);
          if (authenticationResponse == AuthenticationResponse.Success)
          {
            byte[] bytes = this._srp.Salt.GetBytes();
            ByteBuffer byteBuffer = new ByteBuffer(38 + bytes.Length);
            byteBuffer.Write((byte) authenticationResponse);
            this._ksrnSeed = new byte[4];
            for (int index = 0; index < 4; ++index)
              byteBuffer.Write(this._ksrnSeed[index] = MathHelper.Random((byte) 1, byte.MaxValue));
            byteBuffer.Write((byte) bytes.Length);
            byteBuffer.Write(bytes);
            byteBuffer.Write(this._srp.PublicEphemeralValueB.GetBytes(32));
            this._connection.Send(byteBuffer.Data, 0, (int) byteBuffer.Length);
            break;
          }
          this._connection.Send(new byte[1]
          {
            (byte) authenticationResponse
          }, 0, 1);
          if (authenticationResponse != AuthenticationResponse.InvalidCredentials)
            return false;
          this.Reset();
          return true;
        case 1:
          authenticationResponse = this.HandleClientProof(data, length);
          if (authenticationResponse == AuthenticationResponse.Success)
          {
            byte[] numArray = new byte[21];
            numArray[0] = (byte) authenticationResponse;
            Buffer.BlockCopy((Array) this._srp.ServerSessionKeyProof.GetBytes(20), 0, (Array) numArray, 1, 20);
            this._connection.Send(numArray, 0, 21);
            break;
          }
          if (this._firewall != null && !this._targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
            this._firewall.NotifyLoginAttempt(this._connection.Address, this._targetAccount.AccountID, false);
          this._connection.Send(new byte[1]
          {
            (byte) authenticationResponse
          }, 0, 1);
          if (authenticationResponse != AuthenticationResponse.InvalidCredentials)
            return false;
          this.Reset();
          return true;
        case 2:
          authenticationResponse = this.HandleAuthResult(data, length);
          switch (authenticationResponse)
          {
            case AuthenticationResponse.InUse:
              this._connection.Send(new byte[1]
              {
                (byte) authenticationResponse
              }, 0, 1);
              if (authenticationResponse != AuthenticationResponse.InUse)
                return false;
              this.Reset();
              return true;
            case AuthenticationResponse.Success:
              ByteBuffer buffer = new ByteBuffer(32);
              buffer.Write((byte) authenticationResponse);
              this.OnAuthenticated(buffer);
              this._connection.Send(buffer.Data, 0, (int) buffer.Length);
              this._cryptProvider = (ICryptProvider) new ARCProvider(((int) this._ksrnSeed[0] + (int) this._ksrnSeed[2]) / (int) this._ksrnSeed[1] * (int) this._ksrnSeed[3] + 50, this._srp.SessionKey.GetBytes(40), InboundSRPAuthenticationBroker._inboundSeed, InboundSRPAuthenticationBroker._outboundSeed);
              this._connection.Host.NotifyAuthenticated(this._connection, this._targetAccount, (AuthenticationBroker) this);
              break;
            default:
              if (this._firewall != null && !this._targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
              {
                this._firewall.NotifyLoginAttempt(this._connection.Address, this._targetAccount.AccountID, false);
                goto case AuthenticationResponse.InUse;
              }
              else
                goto case AuthenticationResponse.InUse;
          }
          break;
      }
      if (authenticationResponse == AuthenticationResponse.Success)
      {
        ++this._stage;
        return true;
      }
      this._stage = -1;
      return false;
    }

    private AuthenticationResponse HandleIntroduction(byte[] data, int length)
    {
      if (length < this._minLength)
        return AuthenticationResponse.Unspecified;
      ByteBuffer byteBuffer = new ByteBuffer(data, length);
      if (this._localIdentifier != 0 && (this._targetIdentifier = byteBuffer.ReadInt32()) != this._localIdentifier)
        return AuthenticationResponse.Unspecified;
      if (this._localVersion != (Version) null)
        this._targetVersion = byteBuffer.ReadVersion();
      this._targetPlatform = (PlatformID) byteBuffer.ReadByte();
      this._targetServicePack = byteBuffer.ReadString();
      this._targetFingerprint = byteBuffer.ReadUInt32();
      if (this._firewall != null)
      {
        if (this._firewall.IsBlocked(this._targetFingerprint))
          return AuthenticationResponse.MachineBan;
        this._targetHostname = byteBuffer.ReadString();
      }
      this._targetAccountName = byteBuffer.ReadString().ToUpper();
      if (byteBuffer.Length - byteBuffer.Position != 32L)
        return AuthenticationResponse.Unspecified;
      byte[] inData = byteBuffer.ReadBytes(32);
      if ((this._targetAccount = this.ResolveAccount(this._targetAccountName)) == null)
        return AuthenticationResponse.InvalidCredentials;
      if (this._firewall != null)
      {
        if (this._firewall.NetworkLockdown && !this._targetAccount.HasAllPermissions(StandardAccountPermissions.IgnoreNetworkLockdown))
          return AuthenticationResponse.NetworkLockdown;
        if (!this._targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection) && this._firewall.IsBFPThrottled(this._targetAccount.AccountID, this._connection.Address))
        {
          this._firewall.NotifyLoginAttempt(this._connection.Address, this._targetAccount.AccountID, false);
          return AuthenticationResponse.BFProtected;
        }
      }
      if (this._targetAccount.Blocked)
        return this._targetAccount.BlockedLoginResponse;
      this._srp = new SecureRemotePassword(this._targetAccountName, new BigInteger(this._targetAccount.Password), true);
      AuthenticationResponse authenticationResponse = AuthenticationResponse.Success;
      try
      {
        this._srp.PublicEphemeralValueA = new BigInteger(inData);
      }
      catch
      {
        authenticationResponse = AuthenticationResponse.InvalidCredentials;
        if (this._firewall != null)
        {
          if (!this._targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
            this._firewall.NotifyLoginAttempt(this._connection.Address, this._targetAccount.AccountID, false);
        }
      }
      return authenticationResponse;
    }

    private AuthenticationResponse HandleClientProof(byte[] data, int length)
    {
      if (length != 20)
        return AuthenticationResponse.Unspecified;
      AuthenticationResponse authenticationResponse = AuthenticationResponse.Success;
      byte[] numArray = new byte[20];
      Buffer.BlockCopy((Array) data, 0, (Array) numArray, 0, 20);
      try
      {
        if (!this._srp.IsClientProofValid(new BigInteger(numArray)))
          authenticationResponse = AuthenticationResponse.InvalidCredentials;
      }
      catch
      {
        authenticationResponse = AuthenticationResponse.InvalidCredentials;
      }
      return authenticationResponse;
    }

    private AuthenticationResponse HandleAuthResult(byte[] data, int length)
    {
      if (length != 1 || data[0] != (byte) 9)
        return AuthenticationResponse.Unspecified;
      if (this._targetAccount.InUse)
      {
        if ((int) this._targetAccount.Owner.Fingerprint != (int) this._targetFingerprint && !this._targetAccount.HasAllPermissions(StandardAccountPermissions.ReplaceContext))
        {
          if (this._firewall == null || !this._targetAccount.HasAllPermissions(StandardAccountPermissions.ConcurrentLoginRestriction))
            return AuthenticationResponse.InUse;
          this._firewall.NotifyAccountCompromised(this._targetAccount, this._connection.Address, AccountCompromisedReason.ConcurrentLogins);
          return this._targetAccount.BlockedLoginResponse;
        }
        this._connection.Host.NotifyDeauthenticated(this._targetAccount.Owner.Connection, (InboundAuthenticationBroker) this, AuthenticationResponse.InUse);
        if (this._targetAccount.InUse)
          throw new InvalidOperationException("Host failed to deauthenticate connection context.");
      }
      return AuthenticationResponse.Success;
    }

    protected abstract NetworkAccount ResolveAccount(string account);
  }
}
