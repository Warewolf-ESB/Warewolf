using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class OrExpression : Expression
	{
		private readonly Expression _left;

		private readonly Expression _right;

		public Expression Left
		{
			get
			{
				return _left;
			}
		}

		public Expression Right
		{
			get
			{
				return _right;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.OrExpression;
			}
		}

		public OrExpression(Expression left, Expression right, SourceSpan location)
			: base(location)
		{
			_left = left;
			_right = right;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return TransformRead(gen, _left.TransformRead(gen), _right.TransformRead(gen));
		}

		internal override System.Linq.Expressions.Expression TransformReadBoolean(AstGenerator gen, bool positive)
		{
			return AstFactory.Logical(_left.TransformReadBoolean(gen, positive), _right.TransformReadBoolean(gen, positive), !positive);
		}

		internal static System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
		{
			ParameterExpression temp;
			System.Linq.Expressions.Expression result = Utils.CoalesceFalse(Utils.Box(left), Utils.Box(right), Methods.IsTrue, out temp);
			gen.CurrentScope.AddHidden(temp);
			return result;
		}

		internal override Expression ToCondition(LexicalScope currentScope)
		{
			Expression expression = _left.ToCondition(currentScope);
			Expression expression2 = _right.ToCondition(currentScope);
			if (expression != _left || expression2 != _right)
			{
				return new OrExpression(expression, expression2, base.Location);
			}
			return this;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
