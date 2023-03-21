using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime.Calls
{
	public abstract class LibraryOverload : OverloadInfo
	{
		private readonly Delegate _delegate;

		private ReadOnlyCollection<ParameterInfo> _parameters;

		private short _defaultProtocolAttrs;

		private short _notNullAttrs;

		public override MethodBase ReflectionInfo
		{
			get
			{
				return _delegate.Method;
			}
		}

		public override Type ReturnType
		{
			get
			{
				return _delegate.Method.ReturnType;
			}
		}

		public override ParameterInfo ReturnParameter
		{
			get
			{
				return _delegate.Method.ReturnParameter;
			}
		}

		public override Type DeclaringType
		{
			get
			{
				return _delegate.Method.DeclaringType;
			}
		}

		public override string Name
		{
			get
			{
				return string.Empty;
			}
		}

		public override IList<ParameterInfo> Parameters
		{
			get
			{
				return _parameters ?? (_parameters = new ReadOnlyCollection<ParameterInfo>(_delegate.Method.GetParameters()));
			}
		}

		public override MethodAttributes Attributes
		{
			get
			{
				return MethodAttributes.Public | MethodAttributes.Static;
			}
		}

		public override bool IsConstructor
		{
			get
			{
				return false;
			}
		}

		public override bool IsExtension
		{
			get
			{
				return false;
			}
		}

		public override bool IsGenericMethodDefinition
		{
			get
			{
				return false;
			}
		}

		public override bool IsGenericMethod
		{
			get
			{
				return false;
			}
		}

		public override bool ContainsGenericParameters
		{
			get
			{
				return false;
			}
		}

		public override IList<Type> GenericArguments
		{
			get
			{
				return Type.EmptyTypes;
			}
		}

		protected LibraryOverload(Delegate overloadDelegate, short defaultProtocolAttrs, short notNullAttrs)
		{
			_delegate = overloadDelegate;
			_defaultProtocolAttrs = defaultProtocolAttrs;
			_notNullAttrs = notNullAttrs;
		}

		internal static LibraryOverload Create(Delegate overloadDelegate, uint customAttributes)
		{
			return Create(overloadDelegate, (customAttributes & 0x80000000u) != 0, (short)((customAttributes >> 15) & 0x7FFF), (short)(customAttributes & 0x7FFF));
		}

		internal static uint EncodeCustomAttributes(MethodInfo method)
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length > 15)
			{
				throw new NotSupportedException();
			}
			uint num = 0u;
			uint num2 = 0u;
			for (int i = 0; i < parameters.Length; i++)
			{
				num |= (parameters[i].IsDefined(typeof(DefaultProtocolAttribute), false) ? ((uint)(1 << i)) : 0u);
				num2 |= (parameters[i].IsDefined(typeof(NotNullAttribute), false) ? ((uint)(1 << i)) : 0u);
			}
			int num3 = parameters.Length - 1;
			bool flag = num3 >= 0 && parameters[num3].IsDefined(typeof(ParamArrayAttribute), false);
			if (flag)
			{
				num2 |= (parameters[num3].IsDefined(typeof(NotNullItemsAttribute), false) ? ((uint)(1 << num3)) : 0u);
			}
			return (flag ? 2147483648u : 0u) | (num << 15) | num2;
		}

		public static LibraryOverload Create(Delegate overloadDelegate, bool isVariadic, short defaultProtocolAttrs, short notNullAttrs)
		{
			if (isVariadic)
			{
				return new LibraryVariadicOverloadInfo(overloadDelegate, defaultProtocolAttrs, notNullAttrs);
			}
			return new LibraryOverloadInfo(overloadDelegate, defaultProtocolAttrs, notNullAttrs);
		}

		internal static LibraryOverload Reflect(Delegate overloadDelegate)
		{
			return Create(overloadDelegate, EncodeCustomAttributes(overloadDelegate.Method));
		}

		public override bool IsParamArray(int parameterIndex)
		{
			if (IsVariadic)
			{
				return parameterIndex == Parameters.Count - 1;
			}
			return false;
		}

		public override bool IsParamDictionary(int parameterIndex)
		{
			return false;
		}

		public override bool ProhibitsNull(int parameterIndex)
		{
			return (_notNullAttrs & (1 << parameterIndex)) != 0;
		}

		public override bool ProhibitsNullItems(int parameterIndex)
		{
			if (IsParamArray(parameterIndex))
			{
				return ProhibitsNull(parameterIndex);
			}
			return false;
		}

		public bool HasDefaultProtocol(int parameterIndex)
		{
			return (_defaultProtocolAttrs & (1 << parameterIndex)) != 0;
		}

		public override OverloadInfo MakeGenericMethod(Type[] genericArguments)
		{
			throw new InvalidOperationException();
		}
	}
}
