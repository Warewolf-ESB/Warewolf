// Decompiled with JetBrains decompiler
// Type: System.Emission.DefaultValueExpression
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class DefaultValueExpression : Expression
  {
    private Type _type;

    public DefaultValueExpression(Type type) => this._type = type;

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      if (this.IsPrimitiveOrClass(this._type))
        OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, this._type);
      else if (this._type.IsValueType || this._type.IsGenericParameter)
      {
        LocalBuilder local = gen.DeclareLocal(this._type);
        gen.Emit(OpCodes.Ldloca_S, local);
        gen.Emit(OpCodes.Initobj, this._type);
        gen.Emit(OpCodes.Ldloc, local);
      }
      else
      {
        if (!this._type.IsByRef)
          throw new InvalidOperationException("Can't emit default value for type " + this._type?.ToString());
        this.EmitByRef(gen);
      }
    }

    private void EmitByRef(ILGenerator gen)
    {
      Type elementType = this._type.GetElementType();
      if (this.IsPrimitiveOrClass(elementType))
      {
        OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, elementType);
        OpCodeUtil.EmitStoreIndirectOpCodeForType(gen, elementType);
      }
      else
      {
        if (!elementType.IsGenericParameter && !elementType.IsValueType)
          throw new InvalidOperationException("Can't emit default value for reference of type " + elementType?.ToString());
        gen.Emit(OpCodes.Initobj, elementType);
      }
    }

    private bool IsPrimitiveOrClass(Type type)
    {
      if (type.IsPrimitive && type != typeof (IntPtr))
        return true;
      return (type.IsClass || type.IsInterface) && !type.IsGenericParameter && !type.IsByRef;
    }
  }
}
