using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ConditionalStatement : Expression
	{
		private readonly Expression _condition;

		private readonly Expression _body;

		private readonly Expression _elseStatement;

		private readonly bool _negateCondition;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ConditionalStatement;
			}
		}

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Expression Body
		{
			get
			{
				return _body;
			}
		}

		public Expression ElseStatement
		{
			get
			{
				return _elseStatement;
			}
		}

		public bool IsUnless
		{
			get
			{
				return _negateCondition;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ConditionalStatement(Expression condition, bool negateCondition, Expression body, Expression elseStatement, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(condition, "condition");
			ContractUtils.RequiresNotNull(body, "body");
			_condition = condition;
			_body = body;
			_negateCondition = negateCondition;
			_elseStatement = elseStatement;
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return Utils.IfThenElse(_condition.TransformCondition(gen, !_negateCondition), _body.Transform(gen), (_elseStatement != null) ? _elseStatement.Transform(gen) : Utils.Empty());
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.Condition(_condition.TransformReadBoolean(gen, !_negateCondition), Utils.Box(_body.TransformRead(gen)), (_elseStatement != null) ? Utils.Box(_elseStatement.TransformRead(gen)) : Utils.Constant(null));
		}
	}
}
