// Decompiled with JetBrains decompiler
// Type: System.Emission.Contributors.NetworkInterfaceProxyContributor
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Collectors;
using System.Emission.Emitters;
using System.Emission.Generators;
using System.Emission.Meta;
using System.Reflection.Emit;

namespace System.Emission.Contributors
{
  internal sealed class NetworkInterfaceProxyContributor : IEmissionContributor
  {
    private IDesignatingScope _scope;
    private ICollection<Type> interfaces = (ICollection<Type>) new HashSet<Type>();
    private ICollection<MetaProperty> properties = (ICollection<MetaProperty>) new TypeElementCollection<MetaProperty>();
    private ICollection<MetaEvent> events = (ICollection<MetaEvent>) new TypeElementCollection<MetaEvent>();
    private ICollection<MetaMethod> methods = (ICollection<MetaMethod>) new TypeElementCollection<MetaMethod>();
    private string _dynamicAssemblyName;

    internal NetworkInterfaceProxyContributor(IDesignatingScope scope, string dynamicAssemblyName)
    {
      this._scope = scope;
      this._dynamicAssemblyName = dynamicAssemblyName;
    }

    public void Generate(ClassEmitter @class, EmissionProxyOptions options)
    {
      foreach (MetaMethod method in (IEnumerable<MetaMethod>) this.methods)
      {
        if (method.Standalone)
          this.ImplementMethod(method, @class, options, new OverrideMethodDelegate(((AbstractTypeEmitter) @class).CreateMethod));
      }
      foreach (MetaProperty property in (IEnumerable<MetaProperty>) this.properties)
        this.ImplementProperty(@class, property, options);
      foreach (MetaEvent @event in (IEnumerable<MetaEvent>) this.events)
        this.ImplementEvent(@class, @event, options);
    }

    public void CollectElements(IEmissionProxyHook hook, MetaType model)
    {
      foreach (EmissionMemberCollector emissionMemberCollector in this.CollectElementsInternal(hook))
      {
        foreach (MetaMethod method in emissionMemberCollector.Methods)
        {
          model.AddMethod(method);
          this.methods.Add(method);
        }
        foreach (MetaEvent @event in emissionMemberCollector.Events)
        {
          model.AddEvent(@event);
          this.events.Add(@event);
        }
        foreach (MetaProperty property in emissionMemberCollector.Properties)
        {
          model.AddProperty(property);
          this.properties.Add(property);
        }
      }
    }

    private void ImplementEvent(
      ClassEmitter emitter,
      MetaEvent @event,
      EmissionProxyOptions options)
    {
      @event.BuildEventEmitter(emitter);
      this.ImplementMethod(@event.Adder, emitter, options, new OverrideMethodDelegate(@event.Emitter.CreateAddMethod));
      this.ImplementMethod(@event.Remover, emitter, options, new OverrideMethodDelegate(@event.Emitter.CreateRemoveMethod));
    }

    private void ImplementProperty(
      ClassEmitter emitter,
      MetaProperty property,
      EmissionProxyOptions options)
    {
      property.BuildPropertyEmitter(emitter);
      if (property.CanRead)
        this.ImplementMethod(property.Getter, emitter, options, new OverrideMethodDelegate(property.Emitter.CreateGetMethod));
      if (!property.CanWrite)
        return;
      this.ImplementMethod(property.Setter, emitter, options, new OverrideMethodDelegate(property.Emitter.CreateSetMethod));
    }

    private MethodGenerator GetMethodGenerator(
      MetaMethod method,
      ClassEmitter @class,
      EmissionProxyOptions options,
      OverrideMethodDelegate overrideMethod)
    {
      return !method.Proxyable ? (MethodGenerator) new EmptyMethodGenerator(method, overrideMethod) : (MethodGenerator) new NetworkSerializationMethodGenerator(method, overrideMethod);
    }

    private void ImplementMethod(
      MetaMethod method,
      ClassEmitter @class,
      EmissionProxyOptions options,
      OverrideMethodDelegate overrideMethod)
    {
      MethodGenerator methodGenerator = this.GetMethodGenerator(method, @class, options, overrideMethod);
      if (methodGenerator == null)
        return;
      MethodEmitter methodEmitter = methodGenerator.Generate(@class, options, this._scope, this._dynamicAssemblyName);
      foreach (CustomAttributeBuilder inheritableAttribute in method.Method.GetNonInheritableAttributes())
        methodEmitter.DefineCustomAttribute(inheritableAttribute);
    }

    private IEnumerable<EmissionMemberCollector> CollectElementsInternal(IEmissionProxyHook hook)
    {
      foreach (Type interfaceType in (IEnumerable<Type>) this.interfaces)
      {
        NetworkInterfaceProxyMemberCollector proxyMemberCollector = new NetworkInterfaceProxyMemberCollector(this._dynamicAssemblyName, interfaceType);
        proxyMemberCollector.CollectMembers(hook);
        yield return (EmissionMemberCollector) proxyMemberCollector;
      }
    }

    internal void AddInterfaceToProxy(Type interfaceType) => this.interfaces.Add(interfaceType);
  }
}
