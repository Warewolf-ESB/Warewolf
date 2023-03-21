using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ErrorExpression : Expression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ErrorExpression;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ErrorExpression(SourceSpan location)
			: base(location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			throw Assert.Unreachable;
		}
	}
}
