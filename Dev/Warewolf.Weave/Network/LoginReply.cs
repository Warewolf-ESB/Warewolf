// Decompiled with JetBrains decompiler
// Type: System.Network.LoginReply
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public enum LoginReply : byte
  {
    Invalid = 0,
    InUse = 1,
    Blocked = 2,
    Valid = 4,
    NetworkLockdown = 5,
    BFProtected = 6,
    Compromised = 7,
    ServiceDown = 8,
    Logout = 254, // 0xFE
    BadComm = 255, // 0xFF
  }
}
