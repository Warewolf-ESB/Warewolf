// Decompiled with JetBrains decompiler
// Type: System.Network.LoginStateEventArgs
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class LoginStateEventArgs : EventArgs
  {
    private AuthenticationResponse _reply;
    private bool _loggedIn;
    private bool _isError;
    private bool _expectDisconnect;
    private string _message;

    public AuthenticationResponse Reply => this._reply;

    public bool LoggedIn => this._loggedIn;

    public bool IsError => this._isError;

    public bool ExpectDisconnect => this._expectDisconnect;

    public string Message => this._message;

    public LoginStateEventArgs(
      AuthenticationResponse reply,
      bool loggedIn,
      bool isError,
      string message)
    {
      this._reply = reply;
      this._loggedIn = loggedIn;
      this._isError = isError;
      this._message = message;
    }

    public LoginStateEventArgs(AuthenticationResponse reply, bool expectDisconnect)
    {
      this._reply = reply;
      this._loggedIn = this._reply == AuthenticationResponse.Success;
      this._isError = !this._loggedIn && this._reply != AuthenticationResponse.Logout;
      this._expectDisconnect = expectDisconnect;
      switch (this._reply)
      {
        case AuthenticationResponse.Unspecified:
          this._message = "Login attempt failed due to bad communication with the server.";
          break;
        case AuthenticationResponse.MachineBan:
          this._message = "Due to a terms of service violation, this computer has been banned and cannot be used to connect to the server, regardless of the account used. Please contact a member of staff for more information.";
          break;
        case AuthenticationResponse.InUse:
          this._message = "The login credentials you provided are already in use.";
          break;
        case AuthenticationResponse.InvalidCredentials:
          this._message = "The login credentials you provided are invalid.";
          break;
        case AuthenticationResponse.NetworkLockdown:
          this._message = "The server is undergoing maintenance, please try again later.";
          break;
        case AuthenticationResponse.GeneralBan:
          this._message = "That account has been blocked by the server, please contact a member of staff for more information.";
          break;
        case AuthenticationResponse.Logout:
          this._message = "You have logged out from the server.";
          break;
        case AuthenticationResponse.Success:
          this._message = "You have logged in to the server.";
          break;
      }
    }
  }
}
