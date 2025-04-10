// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.TypeConstructorEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Emission.Emitters
{
  internal sealed class TypeConstructorEmitter : ConstructorEmitter
  {
    internal TypeConstructorEmitter(AbstractTypeEmitter maintype)
      : base(maintype, maintype.TypeBuilder.DefineTypeInitializer())
    {
    }

    public override void EnsureValidCodeBlock()
    {
      if (!this.CodeBuilder.IsEmpty)
        return;
      this.CodeBuilder.AddStatement((Statement) new ReturnStatement());
    }
  }
}
