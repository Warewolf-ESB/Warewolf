using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class WhenClause : Node
	{
		public static readonly WhenClause[] EmptyArray = new WhenClause[0];

		private readonly Expression[] _comparisons;

		private readonly Statements _statements;

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public Expression[] Comparisons
		{
			get
			{
				return _comparisons;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.WhenClause;
			}
		}

		public WhenClause(Expression[] comparisons, Statements statements, SourceSpan location)
			: base(location)
		{
			_comparisons = comparisons ?? Expression.EmptyArray;
			_statements = statements;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
