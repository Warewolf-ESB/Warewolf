// Decompiled with JetBrains decompiler
// Type: System.Network.OutboundAuthenticationBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public abstract class OutboundAuthenticationBroker : AuthenticationBroker
  {
    public void BeginAuthentication(Connection connection)
    {
      this._connection = connection;
      this._connection.AuthBroker = (AuthenticationBroker) this;
      this._connection.Crypt = (ICryptProvider) NullCryptProvider.Singleton;
      this.OnBeginAuthentication();
    }

    protected abstract void OnBeginAuthentication();
  }
}
