// Decompiled with JetBrains decompiler
// Type: System.Network.PacketData
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public struct PacketData
  {
    public int Channel;
    public int Length;
    public byte[] Data;
    public ushort ID;

    public PacketData(int channel, int length, byte[] data, ushort id)
    {
      this.Channel = channel;
      this.Length = length;
      this.Data = data;
      this.ID = id;
    }
  }
}
