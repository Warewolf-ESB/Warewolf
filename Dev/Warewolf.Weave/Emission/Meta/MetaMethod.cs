// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaMethod
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Reflection;

namespace System.Emission.Meta
{
  [DebuggerDisplay("{Method}")]
  internal sealed class MetaMethod : MetaTypeElement, IEquatable<MetaMethod>
  {
    private const MethodAttributes ExplicitImplementationAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
    private string _name;
    private MethodAttributes _attributes;
    private bool _hasTarget;
    private MethodInfo _method;
    private MethodInfo _methodOnTarget;
    private bool _proxyable;
    private bool _standalone;
    private MetaMethodSource _source;

    public MethodAttributes Attributes => this._attributes;

    public bool HasTarget => this._hasTarget;

    public MethodInfo Method => this._method;

    public MethodInfo MethodOnTarget => this._methodOnTarget;

    public string Name => this._name;

    public bool Proxyable => this._proxyable;

    public bool Standalone => this._standalone;

    public MetaMethodSource Source => this._source;

    public MetaMethod(
      string dynamicAssemblyName,
      MethodInfo method,
      MethodInfo methodOnTarget,
      bool standalone,
      bool proxyable,
      bool hasTarget,
      MetaMethodSource source)
      : base(method.DeclaringType, dynamicAssemblyName)
    {
      this._method = method;
      this._name = method.Name;
      this._methodOnTarget = methodOnTarget;
      this._standalone = standalone;
      this._proxyable = proxyable;
      this._hasTarget = hasTarget;
      this._attributes = this.ObtainAttributes();
      this._source = source;
    }

    public bool Equals(MetaMethod other)
    {
      if (other == null)
        return false;
      if (this == other)
        return true;
      if (!StringComparer.OrdinalIgnoreCase.Equals(this._name, other._name))
        return false;
      MethodSignatureComparer instance = MethodSignatureComparer.Instance;
      return instance.EqualSignatureTypes(this.Method.ReturnType, other.Method.ReturnType) && instance.EqualGenericParameters(this.Method, other.Method) && instance.EqualParameters(this.Method, other.Method);
    }

    internal override void SwitchToExplicitImplementation()
    {
      this._attributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
      if (!this._standalone)
        this._attributes |= MethodAttributes.SpecialName;
      this._name = this.Method.DeclaringType.Name + "." + this.Method.Name;
    }

    private MethodAttributes ObtainAttributes()
    {
      MethodInfo method = this._method;
      MethodAttributes attributes = MethodAttributes.Virtual;
      if (method.IsFinal || this._method.DeclaringType.IsInterface)
        attributes |= MethodAttributes.NewSlot;
      if (method.IsPublic)
        attributes |= MethodAttributes.Public;
      if (method.IsHideBySig)
        attributes |= MethodAttributes.HideBySig;
      if (method.IsInternal() && method.DeclaringType.Assembly.IsInternalToDynamicProxy(this.DynamicAssemblyName))
        attributes |= MethodAttributes.Assembly;
      if (method.IsFamilyAndAssembly)
        attributes |= MethodAttributes.FamANDAssem;
      else if (method.IsFamilyOrAssembly)
        attributes |= MethodAttributes.FamORAssem;
      else if (method.IsFamily)
        attributes |= MethodAttributes.Family;
      if (!this._standalone)
        attributes |= MethodAttributes.SpecialName;
      return attributes;
    }
  }
}
