using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class BlockExpression : Expression
	{
		internal static readonly BlockExpression Empty = new BlockExpression();

		private readonly Statements _statements;

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.BlockExpression;
			}
		}

		private BlockExpression()
			: base(SourceSpan.None)
		{
			_statements = Expression.EmptyStatements;
		}

		internal BlockExpression(Statements statements, SourceSpan location)
			: base(location)
		{
			_statements = statements;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return gen.TransformStatementsToExpression(_statements);
		}

		internal override System.Linq.Expressions.Expression TransformReadBoolean(AstGenerator gen, bool positive)
		{
			return gen.TransformStatementsToBooleanExpression(_statements, positive);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return TransformRead(gen);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
