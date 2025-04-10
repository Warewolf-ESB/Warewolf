// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaEvent
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection;

namespace System.Emission.Meta
{
  internal sealed class MetaEvent : MetaTypeElement, IEquatable<MetaEvent>
  {
    private MetaMethod _adder;
    private MetaMethod _remover;
    private Type _type;
    private EventEmitter _emitter;
    private EventAttributes _attributes;
    private string _name;

    public EventAttributes Attributes => this._attributes;

    public MetaMethod Adder => this._adder;

    public MetaMethod Remover => this._remover;

    public EventEmitter Emitter => this._emitter != null ? this._emitter : throw new InvalidOperationException("Emitter is not built. You have to build it first using 'BuildEventEmitter' method");

    public MetaEvent(
      string dynamicAssemblyName,
      string name,
      Type declaringType,
      Type eventDelegateType,
      MetaMethod adder,
      MetaMethod remover,
      EventAttributes attributes)
      : base(declaringType, dynamicAssemblyName)
    {
      if (adder == null)
        throw new ArgumentNullException(nameof (adder));
      if (remover == null)
        throw new ArgumentNullException(nameof (remover));
      this._name = name;
      this._type = eventDelegateType;
      this._adder = adder;
      this._remover = remover;
      this._attributes = attributes;
    }

    public void BuildEventEmitter(ClassEmitter classEmitter)
    {
      if (this._emitter != null)
        throw new InvalidOperationException();
      this._emitter = classEmitter.CreateEvent(this._name, this.Attributes, this._type);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      if (this == obj)
        return true;
      return !(obj.GetType() != typeof (MetaEvent)) && this.Equals((MetaEvent) obj);
    }

    public override int GetHashCode() => ((this._adder.Method != (MethodInfo) null ? this._adder.Method.GetHashCode() : 0) * 397 ^ (this._remover.Method != (MethodInfo) null ? this._remover.Method.GetHashCode() : 0)) * 397 ^ this.Attributes.GetHashCode();

    public bool Equals(MetaEvent other) => other != null && (this == other || this._type.Equals(other._type) && StringComparer.OrdinalIgnoreCase.Equals(this._name, other._name));

    internal override void SwitchToExplicitImplementation()
    {
      this._name = this.sourceType.Name + "." + this._name;
      this._adder.SwitchToExplicitImplementation();
      this._remover.SwitchToExplicitImplementation();
    }
  }
}
