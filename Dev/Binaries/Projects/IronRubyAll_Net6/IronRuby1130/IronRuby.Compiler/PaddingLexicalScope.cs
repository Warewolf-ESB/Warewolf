namespace IronRuby.Compiler
{
	internal sealed class PaddingLexicalScope : LexicalScope
	{
		protected override bool AllowsVariableDefinitions
		{
			get
			{
				return false;
			}
		}

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

		public PaddingLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
