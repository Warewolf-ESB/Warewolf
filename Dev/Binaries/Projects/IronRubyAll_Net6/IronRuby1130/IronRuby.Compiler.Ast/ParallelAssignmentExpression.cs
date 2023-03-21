using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class ParallelAssignmentExpression : AssignmentExpression
	{
		private readonly CompoundLeftValue _lhs;

		private readonly Expression[] _rhs;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ParallelAssignmentExpression;
			}
		}

		public CompoundLeftValue Left
		{
			get
			{
				return _lhs;
			}
		}

		public Expression[] Right
		{
			get
			{
				return _rhs;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ParallelAssignmentExpression(CompoundLeftValue lhs, Expression[] rhs, SourceSpan location)
			: base(null, location)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return _lhs.TransformWrite(gen, new Arguments(_rhs));
		}
	}
}
