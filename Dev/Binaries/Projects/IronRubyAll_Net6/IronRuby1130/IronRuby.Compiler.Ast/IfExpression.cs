using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class IfExpression : Expression
	{
		private Expression _condition;

		private Statements _body;

		private List<ElseIfClause> _elseIfClauses;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.IfExpression;
			}
		}

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Statements Body
		{
			get
			{
				return _body;
			}
		}

		public List<ElseIfClause> ElseIfClauses
		{
			get
			{
				return _elseIfClauses;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public IfExpression(Expression condition, Statements body, List<ElseIfClause> elseIfClauses, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(body, "body");
			ContractUtils.RequiresNotNull(condition, "condition");
			ContractUtils.RequiresNotNull(elseIfClauses, "elseIfClauses");
			for (int i = 0; i < elseIfClauses.Count - 1; i++)
			{
				if (elseIfClauses[i].Condition == null)
				{
					throw ExceptionUtils.MakeArgumentItemNullException(i, "elseIfClauses");
				}
			}
			_condition = condition;
			_body = body;
			_elseIfClauses = elseIfClauses;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			int num = _elseIfClauses.Count - 1;
			System.Linq.Expressions.Expression ifFalse;
			if (num >= 0 && _elseIfClauses[num].Condition == null)
			{
				ifFalse = gen.TransformStatementsToExpression(_elseIfClauses[num].Statements);
				num--;
			}
			else
			{
				ifFalse = Utils.Constant(null);
			}
			while (num >= 0)
			{
				ifFalse = AstFactory.Condition(_elseIfClauses[num].Condition.TransformCondition(gen, true), gen.TransformStatementsToExpression(_elseIfClauses[num].Statements), ifFalse);
				num--;
			}
			return AstFactory.Condition(_condition.TransformCondition(gen, true), gen.TransformStatementsToExpression(_body), ifFalse);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return TransformRead(gen);
		}
	}
}
