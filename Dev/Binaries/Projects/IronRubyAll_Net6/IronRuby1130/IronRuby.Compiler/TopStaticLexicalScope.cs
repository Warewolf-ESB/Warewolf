namespace IronRuby.Compiler
{
	internal sealed class TopStaticLexicalScope : LexicalScope
	{
		internal override bool IsTop
		{
			get
			{
				return base.OuterScope == null;
			}
		}

		public TopStaticLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
