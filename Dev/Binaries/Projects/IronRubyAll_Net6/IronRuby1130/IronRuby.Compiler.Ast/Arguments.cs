using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class Arguments
	{
		internal static readonly Arguments Empty = new Arguments();

		private readonly Expression[] _expressions;

		public bool IsEmpty
		{
			get
			{
				return _expressions.Length == 0;
			}
		}

		public Expression[] Expressions
		{
			get
			{
				return _expressions;
			}
		}

		public Arguments()
		{
			_expressions = Expression.EmptyArray;
		}

		public Arguments(Expression arg)
		{
			ContractUtils.RequiresNotNull(arg, "arg");
			_expressions = new Expression[1] { arg };
		}

		public Arguments(Expression[] expressions)
		{
			_expressions = expressions ?? Expression.EmptyArray;
		}

		internal void TransformToCall(AstGenerator gen, CallSiteBuilder siteBuilder)
		{
			siteBuilder.SplattedArgument = TransformToCallInternal(gen, siteBuilder);
		}

		internal System.Linq.Expressions.Expression TransformToYield(AstGenerator gen, System.Linq.Expressions.Expression bfcVariable, System.Linq.Expressions.Expression selfExpression)
		{
			List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>();
			System.Linq.Expressions.Expression splattedArgument = TransformToCallInternal(gen, list);
			return AstFactory.YieldExpression(gen.Context, list, splattedArgument, null, null, bfcVariable, selfExpression);
		}

		private System.Linq.Expressions.Expression TransformToCallInternal(AstGenerator gen, ICollection<System.Linq.Expressions.Expression> result)
		{
			int splattedCount;
			int num = IndexOfSplatted(out splattedCount);
			for (int i = 0; i < ((num != -1) ? num : _expressions.Length); i++)
			{
				result.Add(_expressions[i].TransformRead(gen));
			}
			if (splattedCount == 1)
			{
				return _expressions[num].TransformRead(gen);
			}
			if (splattedCount > 1)
			{
				return UnsplatArguments(gen, num);
			}
			return null;
		}

		internal int IndexOfSplatted(out int splattedCount)
		{
			splattedCount = 0;
			int num = -1;
			for (int i = 0; i < _expressions.Length; i++)
			{
				if (_expressions[i] is SplattedArgument)
				{
					splattedCount++;
					if (num == -1)
					{
						num = i;
					}
				}
			}
			return num;
		}

		internal System.Linq.Expressions.Expression UnsplatArguments(AstGenerator gen, int start)
		{
			if (start == _expressions.Length - 1)
			{
				return Methods.Unsplat.OpCall(Utils.Box(_expressions[start].TransformRead(gen)));
			}
			System.Linq.Expressions.Expression expression = Methods.MakeArray0.OpCall();
			for (int i = start; i < _expressions.Length; i++)
			{
				expression = ((!(_expressions[i] is SplattedArgument)) ? Methods.AddItem.OpCall(expression, Utils.Box(_expressions[i].TransformRead(gen))) : Methods.AddRange.OpCall(expression, _expressions[i].TransformRead(gen)));
			}
			return expression;
		}

		internal System.Linq.Expressions.Expression TransformToArray(AstGenerator gen)
		{
			int splattedCount;
			int num = IndexOfSplatted(out splattedCount);
			if (num >= 0)
			{
				return UnsplatArguments(gen, 0);
			}
			return Methods.MakeArrayOpCall(gen.TransformExpressions(_expressions));
		}

		internal System.Linq.Expressions.Expression TransformToReturnValue(AstGenerator gen)
		{
			if (_expressions.Length == 1)
			{
				return _expressions[0].TransformRead(gen);
			}
			return TransformToArray(gen);
		}
	}
}
