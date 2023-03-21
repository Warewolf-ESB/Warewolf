using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class EncodingExpression : Expression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.EncodingExpression;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public EncodingExpression(SourceSpan location)
			: base(location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Utils.Constant(gen.Encoding);
		}
	}
}
