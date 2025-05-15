// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.MethodEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  [DebuggerDisplay("{_builder.Name}")]
  internal class MethodEmitter : IEmissionMemberEmitter
  {
    private MethodBuilder _builder;
    private GenericTypeParameterBuilder[] _genericTypeParams;
    private ArgumentReference[] _arguments;
    private MethodCodeBuilder _codeBuilder;

    private bool ImplementedByRuntime => (this._builder.GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != 0;

    public ArgumentReference[] Arguments => this._arguments;

    public virtual MethodCodeBuilder CodeBuilder
    {
      get
      {
        if (this._codeBuilder == null)
          this._codeBuilder = new MethodCodeBuilder(this._builder.GetILGenerator());
        return this._codeBuilder;
      }
    }

    public GenericTypeParameterBuilder[] GenericTypeParams => this._genericTypeParams;

    public MethodBuilder MethodBuilder => this._builder;

    public MemberInfo Member => (MemberInfo) this._builder;

    public Type ReturnType => this._builder.ReturnType;

    protected MethodEmitter(MethodBuilder builder) => this._builder = builder;

    internal MethodEmitter(AbstractTypeEmitter owner, string name, MethodAttributes attributes)
      : this(owner.TypeBuilder.DefineMethod(name, attributes))
    {
    }

    internal MethodEmitter(
      AbstractTypeEmitter owner,
      string name,
      MethodAttributes attributes,
      Type returnType,
      params Type[] argumentTypes)
      : this(owner, name, attributes)
    {
      this.SetParameters(argumentTypes);
      this.SetReturnType(returnType);
    }

    internal MethodEmitter(
      AbstractTypeEmitter owner,
      string name,
      MethodAttributes attributes,
      MethodInfo methodToUseAsATemplate)
      : this(owner, name, attributes)
    {
      Dictionary<string, GenericTypeParameterBuilder> genericArgumentsMap = GenericUtil.GetGenericArgumentsMap(owner);
      Type correctType = GenericUtil.ExtractCorrectType(methodToUseAsATemplate.ReturnType, genericArgumentsMap);
      ParameterInfo[] parameters = methodToUseAsATemplate.GetParameters();
      Type[] parametersTypes = GenericUtil.ExtractParametersTypes(parameters, genericArgumentsMap);
      this._genericTypeParams = GenericUtil.CopyGenericArguments(methodToUseAsATemplate, this._builder, genericArgumentsMap);
      this.SetParameters(parametersTypes);
      this.SetReturnType(correctType);
      this.SetSignature(correctType, methodToUseAsATemplate.ReturnParameter, parametersTypes, parameters);
      this.DefineParameters(parameters);
    }

    public void SetParameters(Type[] paramTypes)
    {
      this._builder.SetParameters(paramTypes);
      this._arguments = ArgumentsUtil.ConvertToArgumentReference(paramTypes);
      ArgumentsUtil.InitializeArgumentsByPosition(this._arguments, this.MethodBuilder.IsStatic);
    }

    private void SetReturnType(Type returnType) => this._builder.SetReturnType(returnType);

    private void SetSignature(
      Type returnType,
      ParameterInfo returnParameter,
      Type[] parameters,
      ParameterInfo[] baseMethodParameters)
    {
      this._builder.SetSignature(returnType, returnParameter.GetRequiredCustomModifiers(), returnParameter.GetOptionalCustomModifiers(), parameters, ((IEnumerable<ParameterInfo>) baseMethodParameters).Select<ParameterInfo, Type[]>((Func<ParameterInfo, Type[]>) (x => x.GetRequiredCustomModifiers())).ToArray<Type[]>(), ((IEnumerable<ParameterInfo>) baseMethodParameters).Select<ParameterInfo, Type[]>((Func<ParameterInfo, Type[]>) (x => x.GetOptionalCustomModifiers())).ToArray<Type[]>());
    }

    public void DefineCustomAttribute(CustomAttributeBuilder attribute) => this._builder.SetCustomAttribute(attribute);

    private void DefineParameters(ParameterInfo[] parameters)
    {
      foreach (ParameterInfo parameter in parameters)
      {
        ParameterBuilder parameterBuilder = this._builder.DefineParameter(parameter.Position + 1, parameter.Attributes, parameter.Name);
        foreach (CustomAttributeBuilder inheritableAttribute in parameter.GetNonInheritableAttributes())
          parameterBuilder.SetCustomAttribute(inheritableAttribute);
      }
    }

    public virtual void Generate()
    {
      if (this.ImplementedByRuntime)
        return;
      this._codeBuilder.Generate((IEmissionMemberEmitter) this, this._builder.GetILGenerator());
    }

    public virtual void EnsureValidCodeBlock()
    {
      if (this.ImplementedByRuntime || !this.CodeBuilder.IsEmpty)
        return;
      this.CodeBuilder.AddStatement((Statement) new NopStatement());
      this.CodeBuilder.AddStatement((Statement) new ReturnStatement());
    }
  }
}
