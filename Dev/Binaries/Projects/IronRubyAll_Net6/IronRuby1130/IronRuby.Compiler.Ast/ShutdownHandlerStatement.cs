using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class ShutdownHandlerStatement : Expression
	{
		private readonly BlockDefinition _block;

		public BlockDefinition Block
		{
			get
			{
				return _block;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Finalizer;
			}
		}

		public ShutdownHandlerStatement(LexicalScope definedScope, Statements body, SourceSpan location)
			: base(location)
		{
			_block = new BlockDefinition(definedScope, null, body, location);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return Methods.RegisterShutdownHandler.OpCall(_block.Transform(gen));
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.Block(Transform(gen), Utils.Constant(null));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
