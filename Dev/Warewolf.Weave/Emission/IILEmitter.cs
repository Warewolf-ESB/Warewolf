// Decompiled with JetBrains decompiler
// Type: System.Emission.IILEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal interface IILEmitter
  {
    void Emit(IEmissionMemberEmitter member, ILGenerator gen);
  }
}
