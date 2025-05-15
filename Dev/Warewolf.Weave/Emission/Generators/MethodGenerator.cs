// Decompiled with JetBrains decompiler
// Type: System.Emission.Generators.MethodGenerator
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Emission.Meta;
using System.Reflection;

namespace System.Emission.Generators
{
  internal abstract class MethodGenerator : IEmissionGenerator<MethodEmitter>
  {
    private MetaMethod _method;
    private OverrideMethodDelegate _overrideMethod;

    protected MetaMethod Method => this._method;

    protected MethodInfo MethodOnTarget => this._method.MethodOnTarget;

    protected MethodInfo MethodToOverride => this._method.Method;

    protected MethodGenerator(MetaMethod method, OverrideMethodDelegate overrideMethod)
    {
      this._method = method;
      this._overrideMethod = overrideMethod;
    }

    protected abstract MethodEmitter BuildProxiedMethodBody(
      MethodEmitter emitter,
      ClassEmitter @class,
      EmissionProxyOptions options,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName);

    public MethodEmitter Generate(
      ClassEmitter @class,
      EmissionProxyOptions options,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName)
    {
      MethodEmitter methodEmitter = this.BuildProxiedMethodBody(this._overrideMethod(this._method.Name, this._method.Attributes, this.MethodToOverride), @class, options, designatingScope, dynamicAssemblyName);
      if (this.MethodToOverride.DeclaringType.IsInterface)
        @class.TypeBuilder.DefineMethodOverride((MethodInfo) methodEmitter.MethodBuilder, this.MethodToOverride);
      return methodEmitter;
    }
  }
}
