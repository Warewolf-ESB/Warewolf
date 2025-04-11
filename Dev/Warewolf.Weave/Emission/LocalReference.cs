// Decompiled with JetBrains decompiler
// Type: System.Emission.LocalReference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Emission
{
  [DebuggerDisplay("local {Type}")]
  internal sealed class LocalReference : TypeReference
  {
    private LocalBuilder _localBuilder;

    public LocalReference(Type type)
      : base(type)
    {
    }

    public override void Generate(ILGenerator gen) => this._localBuilder = gen.DeclareLocal(this.Type);

    public override void LoadAddressOfReference(ILGenerator gen) => gen.Emit(OpCodes.Ldloca, this._localBuilder);

    public override void LoadReference(ILGenerator gen) => gen.Emit(OpCodes.Ldloc, this._localBuilder);

    public override void StoreReference(ILGenerator gen) => gen.Emit(OpCodes.Stloc, this._localBuilder);
  }
}
