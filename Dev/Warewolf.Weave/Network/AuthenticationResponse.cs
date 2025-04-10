// Decompiled with JetBrains decompiler
// Type: System.Network.AuthenticationResponse
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public enum AuthenticationResponse : byte
  {
    Unspecified,
    InvalidVersion,
    MachineBan,
    InUse,
    InvalidCredentials,
    NetworkLockdown,
    BFProtected,
    GeneralBan,
    Logout,
    Success,
  }
}
