// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.PropertyEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal sealed class PropertyEmitter : IEmissionMemberEmitter
  {
    private PropertyBuilder _builder;
    private AbstractTypeEmitter _parentTypeEmitter;
    private MethodEmitter _getMethod;
    private MethodEmitter _setMethod;

    public MemberInfo Member => (MemberInfo) null;

    public Type ReturnType => this._builder.PropertyType;

    public PropertyEmitter(
      AbstractTypeEmitter parentTypeEmitter,
      string name,
      PropertyAttributes attributes,
      Type propertyType,
      Type[] arguments)
    {
      this._parentTypeEmitter = parentTypeEmitter;
      this._builder = parentTypeEmitter.TypeBuilder.DefineProperty(name, attributes, CallingConventions.HasThis, propertyType, (Type[]) null, (Type[]) null, arguments, (Type[][]) null, (Type[][]) null);
    }

    public MethodEmitter CreateGetMethod(
      string name,
      MethodAttributes attributes,
      MethodInfo methodToOverride)
    {
      return this.CreateGetMethod(name, attributes, methodToOverride, Type.EmptyTypes);
    }

    public MethodEmitter CreateGetMethod(
      string name,
      MethodAttributes attrs,
      MethodInfo methodToOverride,
      params Type[] parameters)
    {
      if (this._getMethod != null)
        throw new InvalidOperationException("A get method exists");
      this._getMethod = new MethodEmitter(this._parentTypeEmitter, name, attrs, methodToOverride);
      return this._getMethod;
    }

    public MethodEmitter CreateSetMethod(
      string name,
      MethodAttributes attributes,
      MethodInfo methodToOverride)
    {
      return this.CreateSetMethod(name, attributes, methodToOverride, Type.EmptyTypes);
    }

    public MethodEmitter CreateSetMethod(
      string name,
      MethodAttributes attrs,
      MethodInfo methodToOverride,
      params Type[] parameters)
    {
      if (this._setMethod != null)
        throw new InvalidOperationException("A set method exists");
      this._setMethod = new MethodEmitter(this._parentTypeEmitter, name, attrs, methodToOverride);
      return this._setMethod;
    }

    public void DefineCustomAttribute(CustomAttributeBuilder attribute) => this._builder.SetCustomAttribute(attribute);

    public void Generate()
    {
      if (this._setMethod != null)
      {
        this._setMethod.Generate();
        this._builder.SetSetMethod(this._setMethod.MethodBuilder);
      }
      if (this._getMethod == null)
        return;
      this._getMethod.Generate();
      this._builder.SetGetMethod(this._getMethod.MethodBuilder);
    }

    public void EnsureValidCodeBlock()
    {
      if (this._setMethod != null)
        this._setMethod.EnsureValidCodeBlock();
      if (this._getMethod == null)
        return;
      this._getMethod.EnsureValidCodeBlock();
    }
  }
}
