using System;
using System.Linq.Expressions;

namespace IronRuby.Compiler.Ast
{
	internal abstract class ReducibleEmptyExpression : System.Linq.Expressions.Expression
	{
		public sealed override ExpressionType NodeType
		{
			get
			{
				return ExpressionType.Extension;
			}
		}

		public override Type Type
		{
			get
			{
				return typeof(void);
			}
		}

		public override bool CanReduce
		{
			get
			{
				return true;
			}
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return System.Linq.Expressions.Expression.Empty();
		}

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			return this;
		}
	}
}
