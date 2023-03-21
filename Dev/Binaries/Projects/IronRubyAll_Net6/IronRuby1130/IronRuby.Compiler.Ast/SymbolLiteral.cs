using System.Linq.Expressions;
using IronRuby.Builtins;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class SymbolLiteral : StringLiteral
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SymbolLiteral;
			}
		}

		public SymbolLiteral(string value, RubyEncoding encoding, SourceSpan location)
			: base(value, encoding, location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.Constant(gen.Context.CreateSymbol((string)base.Value, base.Encoding));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
