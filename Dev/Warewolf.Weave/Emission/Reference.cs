// Decompiled with JetBrains decompiler
// Type: System.Emission.Reference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection.Emit;

namespace System.Emission
{
  internal abstract class Reference
  {
    protected Reference _owner = (Reference) SelfReference.Self;

    public Reference OwnerReference
    {
      get => this._owner;
      set => this._owner = value;
    }

    protected Reference()
    {
    }

    protected Reference(Reference owner) => this._owner = owner;

    public abstract void LoadAddressOfReference(ILGenerator gen);

    public abstract void LoadReference(ILGenerator gen);

    public abstract void StoreReference(ILGenerator gen);

    public virtual void Generate(ILGenerator gen)
    {
    }

    public virtual Expression ToAddressOfExpression() => (Expression) new AddressOfReferenceExpression(this);

    public virtual Expression ToExpression() => (Expression) new ReferenceExpression(this);
  }
}
