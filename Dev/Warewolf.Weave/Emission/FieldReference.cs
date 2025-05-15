// Decompiled with JetBrains decompiler
// Type: System.Emission.FieldReference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  [DebuggerDisplay("{_fieldBuilder.Name} ({_fieldBuilder.FieldType})")]
  internal sealed class FieldReference : System.Emission.Reference
  {
    private readonly FieldInfo _field;
    private readonly FieldBuilder _fieldBuilder;
    private readonly bool _isStatic;

    public FieldBuilder Fieldbuilder => this._fieldBuilder;

    public FieldInfo Reference => this._field;

    public FieldReference(FieldInfo field)
    {
      this._field = field;
      if ((field.Attributes & FieldAttributes.Static) == FieldAttributes.PrivateScope)
        return;
      this._isStatic = true;
      this._owner = (System.Emission.Reference) null;
    }

    public FieldReference(FieldBuilder fieldbuilder)
    {
      this._fieldBuilder = fieldbuilder;
      this._field = (FieldInfo) fieldbuilder;
      if ((fieldbuilder.Attributes & FieldAttributes.Static) == FieldAttributes.PrivateScope)
        return;
      this._isStatic = true;
      this._owner = (System.Emission.Reference) null;
    }

    public override void LoadAddressOfReference(ILGenerator gen)
    {
      if (this._isStatic)
        gen.Emit(OpCodes.Ldsflda, this.Reference);
      else
        gen.Emit(OpCodes.Ldflda, this.Reference);
    }

    public override void LoadReference(ILGenerator gen)
    {
      if (this._isStatic)
        gen.Emit(OpCodes.Ldsfld, this.Reference);
      else
        gen.Emit(OpCodes.Ldfld, this.Reference);
    }

    public override void StoreReference(ILGenerator gen)
    {
      if (this._isStatic)
        gen.Emit(OpCodes.Stsfld, this.Reference);
      else
        gen.Emit(OpCodes.Stfld, this.Reference);
    }
  }
}
