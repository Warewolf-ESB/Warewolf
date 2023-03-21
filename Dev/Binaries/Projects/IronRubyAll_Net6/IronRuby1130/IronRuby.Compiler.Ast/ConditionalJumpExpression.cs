using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ConditionalJumpExpression : Expression
	{
		private readonly bool _negateCondition;

		private readonly Expression _condition;

		private readonly Expression _value;

		private readonly JumpStatement _jumpStatement;

		public bool NegateCondition
		{
			get
			{
				return _negateCondition;
			}
		}

		public bool IsBooleanExpression
		{
			get
			{
				return _value == null;
			}
		}

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Expression Value
		{
			get
			{
				return _value;
			}
		}

		public JumpStatement JumpStatement
		{
			get
			{
				return _jumpStatement;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ConditionalJumpExpression;
			}
		}

		public ConditionalJumpExpression(Expression condition, JumpStatement jumpStatement, bool negateCondition, Expression value, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(condition, "condition");
			ContractUtils.RequiresNotNull(jumpStatement, "jumpStatement");
			_condition = condition;
			_jumpStatement = jumpStatement;
			_negateCondition = negateCondition;
			_value = value;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			if (_value != null)
			{
				return System.Linq.Expressions.Expression.Block(Utils.IfThen(_condition.TransformReadBoolean(gen, !_negateCondition), _jumpStatement.Transform(gen)), _value.TransformRead(gen));
			}
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#tmp_cond", typeof(object));
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression, Utils.Box(_condition.TransformRead(gen))), Utils.IfThen((_negateCondition ? Methods.IsFalse : Methods.IsTrue).OpCall(expression), _jumpStatement.Transform(gen)), expression);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
