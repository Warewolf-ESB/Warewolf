using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class NotExpression : Expression
	{
		private readonly Expression _expression;

		public Expression Expression
		{
			get
			{
				return _expression;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.NotExpression;
			}
		}

		public NotExpression(Expression expression, SourceSpan location)
			: base(location)
		{
			_expression = expression;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression transformedTarget = ((_expression != null) ? _expression.Transform(gen) : System.Linq.Expressions.Expression.Constant(null));
			return MethodCall.TransformRead(this, gen, false, Symbols.Bang, transformedTarget, null, null, null, null);
		}

		internal override System.Linq.Expressions.Expression TransformReadBoolean(AstGenerator gen, bool positive)
		{
			return (positive ? Methods.IsTrue : Methods.IsFalse).OpCall(Utils.Box(TransformRead(gen)));
		}

		internal override Expression ToCondition(LexicalScope currentScope)
		{
			if (_expression != null)
			{
				Expression expression = _expression.ToCondition(currentScope);
				if (expression != _expression)
				{
					return new NotExpression(expression, base.Location);
				}
			}
			return this;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
