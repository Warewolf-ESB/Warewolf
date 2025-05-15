// Decompiled with JetBrains decompiler
// Type: System.Emission.ConstructorCodeBuilder
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class ConstructorCodeBuilder : AbstractCodeBuilder
  {
    private Type _baseType;

    public ConstructorCodeBuilder(Type baseType, ILGenerator generator)
      : base(generator)
    {
      this._baseType = baseType;
    }

    public void InvokeBaseConstructor()
    {
      Type type = this._baseType;
      if (type.ContainsGenericParameters)
        type = type.GetGenericTypeDefinition();
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      this.InvokeBaseConstructor(type.GetConstructor(bindingAttr, (Binder) null, new Type[0], (ParameterModifier[]) null));
    }

    public void InvokeBaseConstructor(ConstructorInfo constructor) => this.AddStatement((Statement) new ConstructorInvocationStatement(constructor, Array.Empty<Expression>()));

    public void InvokeBaseConstructor(
      ConstructorInfo constructor,
      params ArgumentReference[] arguments)
    {
      this.AddStatement((Statement) new ConstructorInvocationStatement(constructor, ArgumentsUtil.ConvertArgumentReferenceToExpression(arguments)));
    }
  }
}
