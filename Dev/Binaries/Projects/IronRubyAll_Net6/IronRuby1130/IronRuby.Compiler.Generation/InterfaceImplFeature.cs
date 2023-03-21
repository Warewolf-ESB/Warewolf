using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace IronRuby.Compiler.Generation
{
	internal sealed class InterfaceImplFeature : ITypeFeature
	{
		internal static readonly InterfaceImplFeature Empty = new InterfaceImplFeature(Type.EmptyTypes);

		private readonly Type[] _interfaces;

		private readonly int _hash;

		public bool CanInherit
		{
			get
			{
				return true;
			}
		}

		internal InterfaceImplFeature(IList<Type> interfaceTypes)
		{
			List<Type> list = new List<Type>(interfaceTypes.Count);
			foreach (Type interfaceType in interfaceTypes)
			{
				AddInterface(list, interfaceType);
			}
			list.Sort((Type a, Type b) => string.CompareOrdinal(a.FullName, b.FullName));
			_interfaces = list.ToArray();
			_hash = typeof(InterfaceImplFeature).GetHashCode();
			Type[] interfaces = _interfaces;
			foreach (Type type in interfaces)
			{
				_hash ^= type.GetHashCode();
			}
		}

		internal static ITypeFeature Create(IList<Type> interfaceTypes)
		{
			if (interfaceTypes.Count == 0)
			{
				return Empty;
			}
			return new InterfaceImplFeature(interfaceTypes);
		}

		private static void AddInterface(List<Type> types, Type type)
		{
			object.Equals(true, type.IsInterface && !type.ContainsGenericParameters);
			for (int i = 0; i < types.Count; i++)
			{
				Type type2 = types[i];
				if (type2 == type || type.IsAssignableFrom(type2))
				{
					return;
				}
				if (type2.IsAssignableFrom(type))
				{
					types.RemoveAt(i--);
				}
			}
			types.Add(type);
		}

		public bool IsImplementedBy(Type type)
		{
			return _interfaces.Length == 0;
		}

		public IFeatureBuilder MakeBuilder(TypeBuilder tb)
		{
			return new InterfacesBuilder(tb, _interfaces);
		}

		public override int GetHashCode()
		{
			return typeof(InterfaceImplFeature).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			InterfaceImplFeature interfaceImplFeature = obj as InterfaceImplFeature;
			if (interfaceImplFeature == null)
			{
				return false;
			}
			if (_interfaces.Length != interfaceImplFeature._interfaces.Length)
			{
				return false;
			}
			for (int i = 0; i < _interfaces.Length; i++)
			{
				if (!_interfaces[i].Equals(interfaceImplFeature._interfaces[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
