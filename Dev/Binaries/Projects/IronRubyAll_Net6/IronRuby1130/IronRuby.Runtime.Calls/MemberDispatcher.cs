using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public abstract class MemberDispatcher
	{
		internal const int MaxPrecompiledArity = 5;

		internal const int MaxInterpretedArity = 15;

		internal const int MaxRubyMethodArity = 14;

		internal int Version;

		internal static readonly Type[] RubyObjectMethodDispatchers = new Type[5]
		{
			typeof(RubyObjectMethodDispatcher<>),
			typeof(RubyObjectMethodDispatcher<, >),
			typeof(RubyObjectMethodDispatcher<, , >),
			typeof(RubyObjectMethodDispatcher<, , , >),
			typeof(RubyObjectMethodDispatcher<, , , , >)
		};

		internal static readonly Type[] RubyObjectMethodDispatchersWithBlock = new Type[5]
		{
			typeof(RubyObjectMethodDispatcherWithBlock<>),
			typeof(RubyObjectMethodDispatcherWithBlock<, >),
			typeof(RubyObjectMethodDispatcherWithBlock<, , >),
			typeof(RubyObjectMethodDispatcherWithBlock<, , , >),
			typeof(RubyObjectMethodDispatcherWithBlock<, , , , >)
		};

		internal static readonly Type[] RubyObjectMethodDispatchersWithScope = new Type[5]
		{
			typeof(RubyObjectMethodDispatcherWithScope<>),
			typeof(RubyObjectMethodDispatcherWithScope<, >),
			typeof(RubyObjectMethodDispatcherWithScope<, , >),
			typeof(RubyObjectMethodDispatcherWithScope<, , , >),
			typeof(RubyObjectMethodDispatcherWithScope<, , , , >)
		};

		internal static readonly Type[] RubyObjectMethodDispatchersWithScopeAndBlock = new Type[5]
		{
			typeof(RubyObjectMethodDispatcherWithScopeAndBlock<>),
			typeof(RubyObjectMethodDispatcherWithScopeAndBlock<, >),
			typeof(RubyObjectMethodDispatcherWithScopeAndBlock<, , >),
			typeof(RubyObjectMethodDispatcherWithScopeAndBlock<, , , >),
			typeof(RubyObjectMethodDispatcherWithScopeAndBlock<, , , , >)
		};

		internal static readonly Type[] RubyObjectAttributeWriterDispatchersWithScope = new Type[1] { typeof(RubyObjectAttributeWriterDispatcherWithScope<>) };

		internal static readonly HashSet<Type> UntypedFuncs = new HashSet<Type>
		{
			typeof(Func<CallSite, object>),
			typeof(Func<CallSite, object, object>),
			typeof(Func<CallSite, object, object, object>),
			typeof(Func<CallSite, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>),
			typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)
		};

		public abstract object CreateDelegate(bool isUntyped);

		internal static object CreateDispatcher(Type func, int mandatoryParamCount, bool hasScope, bool hasBlock, int version, Func<MethodDispatcher> parameterlessFactory, Type[] genericFactories)
		{
			Type[] genericArguments = func.GetGenericArguments();
			int num = 1 + (hasScope ? 1 : 0);
			int num2 = num + 1 + (hasBlock ? 1 : 0);
			int num3 = genericArguments.Length - num2 - 1;
			if (num3 != mandatoryParamCount)
			{
				return null;
			}
			if (num3 > 5)
			{
				return null;
			}
			if (genericArguments[num] != typeof(object))
			{
				return null;
			}
			if (num3 == 0)
			{
				return parameterlessFactory();
			}
			Type[] slice = genericArguments.GetSlice(num2, num3);
			return Activator.CreateInstance(genericFactories[num3 - 1].MakeGenericType(slice));
		}

		internal static LambdaExpression CreateRubyMethodLambda(Expression body, string name, ICollection<ParameterExpression> parameters)
		{
			switch (parameters.Count)
			{
			case 2:
				return Expression.Lambda<Func<object, Proc, object>>(body, name, parameters);
			case 3:
				return Expression.Lambda<Func<object, Proc, object, object>>(body, name, parameters);
			case 4:
				return Expression.Lambda<Func<object, Proc, object, object, object>>(body, name, parameters);
			case 5:
				return Expression.Lambda<Func<object, Proc, object, object, object, object>>(body, name, parameters);
			case 6:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object>>(body, name, parameters);
			case 7:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object>>(body, name, parameters);
			case 8:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object>>(body, name, parameters);
			case 9:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 10:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 11:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 12:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 13:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 14:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 15:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			case 16:
				return Expression.Lambda<Func<object, Proc, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(body, name, parameters);
			default:
				return null;
			}
		}
	}
}
