using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class Placeholder : Variable
	{
		public static readonly Placeholder Singleton = new Placeholder();

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Placeholder;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		private Placeholder()
			: base(string.Empty, SourceSpan.None)
		{
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			throw Assert.Unreachable;
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			return rightValue;
		}

		public override string ToString()
		{
			return " ";
		}
	}
}
