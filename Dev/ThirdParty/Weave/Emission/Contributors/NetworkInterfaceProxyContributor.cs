
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Emission.Meta;
using System.Emission.Collectors;
using System.Reflection.Emit;
using System.Emission.Emitters;
using System.Emission.Generators;

namespace System.Emission.Contributors
{
    internal sealed class NetworkInterfaceProxyContributor : IEmissionContributor
    {
        private IDesignatingScope _scope;
        private ICollection<Type> interfaces = new HashSet<Type>();
        private ICollection<MetaProperty> properties = new TypeElementCollection<MetaProperty>();
        private ICollection<MetaEvent> events = new TypeElementCollection<MetaEvent>();
        private ICollection<MetaMethod> methods = new TypeElementCollection<MetaMethod>();
        private string _dynamicAssemblyName;

        internal NetworkInterfaceProxyContributor(IDesignatingScope scope, string dynamicAssemblyName)
        {
            _scope = scope;
            _dynamicAssemblyName = dynamicAssemblyName;
        }

        public void Generate(ClassEmitter @class, EmissionProxyOptions options)
        {
            foreach (MetaMethod method in methods)
            {
                if (!method.Standalone) continue;
                ImplementMethod(method, @class, options, @class.CreateMethod);
            }

            foreach (MetaProperty property in properties) ImplementProperty(@class, property, options);
            foreach (MetaEvent @event in events) ImplementEvent(@class, @event, options);
        }

        public void CollectElements(IEmissionProxyHook hook, MetaType model)
        {
            foreach (EmissionMemberCollector collector in CollectElementsInternal(hook))
            {
                foreach (MetaMethod method in collector.Methods)
                {
                    model.AddMethod(method);
                    methods.Add(method);
                }

                foreach (MetaEvent @event in collector.Events)
                {
                    model.AddEvent(@event);
                    events.Add(@event);
                }

                foreach (MetaProperty property in collector.Properties)
                {
                    model.AddProperty(property);
                    properties.Add(property);
                }
            }
        }

        private void ImplementEvent(ClassEmitter emitter, MetaEvent @event, EmissionProxyOptions options)
        {
            @event.BuildEventEmitter(emitter);
            ImplementMethod(@event.Adder, emitter, options, @event.Emitter.CreateAddMethod);
            ImplementMethod(@event.Remover, emitter, options, @event.Emitter.CreateRemoveMethod);
        }

        private void ImplementProperty(ClassEmitter emitter, MetaProperty property, EmissionProxyOptions options)
        {
            property.BuildPropertyEmitter(emitter);
            if (property.CanRead) ImplementMethod(property.Getter, emitter, options, property.Emitter.CreateGetMethod);
            if (property.CanWrite) ImplementMethod(property.Setter, emitter, options, property.Emitter.CreateSetMethod);
        }

        private MethodGenerator GetMethodGenerator(MetaMethod method, ClassEmitter @class, EmissionProxyOptions options, OverrideMethodDelegate overrideMethod)
        {
            if (!method.Proxyable)
            {
                return new EmptyMethodGenerator(method, overrideMethod);
            }

            return new NetworkSerializationMethodGenerator(method, overrideMethod);
        }

        private void ImplementMethod(MetaMethod method, ClassEmitter @class, EmissionProxyOptions options, OverrideMethodDelegate overrideMethod)
        {
            MethodGenerator generator = GetMethodGenerator(method, @class, options, overrideMethod);
            if (generator == null) return;
            var proxyMethod = generator.Generate(@class, options, _scope, _dynamicAssemblyName);
            foreach (CustomAttributeBuilder attribute in method.Method.GetNonInheritableAttributes()) proxyMethod.DefineCustomAttribute(attribute);
        }

        private IEnumerable<EmissionMemberCollector> CollectElementsInternal(IEmissionProxyHook hook)
        {
            foreach (Type @interface in interfaces)
            {
                NetworkInterfaceProxyMemberCollector item = new NetworkInterfaceProxyMemberCollector(_dynamicAssemblyName, @interface);
                item.CollectMembers(hook);
                yield return item;
            }
        }

        internal void AddInterfaceToProxy(Type interfaceType)
        {
            interfaces.Add(interfaceType);
        }
    }
}
