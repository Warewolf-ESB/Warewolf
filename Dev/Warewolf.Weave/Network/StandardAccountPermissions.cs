// Decompiled with JetBrains decompiler
// Type: System.Network.StandardAccountPermissions
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public enum StandardAccountPermissions : byte
  {
    None = 0,
    IgnoreNetworkLockdown = 1,
    NoBruteForceProtection = 2,
    ReplaceContext = 4,
    ConcurrentLoginRestriction = 8,
  }
}
