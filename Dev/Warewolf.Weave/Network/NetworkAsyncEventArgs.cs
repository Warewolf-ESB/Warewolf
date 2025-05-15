// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkAsyncEventArgs
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net.Sockets;

namespace System.Network
{
  public sealed class NetworkAsyncEventArgs : SocketAsyncEventArgs
  {
    internal int Index;
    internal byte[] BufferInternal;

    public NetworkAsyncEventArgs(int capacity) => this.BufferInternal = new byte[capacity];
  }
}
