namespace IronRuby.Compiler
{
	internal sealed class BlockLexicalScope : LexicalScope
	{
		internal override bool IsTop
		{
			get
			{
				return false;
			}
		}

		internal override bool IsStaticTop
		{
			get
			{
				return false;
			}
		}

		public BlockLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
