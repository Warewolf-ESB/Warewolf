// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkStateEventArgs
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class NetworkStateEventArgs : EventArgs
  {
    private NetworkState _fromState;
    private NetworkState _toState;
    private bool _isError;
    private string _message;

    public NetworkState FromState => this._fromState;

    public NetworkState ToState => this._toState;

    public bool IsError => this._isError;

    public string Message => this._message;

    public NetworkStateEventArgs(
      NetworkState fromState,
      NetworkState toState,
      bool isError,
      string message)
    {
      this._fromState = fromState;
      this._toState = toState;
      this._isError = isError;
      this._message = message;
    }

    public NetworkStateEventArgs(NetworkState fromState, NetworkState toState)
      : this(fromState, toState, false, "")
    {
    }
  }
}
