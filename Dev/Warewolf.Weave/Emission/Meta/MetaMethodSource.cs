// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaMethodSource
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Emission.Meta
{
  [Flags]
  internal enum MetaMethodSource
  {
    Unspecified = 0,
    Event = 1,
    Property = 2,
    Method = 4,
    Acquire = 8,
    Release = 16, // 0x00000010
  }
}
