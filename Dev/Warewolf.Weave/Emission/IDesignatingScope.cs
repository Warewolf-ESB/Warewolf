// Decompiled with JetBrains decompiler
// Type: System.Emission.IDesignatingScope
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Emission
{
  public interface IDesignatingScope
  {
    string Name { get; }

    IDesignatingScope ParentScope { get; }

    string GenerateDesignation(string suggestedDesignation);

    IDesignatingScope GenerateChildScope(string suggestedDesignation);
  }
}
