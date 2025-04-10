// Decompiled with JetBrains decompiler
// Type: System.Emission.Generators.EmptyMethodGenerator
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Emission.Meta;
using System.Reflection;

namespace System.Emission.Generators
{
  internal class EmptyMethodGenerator : MethodGenerator
  {
    public EmptyMethodGenerator(MetaMethod method, OverrideMethodDelegate overrideMethod)
      : base(method, overrideMethod)
    {
    }

    protected override MethodEmitter BuildProxiedMethodBody(
      MethodEmitter emitter,
      ClassEmitter @class,
      EmissionProxyOptions options,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName)
    {
      ParameterInfo[] parameters = this.MethodToOverride.GetParameters();
      this.InitOutParameters(emitter, parameters);
      if (emitter.ReturnType == typeof (void))
        emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement());
      else
        emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new DefaultValueExpression(emitter.ReturnType)));
      return emitter;
    }

    private void InitOutParameters(MethodEmitter emitter, ParameterInfo[] parameters)
    {
      for (int index = 0; index < parameters.Length; ++index)
      {
        ParameterInfo parameter = parameters[index];
        if (parameter.IsOut)
          emitter.CodeBuilder.AddStatement((Statement) new AssignArgumentStatement(new ArgumentReference(parameter.ParameterType, index + 1), (Expression) new DefaultValueExpression(parameter.ParameterType)));
      }
    }
  }
}
