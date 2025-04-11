// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkDirection
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  [Flags]
  public enum NetworkDirection : byte
  {
    None = 0,
    Inbound = 1,
    Outbound = 2,
    Bidirectional = Outbound | Inbound, // 0x03
  }
}
