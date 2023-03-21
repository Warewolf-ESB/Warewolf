using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class ElseIfClause : Node
	{
		private readonly Statements _statements;

		private readonly Expression _condition;

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ElseIfClause;
			}
		}

		public ElseIfClause(Expression condition, Statements statements, SourceSpan location)
			: base(location)
		{
			_statements = statements;
			_condition = condition;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
