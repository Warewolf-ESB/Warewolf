// Decompiled with JetBrains decompiler
// Type: System.Emission.Generators.IEmissionGenerator`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;

namespace System.Emission.Generators
{
  internal interface IEmissionGenerator<T>
  {
    T Generate(
      ClassEmitter @class,
      EmissionProxyOptions options,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName);
  }
}
