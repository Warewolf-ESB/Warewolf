// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaProperty
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Meta
{
  internal sealed class MetaProperty : MetaTypeElement, IEquatable<MetaProperty>
  {
    private Type[] _arguments;
    private PropertyAttributes _attributes;
    private IEnumerable<CustomAttributeBuilder> _customAttributes;
    private MetaMethod _getter;
    private MetaMethod _setter;
    private Type _type;
    private PropertyEmitter _emitter;
    private string _name;

    public Type[] Arguments => this._arguments;

    public bool CanRead => this._getter != null;

    public bool CanWrite => this._setter != null;

    public PropertyEmitter Emitter => this._emitter != null ? this._emitter : throw new InvalidOperationException("Emitter is not built. You have to build it first using 'BuildPropertyEmitter' method");

    public MethodInfo GetMethod
    {
      get
      {
        if (!this.CanRead)
          throw new InvalidOperationException();
        return this._getter.Method;
      }
    }

    public MetaMethod Getter => this._getter;

    public MethodInfo SetMethod
    {
      get
      {
        if (!this.CanWrite)
          throw new InvalidOperationException();
        return this._setter.Method;
      }
    }

    public MetaMethod Setter => this._setter;

    public MetaProperty(
      string dynamicAssemblyName,
      string name,
      Type propertyType,
      Type declaringType,
      MetaMethod getter,
      MetaMethod setter,
      IEnumerable<CustomAttributeBuilder> customAttributes,
      Type[] arguments)
      : base(declaringType, dynamicAssemblyName)
    {
      this._name = name;
      this._type = propertyType;
      this._getter = getter;
      this._setter = setter;
      this._attributes = PropertyAttributes.None;
      this._customAttributes = customAttributes;
      this._arguments = arguments ?? Type.EmptyTypes;
    }

    public void BuildPropertyEmitter(ClassEmitter classEmitter)
    {
      if (this._emitter != null)
        throw new InvalidOperationException("Emitter has already been built.");
      this._emitter = classEmitter.CreateProperty(this._name, this._attributes, this._type, this._arguments);
      foreach (CustomAttributeBuilder customAttribute in this._customAttributes)
        this._emitter.DefineCustomAttribute(customAttribute);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      if (this == obj)
        return true;
      return !(obj.GetType() != typeof (MetaProperty)) && this.Equals((MetaProperty) obj);
    }

    public override int GetHashCode() => (this.GetMethod != (MethodInfo) null ? this.GetMethod.GetHashCode() : 0) * 397 ^ (this.SetMethod != (MethodInfo) null ? this.SetMethod.GetHashCode() : 0);

    public bool Equals(MetaProperty other)
    {
      if (other == null)
        return false;
      if (this == other)
        return true;
      if (!this._type.Equals(other._type) || !StringComparer.OrdinalIgnoreCase.Equals(this._name, other._name) || this._arguments.Length != other._arguments.Length)
        return false;
      for (int index = 0; index < this._arguments.Length; ++index)
      {
        if (!this._arguments[index].Equals(other._arguments[index]))
          return false;
      }
      return true;
    }

    internal override void SwitchToExplicitImplementation()
    {
      this._name = this.sourceType.Name + "." + this._name;
      if (this._setter != null)
        this._setter.SwitchToExplicitImplementation();
      if (this._getter == null)
        return;
      this._getter.SwitchToExplicitImplementation();
    }
  }
}
