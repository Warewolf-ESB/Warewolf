using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler
{
	public static class MethodInfoExtensions
	{
		public static Expression OpCall(this MethodInfo method)
		{
			return Expression.Call(null, method);
		}

		public static Expression OpCall(this MethodInfo method, Expression arg0)
		{
			return Expression.Call(method, arg0);
		}

		public static Expression OpCall(this MethodInfo method, Expression arg0, Expression arg1)
		{
			return Expression.Call(method, arg0, arg1);
		}

		public static Expression OpCall(this MethodInfo method, Expression arg0, Expression arg1, Expression arg2)
		{
			return Expression.Call(method, arg0, arg1, arg2);
		}

		public static Expression OpCall(this MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
		{
			return Expression.Call(method, arg0, arg1, arg2, arg3);
		}

		public static Expression OpCall(this MethodInfo method, ReadOnlyCollectionBuilder<Expression> args)
		{
			return Expression.Call(method, args);
		}

		public static Expression OpCall(this MethodInfo method, ExpressionCollectionBuilder args)
		{
			return args.ToMethodCall(null, method);
		}
	}
}
