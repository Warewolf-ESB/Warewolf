// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaType
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Emission.Meta
{
  internal sealed class MetaType
  {
    private ICollection<MetaEvent> _events = (ICollection<MetaEvent>) new TypeElementCollection<MetaEvent>();
    private ICollection<MetaMethod> _methods = (ICollection<MetaMethod>) new TypeElementCollection<MetaMethod>();
    private ICollection<MetaProperty> _properties = (ICollection<MetaProperty>) new TypeElementCollection<MetaProperty>();

    public IEnumerable<MetaMethod> Methods => (IEnumerable<MetaMethod>) this._methods;

    public IEnumerable<MetaProperty> Properties => (IEnumerable<MetaProperty>) this._properties;

    public IEnumerable<MetaEvent> Events => (IEnumerable<MetaEvent>) this._events;

    public void AddEvent(MetaEvent @event) => this._events.Add(@event);

    public void AddMethod(MetaMethod method) => this._methods.Add(method);

    public void AddProperty(MetaProperty property) => this._properties.Add(property);
  }
}
