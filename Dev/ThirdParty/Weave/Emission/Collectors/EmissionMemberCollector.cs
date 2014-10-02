
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Collectors
{
    internal abstract class EmissionMemberCollector
    {
        #region Constants
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        #endregion

        #region Instance Fields
        private ICollection<MethodInfo> _checkedMethods = new HashSet<MethodInfo>();
        private IDictionary<PropertyInfo, MetaProperty> _properties = new Dictionary<PropertyInfo, MetaProperty>();
        private IDictionary<EventInfo, MetaEvent> _events = new Dictionary<EventInfo, MetaEvent>();
        private IDictionary<MethodInfo, MetaMethod> _methods = new Dictionary<MethodInfo, MetaMethod>();
        protected Type _type;
        private string _dynamicAssemblyName;
        #endregion

        #region Public Properties
        public IEnumerable<MetaMethod> Methods { get { return _methods.Values; } }
        public IEnumerable<MetaProperty> Properties { get { return _properties.Values; } }
        public IEnumerable<MetaEvent> Events { get { return _events.Values; } }
        public string DynamicAsssemblyName { get { return _dynamicAssemblyName; } }
        #endregion

        #region Constructor
        protected EmissionMemberCollector(Type type, string dynamicAssemblyName)
        {
            _type = type;
            _dynamicAssemblyName = dynamicAssemblyName;
        }
        #endregion

        #region Collection Handling
        public virtual void CollectMembers(IEmissionProxyHook hook)
        {
            if (_checkedMethods == null) throw new InvalidOperationException( "Can't call 'CollectMembers' method twice.");
            CollectProperties(hook);
            CollectEvents(hook);
            CollectMethods(hook);
            _checkedMethods = null;
        }

        private void CollectProperties(IEmissionProxyHook hook)
        {
            PropertyInfo[] propertiesFound = _type.GetProperties(Flags);
            foreach (PropertyInfo property in propertiesFound) AddProperty(property, hook);
        }

        private void CollectEvents(IEmissionProxyHook hook)
        {
            EventInfo[] eventsFound = _type.GetEvents(Flags);
            foreach (EventInfo @event in eventsFound) AddEvent(@event, hook);
        }

        private void CollectMethods(IEmissionProxyHook hook)
        {
            MethodInfo[] methodsFound = MethodFinder.GetAllInstanceMethods(_type, Flags);
            foreach (MethodInfo method in methodsFound) AddMethod(method, hook, true, MetaMethodSource.Method);
        }
        #endregion

        #region Addition Handling
        private void AddProperty(PropertyInfo property, IEmissionProxyHook hook)
        {
            MetaMethod getter = null;
            MetaMethod setter = null;

            if (property.CanRead)
            {
                MethodInfo getMethod = property.GetGetMethod(true);
                getter = AddMethod(getMethod, hook, false, MetaMethodSource.Property | MetaMethodSource.Acquire);
            }

            if (property.CanWrite)
            {
                MethodInfo setMethod = property.GetSetMethod(true);
                setter = AddMethod(setMethod, hook, false, MetaMethodSource.Property | MetaMethodSource.Release);
            }

            if (setter == null && getter == null) return;

            IEnumerable<CustomAttributeBuilder> nonInheritableAttributes = property.GetNonInheritableAttributes();
            ParameterInfo[] arguments = property.GetIndexParameters();
            _properties[property] = new MetaProperty(_dynamicAssemblyName, property.Name, property.PropertyType, property.DeclaringType, getter, setter, nonInheritableAttributes, arguments.Select(a => a.ParameterType).ToArray());
        }

        private void AddEvent(EventInfo @event, IEmissionProxyHook hook)
        {
            MethodInfo addMethod = @event.GetAddMethod(true);
            MethodInfo removeMethod = @event.GetRemoveMethod(true);
            MetaMethod adder = null;
            MetaMethod remover = null;

            if (addMethod != null) adder = AddMethod(addMethod, hook, false, MetaMethodSource.Event | MetaMethodSource.Acquire);
            if (removeMethod != null) remover = AddMethod(removeMethod, hook, false, MetaMethodSource.Event | MetaMethodSource.Release);
            if (adder == null && remover == null) return;
            _events[@event] = new MetaEvent(_dynamicAssemblyName, @event.Name, @event.DeclaringType, @event.EventHandlerType, adder, remover, EventAttributes.None);
        }

        private MetaMethod AddMethod(MethodInfo method, IEmissionProxyHook hook, bool isStandalone, MetaMethodSource source)
        {
            if (_checkedMethods.Contains(method)) return null;
            _checkedMethods.Add(method);

            if (_methods.ContainsKey(method)) return null;
            MetaMethod methodToGenerate = GetMethodToGenerate(method, hook, isStandalone, source);
            if (methodToGenerate != null) _methods[method] = methodToGenerate;

            return methodToGenerate;
        }
        #endregion

        #region Method Handling
        protected bool AcceptMethod(MethodInfo method, bool onlyVirtuals, IEmissionProxyHook hook)
        {
            if (method.IsFinal) return false;
            if (IsInternalAndNotVisibleToDynamicProxy(method)) return false;

            if (onlyVirtuals && !method.IsVirtual)
            {
                if (method.DeclaringType != typeof(MarshalByRefObject) && method.IsGetType() == false && method.IsMemberwiseClone() == false)
                    hook.NonProxyableMemberNotification(_type, method);

                return false;
            }

            if ((method.IsPublic || method.IsFamily || method.IsAssembly || method.IsFamilyOrAssembly) == false) return false;
            if (method.DeclaringType == typeof(MarshalByRefObject)) return false;
            if (method.IsFinalizer()) return false;

            return hook.ShouldInterceptMethod(_type, method);
        }

        protected abstract MetaMethod GetMethodToGenerate(MethodInfo method, IEmissionProxyHook hook, bool isStandalone, MetaMethodSource source);

        private bool IsInternalAndNotVisibleToDynamicProxy(MethodInfo method)
        {
            return method.IsInternal() && method.DeclaringType.Assembly.IsInternalToDynamicProxy(_dynamicAssemblyName) == false;
        }
        #endregion
    }

    internal class MethodFinder
    {
        private static readonly Dictionary<Type, object> cachedMethodInfosByType = new Dictionary<Type, object>();
        private static readonly object lockObject = new object();

        public static MethodInfo[] GetAllInstanceMethods(Type type, BindingFlags flags)
        {
            if ((flags & ~(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) != 0)
                throw new ArgumentException("MethodFinder only supports the Public, NonPublic, and Instance binding flags.", "flags");

            MethodInfo[] methodsInCache;

            lock (lockObject)
            {
                if (!cachedMethodInfosByType.ContainsKey(type))
                    cachedMethodInfosByType.Add(type, RemoveDuplicates(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance)));

                methodsInCache = (MethodInfo[])cachedMethodInfosByType[type];
            }

            return MakeFilteredCopy(methodsInCache, flags & (BindingFlags.Public | BindingFlags.NonPublic));
        }

        private static MethodInfo[] MakeFilteredCopy(MethodInfo[] methodsInCache, BindingFlags visibilityFlags)
        {
            if ((visibilityFlags & ~(BindingFlags.Public | BindingFlags.NonPublic)) != 0)
                throw new ArgumentException("Only supports BindingFlags.Public and NonPublic.", "visibilityFlags");

            bool includePublic = (visibilityFlags & BindingFlags.Public) == BindingFlags.Public;
            bool includeNonPublic = (visibilityFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;

            List<MethodInfo> result = new List<MethodInfo>(methodsInCache.Length);

            foreach (var method in methodsInCache)
                if ((method.IsPublic && includePublic) || (!method.IsPublic && includeNonPublic))
                    result.Add(method);

            return result.ToArray();
        }

        private static object RemoveDuplicates(MethodInfo[] infos)
        {
            Dictionary<MethodInfo, object> uniqueInfos = new Dictionary<MethodInfo, object>(MethodSignatureComparer.Instance);

            foreach (MethodInfo info in infos)
                if (!uniqueInfos.ContainsKey(info))
                    uniqueInfos.Add(info, null);

            MethodInfo[] result = new MethodInfo[uniqueInfos.Count];
            uniqueInfos.Keys.CopyTo(result, 0);
            return result;
        }
    }
}
