// Decompiled with JetBrains decompiler
// Type: System.Emission.ConvertExpression
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class ConvertExpression : Expression
  {
    private readonly Expression right;
    private Type fromType;
    private Type target;

    public ConvertExpression(Type targetType, Expression right)
      : this(targetType, typeof (object), right)
    {
    }

    public ConvertExpression(Type targetType, Type fromType, Expression right)
    {
      this.target = targetType;
      this.fromType = fromType;
      this.right = right;
    }

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      this.right.Emit(member, gen);
      if (this.fromType == this.target)
        return;
      if (this.fromType.IsByRef)
        this.fromType = this.fromType.GetElementType();
      if (this.target.IsByRef)
        this.target = this.target.GetElementType();
      if (this.target.IsValueType)
      {
        if (this.fromType.IsValueType)
          throw new NotImplementedException("Cannot convert between distinct value types");
        if (LdindOpCodesDictionary.Instance[this.target] != LdindOpCodesDictionary.EmptyOpCode)
        {
          gen.Emit(OpCodes.Unbox, this.target);
          OpCodeUtil.EmitLoadIndirectOpCodeForType(gen, this.target);
        }
        else
          gen.Emit(OpCodes.Unbox_Any, this.target);
      }
      else if (this.fromType.IsValueType)
      {
        gen.Emit(OpCodes.Box, this.fromType);
        ConvertExpression.EmitCastIfNeeded(typeof (object), this.target, gen);
      }
      else
        ConvertExpression.EmitCastIfNeeded(this.fromType, this.target, gen);
    }

    private static void EmitCastIfNeeded(Type from, Type target, ILGenerator gen)
    {
      if (target.IsGenericParameter)
        gen.Emit(OpCodes.Unbox_Any, target);
      else if (from.IsGenericParameter)
        gen.Emit(OpCodes.Box, from);
      else if (target.IsGenericType && target != from)
      {
        gen.Emit(OpCodes.Castclass, target);
      }
      else
      {
        if (!target.IsSubclassOf(from))
          return;
        gen.Emit(OpCodes.Castclass, target);
      }
    }
  }
}
