// Decompiled with JetBrains decompiler
// Type: System.Emission.NewInstanceExpression
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class NewInstanceExpression : Expression
  {
    private Expression[] _arguments;
    private Type[] _constructorArgs;
    private Type _type;
    private ConstructorInfo _constructor;

    public NewInstanceExpression(ConstructorInfo constructor, params Expression[] args)
    {
      this._constructor = constructor;
      this._arguments = args;
    }

    public NewInstanceExpression(Type target, Type[] constructor_args, params Expression[] args)
    {
      this._type = target;
      this._constructorArgs = constructor_args;
      this._arguments = args;
    }

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      foreach (Expression expression in this._arguments)
        expression.Emit(member, gen);
      if (this._constructor == (ConstructorInfo) null)
        this._constructor = this._type.GetConstructor(this._constructorArgs);
      if (this._constructor == (ConstructorInfo) null)
        throw new InvalidOperationException("Could not find constructor matching specified arguments");
      gen.Emit(OpCodes.Newobj, this._constructor);
    }
  }
}
