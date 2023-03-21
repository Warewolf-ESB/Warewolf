using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Generation;

namespace IronRuby.Compiler.Generation
{
	public class RubyTypeEmitter : ClsTypeEmitter
	{
		private readonly FieldBuilder _immediateClassField;

		private readonly FieldBuilder _instanceDataField;

		private static Dictionary<Type, CallSite> _conversionSites;

		internal bool IsDerivedRubyType
		{
			get
			{
				return _immediateClassField == null;
			}
		}

		internal FieldBuilder ImmediateClassField
		{
			get
			{
				return _immediateClassField;
			}
		}

		internal FieldBuilder InstanceDataField
		{
			get
			{
				return _instanceDataField;
			}
		}

		protected override Type ContextType
		{
			get
			{
				return typeof(RubyContext);
			}
		}

		public RubyTypeEmitter(TypeBuilder tb)
			: base(tb)
		{
			if (!typeof(IRubyType).IsAssignableFrom(tb.BaseType))
			{
				_immediateClassField = tb.DefineField("_immediateClass", typeof(RubyClass), FieldAttributes.Private);
				_instanceDataField = tb.DefineField("_instanceData", typeof(RubyInstanceData), FieldAttributes.Private);
			}
		}

		public static void AddRemoveEventHelper(object method, object instance, object dt, object eventValue, string name)
		{
			throw new NotImplementedException();
		}

		protected override MethodInfo EventHelper()
		{
			return typeof(RubyTypeEmitter).GetMethod("AddRemoveEventHelper");
		}

		public static Exception InvokeMethodMissing(object o, string name)
		{
			return RubyExceptions.CreateMethodMissing(RubyContext._Default, o, name);
		}

		protected override MethodInfo MissingInvokeMethodException()
		{
			return typeof(RubyTypeEmitter).GetMethod("InvokeMethodMissing");
		}

		public static RubyCallAction MakeRubyCallSite(string methodName, int argumentCount)
		{
			return RubyCallAction.MakeShared(methodName, new RubyCallSignature(argumentCount, (RubyCallFlags)80));
		}

		protected override void EmitMakeCallAction(string name, int nargs, bool isList)
		{
			ILGen cCtor = GetCCtor();
			cCtor.Emit(OpCodes.Ldstr, name);
			cCtor.EmitInt(nargs);
			cCtor.EmitCall(typeof(RubyTypeEmitter), "MakeRubyCallSite");
		}

		public static CallSite<Func<CallSite, RubyContext, object, T>> GetConversionSite<T>()
		{
			if (_conversionSites == null)
			{
				Interlocked.CompareExchange(ref _conversionSites, new Dictionary<Type, CallSite>(), null);
			}
			Type typeFromHandle = typeof(T);
			lock (_conversionSites)
			{
				CallSite value;
				if (_conversionSites.TryGetValue(typeFromHandle, out value))
				{
					return (CallSite<Func<CallSite, RubyContext, object, T>>)value;
				}
				CallSite<Func<CallSite, RubyContext, object, T>> callSite = CallSite<Func<CallSite, RubyContext, object, T>>.Create(RubyConversionAction.GetConversionAction(null, typeFromHandle, true));
				_conversionSites[typeFromHandle] = callSite;
				return callSite;
			}
		}

		protected override MethodInfo GetGenericConversionSiteFactory(Type toType)
		{
			return typeof(RubyTypeEmitter).GetMethod("GetConversionSite").MakeGenericMethod(toType);
		}

		protected override FieldInfo GetConversionSiteField(Type toType)
		{
			return AllocateDynamicSite(new Type[4]
			{
				typeof(CallSite),
				typeof(RubyContext),
				typeof(object),
				toType
			}, (FieldInfo site) => Expression.Assign(Expression.Field(null, site), Expression.Call(null, site.FieldType.GetMethod("Create"), RubyConversionAction.GetConversionAction(null, toType, true).CreateExpression())));
		}

		protected override void EmitImplicitContext(ILGen il)
		{
			il.EmitLoadArg(0);
			EmitClassObjectFromInstance(il);
			il.EmitPropertyGet(typeof(RubyModule), "Context");
		}

		protected override void EmitClassObjectFromInstance(ILGen il)
		{
			if (typeof(IRubyObject).IsAssignableFrom(base.BaseType))
			{
				il.EmitCall(Methods.IRubyObject_get_ImmediateClass);
			}
			else
			{
				il.EmitFieldGet(_immediateClassField);
			}
		}

		protected override Type[] MakeSiteSignature(int nargs)
		{
			Type[] array = new Type[nargs + 4];
			array[0] = typeof(CallSite);
			array[1] = typeof(RubyContext);
			for (int i = 2; i < array.Length; i++)
			{
				array[i] = typeof(object);
			}
			return array;
		}
	}
}
