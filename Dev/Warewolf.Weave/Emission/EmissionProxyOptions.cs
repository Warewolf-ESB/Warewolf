// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionProxyOptions
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class EmissionProxyOptions
  {
    public static readonly EmissionProxyOptions Default = new EmissionProxyOptions();
    private IEmissionProxyHook _hook;
    private Type _ipBaseType;
    private List<CustomAttributeBuilder> _additionalAttributes;

    public IEmissionProxyHook Hook
    {
      get => this._hook;
      set => this._hook = value;
    }

    public Type InterfaceProxyBaseType
    {
      get => this._ipBaseType;
      set => this._ipBaseType = value;
    }

    public List<CustomAttributeBuilder> AdditionalAttributes => this._additionalAttributes;

    public EmissionProxyOptions(IEmissionProxyHook hook)
    {
      this._ipBaseType = typeof (object);
      this._hook = hook;
      this._additionalAttributes = new List<CustomAttributeBuilder>();
    }

    public EmissionProxyOptions()
      : this((IEmissionProxyHook) new EmissionProxyHook())
    {
    }

    public override bool Equals(object obj) => this == obj || obj is EmissionProxyOptions emissionProxyOptions && object.Equals((object) this._hook, (object) emissionProxyOptions._hook) && object.Equals((object) this._ipBaseType, (object) emissionProxyOptions._ipBaseType);

    public override int GetHashCode() => 29 * (29 * (29 * (this._hook != null ? this._hook.GetType().GetHashCode() : 0))) + (this._ipBaseType != (Type) null ? this._ipBaseType.GetHashCode() : 0);
  }
}
