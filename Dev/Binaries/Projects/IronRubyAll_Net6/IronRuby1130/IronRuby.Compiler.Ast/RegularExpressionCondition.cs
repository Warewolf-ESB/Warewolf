using System.Linq.Expressions;

namespace IronRuby.Compiler.Ast
{
	public class RegularExpressionCondition : Expression
	{
		private readonly RegularExpression _regex;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RegularExpressionCondition;
			}
		}

		public RegularExpression RegularExpression
		{
			get
			{
				return _regex;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public RegularExpressionCondition(RegularExpression regex)
			: base(regex.Location)
		{
			_regex = regex;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Methods.MatchLastInputLine.OpCall(_regex.TransformRead(gen), gen.CurrentScopeVariable);
		}
	}
}
