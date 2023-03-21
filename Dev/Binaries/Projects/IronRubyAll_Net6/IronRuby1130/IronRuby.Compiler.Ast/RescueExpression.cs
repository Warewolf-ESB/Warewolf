using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class RescueExpression : Expression
	{
		private readonly SourceSpan _rescueSpan;

		private readonly Expression _guardedExpression;

		private readonly Expression _rescueClauseStatement;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RescueExpression;
			}
		}

		public Expression GuardedExpression
		{
			get
			{
				return _guardedExpression;
			}
		}

		public Expression RescueClauseStatement
		{
			get
			{
				return _rescueClauseStatement;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public RescueExpression(Expression guardedExpression, Expression rescueClauseStatement, SourceSpan rescueSpan, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(guardedExpression, "guardedExpression");
			ContractUtils.RequiresNotNull(rescueClauseStatement, "rescueClauseStatement");
			_guardedExpression = guardedExpression;
			_rescueClauseStatement = rescueClauseStatement;
			_rescueSpan = rescueSpan;
		}

		private Body ToBody(AstGenerator gen)
		{
			return new Body(new Statements(_guardedExpression), CollectionUtils.MakeList(new RescueClause(Expression.EmptyArray, null, new Statements(_rescueClauseStatement), _rescueSpan)), null, null, base.Location);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return ToBody(gen).TransformResult(gen, ResultOperation.Ignore);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return ToBody(gen).TransformRead(gen);
		}
	}
}
