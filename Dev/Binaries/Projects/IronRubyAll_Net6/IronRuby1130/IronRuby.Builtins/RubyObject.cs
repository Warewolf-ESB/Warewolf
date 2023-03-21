using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins
{
	[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
	[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
	public class RubyObject : IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider, ICustomTypeDescriptor, IRubyObject, IRubyObjectState, IDuplicable, ISerializable
	{
		internal class Meta : RubyMetaObject<IRubyObject>
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.ImmediateClass.Context;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromIRubyObject;
				}
			}

			public Meta(Expression expression, BindingRestrictions restrictions, IRubyObject value)
				: base(expression, restrictions, value)
			{
			}

			public override IEnumerable<string> GetDynamicMemberNames()
			{
				IRubyObject value = base.Value;
				List<string> names = new List<string>();
				RubyClass immediateClass = value.ImmediateClass;
				using (immediateClass.Context.ClassHierarchyLocker())
				{
					immediateClass.ForEachMember(true, RubyMethodAttributes.Public, delegate(string name, RubyModule module, RubyMemberInfo member)
					{
						names.Add(name);
					});
				}
				return names;
			}
		}

		internal const string ImmediateClassFieldName = "_immediateClass";

		internal const string InstanceDataFieldName = "_instanceData";

		internal const string DebuggerDisplayValue = "{_immediateClass.GetDebuggerDisplayValue(this),nq}";

		internal const string DebuggerDisplayType = "{_immediateClass.GetDebuggerDisplayType(),nq}";

		private RubyInstanceData _instanceData;

		private RubyClass _immediateClass;

		public RubyClass ImmediateClass
		{
			get
			{
				return _immediateClass;
			}
			set
			{
				_immediateClass = value;
			}
		}

		public bool IsFrozen
		{
			get
			{
				if (_instanceData != null)
				{
					return _instanceData.IsFrozen;
				}
				return false;
			}
		}

		public bool IsTainted
		{
			get
			{
				if (_instanceData != null)
				{
					return _instanceData.IsTainted;
				}
				return false;
			}
			set
			{
				GetInstanceData().IsTainted = value;
			}
		}

		public bool IsUntrusted
		{
			get
			{
				if (_instanceData != null)
				{
					return _instanceData.IsUntrusted;
				}
				return false;
			}
			set
			{
				GetInstanceData().IsUntrusted = value;
			}
		}

		public virtual DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return CustomTypeDescHelpers.GetAttributes(this);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return CustomTypeDescHelpers.GetClassName(this);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return CustomTypeDescHelpers.GetComponentName(this);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return CustomTypeDescHelpers.GetConverter(this);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return CustomTypeDescHelpers.GetDefaultEvent(this);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return CustomTypeDescHelpers.GetDefaultProperty(this);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return CustomTypeDescHelpers.GetEvents(this, attributes);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return CustomTypeDescHelpers.GetEvents(this);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return CustomTypeDescHelpers.GetProperties(this, attributes);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return CustomTypeDescHelpers.GetProperties(this);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
		}

		public RubyObject(RubyClass cls)
		{
			_immediateClass = cls;
		}

		public RubyObject(RubyClass cls, params object[] args)
			: this(cls)
		{
		}

		protected virtual RubyObject CreateInstance()
		{
			return new RubyObject(_immediateClass.NominalClass);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			RubyObject rubyObject = CreateInstance();
			context.CopyInstanceData(this, rubyObject, copySingletonMembers);
			return rubyObject;
		}

		public override string ToString()
		{
			CallSite<Func<CallSite, object, object>> toStringSite = _immediateClass.ToStringSite;
			object obj = toStringSite.Target(toStringSite, this);
			if (object.ReferenceEquals(obj, RubyOps.ForwardToBase))
			{
				return ((IRubyObject)this).BaseToString();
			}
			string text = obj as string;
			if (text != null)
			{
				return text;
			}
			MutableString mutableString = (obj as MutableString) ?? RubyUtils.ObjectToMutableString(_immediateClass.Context, obj);
			return mutableString.ToString();
		}

		public override bool Equals(object other)
		{
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			CallSite<Func<CallSite, object, object, object>> equalsSite = _immediateClass.EqualsSite;
			object obj = equalsSite.Target(equalsSite, this, other);
			if (obj == RubyOps.ForwardToBase)
			{
				return base.Equals(other);
			}
			return RubyOps.IsTrue(obj);
		}

		public override int GetHashCode()
		{
			CallSite<Func<CallSite, object, object>> getHashCodeSite = _immediateClass.GetHashCodeSite;
			object obj = getHashCodeSite.Target(getHashCodeSite, this);
			if (object.ReferenceEquals(obj, RubyOps.ForwardToBase))
			{
				return base.GetHashCode();
			}
			return Protocols.ToHashCode(obj);
		}

		string IRubyObject.BaseToString()
		{
			return RubyOps.ObjectToString(this);
		}

		bool IRubyObject.BaseEquals(object other)
		{
			return base.Equals(other);
		}

		int IRubyObject.BaseGetHashCode()
		{
			return base.GetHashCode();
		}

		public RubyInstanceData GetInstanceData()
		{
			return RubyOps.GetInstanceData(ref _instanceData);
		}

		public RubyInstanceData TryGetInstanceData()
		{
			return _instanceData;
		}

		public void Freeze()
		{
			GetInstanceData().Freeze();
		}

		protected RubyObject(SerializationInfo info, StreamingContext context)
		{
			RubyOps.DeserializeObject(out _instanceData, out _immediateClass, info);
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			RubyOps.SerializeObject(_instanceData, _immediateClass, info);
		}
	}
}
