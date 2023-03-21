using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class IsDefinedExpression : Expression
	{
		private readonly Expression _expression;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.AstNodeDescriptionExpression;
			}
		}

		public Expression Expression
		{
			get
			{
				return _expression;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public IsDefinedExpression(Expression expression, SourceSpan location)
			: base(location)
		{
			_expression = expression;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return _expression.TransformIsDefined(gen);
		}

		internal override System.Linq.Expressions.Expression TransformReadBoolean(AstGenerator gen, bool positive)
		{
			return _expression.TransformBooleanIsDefined(gen, positive);
		}
	}
}
