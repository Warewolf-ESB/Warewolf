using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public abstract class CallExpression : Expression
	{
		private readonly Arguments _args;

		private Block _block;

		public Arguments Arguments
		{
			get
			{
				return _args;
			}
		}

		public Block Block
		{
			get
			{
				return _block;
			}
			internal set
			{
				_block = value;
			}
		}

		protected CallExpression(Arguments args, Block block, SourceSpan location)
			: base(location)
		{
			_args = args;
			_block = block;
		}
	}
}
