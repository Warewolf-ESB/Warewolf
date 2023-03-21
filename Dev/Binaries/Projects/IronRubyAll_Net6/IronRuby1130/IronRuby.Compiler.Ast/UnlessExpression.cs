using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class UnlessExpression : Expression
	{
		private readonly Expression _condition;

		private readonly Statements _statements;

		private readonly ElseIfClause _elseClause;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.UnlessExpression;
			}
		}

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public ElseIfClause ElseClause
		{
			get
			{
				return _elseClause;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public UnlessExpression(Expression condition, Statements statements, ElseIfClause elseClause, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(condition, "condition");
			ContractUtils.RequiresNotNull(statements, "statements");
			ContractUtils.Requires(elseClause == null || elseClause.Condition == null, "elseClause", "No condition allowed.");
			_statements = statements;
			_condition = condition;
			_elseClause = elseClause;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return AstFactory.Condition(_condition.TransformCondition(gen, false), gen.TransformStatementsToExpression(_statements), gen.TransformStatementsToExpression((_elseClause != null) ? _elseClause.Statements : null));
		}
	}
}
