using System.Linq.Expressions;

namespace IronRuby.Compiler.Ast
{
	public class LambdaDefinition : Expression
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
				return NodeTypes.LambdaDefinition;
			}
		}

		public LambdaDefinition(BlockDefinition block)
			: base(block.Location)
		{
			_block = block;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return _block.Transform(gen, true);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
