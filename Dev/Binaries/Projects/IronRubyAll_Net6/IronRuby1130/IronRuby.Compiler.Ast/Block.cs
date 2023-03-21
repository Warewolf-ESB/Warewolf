using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public abstract class Block : Node
	{
		public abstract bool IsDefinition { get; }

		protected Block(SourceSpan location)
			: base(location)
		{
		}

		internal abstract System.Linq.Expressions.Expression Transform(AstGenerator gen);
	}
}
