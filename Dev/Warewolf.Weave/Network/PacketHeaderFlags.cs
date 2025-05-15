// Decompiled with JetBrains decompiler
// Type: System.Network.PacketHeaderFlags
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  [Flags]
  public enum PacketHeaderFlags : byte
  {
    Extended = 16, // 0x10
    Identifier16 = 32, // 0x20
    Length16 = 64, // 0x40
    Length32 = 128, // 0x80
  }
}
