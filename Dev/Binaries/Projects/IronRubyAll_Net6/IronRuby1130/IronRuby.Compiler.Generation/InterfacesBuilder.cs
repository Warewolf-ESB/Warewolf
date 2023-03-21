using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler.Generation
{
	public class InterfacesBuilder : IFeatureBuilder
	{
		private readonly TypeBuilder _tb;

		private readonly Type[] _interfaces;

		internal InterfacesBuilder(TypeBuilder tb, Type[] interfaces)
		{
			_tb = tb;
			_interfaces = interfaces;
		}

		public void Implement(ClsTypeEmitter emitter)
		{
			Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
			Type[] interfaces = _interfaces;
			foreach (Type type in interfaces)
			{
				if (type != typeof(IRubyType) && type != typeof(IRubyObject) && type != typeof(ICustomTypeDescriptor) && type != typeof(ISerializable) && type != typeof(IRubyDynamicMetaObjectProvider))
				{
					_tb.AddInterfaceImplementation(type);
					ImplementInterface(emitter, type, doneTypes);
				}
			}
		}

		private void ImplementInterface(ClsTypeEmitter emitter, Type interfaceType, Dictionary<Type, bool> doneTypes)
		{
			if (!doneTypes.ContainsKey(interfaceType))
			{
				doneTypes.Add(interfaceType, true);
				emitter.OverrideMethods(interfaceType);
				Type[] interfaces = interfaceType.GetInterfaces();
				foreach (Type interfaceType2 in interfaces)
				{
					ImplementInterface(emitter, interfaceType2, doneTypes);
				}
			}
		}
	}
}
