// Decompiled with JetBrains decompiler
// Type: System.Network.AuthenticationBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public abstract class AuthenticationBroker
  {
    protected Connection _connection;
    protected ICryptProvider _cryptProvider;

    public ICryptProvider CryptProvider => this._cryptProvider;

    protected abstract void Reset();

    internal bool NotifyDataReceived(byte[] data, int length) => this.OnDataReceived(data, length);

    protected virtual void OnAuthenticated(ByteBuffer buffer)
    {
    }

    protected abstract bool OnDataReceived(byte[] data, int length);
  }
}
