using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class FileLiteral : Expression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.FileLiteral;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		internal FileLiteral(SourceSpan location)
			: base(location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Methods.CreateMutableStringL.OpCall(gen.SourcePathConstant, Utils.Constant(gen.Context.GetPathEncoding()));
		}
	}
}
