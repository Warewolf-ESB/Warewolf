// Decompiled with JetBrains decompiler
// Type: System.Emission.Collectors.EmissionMemberCollector
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Collectors
{
  internal abstract class EmissionMemberCollector
  {
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private ICollection<MethodInfo> _checkedMethods = (ICollection<MethodInfo>) new HashSet<MethodInfo>();
    private IDictionary<PropertyInfo, MetaProperty> _properties = (IDictionary<PropertyInfo, MetaProperty>) new Dictionary<PropertyInfo, MetaProperty>();
    private IDictionary<EventInfo, MetaEvent> _events = (IDictionary<EventInfo, MetaEvent>) new Dictionary<EventInfo, MetaEvent>();
    private IDictionary<MethodInfo, MetaMethod> _methods = (IDictionary<MethodInfo, MetaMethod>) new Dictionary<MethodInfo, MetaMethod>();
    protected Type _type;
    private string _dynamicAssemblyName;

    public IEnumerable<MetaMethod> Methods => (IEnumerable<MetaMethod>) this._methods.Values;

    public IEnumerable<MetaProperty> Properties => (IEnumerable<MetaProperty>) this._properties.Values;

    public IEnumerable<MetaEvent> Events => (IEnumerable<MetaEvent>) this._events.Values;

    public string DynamicAsssemblyName => this._dynamicAssemblyName;

    protected EmissionMemberCollector(Type type, string dynamicAssemblyName)
    {
      this._type = type;
      this._dynamicAssemblyName = dynamicAssemblyName;
    }

    public virtual void CollectMembers(IEmissionProxyHook hook)
    {
      if (this._checkedMethods == null)
        throw new InvalidOperationException("Can't call 'CollectMembers' method twice.");
      this.CollectProperties(hook);
      this.CollectEvents(hook);
      this.CollectMethods(hook);
      this._checkedMethods = (ICollection<MethodInfo>) null;
    }

    private void CollectProperties(IEmissionProxyHook hook)
    {
      foreach (PropertyInfo property in this._type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        this.AddProperty(property, hook);
    }

    private void CollectEvents(IEmissionProxyHook hook)
    {
      foreach (EventInfo @event in this._type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        this.AddEvent(@event, hook);
    }

    private void CollectMethods(IEmissionProxyHook hook)
    {
      foreach (MethodInfo allInstanceMethod in MethodFinder.GetAllInstanceMethods(this._type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        this.AddMethod(allInstanceMethod, hook, true, MetaMethodSource.Method);
    }

    private void AddProperty(PropertyInfo property, IEmissionProxyHook hook)
    {
      MetaMethod getter = (MetaMethod) null;
      MetaMethod setter = (MetaMethod) null;
      if (property.CanRead)
        getter = this.AddMethod(property.GetGetMethod(true), hook, false, MetaMethodSource.Property | MetaMethodSource.Acquire);
      if (property.CanWrite)
        setter = this.AddMethod(property.GetSetMethod(true), hook, false, MetaMethodSource.Property | MetaMethodSource.Release);
      if (setter == null && getter == null)
        return;
      IEnumerable<CustomAttributeBuilder> inheritableAttributes = property.GetNonInheritableAttributes();
      ParameterInfo[] indexParameters = property.GetIndexParameters();
      this._properties[property] = new MetaProperty(this._dynamicAssemblyName, property.Name, property.PropertyType, property.DeclaringType, getter, setter, inheritableAttributes, ((IEnumerable<ParameterInfo>) indexParameters).Select<ParameterInfo, Type>((Func<ParameterInfo, Type>) (a => a.ParameterType)).ToArray<Type>());
    }

    private void AddEvent(EventInfo @event, IEmissionProxyHook hook)
    {
      MethodInfo addMethod = @event.GetAddMethod(true);
      MethodInfo removeMethod = @event.GetRemoveMethod(true);
      MetaMethod adder = (MetaMethod) null;
      MetaMethod remover = (MetaMethod) null;
      if (addMethod != (MethodInfo) null)
        adder = this.AddMethod(addMethod, hook, false, MetaMethodSource.Event | MetaMethodSource.Acquire);
      if (removeMethod != (MethodInfo) null)
        remover = this.AddMethod(removeMethod, hook, false, MetaMethodSource.Event | MetaMethodSource.Release);
      if (adder == null && remover == null)
        return;
      this._events[@event] = new MetaEvent(this._dynamicAssemblyName, @event.Name, @event.DeclaringType, @event.EventHandlerType, adder, remover, EventAttributes.None);
    }

    private MetaMethod AddMethod(
      MethodInfo method,
      IEmissionProxyHook hook,
      bool isStandalone,
      MetaMethodSource source)
    {
      if (this._checkedMethods.Contains(method))
        return (MetaMethod) null;
      this._checkedMethods.Add(method);
      if (this._methods.ContainsKey(method))
        return (MetaMethod) null;
      MetaMethod methodToGenerate = this.GetMethodToGenerate(method, hook, isStandalone, source);
      if (methodToGenerate != null)
        this._methods[method] = methodToGenerate;
      return methodToGenerate;
    }

    protected bool AcceptMethod(MethodInfo method, bool onlyVirtuals, IEmissionProxyHook hook)
    {
      if (method.IsFinal || this.IsInternalAndNotVisibleToDynamicProxy(method))
        return false;
      if (onlyVirtuals && !method.IsVirtual)
      {
        if (method.DeclaringType != typeof (MarshalByRefObject) && !method.IsGetType() && !method.IsMemberwiseClone())
          hook.NonProxyableMemberNotification(this._type, (MemberInfo) method);
        return false;
      }
      return (method.IsPublic || method.IsFamily || method.IsAssembly || method.IsFamilyOrAssembly) && !(method.DeclaringType == typeof (MarshalByRefObject)) && !method.IsFinalizer() && hook.ShouldInterceptMethod(this._type, method);
    }

    protected abstract MetaMethod GetMethodToGenerate(
      MethodInfo method,
      IEmissionProxyHook hook,
      bool isStandalone,
      MetaMethodSource source);

    private bool IsInternalAndNotVisibleToDynamicProxy(MethodInfo method) => method.IsInternal() && !method.DeclaringType.Assembly.IsInternalToDynamicProxy(this._dynamicAssemblyName);
  }
}
