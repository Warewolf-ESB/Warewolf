using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public static class CustomTypeDescHelpers
	{
		private class RubyPropertyDescriptor : PropertyDescriptor
		{
			private readonly string _name;

			private readonly Type _propertyType;

			private readonly Type _componentType;

			private readonly CallSite<Func<CallSite, object, object>> _getterSite;

			private readonly CallSite<Func<CallSite, object, object, object>> _setterSite;

			public override Type ComponentType
			{
				get
				{
					return _componentType;
				}
			}

			public override bool IsReadOnly
			{
				get
				{
					return _setterSite == null;
				}
			}

			public override Type PropertyType
			{
				get
				{
					return _propertyType;
				}
			}

			internal RubyPropertyDescriptor(RubyContext context, string name, object testObject, Type componentType)
				: base(name, null)
			{
				_name = name;
				_componentType = componentType;
				_getterSite = CallSite<Func<CallSite, object, object>>.Create(RubyCallAction.Make(context, _name, RubyCallSignature.WithImplicitSelf(0)));
				_setterSite = CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(context, _name + "=", RubyCallSignature.WithImplicitSelf(0)));
				try
				{
					object value = GetValue(testObject);
					_propertyType = ((value != null) ? value.GetType() : typeof(object));
				}
				catch (Exception)
				{
					_propertyType = typeof(object);
				}
			}

			public override object GetValue(object obj)
			{
				return _getterSite.Target(_getterSite, obj);
			}

			public override void SetValue(object obj, object value)
			{
				if (_setterSite != null)
				{
					_setterSite.Target(_setterSite, obj, value);
				}
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override void ResetValue(object component)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}
		}

		private static RubyClass GetImmediateClass(object self)
		{
			IRubyObject rubyObject = self as IRubyObject;
			ContractUtils.RequiresNotNull(rubyObject, "self");
			return rubyObject.ImmediateClass;
		}

		public static AttributeCollection GetAttributes(object self)
		{
			return AttributeCollection.Empty;
		}

		public static string GetClassName(object self)
		{
			return GetImmediateClass(self).NominalClass.Name;
		}

		public static string GetComponentName(object self)
		{
			return null;
		}

		public static TypeConverter GetConverter(object self)
		{
			return new TypeConverter();
		}

		public static EventDescriptor GetDefaultEvent(object self)
		{
			return null;
		}

		public static PropertyDescriptor GetDefaultProperty(object self)
		{
			return null;
		}

		public static object GetEditor(object self, Type editorBaseType)
		{
			return null;
		}

		public static EventDescriptorCollection GetEvents(object self, Attribute[] attributes)
		{
			if (attributes == null || attributes.Length == 0)
			{
				return GetEvents(self);
			}
			return EventDescriptorCollection.Empty;
		}

		public static EventDescriptorCollection GetEvents(object self)
		{
			return EventDescriptorCollection.Empty;
		}

		public static PropertyDescriptorCollection GetProperties(object self)
		{
			return new PropertyDescriptorCollection(GetPropertiesImpl(self, new Attribute[0]));
		}

		public static PropertyDescriptorCollection GetProperties(object self, Attribute[] attributes)
		{
			return new PropertyDescriptorCollection(GetPropertiesImpl(self, attributes));
		}

		private static PropertyDescriptor[] GetPropertiesImpl(object self, Attribute[] attributes)
		{
			bool flag = true;
			foreach (Attribute attribute in attributes)
			{
				if (attribute.GetType() != typeof(BrowsableAttribute))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return new PropertyDescriptor[0];
			}
			RubyClass immediateClass = GetImmediateClass(self);
			RubyContext context = immediateClass.Context;
			Dictionary<string, int> properties = new Dictionary<string, int>();
			using (context.ClassHierarchyLocker())
			{
				immediateClass.ForEachMember(true, RubyMethodAttributes.Public, delegate(string name, RubyModule module, RubyMemberInfo member)
				{
					int num = 0;
					ExpressionType op;
					if (member is RubyAttributeReaderInfo)
					{
						num = 1;
					}
					else if (member is RubyAttributeWriterInfo)
					{
						num = 2;
					}
					else if (!(name == "initialize") && RubyUtils.TryMapOperator(name, out op) == 0)
					{
						switch (member.GetArity())
						{
						case 0:
							num = 1;
							break;
						case 1:
							if (name.LastCharacter() == 61)
							{
								num = 2;
							}
							break;
						}
					}
					switch (num)
					{
					case 2:
						name = name.Substring(0, name.Length - 1);
						break;
					case 0:
						return;
					}
					int value;
					properties.TryGetValue(name, out value);
					properties[name] = value | num;
				});
			}
			List<PropertyDescriptor> list = new List<PropertyDescriptor>(properties.Count);
			foreach (KeyValuePair<string, int> item in properties)
			{
				if (item.Value == 3)
				{
					list.Add(new RubyPropertyDescriptor(context, item.Key, self, immediateClass.GetUnderlyingSystemType()));
				}
			}
			return list.ToArray();
		}

		public static object GetPropertyOwner(object self, PropertyDescriptor pd)
		{
			return self;
		}
	}
}
