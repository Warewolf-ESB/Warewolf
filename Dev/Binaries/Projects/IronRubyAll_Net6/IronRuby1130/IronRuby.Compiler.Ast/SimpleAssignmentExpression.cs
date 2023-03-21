using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class SimpleAssignmentExpression : AssignmentExpression
	{
		public new static SimpleAssignmentExpression[] EmptyArray = new SimpleAssignmentExpression[0];

		private readonly LeftValue _left;

		private readonly Expression _right;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SimpleAssignmentExpression;
			}
		}

		public LeftValue Left
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

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public SimpleAssignmentExpression(LeftValue left, Expression right, string operation, SourceSpan location)
			: base(operation, location)
		{
			_left = left;
			_right = right;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = _left.TransformTargetRead(gen);
			System.Linq.Expressions.Expression expression2 = ((expression == null) ? null : gen.CurrentScope.DefineHiddenVariable(string.Empty, expression.Type));
			System.Linq.Expressions.Expression expression3 = _right.TransformRead(gen);
			if (base.Operation == Symbols.And || base.Operation == Symbols.Or)
			{
				System.Linq.Expressions.Expression left = _left.TransformRead(gen, (expression != null) ? System.Linq.Expressions.Expression.Assign(expression2, expression) : null, true);
				System.Linq.Expressions.Expression right = _left.TransformWrite(gen, expression2, expression3);
				if (base.Operation == Symbols.And)
				{
					return AndExpression.TransformRead(gen, left, right);
				}
				return OrExpression.TransformRead(gen, left, right);
			}
			if (base.Operation != null)
			{
				System.Linq.Expressions.Expression transformedTarget = _left.TransformRead(gen, expression2, false);
				expression3 = MethodCall.TransformRead(this, gen, false, base.Operation, transformedTarget, null, null, expression3, null);
			}
			return _left.TransformWrite(gen, (expression != null) ? System.Linq.Expressions.Expression.Assign(expression2, expression) : null, expression3);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			if (base.Operation == Symbols.And || base.Operation == Symbols.Or)
			{
				return "expression";
			}
			return base.GetNodeName(gen);
		}
	}
}
