using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public static class AstFactory
	{
		internal static readonly System.Linq.Expressions.Expression[] EmptyExpressions = new System.Linq.Expressions.Expression[0];

		internal static readonly ParameterExpression[] EmptyParameters = new ParameterExpression[0];

		internal static readonly System.Linq.Expressions.Expression NullOfMutableString = System.Linq.Expressions.Expression.Constant(null, typeof(MutableString));

		internal static readonly System.Linq.Expressions.Expression NullOfProc = System.Linq.Expressions.Expression.Constant(null, typeof(Proc));

		internal static readonly System.Linq.Expressions.Expression True = System.Linq.Expressions.Expression.Constant(ScriptingRuntimeHelpers.True);

		internal static readonly System.Linq.Expressions.Expression False = System.Linq.Expressions.Expression.Constant(ScriptingRuntimeHelpers.False);

		internal static readonly System.Linq.Expressions.Expression BlockReturnReasonBreak = Microsoft.Scripting.Ast.Utils.Constant(BlockReturnReason.Break);

		public static System.Linq.Expressions.Expression Infinite(LabelTarget @break, LabelTarget @continue, params System.Linq.Expressions.Expression[] body)
		{
			return Microsoft.Scripting.Ast.Utils.Infinite(System.Linq.Expressions.Expression.Block(body), @break, @continue);
		}

		public static System.Linq.Expressions.Expression IsTrue(System.Linq.Expressions.Expression expression)
		{
			if (expression.Type == typeof(bool))
			{
				return expression;
			}
			return Methods.IsTrue.OpCall(Microsoft.Scripting.Ast.Utils.Box(expression));
		}

		public static System.Linq.Expressions.Expression IsFalse(System.Linq.Expressions.Expression expression)
		{
			if (expression.Type == typeof(bool))
			{
				return System.Linq.Expressions.Expression.Not(expression);
			}
			return Methods.IsFalse.OpCall(Microsoft.Scripting.Ast.Utils.Box(expression));
		}

		public static System.Linq.Expressions.Expression Logical(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, bool isConjunction)
		{
			if (isConjunction)
			{
				return System.Linq.Expressions.Expression.AndAlso(left, right);
			}
			return System.Linq.Expressions.Expression.OrElse(left, right);
		}

		internal static TryStatementBuilder FinallyIf(this TryStatementBuilder builder, bool ifdef, params System.Linq.Expressions.Expression[] body)
		{
			if (!ifdef)
			{
				return builder;
			}
			return builder.Finally(body);
		}

		internal static TryStatementBuilder FilterIf(this TryStatementBuilder builder, bool ifdef, ParameterExpression holder, System.Linq.Expressions.Expression condition, params System.Linq.Expressions.Expression[] body)
		{
			if (!ifdef)
			{
				return builder;
			}
			return builder.Filter(holder, condition, body);
		}

		public static System.Linq.Expressions.Expression Condition(System.Linq.Expressions.Expression test, System.Linq.Expressions.Expression ifTrue, System.Linq.Expressions.Expression ifFalse)
		{
			if (ifTrue.Type != ifFalse.Type)
			{
				if (ifTrue.Type.IsAssignableFrom(ifFalse.Type))
				{
					ifFalse = System.Linq.Expressions.Expression.Convert(ifFalse, ifTrue.Type);
				}
				else if (ifFalse.Type.IsAssignableFrom(ifTrue.Type))
				{
					ifTrue = System.Linq.Expressions.Expression.Convert(ifTrue, ifFalse.Type);
				}
				else
				{
					ifTrue = Microsoft.Scripting.Ast.Utils.Box(ifTrue);
					ifFalse = Microsoft.Scripting.Ast.Utils.Box(ifFalse);
				}
			}
			return System.Linq.Expressions.Expression.Condition(test, ifTrue, ifFalse);
		}

		internal static System.Linq.Expressions.Expression CallDelegate(Delegate method, System.Linq.Expressions.Expression[] arguments)
		{
			if (method.Method.DeclaringType == null || !method.Method.DeclaringType.IsPublic || !method.Method.IsPublic)
			{
				return System.Linq.Expressions.Expression.Call(Microsoft.Scripting.Ast.Utils.Constant(method), method.GetType().GetMethod("Invoke"), arguments);
			}
			if (method.Target != null)
			{
				if (method.Method.IsStatic)
				{
					return System.Linq.Expressions.Expression.Call(null, method.Method, ArrayUtils.Insert(Microsoft.Scripting.Ast.Utils.Constant(method.Target), arguments));
				}
				return System.Linq.Expressions.Expression.Call(Microsoft.Scripting.Ast.Utils.Constant(method.Target), method.Method, arguments);
			}
			if (method.Method.IsStatic)
			{
				return System.Linq.Expressions.Expression.Call(null, method.Method, arguments);
			}
			return System.Linq.Expressions.Expression.Call(arguments[0], method.Method, ArrayUtils.RemoveFirst(arguments));
		}

		internal static System.Linq.Expressions.Expression YieldExpression(RubyContext context, ICollection<System.Linq.Expressions.Expression> arguments, System.Linq.Expressions.Expression splattedArgument, System.Linq.Expressions.Expression rhsArgument, System.Linq.Expressions.Expression blockArgument, System.Linq.Expressions.Expression bfcVariable, System.Linq.Expressions.Expression selfArgument)
		{
			bool hasArgumentArray;
			MethodInfo method = Methods.Yield(arguments.Count, splattedArgument != null, rhsArgument != null, out hasArgumentArray);
			ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
			foreach (System.Linq.Expressions.Expression argument in arguments)
			{
				readOnlyCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.Box(argument));
			}
			if (hasArgumentArray)
			{
				ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder2 = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
				readOnlyCollectionBuilder2.Add(System.Linq.Expressions.Expression.NewArrayInit(typeof(object), readOnlyCollectionBuilder));
				readOnlyCollectionBuilder = readOnlyCollectionBuilder2;
			}
			if (splattedArgument != null)
			{
				readOnlyCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.LightDynamic(ProtocolConversionAction<ExplicitSplatAction>.Make(context), typeof(IList), splattedArgument));
			}
			if (rhsArgument != null)
			{
				readOnlyCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.Box(rhsArgument));
			}
			readOnlyCollectionBuilder.Add((blockArgument != null) ? Microsoft.Scripting.Ast.Utils.Convert(blockArgument, typeof(Proc)) : NullOfProc);
			readOnlyCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.Box(selfArgument));
			readOnlyCollectionBuilder.Add(bfcVariable);
			return System.Linq.Expressions.Expression.Call(method, readOnlyCollectionBuilder);
		}
	}
}
