using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class ConditionalExpression : Expression
	{
		private readonly Expression _condition;

		private readonly Expression _trueExpression;

		private readonly Expression _falseExpression;

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Expression TrueExpression
		{
			get
			{
				return _trueExpression;
			}
		}

		public Expression FalseExpression
		{
			get
			{
				return _falseExpression;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ConditionalExpression;
			}
		}

		public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression, SourceSpan location)
			: base(location)
		{
			_condition = condition;
			_trueExpression = trueExpression;
			_falseExpression = falseExpression;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return AstFactory.Condition(_condition.TransformReadBoolean(gen, true), _trueExpression.TransformRead(gen), _falseExpression.TransformRead(gen));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
