// Decompiled with JetBrains decompiler
// Type: System.Network.EnvoyReturnedEventHandler
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;

namespace System.Network
{
  public delegate void EnvoyReturnedEventHandler(
    Envoy envoy,
    bool successful,
    string sourceHostNameOrAddress,
    IPHostEntry resolvedHostEntry);
}
