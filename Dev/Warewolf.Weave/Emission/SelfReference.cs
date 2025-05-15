// Decompiled with JetBrains decompiler
// Type: System.Emission.SelfReference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Emission
{
  [DebuggerDisplay("this")]
  internal sealed class SelfReference : Reference
  {
    public static readonly SelfReference Self = new SelfReference();

    protected SelfReference()
      : base((Reference) null)
    {
    }

    public override void LoadAddressOfReference(ILGenerator gen) => throw new NotSupportedException();

    public override void LoadReference(ILGenerator gen) => gen.Emit(OpCodes.Ldarg_0);

    public override void StoreReference(ILGenerator gen) => gen.Emit(OpCodes.Ldarg_0);
  }
}
