// Decompiled with JetBrains decompiler
// Type: System.Network.InternalTemplates
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public static class InternalTemplates
  {
    public static readonly PacketTemplate Server_LogoutReceived = new PacketTemplate(15, 0, false);
    public static readonly PacketTemplate Server_OnExecuteStringCommandReceived = new PacketTemplate(15, 1, true);
    public static readonly PacketTemplate Server_OnExecuteBinaryCommandReceived = new PacketTemplate(15, 2, true);
    public static readonly PacketTemplate Server_SendClientDetails = new PacketTemplate(15, 3, true);
    public static readonly PacketTemplate Client_LogoutReceived = new PacketTemplate(15, 0, 1);
    public static readonly PacketTemplate Client_OnExecuteStringCommandReceived = new PacketTemplate(15, 1, true);
    public static readonly PacketTemplate Client_OnExecuteBinaryCommandReceived = new PacketTemplate(15, 2, true);
    public static readonly PacketTemplate Client_OnClientDetailsReceived = new PacketTemplate(15, 3, true);
  }
}
